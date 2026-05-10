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

            PointerSnapshot pointer = ReadPointerSnapshot();
            if (!pointer.HasPointer)
                return;

            if (pointer.ActiveTouchCount > 1)
            {
                CancelActiveDrags();
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
                {
                    var newPath = _wallPlacementService.BuildPath(_wallDragStartPosition, dragTilePos);
                    var newPathSet = new System.Collections.Generic.HashSet<Vector2Int>(newPath);

                    // Видаляємо позиції, які були в старому шляху, але не в новому
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

                    // Додаємо нові позиції шляху
                    for (int i = 0; i < newPath.Count; i++)
                    {
                        var tile = newPath[i];

                        if (!_constructionService.HasPendingPlacementAt(tile))
                        {
                            // Якщо на тайлі вже стоїть стіна/ворота — це валідна частина лінії, просто проходимо далі.
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
                            // Тайл вже має pending-стіну/ворота — відстежуємо для можливого видалення.
                            _wallDragPendingPositions.Add(tile);
                        }
                        // Non-wall pending (ратуша, склад тощо) — ігноруємо, не додаємо до сету.
                    }

                    _lastWallDragTile = dragTilePos;
                }

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

            Vector2 screenPos = pointer.Position;
            Vector2Int tilePos = _screenToGrid.ScreenToGrid(screenPos);

            if (VerboseLogs)
            {
                Debug.Log(
                    $"[ConstructionInput] Click screen={screenPos}, tile={tilePos}, " +
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
                bool result = _constructionService.TryDemolishAt(tilePos);
                if (VerboseLogs)
                    Debug.Log($"[ConstructionInput] TryDemolishAt({tilePos}) => {result}");
                return;
            }

            if (_constructionService.State == BuildingPlacementState.Placing)
            {
                string selectedBuildingId = _constructionService.GetSelectedBuildingId();
                bool wallMode = !string.IsNullOrWhiteSpace(selectedBuildingId) && _wallPlacementService.IsWall(selectedBuildingId);

                if (wallMode && _objectsMapService.TryGetOccupant(tilePos, out var occupantId) && _wallPlacementService.IsWallOrGate(occupantId))
                {
                    _wallPlacementService.ShowWallHandles(tilePos);
                    _isDraggingWallPath = true;
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
                        // Ворота: клік по pending-стіні → замінюємо стіну на ворота
                        bool gateMode = !string.IsNullOrWhiteSpace(selectedBuildingId)
                            && _wallPlacementService.IsGate(selectedBuildingId);
                        if (gateMode)
                        {
                            bool placed = _constructionService.TryPreviewAt(tilePos);
                            if (VerboseLogs)
                                Debug.Log($"[ConstructionInput] Gate placement on pending tile {tilePos} => {placed}");
                            return;
                        }

                        _isDraggingPendingPlacement = true;
                        _draggedPlacementPosition = tilePos;

                        if (VerboseLogs)
                            Debug.Log($"[ConstructionInput] Drag started for preview at {tilePos}");

                        return;
                }

                // Wall mode — drag від порожнього тайлу: ставимо перший сегмент (точка А) і починаємо drag
                if (wallMode)
                {
                    bool placed = _constructionService.TryPreviewAt(tilePos);
                    if (placed)
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

                if (result)
                {
                    _isDraggingPendingPlacement = true;
                    _draggedPlacementPosition = tilePos;
                }
            }
            else if (VerboseLogs)
            {
                Debug.Log($"[ConstructionInput] Click ignored: placement state is {_constructionService.State}.");
            }
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

        private void CancelActiveDrags()
        {
            _isDraggingPendingPlacement = false;

            if (_isDraggingWallPath)
                _wallPlacementService.EndDrag();

            _isDraggingWallPath = false;
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
