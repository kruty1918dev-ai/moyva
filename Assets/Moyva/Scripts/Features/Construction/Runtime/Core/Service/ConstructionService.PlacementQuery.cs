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
        private readonly Dictionary<
            BuildingDefinition,
            IReadOnlyList<BuildingValidationIssue>>
            _moduleValidationIssuesCache = new();
        private int _moduleValidationIssuesCacheRevision = -1;

        private IReadOnlyList<BuildingValidationIssue>
            GetModuleValidationIssuesCached(
                BuildingDefinition definition)
        {
            if (definition == null)
                return Array.Empty<BuildingValidationIssue>();

            int revision = BuildingDefinitionAsset.RuntimeRevision;
            if (_moduleValidationIssuesCacheRevision != revision)
            {
                _moduleValidationIssuesCacheRevision = revision;
                _moduleValidationIssuesCache.Clear();
            }

            if (_moduleValidationIssuesCache.TryGetValue(
                    definition,
                    out IReadOnlyList<BuildingValidationIssue> cached))
            {
                return cached;
            }

            IReadOnlyList<BuildingValidationIssue> evaluated =
                BuildingModuleValidation.Validate(definition);
            _moduleValidationIssuesCache[definition] = evaluated;
            return evaluated;
        }

        public ConstructionPlacementQueryResult EvaluatePlacement(ConstructionPlacementQueryRequest request)
        {
            AuditModuleRegistryIfNeeded();
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

            bool requiresReplacement = _replacementPolicy.RequiresReplacement(
                request.BuildingId,
                out ReplacementPlacementRuleModule replacementModule);
            bool gateReplacement = _replacementPolicy.TryResolveGateReplacement(
                request.Position,
                request.BuildingId,
                out Vector2Int replacedWallOriginValue,
                out string replacedWallId);
            bool pendingReplacementSatisfied =
                _replacementPolicy.IsReplacementSatisfiedByPendingMarker(
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
                && _replacementPolicy.RequiresSameOwner(replacementModule)
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
                && _replacementPolicy.RequiresSameOwner(replacementModule)
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


        public ConstructionSelectionAvailabilityResult
            EvaluateSelectionAvailability(
                string buildingId,
                string ownerId = null,
                Vector2Int? preferredFundingPosition = null,
                bool includePendingPlacements = true)
        {
            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            var request = new ConstructionPlacementQueryRequest(
                buildingId,
                preferredFundingPosition.GetValueOrDefault(),
                includeResources: false,
                includeDetails: false,
                ownerId: normalizedOwnerId,
                includePendingPlacements: includePendingPlacements,
                attemptSource: ConstructionPlacementAttemptSource.Unknown,
                allowUniquePreviewRelocation: false);

            if (string.IsNullOrWhiteSpace(buildingId))
            {
                return new ConstructionSelectionAvailabilityResult(
                    globalAvailabilityValid: false,
                    resourcesValid: false,
                    resourceCheckPerformed: false,
                    reason: "Building id is empty.",
                    reasonCode: "building-id-empty");
            }

            PlacementAvailabilityCacheValue availability =
                ResolvePlacementAvailabilityCached(
                    request,
                    normalizedOwnerId);
            if (!availability.IsValid)
            {
                return new ConstructionSelectionAvailabilityResult(
                    globalAvailabilityValid: false,
                    resourcesValid: false,
                    resourceCheckPerformed: false,
                    reason: availability.Reason,
                    reasonCode: availability.ReasonCode);
            }

            // Buildings with no cost are always resource-ready and do not need
            // a settlement/funding position.
            if (BuildConstructionCostMap(buildingId).Count == 0)
            {
                return new ConstructionSelectionAvailabilityResult(
                    globalAvailabilityValid: true,
                    resourcesValid: true,
                    resourceCheckPerformed: true);
            }

            if (!TryResolveSelectionFundingPosition(
                    normalizedOwnerId,
                    preferredFundingPosition,
                    out Vector2Int fundingPosition))
            {
                // Do not incorrectly disable a building just because the menu
                // opened before a settlement position was known. Pointer-click
                // validation still remains authoritative.
                return new ConstructionSelectionAvailabilityResult(
                    globalAvailabilityValid: true,
                    resourcesValid: true,
                    resourceCheckPerformed: false,
                    reasonCode: "resource-context-deferred");
            }

            var resourceRequest = new ConstructionPlacementQueryRequest(
                buildingId,
                fundingPosition,
                includeResources: true,
                includeDetails: false,
                ownerId: normalizedOwnerId,
                includePendingPlacements: includePendingPlacements,
                attemptSource: ConstructionPlacementAttemptSource.Unknown,
                allowUniquePreviewRelocation: false);
            bool resourcesValid = TryValidateConstructionResourcesCached(
                resourceRequest,
                normalizedOwnerId,
                out string resourceReason);

            return new ConstructionSelectionAvailabilityResult(
                globalAvailabilityValid: true,
                resourcesValid: resourcesValid,
                resourceCheckPerformed: true,
                reason: resourcesValid ? null : resourceReason,
                reasonCode: resourcesValid ? null : "resources");
        }

    }
}
