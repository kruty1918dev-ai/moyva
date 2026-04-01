using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Buildings.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Buildings.Runtime
{
    /// <summary>
    /// Сервіс підтверджених будівель.
    /// Зберігає всі будівлі, які були підтверджені через BuildingPlacementService.
    /// </summary>
    internal sealed class BuildingService : IBuildingService, IInitializable, IDisposable
    {
        private readonly IObjectsMapService _objectsMapService;
        private readonly SignalBus _signalBus;
        private readonly Dictionary<string, PlacedBuilding> _buildings = new Dictionary<string, PlacedBuilding>();

        public BuildingService(IObjectsMapService objectsMapService, SignalBus signalBus)
        {
            _objectsMapService = objectsMapService;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            // Можна підписатись на сигнали знищення будівель у майбутньому
        }

        public void Dispose()
        {
            // Відписатись від сигналів при необхідності
        }

        public IReadOnlyList<PlacedBuilding> GetAllBuildings() =>
            new List<PlacedBuilding>(_buildings.Values);

        public PlacedBuilding GetBuilding(Vector2Int position)
        {
            if (_objectsMapService.TryGetOccupant(position, out var id) &&
                _buildings.TryGetValue(id, out var building))
                return building;
            return null;
        }

        public void DestroyBuilding(string buildingId)
        {
            if (!_buildings.TryGetValue(buildingId, out var building)) return;

            _objectsMapService.Unregister(building.Position);

            if (building.GameObject != null)
                UnityEngine.Object.Destroy(building.GameObject);

            _buildings.Remove(buildingId);
            Debug.Log($"[BuildingService] Знищено будівлю '{buildingId}' на {building.Position}");
        }

        public void RegisterBuilding(string buildingId, string typeId, Vector2Int position, GameObject go)
        {
            _buildings[buildingId] = new PlacedBuilding
            {
                BuildingId = buildingId,
                TypeId = typeId,
                Position = position,
                GameObject = go
            };
        }
    }
}
