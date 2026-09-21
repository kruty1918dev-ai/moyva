using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Economy.Runtime
{
    /// <summary>
    /// Tracks construction supply orders for pending placements. Deficits are
    /// evaluated against the target settlement's LOCAL pool; deliveries arrive
    /// only through real caravan routes and are reserved per warehouse until
    /// the placement is confirmed or cancelled.
    /// </summary>
    internal sealed partial class ConstructionSupplyService : IConstructionSupplyService,
        IInitializable, IDisposable
    {
        private const float Epsilon = 0.0001f;
        private static readonly IReadOnlyList<ConstructionSupplyResourceLine> NoLines =
            new List<ConstructionSupplyResourceLine>();
        private static readonly IReadOnlyDictionary<string, float> EmptyShipment =
            new ReadOnlyDictionary<string, float>(new Dictionary<string, float>());
        private static readonly IReadOnlyList<ConstructionSupplySourceSnapshot> NoSources =
            new List<ConstructionSupplySourceSnapshot>();
        private static readonly IReadOnlyList<ConstructionSupplyWagonSnapshot> NoWagons =
            new List<ConstructionSupplyWagonSnapshot>();
        private static readonly IReadOnlyDictionary<string, float> EmptyMap =
            new ReadOnlyDictionary<string, float>(new Dictionary<string, float>());
        private static readonly IReadOnlyList<ConstructionSupplyOrderSnapshot> NoOrders =
            new List<ConstructionSupplyOrderSnapshot>();

        private readonly EconomySettlementRegistryService _settlements;
        private readonly EconomyManager _economy;
        private readonly ICaravanService _caravans;
        private readonly SignalBus _signals;
        private readonly LazyInject<ICaravanGameplayAccess> _gameplay;
        private readonly IUnitService _units;
        private readonly LazyInject<ICaravanRemoteCommandRequester> _remote;
        private readonly Dictionary<Vector2Int, SupplyOrder> _ordersByPosition = new();
        private int _orderSequence;
        private bool _disposed;

        private sealed class SupplyOrder
        {
            public string OrderId;
            public string OwnerId;
            public string BuildingId;
            public Vector2Int Position;
            public string SettlementId;
            public string WarehouseKey;
            public ConstructionSupplyOrderStatus Status;
            public readonly Dictionary<string, float> Required = new(StringComparer.Ordinal);
            public readonly Dictionary<string, float> Delivered = new(StringComparer.Ordinal);
            public readonly Dictionary<string, float> Remaining = new(StringComparer.Ordinal);
            public readonly List<string> WagonIds = new();
            public readonly List<SourceReservation> SourceReservations = new();
        }

        /// <summary>Stock held at a source warehouse while a supply route is in flight.
        /// Released when the route leg loads the cargo or when the order closes.</summary>
        private sealed class SourceReservation
        {
            public string SettlementId;
            public string WarehouseKey;
            public string ResourceId;
            public float Amount;
        }

        public ConstructionSupplyService(
            EconomySettlementRegistryService settlements,
            EconomyManager economy,
            ICaravanService caravans,
            SignalBus signals,
            LazyInject<ICaravanGameplayAccess> gameplay,
            [InjectOptional] IUnitService units = null,
            [InjectOptional] LazyInject<ICaravanRemoteCommandRequester> remote = null)
        {
            _settlements = settlements;
            _economy = economy;
            _caravans = caravans;
            _signals = signals;
            _gameplay = gameplay;
            _units = units;
            _remote = remote;
        }

        public event Action Changed;

        public void Initialize()
            => _signals.Subscribe<CaravanDeliveryCompletedSignal>(OnDeliveryCompleted);

        public void Dispose()
        {
            _disposed = true;
            _signals.TryUnsubscribe<CaravanDeliveryCompletedSignal>(OnDeliveryCompleted);
        }

        public ConstructionSupplyEvaluation Evaluate(string ownerId, string buildingId,
            Vector2Int position, IReadOnlyDictionary<string, float> requiredCosts)
        {
            if (!_economy.TryResolveConstructionSettlement(position, ownerId, out var settlement)
                || settlement == null)
            {
                return new ConstructionSupplyEvaluation(false, null, null, position,
                    "No settlement funds construction in this zone.",
                    NoLines, NoSources, NoWagons);
            }

            _ordersByPosition.TryGetValue(position, out var order);
            var resources = new List<ConstructionSupplyResourceLine>();
            if (requiredCosts != null)
            {
                var sorted = new List<KeyValuePair<string, float>>(requiredCosts);
                sorted.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
                foreach (var pair in sorted)
                {
                    if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value <= 0f) continue;
                    float delivered = order != null
                        && order.Delivered.TryGetValue(pair.Key, out var d) ? d : 0f;
                    float localAvailable = Mathf.Max(0f,
                        settlement.GetResource(pair.Key)
                        - settlement.GetTotalReservedResource(pair.Key)
                        + delivered);
                    float deficit = Mathf.Max(0f, pair.Value - localAvailable);
                    resources.Add(new ConstructionSupplyResourceLine(
                        pair.Key, pair.Value, localAvailable, delivered, deficit));
                }
            }

            return new ConstructionSupplyEvaluation(true, settlement.SettlementId,
                _economy.GetSettlementNameOrFallback(settlement.SettlementId), position, null,
                resources, CollectSources(ownerId, settlement, resources, position),
                CollectWagons(ownerId));
        }

        public CaravanTransferResult DispatchSupply(ConstructionSupplyDispatchRequest request,
            IReadOnlyDictionary<string, float> requiredCosts)
        {
            var evaluation = Evaluate(request.OwnerId, request.BuildingId, request.Position, requiredCosts);
            if (!evaluation.Resolved)
                return CaravanTransferResult.Rejected(evaluation.Reason ?? "No target settlement.");
            if (!evaluation.HasDeficit)
                return CaravanTransferResult.Rejected("The settlement already covers this construction.");

            var targetSettlement = _settlements.GetSettlement(evaluation.SettlementId);
            if (targetSettlement == null || !targetSettlement.IsActive)
                return CaravanTransferResult.Rejected("The target settlement is unavailable.");
            string targetWarehouseKey = ResolveTargetWarehouseKey(request.Position, targetSettlement);
            if (string.IsNullOrWhiteSpace(targetWarehouseKey))
                return CaravanTransferResult.Rejected("The target settlement has no warehouse to receive cargo.");

            var sourceSettlement = _settlements.GetSettlement(request.SourceSettlementId);
            if (sourceSettlement == null || !sourceSettlement.IsActive
                || !string.Equals(sourceSettlement.OwnerId, request.OwnerId, StringComparison.Ordinal))
                return CaravanTransferResult.Rejected("The source settlement is unavailable.");
            if (string.Equals(sourceSettlement.SettlementId, targetSettlement.SettlementId, StringComparison.Ordinal))
                return CaravanTransferResult.Rejected("Source and destination settlements must differ.");
            if (!sourceSettlement.WarehouseResourcePools.TryGetValue(request.SourceWarehouseKey, out var sourcePool)
                || sourcePool == null)
                return CaravanTransferResult.Rejected("The source warehouse no longer exists.");

            if (!_gameplay.Value.TryGetWagon(request.UnitId, out var wagon)
                || !string.Equals(wagon.OwnerId, request.OwnerId, StringComparison.Ordinal))
                return CaravanTransferResult.Rejected("Select a wagon belonging to your kingdom.");
            if (_caravans.TryGetRoute(request.OwnerId, request.UnitId, out var activeRoute)
                && activeRoute.Phase != CaravanRoutePhase.Completed)
                return CaravanTransferResult.Rejected("This wagon already runs a route.");

            float cargoUsed = 0f;
            if (_caravans.TryGetCargo(request.OwnerId, request.UnitId, out var cargo))
                foreach (var pair in cargo.Resources) cargoUsed += pair.Value;
            var shipment = BuildShipment(evaluation.Resources, sourcePool, sourceSettlement,
                request.SourceWarehouseKey, Mathf.Max(0f, wagon.Capacity - cargoUsed));
            if (shipment.Count == 0)
                return CaravanTransferResult.Rejected("The source warehouse cannot ship any missing resource.");

            if (!_gameplay.Value.IsAuthoritative)
            {
                // Clients request the dispatch; the host creates the order,
                // reserves the source stock and starts the route authoritatively.
                string remoteReason = null;
                return _remote != null
                    && _remote.Value.TryRequestSupplyDispatch(request, requiredCosts, out remoteReason)
                    ? CaravanTransferResult.Success()
                    : CaravanTransferResult.Rejected(remoteReason
                        ?? "The wagon could not start the supply route.");
            }

            _ordersByPosition.TryGetValue(request.Position, out var prior);
            float outstanding = prior != null && prior.Status == ConstructionSupplyOrderStatus.Active
                ? RemainingTotal(prior)
                : DeficitTotal(evaluation.Resources);
            var route = new CaravanRouteRequest(request.OwnerId, request.UnitId,
                request.SourceSettlementId, request.SourceWarehouseKey,
                evaluation.SettlementId, targetWarehouseKey, shipment,
                repeat: outstanding > Sum(shipment) + Epsilon);
            var routeResult = _caravans.SetRoute(route);
            if (!routeResult.Succeeded) return routeResult;

            var order = GetOrCreateOrder(request, evaluation.SettlementId, targetWarehouseKey,
                requiredCosts, evaluation.Resources);
            ReserveSourceShipment(order, request.SourceSettlementId,
                request.SourceWarehouseKey, shipment);
            if (!order.WagonIds.Contains(request.UnitId)) order.WagonIds.Add(request.UnitId);
            Changed?.Invoke();
            return CaravanTransferResult.Success();
        }

        public IReadOnlyDictionary<string, float> PreviewShipment(
            ConstructionSupplyDispatchRequest request,
            IReadOnlyDictionary<string, float> requiredCosts)
        {
            var evaluation = Evaluate(request.OwnerId, request.BuildingId,
                request.Position, requiredCosts);
            if (!evaluation.Resolved || !evaluation.HasDeficit) return EmptyShipment;

            var sourceSettlement = _settlements.GetSettlement(request.SourceSettlementId);
            if (sourceSettlement == null || !sourceSettlement.IsActive
                || !string.Equals(sourceSettlement.OwnerId, request.OwnerId, StringComparison.Ordinal)
                || string.Equals(sourceSettlement.SettlementId, evaluation.SettlementId,
                    StringComparison.Ordinal))
                return EmptyShipment;
            if (!sourceSettlement.WarehouseResourcePools.TryGetValue(request.SourceWarehouseKey,
                    out var sourcePool) || sourcePool == null)
                return EmptyShipment;
            if (!_gameplay.Value.TryGetWagon(request.UnitId, out var wagon)
                || !string.Equals(wagon.OwnerId, request.OwnerId, StringComparison.Ordinal))
                return EmptyShipment;

            float cargoUsed = 0f;
            if (_caravans.TryGetCargo(request.OwnerId, request.UnitId, out var cargo))
                foreach (var pair in cargo.Resources) cargoUsed += pair.Value;
            return BuildShipment(evaluation.Resources, sourcePool, sourceSettlement,
                request.SourceWarehouseKey, Mathf.Max(0f, wagon.Capacity - cargoUsed));
        }

        public IReadOnlyList<ConstructionSupplyOrderSnapshot> GetOrders(string ownerId)
        {
            if (_ordersByPosition.Count == 0) return NoOrders;
            var result = new List<ConstructionSupplyOrderSnapshot>();
            foreach (var pair in _ordersByPosition)
            {
                var order = pair.Value;
                if (!string.Equals(order.OwnerId, ownerId, StringComparison.Ordinal)) continue;
                result.Add(ToSnapshot(order));
            }
            return result;
        }

        public bool TryGetOrderAt(Vector2Int position, out ConstructionSupplyOrderSnapshot snapshot)
        {
            snapshot = default;
            if (!_ordersByPosition.TryGetValue(position, out var order)) return false;
            snapshot = ToSnapshot(order);
            return true;
        }

        /// <summary>Releases this placement's reservations and closes its order.
        /// Non-authoritative peers also forward the cancel to the host.</summary>
        public void CancelOrderAt(Vector2Int position)
            => CloseOrderAt(position, notifyRemote: true);

        private void CloseOrderAt(Vector2Int position, bool notifyRemote)
        {
            if (!_ordersByPosition.TryGetValue(position, out var order))
                return;
            if (notifyRemote && !_gameplay.Value.IsAuthoritative)
                _remote?.Value.TryRequestCancelSupply(order.OwnerId, position, out _);
            var settlement = _settlements.GetSettlement(order.SettlementId);
            if (settlement != null)
                foreach (var pair in order.Delivered)
                    settlement.ReleaseResourceAt(order.WarehouseKey, pair.Key, pair.Value);
            foreach (var reservation in order.SourceReservations)
                _settlements.GetSettlement(reservation.SettlementId)
                    ?.ReleaseResourceAt(reservation.WarehouseKey, reservation.ResourceId,
                        reservation.Amount);
            order.SourceReservations.Clear();
            var wagonIds = order.WagonIds.ToArray();
            if (_gameplay.Value.IsAuthoritative)
                foreach (var wagonId in wagonIds)
                    _caravans.StopRoute(order.OwnerId, wagonId);
            order.Status = ConstructionSupplyOrderStatus.Cancelled;
            _ordersByPosition.Remove(position);
            Changed?.Invoke();
            _signals.Fire(new ConstructionSupplyOrderClosedSignal
            {
                OwnerId = order.OwnerId,
                Position = position,
                WagonIds = wagonIds,
            });
        }

        /// <summary>Mirrors a host-confirmed dispatch: registers the order and
        /// reserves the same source stock locally. The route itself is applied
        /// separately via <see cref="ICaravanService.ApplyConfirmedSetRoute"/>.</summary>
        public CaravanTransferResult ApplyConfirmedDispatch(ConstructionSupplyDispatchRequest request,
            IReadOnlyDictionary<string, float> requiredCosts)
        {
            var evaluation = Evaluate(request.OwnerId, request.BuildingId, request.Position, requiredCosts);
            if (!evaluation.Resolved)
                return CaravanTransferResult.Rejected(evaluation.Reason ?? "No target settlement.");
            var targetSettlement = _settlements.GetSettlement(evaluation.SettlementId);
            if (targetSettlement == null || !targetSettlement.IsActive)
                return CaravanTransferResult.Rejected("The target settlement is unavailable.");

            string warehouseKey = null;
            IReadOnlyDictionary<string, float> shipment = null;
            if (_caravans.TryGetRoute(request.OwnerId, request.UnitId, out var route)
                && string.Equals(route.Request.TargetSettlementId, evaluation.SettlementId,
                    StringComparison.Ordinal))
            {
                warehouseKey = route.Request.TargetWarehouseKey;
                shipment = route.Request.Resources;
            }
            if (string.IsNullOrWhiteSpace(warehouseKey))
                warehouseKey = ResolveTargetWarehouseKey(request.Position, targetSettlement);
            if (string.IsNullOrWhiteSpace(warehouseKey))
                return CaravanTransferResult.Rejected("The target settlement has no warehouse to receive cargo.");

            var order = GetOrCreateOrder(request, evaluation.SettlementId, warehouseKey,
                requiredCosts, evaluation.Resources);
            if (!order.WagonIds.Contains(request.UnitId)) order.WagonIds.Add(request.UnitId);
            ReserveSourceShipment(order, request.SourceSettlementId,
                request.SourceWarehouseKey, shipment);
            Changed?.Invoke();
            return CaravanTransferResult.Success();
        }

        /// <summary>Mirrors a host-confirmed order close: stops the wagon's route
        /// mirror and releases the order's reservations.</summary>
        public void ApplyConfirmedCancelOrder(string ownerId, string unitId, Vector2Int position)
        {
            if (!string.IsNullOrWhiteSpace(unitId))
                _caravans.ApplyConfirmedStopRoute(ownerId, unitId);
            CloseOrderAt(position, notifyRemote: false);
        }

        /// <summary>Releases source reservations matched by a route Load leg before
        /// cargo validation, so the wagon can pick up the stock its order held.</summary>
        internal void ReleaseRouteLoadReservations(CaravanCargoRequest request)
        {
            if (request.Resources == null || string.IsNullOrWhiteSpace(request.UnitId))
                return;
            var source = _settlements.GetSettlement(request.SettlementId);
            if (source == null) return;
            foreach (var pair in _ordersByPosition)
            {
                var order = pair.Value;
                if (order.Status != ConstructionSupplyOrderStatus.Active
                    || !order.WagonIds.Contains(request.UnitId)
                    || order.SourceReservations.Count == 0)
                    continue;
                for (int index = order.SourceReservations.Count - 1; index >= 0; index--)
                {
                    var reservation = order.SourceReservations[index];
                    if (!string.Equals(reservation.SettlementId, request.SettlementId, StringComparison.Ordinal)
                        || !string.Equals(reservation.WarehouseKey, request.WarehouseKey, StringComparison.Ordinal)
                        || !request.Resources.TryGetValue(reservation.ResourceId, out float amount)
                        || amount <= 0f)
                        continue;
                    float release = Mathf.Min(reservation.Amount, amount);
                    source.ReleaseResourceAt(reservation.WarehouseKey, reservation.ResourceId, release);
                    reservation.Amount -= release;
                    if (reservation.Amount <= Epsilon)
                        order.SourceReservations.RemoveAt(index);
                }
            }
        }

        private void ReserveSourceShipment(SupplyOrder order, string sourceSettlementId,
            string sourceWarehouseKey, IReadOnlyDictionary<string, float> shipment)
        {
            if (shipment == null || shipment.Count == 0) return;
            var source = _settlements.GetSettlement(sourceSettlementId);
            if (source == null) return;
            foreach (var pair in shipment)
            {
                if (pair.Value <= 0f) continue;
                source.ReserveResourceAt(sourceWarehouseKey, pair.Key, pair.Value);
                order.SourceReservations.Add(new SourceReservation
                {
                    SettlementId = sourceSettlementId,
                    WarehouseKey = sourceWarehouseKey,
                    ResourceId = pair.Key,
                    Amount = pair.Value,
                });
            }
        }

        /// <summary>Resources spendable by the placement at this position
        /// (pool − other reservations + this order's delivered stock).</summary>
        public IReadOnlyDictionary<string, float> GetResourcesForPlacement(
            string settlementId, Vector2Int position)
        {
            var settlement = _settlements.GetSettlement(settlementId);
            if (settlement == null) return EmptyMap;
            _ordersByPosition.TryGetValue(position, out var order);
            var result = new Dictionary<string, float>(StringComparer.Ordinal);
            foreach (var pair in settlement.ResourcePool)
            {
                float own = order != null && order.Delivered.TryGetValue(pair.Key, out var d) ? d : 0f;
                float amount = Mathf.Max(0f,
                    pair.Value - settlement.GetTotalReservedResource(pair.Key) + own);
                if (amount > Epsilon) result[pair.Key] = amount;
            }
            return result;
        }

        private SupplyOrder GetOrCreateOrder(ConstructionSupplyDispatchRequest request,
            string settlementId, string warehouseKey,
            IReadOnlyDictionary<string, float> requiredCosts,
            IReadOnlyList<ConstructionSupplyResourceLine> lines)
        {
            if (_ordersByPosition.TryGetValue(request.Position, out var existing)
                && existing.Status == ConstructionSupplyOrderStatus.Active)
                return existing;

            var order = new SupplyOrder
            {
                OrderId = $"supply-{++_orderSequence}",
                OwnerId = request.OwnerId,
                BuildingId = request.BuildingId,
                Position = request.Position,
                SettlementId = settlementId,
                WarehouseKey = warehouseKey,
                Status = ConstructionSupplyOrderStatus.Active,
            };
            if (requiredCosts != null)
                foreach (var pair in requiredCosts)
                    if (pair.Value > 0f) order.Required[pair.Key] = pair.Value;
            if (lines != null)
                foreach (var line in lines)
                    if (line.Deficit > Epsilon) order.Remaining[line.ResourceId] = line.Deficit;
            _ordersByPosition[request.Position] = order;
            return order;
        }

        private void OnDeliveryCompleted(CaravanDeliveryCompletedSignal signal)
        {
            if (_disposed || string.IsNullOrWhiteSpace(signal.SettlementId)) return;
            var settlement = _settlements.GetSettlement(signal.SettlementId);
            if (settlement == null) return;
            string warehouseKey = $"{signal.WarehousePosition.x}:{signal.WarehousePosition.y}";

            foreach (var pair in _ordersByPosition)
            {
                var order = pair.Value;
                if (order.Status != ConstructionSupplyOrderStatus.Active
                    || !string.Equals(order.SettlementId, signal.SettlementId, StringComparison.Ordinal)
                    || !string.Equals(order.WarehouseKey, warehouseKey, StringComparison.Ordinal))
                    continue;
                if (!order.WagonIds.Contains(signal.UnitId))
                    continue;

                foreach (var resource in signal.Resources)
                {
                    if (resource.Value <= 0f) continue;
                    float delivered = resource.Value;
                    settlement.ReserveResourceAt(order.WarehouseKey, resource.Key, delivered);
                    order.Delivered.TryGetValue(resource.Key, out var d);
                    order.Delivered[resource.Key] = d + delivered;
                    if (order.Remaining.TryGetValue(resource.Key, out var r))
                    {
                        float next = r - delivered;
                        if (next > Epsilon) order.Remaining[resource.Key] = next;
                        else order.Remaining.Remove(resource.Key);
                    }
                }

                if (order.Remaining.Count == 0)
                    CompleteOrder(order, settlement);
                Changed?.Invoke();
            }
        }

        private void CompleteOrder(SupplyOrder order, EconomySettlementState settlement)
        {
            order.Status = ConstructionSupplyOrderStatus.Ready;
            if (_gameplay.Value.IsAuthoritative)
                foreach (var wagonId in order.WagonIds)
                    _caravans.StopRoute(order.OwnerId, wagonId);
            _signals.Fire(new ConstructionSupplyReadySignal
            {
                OwnerId = order.OwnerId,
                SettlementId = order.SettlementId,
                SettlementName = _economy.GetSettlementNameOrFallback(order.SettlementId),
                BuildingId = order.BuildingId,
                Position = order.Position,
            });
        }

        private string ResolveTargetWarehouseKey(Vector2Int position, EconomySettlementState settlement)
        {
            if (_ordersByPosition.TryGetValue(position, out var order)
                && !string.IsNullOrWhiteSpace(order.WarehouseKey)
                && settlement.WarehouseResourcePools.ContainsKey(order.WarehouseKey))
                return order.WarehouseKey;

            string best = null;
            float bestFree = -1f;
            foreach (var pair in settlement.WarehouseResourcePools)
            {
                float free = float.MaxValue;
                if (settlement.WarehousePolicies.TryGetValue(pair.Key, out var policy)
                    && policy != null && policy.Capacity >= 0)
                {
                    float stored = 0f;
                    if (pair.Value != null)
                        foreach (var resource in pair.Value) stored += Mathf.Max(0f, resource.Value);
                    free = policy.Capacity - stored;
                    if (free <= Epsilon) continue;
                }
                if (free > bestFree) { bestFree = free; best = pair.Key; }
            }
            return best;
        }

        private List<ConstructionSupplySourceSnapshot> CollectSources(string ownerId,
            EconomySettlementState targetSettlement,
            IReadOnlyList<ConstructionSupplyResourceLine> resources, Vector2Int position)
        {
            var result = new List<ConstructionSupplySourceSnapshot>();
            if (resources == null) return result;

            Vector2Int targetOrigin = position;
            string targetKey = ResolveTargetWarehouseKey(position, targetSettlement);
            if (!string.IsNullOrWhiteSpace(targetKey)
                && TryParseWarehouseKey(targetKey, out var parsedTarget))
                targetOrigin = parsedTarget;

            foreach (var pair in _settlements.AllSettlements)
            {
                var settlement = pair.Value;
                if (settlement == null || !settlement.IsActive
                    || !string.Equals(settlement.OwnerId, ownerId, StringComparison.Ordinal)
                    || string.Equals(settlement.SettlementId, targetSettlement.SettlementId, StringComparison.Ordinal))
                    continue;
                foreach (var warehouse in settlement.WarehouseResourcePools)
                {
                    if (warehouse.Value == null) continue;
                    if (!TryParseWarehouseKey(warehouse.Key, out var sourceOrigin)
                        || !_gameplay.Value.TryMeasureWarehouseRoute(ownerId, sourceOrigin,
                            targetOrigin, out float routeDistance))
                        continue;
                    var stock = new Dictionary<string, float>(StringComparer.Ordinal);
                    foreach (var line in resources)
                    {
                        if (line.Deficit <= Epsilon) continue;
                        float free = (warehouse.Value.TryGetValue(line.ResourceId, out var stored) ? stored : 0f)
                            - settlement.GetReservedResourceAt(warehouse.Key, line.ResourceId);
                        if (free > Epsilon) stock[line.ResourceId] = free;
                    }
                    if (stock.Count == 0) continue;
                    result.Add(new ConstructionSupplySourceSnapshot(
                        settlement.SettlementId,
                        _economy.GetSettlementNameOrFallback(settlement.SettlementId),
                        warehouse.Key,
                        new ReadOnlyDictionary<string, float>(stock),
                        routeDistance));
                }
            }
            result.Sort((a, b) =>
            {
                int cmp = a.RouteDistance.CompareTo(b.RouteDistance);
                if (cmp != 0) return cmp;
                cmp = string.CompareOrdinal(a.SettlementName, b.SettlementName);
                return cmp != 0 ? cmp : string.CompareOrdinal(a.WarehouseKey, b.WarehouseKey);
            });
            return result;
        }

        private static bool TryParseWarehouseKey(string key, out Vector2Int position)
        {
            position = default;
            int separator = key.IndexOf(':');
            if (separator < 1 || !int.TryParse(key.Substring(0, separator), out int x)
                || !int.TryParse(key.Substring(separator + 1), out int y)) return false;
            position = new Vector2Int(x, y);
            return true;
        }

        private List<ConstructionSupplyWagonSnapshot> CollectWagons(string ownerId)
        {
            var result = new List<ConstructionSupplyWagonSnapshot>();
            if (_units == null) return result;
            foreach (var unitId in _units.GetAllUnitIds())
            {
                if (!_gameplay.Value.TryGetWagon(unitId, out var wagon)
                    || !string.Equals(wagon.OwnerId, ownerId, StringComparison.Ordinal))
                    continue;
                float cargoUsed = 0f;
                if (_caravans.TryGetCargo(ownerId, unitId, out var cargo))
                    foreach (var pair in cargo.Resources) cargoUsed += pair.Value;
                bool busy = _caravans.TryGetRoute(ownerId, unitId, out var route)
                    && route.Phase != CaravanRoutePhase.Completed;
                result.Add(new ConstructionSupplyWagonSnapshot(unitId, wagon.Position,
                    wagon.Capacity, cargoUsed, busy,
                    busy ? $"{route.Phase}: {route.Status}" : "Idle"));
            }
            result.Sort((a, b) => string.CompareOrdinal(a.UnitId, b.UnitId));
            return result;
        }

        private static Dictionary<string, float> BuildShipment(
            IReadOnlyList<ConstructionSupplyResourceLine> lines,
            IReadOnlyDictionary<string, float> sourcePool,
            EconomySettlementState sourceSettlement,
            string sourceWarehouseKey,
            float freeCapacity)
        {
            var shipment = new Dictionary<string, float>(StringComparer.Ordinal);
            if (lines == null || freeCapacity <= Epsilon) return shipment;
            float remaining = freeCapacity;
            foreach (var line in lines)
            {
                if (remaining <= Epsilon) break;
                if (line.Deficit <= Epsilon) continue;
                float stored = sourcePool.TryGetValue(line.ResourceId, out var s) ? s : 0f;
                float free = Mathf.Max(0f,
                    stored - sourceSettlement.GetReservedResourceAt(sourceWarehouseKey, line.ResourceId));
                float amount = Mathf.Min(line.Deficit, free, remaining);
                if (amount <= Epsilon) continue;
                shipment[line.ResourceId] = amount;
                remaining -= amount;
            }
            return shipment;
        }

        private static float RemainingTotal(SupplyOrder order)
        {
            float total = 0f;
            foreach (var pair in order.Remaining) total += pair.Value;
            return total;
        }

        private static float DeficitTotal(IReadOnlyList<ConstructionSupplyResourceLine> lines)
        {
            float total = 0f;
            if (lines != null)
                foreach (var line in lines)
                    if (line.Deficit > 0f) total += line.Deficit;
            return total;
        }

        private static float Sum(IReadOnlyDictionary<string, float> map)
        {
            float total = 0f;
            if (map != null) foreach (var pair in map) total += pair.Value;
            return total;
        }

        private ConstructionSupplyOrderSnapshot ToSnapshot(SupplyOrder order)
            => new(order.OrderId, order.OwnerId, order.BuildingId, order.Position,
                order.SettlementId, _economy.GetSettlementNameOrFallback(order.SettlementId), order.Status,
                new ReadOnlyDictionary<string, float>(order.Required),
                new ReadOnlyDictionary<string, float>(order.Delivered),
                new ReadOnlyDictionary<string, float>(order.Remaining),
                new ReadOnlyCollection<string>(order.WagonIds));
    }
}
