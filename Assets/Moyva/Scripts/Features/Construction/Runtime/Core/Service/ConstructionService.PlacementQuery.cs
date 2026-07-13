using Kruty1918.Moyva.Construction.API;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        private BuildingPlacementEvaluationRequest _placementQueryEvaluationRequest;
        private Vector2Int? _placementQueryIgnoredOccupiedPosition;
        private string _placementQueryIgnoredOccupiedBuildingId;
        private Vector2Int? _placementQuerySecondaryIgnoredOccupiedPosition;
        private string _placementQuerySecondaryIgnoredOccupiedBuildingId;
        private readonly Dictionary<ResourceValidationCacheKey, ResourceValidationCacheValue> _resourceValidationCache = new();
        private int _resourceValidationCacheFrame = -1;

        public ConstructionPlacementQueryResult EvaluatePlacement(ConstructionPlacementQueryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.BuildingId))
                return InvalidPlacementQueryResult("Building id is empty.", request, BuildingPlacementBlockerKind.Configuration);

            if (_gridService != null && !_gridService.TryGetTileData(request.Position, out _))
                return InvalidPlacementQueryResult("Tile does not exist.", request, BuildingPlacementBlockerKind.Terrain);

            bool isGate = _wallTopologyService != null && _wallTopologyService.IsGate(request.BuildingId);
            bool gateReplacement = TryResolveGateReplacement(
                request.Position,
                request.BuildingId,
                out Vector2Int replacedWallOriginValue,
                out string replacedWallId);
            if (isGate && !gateReplacement)
                return InvalidPlacementQueryResult("Gate must replace a valid wall.", request, BuildingPlacementBlockerKind.Configuration);

            Vector2Int? ignoredOccupiedPosition = ResolveIgnoredOccupiedPosition(request);
            Vector2Int? replacedWallOrigin = gateReplacement
                ? replacedWallOriginValue
                : null;
            string placementOwnerId = string.IsNullOrWhiteSpace(request.OwnerId)
                ? _activeOwnerId
                : request.OwnerId.Trim();
            if (gateReplacement
                && replacedWallOrigin.HasValue
                && _factionPlacedBuildings.TryGetValue(replacedWallOrigin.Value, out var replacedFactionEntry)
                && !string.Equals(replacedFactionEntry.FactionId, placementOwnerId, StringComparison.Ordinal))
            {
                return InvalidPlacementQueryResult(
                    "Gate cannot replace a wall owned by another faction.",
                    request,
                    BuildingPlacementBlockerKind.Configuration);
            }
            if (gateReplacement
                && replacedWallOrigin.HasValue
                && _playerPlacedBuildings.ContainsKey(replacedWallOrigin.Value)
                && !string.Equals(_activeOwnerId, placementOwnerId, StringComparison.Ordinal))
            {
                return InvalidPlacementQueryResult(
                    "Gate cannot replace a player wall owned by another faction.",
                    request,
                    BuildingPlacementBlockerKind.Configuration);
            }

            BuildingPlacementEvaluationResult evaluationResult = null;
            bool spatiallyValid = EvaluateSpatialRules(
                request,
                ignoredOccupiedPosition,
                request.BuildingId,
                replacedWallOrigin,
                replacedWallId,
                out evaluationResult);
            if (!spatiallyValid)
            {
                string reason = ResolveEvaluationReason(evaluationResult) ?? "Placement rules blocked this tile.";
                return new ConstructionPlacementQueryResult(false, false, gateReplacement, reason, evaluationResult);
            }

            bool resourcesValid = true;
            string resourceReason = null;
            if (request.IncludeResources && !IsRelocationQuery(request))
            {
                resourcesValid = TryValidateConstructionResourcesCached(request, placementOwnerId, out resourceReason);
            }

            return new ConstructionPlacementQueryResult(
                isSpatiallyValid: true,
                resourcesValid,
                gateReplacement,
                resourceReason,
                evaluationResult);
        }

        private bool EvaluateSpatialRules(
            ConstructionPlacementQueryRequest request,
            Vector2Int? ignoredOccupiedPosition,
            string ignoredOccupiedBuildingId,
            Vector2Int? secondaryIgnoredOccupiedPosition,
            string secondaryIgnoredOccupiedBuildingId,
            out BuildingPlacementEvaluationResult evaluationResult)
        {
            EnsurePlacementQueryRequest();
            _placementQueryIgnoredOccupiedPosition = ignoredOccupiedPosition;
            _placementQueryIgnoredOccupiedBuildingId = ignoredOccupiedBuildingId;
            _placementQuerySecondaryIgnoredOccupiedPosition = secondaryIgnoredOccupiedPosition;
            _placementQuerySecondaryIgnoredOccupiedBuildingId = secondaryIgnoredOccupiedBuildingId;
            _placementQueryEvaluationRequest.BuildingId = request.BuildingId;
            _placementQueryEvaluationRequest.Position = request.Position;
            _placementQueryEvaluationRequest.IgnoredPendingPosition = request.IgnoredPendingPosition;
            _placementQueryEvaluationRequest.IgnoredOccupiedPosition = ignoredOccupiedPosition;
            _placementQueryEvaluationRequest.PendingPlacements = BuildPlacementSimulationEntries();
            evaluationResult = request.IncludeDetails
                ? BuildingPlacementEvaluator.Evaluate(_placementQueryEvaluationRequest)
                : null;
            return evaluationResult?.IsValid
                ?? BuildingPlacementEvaluator.CanPlace(_placementQueryEvaluationRequest);
        }

        private void EnsurePlacementQueryRequest()
        {
            if (_placementQueryEvaluationRequest != null)
                return;

            _placementQueryEvaluationRequest = new BuildingPlacementEvaluationRequest
            {
                BuildingRegistry = _placementBuildingRegistry,
                MinSpacing = _minSpacing,
                TownHallBuildRadius = _townHallBuildRadius,
                IsOccupied = IsOccupiedForPlacementQuery,
                GetOccupantId = GetOccupantForPlacementQuery,
                GetOccupantOrigin = GetOccupantOriginForPlacementQuery,
                IsFogBlocked = IsBlockedByFog,
                IsTerrainBlocked = IsTerrainBlockedForPlacementQuery,
                GetTerrainLevel = GetTerrainLevelForPlacementQuery,
                GetTileId = GetTileId,
                PendingPlacements = _placementSimulationSnapshot,
                SkipInfluenceRules = _placementRulesProvider != null
                    && !_placementRulesProvider.EnableInfluenceZoneRules,
            };
        }

        private bool IsOccupiedForPlacementQuery(Vector2Int position)
        {
            return !IsIgnoredOccupiedFootprintCell(position)
                && _objectsMapService != null
                && _objectsMapService.IsOccupied(position);
        }

        private string GetOccupantForPlacementQuery(Vector2Int position)
            => IsIgnoredOccupiedFootprintCell(position)
                ? null
                : GetObjectOccupantId(position);

        private Vector2Int? GetOccupantOriginForPlacementQuery(Vector2Int position)
            => _placedOriginByOccupiedTile.TryGetValue(position, out Vector2Int origin)
                ? origin
                : null;

        private bool IsTerrainBlockedForPlacementQuery(Vector2Int position)
        {
            if (_gridService != null && !_gridService.TryGetTileData(position, out _))
                return true;

            return IsBlockedByTerrain(position, out _);
        }

        private int? GetTerrainLevelForPlacementQuery(Vector2Int position)
        {
            return _generatedTerrainLevelQuery != null
                && _generatedTerrainLevelQuery.TryGetTerrainLevel(position, out int level)
                ? level
                : null;
        }

        private bool IsIgnoredOccupiedFootprintCell(Vector2Int position)
        {
            return IsFootprintCell(
                       position,
                       _placementQueryIgnoredOccupiedPosition,
                       _placementQueryIgnoredOccupiedBuildingId)
                || IsFootprintCell(
                       position,
                       _placementQuerySecondaryIgnoredOccupiedPosition,
                       _placementQuerySecondaryIgnoredOccupiedBuildingId);
        }

        private bool IsFootprintCell(
            Vector2Int position,
            Vector2Int? footprintOrigin,
            string buildingId)
        {
            if (!footprintOrigin.HasValue)
                return false;

            BuildingDefinition definition = _placementBuildingRegistry.GetById(buildingId);
            return BuildingFootprintUtility.Contains(definition, footprintOrigin.Value, position);
        }

        private Vector2Int? ResolveIgnoredOccupiedPosition(ConstructionPlacementQueryRequest request)
        {
            if (request.IgnoredOccupiedPosition.HasValue)
                return request.IgnoredOccupiedPosition;

            if (!request.IgnoredPendingPosition.HasValue)
                return null;

            int pendingIndex = FindPendingPlacementIndexForQuery(request.IgnoredPendingPosition.Value);
            return pendingIndex >= 0 ? _pendingPlacements[pendingIndex].OriginalPosition : null;
        }

        private bool IsRelocationQuery(ConstructionPlacementQueryRequest request)
        {
            if (!request.IgnoredPendingPosition.HasValue)
                return false;

            int pendingIndex = FindPendingPlacementIndexForQuery(request.IgnoredPendingPosition.Value);
            return pendingIndex >= 0 && IsRelocation(_pendingPlacements[pendingIndex]);
        }

        private int FindPendingPlacementIndexForQuery(Vector2Int position)
        {
            for (int i = 0; i < _pendingPlacements.Count; i++)
            {
                if (_pendingPlacements[i].Position == position)
                    return i;
            }

            return -1;
        }

        private void MarkPendingPlacementsChanged()
        {
            _pendingPlacementsVersion++;
        }

        private void InvalidatePlacementResourceValidationCache()
        {
            _resourceValidationCacheFrame = -1;
            _resourceValidationCache.Clear();
        }

        private bool TryValidateConstructionResourcesCached(
            ConstructionPlacementQueryRequest request,
            string ownerId,
            out string reason)
        {
            if (request.IgnoredPendingPosition.HasValue || !Application.isPlaying)
            {
                return TryValidateConstructionResources(
                    request.Position,
                    request.BuildingId,
                    ownerId,
                    request.IgnoredPendingPosition,
                    out reason);
            }

            int frame = Time.frameCount;
            if (_resourceValidationCacheFrame != frame)
            {
                _resourceValidationCacheFrame = frame;
                _resourceValidationCache.Clear();
            }

            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            string fundingContext = ResolveResourceFundingContext(request.Position, normalizedOwnerId);
            var key = new ResourceValidationCacheKey(
                request.BuildingId,
                normalizedOwnerId,
                fundingContext,
                _pendingPlacementsVersion);
            if (_resourceValidationCache.TryGetValue(key, out ResourceValidationCacheValue cached))
            {
                reason = cached.Reason;
                return cached.IsValid;
            }

            bool isValid = TryValidateConstructionResources(
                request.Position,
                request.BuildingId,
                normalizedOwnerId,
                ignoredPendingPosition: null,
                out reason);
            _resourceValidationCache[key] = new ResourceValidationCacheValue(isValid, reason);
            return isValid;
        }

        private string ResolveResourceFundingContext(Vector2Int position, string ownerId)
        {
            if (ShouldUseOwnerPoolConstructionFunding(ownerId))
                return "owner-pool";

            return _economyInfoMediator != null
                   && _economyInfoMediator.TryResolveConstructionSettlement(position, ownerId, out var settlement)
                   && !string.IsNullOrWhiteSpace(settlement.SettlementId)
                ? settlement.SettlementId
                : "no-settlement";
        }

        private static string ResolveEvaluationReason(BuildingPlacementEvaluationResult evaluationResult)
        {
            return evaluationResult != null
                   && evaluationResult.Blockers.Count > 0
                ? evaluationResult.Blockers[0].Message
                : null;
        }

        private static ConstructionPlacementQueryResult InvalidPlacementQueryResult(
            string reason,
            ConstructionPlacementQueryRequest request,
            BuildingPlacementBlockerKind blockerKind)
        {
            if (!request.IncludeDetails)
                return new ConstructionPlacementQueryResult(false, false, false, reason);

            var evaluationResult = new BuildingPlacementEvaluationResult
            {
                ConfigurationBlocked = blockerKind == BuildingPlacementBlockerKind.Configuration,
                TerrainBlocked = blockerKind == BuildingPlacementBlockerKind.Terrain,
            };
            evaluationResult.AddBlocker(new BuildingPlacementBlocker
            {
                Kind = blockerKind,
                Message = reason,
                Position = request.Position,
                BuildingId = request.BuildingId,
            });
            return new ConstructionPlacementQueryResult(false, false, false, reason, evaluationResult);
        }

        private readonly struct ResourceValidationCacheKey : IEquatable<ResourceValidationCacheKey>
        {
            public ResourceValidationCacheKey(string buildingId, string ownerId, string fundingContext, int pendingVersion)
            {
                BuildingId = buildingId;
                OwnerId = ownerId;
                FundingContext = fundingContext;
                PendingVersion = pendingVersion;
            }

            private string BuildingId { get; }
            private string OwnerId { get; }
            private string FundingContext { get; }
            private int PendingVersion { get; }

            public bool Equals(ResourceValidationCacheKey other)
                => PendingVersion == other.PendingVersion
                    && string.Equals(BuildingId, other.BuildingId, StringComparison.Ordinal)
                    && string.Equals(OwnerId, other.OwnerId, StringComparison.Ordinal)
                    && string.Equals(FundingContext, other.FundingContext, StringComparison.Ordinal);

            public override bool Equals(object obj)
                => obj is ResourceValidationCacheKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = PendingVersion;
                    hash = hash * 397 ^ (BuildingId != null ? StringComparer.Ordinal.GetHashCode(BuildingId) : 0);
                    hash = hash * 397 ^ (OwnerId != null ? StringComparer.Ordinal.GetHashCode(OwnerId) : 0);
                    hash = hash * 397 ^ (FundingContext != null ? StringComparer.Ordinal.GetHashCode(FundingContext) : 0);
                    return hash;
                }
            }
        }

        private readonly struct ResourceValidationCacheValue
        {
            public ResourceValidationCacheValue(bool isValid, string reason)
            {
                IsValid = isValid;
                Reason = reason;
            }

            public bool IsValid { get; }
            public string Reason { get; }
        }
    }
}
