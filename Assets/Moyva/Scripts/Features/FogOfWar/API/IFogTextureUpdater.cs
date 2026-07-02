using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    [Obsolete("Use IFogVisualUpdater for TWC dual-grid fog volume visuals.")]
    /// <summary>
    /// Legacy API для 2D shader/texture fog presentation.
    /// Використовується лише старим visual path і не є source of truth для fog state.
    /// Новий код має працювати через <see cref="IFogVisualUpdater"/>.
    /// </summary>
    public interface IFogTextureUpdater
    {
        /// <summary>
        /// Готує texture-based fog presentation до роботи після ініціалізації світу.
        /// </summary>
        /// <param name="width">Ширина fog map у клітинках.</param>
        /// <param name="height">Висота fog map у клітинках.</param>
        /// <param name="fogMaterial">Матеріал, у який буде записано fog texture state.</param>
        void Initialize(int width, int height, Material fogMaterial);

        /// <summary>
        /// Оновлює лише dirty-клітинки у legacy texture presentation.
        /// </summary>
        /// <param name="fogService">Gameplay source of truth для fog state.</param>
        /// <param name="dirtyTiles">Перелік клітинок, які змінили стан.</param>
        void UpdateDirtyTiles(IFogOfWarService fogService, IEnumerable<Vector2Int> dirtyTiles);

        /// <summary>
        /// Повністю перебудовує legacy fog texture зі стану gameplay fog service.
        /// </summary>
        /// <param name="fogService">Gameplay source of truth для fog state.</param>
        void RebuildFullTexture(IFogOfWarService fogService);
    }
}
