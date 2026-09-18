using Kruty1918.Moyva.Construction.API;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
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
            _placementQueryEvaluationRequest.OwnerId = string.IsNullOrWhiteSpace(
                request.OwnerId)
                ? _activeOwnerId
                : request.OwnerId.Trim();
            _placementQueryEvaluationRequest.Position = request.Position;
            _placementQueryEvaluationRequest.Rotation = request.Rotation;
            _placementQueryEvaluationRequest.IgnoredPendingPosition = request.IgnoredPendingPosition;
            _placementQueryEvaluationRequest.IgnoredOccupiedPosition = ignoredOccupiedPosition;
            BuildPlacedBuildingSimulationEntries();
            _placementQueryEvaluationRequest.PendingPlacements = request.IncludePendingPlacements
                ? BuildPlacementSimulationEntries()
                : Array.Empty<BuildingPlacementSimulationEntry>();
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
                TileExists = TileExistsForPlacementQuery,
                GetOccupantId = GetOccupantForPlacementQuery,
                GetOccupantOrigin = GetOccupantOriginForPlacementQuery,
                GetOccupantOwnerId =
                    GetOccupantOwnerForPlacementQuery,
                IsFogBlocked = _placementEnvironmentRules.IsBlockedByFog,
                GetFogState = GetFogStateForPlacementQuery,
                IsTerrainBlocked = IsTerrainBlockedForPlacementQuery,
                GetTerrainLevel = GetTerrainLevelForPlacementQuery,
                GetTileId = _placementEnvironmentRules.GetTileId,
                HasTerrainTag = HasTerrainTagForPlacementQuery,
                PendingPlacements = _placementSimulationSnapshot,
                PlacedBuildings = _placedBuildingSimulationSnapshot,
                TileMatchWorkspace = _placementTileMatchWorkspace,
                HasInfluenceCenterDefinitions =
                    _influencePolicy.HasAnyInfluenceCenterDefinition(),
                MaxInfluenceRadius = _influencePolicy.ResolveMaxInfluenceRadius(),
                RuleEvaluators = _placementRuleEvaluators,
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

        private bool TileExistsForPlacementQuery(Vector2Int position)
            => _gridService == null
                || _gridService.TryGetTileData(position, out _);

        private Kruty1918.Moyva.FogOfWar.API.FogStateType? GetFogStateForPlacementQuery(
            Vector2Int position)
            => _fogOfWarService != null
                ? _fogOfWarService.GetFogState(position)
                : null;

        private bool HasTerrainTagForPlacementQuery(
            Vector2Int position,
            string tag)
        {
            string tileId = _placementEnvironmentRules.GetTileId(position);
            return _tileSettings is Kruty1918.Moyva.Grid.API.ITerrainTagQuery tagQuery
                && tagQuery.HasTerrainTag(tileId, tag);
        }

        private string GetOccupantForPlacementQuery(Vector2Int position)
            => IsIgnoredOccupiedFootprintCell(position)
                ? null
                : GetObjectOccupantId(position);

        private Vector2Int? GetOccupantOriginForPlacementQuery(
            Vector2Int position)
            => _footprints.TryGetOrigin(
                    position,
                    out Vector2Int origin)
                ? origin
                : null;

        private string GetOccupantOwnerForPlacementQuery(
            Vector2Int position)
        {
            Vector2Int origin =
                _footprints.ResolveOrigin(position);
            if (_factionPlacedBuildings.TryGetValue(
                    origin,
                    out var factionPlacement))
            {
                return NormalizeOwnerId(factionPlacement.FactionId);
            }

            return _playerPlacedBuildings.ContainsKey(origin)
                ? NormalizeOwnerId(_activeOwnerId)
                : null;
        }

        private bool IsTerrainBlockedForPlacementQuery(Vector2Int position)
        {
            if (_gridService != null && !_gridService.TryGetTileData(position, out _))
                return true;

            return _placementEnvironmentRules.IsBlockedByTerrain(position, out _);
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

            return _pendingPlacementByPosition.TryGetValue(
                    request.IgnoredPendingPosition.Value,
                    out PendingPlacement placement)
                ? placement.OriginalPosition
                : null;
        }

        private bool IsRelocationQuery(ConstructionPlacementQueryRequest request)
        {
            return request.IgnoredPendingPosition.HasValue
                && _pendingPlacementByPosition.TryGetValue(
                    request.IgnoredPendingPosition.Value,
                    out PendingPlacement placement)
                && IsRelocation(placement);
        }

        private void MarkPendingPlacementsChanged()
        {
            _pendingPlacementsVersion++;
            InvalidatePlacementAvailabilityCache();

            // Pass 62: pending count and reserved resources are part of
            // selection availability. As soon as the last legal preview is
            // added, the old selected building must stop accepting new clicks.
            RevalidateActiveSelectionAvailability(
                "pending-changed");
        }

        private void InvalidatePlacementAvailabilityCache()
        {
            _placementAvailabilityCacheFrame = -1;
            _placementAvailabilityCache.Clear();
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
                    out reason,
                    request.IncludePendingPlacements);
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
                request.IncludePendingPlacements ? _pendingPlacementsVersion : -1);
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
                out reason,
                request.IncludePendingPlacements);
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

        private ConstructionPlacementQueryResult InvalidPlacementQueryResult(
            bool availabilityValid,
            string reason,
            ConstructionPlacementQueryRequest request,
            string ownerId,
            BuildingPlacementBlockerKind blockerKind)
        {
            BuildingPlacementEvaluationResult evaluationResult = null;
            if (request.IncludeDetails)
            {
                evaluationResult = new BuildingPlacementEvaluationResult
                {
                    ConfigurationBlocked =
                        blockerKind == BuildingPlacementBlockerKind.Configuration,
                    TerrainBlocked =
                        blockerKind == BuildingPlacementBlockerKind.Terrain,
                };
                evaluationResult.AddBlocker(
                    new BuildingPlacementBlocker
                    {
                        Kind = blockerKind,
                        Message = reason,
                        Position = request.Position,
                        BuildingId = request.BuildingId,
                    });
            }

            return CreatePlacementQueryResult(
                availabilityValid,
                isSpatiallyValid: false,
                resourcesValid: false,
                authorityValid: true,
                isGateReplacement: false,
                reason,
                evaluationResult,
                request,
                ownerId);
        }
    }
}
