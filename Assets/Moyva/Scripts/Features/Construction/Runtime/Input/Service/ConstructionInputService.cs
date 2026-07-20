using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Обробляє pointer-введення для будівництва через абстрактне джерело вводу.
    /// Реалізує ITickable для прямого читання натискань — TileClickInputService
    /// блокує TileClickedSignal через IsPointerOverGameObject(),
    /// коли Construction UI панелі видимі.
    /// </summary>
    internal sealed partial class ConstructionInputService : IConstructionInputService, IInitializable, IDisposable, ITickable
    {
        private const string LogTag = "[ConstructionInput]";
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
        private readonly IConstructionPlacementQuery _placementQuery;
        private readonly BuildModeGridStateController _buildGridState;
        private readonly IConstructionBuildGridDiagnostics _buildGridDiagnostics;
        private readonly IConstructionInteractiveUiHitTester _uiHitTester;
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
            SignalBus signalBus)
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
            _signalBus = signalBus;
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
            if (VerboseLogs)
                Debug.Log($"{LogTag} Initialized.");
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
            _signalBus.TryUnsubscribe<BuildingSelectionChangedSignal>(InvalidateBuildGridHover);
            _signalBus.TryUnsubscribe<BuildingPreviewChangedSignal>(InvalidateBuildGridHover);
            _signalBus.TryUnsubscribe<BuildingPreviewMovedSignal>(InvalidateBuildGridHover);
            _signalBus.TryUnsubscribe<OnObjectsMapChangedSignal>(InvalidateBuildGridHover);
            _signalBus.TryUnsubscribe<GridTileChangedSignal>(InvalidateBuildGridHover);
            _signalBus.TryUnsubscribe<FogStateChangedSignal>(InvalidateBuildGridHover);
            _signalBus.TryUnsubscribe<SettlementResourceChangedSignal>(InvalidateBuildGridHover);
            if (VerboseLogs)
                Debug.Log($"{LogTag} Disposed.");
        }

        public void Tick()
        {
            if (!_isActive)
                return;

            RefreshPlacementSelectionState();

            ConstructionPointerSnapshot pointer = ReadPointerSnapshot();
            if (!pointer.HasPointer)
            {
                ClearBuildGridHover();
                return;
            }

            UpdateBuildGridHover(pointer);

            if (TryHandleReleaseSelectionInput(pointer))
                return;

            if (pointer.ActivePointerCount > 1)
            {
                ClearBuildGridHover();
                CancelActivePointerDrags();
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
                _lastObservedSelectedBuildingId = null;
                return;
            }

            string selectedBuildingId = _constructionService.GetSelectedBuildingId();
            if (!string.Equals(_lastObservedSelectedBuildingId, selectedBuildingId, StringComparison.Ordinal))
            {
                ClearTouchPlacementState();
                ClearBuildGridHover();
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
                    new[] { tile },
                    Array.Empty<Vector2Int>());
                return;
            }

            if (_buildGridState.HoverPosition == tile)
                return;

            ConstructionPlacementQueryResult summary = _placementQuery.EvaluatePlacement(
                new ConstructionPlacementQueryRequest(
                    buildingId,
                    tile,
                    includeResources: true,
                    attemptSource:
                        ConstructionPlacementAttemptSource.PointerHover,
                    allowUniquePreviewRelocation: true));
            ConstructionBuildGridTileVisualState visualState = summary.IsValid
                ? ConstructionBuildGridTileVisualState.Valid
                : ConstructionBuildGridTileVisualState.Invalid;
            if (!_buildGridState.SetHover(tile, visualState))
                return;

            ConstructionPlacementQueryResult detailed = _placementQuery.EvaluatePlacement(
                new ConstructionPlacementQueryRequest(
                    buildingId,
                    tile,
                    includeResources: true,
                    includeDetails: true,
                    attemptSource:
                        ConstructionPlacementAttemptSource.PointerHover,
                    allowUniquePreviewRelocation: true));
            BuildHoverFootprintArrays(
                tile,
                detailed,
                out Vector2Int[] footprintPositions,
                out Vector2Int[] invalidPositions);
            PublishBuildGridHover(
                tile,
                buildingId,
                detailed.IsValid,
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

        private void InvalidateBuildGridHover(BuildingSelectionChangedSignal _) => ClearBuildGridHover();
        private void InvalidateBuildGridHover(BuildingPreviewChangedSignal _) => ClearBuildGridHover();
        private void InvalidateBuildGridHover(BuildingPreviewMovedSignal _) => ClearBuildGridHover();
        private void InvalidateBuildGridHover(OnObjectsMapChangedSignal _) => ClearBuildGridHover();
        private void InvalidateBuildGridHover(GridTileChangedSignal _) => ClearBuildGridHover();
        private void InvalidateBuildGridHover(FogStateChangedSignal _) => ClearBuildGridHover();
        private void InvalidateBuildGridHover(SettlementResourceChangedSignal _) => ClearBuildGridHover();

        private void PublishBuildGridHover(
            Vector2Int tile,
            string buildingId,
            bool isPlacementValid,
            Vector2Int[] footprintPositions,
            Vector2Int[] invalidPositions)
        {
            _signalBus.Fire(new BuildGridHoverChangedSignal
            {
                HasTile = true,
                Position = tile,
                BuildingId = buildingId,
                IsPlacementValid = isPlacementValid,
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
                invalidPositions = result.IsValid
                    ? Array.Empty<Vector2Int>()
                    : new[] { origin };
                return;
            }

            footprintPositions = new Vector2Int[evaluation.FootprintPositions.Count];
            for (int index = 0; index < footprintPositions.Length; index++)
                footprintPositions[index] = evaluation.FootprintPositions[index];

            if (!result.IsValid)
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

            return _placementQuery.EvaluatePlacement(
                new ConstructionPlacementQueryRequest(
                    buildingId,
                    position,
                    ignoredPendingPosition,
                    includeResources: true,
                    attemptSource:
                        ConstructionPlacementAttemptSource.DragValidation,
                    allowUniquePreviewRelocation: true)).IsValid;
        }

        private void PublishPendingPlacementDragVisual(Vector2 screenPosition, Vector2Int tilePosition, bool snapToGrid)
        {
            string buildingId = _constructionService.GetSelectedBuildingId();
            if (string.IsNullOrWhiteSpace(buildingId)
                && !_constructionService.TryGetPendingBuildingIdAt(tilePosition, out buildingId))
            {
                return;
            }

            Vector3 worldPosition = snapToGrid
                ? Vector3.zero
                : ResolvePointerWorldOnConstructionPlane(screenPosition, tilePosition);
            Vector2Int snapTargetPosition = tilePosition;
            bool hasSnapTarget = !snapToGrid
                && TryResolveActualPointerTile(worldPosition, screenPosition, out snapTargetPosition);
            bool isSnapTargetValid = hasSnapTarget
                && IsBuildGridPlacementAllowed(snapTargetPosition, buildingId, tilePosition);
            CachePendingPlacementSnapTarget(isSnapTargetValid, snapTargetPosition);

            _signalBus.Fire(new BuildingPreviewDragVisualSignal
            {
                Position = tilePosition,
                BuildingId = buildingId,
                WorldPosition = worldPosition,
                SnapToGrid = snapToGrid,
                HasSnapTarget = hasSnapTarget,
                SnapTargetPosition = hasSnapTarget ? snapTargetPosition : tilePosition,
                IsSnapTargetValid = isSnapTargetValid,
            });
        }

        private bool TryResolveActualPointerTile(
            Vector3 pointerWorld,
            Vector2 screenPosition,
            out Vector2Int tile)
        {
            tile = ResolvePointerGridTile(pointerWorld, screenPosition);
            return _gridService != null && _gridService.TryGetTileData(tile, out _);
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

            Vector3 pointerWorld = ResolvePointerWorldOnConstructionPlane(screenPosition, _draggedPlacementPosition);
            return TryResolveActualPointerTile(pointerWorld, screenPosition, out tile)
                && IsBuildGridPlacementAllowed(tile, buildingId, _draggedPlacementPosition);
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
            if (camera == null)
                return new Vector3(fallbackTile.x, 0f, fallbackTile.y);

            Ray ray = camera.ScreenPointToRay(screenPosition);
            float planeY = _gridGeometry != null && _gridGeometry.TryGetGridPlaneY(out float gridPlaneY)
                ? gridPlaneY
                : PointerFollowPlaneFallbackY;
            Plane plane = new(Vector3.up, new Vector3(0f, planeY, 0f));
            return plane.Raycast(ray, out float distance)
                ? ray.GetPoint(distance)
                : new Vector3(fallbackTile.x, 0f, fallbackTile.y);
        }

    }
}
