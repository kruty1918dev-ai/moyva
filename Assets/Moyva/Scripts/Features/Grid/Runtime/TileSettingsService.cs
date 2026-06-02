using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Grid.Runtime
{
    /// <summary>
    /// Runtime-сервіс параметрів тайлів.
    /// За наявності <see cref="TerrainLayerProfileSO"/> (нова layer-based ідентичність)
    /// читає параметри з нього за id шару; інакше — відкочується на класичний
    /// <see cref="TileRegistrySO"/> з кешем <see cref="TileTypeDefinition"/> за TileId.
    /// Використовується системами руху/патфайндингу та будівництва.
    /// </summary>
    internal sealed class TileSettingsService : ITileSettingsService
    {
        /// <summary>
        /// Локальний кеш визначень тайлів для O(1) доступу за TileId.
        /// </summary>
        private readonly Dictionary<string, TileTypeDefinition> _cache = new();

        /// <summary>
        /// Нове джерело параметрів за id шару. Має пріоритет, якщо задане.
        /// </summary>
        private readonly TerrainLayerProfileSO _layerProfiles;

        /// <summary>
        /// Будує кеш із реєстру тайлів під час створення сервісу.
        /// </summary>
        /// <param name="registry">Реєстр визначень тайлів (legacy, може бути null).</param>
        /// <param name="layerProfiles">Профілі шарів terrain (нова ідентичність, опційно).</param>
        public TileSettingsService(
            [Zenject.InjectOptional] TileRegistrySO registry = null,
            [Zenject.InjectOptional] TerrainLayerProfileSO layerProfiles = null)
        {
            _layerProfiles = layerProfiles;

            // Кеш legacy-реєстру лишаємо для зворотної сумісності (відкат).
            if (registry?.Definitions != null)
            {
                foreach (var def in registry.Definitions)
                    _cache[def.Id] = def;
            }
        }

        /// <summary>
        /// Повертає вагу руху для переданого TileId / layer-id.
        /// </summary>
        /// <param name="tileTypeId">Ідентифікатор типу тайла або id шару.</param>
        /// <returns>Вага руху, або <c>0f</c> для невалідного/невідомого/непрохідного.</returns>
        public float GetTileWeight(string tileTypeId)
        {
            // 1) Ранній захист від порожнього або null-ідентифікатора.
            if (String.IsNullOrEmpty(tileTypeId))
            {
                Debug.LogWarning("TileSettingsService: GetTileWeight called with null or empty tileTypeId");
                return 0f;
            }

            // 2) Новий шлях: параметри з профілю шару (0 = непрохідний).
            if (_layerProfiles != null)
                return _layerProfiles.GetMovementCost(tileTypeId);

            // 3) Legacy happy-path: знайшли тайл у кеші реєстру.
            if (_cache.TryGetValue(tileTypeId, out var props))
                return props.MovementCost;

            // 4) Якщо ключ відсутній, логуємо попередження для діагностики даних.
            Debug.LogWarning($"TileSettingsService: Tile type ID '{tileTypeId}' not found in registry!");

            // 5) Повертаємо fallback, щоб ігрова логіка лишалась стабільною.
            return 0f;
        }

        /// <summary>
        /// Чи заблоковано тайл/шар для будівництва. Працює лише з профілями шарів;
        /// для legacy-реєстру (без поля) завжди <c>false</c>.
        /// </summary>
        public bool IsBuildBlocked(string tileTypeId)
        {
            if (String.IsNullOrEmpty(tileTypeId) || _layerProfiles == null)
                return false;

            return _layerProfiles.IsBuildBlocked(tileTypeId);
        }

        /// <summary>
        /// Вертикальний зсув поверхні шару. Працює лише з профілями шарів;
        /// для legacy-реєстру (без поля) завжди <c>0</c>.
        /// </summary>
        public float GetSurfaceOffset(string tileTypeId)
        {
            if (String.IsNullOrEmpty(tileTypeId) || _layerProfiles == null)
                return 0f;

            return _layerProfiles.GetSurfaceOffset(tileTypeId);
        }
    }
}