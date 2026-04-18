using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.Runtime
{
    internal sealed class TileSettingsService : ITileSettingsService
    {
        private readonly Dictionary<string, TileTypeDefinition> _cache = new();

        public TileSettingsService(TileRegistrySO registry)
        {
            foreach (var def in registry.Definitions)
            {
                _cache[def.Id] = def;
            }
        }

        public float GetTileWeight(string tileTypeId)
        {
            if (String.IsNullOrEmpty(tileTypeId))
            {
                Debug.LogWarning("TileSettingsService: GetTileWeight called with null or empty tileTypeId");
                return 0f; // або можна повернути 1f, залежно від логіки гри    
            }

            if (_cache.TryGetValue(tileTypeId, out var props))
                return props.MovementCost;

            Debug.LogWarning($"TileSettingsService: Tile type ID '{tileTypeId}' not found in registry!");

            // Повертаємо дефолтні значення, якщо ID не знайдено, щоб гра не впала
            return 0f; // або можна повернути 1f, залежно від логіки гри
        }
    }
}