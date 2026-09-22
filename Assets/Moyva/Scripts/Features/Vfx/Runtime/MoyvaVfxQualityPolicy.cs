using Kruty1918.Moyva.Shared.Graphics;
using Kruty1918.Vfx;
using UnityEngine;

namespace Kruty1918.Moyva.Vfx.Runtime
{
    /// <summary>Maps the Moyva graphics quality profile onto package VfxQualityState budgets.</summary>
    public static class MoyvaVfxQualityPolicy
    {
        /// <summary>Будує стан якості для вказаного профілю.</summary>
        public static VfxQualityState ForProfile(
            GraphicsQualityProfile profile,
            VfxBudgetSettings budget)
        {
            budget ??= new VfxBudgetSettings();
            bool mobile = Application.isMobilePlatform;

            switch (profile)
            {
                case GraphicsQualityProfile.Performance:
                    return new VfxQualityState(
                        budget.maxActivePerformance,
                        0.5f,
                        budget.defaultCullDistance * 0.7f,
                        EffectiveLowOrtho(budget, 16f),
                        budget.maxSpawnsPerFrame);

                case GraphicsQualityProfile.Quality:
                    return new VfxQualityState(
                        budget.maxActiveQuality,
                        1f,
                        budget.defaultCullDistance * 1.25f,
                        EffectiveLowOrtho(budget, 0f),
                        budget.maxSpawnsPerFrame);

                case GraphicsQualityProfile.Custom:
                case GraphicsQualityProfile.Balanced:
                case GraphicsQualityProfile.Auto:
                default:
                    // Auto resolves to the platform default tier.
                    return mobile
                        ? new VfxQualityState(
                            budget.maxActivePerformance,
                            0.5f,
                            budget.defaultCullDistance * 0.7f,
                            EffectiveLowOrtho(budget, 16f),
                            budget.maxSpawnsPerFrame)
                        : new VfxQualityState(
                            budget.maxActiveBalanced,
                            0.85f,
                            budget.defaultCullDistance,
                            EffectiveLowOrtho(budget, 0f),
                            budget.maxSpawnsPerFrame);
            }
        }

        private static float EffectiveLowOrtho(VfxBudgetSettings budget, float fallback)
            => budget.lowPriorityMaxOrthoSize > 0f
                ? budget.lowPriorityMaxOrthoSize
                : fallback;
    }
}
