using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.InputRouting.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.UIActions.API;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Обробляє pointer-введення для будівництва через абстрактне джерело вводу.
    /// Реалізує ITickable для прямого читання натискань — TileClickInputService
    /// блокує TileClickedSignal через IsPointerOverGameObject(),
    /// коли Construction UI панелі видимі.
    /// </summary>
    internal sealed partial class ConstructionInputService : IConstructionInputService, IInitializable, IDisposable, ITickable, IUiActionHandler
    {
        private const string LogTag = "[ConstructionInput]";
        private const float PointerFollowPlaneFallbackY = 0f;

        private readonly IConstructionSessionCommands _constructionService;
        private readonly IWallTopologyService _wallTopologyService;
        private readonly IWallPathfinder _wallPathfinder;
        private readonly WallHandleController _wallHandleController;
        private readonly IObjectsMapService _objectsMapService;
        private readonly IScreenToGridConverter _screenToGrid;
        private readonly IConstructionPointerInputSource _pointerInputSource;
        private readonly IConstructionInputSettingsProvider _inputSettingsProvider;
        private readonly IGridService _gridService;
        private readonly IConstructionGridGeometryService _gridGeometry;
        private readonly ConstructionTerrainAlignmentService _terrainAlignment;
        private readonly IConstructionPlacementQuery _placementQuery;
        private readonly BuildModeGridStateController _buildGridState;
        private readonly IConstructionInteractiveUiHitTester _uiHitTester;
        private readonly IGameplayInputPolicy _inputPolicy;
        private readonly IUiContextStack _uiContexts;
        private readonly SignalBus _signalBus;
        private readonly TouchTapTracker _touchTapTracker = new TouchTapTracker();
        private readonly HashSet<Vector2Int> _wallDragPendingPositions = new();
        private readonly HashSet<Vector2Int> _wallDragNewPathSet = new();
        private readonly List<Vector2Int> _wallDragPositionsToRemove = new();
        private readonly float _touchTapMaxMovePixels;
        private readonly float _touchTapMaxDurationSeconds;
        private readonly bool _enableMousePendingPreviewDrag;
        private readonly bool _enableTouchPendingPreviewDrag;
        private readonly bool _enableMultiTouchCancel;
        private readonly bool _blockInteractiveUi;
        private readonly bool _allowClicksThroughNonInteractiveUi;

        private bool _isActive;
        private bool _isDraggingPendingPlacement;
        private Vector2Int _draggedPlacementPosition;
        private bool _hasPendingPlacementSnapTarget;
        private Vector2Int _pendingPlacementSnapTarget;
        private bool _isDraggingWallPath;
        private bool _wallUndoBatchActive;
        private Vector2Int _wallDragStartPosition;
        private Vector2Int _lastWallDragTile;
        private bool _hasTouchPendingDragCandidate;
        private Vector2Int _touchPendingDragCandidatePosition;
        private bool _hasTouchWallAnchor;
        private Vector2Int _touchWallAnchorPosition;
        private bool _hasTouchPendingMoveSource;
        private Vector2Int _touchPendingMoveSourcePosition;
        private string _lastObservedSelectedBuildingId;
        private Camera _cachedCamera;
        private bool _hasPlacementValidationCache;
        private Vector2Int _cachedPlacementValidationPosition;
        private string _cachedPlacementValidationBuildingId;
        private Vector2Int? _cachedPlacementValidationIgnoredPendingPosition;
        private bool _cachedPlacementValidationAllowed;
        private IDisposable _constructionModeContext;
        private IDisposable _buildingPlacementContext;

        [Inject]
        public ConstructionInputService(
            IConstructionSessionCommands constructionService,
            IWallTopologyService wallTopologyService,
            IWallPathfinder wallPathfinder,
            WallHandleController wallHandleController,
            IObjectsMapService objectsMapService,
            IScreenToGridConverter screenToGrid,
            IConstructionPointerInputSource pointerInputSource,
            [InjectOptional] IConstructionInputSettingsProvider inputSettingsProvider,
            IGridService gridService,
            [InjectOptional] IConstructionGridGeometryService gridGeometry,
            IConstructionPlacementQuery placementQuery,
            BuildModeGridStateController buildGridState,
            [InjectOptional] IConstructionInteractiveUiHitTester uiHitTester,
            [InjectOptional] IGameplayInputPolicy inputPolicy,
            [InjectOptional] IUiContextStack uiContexts,
            SignalBus signalBus,
            [InjectOptional] ConstructionTerrainAlignmentService terrainAlignment = null)
        {
            _constructionService = constructionService;
            _wallTopologyService = wallTopologyService;
            _wallPathfinder = wallPathfinder;
            _wallHandleController = wallHandleController;
            _objectsMapService = objectsMapService;
            _screenToGrid = screenToGrid;
            _pointerInputSource = pointerInputSource;
            _inputSettingsProvider = inputSettingsProvider;
            _gridService = gridService;
            _gridGeometry = gridGeometry;
            _placementQuery = placementQuery;
            _buildGridState = buildGridState;
            _uiHitTester = uiHitTester ?? new ConstructionInteractiveUiHitTester();
            _inputPolicy = inputPolicy;
            _uiContexts = uiContexts;
            _signalBus = signalBus;
            _terrainAlignment = terrainAlignment;
            _touchTapMaxMovePixels = Mathf.Max(0f, _inputSettingsProvider?.TouchTapMaxMovePixels ?? 18f);
            _touchTapMaxDurationSeconds = Mathf.Max(0f, _inputSettingsProvider?.TouchTapMaxDurationSeconds ?? 0.45f);
            _enableMousePendingPreviewDrag = _inputSettingsProvider?.EnableMousePendingPreviewDrag ?? true;
            _enableTouchPendingPreviewDrag = _inputSettingsProvider?.EnableTouchPendingPreviewDrag ?? true;
            _enableMultiTouchCancel = _inputSettingsProvider?.EnableMultiTouchCancel ?? true;
            _blockInteractiveUi = _inputSettingsProvider?.BlockInteractiveUI ?? true;
            _allowClicksThroughNonInteractiveUi = _inputSettingsProvider?.AllowClicksThroughNonInteractiveUI ?? true;
        }

        public void Initialize()
        {
            if (!ValidateDependencies())
                return;

            _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);
            _signalBus.Subscribe<BuildingSelectionChangedSignal>(InvalidateBuildGridHover);
            _signalBus.Subscribe<BuildingPreviewChangedSignal>(InvalidateBuildGridHover);
            _signalBus.Subscribe<BuildingPreviewMovedSignal>(InvalidateBuildGridHover);
            _signalBus.Subscribe<OnObjectsMapChangedSignal>(InvalidateBuildGridHover);
            _signalBus.Subscribe<GridTileChangedSignal>(InvalidateBuildGridHover);
            _signalBus.Subscribe<FogStateChangedSignal>(InvalidateBuildGridHover);
            _signalBus.Subscribe<SettlementResourceChangedSignal>(InvalidateBuildGridHover);
            RegisterUiContexts();
        }

        public void Dispose()
        {
            EndWallUndoBatch();
            _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
            _signalBus.TryUnsubscribe<BuildingSelectionChangedSignal>(InvalidateBuildGridHover);
            _signalBus.TryUnsubscribe<BuildingPreviewChangedSignal>(InvalidateBuildGridHover);
            _signalBus.TryUnsubscribe<BuildingPreviewMovedSignal>(InvalidateBuildGridHover);
            _signalBus.TryUnsubscribe<OnObjectsMapChangedSignal>(InvalidateBuildGridHover);
            _signalBus.TryUnsubscribe<GridTileChangedSignal>(InvalidateBuildGridHover);
            _signalBus.TryUnsubscribe<FogStateChangedSignal>(InvalidateBuildGridHover);
            _signalBus.TryUnsubscribe<SettlementResourceChangedSignal>(InvalidateBuildGridHover);
            _constructionModeContext?.Dispose();
            _buildingPlacementContext?.Dispose();
        }

        public void Tick()
        {
            if (!_isActive)
                return;

            if (HandlePcCommands())
                return;

            RefreshPlacementSelectionState();

            ConstructionPointerSnapshot pointer = ReadPointerSnapshot();
            if (!pointer.HasPointer)
            {
                ClearBuildGridHover();
                EndWallUndoBatch();
                return;
            }

            UpdateBuildGridHover(pointer);

            if (TryHandleReleaseSelectionInput(pointer))
                return;

            if (pointer.ActivePointerCount > 1)
            {
                ClearBuildGridHover();
                CancelActivePointerDrags();
                EndWallUndoBatch();
                return;
            }

            HandlePointerRelease(pointer);

            if (TryHandleWallPathDrag(pointer))
                return;

            if (TryHandlePendingPlacementDrag(pointer))
                return;

            if (!pointer.WasPressedThisFrame)
                return;

            if (IsPointerOverInteractiveUI(pointer.Position, pointer.PointerId))
            {
                return;
            }

            HandlePointerSelection(pointer.Position, pointer.PointerId, allowDragStart: true, selectionOnRelease: pointer.SelectOnRelease, skipUiCheck: true);
        }

        public void OnUndoRequested() => _constructionService.UndoLast();

        public void OnRedoRequested() => _constructionService.RedoLast();

        private bool HandlePcCommands()
        {
            Keyboard keyboard = Keyboard.current;
            Vector2 pointerPosition = Mouse.current?.position.ReadValue() ?? Vector2.zero;
            bool keyboardAllowed = _inputPolicy?.CanProcess(
                GameplayInputKind.KeyboardNavigation,
                pointerPosition) ?? true;

            if (keyboardAllowed && keyboard != null)
            {
                bool control = keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
                if (control && keyboard.zKey.wasPressedThisFrame)
                {
                    _constructionService.UndoLast();
                    return true;
                }

                if (control && keyboard.yKey.wasPressedThisFrame)
                {
                    _constructionService.RedoLast();
                    return true;
                }

                if (keyboard.rKey.wasPressedThisFrame
                    && _constructionService
                        is IConstructionRotationService rotationService)
                {
                    if (rotationService.RotateSelectedClockwise())
                    {
                        InvalidatePlacementInteractionCaches();
                        return true;
                    }
                }

                // Confirm and Escape are routed through UIActions so buttons,
                // hotkeys and Escape share one action path.
            }

            Mouse mouse = Mouse.current;
            if (mouse != null
                && mouse.rightButton.wasPressedThisFrame
                && (_inputPolicy?.CanProcess(
                    GameplayInputKind.SecondaryPointer,
                    pointerPosition) ?? true))
            {
                CancelPlacementOrExitMode();
                return true;
            }

            return false;
        }

        private void CancelPlacementOrExitMode()
        {
            if (IsInitialCastleRequired())
                return;

            bool hasActiveConstructionAction =
                _constructionService.State == BuildingPlacementState.Placing
                || _constructionService.IsDemolishMode
                || _constructionService.GetPendingPlacements().Count > 0;
            if (hasActiveConstructionAction)
            {
                _constructionService.Cancel();
                ClearBuildGridHover();
                CancelActiveDrags();
                return;
            }

            _signalBus.Fire(new GameModeChangeRequestedSignal
            {
                RequestedMode = GameModeType.Normal,
            });
        }

        public IReadOnlyCollection<UiActionId> ActionIds { get; } =
            new[]
            {
                UiActionIds.Construction.CancelPlacement,
                UiActionIds.Construction.ConfirmPlacement,
                UiActionIds.Construction.RotatePlacement,
                UiActionIds.Construction.UndoPlacement,
                UiActionIds.Construction.RedoPlacement,
            };

        public UiActionResult Execute(in UiActionRequest request)
        {
            if (request.ActionId == UiActionIds.Construction.CancelPlacement)
            {
                if (!_isActive)
                    return UiActionResult.Rejected(UiActionReason.WrongContext);
                if (IsInitialCastleRequired())
                {
                    return UiActionResult.Rejected(
                        UiActionReason.ActionUnavailable,
                        consumed: true,
                        "Place your first castle before leaving construction mode.");
                }

                CancelPlacementOrExitMode();
                return UiActionResult.Performed();
            }

            if (request.ActionId == UiActionIds.Construction.ConfirmPlacement)
            {
                if (!_isActive || _constructionService.GetPendingPlacements().Count == 0)
                    return UiActionResult.Rejected(UiActionReason.NoSelection);
                _signalBus.Fire(new PlaceBuildingConfirmRequestSignal());
                return UiActionResult.Performed();
            }

            if (request.ActionId == UiActionIds.Construction.RotatePlacement)
            {
                if (!_isActive || _constructionService is not IConstructionRotationService rotationService)
                    return UiActionResult.Rejected(UiActionReason.WrongContext);
                if (!rotationService.RotateSelectedClockwise())
                    return UiActionResult.Rejected(UiActionReason.NoSelection);
                InvalidatePlacementInteractionCaches();
                return UiActionResult.Performed();
            }

            if (request.ActionId == UiActionIds.Construction.UndoPlacement)
            {
                _constructionService.UndoLast();
                return UiActionResult.Performed();
            }

            if (request.ActionId == UiActionIds.Construction.RedoPlacement)
            {
                _constructionService.RedoLast();
                return UiActionResult.Performed();
            }

            return UiActionResult.Ignored(UiActionReason.ActionUnavailable);
        }

        private void RegisterUiContexts()
        {
            if (_uiContexts == null)
                return;

            _constructionModeContext = _uiContexts.Push(new UiContextRegistration(
                "ConstructionMode",
                UiContextLayer.Mode,
                10,
                () => _isActive,
                UiActionIds.Construction.CancelPlacement,
                blocksLowerHotkeys: false));

            _buildingPlacementContext = _uiContexts.Push(new UiContextRegistration(
                "BuildingPlacement",
                UiContextLayer.Mode,
                30,
                HasActiveConstructionAction,
                UiActionIds.Construction.CancelPlacement,
                blocksLowerHotkeys: false));
        }

        private bool HasActiveConstructionAction()
            => _isActive
                && (_constructionService.State == BuildingPlacementState.Placing
                    || _constructionService.IsDemolishMode
                    || _constructionService.GetPendingPlacements().Count > 0);

        private bool IsInitialCastleRequired()
        {
            if (_constructionService is not IConstructionBootstrapQuery bootstrap)
                return false;

            string ownerId = _constructionService.GetActiveOwner();
            return !string.IsNullOrWhiteSpace(ownerId)
                   && bootstrap.RequiresInitialCastle(ownerId.Trim(), out _);
        }

        private bool ValidateDependencies()
        {
            bool valid = true;
            valid &= LogMissingDependency(_constructionService, nameof(_constructionService));
            valid &= LogMissingDependency(_wallTopologyService, nameof(_wallTopologyService));
            valid &= LogMissingDependency(_wallPathfinder, nameof(_wallPathfinder));
            valid &= LogMissingDependency(_wallHandleController, nameof(_wallHandleController));
            valid &= LogMissingDependency(_objectsMapService, nameof(_objectsMapService));
            valid &= LogMissingDependency(_screenToGrid, nameof(_screenToGrid));
            valid &= LogMissingDependency(_pointerInputSource, nameof(_pointerInputSource));
            valid &= LogMissingDependency(_gridService, nameof(_gridService));
            valid &= LogMissingDependency(_placementQuery, nameof(_placementQuery));
            valid &= LogMissingDependency(_buildGridState, nameof(_buildGridState));
            valid &= LogMissingDependency(_uiHitTester, nameof(_uiHitTester));
            valid &= LogMissingDependency(_signalBus, nameof(_signalBus));
            return valid;
        }

        private static bool LogMissingDependency(object dependency, string name)
        {
            if (dependency != null)
                return true;

            Debug.LogError($"{LogTag} Missing dependency: {name}.");
            return false;
        }

        private bool IsPlacementSessionActive() => _constructionService.State == BuildingPlacementState.Placing;

        private bool IsPlacementSessionInactive() => !IsPlacementSessionActive();

        private void RefreshPlacementSelectionState()
        {
            if (IsPlacementSessionInactive())
            {
                ClearTouchPlacementState();
                ClearPlacementValidationCache();
                _lastObservedSelectedBuildingId = null;
                return;
            }

            string selectedBuildingId = _constructionService.GetSelectedBuildingId();
            if (!string.Equals(_lastObservedSelectedBuildingId, selectedBuildingId, StringComparison.Ordinal))
            {
                ClearTouchPlacementState();
                ClearBuildGridHover();
                ClearPlacementValidationCache();
                _lastObservedSelectedBuildingId = selectedBuildingId;
            }
        }

    }
}
