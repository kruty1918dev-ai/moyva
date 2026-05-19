using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using UnityEngine;
using Zenject;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitFactory : IUnitFactory
    {
        private readonly DiContainer _container;
        private readonly IUnitClassConfig _unitClassConfig;
        private readonly IUnitGameplayProfileService _unitGameplayProfileService;
        private readonly SignalBus _signalBus;
        private readonly IGridService _gridService;
        private readonly IObjectsMapService _objectsMapService;
        
        private readonly Dictionary<string, int> _typeCounters = new();

        public UnitFactory(
            DiContainer container,
            IUnitClassConfig unitClassConfig,
            IUnitGameplayProfileService unitGameplayProfileService,
            SignalBus signalBus,
            IGridService gridService,
            IObjectsMapService objectsMapService)
        {
            _container = container;
            _unitClassConfig = unitClassConfig;
            _unitGameplayProfileService = unitGameplayProfileService;
            _signalBus = signalBus;
            _gridService = gridService;
            _objectsMapService = objectsMapService;
        }

        public string CreateUnit(string typeId, Vector2Int gridPosition)
            => CreateUnit(typeId, gridPosition, null);

        public string CreateUnit(string typeId, Vector2Int gridPosition, string ownerId)
        {
            // Generate an ID the normal way, then delegate.
            if (!_typeCounters.ContainsKey(typeId)) _typeCounters[typeId] = 0;
            _typeCounters[typeId]++;

            var config = _unitClassConfig.GetConfig(typeId);
            if (config == null || config.Prefab == null)
            {
                Debug.LogError($"[UnitFactory] Cannot find config or prefab for {typeId}");
                _typeCounters[typeId]--;
                return null;
            }

            if (_objectsMapService.IsOccupied(gridPosition))
            {
                _objectsMapService.TryGetOccupant(gridPosition, out var occupantId);
                Debug.LogWarning($"[UnitFactory] Cannot create unit '{typeId}' at {gridPosition}: tile is already occupied by '{occupantId}'.");
                _typeCounters[typeId]--;
                return null;
            }

            Vector3 worldPos = new Vector3(gridPosition.x, gridPosition.y);
            GameObject unitObj = _container.InstantiatePrefab(config.Prefab, worldPos, Quaternion.identity, null);

            string instanceId = unitObj.GetInstanceID().ToString().Replace("-", "");
            string finalUnitId = $"{typeId}_{_typeCounters[typeId]:D2}_{instanceId}";

            return FireUnitCreated(finalUnitId, typeId, gridPosition, unitObj, ownerId);
        }

        public string CreateUnitWithId(string forcedUnitId, string typeId, Vector2Int gridPosition, string ownerId)
        {
            if (string.IsNullOrEmpty(forcedUnitId))
            {
                Debug.LogError("[UnitFactory] CreateUnitWithId called with null/empty forcedUnitId.");
                return null;
            }

            var config = _unitClassConfig.GetConfig(typeId);
            if (config == null || config.Prefab == null)
            {
                Debug.LogError($"[UnitFactory] Cannot find config or prefab for {typeId}");
                return null;
            }

            if (_objectsMapService.IsOccupied(gridPosition))
            {
                _objectsMapService.TryGetOccupant(gridPosition, out var occupantId);
                Debug.LogWarning($"[UnitFactory] CreateUnitWithId: tile {gridPosition} already occupied by '{occupantId}'.");
                return null;
            }

            Vector3 worldPos = new Vector3(gridPosition.x, gridPosition.y);
            GameObject unitObj = _container.InstantiatePrefab(config.Prefab, worldPos, Quaternion.identity, null);

            return FireUnitCreated(forcedUnitId, typeId, gridPosition, unitObj, ownerId);
        }

        private string FireUnitCreated(string unitId, string typeId, Vector2Int gridPosition, GameObject unitObj, string ownerId)
        {
            var profile = _unitGameplayProfileService.GetOrDefault(typeId);

            _signalBus.Fire(new UnitCreatedSignal
            {
                UnitId                   = unitId,
                UnitTypeId               = typeId,
                Position                 = gridPosition,
                VisionRange              = profile.ResolveVisionRange(0),
                HasCustomVisionModifiers = true,
                CanSeeCrest              = profile.CanSeeCrest,
                CrestVisibilityFactor    = profile.CrestVisibilityFactor,
                DownSlopeVisionBonus     = profile.DownSlopeVisionBonus,
                SilhouettePenalty        = profile.SilhouettePenalty,
                UnitObject               = unitObj,
                OwnerId                  = ownerId
            });

            return unitId;
        }
    }
}