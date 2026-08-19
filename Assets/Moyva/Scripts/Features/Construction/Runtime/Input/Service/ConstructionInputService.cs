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
        private const string PerfLogTag =
            "[MoyvaConstructionPerf]";
        private const float PointerFollowPlaneFallbackY = 0f;

        private readonly IConstructionService _constructionService;
        private readonly IWallTopologyService _wallTopologyService;
        private readonly IWallPathfinder _wallPathfinder;
        private readonly IWallHandleController _wallHandleController;
        private readonly IObjectsMapService _objectsMapService;
        private readonly IScreenToGridConverter _screenToGrid;
        private readonly IConstructionPointerInputSource _pointerInputSource;
        private readonly IConstructionInputSettingsProvider _inputSettingsProvider;
        private readonly IConstructionDiagnosticsSettingsProvider _diagnosticsSettingsProvider;
        private readonly IGridService _gridService;
        private readonly IConstructionGridGeometryService _gridGeometry;
        private readonly IConstructionTerrainAlignmentService _terrainAlignment;
        private readonly IConstructionPlacementQuery _placementQuery;
        private readonly BuildModeGridStateController _buildGridState;
        private readonly IConstructionBuildGridDiagnostics _buildGridDiagnostics;
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
        private bool VerboseLogs => _diagnosticsSettingsProvider?.EnableVerboseLogs ?? (Application.isEditor && Debug.isDebugBuild);

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
            IConstructionService constructionService,
            IWallTopologyService wallTopologyService,
            IWallPathfinder wallPathfinder,
            IWallHandleController wallHandleController,
            IObjectsMapService objectsMapService,
            IScreenToGridConverter screenToGrid,
            IConstructionPointerInputSource pointerInputSource,
            [InjectOptional] IConstructionInputSettingsProvider inputSettingsProvider,
            [InjectOptional] IConstructionDiagnosticsSettingsProvider diagnosticsSettingsProvider,
            IGridService gridService,
            [InjectOptional] IConstructionGridGeometryService gridGeometry,
            IConstructionPlacementQuery placementQuery,
            BuildModeGridStateController buildGridState,
            IConstructionBuildGridDiagnostics buildGridDiagnostics,
            [InjectOptional] IConstructionInteractiveUiHitTester uiHitTester,
            [InjectOptional] IGameplayInputPolicy inputPolicy,
            [InjectOptional] IUiContextStack uiContexts,
            SignalBus signalBus,
            [InjectOptional] IConstructionTerrainAlignmentService terrainAlignment = null)
        {
            _constructionService = constructionService;
            _wallTopologyService = wallTopologyService;
            _wallPathfinder = wallPathfinder;
            _wallHandleController = wallHandleController;
            _objectsMapService = objectsMapService;
            _screenToGrid = screenToGrid;
            _pointerInputSource = pointerInputSource;
            _inputSettingsProvider = inputSettingsProvider;
            _diagnosticsSettingsProvider = diagnosticsSettingsProvider;
            _gridService = gridService;
            _gridGeometry = gridGeometry;
            _placementQuery = placementQuery;
            _buildGridState = buildGridState;
            _buildGridDiagnostics = buildGridDiagnostics;
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
            if (VerboseLogs)
                Debug.Log($"{LogTag} Initialized.");
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
            if (VerboseLogs)
                Debug.Log($"{LogTag} Disposed.");
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
                if (VerboseLogs)
                    Debug.Log($"{LogTag} Click ignored: pointer over interactive UI.");
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

        private void UpdateBuildGridHover(ConstructionPointerSnapshot pointer)
        {
            if (pointer.ActivePointerCount > 1
                || IsPointerOverInteractiveUI(pointer.Position, pointer.PointerId)
                || !TryResolvePointerTile(pointer.Position, out Vector2Int tile))
            {
                ClearBuildGridHover();
                return;
            }

            string buildingId = IsPlacementSessionActive() && !_constructionService.IsDemolishMode
                ? _constructionService.GetSelectedBuildingId()
                : null;
            if (string.IsNullOrWhiteSpace(buildingId))
            {
                if (!_buildGridState.SetHover(tile, ConstructionBuildGridTileVisualState.General))
                    return;

                PublishBuildGridHover(
                    tile,
                    null,
                    true,
                    true,
                    new[] { tile },
                    Array.Empty<Vector2Int>());
                return;
            }

            if (_buildGridState.HoverPosition == tile)
                return;

            ConstructionPlacementQueryResult detailed = _placementQuery.EvaluatePlacement(
                new ConstructionPlacementQueryRequest(
                    buildingId,
                    tile,
                    includeResources: true,
                    includeDetails: true,
                    attemptSource:
                        ConstructionPlacementAttemptSource.PointerHover,
                    allowUniquePreviewRelocation: true,
                    rotation: ResolveSelectedRotation()));
            ConstructionBuildGridTileVisualState visualState =
                ResolvePlacementVisualState(detailed);
            if (!_buildGridState.SetHover(tile, visualState))
                return;
            BuildHoverFootprintArrays(
                tile,
                detailed,
                out Vector2Int[] footprintPositions,
                out Vector2Int[] invalidPositions);
            PublishBuildGridHover(
                tile,
                buildingId,
                detailed.CanPreview,
                detailed.ResourcesValid,
                footprintPositions,
                invalidPositions);
        }

        private void ClearBuildGridHover()
        {
            if (_buildGridState == null || !_buildGridState.ClearHover())
                return;

            _signalBus.Fire(new BuildGridHoverChangedSignal
            {
                HasTile = false,
                FootprintPositions = Array.Empty<Vector2Int>(),
                InvalidFootprintPositions = Array.Empty<Vector2Int>(),
            });
            _buildGridDiagnostics?.LogHoverChanged(
                false,
                default,
                ConstructionBuildGridTileVisualState.Missing);
        }

        private void InvalidateBuildGridHover(BuildingSelectionChangedSignal _) => InvalidatePlacementInteractionCaches();
        private void InvalidateBuildGridHover(BuildingPreviewChangedSignal _) => InvalidatePlacementInteractionCaches();
        private void InvalidateBuildGridHover(BuildingPreviewMovedSignal _) => InvalidatePlacementInteractionCaches();
        private void InvalidateBuildGridHover(OnObjectsMapChangedSignal _) => InvalidatePlacementInteractionCaches();
        private void InvalidateBuildGridHover(GridTileChangedSignal _) => InvalidatePlacementInteractionCaches();
        private void InvalidateBuildGridHover(FogStateChangedSignal _) => InvalidatePlacementInteractionCaches();
        private void InvalidateBuildGridHover(SettlementResourceChangedSignal _) => InvalidatePlacementInteractionCaches();

        private void InvalidatePlacementInteractionCaches()
        {
            ClearBuildGridHover();
            ClearPlacementValidationCache();
        }

        private void ClearPlacementValidationCache()
        {
            _hasPlacementValidationCache = false;
            _cachedPlacementValidationPosition = default;
            _cachedPlacementValidationBuildingId = null;
            _cachedPlacementValidationIgnoredPendingPosition = null;
            _cachedPlacementValidationAllowed = false;
        }

        private void PublishBuildGridHover(
            Vector2Int tile,
            string buildingId,
            bool isPlacementValid,
            bool isAffordable,
            Vector2Int[] footprintPositions,
            Vector2Int[] invalidPositions)
        {
            _signalBus.Fire(new BuildGridHoverChangedSignal
            {
                HasTile = true,
                Position = tile,
                BuildingId = buildingId,
                IsPlacementValid = isPlacementValid,
                IsAffordable = isAffordable,
                RotationQuarterTurns =
                    (int)ResolveSelectedRotation(),
                FootprintPositions = footprintPositions,
                InvalidFootprintPositions = invalidPositions,
            });
            _buildGridDiagnostics?.LogHoverChanged(
                true,
                tile,
                _buildGridState.HoverVisualState);
        }

        private static void BuildHoverFootprintArrays(
            Vector2Int origin,
            ConstructionPlacementQueryResult result,
            out Vector2Int[] footprintPositions,
            out Vector2Int[] invalidPositions)
        {
            BuildingPlacementEvaluationResult evaluation = result.EvaluationResult;
            if (evaluation == null || evaluation.FootprintPositions.Count == 0)
            {
                footprintPositions = new[] { origin };
                invalidPositions = result.SpatialValid
                    ? Array.Empty<Vector2Int>()
                    : new[] { origin };
                return;
            }

            footprintPositions = new Vector2Int[evaluation.FootprintPositions.Count];
            for (int index = 0; index < footprintPositions.Length; index++)
                footprintPositions[index] = evaluation.FootprintPositions[index];

            if (!result.SpatialValid)
            {
                invalidPositions = (Vector2Int[])footprintPositions.Clone();
                return;
            }

            invalidPositions = Array.Empty<Vector2Int>();
        }

        private void HandlePointerRelease(ConstructionPointerSnapshot pointer)
        {
            if (!pointer.WasReleasedThisFrame)
                return;

            if (_isDraggingPendingPlacement)
            {
                if (VerboseLogs)
                    Debug.Log($"{LogTag} Drag ended at {_draggedPlacementPosition}.");

                SnapPendingPlacementToPointerTile(pointer.Position);
                PublishPendingPlacementDragVisual(pointer.Position, _draggedPlacementPosition, snapToGrid: true);
                _isDraggingPendingPlacement = false;
            }

            if (_isDraggingWallPath)
            {
                _isDraggingWallPath = false;
                _wallDragPendingPositions.Clear();
                _wallHandleController.EndDrag();
                EndWallUndoBatch();

                if (VerboseLogs)
                    Debug.Log($"{LogTag} Wall drag ended at {_lastWallDragTile}.");
            }
        }

        private bool TryResolvePointerTile(Vector2 screenPosition, out Vector2Int tilePosition)
        {
            tilePosition = _screenToGrid.ScreenToGrid(screenPosition);
            return _gridService != null && _gridService.TryGetTileData(tilePosition, out _);
        }

        private Camera ResolveCamera()
        {
            if (_cachedCamera != null && _cachedCamera.isActiveAndEnabled)
                return _cachedCamera;

            _cachedCamera = Camera.main;
            return _cachedCamera != null && _cachedCamera.isActiveAndEnabled
                ? _cachedCamera
                : null;
        }

        private bool TryHandleWallPathDrag(ConstructionPointerSnapshot pointer)
        {
            if (!_isDraggingWallPath || !pointer.IsPressed)
                return false;

            if (IsPointerOverInteractiveUI(pointer.Position, pointer.PointerId))
                return true;

            if (!TryResolvePointerTile(pointer.Position, out Vector2Int dragTilePos))
                return true;

            if (dragTilePos != _lastWallDragTile)
                PreviewWallPathTo(dragTilePos);

            return true;
        }

        private bool TryHandlePendingPlacementDrag(ConstructionPointerSnapshot pointer)
        {
            if (!_isDraggingPendingPlacement || !pointer.IsPressed)
                return false;

            if (IsPointerOverInteractiveUI(pointer.Position, pointer.PointerId))
                return true;

            PublishPendingPlacementDragVisual(pointer.Position, _draggedPlacementPosition, snapToGrid: false);
            return true;
        }

        private void SnapPendingPlacementToPointerTile(Vector2 screenPosition)
        {
            if (TryResolvePendingPlacementSnapTarget(screenPosition, out Vector2Int snappedPosition)
                && TryMoveDraggedPlacementTo(snappedPosition))
            {
                if (VerboseLogs && snappedPosition != _draggedPlacementPosition)
                    Debug.Log($"{LogTag} Drag snapped preview: {_draggedPlacementPosition} -> {snappedPosition}");

                _draggedPlacementPosition = snappedPosition;
            }

            ClearPendingPlacementSnapTarget();
        }

        private bool TryMoveDraggedPlacementTo(Vector2Int targetPosition)
        {
            if (!_gridService.TryGetTileData(targetPosition, out _))
                return false;

            if (targetPosition == _draggedPlacementPosition)
                return true;

            return TryResolveDraggedPlacementBuildingId(out string buildingId)
                && IsBuildGridPlacementAllowed(targetPosition, buildingId, _draggedPlacementPosition)
                && _constructionService.TryMovePendingPlacement(_draggedPlacementPosition, targetPosition);
        }

        private bool IsBuildGridPlacementAllowed(Vector2Int position, string buildingId, Vector2Int? ignoredPendingPosition = null)
        {
            if (_placementQuery == null || string.IsNullOrWhiteSpace(buildingId))
                return false;

            if (_hasPlacementValidationCache
                && _cachedPlacementValidationPosition == position
                && string.Equals(
                    _cachedPlacementValidationBuildingId,
                    buildingId,
                    StringComparison.Ordinal)
                && _cachedPlacementValidationIgnoredPendingPosition == ignoredPendingPosition)
            {
                return _cachedPlacementValidationAllowed;
            }

            bool allowed = _placementQuery.EvaluatePlacement(
                new ConstructionPlacementQueryRequest(
                    buildingId,
                    position,
                    ignoredPendingPosition,
                    includeResources: true,
                    attemptSource:
                        ConstructionPlacementAttemptSource.DragValidation,
                    allowUniquePreviewRelocation: true,
                    rotation: ResolvePlacementRotation(
                        ignoredPendingPosition))).CanPreview;

            _hasPlacementValidationCache = true;
            _cachedPlacementValidationPosition = position;
            _cachedPlacementValidationBuildingId = buildingId;
            _cachedPlacementValidationIgnoredPendingPosition = ignoredPendingPosition;
            _cachedPlacementValidationAllowed = allowed;
            return allowed;
        }

        private ConstructionRotation ResolveSelectedRotation()
            => _constructionService
                is IConstructionRotationService rotationService
                ? rotationService.SelectedRotation
                : ConstructionRotation.Degrees0;

        private ConstructionRotation ResolvePlacementRotation(
            Vector2Int? pendingPosition)
        {
            if (pendingPosition.HasValue
                && _constructionService
                    is IConstructionRotationService rotationService
                && rotationService.TryGetPendingRotation(
                    pendingPosition.Value,
                    out ConstructionRotation pendingRotation))
            {
                return pendingRotation;
            }

            return ResolveSelectedRotation();
        }

        private static ConstructionBuildGridTileVisualState ResolvePlacementVisualState(
            ConstructionPlacementQueryResult result)
        {
            if (!result.CanSelect)
                return ConstructionBuildGridTileVisualState.General;
            if (!result.SpatialValid)
                return ConstructionBuildGridTileVisualState.Invalid;
            return result.ResourcesValid
                ? ConstructionBuildGridTileVisualState.Valid
                : ConstructionBuildGridTileVisualState.Unaffordable;
        }

        private void PublishPendingPlacementDragVisual(Vector2 screenPosition, Vector2Int tilePosition, bool snapToGrid)
        {
            // DragPlacementFix: use the same surface-aware screen mapping as
            // normal construction hover. Grid-plane world projection is only
            // used to obtain smooth XZ cursor following after the target tile
            // has already been resolved.
            string buildingId = _constructionService.GetSelectedBuildingId();
            if (string.IsNullOrWhiteSpace(buildingId)
                && !_constructionService.TryGetPendingBuildingIdAt(tilePosition, out buildingId))
            {
                return;
            }

            Vector2Int snapTargetPosition = tilePosition;
            bool hasSnapTarget = !snapToGrid
                && TryResolveActualPointerTile(
                    screenPosition,
                    out snapTargetPosition);

            Vector2Int pointerSurfaceTile = hasSnapTarget
                ? snapTargetPosition
                : tilePosition;

            Vector3 worldPosition = snapToGrid
                ? Vector3.zero
                : ResolvePointerWorldOnConstructionPlane(
                    screenPosition,
                    pointerSurfaceTile);

            bool isSnapTargetValid = hasSnapTarget
                && IsBuildGridPlacementAllowed(
                    snapTargetPosition,
                    buildingId,
                    tilePosition);

            CachePendingPlacementSnapTarget(
                isSnapTargetValid,
                snapTargetPosition);

            _signalBus.Fire(new BuildingPreviewDragVisualSignal
            {
                Position = tilePosition,
                BuildingId = buildingId,
                WorldPosition = worldPosition,
                SnapToGrid = snapToGrid,
                HasSnapTarget = hasSnapTarget,
                SnapTargetPosition = hasSnapTarget
                    ? snapTargetPosition
                    : tilePosition,
                IsSnapTargetValid = isSnapTargetValid,
            });
        }


        private bool TryResolveActualPointerTile(
            Vector2 screenPosition,
            out Vector2Int tile)
        {
            // DragPlacementFix: ScreenToGridConverter already resolves the
            // nearest generated terrain surface. Do not convert a point from
            // the lower base grid plane back into a cell.
            tile = _screenToGrid.ScreenToGrid(screenPosition);
            return _gridService != null
                && _gridService.TryGetTileData(tile, out _);
        }


        private bool TryResolvePendingPlacementSnapTarget(Vector2 screenPosition, out Vector2Int tile)
        {
            if (_hasPendingPlacementSnapTarget)
            {
                tile = _pendingPlacementSnapTarget;
                return true;
            }

            if (!TryResolveDraggedPlacementBuildingId(out string buildingId))
            {
                tile = default;
                return false;
            }

            // DragPlacementFix: release uses exactly the same terrain-aware
            // tile mapping that was shown while dragging.
            return TryResolveActualPointerTile(screenPosition, out tile)
                && IsBuildGridPlacementAllowed(
                    tile,
                    buildingId,
                    _draggedPlacementPosition);
        }


        private void CachePendingPlacementSnapTarget(bool hasSnapTarget, Vector2Int snapTargetPosition)
        {
            _hasPendingPlacementSnapTarget = hasSnapTarget;
            _pendingPlacementSnapTarget = hasSnapTarget ? snapTargetPosition : default;
        }

        private void ClearPendingPlacementSnapTarget()
        {
            _hasPendingPlacementSnapTarget = false;
            _pendingPlacementSnapTarget = default;
        }

        private bool TryResolveDraggedPlacementBuildingId(out string buildingId)
        {
            buildingId = _constructionService.GetSelectedBuildingId();
            if (!string.IsNullOrWhiteSpace(buildingId))
                return true;

            return _constructionService.TryGetPendingBuildingIdAt(_draggedPlacementPosition, out buildingId);
        }

        private Vector2Int ResolvePointerGridTile(Vector3 pointerWorld, Vector2 screenPosition)
        {
            if (_gridGeometry != null && _gridGeometry.TryGetCellAtWorld(pointerWorld, out Vector2Int tile))
                return tile;

            return _screenToGrid.ScreenToGrid(screenPosition);
        }

        private Vector3 ResolvePointerWorldOnConstructionPlane(Vector2 screenPosition, Vector2Int fallbackTile)
        {
            Camera camera = ResolveCamera();

            float planeY;
            if (_terrainAlignment != null)
            {
                // DragPlacementFix: project the pointer onto the visible
                // terrain surface of the resolved target tile, not the base
                // construction plane underneath elevated terrain.
                planeY = _terrainAlignment
                    .ResolveWorldPosition(fallbackTile, 0f)
                    .y;
            }
            else
            {
                planeY = _gridGeometry != null
                    && _gridGeometry.TryGetGridPlaneY(out float gridPlaneY)
                        ? gridPlaneY
                        : PointerFollowPlaneFallbackY;
            }

            if (camera == null)
                return new Vector3(
                    fallbackTile.x,
                    planeY,
                    fallbackTile.y);

            Ray ray = camera.ScreenPointToRay(screenPosition);
            Plane plane = new(
                Vector3.up,
                new Vector3(0f, planeY, 0f));

            return plane.Raycast(ray, out float distance)
                ? ray.GetPoint(distance)
                : new Vector3(
                    fallbackTile.x,
                    planeY,
                    fallbackTile.y);
        }


    }
}
