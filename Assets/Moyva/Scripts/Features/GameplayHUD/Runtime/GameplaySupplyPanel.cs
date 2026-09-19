using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.UIActions.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class GameplaySupplyResourceSnapshot
    {
        public string ResourceId = string.Empty;
        public string DisplayName = string.Empty;
        public float Required;
        public float LocalAvailable;
        public float Delivered;
        public float Deficit;
    }

    internal sealed class GameplaySupplySourceSnapshot
    {
        public string SettlementId = string.Empty;
        public string SettlementName = string.Empty;
        public string WarehouseKey = string.Empty;
        public string StockSummary = string.Empty;
    }

    internal sealed class GameplaySupplyWagonSnapshot
    {
        public string UnitId = string.Empty;
        public float FreeCapacity;
        public bool Busy;
        public string Status = string.Empty;
    }

    internal sealed class GameplaySupplySnapshot
    {
        public bool Resolved;
        public string Reason = string.Empty;
        public string BuildingId = string.Empty;
        public string BuildingName = string.Empty;
        public Vector2Int Position;
        public string SettlementName = string.Empty;
        public GameplaySupplyResourceSnapshot[] Resources = Array.Empty<GameplaySupplyResourceSnapshot>();
        public GameplaySupplySourceSnapshot[] Sources = Array.Empty<GameplaySupplySourceSnapshot>();
        public GameplaySupplyWagonSnapshot[] Wagons = Array.Empty<GameplaySupplyWagonSnapshot>();
        public int SourceIndex = -1;
        public int WagonIndex = -1;
        public bool CanDispatch;
        public string DispatchUnavailableReason = string.Empty;
        public ConstructionSupplyOrderSnapshot? Order;
    }

    /// <summary>
    /// Construction supply dialog: evaluates a pending placement's local
    /// deficit, lists real source warehouses and wagons, and dispatches actual
    /// caravan routes. Resources move physically — never instantly.
    /// </summary>
    internal sealed class GameplaySupplyPanel : IUiActionHandler, IInitializable, IDisposable
    {
        private static readonly UiActionId[] Actions = { UiActionIds.Logistics.Supply };
        private readonly IConstructionSessionCommands _construction;
        private readonly IConstructionSupplyService _supply;
        private readonly ILocalGameplayRoleResolver _roles;
        private readonly ITurnService _turns;
        private readonly GameplayHtmlState _state;
        private string _sourceSettlementId;
        private string _sourceWarehouseKey;
        private string _wagonId;
        private GameplaySupplySnapshot _lastSnapshot;

        public GameplaySupplyPanel(IConstructionSessionCommands construction,
            IConstructionSupplyService supply, ILocalGameplayRoleResolver roles,
            ITurnService turns, GameplayHtmlState state)
        {
            _construction = construction;
            _supply = supply;
            _roles = roles;
            _turns = turns;
            _state = state;
        }

        private string LocalOwner => string.IsNullOrWhiteSpace(_turns.LocalOwnerId)
            ? _roles.Resolve().PlayerId : _turns.LocalOwnerId;

        public IReadOnlyCollection<UiActionId> ActionIds => Actions;

        public void Initialize() => _supply.Changed += _state.MarkDirty;

        public void Dispose() => _supply.Changed -= _state.MarkDirty;

        public GameplaySupplySnapshot Capture()
        {
            if (_state.OpenPanelId != GameplayHtmlPanel.Supply || !_state.SupplyPosition.HasValue)
                return null;

            Vector2Int position = _state.SupplyPosition.Value;
            string ownerId = LocalOwner;
            var snapshot = new GameplaySupplySnapshot { Position = position };

            if (!_construction.TryGetPendingPlacementStatus(position, out var status))
            {
                snapshot.Reason = "There is no pending placement at this position.";
                return snapshot;
            }

            snapshot.BuildingId = status.BuildingId;
            snapshot.BuildingName = status.BuildingId;
            var required = _construction.GetBuildingResourceCosts(status.BuildingId);
            var evaluation = _supply.Evaluate(ownerId, status.BuildingId, position, required);
            snapshot.Resolved = evaluation.Resolved;
            snapshot.Reason = evaluation.Reason ?? string.Empty;
            snapshot.SettlementName = evaluation.SettlementName ?? string.Empty;
            snapshot.Order = _supply.TryGetOrderAt(position, out var order) ? order : null;

            var resources = new List<GameplaySupplyResourceSnapshot>();
            if (evaluation.Resources != null)
                foreach (var line in evaluation.Resources)
                    resources.Add(new GameplaySupplyResourceSnapshot
                    {
                        ResourceId = line.ResourceId,
                        DisplayName = line.ResourceId,
                        Required = line.Required,
                        LocalAvailable = line.LocalAvailable,
                        Delivered = line.Delivered,
                        Deficit = line.Deficit,
                    });
            snapshot.Resources = resources.ToArray();

            var sources = new List<GameplaySupplySourceSnapshot>();
            if (evaluation.Sources != null)
                foreach (var source in evaluation.Sources)
                {
                    var parts = new List<string>();
                    foreach (var pair in source.Available)
                        parts.Add($"{pair.Key} {pair.Value:0.#}");
                    sources.Add(new GameplaySupplySourceSnapshot
                    {
                        SettlementId = source.SettlementId,
                        SettlementName = source.SettlementName,
                        WarehouseKey = source.WarehouseKey,
                        StockSummary = string.Join(", ", parts),
                    });
                }
            snapshot.Sources = sources.ToArray();

            var wagons = new List<GameplaySupplyWagonSnapshot>();
            if (evaluation.Wagons != null)
                foreach (var wagon in evaluation.Wagons)
                    wagons.Add(new GameplaySupplyWagonSnapshot
                    {
                        UnitId = wagon.UnitId,
                        FreeCapacity = wagon.FreeCapacity,
                        Busy = wagon.Busy,
                        Status = wagon.Status,
                    });
            snapshot.Wagons = wagons.ToArray();

            if (_sourceWarehouseKey == null && sources.Count > 0)
            {
                _sourceSettlementId = sources[0].SettlementId;
                _sourceWarehouseKey = sources[0].WarehouseKey;
            }
            if (_wagonId == null)
            {
                var free = wagons.Find(w => !w.Busy && w.FreeCapacity > 0f);
                if (free != null) _wagonId = free.UnitId;
            }
            snapshot.SourceIndex = sources.FindIndex(s =>
                s.SettlementId == _sourceSettlementId && s.WarehouseKey == _sourceWarehouseKey);
            snapshot.WagonIndex = wagons.FindIndex(w => w.UnitId == _wagonId);

            bool hasDeficit = false;
            foreach (var line in snapshot.Resources)
                if (line.Deficit > 0.0001f) hasDeficit = true;
            if (!evaluation.Resolved)
                snapshot.DispatchUnavailableReason = evaluation.Reason ?? "No target settlement.";
            else if (!hasDeficit)
                snapshot.DispatchUnavailableReason = "The settlement already covers this construction.";
            else if (snapshot.SourceIndex < 0)
                snapshot.DispatchUnavailableReason = "Select a source warehouse with the missing resources.";
            else if (snapshot.WagonIndex < 0)
                snapshot.DispatchUnavailableReason = "Select a free wagon.";
            else if (wagons[snapshot.WagonIndex].Busy)
                snapshot.DispatchUnavailableReason = "This wagon already runs a route.";
            else if (wagons[snapshot.WagonIndex].FreeCapacity <= 0f)
                snapshot.DispatchUnavailableReason = "This wagon has no free capacity.";
            snapshot.CanDispatch = string.IsNullOrWhiteSpace(snapshot.DispatchUnavailableReason);
            _lastSnapshot = snapshot;
            return snapshot;
        }

        public UiActionResult Execute(in UiActionRequest request)
        {
            if (request.ActionId != UiActionIds.Logistics.Supply)
                return UiActionResult.Rejected(UiActionReason.ActionUnavailable, "Unknown supply action.");
            if (!_state.SupplyPosition.HasValue
                || !_construction.TryGetPendingPlacementStatus(_state.SupplyPosition.Value, out var status))
                return UiActionResult.Rejected(UiActionReason.ActionUnavailable,
                    "There is no pending placement at this position.");

            var required = _construction.GetBuildingResourceCosts(status.BuildingId);
            var result = _supply.DispatchSupply(new ConstructionSupplyDispatchRequest(
                LocalOwner, status.BuildingId, _state.SupplyPosition.Value,
                _sourceSettlementId, _sourceWarehouseKey, _wagonId), required);
            _state.SetFeedback(result.Succeeded
                ? "Supply route dispatched. Construction unlocks when the cargo arrives."
                : result.Reason);
            if (result.Succeeded) _state.MarkDirty();
            return result.Succeeded ? UiActionResult.Performed()
                : UiActionResult.Rejected(UiActionReason.ActionUnavailable, result.Reason);
        }

        public void SetSource(int index)
        {
            if (_lastSnapshot == null || index < 0 || index >= _lastSnapshot.Sources.Length) return;
            var source = _lastSnapshot.Sources[index];
            _sourceSettlementId = source.SettlementId;
            _sourceWarehouseKey = source.WarehouseKey;
            _state.MarkDirty();
        }

        public void SetWagon(int index)
        {
            if (_lastSnapshot == null || index < 0 || index >= _lastSnapshot.Wagons.Length) return;
            _wagonId = _lastSnapshot.Wagons[index].UnitId;
            _state.MarkDirty();
        }
    }
}
