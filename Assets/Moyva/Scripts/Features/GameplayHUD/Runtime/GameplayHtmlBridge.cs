using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Jsonization;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Notifications.API;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.UIActions.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using UnityHTML.Runtime;
using Zenject;
using System.Text;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class GameplayHtmlBridge
    {
        private readonly GameplayHtmlState _state;
        private readonly GameplayHudReadModel _readModel;
        private readonly LazyInject<IUiActionRouter> _actions;
        private readonly IConstructionSessionCommands _construction;
        private readonly ILocalGameplayRoleResolver _roles;
        private readonly IGameplayCameraFocusService _cameraFocus;
        private readonly IExitMatchCoordinator _exit;
        private readonly IGameplayProgressClock _progressClock;
        private readonly GameplayCargoPanel _cargo;
        private readonly SignalBus _notificationSignals;

        public GameplayHtmlBridge(
            GameplayHtmlState state,
            GameplayHudReadModel readModel,
            LazyInject<IUiActionRouter> actions,
            IConstructionSessionCommands construction,
            ILocalGameplayRoleResolver roles,
            IGameplayCameraFocusService cameraFocus,
            IExitMatchCoordinator exit,
            IGameplayProgressClock progressClock = null,
            GameplayCargoPanel cargo = null,
            SignalBus notificationSignals = null)
        {
            _state = state;
            _readModel = readModel;
            _actions = actions;
            _construction = construction;
            _roles = roles;
            _cameraFocus = cameraFocus;
            _exit = exit;
            _progressClock = progressClock;
            _cargo = cargo;
            _notificationSignals = notificationSignals;
        }
        public void Kingdom() => OpenOverlayPanel(GameplayHtmlPanel.Kingdom);
        public void Construction()
        {
            UiActionResult result = Execute(UiActionIds.Construction.Toggle, "GameplayHTML");
            if (result.Status != UiActionStatus.Performed)
                SetResult(result, "Construction is unavailable.");
        }

        public void ClosePanel()
        {
            if (_state.OpenPanelId == GameplayHtmlPanel.Construction)
            {
                UiActionResult closeResult = Execute(UiActionIds.Construction.Close, "GameplayHTML");
                if (closeResult.Status == UiActionStatus.Rejected)
                {
                    SetResult(closeResult, string.Empty);
                    return;
                }
            }
            _state.ClosePanel();
        }

        public void SelectBuilding(object value)
        {
            string buildingId = value?.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(buildingId))
                return;
            UiActionResult openResult = Execute(UiActionIds.Construction.Open, "GameplayHTML");
            if (openResult.Status == UiActionStatus.Rejected)
            {
                SetResult(openResult, "Construction is unavailable.");
                return;
            }

            UiActionResult result = _actions.Value.Execute(
                UiActionIds.Construction.SelectBuilding,
                UiActionSource.Button,
                "GameplayHTML",
                buildingId);
            if (result.Status == UiActionStatus.Performed)
            {
                string displayName = _readModel.ResolveBuildingDisplayName(buildingId);
                _state.SetFeedback($"Selected {displayName}. Choose a tile on the map.");
                return;
            }

            SetResult(result, "Building selection was rejected.");
        }

        public void ConfirmPlacement()
        {
            int before = _construction?.GetPendingPlacements()?.Count ?? 0;
            UiActionResult action = Execute(UiActionIds.Construction.ConfirmPlacement, "GameplayHTML/Placement");
            int after = _construction?.GetPendingPlacements()?.Count ?? 0;
            int confirmed = Math.Max(0, before - after);

            if (confirmed > 0)
            {
                _state.SetFeedback($"Placed {confirmed} building(s).");
                return;
            }

            if (action.Status == UiActionStatus.Performed
                && _roles?.Resolve().Role == LocalGameplayRole.Client)
            {
                _state.SetFeedback("Placement request sent. Waiting for the host.");
                return;
            }

            string reason = action.Details;
            if (string.IsNullOrWhiteSpace(reason))
                reason = _construction?.GetLastActionMessage();
            if (string.IsNullOrWhiteSpace(reason))
                reason = before == 0
                    ? "Choose a location on the map before confirming."
                    : "The placement was rejected by gameplay rules.";
            _state.SetFeedback(reason);
        }

        public void CancelPlacement()
        {
            if (IsInitialCastleRequired())
            {
                _state.SetFeedback("Place your first castle before leaving construction mode.");
                return;
            }

            SetResult(
                Execute(UiActionIds.Construction.CancelPlacement, "GameplayHTML/Placement"),
                "Placement cancelled.");
        }
        public void RotatePlacement() => SetResult(
            Execute(UiActionIds.Construction.RotatePlacement, "GameplayHTML/Placement"),
            "Placement rotated.");
        public void UndoPlacement() => SetResult(
            Execute(UiActionIds.Construction.UndoPlacement, "GameplayHTML/Placement"),
            "Placement undone.");
        public void RedoPlacement() => SetResult(
            Execute(UiActionIds.Construction.RedoPlacement, "GameplayHTML/Placement"),
            "Placement restored.");
        public void ClearSelection() => SetResult(
            Execute(UiActionIds.ClearSelection, "GameplayHTML"),
            "Selection cleared.");
        public void EndTurn() => SetResult(
            Execute(UiActionIds.EndTurn, "GameplayHTML"),
            "Turn ended.");
        public void SandboxSpeed(object value)
        {
            if (_progressClock == null || !_progressClock.IsRealtime)
                return;

            float speed = ToFloat(value);
            _progressClock.SetSpeed(speed);
            _state.MarkDirty();
        }
        public void Pause() => Execute(UiActionIds.Pause.Open, "GameplayHTML");
        public void Resume() => Execute(UiActionIds.Pause.Close, "GameplayHTML");
        public void ClearNotifications() => _state.ClearNotifications();
        public void RemoveNotification(object value)
        {
            if (long.TryParse(value?.ToString(), out long id)) _state.RemoveNotification(id);
        }
        public void OpenNotification(object value)
        {
            if (!long.TryParse(value?.ToString(), out long id)) return;
            foreach (var item in _state.Notifications)
            {
                if (item.Id != id || !item.Position.HasValue) continue;
                _cameraFocus?.FocusGridPosition(item.Position.Value, item.TargetId);
                _state.ClosePanel();
                if (_readModel.TryResolveNotificationBuilding(item.Position.Value, out string buildingId))
                {
                    _notificationSignals?.Fire(new BuildingInfoPanelRequestedSignal
                    { BuildingId = buildingId, Position = item.Position.Value });
                    if (item.QueueId > 0) _state.SetSelectionTab(GameplaySelectionTab.Queue);
                }
                else _state.SetFeedback("The source no longer exists. Showing its last known location.");
                if (!string.IsNullOrWhiteSpace(item.DeliveryDetails)) _state.SetFeedback(item.DeliveryDetails);
                return;
            }
        }
        public void Notifications() => OpenOverlayPanel(GameplayHtmlPanel.Notifications);
        public void SetConstructionCategory(object value)
            => _state.SetConstructionCategory(value?.ToString());
        public void SetConstructionSearch(string value)
            => _state.SetConstructionSearch(value);
        public void PreviousConstructionPage() => _state.MoveConstructionPage(-1);
        public void NextConstructionPage() => _state.MoveConstructionPage(1);

        public void ShowOverview() => _state.SetDashboardTab(KingdomDashboardTab.Overview);
        public void ShowResources() => _state.SetDashboardTab(KingdomDashboardTab.Resources);
        public void ShowStorage() => _state.SetDashboardTab(KingdomDashboardTab.Storage);
        public void ShowBuildings() => _state.SetDashboardTab(KingdomDashboardTab.Buildings);
        public void ShowUnits() => _state.SetDashboardTab(KingdomDashboardTab.Units);
        public void ShowTurns() => _state.SetDashboardTab(KingdomDashboardTab.Turns);
        public void ShowSelectionDetails() => _state.SetSelectionTab(GameplaySelectionTab.Details);
        public void ShowRecruitment() => _state.SetSelectionTab(GameplaySelectionTab.Recruit);
        public void ShowRecruitmentQueue() => _state.SetSelectionTab(GameplaySelectionTab.Queue);
        public void ShowCargo() => _state.SetSelectionTab(GameplaySelectionTab.Cargo);
        public void ShowCargoRoute() => _state.SetSelectionTab(GameplaySelectionTab.Route);
        public void SetCargoOperation(object value) => _cargo?.SetOperation(ToInt(value));
        public void SetCargoWarehouse(object value) => _cargo?.SetWarehouse(ToInt(value) - 1);
        public void SetCargoTarget(object value) => _cargo?.SetTargetWarehouse(ToInt(value) - 1);
        public void SetCargoResource(object value) => _cargo?.SetResource(ToInt(value) - 1);
        public void SetCargoRepeat(object value) => _cargo?.SetRepeat(ToBool(value));
        public void SetCargoAmount(object value) => _cargo?.SetAmount(value);
        public void TransferCargo()
        {
            var result = Execute(UiActionIds.Logistics.Transfer, "GameplayHTML/Cargo");
            if (result.Status != UiActionStatus.Performed) SetResult(result, string.Empty);
        }
        public void StartCargoRoute()
        {
            var result = Execute(UiActionIds.Logistics.StartRoute, "GameplayHTML/Route");
            if (result.Status != UiActionStatus.Performed) SetResult(result, string.Empty);
        }
        public void StopCargoRoute()
        {
            var result = Execute(UiActionIds.Logistics.StopRoute, "GameplayHTML/Route");
            if (result.Status != UiActionStatus.Performed) SetResult(result, string.Empty);
        }
        public void FoundSettlement()
        {
            var result = Execute(UiActionIds.Logistics.FoundSettlement, "GameplayHTML/Cargo");
            if (result.Status != UiActionStatus.Performed) SetResult(result, string.Empty);
        }

        public async void AttackSelection()
        {
            try
            {
                CombatCommandResult result = await _readModel.AttackSelectionAsync();
                _state.SetFeedback(result.Succeeded
                    ? result.DamageApplied > 0
                        ? $"Attack dealt {result.DamageApplied} damage."
                        : string.IsNullOrWhiteSpace(result.Reason)
                            ? "Attack request sent."
                            : result.Reason
                    : string.IsNullOrWhiteSpace(result.Reason)
                        ? "Attack was rejected."
                        : result.Reason);
                if (result.Succeeded && result.DamageApplied > 0)
                {
                    Vector2Int? position = _readModel.TryGetUnitPosition(result.TargetEntityId, out var targetPosition)
                        ? targetPosition
                        : null;
                    _state.AddNotification(
                        result.TargetDied
                            ? $"Attack destroyed {result.TargetEntityId}."
                            : $"Attack hit {result.TargetEntityId} for {result.DamageApplied}.",
                        result.TargetDied ? "Warning" : "Info",
                        position,
                        result.TargetEntityId);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"[GameplayHTML] Attack failed: {exception.Message}");
                _state.SetFeedback(exception.Message);
            }
        }

        public void CaptureSelection()
        {
            UiActionResult result = Execute(UiActionIds.Combat.CaptureSelection, "GameplayHTML/Combat");
            SetResult(result, "Settlement captured.");
        }

        public void Recruit(object value)
        {
            string unitTypeId = value?.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(unitTypeId))
                return;
            UiActionResult result = _actions.Value.Execute(
                UiActionIds.Recruitment.Enqueue,
                UiActionSource.Button,
                "GameplayHTML/Recruitment",
                unitTypeId);
            SetResult(result, $"{unitTypeId} added to the recruitment queue.");
        }

        public void CancelRecruitment(object value)
            => SetResult(_actions.Value.Execute(UiActionIds.Recruitment.Cancel,
                UiActionSource.Button, "GameplayHTML/Recruitment", value?.ToString()),
                "Training cancelled. Resources returned.");

        public void DeployRecruitment(object value)
            => SetResult(_actions.Value.Execute(UiActionIds.Recruitment.Deploy,
                UiActionSource.Button, "GameplayHTML/Recruitment", value?.ToString()),
                "Choose a deployment tile.");

        public void FocusWarehouse(object x, object y, object targetId)
        {
            _cameraFocus?.FocusGridPosition(
                new Vector2Int(ToInt(x), ToInt(y)),
                targetId?.ToString());
            _state.ClosePanel();
        }

        public async void ExitToMenu()
        {
            if (_exit == null || _exit.IsExiting)
                return;
            try
            {
                ExitMatchResult result = await _exit.ExitToMenuAsync();
                if (!result.Succeeded && !result.Cancelled)
                    _state.SetFeedback(string.IsNullOrWhiteSpace(result.Error) ? "Could not leave the session." : result.Error);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[GameplayHTML] Exit failed: {exception.Message}");
                _state.SetFeedback(exception.Message);
            }
        }

        private void OpenOverlayPanel(GameplayHtmlPanel panel)
        {
            if (_state.OpenPanelId == GameplayHtmlPanel.Construction)
            {
                UiActionResult closeResult = Execute(UiActionIds.Construction.Close, "GameplayHTML");
                if (closeResult.Status == UiActionStatus.Rejected)
                {
                    SetResult(closeResult, string.Empty);
                    return;
                }
            }
            _state.OpenPanel(panel);
        }

        private UiActionResult Execute(UiActionId id, string context)
            => _actions?.Value?.Execute(id, UiActionSource.Button, context)
               ?? UiActionResult.Rejected(UiActionReason.ActionUnavailable, "UI action router is unavailable.");

        private bool IsInitialCastleRequired()
        {
            if (_construction is not IConstructionBootstrapQuery bootstrap)
                return false;

            string ownerId = _roles?.Resolve().PlayerId;
            if (string.IsNullOrWhiteSpace(ownerId))
                ownerId = _construction.GetActiveOwner();

            return !string.IsNullOrWhiteSpace(ownerId)
                   && bootstrap.RequiresInitialCastle(ownerId.Trim(), out _);
        }

        private void SetResult(UiActionResult result, string success)
        {
            _state.SetFeedback(result.Status == UiActionStatus.Performed
                ? string.IsNullOrWhiteSpace(result.Details)
                    ? success
                    : result.Details
                : string.IsNullOrWhiteSpace(result.Details)
                    ? result.Reason.ToString()
                    : result.Details);
        }

        private static int ToInt(object value)
        {
            if (value == null)
                return 0;
            if (value is int integer)
                return integer;
            if (value is double number)
                return Mathf.RoundToInt((float)number);
            return int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
                ? parsed
                : 0;
        }

        private static float ToFloat(object value)
        {
            if (value == null)
                return 1f;
            if (value is float single)
                return single;
            if (value is double number)
                return (float)number;
            return float.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed)
                ? parsed
                : 1f;
        }

        private static bool ToBool(object value)
        {
            if (value == null)
                return false;
            if (value is bool boolean)
                return boolean;
            if (value is int integer)
                return integer != 0;
            string text = value.ToString();
            return string.Equals(text, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(text, "on", StringComparison.OrdinalIgnoreCase)
                || string.Equals(text, "1", StringComparison.Ordinal);
        }
    }
}
