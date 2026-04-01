using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    /// <summary>
    /// Реалізація системи будівництва. Керує pending-будівлями, undo/redo-стеками
    /// та обробляє клавіші Ctrl+Z / Ctrl+Y у режимі будівництва.
    /// </summary>
    internal sealed class BuildingConstructionService
        : IBuildingConstructionService, IInitializable, IDisposable, ITickable
    {
        private readonly IGridService _gridService;
        private readonly IObjectsMapService _objectsMapService;
        private readonly SignalBus _signalBus;

        private readonly List<PendingBuilding> _pendingBuildings = new();
        private readonly Stack<PendingBuilding> _undoStack = new();
        private readonly Stack<PendingBuilding> _redoStack = new();

        private int _buildingCounter;

        public bool IsInConstructionMode { get; private set; }
        public string SelectedBuildingTypeId { get; private set; }
        public IReadOnlyList<PendingBuilding> PendingBuildings => _pendingBuildings;

        public BuildingConstructionService(
            IGridService gridService,
            IObjectsMapService objectsMapService,
            SignalBus signalBus)
        {
            _gridService = gridService;
            _objectsMapService = objectsMapService;
            _signalBus = signalBus;
        }

        public void Initialize() { }
        public void Dispose() { }

        // ── Keyboard shortcuts (Ctrl+Z / Ctrl+Y) ────────────────────────────

        public void Tick()
        {
            if (!IsInConstructionMode) return;

            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            if (!ctrl) return;

            if (Input.GetKeyDown(KeyCode.Z)) Undo();
            if (Input.GetKeyDown(KeyCode.Y)) Redo();
        }

        // ── Public API ───────────────────────────────────────────────────────

        public void StartPlacement(string buildingTypeId)
        {
            SelectedBuildingTypeId = buildingTypeId;
            IsInConstructionMode = true;
            _signalBus.Fire(new BuildingModeStartedSignal { TypeId = buildingTypeId });
        }

        public void PreviewAt(Vector2Int position)
        {
            if (!IsInConstructionMode) return;
            if (!_gridService.TryGetTileData(position, out _)) return;

            bool isBlocked = _objectsMapService.IsOccupied(position)
                             || _pendingBuildings.Any(b => b.Position == position);

            _signalBus.Fire(new BuildingPreviewMovedSignal
            {
                Position = position,
                TypeId = SelectedBuildingTypeId,
                IsBlocked = isBlocked
            });
        }

        public void PlaceAt(Vector2Int position)
        {
            if (!IsInConstructionMode) return;
            if (!_gridService.TryGetTileData(position, out _)) return;
            if (_objectsMapService.IsOccupied(position)) return;
            if (_pendingBuildings.Any(b => b.Position == position)) return;

            var tempId = $"pending-{SelectedBuildingTypeId}-{_buildingCounter++}";
            var pending = new PendingBuilding(SelectedBuildingTypeId, position, tempId);

            _pendingBuildings.Add(pending);
            _undoStack.Push(pending);
            _redoStack.Clear();

            _signalBus.Fire(new BuildingPlacedSignal
            {
                TempId = tempId,
                TypeId = SelectedBuildingTypeId,
                Position = position
            });
        }

        public void Undo()
        {
            if (_undoStack.Count == 0) return;

            var last = _undoStack.Pop();
            _pendingBuildings.Remove(last);
            _redoStack.Push(last);

            _signalBus.Fire(new BuildingUndoneSignal { TempId = last.TempId, Position = last.Position });
        }

        public void Redo()
        {
            if (_redoStack.Count == 0) return;

            var building = _redoStack.Peek();

            // Повторна перевірка — позиція могла стати зайнятою з часу скасування
            if (_objectsMapService.IsOccupied(building.Position)) return;
            if (_pendingBuildings.Any(b => b.Position == building.Position)) return;

            _redoStack.Pop();
            _pendingBuildings.Add(building);
            _undoStack.Push(building);

            _signalBus.Fire(new BuildingRedoneSignal { TempId = building.TempId, Position = building.Position });
        }

        public void CancelAll()
        {
            var cancelledTempIds = _pendingBuildings.Select(b => b.TempId).ToArray();
            var cancelledPositions = _pendingBuildings.Select(b => b.Position).ToArray();

            _pendingBuildings.Clear();
            _undoStack.Clear();
            _redoStack.Clear();
            IsInConstructionMode = false;
            SelectedBuildingTypeId = null;

            _signalBus.Fire(new BuildingCancelledSignal
            {
                TempIds = cancelledTempIds,
                Positions = cancelledPositions
            });
        }

        public void ConfirmAll()
        {
            var confirmedTempIds = new List<string>(_pendingBuildings.Count);
            var confirmedTypeIds = new List<string>(_pendingBuildings.Count);
            var confirmedPositions = new List<Vector2Int>(_pendingBuildings.Count);

            foreach (var pending in _pendingBuildings)
            {
                if (!_objectsMapService.IsOccupied(pending.Position))
                {
                    _objectsMapService.Register(pending.Position, pending.TempId);
                    confirmedTempIds.Add(pending.TempId);
                    confirmedTypeIds.Add(pending.TypeId);
                    confirmedPositions.Add(pending.Position);
                }
                else
                {
                    Debug.LogWarning($"[Construction] ConfirmAll: позиція {pending.Position} зайнята, будівлю '{pending.TempId}' пропущено.");
                }
            }

            _pendingBuildings.Clear();
            _undoStack.Clear();
            _redoStack.Clear();
            IsInConstructionMode = false;
            SelectedBuildingTypeId = null;

            _signalBus.Fire(new BuildingConfirmedSignal
            {
                TempIds = confirmedTempIds.ToArray(),
                TypeIds = confirmedTypeIds.ToArray(),
                Positions = confirmedPositions.ToArray()
            });
        }
    }
}
