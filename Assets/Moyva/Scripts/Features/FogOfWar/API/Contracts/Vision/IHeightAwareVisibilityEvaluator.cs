using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Інкапсулює лише LOS/visibility evaluation між клітинками на основі рельєфу.
    /// </summary>
    public interface IHeightAwareVisibilityEvaluator
    {
        /// <summary>
        /// Повертає нормалізований visibility factor між двома клітинками.
        /// </summary>
        /// <param name="origin">Клітинка спостерігача.</param>
        /// <param name="target">Клітинка цілі.</param>
        /// <param name="baseVisionRange">Базовий vision range спостерігача.</param>
        /// <param name="maxVisionRange">Максимально допустимий range для LOS.</param>
        /// <param name="observerModifiers">Модифікатори спостерігача.</param>
        /// <param name="targetModifiers">Модифікатори цілі.</param>
        /// <returns>Видимість у діапазоні [0..1].</returns>
        float GetVisibilityFactor(Vector2Int origin, Vector2Int target, int baseVisionRange, int maxVisionRange, FogVisionModifiers observerModifiers = default, FogVisionModifiers targetModifiers = default);

        /// <summary>
        /// Перевіряє, чи ціль вважається видимою з урахуванням LOS, рельєфу та порогів видимості.
        /// </summary>
        /// <param name="origin">Клітинка спостерігача.</param>
        /// <param name="target">Клітинка цілі.</param>
        /// <param name="baseVisionRange">Базовий vision range спостерігача.</param>
        /// <param name="maxVisionRange">Максимально допустимий range для LOS.</param>
        /// <param name="observerModifiers">Модифікатори спостерігача.</param>
        /// <param name="targetModifiers">Модифікатори цілі.</param>
        /// <returns><see langword="true"/>, якщо ціль проходить visibility threshold.</returns>
        bool IsTargetVisible(Vector2Int origin, Vector2Int target, int baseVisionRange, int maxVisionRange, FogVisionModifiers observerModifiers = default, FogVisionModifiers targetModifiers = default);
    }
}
