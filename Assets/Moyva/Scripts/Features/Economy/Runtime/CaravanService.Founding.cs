using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Economy.Runtime
{
    internal sealed partial class CaravanService
    {
        private const string DefaultTownHallBuildingId = "townhall";
        private readonly Dictionary<Vector2Int, string> _pendingFoundingCargoDeposits = new();
        private static readonly Vector2Int[] FoundingOffsets =
        {
            new(0, 1), new(1, 0), new(0, -1), new(-1, 0),
            new(1, 1), new(1, -1), new(-1, -1), new(-1, 1),
        };

        public CaravanTransferResult CanFoundSettlement(string ownerId, string unitId, string buildingId,
            out Vector2Int position)
        {
            position = default;
            string targetBuildingId = string.IsNullOrWhiteSpace(buildingId)
                ? DefaultTownHallBuildingId
                : buildingId.Trim();
            if (_buildings == null || _placementQuery == null || _prepaidPlacement == null)
                return CaravanTransferResult.Rejected("Settlement founding is not connected.");
            if (HasActiveRoute(unitId))
                return CaravanTransferResult.Rejected("Stop the route before founding a settlement.");
            if (!_gameplay.Value.CanCommand(ownerId, unitId, out string reason))
                return CaravanTransferResult.Rejected(reason);
            if (!_gameplay.Value.TryGetWagon(unitId, out var unit)
                || !string.Equals(unit.OwnerId, ownerId, StringComparison.Ordinal))
                return CaravanTransferResult.Rejected("Select a wagon belonging to your kingdom.");
            var definition = _buildings.GetById(targetBuildingId);
            if (definition == null || !BuildingDefinitionCapabilities.IsTownHall(definition))
                return CaravanTransferResult.Rejected("Town Hall definition is missing or invalid.");
            var costs = BuildConstructionCostMap(definition);
            if (costs.Count == 0)
                return CaravanTransferResult.Rejected("Town Hall founding cost is not configured.");
            if (!_cargo.TryGetValue(unitId, out var cargo) || !ContainsAll(cargo.Resources, costs))
                return CaravanTransferResult.Rejected("Load the wagon with the Town Hall founding materials.");

            for (int index = 0; index < FoundingOffsets.Length; index++)
            {
                var candidate = unit.Position + FoundingOffsets[index];
                var placement = _placementQuery.EvaluatePlacement(new ConstructionPlacementQueryRequest(
                    targetBuildingId,
                    candidate,
                    includeResources: false,
                    includeDetails: true,
                    ownerId: ownerId,
                    includePendingPlacements: false,
                    attemptSource: ConstructionPlacementAttemptSource.DirectPlace,
                    allowUniquePreviewRelocation: false));
                if (placement.AvailabilityValid && placement.SpatialValid && placement.AuthorityValid)
                {
                    position = candidate;
                    return CaravanTransferResult.Success();
                }
                if (string.IsNullOrWhiteSpace(reason))
                    reason = placement.Reason;
            }

            return CaravanTransferResult.Rejected(string.IsNullOrWhiteSpace(reason)
                ? "No valid adjacent tile for a Town Hall."
                : reason);
        }

        public CaravanTransferResult FoundSettlement(string ownerId, string unitId, string buildingId)
        {
            string targetBuildingId = string.IsNullOrWhiteSpace(buildingId)
                ? DefaultTownHallBuildingId
                : buildingId.Trim();
            var validation = CanFoundSettlement(ownerId, unitId, targetBuildingId, out var position);
            if (!validation.Succeeded)
                return validation;

            var costs = BuildConstructionCostMap(_buildings.GetById(targetBuildingId));
            var before = new Dictionary<string, float>(_cargo[unitId].Resources, StringComparer.Ordinal);
            foreach (var cost in costs)
                Add(_cargo[unitId].Resources, cost.Key, -cost.Value);

            if (!_prepaidPlacement.TryPlacePrepaidAuthoritatively(
                    targetBuildingId,
                    position,
                    ownerId,
                    ConstructionPlacementCommitIntent.None))
            {
                _cargo[unitId].Resources.Clear();
                foreach (var pair in before)
                    _cargo[unitId].Resources[pair.Key] = pair.Value;
                return CaravanTransferResult.Rejected("Town Hall placement was rejected by construction rules.");
            }

            DepositRemainingCargoToNewSettlement(unitId, position);
            Changed?.Invoke();
            return CaravanTransferResult.Success();
        }

        public CaravanTransferResult ApplyConfirmedTransfer(CaravanCargoRequest request)
            => ExecuteCargo(request, fromRoute: true, requireAuthority: false);

        public CaravanTransferResult ApplyConfirmedRouteTransfer(CaravanCargoRequest request,
            CaravanRoutePhase nextPhase)
        {
            if (nextPhase < CaravanRoutePhase.ToSource || nextPhase > CaravanRoutePhase.Completed)
                return CaravanTransferResult.Rejected("Confirmed route phase is invalid.");
            if (!_routes.TryGetValue(request.UnitId, out var route))
                return CaravanTransferResult.Rejected("Confirmed route is unavailable.");
            if (!string.Equals(route.Request.OwnerId, request.OwnerId, StringComparison.Ordinal))
                return CaravanTransferResult.Rejected("Confirmed route owner does not match.");
            return ExecuteCargo(request, true, () => route.Phase = nextPhase, false);
        }

        public CaravanTransferResult ApplyConfirmedSetRoute(CaravanRouteRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.OwnerId) || string.IsNullOrWhiteSpace(request.UnitId))
                return CaravanTransferResult.Rejected("Confirmed route is missing owner or wagon.");
            if (!TryGetCargo(request.OwnerId, request.UnitId, out _))
                return CaravanTransferResult.Rejected("Confirmed route wagon is unavailable.");
            if (!TryGetTotal(request.Resources, out _))
                return CaravanTransferResult.Rejected("Confirmed route has invalid resources.");
            SetRouteState(request);
            Changed?.Invoke();
            return CaravanTransferResult.Success();
        }

        public CaravanTransferResult ApplyConfirmedStopRoute(string ownerId, string unitId)
        {
            RemoveRoute(unitId);
            Changed?.Invoke();
            return CaravanTransferResult.Success();
        }

        public CaravanTransferResult ApplyConfirmedFoundSettlement(string ownerId, string unitId, string buildingId,
            Vector2Int position)
        {
            string targetBuildingId = string.IsNullOrWhiteSpace(buildingId)
                ? DefaultTownHallBuildingId
                : buildingId.Trim();
            if (_buildings == null || !_cargo.TryGetValue(unitId, out var cargo))
                return CaravanTransferResult.Rejected("Confirmed founding cargo is unavailable.");
            var costs = BuildConstructionCostMap(_buildings.GetById(targetBuildingId));
            if (!ContainsAll(cargo.Resources, costs))
                return CaravanTransferResult.Rejected("Confirmed founding cargo is incomplete.");
            foreach (var cost in costs)
                Add(cargo.Resources, cost.Key, -cost.Value);
            if (!DepositRemainingCargoToNewSettlement(unitId, position) && cargo.Resources.Count > 0)
                _pendingFoundingCargoDeposits[position] = unitId;
            Changed?.Invoke();
            return CaravanTransferResult.Success();
        }

        private bool DepositRemainingCargoToNewSettlement(string unitId, Vector2Int townHallPosition)
        {
            if (!_cargo.TryGetValue(unitId, out var cargo) || cargo.Resources.Count == 0)
                return true;
            if (!_settlements.TryGetSettlementByPosition(townHallPosition, out var settlement) || settlement == null)
                return false;

            string warehouseKey = $"{townHallPosition.x}:{townHallPosition.y}";
            foreach (var pair in new Dictionary<string, float>(cargo.Resources, StringComparer.Ordinal))
            {
                settlement.AddResource(pair.Key, pair.Value, warehouseKey);
                Add(cargo.Resources, pair.Key, -pair.Value);
                _signals.Fire(new SettlementResourceChangedSignal
                {
                    SettlementId = settlement.SettlementId,
                    OwnerId = settlement.OwnerId,
                    ResourceId = pair.Key,
                    NewAmount = settlement.GetResource(pair.Key),
                    Delta = pair.Value,
                });
            }
            settlement.EnsureWarehouseConsistency();
            _pendingFoundingCargoDeposits.Remove(townHallPosition);
            return true;
        }

        private void OnSettlementCreated(SettlementCreatedSignal signal)
        {
            if (_pendingFoundingCargoDeposits.TryGetValue(signal.TownHallPosition, out string unitId)
                && DepositRemainingCargoToNewSettlement(unitId, signal.TownHallPosition))
            {
                Changed?.Invoke();
            }
        }

        private static Dictionary<string, float> BuildConstructionCostMap(BuildingDefinition definition)
        {
            var result = new Dictionary<string, float>(StringComparer.Ordinal);
            var costs = BuildingDefinitionCapabilities.GetConstructionCost(definition);
            for (int index = 0; index < costs.Count; index++)
            {
                var entry = costs[index];
                if (entry == null || string.IsNullOrWhiteSpace(entry.ResourceId) || entry.Amount <= 0)
                    continue;
                Add(result, entry.ResourceId.Trim(), entry.Amount);
            }
            return result;
        }
    }
}
