using System;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Обробляє введення миші для будівництва.
    /// Реалізує ITickable для прямого читання кліків — TileClickInputService
    /// блокує TileClickedSignal через IsPointerOverGameObject(),
    /// коли Construction UI панелі видимі.
    /// </summary>
    internal sealed class ConstructionInputService : IConstructionInputService, IInitializable, IDisposable, ITickable
    {
        private const bool VerboseLogs = true;

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

            var mouse = Mouse.current;
            if (mouse == null)
                return;

            if (mouse.leftButton.wasReleasedThisFrame)
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
                    _wallPlacementService.EndDrag();

                    if (VerboseLogs)
                        Debug.Log($"[ConstructionInput] Wall drag ended at {_lastWallDragTile}.");
                }
            }

            if (_isDraggingWallPath && mouse.leftButton.isPressed)
            {
                if (IsClickOnInteractiveUI())
                    return;

                Vector2 dragScreenPos = mouse.position.ReadValue();
                Vector2Int dragTilePos = _screenToGrid.ScreenToGrid(dragScreenPos);
                if (!_gridService.TryGetTileData(dragTilePos, out _))
                    return;

                if (dragTilePos != _lastWallDragTile)
                {
                    var path = _wallPlacementService.BuildPath(_wallDragStartPosition, dragTilePos);
                    for (int i = 0; i < path.Count; i++)
                        _constructionService.TryPreviewAt(path[i]);

                    _lastWallDragTile = dragTilePos;
                }

                return;
            }

            if (_isDraggingPendingPlacement && mouse.leftButton.isPressed)
            {
                if (IsClickOnInteractiveUI())
                    return;

                Vector2 dragScreenPos = mouse.position.ReadValue();
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

            if (!mouse.leftButton.wasPressedThisFrame)
                return;

            // Пропускаємо кліки на інтерактивних UI елементах (Button, Toggle тощо),
            // але дозволяємо кліки «крізь» фонові панелі Construction UI.
            if (IsClickOnInteractiveUI())
            {
                if (VerboseLogs)
                    Debug.Log("[ConstructionInput] Click ignored: pointer over interactive UI.");
                return;
            }

            Vector2 screenPos = mouse.position.ReadValue();
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

            if (_constructionService.IsDemolishMode)
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

                    if (VerboseLogs)
                        Debug.Log($"[ConstructionInput] Wall drag started from existing segment at {tilePos}");

                    return;
                }

                if (_constructionService.HasPendingPlacementAt(tilePos))
                {
                    _isDraggingPendingPlacement = true;
                    _draggedPlacementPosition = tilePos;

                    if (VerboseLogs)
                        Debug.Log($"[ConstructionInput] Drag started for preview at {tilePos}");

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

        private static bool IsClickOnInteractiveUI()
        {
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            if (eventSystem == null)
                return false;

            if (!eventSystem.IsPointerOverGameObject())
                return false;

            // Курсор над UI — перевіряємо чи є під ним інтерактивний елемент
            var pointer = new UnityEngine.EventSystems.PointerEventData(eventSystem)
            {
                position = Mouse.current.position.ReadValue()
            };
            var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            eventSystem.RaycastAll(pointer, results);

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].gameObject.GetComponentInParent<Selectable>() != null)
                    return true;
            }

            return false;
        }

        private void OnGameModeChanged(GameModeChangedSignal signal)
        {
            _isActive = signal.NewMode == GameModeType.Construction;
            if (!_isActive)
            {
                _isDraggingPendingPlacement = false;
                _isDraggingWallPath = false;
                _wallPlacementService.EndDrag();
            }

            if (VerboseLogs)
                Debug.Log($"[ConstructionInput] Active changed -> {_isActive}");
        }

        public void OnUndoRequested() => _constructionService.UndoLast();

        public void OnRedoRequested() => _constructionService.RedoLast();
    }
}
