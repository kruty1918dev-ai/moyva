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

        /*
         * Prerequisites and per-owner limits are global for a building
         * selection. They do not depend on the candidate tile position.
         */
        private readonly Dictionary<
            PlacementAvailabilityCacheKey,
            PlacementAvailabilityCacheValue>
            _placementAvailabilityCache = new();

        private int _placementAvailabilityCacheFrame = -1;

        public ConstructionPlacementQueryResult EvaluatePlacement(ConstructionPlacementQueryRequest request)
        {
            string placementOwnerId = string.IsNullOrWhiteSpace(request.OwnerId)
                ? _activeOwnerId
                : request.OwnerId.Trim();
            request = ResolveEffectivePlacementRequest(request, placementOwnerId);
            BuildingPerPlayerLimitEvaluation limitEvaluation =
                BuildingPerPlayerLimitEvaluation.Disabled;

            if (string.IsNullOrWhiteSpace(request.BuildingId))
            {
                return InvalidPlacementQueryResult(
                    availabilityValid: false,
                    "Building id is empty.",
                    "building-id-empty",
                    request,
                    placementOwnerId,
                    limitEvaluation,
                    BuildingPlacementBlockerKind.Configuration);
            }

            PlacementAvailabilityCacheValue availability =
                ResolvePlacementAvailabilityCached(
                    request,
                    placementOwnerId);

            limitEvaluation =
                availability.LimitEvaluation;

            if (!availability.IsValid)
            {
                return InvalidPlacementQueryResult(
                    availabilityValid: false,
                    availability.Reason,
                    availability.ReasonCode,
                    request,
                    placementOwnerId,
                    limitEvaluation,
                    availability.BlockerKind);
            }

            if (_gridService != null
                && !_gridService.TryGetTileData(request.Position, out _))
            {
                return InvalidPlacementQueryResult(
                    availabilityValid: true,
                    "Tile does not exist.",
                    "tile-missing",
                    request,
                    placementOwnerId,
                    limitEvaluation,
                    BuildingPlacementBlockerKind.Terrain);
            }

            bool requiresReplacement = RequiresReplacement(
                request.BuildingId,
                out ReplacementPlacementRuleModule replacementModule);
            bool gateReplacement = TryResolveGateReplacement(
                request.Position,
                request.BuildingId,
                out Vector2Int replacedWallOriginValue,
                out string replacedWallId);
            bool pendingReplacementSatisfied =
                IsReplacementSatisfiedByPendingMarker(
                    request.BuildingId,
                    request.SatisfiedReplacementBuildingId,
                    replacementModule);
            if (requiresReplacement
                && !gateReplacement
                && !pendingReplacementSatisfied)
            {
                return InvalidPlacementQueryResult(
                    availabilityValid: true,
                    "This building must replace a configured building type.",
                    "replacement-required",
                    request,
                    placementOwnerId,
                    limitEvaluation,
                    BuildingPlacementBlockerKind.Configuration);
            }

            Vector2Int? ignoredOccupiedPosition = ResolveIgnoredOccupiedPosition(request);
            Vector2Int? replacedWallOrigin = gateReplacement
                ? replacedWallOriginValue
                : null;
            if (gateReplacement
                && RequiresSameOwner(replacementModule)
                && replacedWallOrigin.HasValue
                && _factionPlacedBuildings.TryGetValue(
                    replacedWallOrigin.Value,
                    out var replacedFactionEntry)
                && !string.Equals(
                    replacedFactionEntry.FactionId,
                    placementOwnerId,
                    StringComparison.Ordinal))
            {
                return InvalidPlacementQueryResult(
                    availabilityValid: true,
                    "Gate cannot replace a wall owned by another faction.",
                    "gate-foreign-faction-wall",
                    request,
                    placementOwnerId,
                    limitEvaluation,
                    BuildingPlacementBlockerKind.Configuration);
            }

            if (gateReplacement
                && RequiresSameOwner(replacementModule)
                && replacedWallOrigin.HasValue
                && _playerPlacedBuildings.ContainsKey(replacedWallOrigin.Value)
                && !string.Equals(
                    _activeOwnerId,
                    placementOwnerId,
                    StringComparison.Ordinal))
            {
                return InvalidPlacementQueryResult(
                    availabilityValid: true,
                    "Gate cannot replace a player wall owned by another faction.",
                    "gate-foreign-player-wall",
                    request,
                    placementOwnerId,
                    limitEvaluation,
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
                string reason = ResolveEvaluationReason(evaluationResult)
                    ?? "Placement rules blocked this tile.";
                return CreatePlacementQueryResult(
                    availabilityValid:
                        evaluationResult?.ConfigurationBlocked != true,
                    isSpatiallyValid: false,
                    resourcesValid: false,
                    authorityValid: true,
                    gateReplacement,
                    reason,
                    evaluationResult,
                    request,
                    placementOwnerId,
                    limitEvaluation);
            }

            bool resourcesValid = true;
            string resourceReason = null;
            if (request.IncludeResources && !IsRelocationQuery(request))
            {
                resourcesValid = TryValidateConstructionResourcesCached(
                    request,
                    placementOwnerId,
                    out resourceReason);
            }

            return CreatePlacementQueryResult(
                availabilityValid: true,
                isSpatiallyValid: true,
                resourcesValid,
                authorityValid: true,
                gateReplacement,
                resourceReason,
                evaluationResult,
                request,
                placementOwnerId,
                limitEvaluation,
                resourcesValid ? null : "resources");
        }


        private PlacementAvailabilityCacheValue
            ResolvePlacementAvailabilityCached(
                ConstructionPlacementQueryRequest request,
                string ownerId)
        {
            /*
             * Relocation queries can ignore an existing or pending origin,
             * which changes the per-owner limit result. Keep those queries
             * on the authoritative uncached path.
             */
            bool cacheable =
                Application.isPlaying
                && !request.IgnoredPendingPosition.HasValue
                && !request.IgnoredOccupiedPosition.HasValue;

            if (!cacheable)
            {
                return EvaluatePlacementAvailability(
                    request,
                    ownerId);
            }

            int frame =
                Time.frameCount;

            if (_placementAvailabilityCacheFrame
                != frame)
            {
                _placementAvailabilityCacheFrame =
                    frame;

                _placementAvailabilityCache.Clear();
            }

            string normalizedOwnerId =
                NormalizeOwnerId(ownerId);

            var key =
                new PlacementAvailabilityCacheKey(
                    request.BuildingId,
                    normalizedOwnerId,
                    request.IncludePendingPlacements,
                    request.IncludePendingPlacements
                        ? _pendingPlacementsVersion
                        : -1);

            if (_placementAvailabilityCache.TryGetValue(
                    key,
                    out PlacementAvailabilityCacheValue cached))
            {
                return cached;
            }

            PlacementAvailabilityCacheValue evaluated =
                EvaluatePlacementAvailability(
                    request,
                    normalizedOwnerId);

            _placementAvailabilityCache[key] =
                evaluated;

            return evaluated;
        }

        private PlacementAvailabilityCacheValue
            EvaluatePlacementAvailability(
                ConstructionPlacementQueryRequest request,
                string ownerId)
        {
            BuildingDefinition requestedDefinition =
                _placementBuildingRegistry?.GetById(
                    request.BuildingId);

            if (requestedDefinition == null)
            {
                return PlacementAvailabilityCacheValue.Invalid(
                    $"Building definition '{request.BuildingId}' is missing.",
                    "building-definition-missing",
                    BuildingPlacementBlockerKind.Configuration,
                    BuildingPerPlayerLimitEvaluation.Disabled);
            }

            if (!TryValidateBuildingPrerequisites(
                    requestedDefinition,
                    ownerId,
                    out string prerequisiteReason))
            {
                return PlacementAvailabilityCacheValue.Invalid(
                    prerequisiteReason,
                    "building-prerequisite",
                    BuildingPlacementBlockerKind.Prerequisite,
                    BuildingPerPlayerLimitEvaluation.Disabled);
            }

            if (!TryValidatePerPlayerBuildingLimit(
                    request,
                    ownerId,
                    out BuildingPerPlayerLimitEvaluation limitEvaluation))
            {
                return PlacementAvailabilityCacheValue.Invalid(
                    limitEvaluation.Reason,
                    "per-player-limit",
                    BuildingPlacementBlockerKind.Configuration,
                    limitEvaluation);
            }

            return PlacementAvailabilityCacheValue.Valid(
                limitEvaluation);
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
            _placementQueryEvaluationRequest.OwnerId = string.IsNullOrWhiteSpace(
                request.OwnerId)
                ? _activeOwnerId
                : request.OwnerId.Trim();
            _placementQueryEvaluationRequest.Position = request.Position;
            _placementQueryEvaluationRequest.IgnoredPendingPosition = request.IgnoredPendingPosition;
            _placementQueryEvaluationRequest.IgnoredOccupiedPosition = ignoredOccupiedPosition;
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
                IsFogBlocked = IsBlockedByFog,
                GetFogState = GetFogStateForPlacementQuery,
                IsTerrainBlocked = IsTerrainBlockedForPlacementQuery,
                GetTerrainLevel = GetTerrainLevelForPlacementQuery,
                GetTileId = GetTileId,
                HasTerrainTag = HasTerrainTagForPlacementQuery,
                PendingPlacements = _placementSimulationSnapshot,
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
            string tileId = GetTileId(position);
            return _tileSettings is Kruty1918.Moyva.Grid.API.ITerrainTagQuery tagQuery
                && tagQuery.HasTerrainTag(tileId, tag);
        }

        private string GetOccupantForPlacementQuery(Vector2Int position)
            => IsIgnoredOccupiedFootprintCell(position)
                ? null
                : GetObjectOccupantId(position);

        private Vector2Int? GetOccupantOriginForPlacementQuery(Vector2Int position)
            => _placedOriginByOccupiedTile.TryGetValue(position, out Vector2Int origin)
                ? origin
                : null;

        private string GetOccupantOwnerForPlacementQuery(
            Vector2Int position)
        {
            Vector2Int origin = _placedOriginByOccupiedTile.TryGetValue(
                position,
                out Vector2Int resolvedOrigin)
                ? resolvedOrigin
                : position;
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
            InvalidatePlacementAvailabilityCache();
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
            string reasonCode,
            ConstructionPlacementQueryRequest request,
            string ownerId,
            BuildingPerPlayerLimitEvaluation limitEvaluation,
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
                ownerId,
                limitEvaluation,
                reasonCode);
        }


        private readonly struct PlacementAvailabilityCacheKey
            : IEquatable<PlacementAvailabilityCacheKey>
        {
            public PlacementAvailabilityCacheKey(
                string buildingId,
                string ownerId,
                bool includePendingPlacements,
                int pendingVersion)
            {
                BuildingId = buildingId;
                OwnerId = ownerId;
                IncludePendingPlacements =
                    includePendingPlacements;
                PendingVersion = pendingVersion;
            }

            private string BuildingId { get; }
            private string OwnerId { get; }
            private bool IncludePendingPlacements { get; }
            private int PendingVersion { get; }

            public bool Equals(
                PlacementAvailabilityCacheKey other)
            {
                return IncludePendingPlacements
                       == other.IncludePendingPlacements
                    && PendingVersion
                       == other.PendingVersion
                    && string.Equals(
                        BuildingId,
                        other.BuildingId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        OwnerId,
                        other.OwnerId,
                        StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                return obj
                    is PlacementAvailabilityCacheKey other
                    && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash =
                        PendingVersion;

                    hash =
                        hash * 397
                        ^ (
                            IncludePendingPlacements
                                ? 1
                                : 0
                        );

                    hash =
                        hash * 397
                        ^ (
                            BuildingId != null
                                ? StringComparer.Ordinal.GetHashCode(
                                    BuildingId)
                                : 0
                        );

                    hash =
                        hash * 397
                        ^ (
                            OwnerId != null
                                ? StringComparer.Ordinal.GetHashCode(
                                    OwnerId)
                                : 0
                        );

                    return hash;
                }
            }
        }

        private readonly struct PlacementAvailabilityCacheValue
        {
            private PlacementAvailabilityCacheValue(
                bool isValid,
                string reason,
                string reasonCode,
                BuildingPlacementBlockerKind blockerKind,
                BuildingPerPlayerLimitEvaluation limitEvaluation)
            {
                IsValid = isValid;
                Reason = reason;
                ReasonCode = reasonCode;
                BlockerKind = blockerKind;
                LimitEvaluation = limitEvaluation;
            }

            public bool IsValid { get; }
            public string Reason { get; }
            public string ReasonCode { get; }
            public BuildingPlacementBlockerKind BlockerKind { get; }
            public BuildingPerPlayerLimitEvaluation LimitEvaluation { get; }

            public static PlacementAvailabilityCacheValue Valid(
                BuildingPerPlayerLimitEvaluation limitEvaluation)
            {
                return new PlacementAvailabilityCacheValue(
                    true,
                    null,
                    null,
                    BuildingPlacementBlockerKind.Configuration,
                    limitEvaluation);
            }

            public static PlacementAvailabilityCacheValue Invalid(
                string reason,
                string reasonCode,
                BuildingPlacementBlockerKind blockerKind,
                BuildingPerPlayerLimitEvaluation limitEvaluation)
            {
                return new PlacementAvailabilityCacheValue(
                    false,
                    reason,
                    reasonCode,
                    blockerKind,
                    limitEvaluation);
            }
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
