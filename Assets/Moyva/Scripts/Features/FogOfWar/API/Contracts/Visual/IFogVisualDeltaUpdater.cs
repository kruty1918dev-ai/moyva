using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Відповідає лише за інкрементальні visual updates для dirty fog changes.
    /// </summary>
    public interface IFogVisualDeltaUpdater
    {
        /// <summary>
        /// Оновлює visual state лише для клітинок, які змінилися у gameplay fog service.
        /// </summary>
        /// <param name="fogService">Gameplay source of truth для fog state.</param>
        /// <param name="dirtyTiles">Перелік dirty-клітинок.</param>
        void UpdateDirtyTiles(IFogOfWarService fogService, IEnumerable<Vector2Int> dirtyTiles);

        /// <summary>
        /// Оновлює visual state за точним списком old→new змін.
        /// Нові clustered renderers використовують цей шлях, щоб перебудувати тільки affected mesh clusters.
        /// Старі реалізації можуть fallback-нутись до <see cref="UpdateDirtyTiles"/>.
        /// </summary>
        /// <param name="fogService">Gameplay source of truth для fog state.</param>
        /// <param name="changes">Список змін клітинок з old/new state та height bucket-ами.</param>
        /// <param name="context">Актуальний world visual context.</param>
        void RequestCellsUpdate(
            IFogOfWarService fogService,
            IReadOnlyList<FogCellVisualChange> changes,
            FogWorldVisualContext context);
    }
}
