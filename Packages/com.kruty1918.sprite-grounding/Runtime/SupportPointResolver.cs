using System;
using UnityEngine;

namespace Kruty1918.SpriteGrounding
{
    /// <summary>
    /// Solves the ground-contact (support) point of a sprite/card in UV
    /// space. Bottom row of the result always sits on
    /// <see cref="VisibleBounds.Uv"/>.yMin so callers can land the visible
    /// bottom edge exactly on the surface.
    /// </summary>
    public static class SupportPointResolver
    {
        /// <summary>Pixel-free variants; falls back to bottom-centre.</summary>
        public static Vector2 SolveUv(in VisibleBounds bounds, SupportPointMode mode)
        {
            if (!bounds.HasContent)
                return new Vector2(0.5f, 0f);
            return new Vector2(bounds.Uv.xMin + bounds.Uv.width * 0.5f, bounds.Uv.yMin);
        }

        /// <summary>
        /// Full variant over the analyzed pixels. Iteration order is fixed
        /// (bottom row up, left to right) so the centroid is deterministic.
        /// </summary>
        public static Vector2 SolveUv(
            in VisibleBounds bounds,
            ReadOnlySpan<Color32> pixels,
            int width,
            int height,
            float alphaThreshold,
            in SpriteGroundingProfile profile)
        {
            if (!bounds.HasContent || pixels.Length < width * height)
                return new Vector2(0.5f, 0f);

            if (profile.Support == SupportPointMode.BottomCenter)
                return new Vector2(bounds.Uv.xMin + bounds.Uv.width * 0.5f, bounds.Uv.yMin);

            byte cutoff = AlphaBoundsAnalyzer.ThresholdByte(alphaThreshold);
            int bandRows = Math.Max(1, profile.SupportBandRows);
            int y0 = bounds.Pixels.yMin;
            int y1 = Math.Min(bounds.Pixels.yMax, y0 + bandRows - 1);
            int x0 = bounds.Pixels.xMin;
            int x1 = bounds.Pixels.xMax;

            long sumX = 0;
            int count = 0;
            for (int y = y0; y <= y1; y++)
            {
                int row = y * width;
                for (int x = x0; x <= x1; x++)
                {
                    if (pixels[row + x].a < cutoff)
                        continue;
                    sumX += x;
                    count++;
                }
            }

            if (count == 0)
                return new Vector2(bounds.Uv.xMin + bounds.Uv.width * 0.5f, bounds.Uv.yMin);

            float u = (float)((double)sumX / count) + 0.5f;
            return new Vector2(u / width, bounds.Uv.yMin);
        }
    }
}
