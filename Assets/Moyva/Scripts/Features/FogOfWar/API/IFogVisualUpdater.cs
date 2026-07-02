using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Описує visual layer для FogOfWar.
    /// Реалізація отримує gameplay fog state через <see cref="IFogOfWarService"/> і будує presentation,
    /// але не повинна ставати source of truth для логіки видимості.
    /// </summary>
    public interface IFogVisualUpdater
    {
        /// <summary>
        /// Ініціалізує visual representation для світу заданого розміру.
        /// </summary>
        /// <param name="width">Ширина fog map у клітинках.</param>
        /// <param name="height">Висота fog map у клітинках.</param>
        /// <param name="context">Світовий контекст, потрібний для побудови visuals.</param>
        void Initialize(int width, int height, FogWorldVisualContext context);

        /// <summary>
        /// Оновлює world/grid context без зміни gameplay state.
        /// Викликається, коли змінились bounds, cell size або height/terrain maps.
        /// </summary>
        /// <param name="context">Оновлений контекст світу для visual presentation.</param>
        void SetWorldContext(FogWorldVisualContext context);

        /// <summary>
        /// Будує тимчасовий preview reveal без зміни gameplay fog state.
        /// Використовується editor/runtime preview path.
        /// </summary>
        /// <param name="center">Центр preview reveal.</param>
        /// <param name="radius">Радіус reveal у клітинках.</param>
        /// <param name="shape">Форма reveal області.</param>
        /// <param name="keepVisible">Чи слід трактувати preview як постійно видиму область.</param>
        void PreviewRevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible);

        /// <summary>
        /// Оновлює visual state лише для клітинок, які змінилися у gameplay fog service.
        /// </summary>
        /// <param name="fogService">Gameplay source of truth для fog state.</param>
        /// <param name="dirtyTiles">Перелік dirty-клітинок.</param>
        void UpdateDirtyTiles(IFogOfWarService fogService, IEnumerable<Vector2Int> dirtyTiles);

        /// <summary>
        /// Повністю перебудовує visual presentation зі стану gameplay fog service.
        /// </summary>
        /// <param name="fogService">Gameplay source of truth для fog state.</param>
        void RebuildFullVisual(IFogOfWarService fogService);
    }
}
