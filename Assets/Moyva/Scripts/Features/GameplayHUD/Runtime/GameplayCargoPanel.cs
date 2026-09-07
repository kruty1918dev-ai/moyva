using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Notifications.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.UIActions.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class GameplayCargoSnapshot
    {
        public CaravanCargoSnapshot Cargo;
        public EconomyWarehouseSnapshot[] Warehouses = Array.Empty<EconomyWarehouseSnapshot>();
        public KeyValuePair<string, float>[] Resources = Array.Empty<KeyValuePair<string, float>>();
        public CaravanCargoOperation Operation;
        public int WarehouseIndex = -1;
        public int ResourceIndex = -1;
        public string Amount;
        public CaravanTransferResult Availability;
        public CaravanTransferResult RouteAvailability;
        public CaravanTransferResult FoundSettlementAvailability;
        public string FoundSettlementPosition;
        public CaravanRouteSnapshot? Route;
        public int TargetIndex = -1;
        public bool Repeat;

        public static GameplayCargoSnapshot CreatePreview()
        {
            var resources = new Dictionary<string, float> { ["walnut-wood-materials-resources"] = 40f };
            return new GameplayCargoSnapshot
            {
                Cargo = new CaravanCargoSnapshot(new CaravanUnitSnapshot("player_0", new Vector2Int(41, 27), 120),
                    new Dictionary<string, float> { ["stone-materials-resources"] = 20 }),
                Warehouses = new[]
                {
                    new EconomyWarehouseSnapshot("42:27", "storage", "northhold", "Northhold",
                        new Vector2Int(42, 27), 40, 250, resources),
                    new EconomyWarehouseSnapshot("67:31", "warehouse", "rivergate", "Rivergate",
                        new Vector2Int(67, 31), 15, 200,
                        new Dictionary<string, float> { ["walnut-wood-materials-resources"] = 10f }),
                },
                Resources = resources.ToArray(), Amount = "10",
                WarehouseIndex = 0, ResourceIndex = 0, TargetIndex = 1,
                Availability = CaravanTransferResult.Success(),
                RouteAvailability = CaravanTransferResult.Success(),
                FoundSettlementAvailability = CaravanTransferResult.Success(),
                FoundSettlementPosition = "42, 28",
            };
        }
    }

    internal sealed class GameplayCargoPanel : IUiActionHandler, IInitializable, IDisposable
    {
        private static readonly UiActionId[] Actions =
        {
            UiActionIds.Logistics.Transfer,
            UiActionIds.Logistics.StartRoute,
            UiActionIds.Logistics.StopRoute,
            UiActionIds.Logistics.FoundSettlement,
        };
        private readonly ICaravanService _caravans;
        private readonly IEconomyRuntimeApi _economy;
        private readonly ILocalGameplayRoleResolver _roles;
        private readonly ICaravanRemoteCommandRequester _remoteCommands;
        private readonly GameplayHtmlState _state;
        private readonly SignalBus _signals;
        private readonly ITurnService _turns;
        private string _unitId;
        private string _settlementId;
        private string _warehouseKey;
        private string _resourceId;
        private string _targetSettlement;
        private string _targetWarehouse;
        private bool _repeat;
        private string _amount = "10";
        private CaravanCargoOperation _operation;
        private GameplayCargoSnapshot _lastSnapshot;

        public GameplayCargoPanel(ICaravanService caravans, IEconomyRuntimeApi economy,
            ILocalGameplayRoleResolver roles, GameplayHtmlState state, SignalBus signals, ITurnService turns,
            [InjectOptional] ICaravanRemoteCommandRequester remoteCommands = null)
        {
            _caravans = caravans;
            _economy = economy;
            _roles = roles;
            _state = state;
            _signals = signals;
            _turns = turns;
            _remoteCommands = remoteCommands;
        }

        private string LocalOwner => string.IsNullOrWhiteSpace(_turns.LocalOwnerId)
            ? _roles.Resolve().PlayerId : _turns.LocalOwnerId;

        public IReadOnlyCollection<UiActionId> ActionIds => Actions;

        public void Initialize()
        {
            _caravans.Changed += _state.MarkDirty;
            if (_remoteCommands != null)
                _remoteCommands.CommandRejected += OnRemoteCommandRejected;
            _signals.Subscribe<WorldInfoSelectionChangedSignal>(OnSelectionChanged);
            _signals.Subscribe<UnitMovedSignal>(OnMoved);
        }

        public void Dispose()
        {
            _caravans.Changed -= _state.MarkDirty;
            if (_remoteCommands != null)
                _remoteCommands.CommandRejected -= OnRemoteCommandRejected;
            _signals.TryUnsubscribe<WorldInfoSelectionChangedSignal>(OnSelectionChanged);
            _signals.TryUnsubscribe<UnitMovedSignal>(OnMoved);
        }

        private void OnRemoteCommandRejected(CaravanRemoteCommandResult result)
        {
            if (!string.IsNullOrWhiteSpace(_unitId)
                && !string.Equals(_unitId, result.UnitId, StringComparison.Ordinal))
            {
                return;
            }

            _state.AddNotification(
                string.IsNullOrWhiteSpace(result.Reason)
                    ? "Logistics command was rejected by host."
                    : result.Reason,
                GameplayNotificationKind.Error.ToString());
        }

        private void OnSelectionChanged(WorldInfoSelectionChangedSignal signal)
        {
            string next = signal.Kind == WorldInfoSelectionKind.Unit ? signal.ObjectId : null;
            if (string.Equals(next, _unitId, StringComparison.Ordinal)) return;
            _unitId = next;
            _settlementId = _warehouseKey = _resourceId = null;
            _targetSettlement = _targetWarehouse = null;
            _lastSnapshot = null;
            _state.MarkDirty();
        }

        private void OnMoved(UnitMovedSignal signal)
        {
            if (signal.UnitId == _unitId) _state.MarkDirty();
        }

        public GameplayCargoSnapshot Capture(string selectedUnitId)
        {
            string ownerId = LocalOwner;
            if (selectedUnitId != _unitId
                || !_caravans.TryGetCargo(ownerId, _unitId, out var cargo)) return null;
            var snapshot = new GameplayCargoSnapshot
            {
                Cargo = cargo, Operation = _operation, Amount = _amount,
                Warehouses = _economy.GetOwnerWarehouseSnapshots(ownerId).ToArray(),
            };
            // Retain identities, not list indices: a destroyed warehouse must never redirect a command.
            if (_warehouseKey == null && snapshot.Warehouses.Length > 0)
            {
                var nearest = snapshot.Warehouses.OrderBy(w =>
                    ((Vector2)w.GridPosition - (Vector2)cargo.Unit.Position).sqrMagnitude).First();
                _settlementId = nearest.SettlementId;
                _warehouseKey = nearest.WarehouseKey;
            }
            snapshot.WarehouseIndex = Array.FindIndex(snapshot.Warehouses,
                w => w.SettlementId == _settlementId && w.WarehouseKey == _warehouseKey);
            if (_targetWarehouse == null && snapshot.Warehouses.Length > 1)
            {
                var target = snapshot.Warehouses.FirstOrDefault(w =>
                    w.SettlementId != _settlementId || w.WarehouseKey != _warehouseKey);
                if (!string.IsNullOrWhiteSpace(target.WarehouseKey))
                {
                    _targetSettlement = target.SettlementId;
                    _targetWarehouse = target.WarehouseKey;
                }
            }
            snapshot.TargetIndex = Array.FindIndex(snapshot.Warehouses,
                w => w.SettlementId == _targetSettlement && w.WarehouseKey == _targetWarehouse);
            snapshot.Repeat = _repeat;
            var operation = _state.SelectionTab == GameplaySelectionTab.Route ? CaravanCargoOperation.Load : _operation;
            IReadOnlyDictionary<string, float> resources = operation switch
            {
                CaravanCargoOperation.Unload => cargo.Resources,
                CaravanCargoOperation.CollectLoot => _caravans.GetLootAtWagon(ownerId, _unitId),
                _ => snapshot.WarehouseIndex >= 0 ? snapshot.Warehouses[snapshot.WarehouseIndex].Resources : null,
            };
            snapshot.Resources = resources?.Where(r => r.Value > 0).OrderBy(r => r.Key, StringComparer.Ordinal).ToArray()
                ?? Array.Empty<KeyValuePair<string, float>>();
            if (_resourceId == null && snapshot.Resources.Length > 0) _resourceId = snapshot.Resources[0].Key;
            snapshot.ResourceIndex = Array.FindIndex(snapshot.Resources, r => r.Key == _resourceId);
            snapshot.Availability = _caravans.CanExecute(BuildRequest(ownerId));
            snapshot.RouteAvailability = _caravans.CanSetRoute(BuildRouteRequest(ownerId));
            snapshot.FoundSettlementAvailability =
                _caravans.CanFoundSettlement(ownerId, _unitId, "townhall", out var foundingPosition);
            snapshot.FoundSettlementPosition = snapshot.FoundSettlementAvailability.Succeeded
                ? $"{foundingPosition.x}, {foundingPosition.y}"
                : string.Empty;
            if (_caravans.TryGetRoute(ownerId, _unitId, out var route)) snapshot.Route = route;
            _lastSnapshot = snapshot;
            return snapshot;
        }

        private CaravanCargoRequest BuildRequest(string ownerId)
        {
            var resources = new Dictionary<string, float>(StringComparer.Ordinal);
            if (!string.IsNullOrEmpty(_resourceId)
                && float.TryParse(_amount, NumberStyles.Float, CultureInfo.InvariantCulture, out float amount))
                resources[_resourceId] = amount;
            return new CaravanCargoRequest(ownerId, _unitId, _operation, _settlementId, _warehouseKey, resources);
        }

        public UiActionResult Execute(in UiActionRequest request)
        {
            if (request.ActionId == UiActionIds.Logistics.StartRoute || request.ActionId == UiActionIds.Logistics.StopRoute)
            {
                if (TryRequestRemote(request.ActionId, out var remoteResult))
                    return remoteResult;
                var action = request.ActionId == UiActionIds.Logistics.StartRoute
                    ? _caravans.SetRoute(BuildRouteRequest(LocalOwner)) : _caravans.StopRoute(LocalOwner, _unitId);
                _state.SetFeedback(action.Succeeded ? request.ActionId == UiActionIds.Logistics.StartRoute
                    ? "Delivery route scheduled." : "Route stopped. Cargo remains on the wagon." : action.Reason);
                return action.Succeeded ? UiActionResult.Performed()
                    : UiActionResult.Rejected(UiActionReason.ActionUnavailable, action.Reason);
            }
            if (request.ActionId == UiActionIds.Logistics.FoundSettlement)
            {
                if (TryRequestRemote(request.ActionId, out var remoteResult))
                    return remoteResult;
                var action = _caravans.FoundSettlement(LocalOwner, _unitId, "townhall");
                _state.SetFeedback(action.Succeeded
                    ? "Settlement founded. Remaining cargo moved to the Town Hall stockpile."
                    : action.Reason);
                return action.Succeeded ? UiActionResult.Performed()
                    : UiActionResult.Rejected(UiActionReason.ActionUnavailable, action.Reason);
            }
            if (request.ActionId != UiActionIds.Logistics.Transfer)
                return UiActionResult.Rejected(UiActionReason.ActionUnavailable, "Unknown cargo action.");
            if (TryRequestRemote(request.ActionId, out var transferRemoteResult))
                return transferRemoteResult;
            var result = _caravans.Execute(BuildRequest(LocalOwner));
            _state.SetFeedback(result.Succeeded ? _operation switch
            {
                CaravanCargoOperation.Load => "Cargo loaded onto the wagon.",
                CaravanCargoOperation.Unload => "Cargo delivered to the warehouse.",
                _ => "Cargo recovered from the ground.",
            } : result.Reason);
            return result.Succeeded ? UiActionResult.Performed()
                : UiActionResult.Rejected(UiActionReason.ActionUnavailable, result.Reason);
        }

        private bool TryRequestRemote(UiActionId actionId, out UiActionResult result)
        {
            result = default;
            var role = _roles?.Resolve() ?? new LocalGameplayRoleSnapshot(LocalGameplayRole.Offline, LocalOwner);
            if (role.Role != LocalGameplayRole.Client)
                return false;
            if (_remoteCommands == null)
            {
                result = UiActionResult.Rejected(UiActionReason.ActionUnavailable,
                    "Network logistics endpoint is not connected.");
                return true;
            }

            bool sent;
            string reason;
            if (actionId == UiActionIds.Logistics.StartRoute)
                sent = _remoteCommands.TryRequestSetRoute(BuildRouteRequest(LocalOwner), out reason);
            else if (actionId == UiActionIds.Logistics.StopRoute)
                sent = _remoteCommands.TryRequestStopRoute(LocalOwner, _unitId, out reason);
            else if (actionId == UiActionIds.Logistics.FoundSettlement)
                sent = _remoteCommands.TryRequestFoundSettlement(LocalOwner, _unitId, "townhall", out reason);
            else
                sent = _remoteCommands.TryRequestExecute(BuildRequest(LocalOwner), out reason);

            _state.SetFeedback(sent ? "Logistics request sent. Waiting for the host." : reason);
            result = sent ? UiActionResult.Performed()
                : UiActionResult.Rejected(UiActionReason.ActionUnavailable, reason);
            return true;
        }

        private CaravanRouteRequest BuildRouteRequest(string ownerId)
            => new(ownerId, _unitId, _settlementId, _warehouseKey, _targetSettlement, _targetWarehouse,
                BuildRequest(ownerId).Resources, _repeat);

        public void SetTargetWarehouse(int index)
        {
            if (_lastSnapshot == null || index < -1 || index >= _lastSnapshot.Warehouses.Length) return;
            _targetSettlement = index < 0 ? "" : _lastSnapshot.Warehouses[index].SettlementId;
            _targetWarehouse = index < 0 ? "" : _lastSnapshot.Warehouses[index].WarehouseKey;
            _state.MarkDirty();
        }

        public void SetRepeat(bool repeat) { _repeat = repeat; _state.MarkDirty(); }

        public void SetOperation(int operation)
        {
            if (operation < 0 || operation > 2 || (int)_operation == operation) return;
            _operation = (CaravanCargoOperation)operation;
            _resourceId = null;
            _state.MarkDirty();
        }

        public void SetWarehouse(int index)
        {
            if (_lastSnapshot == null || index < -1 || index >= _lastSnapshot.Warehouses.Length) return;
            if (index == -1)
            {
                _settlementId = _warehouseKey = _resourceId = string.Empty;
                _state.MarkDirty();
                return;
            }
            var warehouse = _lastSnapshot.Warehouses[index];
            _settlementId = warehouse.SettlementId;
            _warehouseKey = warehouse.WarehouseKey;
            _resourceId = null;
            _state.MarkDirty();
        }

        public void SetResource(int index)
        {
            if (_lastSnapshot == null || index < -1 || index >= _lastSnapshot.Resources.Length) return;
            _resourceId = index == -1 ? string.Empty : _lastSnapshot.Resources[index].Key;
            _state.MarkDirty();
        }

        public void SetAmount(object value)
        {
            string amount = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            if (_amount == amount) return;
            _amount = amount;
            _state.MarkDirty();
        }
    }
}
