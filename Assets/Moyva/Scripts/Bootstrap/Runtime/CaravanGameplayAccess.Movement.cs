using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed partial class CaravanGameplayAccess
    {
        public async Task<CaravanTransferResult> MoveToWarehouseAsync(
            string unitId, Vector2Int origin, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (!TryGetWagon(unitId, out var unit))
                return CaravanTransferResult.Rejected("The wagon no longer exists.");
            if (!CanCommand(unit.OwnerId, unitId, out var reason))
                return CaravanTransferResult.Rejected(reason);
            if (CanAccessWarehouse(unitId, origin, out _)) return CaravanTransferResult.Success();
            if (_lifecycle != null && !_lifecycle.IsOperational(origin))
                return CaravanTransferResult.Rejected("The route warehouse is not operational yet.");
            if (_pathfinder is not ICostAwarePathfinder pathfinder)
                return CaravanTransferResult.Rejected("Route planning is unavailable.");

            var footprint = new HashSet<Vector2Int>();
            foreach (var placement in _portfolio.GetOwnerPlacements(unit.OwnerId))
            {
                if (placement.Position != origin) continue;
                var definition = _buildings.GetById(placement.BuildingId);
                for (int i = 0; i < BuildingFootprintUtility.GetOccupiedCellCount(definition); i++)
                    footprint.Add(BuildingFootprintUtility.GetOccupiedCell(definition, origin, i, placement.Rotation));
                break;
            }
            if (footprint.Count == 0) return CaravanTransferResult.Rejected("The destination warehouse no longer exists.");
            if (_units.GetStamina(unitId) <= 0) return CaravanTransferResult.Rejected("Waiting for movement to recover.");

            // Treat only the target footprint as a virtual destination. Execution stops outside it.
            var path = pathfinder.FindPathWithCosts(unit.Position, origin,
                (Vector2Int from, Vector2Int to, out float cost) =>
                {
                    if (footprint.Contains(to)) { cost = 1f; return true; }
                    return _traversal.TryEvaluateStep(unitId, from, to, float.PositiveInfinity,
                        UnitTraversalMode.Pathfinding, out cost, out _);
                });
            if (path == null || path.Count <= 1)
                return CaravanTransferResult.Rejected("The route is blocked. Waiting for a clear path.");

            float remaining = _units.GetStamina(unitId);
            int last = 0;
            for (int i = 1; i < path.Count && !BesideFootprint(path[last], footprint); i++)
            {
                if (footprint.Contains(path[i])
                    || !_traversal.TryEvaluateStep(unitId, path[i - 1], path[i], remaining,
                        UnitTraversalMode.Pathfinding, out float cost, out _)) break;
                remaining -= cost;
                last = i;
            }
            if (last == 0) return CaravanTransferResult.Rejected("Waiting for enough movement to continue.");
            await _movement.MoveUnitAsync(unitId, path[last], token);
            token.ThrowIfCancellationRequested();
            return CanAccessWarehouse(unitId, origin, out reason) ? CaravanTransferResult.Success()
                : CaravanTransferResult.Rejected("Delivery is in progress. Waiting for the next movement allowance.");
        }

        private static bool BesideFootprint(Vector2Int position, HashSet<Vector2Int> footprint)
        {
            for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                    if (footprint.Contains(position + new Vector2Int(x, y))) return true;
            return false;
        }
    }
}
