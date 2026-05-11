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
            var config = _unitClassConfig.GetConfig(typeId);
            if (config == null || config.Prefab == null)
            {
                Debug.LogError($"[UnitFactory] Cannot find config or prefab for {typeId}");
                return null;
            }

            if (_objectsMapService.IsOccupied(gridPosition))
            {
                _objectsMapService.TryGetOccupant(gridPosition, out var occupantId);
                Debug.LogWarning($"[UnitFactory] Cannot create unit '{typeId}' at {gridPosition}: tile is already occupied by '{occupantId}'.");
                return null;
            }

            // 1. Розрахунок World Position через GridService (якщо є такий метод) 
            // або просто Vector3 для тесту
            Vector3 worldPos = new Vector3(gridPosition.x, gridPosition.y); 

            // 2. Спавн через Zenject для підтримки ін'єкцій у сам юніт
            GameObject unitObj = _container.InstantiatePrefab(config.Prefab, worldPos, Quaternion.identity, null);
            

            // 3. Генерація унікального ID: warior-01_12345
            if (!_typeCounters.ContainsKey(typeId)) _typeCounters[typeId] = 0;
            _typeCounters[typeId]++;
            
            string instanceId = unitObj.GetInstanceID().ToString().Replace("-", "");
            string finalUnitId = $"{typeId}_{_typeCounters[typeId]:D2}_{instanceId}";

            // // 4. Налаштування View
            // var view = unitObj.GetComponent<UnitView>();
            // if (view != null) view.Setup(finalUnitId);

            var profile = _unitGameplayProfileService.GetOrDefault(typeId);

            // 5. Подія створення (UnitService її підхопить)
            _signalBus.Fire(new UnitCreatedSignal 
            { 
                UnitId = finalUnitId, 
                UnitTypeId = typeId, 
                Position = gridPosition,
                VisionRange = profile.ResolveVisionRange(0),
                HasCustomVisionModifiers = true,
                CanSeeCrest = profile.CanSeeCrest,
                CrestVisibilityFactor = profile.CrestVisibilityFactor,
                DownSlopeVisionBonus = profile.DownSlopeVisionBonus,
                SilhouettePenalty = profile.SilhouettePenalty,
                UnitObject = unitObj,
                OwnerId = ownerId
            });

            return finalUnitId;
        }
    }
}