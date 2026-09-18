using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        private ConstructionPlacementQueryRequest ResolveEffectivePlacementRequest(
            ConstructionPlacementQueryRequest request,
            string ownerId)
        {
            if (!request.AllowUniquePreviewRelocation
                || request.IgnoredPendingPosition.HasValue
                || request.IgnoredOccupiedPosition.HasValue
                || string.IsNullOrWhiteSpace(request.BuildingId))
            {
                return request;
            }

            BuildingDefinition definition =
                _placementBuildingRegistry?.GetById(request.BuildingId);
            if (BuildingDefinitionCapabilities.TryGetEnabledModule(
                    definition,
                    out BuildingPerPlayerLimitModule limitModule)
                && Mathf.Max(0, limitModule.MaxBuildingsPerPlayer) > 0
                && limitModule.OverflowPolicy != BuildingLimitOverflowPolicy.Legacy)
            {
                if (limitModule.OverflowPolicy == BuildingLimitOverflowPolicy.Block
                    || !IsBuildingLimitAtCapacity(request.BuildingId, ownerId, limitModule))
                {
                    return request;
                }

                if (TryFindPendingPlacementByBuildingId(request.BuildingId, out int pendingIndex))
                {
                    PendingPlacement pending = _pendingPlacements[pendingIndex];
                    return request.WithIgnoredPositions(pending.Position, pending.OriginalPosition);
                }

                if (limitModule.OverflowPolicy == BuildingLimitOverflowPolicy.RelocateExisting
                    && TryFindOwnedPlacedBuildingPosition(
                        request.BuildingId,
                        ownerId,
                        out Vector2Int originalPosition))
                {
                    return request.WithIgnoredPositions(null, originalPosition);
                }

                return request;
            }

            BuildingPlacementUniquenessScope scope =
                BuildingDefinitionCapabilities.GetPlacementUniquenessScope(definition);
            if (scope == BuildingPlacementUniquenessScope.None)
                return request;

            if (TryFindPendingPlacementByBuildingId(request.BuildingId, out int uniquePendingIndex))
            {
                PendingPlacement pending = _pendingPlacements[uniquePendingIndex];
                return request.WithIgnoredPositions(pending.Position, pending.OriginalPosition);
            }

            return TryFindPlacedBuildingPosition(
                    request.BuildingId,
                    ownerId,
                    scope,
                    out Vector2Int uniqueOriginalPosition)
                ? request.WithIgnoredPositions(null, uniqueOriginalPosition)
                : request;
        }

        private ConstructionPlacementQueryResult CreatePlacementQueryResult(
            bool availabilityValid,
            bool isSpatiallyValid,
            bool resourcesValid,
            bool authorityValid,
            bool isGateReplacement,
            string reason,
            BuildingPlacementEvaluationResult evaluationResult,
            ConstructionPlacementQueryRequest request,
            string ownerId)
        {
            if (authorityValid
                && _placementAuthorityPolicy != null
                && !_placementAuthorityPolicy.CanCommit(
                    ownerId,
                    request.AttemptSource,
                    out string authorityReason))
            {
                authorityValid = false;
                if (string.IsNullOrWhiteSpace(reason))
                    reason = authorityReason ?? "Placement requires host authority.";
            }

            return new ConstructionPlacementQueryResult(
                availabilityValid,
                isSpatiallyValid,
                resourcesValid,
                authorityValid,
                isGateReplacement,
                reason,
                evaluationResult);
        }
    }
}
