using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Відповідає лише за оцінку search radius для terrain-aware visibility перевірок.
    /// </summary>
    public interface IHeightAwareSearchRadiusProvider
    {
        /// <summary>
        /// Обчислює максимальний радіус пошуку, який варто перевіряти для спостерігача.
        /// </summary>
        /// <param name="origin">Клітинка спостерігача.</param>
        /// <param name="baseVisionRange">Базовий vision range.</param>
        /// <param name="maxVisionRange">Глобальна верхня межа для пошуку.</param>
        /// <param name="observerModifiers">Додаткові модифікатори спостерігача.</param>
        /// <returns>Безпечний радіус пошуку для visibility resolver.</returns>
        int GetSearchRadius(Vector2Int origin, int baseVisionRange, int maxVisionRange, FogVisionModifiers observerModifiers = default);
    }
}
