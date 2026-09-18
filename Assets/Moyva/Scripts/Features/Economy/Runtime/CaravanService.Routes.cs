using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Signals;
using Zenject;

namespace Kruty1918.Moyva.Economy.Runtime
{
    internal sealed partial class CaravanService : ITickable
    {
        private sealed class RouteState
        {
            public CaravanRouteRequest Request;
            public CaravanRoutePhase Phase;
            public string Status = "Route scheduled.";
            public CancellationTokenSource Cancellation;
        }

        private readonly Dictionary<string, RouteState> _routes = new(StringComparer.Ordinal);
        private readonly List<RouteState> _routeWork = new();
        private bool _routesDirty;
        private bool _disposed;

        private bool HasActiveRoute(string unitId)
            => unitId != null && _routes.TryGetValue(unitId, out var route)
                && route.Phase != CaravanRoutePhase.Completed;

        public bool TryGetRoute(string ownerId, string unitId, out CaravanRouteSnapshot snapshot)
        {
            snapshot = default;
            if (!TryGetCargo(ownerId, unitId, out _) || !_routes.TryGetValue(unitId, out var route)) return false;
            snapshot = new CaravanRouteSnapshot(route.Request, route.Phase, route.Status, route.Cancellation != null);
            return true;
        }

        public CaravanTransferResult CanSetRoute(CaravanRouteRequest request)
        {
            if (_executing || _disposed) return CaravanTransferResult.Rejected("Logistics is busy.");
            if (!_gameplay.Value.CanCommand(request.OwnerId, request.UnitId, out string reason))
                return CaravanTransferResult.Rejected(reason);
            if (HasActiveRoute(request.UnitId)) return CaravanTransferResult.Rejected("Stop the current route first.");
            if (!TryGetCargo(request.OwnerId, request.UnitId, out var cargo))
                return CaravanTransferResult.Rejected("Select your wagon.");
            if (cargo.Resources.Count > 0) return CaravanTransferResult.Rejected("Unload existing cargo before starting a new route.");
            if (!TryGetTotal(request.Resources, out double total) || total > cargo.Unit.Capacity)
                return CaravanTransferResult.Rejected("Select a positive shipment within the wagon capacity.");
            if (!TryRouteWarehouse(request.OwnerId, request.SourceSettlementId, request.SourceWarehouseKey, out _, out reason)
                || !TryRouteWarehouse(request.OwnerId, request.TargetSettlementId, request.TargetWarehouseKey, out _, out reason))
                return CaravanTransferResult.Rejected(reason);
            if (request.SourceWarehouseKey == request.TargetWarehouseKey)
                return CaravanTransferResult.Rejected("Choose different warehouses.");
            return CaravanTransferResult.Success();
        }

        public CaravanTransferResult SetRoute(CaravanRouteRequest request)
        {
            var validation = CanSetRoute(request);
            if (!validation.Succeeded) return validation;
            SetRouteState(request);
            _routesDirty = true;
            Changed?.Invoke();
            return CaravanTransferResult.Success();
        }

        public CaravanTransferResult StopRoute(string ownerId, string unitId)
        {
            if (!_gameplay.Value.IsAuthoritative || !TryGetCargo(ownerId, unitId, out _))
                return CaravanTransferResult.Rejected("Only the authoritative owner can stop this route.");
            RemoveRoute(unitId);
            Changed?.Invoke();
            return CaravanTransferResult.Success();
        }

        private void SetRouteState(CaravanRouteRequest request)
            => _routes[request.UnitId] = new RouteState { Request = CopyRouteRequest(request) };

        private bool RemoveRoute(string unitId)
        {
            if (unitId == null || !_routes.TryGetValue(unitId, out var route)) return false;
            _routes.Remove(unitId);
            route.Cancellation?.Cancel();
            return true;
        }

        private void ClearRoutes()
        {
            var routes = new List<RouteState>(_routes.Values);
            _routes.Clear();
            foreach (var route in routes) route.Cancellation?.Cancel();
        }

        private void RequestRouteTick() => _routesDirty = true;
        private void OnSuppliesChanged(SettlementResourceChangedSignal signal)
        {
            if (!_executing && _routes.Count > 0) RequestRouteTick();
        }

        private void OnManualMoveRequested(MoveUnitRequestSignal signal)
        {
            if (_gameplay.Value.IsAuthoritative && _routes.TryGetValue(signal.UnitId ?? "", out var route)
                && route.Request.OwnerId == signal.RequesterOwnerId && RemoveRoute(signal.UnitId))
                Changed?.Invoke();
        }

