using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Обробляє mouse/touch pointer-введення для будівництва.
    /// Реалізує ITickable для прямого читання натискань — TileClickInputService
    /// блокує TileClickedSignal через IsPointerOverGameObject(),
    /// коли Construction UI панелі видимі.
    /// </summary>
    internal sealed class ConstructionInputService : IConstructionInputService, IInitializable, IDisposable, ITickable
    {
        private const float TouchTapMaxMovePixels = 18f;
        private const float TouchTapMaxDurationSeconds = 0.45f;

        private static bool VerboseLogs => Application.isEditor && Debug.isDebugBuild;

        private readonly IConstructionService _constructionService;
        private readonly IWallPlacementService _wallPlacementService;
        private readonly IObjectsMapService _objectsMapService;
        private readonly IScreenToGridConverter _screenToGrid;
        private readonly IGridService _gridService;
        private readonly SignalBus _signalBus;
        private bool _isActive;
        private bool _isDraggingPendingPlacement;
        private Vector2Int _draggedPlacementPosition;
        private bool _isDraggingWallPath;
        private Vector2Int _wallDragStartPosition;
        private Vector2Int _lastWallDragTile;
        private bool _hasTouchWallAnchor;
        private Vector2Int _touchWallAnchorPosition;
        private bool _hasTouchPendingMoveSource;
        private Vector2Int _touchPendingMoveSourcePosition;
        private string _lastObservedSelectedBuildingId;
        private int _trackedTouchId = -1;
        private Vector2 _touchStartScreenPosition;
        private float _touchStartTime;
        private bool _touchStartedOverUi;
        private bool _touchMovedBeyondTap;
        private bool _multiTouchObserved;
        private readonly System.Collections.Generic.HashSet<Vector2Int> _wallDragPendingPositions = new();
        private readonly List<RaycastResult> _uiRaycastResults = new List<RaycastResult>(8);
        private PointerEventData _pointerEventData;

        [Inject]
        public ConstructionInputService(
            IConstructionService constructionService,
            IWallPlacementService wallPlacementService,
            IObjectsMapService objectsMapService,
            IScreenToGridConverter screenToGrid,
            IGridService gridService,
            SignalBus signalBus)
        {
            _constructionService = constructionService;
            _wallPlacementService = wallPlacementService;
            _objectsMapService = objectsMapService;
            _screenToGrid = screenToGrid;
            _gridService = gridService;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<GameModeChangedSignal>(OnGameModeChanged);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<GameModeChangedSignal>(OnGameModeChanged);
        }

        public void Tick()
        {
            if (!_isActive)
                return;

            if (_constructionService.State != BuildingPlacementState.Placing)
            {
                ClearTouchPlacementState();
                _lastObservedSelectedBuildingId = null;
            }
            else
            {
                string selectedBuildingId = _constructionService.GetSelectedBuildingId();
                if (!string.Equals(_lastObservedSelectedBuildingId, selectedBuildingId, StringComparison.Ordinal))
                {
                    ClearTouchPlacementState();
                    _lastObservedSelectedBuildingId = selectedBuildingId;
                }
            }

            if (TryHandleTouchInput())
                return;

            PointerSnapshot pointer = ReadPointerSnapshot();
            if (!pointer.HasPointer)
                return;

            if (pointer.ActiveTouchCount > 1)
            {
                CancelActivePointerDrags();
                return;
            }

            if (pointer.WasReleasedThisFrame)
            {
                if (_isDraggingPendingPlacement)
                {
                    if (VerboseLogs)
                        Debug.Log($"[ConstructionInput] Drag ended at {_draggedPlacementPosition}.");

                    _isDraggingPendingPlacement = false;
                }

                if (_isDraggingWallPath)
                {
                    _isDraggingWallPath = false;
                    _wallDragPendingPositions.Clear();
                    _wallPlacementService.EndDrag();

                    if (VerboseLogs)
                        Debug.Log($"[ConstructionInput] Wall drag ended at {_lastWallDragTile}.");
                }
            }

            if (_isDraggingWallPath && pointer.IsPressed)
            {
                if (IsPointerOverInteractiveUI(pointer.Position, pointer.PointerId))
                    return;

                Vector2 dragScreenPos = pointer.Position;
                Vector2Int dragTilePos = _screenToGrid.ScreenToGrid(dragScreenPos);
                if (!_gridService.TryGetTileData(dragTilePos, out _))
                    return;

                if (dragTilePos != _lastWallDragTile)
                    PreviewWallPathTo(dragTilePos);

                return;
            }

            if (_isDraggingPendingPlacement && pointer.IsPressed)
            {
                if (IsPointerOverInteractiveUI(pointer.Position, pointer.PointerId))
                    return;

                Vector2 dragScreenPos = pointer.Position;
                Vector2Int dragTilePos = _screenToGrid.ScreenToGrid(dragScreenPos);
                if (!_gridService.TryGetTileData(dragTilePos, out _))
                    return;

                if (dragTilePos != _draggedPlacementPosition)
                {
                    bool moved = _constructionService.TryMovePendingPlacement(_draggedPlacementPosition, dragTilePos);
                    if (moved)
                    {
                        if (VerboseLogs)
                            Debug.Log($"[ConstructionInput] Drag moved preview: {_draggedPlacementPosition} -> {dragTilePos}");

                        _draggedPlacementPosition = dragTilePos;
                    }
                }

                return;
            }

            if (!pointer.WasPressedThisFrame)
                return;

            // Пропускаємо кліки на інтерактивних UI елементах (Button, Toggle тощо),
            // але дозволяємо кліки «крізь» фонові панелі Construction UI.
            if (IsPointerOverInteractiveUI(pointer.Position, pointer.PointerId))
            {
                if (VerboseLogs)
                    Debug.Log("[ConstructionInput] Click ignored: pointer over interactive UI.");
                return;
            }

            HandlePointerSelection(pointer.Position, pointer.PointerId, allowDragStart: true, touchMode: false);
        }

        private bool TryHandleTouchInput()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null)
                return false;

            int pressedTouchCount = CountPressedTouches(touchscreen);
            if (pressedTouchCount > 1)
            {
                _multiTouchObserved = true;
                CancelActivePointerDrags();
                ResetTouchTracking();
                return true;
            }

            var touches = touchscreen.touches;
            for (int touchIndex = 0; touchIndex < touches.Count; touchIndex++)
            {
                TouchControl touch = touches[touchIndex];
                int touchId = touch.touchId.ReadValue();

                if (_trackedTouchId < 0 && touch.press.wasPressedThisFrame)
                {
                    BeginTouchTracking(touch, touchId);
                    return true;
                }

                if (touchId != _trackedTouchId)
                    continue;

                if (touch.press.isPressed)
                {
                    Vector2 currentPosition = touch.position.ReadValue();
                    if ((currentPosition - _touchStartScreenPosition).sqrMagnitude > TouchTapMaxMovePixels * TouchTapMaxMovePixels)
                        _touchMovedBeyondTap = true;

                    return true;
                }

                if (touch.press.wasReleasedThisFrame)
                {
                    CompleteTouchTracking(touch.position.ReadValue(), touchId);
                    return true;
                }
            }

            if (pressedTouchCount > 0)
                return true;

            if (_trackedTouchId >= 0)
                ResetTouchTracking();

            return false;
        }

        private void BeginTouchTracking(TouchControl touch, int touchId)
        {
            _trackedTouchId = touchId;
            _touchStartScreenPosition = touch.position.ReadValue();
            _touchStartTime = Time.unscaledTime;
            _touchStartedOverUi = IsPointerOverInteractiveUI(_touchStartScreenPosition, touchId);
            _touchMovedBeyondTap = false;
            _multiTouchObserved = CountPressedTouches(Touchscreen.current) > 1;
        }

        private void CompleteTouchTracking(Vector2 releaseScreenPosition, int touchId)
        {
            bool isTap = !_touchStartedOverUi
                && !_touchMovedBeyondTap
                && !_multiTouchObserved
                && Time.unscaledTime - _touchStartTime <= TouchTapMaxDurationSeconds;

            if (isTap)
                HandlePointerSelection(releaseScreenPosition, touchId, allowDragStart: false, touchMode: true);

            ResetTouchTracking();
        }

        private void ResetTouchTracking()
        {
            _trackedTouchId = -1;
            _touchStartScreenPosition = Vector2.zero;
            _touchStartTime = 0f;
            _touchStartedOverUi = false;
            _touchMovedBeyondTap = false;
            _multiTouchObserved = false;
        }

        private static int CountPressedTouches(Touchscreen touchscreen)
        {
            if (touchscreen == null)
                return 0;

            int pressedTouchCount = 0;
            var touches = touchscreen.touches;
            for (int touchIndex = 0; touchIndex < touches.Count; touchIndex++)
            {
                if (touches[touchIndex].press.isPressed)
                    pressedTouchCount++;
            }

            return pressedTouchCount;
        }

        private void HandlePointerSelection(Vector2 screenPos, int pointerId, bool allowDragStart, bool touchMode)
        {
            if (IsPointerOverInteractiveUI(screenPos, pointerId))
            {
                if (VerboseLogs)
                    Debug.Log("[ConstructionInput] Click ignored: pointer over interactive UI.");
                return;
            }

            Vector2Int tilePos = _screenToGrid.ScreenToGrid(screenPos);

            if (VerboseLogs)
            {
                string inputKind = touchMode ? "Tap" : "Click";
                Debug.Log(
                    $"[ConstructionInput] {inputKind} screen={screenPos}, tile={tilePos}, " +
                    $"state={_constructionService.State}, demolish={_constructionService.IsDemolishMode}");
            }

            if (!_gridService.TryGetTileData(tilePos, out _))
            {
                if (VerboseLogs)
                    Debug.LogWarning($"[ConstructionInput] Tile {tilePos} is outside grid. Click ignored.");
                return;
            }

            if (_constructionService.IsDemolishMode && _constructionService.State != BuildingPlacementState.Placing)
            {
                ClearTouchPlacementState();
                bool result = _constructionService.TryDemolishAt(tilePos);
                if (VerboseLogs)
                    Debug.Log($"[ConstructionInput] TryDemolishAt({tilePos}) => {result}");
                return;
            }

            if (_constructionService.State == BuildingPlacementState.Placing)
            {
                HandlePlacementSelection(tilePos, allowDragStart, touchMode);
                return;
            }

            if (VerboseLogs)
                Debug.Log($"[ConstructionInput] Click ignored: placement state is {_constructionService.State}.");
        }

        private void HandlePlacementSelection(Vector2Int tilePos, bool allowDragStart, bool touchMode)
        {
            string selectedBuildingId = _constructionService.GetSelectedBuildingId();
            bool wallMode = !string.IsNullOrWhiteSpace(selectedBuildingId) && _wallPlacementService.IsWall(selectedBuildingId);

            if (touchMode)
            {
                HandleTouchPlacementSelection(tilePos, selectedBuildingId, wallMode);
                return;
            }

            if (wallMode && _objectsMapService.TryGetOccupant(tilePos, out var occupantId) && _wallPlacementService.IsWallOrGate(occupantId))
            {
                _wallPlacementService.ShowWallHandles(tilePos);
                _isDraggingWallPath = allowDragStart;
                _wallDragStartPosition = tilePos;
                _lastWallDragTile = tilePos;
                _wallDragPendingPositions.Clear();
                _wallDragPendingPositions.Add(tilePos);

                if (VerboseLogs)
                    Debug.Log($"[ConstructionInput] Wall drag started from existing segment at {tilePos}");

                return;
            }

            if (_constructionService.HasPendingPlacementAt(tilePos))
            {
                bool gateMode = !string.IsNullOrWhiteSpace(selectedBuildingId)
                    && _wallPlacementService.IsGate(selectedBuildingId);
                if (gateMode)
                {
                    bool placed = _constructionService.TryPreviewAt(tilePos);
                    if (VerboseLogs)
                        Debug.Log($"[ConstructionInput] Gate placement on pending tile {tilePos} => {placed}");
                    return;
                }

                _isDraggingPendingPlacement = allowDragStart;
                _draggedPlacementPosition = tilePos;

                if (VerboseLogs)
                    Debug.Log($"[ConstructionInput] Drag started for preview at {tilePos}");

                return;
            }

            if (wallMode)
            {
                bool placed = _constructionService.TryPreviewAt(tilePos);
                if (placed && allowDragStart)
                {
                    _isDraggingWallPath = true;
                    _wallDragStartPosition = tilePos;
                    _lastWallDragTile = tilePos;
                    _wallDragPendingPositions.Clear();
                    _wallDragPendingPositions.Add(tilePos);

                    if (VerboseLogs)
                        Debug.Log($"[ConstructionInput] Wall drag started from empty tile at {tilePos}");
                }

                return;
            }

            bool result = _constructionService.TryPreviewAt(tilePos);
            if (VerboseLogs)
                Debug.Log($"[ConstructionInput] TryPreviewAt({tilePos}) => {result}");

            if (result && allowDragStart)
            {
                _isDraggingPendingPlacement = true;
                _draggedPlacementPosition = tilePos;
            }
        }

        private void HandleTouchPlacementSelection(Vector2Int tilePos, string selectedBuildingId, bool wallMode)
        {
            bool gateMode = !string.IsNullOrWhiteSpace(selectedBuildingId)
                && _wallPlacementService.IsGate(selectedBuildingId);

            if (gateMode)
            {
                ClearTouchPendingMoveSource();
                ClearTouchWallAnchor();
                bool gatePlaced = _constructionService.TryPreviewAt(tilePos);
                if (VerboseLogs)
                    Debug.Log($"[ConstructionInput] Touch gate placement at {tilePos} => {gatePlaced}");
                return;
            }

            if (wallMode)
            {
                ClearTouchPendingMoveSource();
                HandleTouchWallSelection(tilePos);
                return;
            }

            ClearTouchWallAnchor();

            if (_hasTouchPendingMoveSource)
            {
                if (_constructionService.HasPendingPlacementAt(tilePos))
                {
                    _touchPendingMoveSourcePosition = tilePos;
                    if (VerboseLogs)
                        Debug.Log($"[ConstructionInput] Touch move source changed to {tilePos}.");
                    return;
                }

                bool moved = _constructionService.TryMovePendingPlacement(_touchPendingMoveSourcePosition, tilePos);
                if (VerboseLogs)
                    Debug.Log($"[ConstructionInput] Touch move preview {_touchPendingMoveSourcePosition} -> {tilePos} => {moved}");

                if (moved)
                    ClearTouchPendingMoveSource();

                return;
            }

            if (_constructionService.HasPendingPlacementAt(tilePos))
            {
                _hasTouchPendingMoveSource = true;
                _touchPendingMoveSourcePosition = tilePos;

                if (VerboseLogs)
                    Debug.Log($"[ConstructionInput] Touch move source selected at {tilePos}.");

                return;
            }

            bool placed = _constructionService.TryPreviewAt(tilePos);
            if (VerboseLogs)
                Debug.Log($"[ConstructionInput] Touch preview at {tilePos} => {placed}");
        }

        private void HandleTouchWallSelection(Vector2Int tilePos)
        {
            if (_hasTouchWallAnchor)
            {
                if (tilePos != _lastWallDragTile)
                    PreviewWallPathTo(tilePos);

                if (VerboseLogs)
                    Debug.Log($"[ConstructionInput] Touch wall path completed: {_touchWallAnchorPosition} -> {tilePos}");

                ClearTouchWallAnchor();
                return;
            }

            bool startsFromExistingWall = _objectsMapService.TryGetOccupant(tilePos, out var occupantId)
                && _wallPlacementService.IsWallOrGate(occupantId);
            bool startsFromPendingWall = _constructionService.TryGetPendingBuildingIdAt(tilePos, out var pendingId)
                && _wallPlacementService.IsWallOrGate(pendingId);

            if (startsFromExistingWall)
            {
                _wallPlacementService.ShowWallHandles(tilePos);
            }
            else if (!startsFromPendingWall)
            {
                bool placed = _constructionService.TryPreviewAt(tilePos);
                if (!placed)
                {
                    if (VerboseLogs)
                        Debug.Log($"[ConstructionInput] Touch wall anchor at {tilePos} rejected.");
                    return;
                }
            }

            _hasTouchWallAnchor = true;
            _touchWallAnchorPosition = tilePos;
            _wallDragStartPosition = tilePos;
            _lastWallDragTile = tilePos;
            _wallDragPendingPositions.Clear();
            _wallDragPendingPositions.Add(tilePos);

            if (VerboseLogs)
                Debug.Log($"[ConstructionInput] Touch wall anchor set at {tilePos}.");
        }

        private void PreviewWallPathTo(Vector2Int targetTile)
        {
            var newPath = _wallPlacementService.BuildPath(_wallDragStartPosition, targetTile);
            var newPathSet = new System.Collections.Generic.HashSet<Vector2Int>(newPath);

            var toRemove = new System.Collections.Generic.List<Vector2Int>();
            foreach (var pos in _wallDragPendingPositions)
            {
                if (!newPathSet.Contains(pos))
                    toRemove.Add(pos);
            }
            foreach (var pos in toRemove)
            {
                _constructionService.RemovePendingAt(pos);
                _wallDragPendingPositions.Remove(pos);
            }

            for (int i = 0; i < newPath.Count; i++)
            {
                var tile = newPath[i];

                if (!_constructionService.HasPendingPlacementAt(tile))
                {
                    if (!_objectsMapService.TryGetOccupant(tile, out var occupantId) || !_wallPlacementService.IsWallOrGate(occupantId))
                    {
                        bool placed = _constructionService.TryPreviewAt(tile);
                        if (!placed)
                            break;
                    }

                    _wallDragPendingPositions.Add(tile);
                }
                else if (_constructionService.TryGetPendingBuildingIdAt(tile, out var existingPendingId)
                         && _wallPlacementService.IsWallOrGate(existingPendingId))
                {
                    _wallDragPendingPositions.Add(tile);
                }
            }

            _lastWallDragTile = targetTile;
        }

        private PointerSnapshot ReadPointerSnapshot()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                TouchControl activeTouch = null;
                TouchControl pressedTouch = null;
                TouchControl releasedTouch = null;
                int activeTouchCount = 0;

                var touches = touchscreen.touches;
                for (int touchIndex = 0; touchIndex < touches.Count; touchIndex++)
                {
                    TouchControl touch = touches[touchIndex];
                    if (touch.press.isPressed)
                    {
                        activeTouchCount++;
                        activeTouch ??= touch;
                    }

                    if (pressedTouch == null && touch.press.wasPressedThisFrame)
                        pressedTouch = touch;

                    if (releasedTouch == null && touch.press.wasReleasedThisFrame)
                        releasedTouch = touch;
                }

                TouchControl selectedTouch = activeTouch ?? releasedTouch ?? pressedTouch;
                if (selectedTouch != null)
                {
                    return new PointerSnapshot(
                        hasPointer: true,
                        wasPressedThisFrame: selectedTouch.press.wasPressedThisFrame,
                        wasReleasedThisFrame: selectedTouch.press.wasReleasedThisFrame,
                        isPressed: selectedTouch.press.isPressed,
                        position: selectedTouch.position.ReadValue(),
                        pointerId: selectedTouch.touchId.ReadValue(),
                        activeTouchCount: activeTouchCount);
                }
            }

            var mouse = Mouse.current;
            if (mouse == null)
                return default;

            return new PointerSnapshot(
                hasPointer: true,
                wasPressedThisFrame: mouse.leftButton.wasPressedThisFrame,
                wasReleasedThisFrame: mouse.leftButton.wasReleasedThisFrame,
                isPressed: mouse.leftButton.isPressed,
                position: mouse.position.ReadValue(),
                pointerId: -1,
                activeTouchCount: 0);
        }

        private bool IsPointerOverInteractiveUI(Vector2 screenPosition, int pointerId)
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
                return false;

            // Курсор над UI — перевіряємо чи є під ним інтерактивний елемент
            _pointerEventData ??= new PointerEventData(eventSystem);
            _pointerEventData.Reset();
            _pointerEventData.pointerId = pointerId;
            _pointerEventData.position = screenPosition;

            _uiRaycastResults.Clear();
            eventSystem.RaycastAll(_pointerEventData, _uiRaycastResults);

            for (int resultIndex = 0; resultIndex < _uiRaycastResults.Count; resultIndex++)
            {
                if (_uiRaycastResults[resultIndex].gameObject.GetComponentInParent<Selectable>() != null)
                    return true;
            }

            return false;
        }

        private void CancelActivePointerDrags()
        {
            _isDraggingPendingPlacement = false;

            if (_isDraggingWallPath)
            {
                _wallPlacementService.EndDrag();
                _wallDragPendingPositions.Clear();
            }

            _isDraggingWallPath = false;
        }

        private void CancelActiveDrags()
        {
            CancelActivePointerDrags();
            ClearTouchPlacementState();
            ResetTouchTracking();
        }

        private void ClearTouchPlacementState()
        {
            ClearTouchPendingMoveSource();
            ClearTouchWallAnchor();
        }

        private void ClearTouchPendingMoveSource()
        {
            _hasTouchPendingMoveSource = false;
            _touchPendingMoveSourcePosition = default;
        }

        private void ClearTouchWallAnchor()
        {
            if (_hasTouchWallAnchor)
                _wallPlacementService.EndDrag();

            _hasTouchWallAnchor = false;
            _touchWallAnchorPosition = default;

            if (!_isDraggingWallPath)
                _wallDragPendingPositions.Clear();
        }

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            _isActive = signal.NewMode == GameModeType.Construction;
            if (!_isActive)
            {
                CancelActiveDrags();
            }

            if (VerboseLogs)
                Debug.Log($"[ConstructionInput] Active changed -> {_isActive}");
        }

        public void OnUndoRequested() => _constructionService.UndoLast();

        public void OnRedoRequested() => _constructionService.RedoLast();

        private readonly struct PointerSnapshot
        {
            public readonly bool HasPointer;
            public readonly bool WasPressedThisFrame;
            public readonly bool WasReleasedThisFrame;
            public readonly bool IsPressed;
            public readonly Vector2 Position;
            public readonly int PointerId;
            public readonly int ActiveTouchCount;

            public PointerSnapshot(
                bool hasPointer,
                bool wasPressedThisFrame,
                bool wasReleasedThisFrame,
                bool isPressed,
                Vector2 position,
                int pointerId,
                int activeTouchCount)
            {
                HasPointer = hasPointer;
                WasPressedThisFrame = wasPressedThisFrame;
                WasReleasedThisFrame = wasReleasedThisFrame;
                IsPressed = isPressed;
                Position = position;
                PointerId = pointerId;
                ActiveTouchCount = activeTouchCount;
            }
        }
    }
}
