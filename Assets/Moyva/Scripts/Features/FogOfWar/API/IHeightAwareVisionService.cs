using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Набір локальних модифікаторів, що впливають на розрахунок LOS і visibility factor.
    /// Використовується height-aware vision service і не повинен тягнути за собою camera, save або UI логіку.
    /// </summary>
    public readonly struct FogVisionModifiers
    {
        /// <summary>
        /// Створює набір overrides для розрахунку видимості.
        /// </summary>
        /// <param name="canSeeCrest">Чи дозволено бачити через crest/edge.</param>
        /// <param name="crestVisibilityFactor">Множник видимості на crest.</param>
        /// <param name="downSlopeVisionBonus">Додатковий бонус огляду вниз по схилу.</param>
        /// <param name="silhouettePenalty">Штраф видимості для силуетної цілі.</param>
        public FogVisionModifiers(
            bool canSeeCrest,
            float crestVisibilityFactor,
            float downSlopeVisionBonus,
            float silhouettePenalty)
        {
            HasOverrides = true;
            CanSeeCrest = canSeeCrest;
            CrestVisibilityFactor = Mathf.Clamp01(crestVisibilityFactor);
            DownSlopeVisionBonus = Mathf.Max(0f, downSlopeVisionBonus);
            SilhouettePenalty = Mathf.Clamp01(silhouettePenalty);
        }

        /// <summary>
        /// Чи містить структура явні overrides.
        /// </summary>
        public bool HasOverrides { get; }

        /// <summary>
        /// Чи може спостерігач бачити через crest/edge.
        /// </summary>
        public bool CanSeeCrest { get; }

        /// <summary>
        /// Множник видимості поблизу crest.
        /// </summary>
        public float CrestVisibilityFactor { get; }

        /// <summary>
        /// Додатковий бонус огляду вниз по схилу.
        /// </summary>
        public float DownSlopeVisionBonus { get; }

        /// <summary>
        /// Додатковий штраф для цілі, що читається як силует.
        /// </summary>
        public float SilhouettePenalty { get; }

        /// <summary>
        /// Повертає ефективне значення <see cref="CanSeeCrest"/> з урахуванням default behavior.
        /// </summary>
        public bool EffectiveCanSeeCrest => !HasOverrides || CanSeeCrest;

        /// <summary>
        /// Повертає ефективний crest visibility factor.
        /// </summary>
        public float EffectiveCrestVisibilityFactor => HasOverrides ? Mathf.Clamp01(CrestVisibilityFactor) : 1f;

        /// <summary>
        /// Повертає ефективний down-slope bonus.
        /// </summary>
        public float EffectiveDownSlopeVisionBonus => HasOverrides ? Mathf.Max(0f, DownSlopeVisionBonus) : 0f;

        /// <summary>
        /// Повертає ефективний silhouette penalty.
        /// </summary>
        public float EffectiveSilhouettePenalty => HasOverrides ? Mathf.Clamp01(SilhouettePenalty) : 0f;

        /// <summary>
        /// Формує детермінований підпис модифікаторів для кешування visibility calculations.
        /// </summary>
        /// <returns>Цілочисельний signature для cache key.</returns>
        public int GetSignature()
        {
            unchecked
            {
                int hash = HasOverrides ? 23 : 17;
                hash = hash * 31 + (EffectiveCanSeeCrest ? 1 : 0);
                hash = hash * 31 + Mathf.RoundToInt(EffectiveCrestVisibilityFactor * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(EffectiveDownSlopeVisionBonus * 1000f);
                hash = hash * 31 + Mathf.RoundToInt(EffectiveSilhouettePenalty * 1000f);
                return hash;
            }
        }
    }

    /// <summary>
    /// Ізолює LOS/visibility logic, яка враховує terrain height та slope modifiers.
    /// Реалізацію зазвичай викликає <see cref="IFogVisibilityResolver"/>, а інші системи не повинні
    /// напряму залежати від конкретного алгоритму трасування променів.
    /// </summary>
    public interface IHeightAwareVisionService
    {
        /// <summary>
        /// Передає height map generated світу для подальших LOS-обчислень.
        /// </summary>
        /// <param name="heightMap">Мапа висот у координатах клітинок.</param>
        void SetHeightMap(float[,] heightMap);

        /// <summary>
        /// Обчислює максимальний радіус пошуку, який варто перевіряти для спостерігача.
        /// </summary>
        /// <param name="origin">Клітинка спостерігача.</param>
        /// <param name="baseVisionRange">Базовий vision range.</param>
        /// <param name="maxVisionRange">Глобальна верхня межа для пошуку.</param>
        /// <param name="observerModifiers">Додаткові модифікатори спостерігача.</param>
        /// <returns>Безпечний радіус пошуку для visibility resolver.</returns>
        int GetSearchRadius(Vector2Int origin, int baseVisionRange, int maxVisionRange, FogVisionModifiers observerModifiers = default);

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
