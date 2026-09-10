using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Economy.Runtime
{
    internal sealed partial class CaravanService : ICaravanService
    {
        private const float Epsilon = 0.0001f;
        private static readonly IReadOnlyDictionary<string, float> Empty =
            new ReadOnlyDictionary<string, float>(new Dictionary<string, float>());
        private readonly ISettlementRegistry _settlements;
        private readonly SignalBus _signals;
        private readonly LazyInject<ICaravanGameplayAccess> _gameplay;
        private readonly IBuildingRegistry _buildings;
        private readonly IConstructionPlacementQuery _placementQuery;
        private readonly IConstructionPrepaidPlacementExecutor _prepaidPlacement;
        private readonly Dictionary<string, CargoState> _cargo = new(StringComparer.Ordinal);
        private readonly Dictionary<Vector2Int, Dictionary<string, float>> _loot = new();
        private bool _executing;

        private sealed class CargoState
        {
            public string OwnerId;
            public Vector2Int Position;
            public readonly Dictionary<string, float> Resources = new(StringComparer.Ordinal);
        }

        public CaravanService(ISettlementRegistry settlements, SignalBus signals,
            LazyInject<ICaravanGameplayAccess> gameplay,
            [InjectOptional] IBuildingRegistry buildings = null,
            [InjectOptional] IConstructionPlacementQuery placementQuery = null,
            [InjectOptional] IConstructionPrepaidPlacementExecutor prepaidPlacement = null)
        {
            _settlements = settlements;
            _signals = signals;
            _gameplay = gameplay;
            _buildings = buildings;
            _placementQuery = placementQuery;
            _prepaidPlacement = prepaidPlacement;
        }

        public event Action Changed;
        public event Action<CaravanRouteTransferCommitted> RouteTransferCommitted;

        public bool TryGetCargo(string ownerId, string unitId, out CaravanCargoSnapshot snapshot)
        {
            snapshot = default;
            if (string.IsNullOrWhiteSpace(ownerId) || string.IsNullOrWhiteSpace(unitId)
                || !_gameplay.Value.TryGetWagon(unitId, out var unit)
                || !string.Equals(unit.OwnerId, ownerId, StringComparison.Ordinal)) return false;
            snapshot = new CaravanCargoSnapshot(unit,
                _cargo.TryGetValue(unitId, out var cargo) ? Freeze(cargo.Resources) : Empty);
            return true;
        }

        public IReadOnlyDictionary<string, float> GetLootAtWagon(string ownerId, string unitId)
            => TryGetCargo(ownerId, unitId, out var cargo)
                && _loot.TryGetValue(cargo.Unit.Position, out var loot) ? Freeze(loot) : Empty;

        public CaravanTransferResult CanExecute(CaravanCargoRequest request)
            => HasActiveRoute(request.UnitId)
                ? CaravanTransferResult.Rejected("Stop the route before handling cargo manually.")
                : Validate(request, out _, out _, out _);

        public CaravanTransferResult Execute(CaravanCargoRequest request)
            => ExecuteCargo(request, false);

        private CaravanTransferResult ExecuteCargo(CaravanCargoRequest request, bool fromRoute,
            Action beforeNotify = null,
            bool requireAuthority = true)
        {
            if (!fromRoute && HasActiveRoute(request.UnitId))
                return CaravanTransferResult.Rejected("Stop the route before handling cargo manually.");
            if (_executing) return CaravanTransferResult.Rejected("A cargo operation is already in progress.");
            var result = Validate(request, out var unit, out var settlement, out var warehouse, requireAuthority);
            if (!result.Succeeded) return result;
            // Freeze the batch before mutation; notify only after both inventories commit.
            var resources = new Dictionary<string, float>(StringComparer.Ordinal);
            foreach (var pair in request.Resources) resources.Add(pair.Key, pair.Value);
            _executing = true;
            try
            {
                if (!_cargo.TryGetValue(request.UnitId, out var cargo))
                {
                    cargo = new CargoState { OwnerId = unit.OwnerId, Position = unit.Position };
                    _cargo.Add(request.UnitId, cargo);
                }
                cargo.Position = unit.Position;
                foreach (var pair in resources)
                {
                    if (request.Operation == CaravanCargoOperation.CollectLoot)
                    {
                        Add(_loot[unit.Position], pair.Key, -pair.Value);
                        Add(cargo.Resources, pair.Key, pair.Value);
                    }
                    else
                    {
                        float delta = request.Operation == CaravanCargoOperation.Load ? -pair.Value : pair.Value;
                        Add(warehouse, pair.Key, delta);
                        Add(settlement.ResourcePool, pair.Key, delta);
                        Add(cargo.Resources, pair.Key, -delta);
                    }
                }
                if (_loot.TryGetValue(unit.Position, out var remaining) && remaining.Count == 0)
                    _loot.Remove(unit.Position);
                beforeNotify?.Invoke();
                if (settlement != null)
                    foreach (var pair in resources)
                        _signals.Fire(new SettlementResourceChangedSignal
                        {
                            SettlementId = settlement.SettlementId, OwnerId = settlement.OwnerId,
                            ResourceId = pair.Key, NewAmount = settlement.GetResource(pair.Key),
                            Delta = request.Operation == CaravanCargoOperation.Load ? -pair.Value : pair.Value,
                        });
                if (request.Operation == CaravanCargoOperation.Unload && settlement != null
                    && TryWarehousePosition(request.WarehouseKey, out var deliveryPosition))
                    _signals.Fire(new CaravanDeliveryCompletedSignal
                    {
                        OwnerId = request.OwnerId, UnitId = request.UnitId, SettlementId = settlement.SettlementId,
                        WarehousePosition = deliveryPosition,
                        Resources = new System.Collections.ObjectModel.ReadOnlyDictionary<string, float>(resources),
                    });
                Changed?.Invoke();
                return CaravanTransferResult.Success();
            }
            finally { _executing = false; }
        }

        private CaravanTransferResult Validate(CaravanCargoRequest request, out CaravanUnitSnapshot unit,
            out EconomySettlementState settlement, out Dictionary<string, float> warehouse,
            bool requireAuthority = true)
        {
            unit = default;
            settlement = null;
            warehouse = null;
            string reason = null;
            if (requireAuthority && !_gameplay.Value.CanCommand(request.OwnerId, request.UnitId, out reason))
                return CaravanTransferResult.Rejected(reason);
            if (!_gameplay.Value.TryGetWagon(request.UnitId, out unit)
                || !string.Equals(request.OwnerId, unit.OwnerId, StringComparison.Ordinal))
                return CaravanTransferResult.Rejected("Select a wagon belonging to your kingdom.");
            if (request.Operation < CaravanCargoOperation.Load || request.Operation > CaravanCargoOperation.CollectLoot)
                return CaravanTransferResult.Rejected("Unknown cargo operation.");
            if (!TryGetTotal(request.Resources, out double total))
                return CaravanTransferResult.Rejected("Select cargo with finite, positive amounts.");

            _cargo.TryGetValue(request.UnitId, out var cargo);
            if (request.Operation != CaravanCargoOperation.Unload
                && Sum(cargo?.Resources) + total > unit.Capacity)
                return CaravanTransferResult.Rejected("The wagon has insufficient free capacity.");
            if (request.Operation == CaravanCargoOperation.CollectLoot)
            {
                if (!_loot.TryGetValue(unit.Position, out var loot))
                    return CaravanTransferResult.Rejected("There is no cargo on this tile.");
                return ContainsAll(loot, request.Resources) ? CaravanTransferResult.Success()
                    : CaravanTransferResult.Rejected("The selected loot is no longer available.");
            }

            if (string.IsNullOrWhiteSpace(request.SettlementId) || string.IsNullOrWhiteSpace(request.WarehouseKey))
                return CaravanTransferResult.Rejected("Select a warehouse.");
            settlement = _settlements.GetSettlement(request.SettlementId);
            if (settlement == null || !settlement.IsActive || settlement.OwnerId != request.OwnerId)
                return CaravanTransferResult.Rejected("The settlement is unavailable or belongs to another kingdom.");
            if (!settlement.WarehouseResourcePools.TryGetValue(request.WarehouseKey, out warehouse)
                || warehouse == null || !TryWarehousePosition(request.WarehouseKey, out var position))
                return CaravanTransferResult.Rejected("The warehouse no longer exists.");
            if (requireAuthority && !_gameplay.Value.CanAccessWarehouse(request.UnitId, position, out reason))
                return CaravanTransferResult.Rejected(reason);
            if (request.Operation == CaravanCargoOperation.Load)
                return ContainsAll(warehouse, request.Resources) && ContainsAll(settlement.ResourcePool, request.Resources)
                    ? CaravanTransferResult.Success()
                    : CaravanTransferResult.Rejected("This warehouse does not contain the selected cargo.");

            if (!ContainsAll(cargo?.Resources, request.Resources))
                return CaravanTransferResult.Rejected("The wagon does not contain the selected cargo.");
            if (settlement.WarehousePolicies.TryGetValue(request.WarehouseKey, out var policy) && policy != null)
            {
                if (policy.Capacity >= 0 && Sum(warehouse) + total > policy.Capacity)
                    return CaravanTransferResult.Rejected("The warehouse is full. Cargo remains on the wagon.");
                foreach (var pair in request.Resources)
                    if (policy.AcceptedResourceIds != null && policy.AcceptedResourceIds.Length > 0
                        && Array.IndexOf(policy.AcceptedResourceIds, pair.Key) < 0)
                        return CaravanTransferResult.Rejected("This warehouse does not accept the selected resource.");
            }
            if (Sum(warehouse) + total > float.MaxValue || Sum(settlement.ResourcePool) + total > float.MaxValue)
                return CaravanTransferResult.Rejected("The warehouse cannot hold this amount.");
            return CaravanTransferResult.Success();
        }

        private static bool TryWarehousePosition(string key, out Vector2Int position)
        {
            position = default;
            int separator = key.IndexOf(':');
            if (separator < 1 || !int.TryParse(key.Substring(0, separator), out int x)
                || !int.TryParse(key.Substring(separator + 1), out int y)) return false;
            position = new Vector2Int(x, y);
            return true;
        }

        private static bool TryGetTotal(IReadOnlyDictionary<string, float> resources, out double total)
        {
            total = 0;
            if (resources == null || resources.Count == 0 || resources.Count > 256) return false;
            foreach (var pair in resources)
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || float.IsNaN(pair.Value)
                    || float.IsInfinity(pair.Value) || pair.Value <= Epsilon) return false;
                total += pair.Value;
            }
            return total <= float.MaxValue;
        }

        private static double Sum(IReadOnlyDictionary<string, float> resources)
        {
            double total = 0;
            if (resources != null) foreach (var pair in resources) total += pair.Value;
            return total;
        }

        private static bool ContainsAll(IReadOnlyDictionary<string, float> source,
            IReadOnlyDictionary<string, float> requested)
        {
            if (source == null) return false;
            foreach (var pair in requested)
                if (!source.TryGetValue(pair.Key, out float available) || float.IsNaN(available)
                    || float.IsInfinity(available) || available < pair.Value) return false;
            return true;
        }

        private static void Add(Dictionary<string, float> pool, string id, float delta)
        {
            pool.TryGetValue(id, out float current);
            float next = current + delta;
            if (next <= 0f) pool.Remove(id);
            else pool[id] = next;
        }

        private static IReadOnlyDictionary<string, float> Freeze(Dictionary<string, float> resources)
            => new ReadOnlyDictionary<string, float>(new Dictionary<string, float>(resources, StringComparer.Ordinal));
    }
}
