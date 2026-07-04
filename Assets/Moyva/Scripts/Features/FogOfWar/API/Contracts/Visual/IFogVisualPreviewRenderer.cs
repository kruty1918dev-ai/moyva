using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Відповідає лише за preview-only fog presentation без зміни gameplay state.
    /// </summary>
    public interface IFogVisualPreviewRenderer
    {
        /// <summary>
        /// Будує тимчасовий preview reveal без зміни gameplay fog state.
        /// Використовується editor/runtime preview path.
        /// </summary>
        /// <param name="center">Центр preview reveal.</param>
        /// <param name="radius">Радіус reveal у клітинках.</param>
        /// <param name="shape">Форма reveal області.</param>
        /// <param name="keepVisible">Чи слід трактувати preview як постійно видиму область.</param>
        void PreviewRevealArea(Vector2Int center, int radius, FogRevealShape shape, bool keepVisible);
    }
}
