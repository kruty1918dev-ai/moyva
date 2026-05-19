using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.Runtime
{
    /// <summary>
    /// Runtime-сервіс параметрів тайлів.
    /// Залежить від <see cref="TileRegistrySO"/> і кешує <see cref="TileTypeDefinition"/> за TileId.
    /// Використовується системами руху/патфайндингу для отримання ваги тайла.
    /// </summary>
    internal sealed class TileSettingsService : ITileSettingsService
    {
        /// <summary>
        /// Локальний кеш визначень тайлів для O(1) доступу за TileId.
        /// </summary>
        private readonly Dictionary<string, TileTypeDefinition> _cache = new();

        /// <summary>
        /// Будує кеш із реєстру тайлів під час створення сервісу.
        /// </summary>
        /// <param name="registry">Реєстр визначень тайлів.</param>
        public TileSettingsService(TileRegistrySO registry)
        {
            // 1) Ітеруємо всі визначення з ScriptableObject-реєстру.
            foreach (var def in registry.Definitions)
            {
                // 2) Кешуємо визначення за Id; повторний ключ перезапише попередній.
                _cache[def.Id] = def;
            }
        }

        /// <summary>
        /// Повертає вагу руху для переданого TileId.
        /// </summary>
        /// <param name="tileTypeId">Ідентифікатор типу тайла.</param>
        /// <returns>Вага руху, або <c>0f</c> для невалідного/невідомого TileId.</returns>
        public float GetTileWeight(string tileTypeId)
        {
            // 1) Ранній захист від порожнього або null-ідентифікатора.
            if (String.IsNullOrEmpty(tileTypeId))
            {
                Debug.LogWarning("TileSettingsService: GetTileWeight called with null or empty tileTypeId");
                // 2) Повертаємо fallback-значення, щоб уникнути падіння логіки вище за стеком.
                return 0f;
            }

            // 3) Основний happy-path: знайшли тайл у кеші і повернули його вагу.
            if (_cache.TryGetValue(tileTypeId, out var props))
                return props.MovementCost;

            // 4) Якщо ключ відсутній, логуємо попередження для діагностики даних.
            Debug.LogWarning($"TileSettingsService: Tile type ID '{tileTypeId}' not found in registry!");

            // 5) Повертаємо fallback, щоб ігрова логіка лишалась стабільною.
            return 0f;
        }
    }
}