using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Buildings.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Buildings.Runtime
{
    /// <summary>
    /// Реалізація сервісу розміщення будівель.
    /// Підтримує:
    ///   – вибір будівлі з меню (Military / Civilian / Industrial),
    ///   – перевірку доступності тайлу,
    ///   – розміщення (pending-список),
    ///   – підтвердження (Confirm) та скасування (Cancel) всієї сесії,
    ///   – Undo / Redo (Ctrl+Z / Ctrl+Y).
    /// Клавіатурні скорочення обробляються через <see cref="BuildingInputHandler"/>.
    /// </summary>
    internal sealed class BuildingPlacementService : IBuildingPlacementService, IInitializable, IDisposable
    {
        private readonly IGridService _gridService;
        private readonly IObjectsMapService _objectsMapService;
        private readonly SignalBus _signalBus;
        private readonly BuildingRegistrySO _registry;

        // Будівлі, розміщені у поточній сесії (ще не підтверджені).
        private readonly List<BuildingPlacedEntry> _pending = new List<BuildingPlacedEntry>();

        // Стеки для Undo/Redo.
        private readonly Stack<BuildingPlacedEntry> _undoStack = new Stack<BuildingPlacedEntry>();
        private readonly Stack<BuildingPlacedEntry> _redoStack = new Stack<BuildingPlacedEntry>();

        // Набір тайлів, зарезервованих pending-будівлями (щоб не дозволяти перекриття).
        private readonly HashSet<Vector2Int> _reservedPositions = new HashSet<Vector2Int>();

        public bool IsPlacingMode { get; private set; }
        public string SelectedBuildingId { get; private set; }

        public BuildingPlacementService(
            IGridService gridService,
            IObjectsMapService objectsMapService,
            SignalBus signalBus,
            BuildingRegistrySO registry)
        {
            _gridService = gridService;
            _objectsMapService = objectsMapService;
            _signalBus = signalBus;
            _registry = registry;
        }

        public void Initialize() { }

        public void Dispose()
        {
            Cancel();
        }

        // ──────────────────────────────── вибір будівлі ────────────────────────────────

        public void SelectBuilding(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId)) return;
            SelectedBuildingId = buildingId;
            IsPlacingMode = true;
            // Очищаємо redo-стек при виборі нової будівлі.
            _redoStack.Clear();
            Debug.Log($"[Buildings] Вибрано будівлю: {buildingId}");
        }

        public void ExitPlacingMode()
        {
            SelectedBuildingId = null;
            IsPlacingMode = false;
        }

        // ──────────────────────────────── перевірка ────────────────────────────────────

        public bool CanPlaceAt(Vector2Int position)
        {
            if (!IsPlacingMode || string.IsNullOrEmpty(SelectedBuildingId)) return false;

            var config = _registry.GetById(SelectedBuildingId);
            if (config == null) return false;

            return AreTilesFree(position, config.Size);
        }

        private bool AreTilesFree(Vector2Int origin, Vector2Int size)
        {
            for (int dx = 0; dx < size.x; dx++)
            {
                for (int dy = 0; dy < size.y; dy++)
                {
                    var pos = origin + new Vector2Int(dx, dy);

                    if (!_gridService.TryGetTileData(pos, out _))
                        return false;

                    if (_objectsMapService.IsOccupied(pos))
                        return false;

                    if (_reservedPositions.Contains(pos))
                        return false;
                }
            }
            return true;
        }

        // ──────────────────────────────── розміщення ───────────────────────────────────

        public void PlaceBuilding(Vector2Int position)
        {
            if (!CanPlaceAt(position)) return;

            var config = _registry.GetById(SelectedBuildingId);
            if (config == null) return;

            var entry = new BuildingPlacedEntry
            {
                BuildingId = SelectedBuildingId,
                Position = position,
                InstanceId = GenerateInstanceId(SelectedBuildingId)
            };

            _pending.Add(entry);
            _undoStack.Push(entry);
            _redoStack.Clear();

            ReservePositions(position, config.Size);

            Debug.Log($"[Buildings] Розміщено (pending): {entry.BuildingId} @ {position} [{entry.InstanceId}]");
        }

        // ──────────────────────────────── підтвердження ────────────────────────────────

        public void Confirm()
        {
            foreach (var entry in _pending)
            {
                _objectsMapService.Register(entry.Position, entry.InstanceId);
                _signalBus.Fire(new BuildingPlacedSignal
                {
                    BuildingId = entry.BuildingId,
                    InstanceId = entry.InstanceId,
                    Position   = entry.Position
                });
                Debug.Log($"[Buildings] Підтверджено: {entry.BuildingId} @ {entry.Position}");
            }

            ClearSession();
            ExitPlacingMode();
        }

        // ──────────────────────────────── скасування ───────────────────────────────────

        public void Cancel()
        {
            foreach (var entry in _pending)
            {
                var config = _registry.GetById(entry.BuildingId);
                if (config != null)
                    UnreservePositions(entry.Position, config.Size);

                _signalBus.Fire(new BuildingCancelledSignal
                {
                    InstanceId = entry.InstanceId,
                    Position   = entry.Position
                });
            }

            ClearSession();
            ExitPlacingMode();
            Debug.Log("[Buildings] Сесію будівництва скасовано.");
        }

        // ──────────────────────────────── Undo / Redo ──────────────────────────────────

        public void Undo()
        {
            if (_undoStack.Count == 0) return;

            var entry = _undoStack.Pop();
            _pending.Remove(entry);
            _redoStack.Push(entry);

            var config = _registry.GetById(entry.BuildingId);
            if (config != null)
                UnreservePositions(entry.Position, config.Size);

            _signalBus.Fire(new BuildingCancelledSignal
            {
                InstanceId = entry.InstanceId,
                Position   = entry.Position
            });

            Debug.Log($"[Buildings] Undo: видалено {entry.BuildingId} @ {entry.Position}");
        }

        public void Redo()
        {
            if (_redoStack.Count == 0) return;

            var entry = _redoStack.Pop();

            var config = _registry.GetById(entry.BuildingId);
            if (config == null) return;

            // Перевіряємо, що тайли все ще вільні.
            if (!AreTilesFree(entry.Position, config.Size))
            {
                Debug.LogWarning($"[Buildings] Redo неможливий: позиція {entry.Position} вже зайнята.");
                return;
            }

            _pending.Add(entry);
            _undoStack.Push(entry);
            ReservePositions(entry.Position, config.Size);

            Debug.Log($"[Buildings] Redo: повернено {entry.BuildingId} @ {entry.Position}");
        }

        // ──────────────────────────────── допоміжні ────────────────────────────────────

        private void ReservePositions(Vector2Int origin, Vector2Int size)
        {
            for (int dx = 0; dx < size.x; dx++)
                for (int dy = 0; dy < size.y; dy++)
                    _reservedPositions.Add(origin + new Vector2Int(dx, dy));
        }

        private void UnreservePositions(Vector2Int origin, Vector2Int size)
        {
            for (int dx = 0; dx < size.x; dx++)
                for (int dy = 0; dy < size.y; dy++)
                    _reservedPositions.Remove(origin + new Vector2Int(dx, dy));
        }

        private void ClearSession()
        {
            _pending.Clear();
            _undoStack.Clear();
            _redoStack.Clear();
            _reservedPositions.Clear();
        }

        private static string GenerateInstanceId(string buildingId)
        {
            return $"{buildingId}_{System.Guid.NewGuid():N}";
        }
    }
}
