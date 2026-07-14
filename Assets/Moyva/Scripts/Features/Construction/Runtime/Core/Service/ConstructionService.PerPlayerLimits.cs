using System;
using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed partial class ConstructionService
    {
        private bool TryValidatePerPlayerBuildingLimit(
            ConstructionPlacementQueryRequest request,
            string ownerId,
            out string reason)
        {
            reason = null;

            BuildingDefinition definition = _placementBuildingRegistry?.GetById(request.BuildingId);
            int limit = BuildingDefinitionCapabilities.GetMaxBuildingsPerPlayer(definition);
            if (limit <= 0)
                return true;

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
            if (total < limit)
                return true;

            reason = $"Досягнуто ліміту будівель для гравця: {total}/{limit}.";
            return false;
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

                if (string.Equals(pair.Value.BuildingId, buildingId, StringComparison.Ordinal)
                    && string.Equals(pair.Value.FactionId, ownerId, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            // Legacy/local placements do not store an owner separately. They belong to the
            // currently active owner and are used only outside faction-authoritative placement.
            if (!string.Equals(ownerId, NormalizeOwnerId(_activeOwnerId), StringComparison.Ordinal))
                return count;

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
            if (!string.Equals(ownerId, NormalizeOwnerId(_activeOwnerId), StringComparison.Ordinal))
                return 0;

            int count = 0;
            for (int index = 0; index < _pendingPlacements.Count; index++)
            {
                var placement = _pendingPlacements[index];
                if (request.IgnoredPendingPosition.HasValue
                    && placement.Position == request.IgnoredPendingPosition.Value)
                {
                    continue;
                }

                if (string.Equals(placement.BuildingId, request.BuildingId, StringComparison.Ordinal))
                    count++;
            }

            return count;
        }
    }
}
