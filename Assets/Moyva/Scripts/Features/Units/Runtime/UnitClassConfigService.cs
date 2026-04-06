using System.Collections.Generic;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitClassConfigService : IUnitClassConfig
    {
        private readonly Dictionary<string, UnitClassConfig> _configByTypeId;

        public UnitClassConfigService(UnitRegistrySO registry)
        {
            _configByTypeId = new Dictionary<string, UnitClassConfig>();
            foreach (var config in registry.Configs)
            {
                if (!_configByTypeId.ContainsKey(config.TypeId))
                    _configByTypeId.Add(config.TypeId, config);
                else
                    Debug.LogWarning($"[UnitClassConfigService] Дублікат TypeId '{config.TypeId}' у реєстрі юнітів. Ігнорую цей запис.");
            }
        }

        public UnitClassConfig GetConfig(string typeId)
        {
            if (string.IsNullOrEmpty(typeId)) return null;

            // Fast path for exact type IDs (e.g. "warrior").
            if (_configByTypeId.TryGetValue(typeId, out var exactConfig))
            {
                return exactConfig;
            }

            // 1. "Розумне" очищення: беремо все ДО першого символу '_'
            string baseTypeId = typeId;
            int underscoreIndex = typeId.IndexOf('_');

            if (underscoreIndex != -1)
            {
                baseTypeId = typeId.Substring(0, underscoreIndex);
            }

            // 2. Пошук у словнику за базовим назвою (наприклад, "warrior-01")
            if (_configByTypeId.TryGetValue(baseTypeId, out var config))
            {
                return config;
            }

            return null;
        }
    }
}