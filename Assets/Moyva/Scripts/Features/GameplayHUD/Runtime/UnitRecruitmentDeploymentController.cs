using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.InputRouting.API;
using Kruty1918.Moyva.Presentation.API;
using Kruty1918.Moyva.Presentation.Runtime;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.UIActions.API;
using Kruty1918.Moyva.Units.API;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed partial class UnitRecruitmentDeploymentController :
        IInitializable,
        ITickable,
        IDisposable,
        ITurnBlocker,
        IUiActionHandler
    {
        private const string LogTag = "[UnitDeployment]";
        private const string PreviewRootName = "UnitDeploymentPreviewRoot";
        private const string ControlsRootName = "UnitDeploymentControls";
        private const float PreviewLift = 0.08f;
        private const float SpritePreviewHeight = 0.95f;
        private const float FallbackPreviewHeight = 0.85f;
        private const int OverlayRenderQueue = 3988;
        private const string TurnBlockReason =
            "Завершіть або скасуйте розміщення готового юніта.";

        private static readonly GameplayInputKind DeploymentInputMask =
            GameplayInputKind.PrimaryPointer
            | GameplayInputKind.SecondaryPointer
            | GameplayInputKind.Placement;

        private static readonly Color PreviewTint =
            new(0.72f, 1f, 0.74f, 0.72f);

        private readonly SignalBus _signalBus;
        private readonly ITurnService _turns;
        private readonly IGameplayProgressClock _progressClock;
        private readonly IUnitRecruitmentService _recruitment;
        private readonly IUnitClassConfig _unitConfigs;
        private readonly IGridProjection _gridProjection;
        private readonly IGridService _grid;
        private readonly IScreenToGridConverter _screenToGrid;
        private readonly IWorldPointerGridResolver _pointerGridResolver;
        private readonly IGridActionOverlayService _gridOverlay;
        private readonly IUnitWorldPositionResolver _worldPositionResolver;
        private readonly IGameplayInputPolicy _inputPolicy;
        private readonly IUiContextStack _uiContexts;
        private readonly IGameModeService _gameModeService;
        private readonly GameplayTurnHudView _hudView;
        private readonly List<GridActionOverlayCell> _overlayCells = new();
        private readonly MaterialPropertyBlock _previewPropertyBlock = new();

        private DeploymentSession _session;
        private Transform _previewRoot;
        private GameObject _previewObject;
        private Material _fallbackPreviewMaterial;
        private Canvas _canvas;
        private RectTransform _controlsRoot;
        private Button _confirmButton;
        private Button _cancelButton;
        private UnityEngine.Camera _camera;
        private bool _confirmInProgress;
        private bool _warnedMissingCamera;
        private IDisposable _deploymentContext;

        public UnitRecruitmentDeploymentController(
            SignalBus signalBus,
            ITurnService turns,
            IUnitRecruitmentService recruitment,
            IUnitClassConfig unitConfigs,
            IGridProjection gridProjection,
            [InjectOptional] IGridService grid = null,
            [InjectOptional] IScreenToGridConverter screenToGrid = null,
            [InjectOptional] IWorldPointerGridResolver pointerGridResolver = null,
            [InjectOptional] IGridActionOverlayService gridOverlay = null,
            [InjectOptional] IUnitWorldPositionResolver worldPositionResolver = null,
            [InjectOptional] IGameplayInputPolicy inputPolicy = null,
            [InjectOptional] IUiContextStack uiContexts = null,
            [InjectOptional] IGameModeService gameModeService = null,
            [InjectOptional] GameplayTurnHudView hudView = null,
            [InjectOptional] IGameplayProgressClock progressClock = null)
        {
            _signalBus = signalBus;
            _turns = turns;
            _progressClock = progressClock;
            _recruitment = recruitment;
            _unitConfigs = unitConfigs;
            _gridProjection = gridProjection;
            _grid = grid;
            _screenToGrid = screenToGrid;
            _pointerGridResolver = pointerGridResolver;
            _gridOverlay = gridOverlay;
            _worldPositionResolver = worldPositionResolver;
            _inputPolicy = inputPolicy;
            _uiContexts = uiContexts;
            _gameModeService = gameModeService;
            _hudView = hudView;
        }

        public void Initialize()
        {
            _turns.StateChanged += OnTurnStateChanged;
            _signalBus.Subscribe<UnitRecruitmentReadyIndicatorClickedSignal>(
                OnReadyIndicatorClicked);
            _signalBus.Subscribe<UnitRecruitmentQueueChangedSignal>(
                OnRecruitmentQueueChanged);
            _signalBus.Subscribe<UnitRecruitmentDeployedSignal>(
                OnRecruitmentDeployed);
            _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);
            RegisterUiContexts();
        }

        public void Dispose()
        {
            _turns.StateChanged -= OnTurnStateChanged;
            _signalBus.TryUnsubscribe<UnitRecruitmentReadyIndicatorClickedSignal>(
                OnReadyIndicatorClicked);
            _signalBus.TryUnsubscribe<UnitRecruitmentQueueChangedSignal>(
                OnRecruitmentQueueChanged);
            _signalBus.TryUnsubscribe<UnitRecruitmentDeployedSignal>(
                OnRecruitmentDeployed);
            _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
            _deploymentContext?.Dispose();

            EndSession(destroyPreview: true);
            DestroyRuntimeRoot(_previewRoot);
            if (_controlsRoot != null)
                Object.Destroy(_controlsRoot.gameObject);
            _previewRoot = null;
            _controlsRoot = null;
            _confirmButton = null;
            _cancelButton = null;
            DestroyMaterial(_fallbackPreviewMaterial);
            _fallbackPreviewMaterial = null;
        }

        public void Tick()
        {
            if (_session == null)
                return;

            if (_progressClock?.IsRealtime != true
                && !_turns.CanOwnerAct(_session.OwnerId, out _))
            {
                ExecuteActionOrFallback(UiActionIds.Deployment.Cancel, UiActionSource.Programmatic);
                return;
            }

            HandleKeyboard();
            HandleMouse();
            FaceSpritePreviewToCamera();
        }

        public bool IsTurnBlocked(out string reason)
        {
            if (_session == null)
            {
                reason = null;
                return false;
            }

            reason = TurnBlockReason;
            return true;
        }

    }
}
