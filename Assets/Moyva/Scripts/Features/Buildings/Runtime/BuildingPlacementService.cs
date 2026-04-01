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
    /// Внутрішній запис про будівлю в поточній сесії розміщення
    /// </summary>
    internal class SessionBuilding
    {
        public string SessionId;
        public string TypeId;
        public Vector2Int Position;
        public GameObject GameObject;
    }

    /// <summary>
    /// Сервіс режиму розміщення будівель.
    /// Керує сесійними (непідтвердженими) будівлями, підтримує undo/redo.
    /// При підтвердженні реєструє будівлі через IBuildingService та IObjectsMapService.
    /// </summary>
    internal sealed class BuildingPlacementService : IBuildingPlacementService, IInitializable, IDisposable
    {
        private readonly IGridService _gridService;
        private readonly IObjectsMapService _objectsMapService;
        private readonly IBuildingService _buildingService;
        private readonly BuildingRegistrySO _buildingRegistry;
        private readonly SignalBus _signalBus;
        private readonly DiContainer _container;

        // Стек для undo/redo
        private readonly Stack<SessionBuilding> _undoStack = new Stack<SessionBuilding>();
        private readonly Stack<SessionBuilding> _redoStack = new Stack<SessionBuilding>();

        // Поточна сесія: sessionId → будівля
        private readonly Dictionary<string, SessionBuilding> _sessionBuildings = new Dictionary<string, SessionBuilding>();

        // Зайняті сесійними будівлями позиції (для перевірки перетину)
        private readonly HashSet<Vector2Int> _sessionOccupied = new HashSet<Vector2Int>();

        private string _activeBuildingTypeId;
        private int _sessionCounter;

        public bool IsPlacementModeActive { get; private set; }
        public string ActiveBuildingTypeId => _activeBuildingTypeId;

        public BuildingPlacementService(
            IGridService gridService,
            IObjectsMapService objectsMapService,
            IBuildingService buildingService,
            BuildingRegistrySO buildingRegistry,
            SignalBus signalBus,
            DiContainer container)
        {
            _gridService = gridService;
            _objectsMapService = objectsMapService;
            _buildingService = buildingService;
            _buildingRegistry = buildingRegistry;
            _signalBus = signalBus;
            _container = container;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<TileClickedSignal>(OnTileClicked);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<TileClickedSignal>(OnTileClicked);
            ClearSession();
        }

        private void OnTileClicked(TileClickedSignal signal)
        {
            if (!IsPlacementModeActive) return;
            TryPlace(signal.Position);
        }

        public void StartPlacement(string buildingTypeId)
        {
            if (string.IsNullOrEmpty(buildingTypeId)) return;

            // Якщо вже є сесійні будівлі — скасовуємо їх перед новою сесією
            ClearSession();

            _activeBuildingTypeId = buildingTypeId;
            IsPlacementModeActive = true;

            Debug.Log($"[BuildingPlacement] Розпочато розміщення: {buildingTypeId}");
        }

        public bool CanPlace(Vector2Int position)
        {
            if (!IsPlacementModeActive || string.IsNullOrEmpty(_activeBuildingTypeId)) return false;
            if (!_gridService.TryGetTileData(position, out _)) return false;
            if (_objectsMapService.IsOccupied(position)) return false;
            if (_sessionOccupied.Contains(position)) return false;
            return true;
        }

        public bool TryPlace(Vector2Int position)
        {
            if (!CanPlace(position)) return false;

            var config = _buildingRegistry.GetConfig(_activeBuildingTypeId);
            if (config == null || config.Prefab == null)
            {
                Debug.LogWarning($"[BuildingPlacement] Конфіг або префаб не знайдено для: {_activeBuildingTypeId}");
                return false;
            }

            var sessionId = $"{_activeBuildingTypeId}_s{++_sessionCounter}";
            var worldPos = new Vector3(position.x, position.y, 0f);
            var prefabInstance = _container.InstantiatePrefab(config.Prefab, worldPos, Quaternion.identity, null);

            var entry = new SessionBuilding
            {
                SessionId = sessionId,
                TypeId = _activeBuildingTypeId,
                Position = position,
                GameObject = prefabInstance
            };

            _sessionBuildings[sessionId] = entry;
            _sessionOccupied.Add(position);
            _undoStack.Push(entry);
            _redoStack.Clear(); // після нової дії redo-стек очищається

            _signalBus.Fire(new BuildingPreviewPlacedSignal
            {
                SessionId = sessionId,
                TypeId = _activeBuildingTypeId,
                Position = position
            });

            Debug.Log($"[BuildingPlacement] Розміщено сесійну будівлю '{_activeBuildingTypeId}' на {position} (id={sessionId})");
            return true;
        }

        public bool Undo()
        {
            if (_undoStack.Count == 0) return false;

            var entry = _undoStack.Pop();
            _sessionBuildings.Remove(entry.SessionId);
            _sessionOccupied.Remove(entry.Position);

            if (entry.GameObject != null)
                UnityEngine.Object.Destroy(entry.GameObject);

            _redoStack.Push(entry);

            _signalBus.Fire(new BuildingPreviewRemovedSignal
            {
                SessionId = entry.SessionId,
                Position = entry.Position
            });

            Debug.Log($"[BuildingPlacement] Undo: видалено '{entry.TypeId}' на {entry.Position}");
            return true;
        }

        public bool Redo()
        {
            if (_redoStack.Count == 0) return false;

            var entry = _redoStack.Pop();

            // Перевіряємо, чи позиція ще вільна (могла бути зайнята іншою дією)
            if (!CanPlace(entry.Position))
            {
                Debug.LogWarning($"[BuildingPlacement] Redo неможливий: позиція {entry.Position} зайнята. Пропускаємо цей запис.");
                return false;
            }

            var config = _buildingRegistry.GetConfig(entry.TypeId);
            if (config == null || config.Prefab == null) return false;

            var worldPos = new Vector3(entry.Position.x, entry.Position.y, 0f);
            entry.GameObject = _container.InstantiatePrefab(config.Prefab, worldPos, Quaternion.identity, null);

            _sessionBuildings[entry.SessionId] = entry;
            _sessionOccupied.Add(entry.Position);
            _undoStack.Push(entry);

            _signalBus.Fire(new BuildingPreviewPlacedSignal
            {
                SessionId = entry.SessionId,
                TypeId = entry.TypeId,
                Position = entry.Position
            });

            Debug.Log($"[BuildingPlacement] Redo: відновлено '{entry.TypeId}' на {entry.Position}");
            return true;
        }

        public void Confirm()
        {
            foreach (var entry in _sessionBuildings.Values)
            {
                // Реєструємо будівлю в картах та сервісі
                _objectsMapService.Register(entry.Position, entry.SessionId);
                _buildingService.RegisterBuilding(entry.SessionId, entry.TypeId, entry.Position, entry.GameObject);
            }

            var count = _sessionBuildings.Count;
            _sessionBuildings.Clear();
            _sessionOccupied.Clear();
            _undoStack.Clear();
            _redoStack.Clear();

            _signalBus.Fire(new BuildingConstructionConfirmedSignal());
            ExitPlacementMode();

            Debug.Log($"[BuildingPlacement] Підтверджено {count} будівель.");
        }

        public void Cancel()
        {
            ClearSession();
            _signalBus.Fire(new BuildingConstructionCanceledSignal());
            ExitPlacementMode();

            Debug.Log("[BuildingPlacement] Будівництво скасовано.");
        }

        public void ExitPlacementMode()
        {
            _activeBuildingTypeId = null;
            IsPlacementModeActive = false;
        }

        private void ClearSession()
        {
            foreach (var entry in _sessionBuildings.Values)
            {
                if (entry.GameObject != null)
                    UnityEngine.Object.Destroy(entry.GameObject);
            }

            _sessionBuildings.Clear();
            _sessionOccupied.Clear();
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }
}
