using System;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Chooses a safe startup fallback reveal center when bootstrap reveal data is missing.
    /// </summary>
    internal static class FogStartupFallbackRevealSelector
    {
        public static FogStartupFallbackRevealSelection SelectCenter(int width, int height, FogOfWarSettings settings)
        {
            int minSide = Mathf.Min(width, height);
            int relativeMargin = Mathf.FloorToInt(minSide * Mathf.Clamp01(settings != null ? settings.StartupFallbackRelativeMarginFactor : 0.1667f));
            int margin = Mathf.Max(settings != null ? settings.StartupFallbackMinMarginFromBorder : 5, relativeMargin);

            int xMin = Mathf.Clamp(margin, 0, Mathf.Max(0, width - 1));
            int xMax = Mathf.Clamp(width - margin - 1, xMin, Mathf.Max(0, width - 1));
            int yMin = Mathf.Clamp(margin, 0, Mathf.Max(0, height - 1));
            int yMax = Mathf.Clamp(height - margin - 1, yMin, Mathf.Max(0, height - 1));
            int seed = CreateRandomSeed();
            var random = new System.Random(seed);
            var center = new Vector2Int(
                random.Next(xMin, xMax + 1),
                random.Next(yMin, yMax + 1));

            return new FogStartupFallbackRevealSelection(center, seed, xMin, xMax, yMin, yMax);
        }

        private static int CreateRandomSeed()
        {
            unchecked
            {
                return Environment.TickCount ^ Guid.NewGuid().GetHashCode() ^ (Time.frameCount * 397);
            }
        }
    }

    internal readonly struct FogStartupFallbackRevealSelection
    {
        public FogStartupFallbackRevealSelection(Vector2Int center, int seed, int xMin, int xMax, int yMin, int yMax)
        {
            Center = center;
            Seed = seed;
            XMin = xMin;
            XMax = xMax;
            YMin = yMin;
            YMax = yMax;
        }

        public Vector2Int Center { get; }

        public int Seed { get; }

        public int XMin { get; }

        public int XMax { get; }

        public int YMin { get; }

        public int YMax { get; }
    }
}