        public void Tick()
        {
            if (!_routesDirty || _disposed || !_gameplay.Value.IsAuthoritative) return;
            _routesDirty = false;
            _routeWork.Clear();
            foreach (var route in _routes.Values)
                if (route.Cancellation == null && route.Phase != CaravanRoutePhase.Completed)
                    _routeWork.Add(route);
            foreach (var route in _routeWork) _ = RunRouteAsync(route);
            _routeWork.Clear();
        }

        private async Task RunRouteAsync(RouteState route)
        {
            if (!_routes.TryGetValue(route.Request.UnitId, out var current) || !ReferenceEquals(current, route)) return;
            if (!_gameplay.Value.CanCommand(route.Request.OwnerId, route.Request.UnitId, out string reason))
            { SetRouteStatus(route, reason); return; }
            using var cancellation = new CancellationTokenSource();
            route.Cancellation = cancellation;
            try
            {
                // At most a pickup and a delivery per dispatch. A repeating route resumes on a later event.
                for (int leg = 0; leg < 2; leg++)
                {
                    bool loading = route.Phase == CaravanRoutePhase.ToSource;
                    var request = route.Request;
                    string settlement = loading ? request.SourceSettlementId : request.TargetSettlementId;
                    string warehouse = loading ? request.SourceWarehouseKey : request.TargetWarehouseKey;
                    if (!TryRouteWarehouse(request.OwnerId, settlement, warehouse, out var position, out reason))
                    { SetRouteStatus(route, reason); return; }
                    SetRouteStatus(route, loading ? "Travelling to pickup." : "Delivering cargo.");
                    var movement = await _gameplay.Value.MoveToWarehouseAsync(request.UnitId, position, cancellation.Token);
                    cancellation.Token.ThrowIfCancellationRequested();
                    if (!movement.Succeeded) { SetRouteStatus(route, movement.Reason); return; }
                    var nextPhase = loading ? CaravanRoutePhase.ToDestination
                        : request.Repeat ? CaravanRoutePhase.ToSource : CaravanRoutePhase.Completed;
                    var cargoRequest = new CaravanCargoRequest(request.OwnerId, request.UnitId,
                        loading ? CaravanCargoOperation.Load : CaravanCargoOperation.Unload,
                        settlement, warehouse, request.Resources);
                    var transfer = ExecuteCargo(cargoRequest, true, () => route.Phase = nextPhase);
                    if (!transfer.Succeeded) { SetRouteStatus(route, transfer.Reason); return; }
                    RouteTransferCommitted?.Invoke(new CaravanRouteTransferCommitted(cargoRequest, nextPhase));
                    if (!loading)
                    {
                        SetRouteStatus(route, request.Repeat ? "Delivered. Waiting to repeat." : "Delivery complete.");
                        return;
                    }
                }
            }
            catch (OperationCanceledException) { SetRouteStatus(route, "Route interrupted. Cargo remains on the wagon."); }
            catch (Exception exception)
            { SetRouteStatus(route, $"Delivery stopped: {exception.Message}"); }
            finally
            {
                route.Cancellation = null;
                if (!_disposed) Changed?.Invoke();
            }
        }

        private void SetRouteStatus(RouteState route, string status)
        {
            if (route.Status == status) return;
            route.Status = status;
            if (!_disposed) Changed?.Invoke();
        }

        private bool TryRouteWarehouse(string ownerId, string settlementId, string key,
            out UnityEngine.Vector2Int position, out string reason)
        {
            position = default;
            reason = "The route warehouse is unavailable or belongs to another kingdom.";
            if (string.IsNullOrWhiteSpace(settlementId) || string.IsNullOrWhiteSpace(key)) return false;
            var settlement = _settlements.GetSettlement(settlementId);
            return settlement != null && settlement.IsActive && settlement.OwnerId == ownerId
                && settlement.WarehouseResourcePools.ContainsKey(key) && TryWarehousePosition(key, out position);
        }

        private static CaravanRouteRequest CopyRouteRequest(CaravanRouteRequest request)
        {
            var resources = new Dictionary<string, float>(StringComparer.Ordinal);
            foreach (var pair in request.Resources) resources.Add(pair.Key, pair.Value);
            return new CaravanRouteRequest(request.OwnerId, request.UnitId, request.SourceSettlementId,
                request.SourceWarehouseKey, request.TargetSettlementId, request.TargetWarehouseKey, Freeze(resources), request.Repeat);
        }
    }
}
