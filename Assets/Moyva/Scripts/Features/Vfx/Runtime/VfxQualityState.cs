using Kruty1918.Moyva.Shared.Graphics;
using Kruty1918.Moyva.Vfx.API;
using UnityEngine;

namespace Kruty1918.Moyva.Vfx.Runtime
{
    /// <summary>
    /// Resolved per-quality-profile VFX parameters: how many effects may be
    /// active, how much particle emission is scaled and how aggressively
    /// distance/zoom culls low-priority effects.
    /// </summary>
    public readonly struct VfxQualityState
    {
        public VfxQualityState(
            int maxActive,
            float countScale,
            float cullDistance,
            float lowPriorityMaxOrthoSize,
            int maxSpawnsPerFrame)
        {
            MaxActive = Mathf.Max(1, maxActive);
            CountScale = Mathf.Max(0.05f, countScale);
            CullDistance = Mathf.Max(0f, cullDistance);
            LowPriorityMaxOrthoSize = Mathf.Max(0f, lowPriorityMaxOrthoSize);
            MaxSpawnsPerFrame = Mathf.Max(1, maxSpawnsPerFrame);
        }

        public int MaxActive { get; }
        public float CountScale { get; }
        public float CullDistance { get; }
        public float LowPriorityMaxOrthoSize { get; }
        public int MaxSpawnsPerFrame { get; }

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
