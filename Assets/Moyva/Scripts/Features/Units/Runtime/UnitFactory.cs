using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;
using System.Collections.Generic;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitFactory : IUnitFactory
    {
        private readonly DiContainer _container;
        private readonly IUnitClassConfig _unitClassConfig;
        private readonly SignalBus _signalBus;
        private readonly IGridService _gridService;
        
        private readonly Dictionary<string, int> _typeCounters = new();

        public UnitFactory(DiContainer container, IUnitClassConfig unitClassConfig, SignalBus signalBus, IGridService gridService)
        {
            _container = container;
            _unitClassConfig = unitClassConfig;
            _signalBus = signalBus;
            _gridService = gridService;
        }

        public string CreateUnit(string typeId, Vector2Int gridPosition)
        {
            var config = _unitClassConfig.GetConfig(typeId);
            if (config == null || config.Prefab == null)
            {
                Debug.LogError($"[UnitFactory] Cannot find config or prefab for {typeId}");
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

            // 5. Подія створення (UnitService її підхопить)
            _signalBus.Fire(new UnitCreatedSignal 
            { 
                UnitId = finalUnitId, 
                UnitTypeId = typeId, 
                Position = gridPosition ,
                UnitObject = unitObj // Додаємо посилання на GameObject юніта
            });

            return finalUnitId;
        }
    }
}