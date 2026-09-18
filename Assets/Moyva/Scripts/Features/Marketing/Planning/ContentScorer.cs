using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Marketing.Contracts;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.Planning
{
    /// <summary>
    /// Scores content-index entries for marketing suitability. Deterministic:
    /// same index + same history + same bias ⇒ same score.
    /// </summary>
    public static class ContentScorer
    {
        public sealed class Context
        {
            public ShotCategory shotCategory = ShotCategory.WorldBeauty;
            public bool needsAnimation;                 // e.g. hero close-up
            public string preferredBiome = string.Empty;
            public float aspectIntent = 16f / 9f;
            /// <summary>contentId → times already used this run (repetition penalty).</summary>
            public IReadOnlyDictionary<string, int> usageCounts;
            /// <summary>contentId → extra manual bias from metadata.</summary>
            public IReadOnlyDictionary<string, float> scoreBias;
        }

        public static float Score(ContentIndexEntry e, Context ctx)
        {
            ctx ??= new Context();
            if (e == null || e.marketingDisabled || e.selectionOverride == SelectionOverride.Ban)
                return float.NegativeInfinity;
            if (e.selectionOverride == SelectionOverride.Pin)
                return float.PositiveInfinity;
            if (e.missingMaterial || e.placeholderSuspect)
                return float.NegativeInfinity;
            if (ctx.needsAnimation && !e.hasAnimator)
                return float.NegativeInfinity;

            float score = 0f;

            // gameplay relevance: combat/economy roles rank higher for their shots
            score += GameplayRelevance(e, ctx.shotCategory);

            // visual quality proxies
            score += e.rendererCount > 0 ? 1f : -4f;
            score += ValueScore(e);

            // animation quality
            if (e.hasAnimator)
                score += 1f + Mathf.Min(e.animationClips.Length, 4) * 0.25f;

            // novelty: penalize assets already used this run
            int used = ctx.usageCounts != null && ctx.usageCounts.TryGetValue(e.id, out int n) ? n : 0;
            score -= used * 2.5f;

            // biome compatibility
            if (!string.IsNullOrEmpty(ctx.preferredBiome) && !string.IsNullOrEmpty(e.biome))
                score += string.Equals(e.biome, ctx.preferredBiome, StringComparison.OrdinalIgnoreCase) ? 1.5f : -1f;

            // manual override weight
            if (e.selectionOverride == SelectionOverride.Prefer) score += 3f;
            if (e.selectionOverride == SelectionOverride.Avoid) score -= 3f;
            if (ctx.scoreBias != null && ctx.scoreBias.TryGetValue(e.id, out float bias))
                score += bias;

            return score;
        }

        private static float GameplayRelevance(ContentIndexEntry e, ShotCategory shot)
        {
            switch (shot)
            {
                case ShotCategory.Army:
                case ShotCategory.Combat:
                case ShotCategory.Tactical:
                    return e.category == MarketingContentCategory.Unit ? 3f : -2f;
                case ShotCategory.Building:
                case ShotCategory.Settlement:
                case ShotCategory.Economy:
                    return e.category == MarketingContentCategory.Building ? 3f : -2f;
                case ShotCategory.WorldBeauty:
                case ShotCategory.Atmosphere:
                case ShotCategory.Exploration:
                    return e.category == MarketingContentCategory.Environment
                        || e.category == MarketingContentCategory.Landmark
                        || e.category == MarketingContentCategory.Terrain ? 2f : 0f;
                default:
                    return 0f;
            }
        }

        private static float ValueScore(ContentIndexEntry e) => e.marketingValue switch
        {
            MarketingValueTier.Hero => 2f,
            MarketingValueTier.Supporting => 1f,
            _ => 0f,
        };
    }
}
