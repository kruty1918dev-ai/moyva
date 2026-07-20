using System;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal readonly struct BuildingPerPlayerLimitEvaluation
    {
        public BuildingPerPlayerLimitEvaluation(
            int limit,
            int existingCount,
            int pendingCount,
            string reason)
        {
            Limit = Mathf.Max(0, limit);
            ExistingCount = Mathf.Max(0, existingCount);
            PendingCount = Mathf.Max(0, pendingCount);
            Reason = reason;
        }

        public int Limit { get; }
        public int ExistingCount { get; }
        public int PendingCount { get; }
        public int TotalCount => ExistingCount + PendingCount;
        public string Reason { get; }
        public bool IsEnabled => Limit > 0;
        public bool IsValid => !IsEnabled || TotalCount < Limit;

        public static BuildingPerPlayerLimitEvaluation Disabled
            => new BuildingPerPlayerLimitEvaluation(0, 0, 0, null);
    }

    internal sealed partial class ConstructionService
    {
        private bool TryValidatePerPlayerBuildingLimit(
            ConstructionPlacementQueryRequest request,
            string ownerId,
            out BuildingPerPlayerLimitEvaluation evaluation)
        {
            BuildingDefinition definition =
                _placementBuildingRegistry?.GetById(request.BuildingId);
            int limit = BuildingDefinitionCapabilities.GetMaxBuildingsPerPlayer(definition);
            if (limit <= 0)
            {
                evaluation = BuildingPerPlayerLimitEvaluation.Disabled;
                return true;
            }

            string normalizedOwnerId = NormalizeOwnerId(ownerId);
            Vector2Int? ignoredPlacedOrigin = ResolveIgnoredOccupiedPosition(request);
            int existingCount = CountPlacedBuildingsForOwner(
                request.BuildingId,
                normalizedOwnerId,
                ignoredPlacedOrigin);
            int pendingCount = request.IncludePendingPlacements
                ? CountPendingBuildingsForOwner(request, normalizedOwnerId)
                : 0;
            int total = existingCount + pendingCount;
            string reason = total < limit
                ? null
                : $"Досягнуто ліміту будівель для гравця: {total}/{limit}.";

            evaluation = new BuildingPerPlayerLimitEvaluation(
                limit,
                existingCount,
                pendingCount,
                reason);
            return evaluation.IsValid;
        }

        private int CountPlacedBuildingsForOwner(
            string buildingId,
            string ownerId,
            Vector2Int? ignoredOrigin)
        {
            int count = 0;
            foreach (var pair in _factionPlacedBuildings)
            {
                if (ignoredOrigin.HasValue && pair.Key == ignoredOrigin.Value)
                    continue;

                if (string.Equals(
                        pair.Value.BuildingId,
                        buildingId,
                        StringComparison.Ordinal)
                    && string.Equals(
                        pair.Value.FactionId,
                        ownerId,
                        StringComparison.Ordinal))
                {
                    count++;
                }
            }

            // Legacy/local placements do not store an owner separately. They belong to the
            // currently active owner and are used only outside faction-authoritative placement.
            if (!string.Equals(
                    ownerId,
                    NormalizeOwnerId(_activeOwnerId),
                    StringComparison.Ordinal))
            {
                return count;
            }

            foreach (var pair in _playerPlacedBuildings)
            {
                if (ignoredOrigin.HasValue && pair.Key == ignoredOrigin.Value)
                    continue;

                if (string.Equals(pair.Value, buildingId, StringComparison.Ordinal))
                    count++;
            }

            return count;
        }

        private int CountPendingBuildingsForOwner(
            ConstructionPlacementQueryRequest request,
            string ownerId)
        {
            // Pending placements are local to the active construction owner. Network commands
            // use TryDirectPlace and therefore have no local pending state to count.
            if (!string.Equals(
                    ownerId,
                    NormalizeOwnerId(_activeOwnerId),
                    StringComparison.Ordinal))
            {
                return 0;
            }

            int count = 0;
            for (int index = 0; index < _pendingPlacements.Count; index++)
            {
                PendingPlacement placement = _pendingPlacements[index];
                if (request.IgnoredPendingPosition.HasValue
                    && placement.Position == request.IgnoredPendingPosition.Value)
                {
                    continue;
                }

                if (string.Equals(
                        placement.BuildingId,
                        request.BuildingId,
                        StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
