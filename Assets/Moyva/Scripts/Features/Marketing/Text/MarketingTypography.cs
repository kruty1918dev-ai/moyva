using System;
using Kruty1918.Moyva.Marketing.Contracts;
using UnityEngine;

namespace Kruty1918.Moyva.Marketing.Text
{
    public enum TextTemplate
    {
        Title = 0,       // large centered, end cards / hero stills
        BeatWord = 1,    // single strong word, lower third
        ShortPhrase = 2, // up to ~5 words, lower third
        EndCard = 3,     // centered block: title + optional sub-line
    }

    /// <summary>
    /// Typography layout: where text sits for a given aspect + safe area.
    /// Pure math — returns normalized viewport rects the overlay rig renders.
    /// </summary>
    public static class MarketingTypography
    {
        public sealed class Layout
        {
            public Rect anchorRect;       // normalized viewport rect (0..1)
            public float fontScale;       // relative to template base
            public TextAnchor anchor;
        }

        /// <summary>Safe-area inset fraction per aspect (portrait needs more).</summary>
        public static float SafeMargin(float aspect, float profileMargin)
        {
            float baseMargin = Mathf.Max(0.02f, profileMargin);
            if (aspect < 0.8f) return baseMargin + 0.06f;  // 9:16
            if (aspect < 1.1f) return baseMargin + 0.03f;  // 1:1 / 4:5
            return baseMargin;
        }

        public static Layout Resolve(TextTemplate template, float aspect, float profileMargin)
        {
            float m = SafeMargin(aspect, profileMargin);
            var layout = new Layout { anchor = TextAnchor.MiddleCenter };

            switch (template)
            {
                case TextTemplate.Title:
                    layout.anchorRect = new Rect(m, 0.55f, 1f - 2f * m, 0.30f);
                    layout.fontScale = aspect < 0.8f ? 0.9f : 1.0f;
                    layout.anchor = TextAnchor.MiddleCenter;
                    break;
                case TextTemplate.BeatWord:
                    layout.anchorRect = new Rect(m, m + 0.02f, 1f - 2f * m, 0.16f);
                    layout.fontScale = 0.8f;
                    layout.anchor = TextAnchor.LowerCenter;
                    break;
                case TextTemplate.ShortPhrase:
                    layout.anchorRect = new Rect(m + 0.04f, m + 0.04f, 1f - 2f * (m + 0.04f), 0.14f);
                    layout.fontScale = 0.6f;
                    layout.anchor = TextAnchor.LowerCenter;
                    break;
                case TextTemplate.EndCard:
                    layout.anchorRect = new Rect(m, 0.25f, 1f - 2f * m, 0.5f);
                    layout.fontScale = 1.1f;
                    layout.anchor = TextAnchor.MiddleCenter;
                    break;
            }
            return layout;
        }

        /// <summary>Guard: does a text block fit its rect at a given character
        /// count? Rejects copy that would overflow a lower-third.</summary>
        public static bool Fits(string text, TextTemplate template)
        {
            int max = template switch
            {
                TextTemplate.BeatWord => 14,
                TextTemplate.ShortPhrase => 42,
                TextTemplate.Title => 24,
                TextTemplate.EndCard => 60,
                _ => 42,
            };
            return !string.IsNullOrEmpty(text) && text.Length <= max;
        }
    }
}
