using Kruty1918.Moyva.Shared.Controls;
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
    internal sealed class GameplayHtmlPresenter :
        IInitializable,
        ITickable,
        IDisposable,
        IUiActionHandler
    {
        private static readonly UiActionId[] HandledActions =
        {
            UiActionIds.Diagnostics.PanelClose,
            UiActionIds.Recruitment.Enqueue,
            UiActionIds.Recruitment.Cancel,
            UiActionIds.Recruitment.Deploy,
            UiActionIds.Combat.CaptureSelection,
            UiActionIds.EndTurn,
            UiActionIds.ClearSelection,
        };

        private readonly SignalBus _signals;
        private readonly IUnityHtmlHost _host;
        private readonly GameplayHtmlState _state;
        private readonly GameplayHudReadModel _readModel;
        private readonly GameplayHtmlBridge _bridge;
        private readonly ITurnService _turns;
        private readonly IUiContextStack _contexts;
        private readonly GameplayHtmlAnchor[] _anchors;
        private bool _controlHintsDirty = true;
        private ControlProfile _hintProfile;
        private void OnControlSettingsChanged(PlayerControlSettingsData _) => _controlHintsDirty = true;
        private readonly IInputDeviceContext _inputDevices;
        private readonly IPlayerControlSettingsService _controlSettings;
        private readonly LazyInject<IUiActionRouter> _actions;
        private readonly IConstructionSessionCommands _construction;
        private readonly IConstructionBootstrapQuery _bootstrap;
        private readonly ILocalGameplayRoleResolver _roles;
        private readonly IGameplayProgressClock _progressClock;
        private readonly ICombatRemoteCommandRequester _remoteCombat;
        private readonly ISettlementCaptureRemoteCommandRequester _remoteSettlementCapture;
        private readonly IGameResultStateStore _gameResult;
        private readonly ITurnRemoteCommandRequester _remoteTurns;

        private GameplayHtmlAnchor _anchor;
        private IDisposable _panelContext;
        private IDisposable _initialCastleContext;
        private int _lastRenderFrame = -1;
        private string _viewportClass = string.Empty;
        private bool _initialCastleModeActive;
        private bool _mounted;
        private string _css;
        private long _lastSandboxSecond = -1;
        private int _lastSandboxCountdown = -1;

        public GameplayHtmlPresenter(
            SignalBus signals,
            IUnityHtmlHost host,
            GameplayHtmlState state,
            GameplayHudReadModel readModel,
            LazyInject<IUiActionRouter> actions,
            IConstructionSessionCommands construction,
            ITurnService turns,
            [InjectOptional] IUiContextStack contexts = null,
            [InjectOptional] ILocalGameplayRoleResolver roles = null,
            [InjectOptional] IGameplayCameraFocusService cameraFocus = null,
            [InjectOptional] IExitMatchCoordinator exit = null,
            [InjectOptional] IGameplayProgressClock progressClock = null,
            [InjectOptional] GameplayHtmlAnchor[] anchors = null,
            [InjectOptional] GameplayCargoPanel cargo = null,
            [InjectOptional] ICombatRemoteCommandRequester remoteCombat = null,
            [InjectOptional] ISettlementCaptureRemoteCommandRequester remoteSettlementCapture = null,
            [InjectOptional] IGameResultStateStore gameResult = null,
            [InjectOptional] ITurnRemoteCommandRequester remoteTurns = null,
            [InjectOptional] IInputDeviceContext inputDevices = null,
            [InjectOptional] IPlayerControlSettingsService controlSettings = null)
        {
            _inputDevices = inputDevices; _controlSettings = controlSettings;
            if (_controlSettings != null) _controlSettings.OnSettingsChanged += OnControlSettingsChanged;
            _signals = signals;
            _host = host;
            _state = state;
            _readModel = readModel;
            _turns = turns;
            _contexts = contexts;
            _anchors = anchors ?? Array.Empty<GameplayHtmlAnchor>();
            _actions = actions;
            _construction = construction;
            _bootstrap = construction as IConstructionBootstrapQuery;
            _roles = roles;
            _progressClock = progressClock;
            _remoteCombat = remoteCombat;
            _remoteSettlementCapture = remoteSettlementCapture;
            _gameResult = gameResult;
            _remoteTurns = remoteTurns;
            _bridge = new GameplayHtmlBridge(state, readModel, actions, construction, roles, cameraFocus, exit, progressClock, cargo, signals);
        }

        public IReadOnlyCollection<UiActionId> ActionIds => HandledActions;

        public void Initialize()
        {
            _anchor = _anchors.FirstOrDefault(anchor => anchor != null);
            if (_anchor == null || _anchor.MountRoot == null || _anchor.CssAsset == null)
            {
                Debug.LogError("[GameplayHTML] GameplayHtmlAnchor or CSS is missing. Legacy HUD remains disabled.");
                return;
            }

            _anchor.StopEditorPreview();
            _anchor.PrepareForMount();
            _css = _anchor.CssAsset.text;
            _anchor.SetLegacyUiVisible(false);
            _state.Changed += MarkDirty;
            GameplayNotificationStream.Published += OnNotificationPublished;
            if (_turns != null)
                _turns.StateChanged += MarkDirty;
            if (_progressClock != null)
                _progressClock.Progressed += OnProgressed;
            if (_remoteCombat != null)
                _remoteCombat.AttackRejected += OnRemoteAttackRejected;
            if (_remoteSettlementCapture != null)
                _remoteSettlementCapture.CaptureRejected += OnRemoteCaptureRejected;
            if (_remoteTurns != null)
                _remoteTurns.EndTurnResolved += OnRemoteEndTurnResolved;
            SubscribeSignals();
            if (_gameResult != null)
                _state.SetGameResult(_gameResult.IsGameOver, _gameResult.WinnerId);
            _panelContext = _contexts?.Push(new UiContextRegistration(
                "GameplayHTML/Panel",
                UiContextLayer.Panel,
                300,
                () => _state.OpenPanelId != GameplayHtmlPanel.None || _readModel.HasSelection,
                UiActionIds.Diagnostics.PanelClose,
                true,
                allowedHotkeyActionIds: new[]
                {
                    UiActionIds.Construction.Toggle,
                    UiActionIds.Construction.ConfirmPlacement,
                    UiActionIds.Construction.RotatePlacement,
                    UiActionIds.Construction.UndoPlacement,
                    UiActionIds.Construction.RedoPlacement,
                }));
            _initialCastleContext = _contexts?.Push(new UiContextRegistration(
                "InitialCastlePlacement",
                UiContextLayer.Modal,
                250,
                IsInitialCastleRequired,
                UiActionIds.Construction.CancelPlacement,
                blocksLowerHotkeys: true,
                allowedHotkeyActionIds: new[]
                {
                    UiActionIds.Construction.ConfirmPlacement,
                    UiActionIds.Construction.RotatePlacement,
                    UiActionIds.Construction.UndoPlacement,
                    UiActionIds.Construction.RedoPlacement,
                }));
            Render(true);
        }

        public void Tick()
        {
            var profile = _inputDevices?.ActiveProfile ?? ControlProfile.KeyboardMouse;
            if (_controlHintsDirty || _hintProfile != profile)
            {
                _controlHintsDirty = false; _hintProfile = profile;
                var options = _controlSettings?.Settings.Profile(profile);
                _state.ControlHints = ControlPromptText.Camera(profile, options);
                _state.GamepadAim = profile == ControlProfile.Gamepad;
                _state.MarkDirty();
            }
            if (_anchor == null)
                return;

            _state.ExpireFeedbackIfNeeded();
            if (_progressClock?.IsRealtime == true)
            {
                long second = (long)Math.Floor(_progressClock.ElapsedGameplaySeconds);
                int countdown = Mathf.CeilToInt(_progressClock.SecondsUntilNextProgress);
                if (second != _lastSandboxSecond || countdown != _lastSandboxCountdown)
                {
                    _lastSandboxSecond = second;
                    _lastSandboxCountdown = countdown;
                    _state.MarkDirty();
                }
            }
            EnsureInitialCastlePlacement();
            string viewport = _anchor.ViewportClass;
            if (!string.Equals(viewport, _viewportClass, StringComparison.Ordinal))
                _state.MarkDirty();

            if (_lastRenderFrame == Time.frameCount)
                return;
            Render(false);
        }

        public void Dispose()
        {
            if (_controlSettings != null) _controlSettings.OnSettingsChanged -= OnControlSettingsChanged;
            _state.Changed -= MarkDirty;
            GameplayNotificationStream.Published -= OnNotificationPublished;
            if (_turns != null)
                _turns.StateChanged -= MarkDirty;
            if (_progressClock != null)
                _progressClock.Progressed -= OnProgressed;
            if (_remoteCombat != null)
                _remoteCombat.AttackRejected -= OnRemoteAttackRejected;
            if (_remoteSettlementCapture != null)
                _remoteSettlementCapture.CaptureRejected -= OnRemoteCaptureRejected;
            if (_remoteTurns != null)
                _remoteTurns.EndTurnResolved -= OnRemoteEndTurnResolved;
            UnsubscribeSignals();
            _panelContext?.Dispose();
            _panelContext = null;
            _initialCastleContext?.Dispose();
            _initialCastleContext = null;
            _host?.Dispose();
        }

        private void OnProgressed(GameplayProgressTick _)
            => MarkDirty();

        public UiActionResult Execute(in UiActionRequest request)
        {
            if (request.ActionId == UiActionIds.Recruitment.Enqueue)
                return _readModel.TryRecruitSelected(request.TargetId);
            if (request.ActionId == UiActionIds.Recruitment.Cancel)
                return _readModel.TryCancelRecruitment(request.TargetId);
            if (request.ActionId == UiActionIds.Recruitment.Deploy)
            {
                if (!_readModel.TryGetReadyRecruitment(request.TargetId, out var signal))
                    return UiActionResult.Rejected(UiActionReason.NoSelection);
                _signals.Fire(signal);
                return UiActionResult.Performed();
            }

            if (request.ActionId == UiActionIds.Combat.CaptureSelection)
            {
                SettlementCaptureResult result = _readModel.CaptureSelection();
                return result.Succeeded
                    ? new UiActionResult(
                        UiActionStatus.Performed,
                        details: string.IsNullOrWhiteSpace(result.Reason)
                            ? "Settlement captured."
                            : result.Reason)
                    : UiActionResult.Rejected(
                        UiActionReason.ActionUnavailable,
                        string.IsNullOrWhiteSpace(result.Reason)
                            ? "Settlement capture was rejected."
                            : result.Reason);
            }

            if (request.ActionId == UiActionIds.EndTurn)
            {
                if (!_readModel.IsTurnUiEnabled())
                    return UiActionResult.Performed();

                if (_turns == null || string.IsNullOrWhiteSpace(_turns.LocalOwnerId))
                    return UiActionResult.Rejected(UiActionReason.ActionUnavailable, "Local turn owner is unavailable.");
                if (_roles?.Resolve().Role == LocalGameplayRole.Client)
                {
                    if (_remoteTurns == null)
                        return UiActionResult.Rejected(UiActionReason.ActionUnavailable, "Turn transport is unavailable.");
                    return _remoteTurns.TryRequestEndTurn(out string remoteReason)
                        ? new UiActionResult(UiActionStatus.Performed, details: "Waiting for the host to confirm the turn.")
                        : UiActionResult.Rejected(UiActionReason.ActionUnavailable, remoteReason);
                }
                return _turns.TryEndTurn(_turns.LocalOwnerId, out string reason)
                    ? UiActionResult.Performed()
                    : UiActionResult.Rejected(
                        UiActionReason.ActionUnavailable,
                        string.IsNullOrWhiteSpace(reason) ? "The turn cannot be ended yet." : reason);
            }

            if (request.ActionId == UiActionIds.ClearSelection)
            {
                _signals.Fire(new WorldInfoPanelClosedSignal());
                return UiActionResult.Performed();
            }

            if (request.ActionId != UiActionIds.Diagnostics.PanelClose
                || (_state.OpenPanelId == GameplayHtmlPanel.None && !_readModel.HasSelection))
            {
                return UiActionResult.Ignored(UiActionReason.WrongContext);
            }

            if (_state.OpenPanelId != GameplayHtmlPanel.None)
                _bridge.ClosePanel();
            else
                _signals.Fire(new WorldInfoPanelClosedSignal());
            return UiActionResult.Performed();
        }

        private void Render(bool force)
        {
            if (!force && !_state.ConsumeDirty())
                return;
            if (force)
                _state.ConsumeDirty();

            _lastRenderFrame = Time.frameCount;
            string viewportClass = _anchor.ViewportClass;
            GameplayHtmlSnapshot snapshot = _readModel.Capture(_state);
            snapshot.EndTurnPending = _remoteTurns?.IsEndTurnPending ?? false;
            var globals = new Dictionary<string, object>
            {
                ["gameplay"] = _bridge,
            };
            if (_anchor.FontAsset != null)
                globals["moyvaFont"] = _anchor.FontAsset;
            AddIconGlobals(globals, snapshot);
            if (_anchor.NotificationsIcon != null) globals["gameplay_notifications_icon"] = _anchor.NotificationsIcon;
            if (_anchor.MenuIcon != null) globals["gameplay_menu_icon"] = _anchor.MenuIcon;

            var regions = GameplayHtmlMarkup.BuildRegions(snapshot, _state);
            if (force || !_mounted || _viewportClass != viewportClass || !_host.UpdateRegions(regions, globals))
            {
                UnityHtmlMountResult result = _host.Mount(
                    _anchor.MountRoot,
                    new UnityHtmlDocument(
                        GameplayHtmlMarkup.BuildDocument(regions, viewportClass), _css, "Moyva Gameplay"),
                    globals);
                _mounted = result.Succeeded;
                if (!_mounted)
                    Debug.LogError($"[GameplayHTML] Mount failed: {result.ErrorMessage}");
            }
            _viewportClass = viewportClass;
            _anchor.SyncInputShields(snapshot, _state);
        }

        private static void AddIconGlobals(
            IDictionary<string, object> globals,
            GameplayHtmlSnapshot snapshot)
        {
            if (globals == null || snapshot == null)
                return;
            foreach (var icon in snapshot.Icons)
                globals[icon.Key] = icon.Value;

            for (int index = 0; index < snapshot.BuildingOptions.Length; index++)
            {
                GameplayBuildingOptionSnapshot option = snapshot.BuildingOptions[index];
                if (option.Icon != null && !string.IsNullOrWhiteSpace(option.IconGlobalKey))
                    globals[option.IconGlobalKey] = option.Icon;
            }

            for (int index = 0; index < snapshot.RecruitmentRecipes.Length; index++)
            {
                GameplayRecruitmentRecipeSnapshot recipe = snapshot.RecruitmentRecipes[index];
                if (recipe.Icon != null && !string.IsNullOrWhiteSpace(recipe.IconGlobalKey))
                    globals[recipe.IconGlobalKey] = recipe.Icon;
            }
        }

        private bool IsInitialCastleRequired()
        {
            if (_turns == null
                || string.IsNullOrWhiteSpace(_turns.LocalOwnerId)
                || _bootstrap == null)
            {
                return false;
            }

            return _bootstrap.RequiresInitialCastle(_turns.LocalOwnerId, out _);
        }

        private void EnsureInitialCastlePlacement()
        {
            if (_turns == null
                || _turns.Phase != TurnPhase.AwaitingInput
                || string.IsNullOrWhiteSpace(_turns.LocalOwnerId)
                || !string.Equals(_turns.ActiveOwnerId, _turns.LocalOwnerId, StringComparison.Ordinal)
                || _bootstrap == null)
            {
                return;
            }

            bool required = _bootstrap.RequiresInitialCastle(
                _turns.LocalOwnerId,
                out string castleBuildingId);
            if (!required)
            {
                if (_initialCastleModeActive)
                {
                    _actions.Value.Execute(
                        UiActionIds.Construction.Close,
                        UiActionSource.Programmatic,
                        "GameplayHTML/Onboarding");
                    _initialCastleModeActive = false;
                }
                return;
            }

            if (!_initialCastleModeActive)
            {
                UiActionResult opened = _actions.Value.Execute(
                    UiActionIds.Construction.Open,
                    UiActionSource.Programmatic,
                    "GameplayHTML/Onboarding");
                _initialCastleModeActive = opened.Status == UiActionStatus.Performed;
            }

            if (_initialCastleModeActive
                && !string.Equals(
                    _construction.GetSelectedBuildingId(),
                    castleBuildingId,
                    StringComparison.Ordinal))
            {
                foreach (string pendingId in _construction.GetPendingPlacements().Values)
                    if (string.Equals(pendingId, castleBuildingId, StringComparison.Ordinal)) return;
                _actions.Value.Execute(
                    UiActionIds.Construction.SelectBuilding,
                    UiActionSource.Programmatic,
                    "GameplayHTML/Onboarding",
                    castleBuildingId);
            }
        }

        private void MarkDirty()
        {
            if (!_state.Dirty)
                _state.MarkDirty();
        }

        private void SubscribeSignals()
        {
            _signals.Subscribe<BuildingPlacedSignal>(OnGameplayChanged);
            _signals.Subscribe<BuildingCancelledSignal>(OnGameplayChanged);
            _signals.Subscribe<BuildingPreviewChangedSignal>(OnPreviewChanged);
            _signals.Subscribe<BuildingSelectionChangedSignal>(OnGameplayChanged);
            _signals.Subscribe<BuildingOperationalSignal>(OnBuildingOperational);
            _signals.Subscribe<CaravanDeliveryCompletedSignal>(OnDeliveryCompleted);
            _signals.Subscribe<BuildingDemolishedSignal>(OnGameplayChanged);
            _signals.Subscribe<EconomyTickCompletedSignal>(OnGameplayChanged);
            _signals.Subscribe<SettlementCreatedSignal>(OnGameplayChanged);
            _signals.Subscribe<SettlementDeactivatedSignal>(OnGameplayChanged);
            _signals.Subscribe<SettlementCapturedSignal>(OnGameplayChanged);
            _signals.Subscribe<SettlementResourceChangedSignal>(OnGameplayChanged);
            _signals.Subscribe<UnitCreatedSignal>(OnGameplayChanged);
            _signals.Subscribe<UnitDestroyedSignal>(OnGameplayChanged);
            _signals.Subscribe<SettlementPopulationChangedSignal>(OnPopulationChanged);
            _signals.Subscribe<GameModeChangedSignal>(OnGameModeChanged);
            _signals.Subscribe<UnitRecruitmentQueueChangedSignal>(OnGameplayChanged);
            _signals.Subscribe<UnitRecruitmentReadySignal>(OnGameplayChanged);
            _signals.Subscribe<UnitRecruitmentDeployedSignal>(OnGameplayChanged);
            _signals.Subscribe<WorldInfoSelectionChangedSignal>(OnSelectionChanged);
            _signals.Subscribe<GamePausedSignal>(OnPauseChanged);
            _signals.Subscribe<GameEndedSignal>(OnGameEnded);
            _signals.Subscribe<GameStartedSignal>(OnGameStarted);
        }

        private void UnsubscribeSignals()
        {
            _signals.TryUnsubscribe<BuildingPlacedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<BuildingCancelledSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<BuildingPreviewChangedSignal>(OnPreviewChanged);
            _signals.TryUnsubscribe<BuildingSelectionChangedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<BuildingOperationalSignal>(OnBuildingOperational);
            _signals.TryUnsubscribe<CaravanDeliveryCompletedSignal>(OnDeliveryCompleted);
            _signals.TryUnsubscribe<BuildingDemolishedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<EconomyTickCompletedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<SettlementCreatedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<SettlementDeactivatedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<SettlementCapturedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<SettlementResourceChangedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<UnitCreatedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<UnitDestroyedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<SettlementPopulationChangedSignal>(OnPopulationChanged);
            _signals.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
            _signals.TryUnsubscribe<UnitRecruitmentQueueChangedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<UnitRecruitmentReadySignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<UnitRecruitmentDeployedSignal>(OnGameplayChanged);
            _signals.TryUnsubscribe<WorldInfoSelectionChangedSignal>(OnSelectionChanged);
            _signals.TryUnsubscribe<GamePausedSignal>(OnPauseChanged);
            _signals.TryUnsubscribe<GameEndedSignal>(OnGameEnded);
            _signals.TryUnsubscribe<GameStartedSignal>(OnGameStarted);
        }

        private void OnPreviewChanged(BuildingPreviewChangedSignal signal)
        {
            if (_readModel.SetPreview(signal))
                _state.MarkDirty();
        }

        private void OnSelectionChanged(WorldInfoSelectionChangedSignal signal)
        {
            _readModel.SetSelection(signal);
            _state.ResetSelectionTab();
            _state.MarkDirty();
        }

        private void OnPauseChanged(GamePausedSignal signal) => _state.SetPaused(signal.IsPaused);
        private void OnGameEnded(GameEndedSignal signal) => _state.SetGameResult(true, signal.WinnerId);
        private void OnGameStarted(GameStartedSignal _) => _state.SetGameResult(false, string.Empty);
        private void OnRemoteEndTurnResolved(bool accepted, string reason)
            => _state.SetFeedback(reason);
        private void OnNotificationPublished(GameplayNotificationRequest request)
            => _state.AddNotification(request.Message, request.Kind.ToString());
        private void OnRemoteAttackRejected(CombatRemoteCommandResult result)
            => _state.AddNotification(
                string.IsNullOrWhiteSpace(result.Reason)
                    ? "Attack was rejected by host."
                    : result.Reason,
                GameplayNotificationKind.Error.ToString());
        private void OnRemoteCaptureRejected(SettlementCaptureRemoteResult result)
            => _state.AddNotification(
                string.IsNullOrWhiteSpace(result.Reason)
                    ? "Settlement capture was rejected by host."
                    : result.Reason,
                GameplayNotificationKind.Error.ToString());
        private void OnGameplayChanged(BuildingPlacedSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(BuildingCancelledSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(BuildingSelectionChangedSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(BuildingDemolishedSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(EconomyTickCompletedSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(SettlementCreatedSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(SettlementDeactivatedSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(SettlementCapturedSignal signal)
        {
            string ownerId = ResolveLocalOwnerId();
            if (string.Equals(signal.NewOwnerId, ownerId, StringComparison.Ordinal))
            {
                _state.AddNotification("Settlement captured.", GameplayNotificationKind.Success.ToString());
                return;
            }

            if (string.Equals(signal.PreviousOwnerId, ownerId, StringComparison.Ordinal))
            {
                _state.AddNotification("A settlement was captured by another kingdom.", GameplayNotificationKind.Warning.ToString());
                return;
            }

            _state.AddNotification("A settlement changed hands.", GameplayNotificationKind.Info.ToString());
        }
        private void OnGameplayChanged(SettlementResourceChangedSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(UnitCreatedSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(UnitDestroyedSignal _) => _state.MarkDirty();
        private void OnPopulationChanged(SettlementPopulationChangedSignal _) => _state.MarkDirty();

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            if (signal.NewMode == GameModeType.Construction)
                _state.OpenPanel(GameplayHtmlPanel.Construction);
            else if (_state.OpenPanelId == GameplayHtmlPanel.Construction)
                _state.ClosePanel();
            _state.MarkDirty();
        }

        private void OnGameplayChanged(UnitRecruitmentQueueChangedSignal _) => _state.MarkDirty();
        private void OnGameplayChanged(UnitRecruitmentReadySignal signal)
        {
            _state.MarkDirty();
            if (signal.OwnerId == ResolveLocalOwnerId())
                _state.AddNotification($"{signal.UnitTypeId} is ready to deploy.", "Success",
                    signal.BuildingPosition, signal.UnitTypeId, signal.QueueId);
        }
        private void OnBuildingOperational(BuildingOperationalSignal signal)
        {
            _state.MarkDirty();
            if (signal.OwnerId == ResolveLocalOwnerId())
                _state.AddNotification($"{_readModel.ResolveBuildingDisplayName(signal.BuildingId)} is complete.",
                    "Success", signal.Position, signal.BuildingId);
        }
        private void OnDeliveryCompleted(CaravanDeliveryCompletedSignal signal)
        {
            if (signal.OwnerId != ResolveLocalOwnerId()) return;
            var details = new StringBuilder("Delivered successfully: ");
            foreach (var resource in signal.Resources)
                details.Append(resource.Key).Append(" +").Append(resource.Value.ToString("0.#")).Append("  ");
            _state.AddNotification("Wagon delivery complete.", "Success", signal.WarehousePosition,
                signal.UnitId, deliveryDetails: details.ToString());
        }
        private void OnGameplayChanged(UnitRecruitmentDeployedSignal _) => _state.MarkDirty();

        private string ResolveLocalOwnerId()
        {
            string ownerId = _turns?.LocalOwnerId;
            if (string.IsNullOrWhiteSpace(ownerId))
                ownerId = _roles?.Resolve().PlayerId;
            if (string.IsNullOrWhiteSpace(ownerId))
                ownerId = _construction?.GetActiveOwner();
            return string.IsNullOrWhiteSpace(ownerId) ? "player_0" : ownerId.Trim();
        }
    }
}
