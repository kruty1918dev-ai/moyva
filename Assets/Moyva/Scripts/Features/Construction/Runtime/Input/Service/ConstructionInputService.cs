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
            if (VerboseLogs)
                Debug.Log($"{LogTag} Initialized.");
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
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
                return;

            if (TryHandleReleaseSelectionInput(pointer))
                return;

            if (pointer.ActivePointerCount > 1)
            {
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
                _lastObservedSelectedBuildingId = selectedBuildingId;
            }
        }

        private void HandlePointerRelease(ConstructionPointerSnapshot pointer)
        {
            if (!pointer.WasReleasedThisFrame)
                return;

            if (_isDraggingPendingPlacement)
            {
                if (VerboseLogs)
                    Debug.Log($"{LogTag} Drag ended at {_draggedPlacementPosition}.");

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
            if (TryResolvePointerTarget(screenPosition, out var pointerTarget))
            {
                tilePosition = pointerTarget.TilePosition;
                return true;
            }

            tilePosition = _screenToGrid.ScreenToGrid(screenPosition);
            return true;
        }

        private bool TryResolvePointerTarget(Vector2 screenPosition, out ConstructionBuildingPointerTarget pointerTarget)
        {
            pointerTarget = null;

            Camera camera = ResolveCamera();
            if (camera == null)
                return false;

            Ray ray = camera.ScreenPointToRay(screenPosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, float.PositiveInfinity, ~0, QueryTriggerInteraction.Collide);
            if (hits == null || hits.Length == 0)
                return false;

            float nearestDistance = float.PositiveInfinity;
            ConstructionBuildingPointerTarget nearestTarget = null;
            for (int i = 0; i < hits.Length; i++)
            {
                var hitCollider = hits[i].collider;
                if (hitCollider == null)
                    continue;

                var candidate = hitCollider.GetComponentInParent<ConstructionBuildingPointerTarget>();
                if (candidate == null)
                    continue;

                if (hits[i].distance < nearestDistance)
                {
                    nearestDistance = hits[i].distance;
                    nearestTarget = candidate;
                }
            }

            pointerTarget = nearestTarget;
            return pointerTarget != null;
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

            TryResolvePointerTile(pointer.Position, out Vector2Int dragTilePos);
            if (!_gridService.TryGetTileData(dragTilePos, out _))
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

            TryResolvePointerTile(pointer.Position, out Vector2Int dragTilePos);
            if (!_gridService.TryGetTileData(dragTilePos, out _))
                return true;

            if (dragTilePos != _draggedPlacementPosition)
            {
                bool moved = _constructionService.TryMovePendingPlacement(_draggedPlacementPosition, dragTilePos);
                if (moved)
                {
                    if (VerboseLogs)
                        Debug.Log($"{LogTag} Drag moved preview: {_draggedPlacementPosition} -> {dragTilePos}");

                    _draggedPlacementPosition = dragTilePos;
                }
            }

            return true;
        }
    }
}
