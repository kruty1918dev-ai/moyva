using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class TileWorldCreatorTerrainLevelResolver
    {
        public static int ResolveVisualTerrainLevel(
            string biomeId,
            int capturedLevel,
            int capturedMin,
            int capturedMax,
            bool nearWater,
            TileWorldCreatorBuildOptions options)
        {
            if (TileWorldCreatorTerrainBiomeClassifier.IsWater(biomeId))
                return options.WaterTerrainLevel;

            string lower = biomeId?.ToLowerInvariant() ?? string.Empty;
            if (TileWorldCreatorTerrainBiomeClassifier.IsSand(lower) && nearWater)
                return Mathf.Max(options.ShoreTerrainLevel, options.WaterTerrainLevel);
            if (lower.Contains("mountain") || lower.Contains("snow"))
                return options.MaxTerrainLevel;
            if (lower.Contains("hill") || lower.Contains("stone"))
                return Mathf.Clamp(options.HillTerrainLevel, options.LandTerrainLevel, options.MaxTerrainLevel);

            int explicitLevel = ResolveExplicitLevelFromTileId(lower);
            if (explicitLevel >= 0)
                return Mathf.Clamp(explicitLevel, options.LandTerrainLevel, options.MaxTerrainLevel);

            return ProjectCapturedLevel(capturedLevel, capturedMin, capturedMax, options.LandTerrainLevel, options.MaxTerrainLevel);
        }

        public static int ResolveBiomeFallbackLevel(string biomeId)
        {
            if (string.IsNullOrWhiteSpace(biomeId))
                return 1;

            string lower = biomeId.ToLowerInvariant();
            if (TileWorldCreatorTerrainBiomeClassifier.IsWater(lower))
                return 0;
            if (lower.Contains("sand") || lower.Contains("beach") || lower.Contains("coast"))
                return 1;
            if (lower.Contains("mountain") || lower.Contains("snow"))
                return 3;
            if (lower.Contains("hill") || lower.Contains("forest"))
                return 2;

            return 1;
        }

        public static void FindCapturedLandLevelRange(GeneratedWorldData worldData, int width, int height, out int min, out int max)
        {
            min = int.MaxValue;
            max = int.MinValue;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (TileWorldCreatorTerrainBiomeClassifier.IsWater(worldData.BiomeMap[x, y]))
                    continue;

                int value = worldData.TerrainLevelMap[x, y];
                min = Mathf.Min(min, value);
                max = Mathf.Max(max, value);
            }

            if (min == int.MaxValue || max == int.MinValue)
            {
                min = 0;
                max = 0;
            }
        }

        public static void FindLandHeightRange(GeneratedWorldData worldData, int width, int height, out float min, out float max)
        {
            min = float.PositiveInfinity;
            max = float.NegativeInfinity;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (TileWorldCreatorTerrainBiomeClassifier.IsWater(worldData.BiomeMap[x, y]))
                    continue;

                float value = worldData.HeightMap[x, y];
                min = Mathf.Min(min, value);
                max = Mathf.Max(max, value);
            }

            if (float.IsInfinity(min) || float.IsInfinity(max))
            {
                min = 0f;
                max = 0f;
            }
        }

        private static int ResolveExplicitLevelFromTileId(string lower)
        {
            const string marker = "level-";
            int index = string.IsNullOrWhiteSpace(lower) ? -1 : lower.IndexOf(marker, System.StringComparison.Ordinal);
            int start = index + marker.Length;
            return index >= 0 && start < lower.Length && char.IsDigit(lower[start]) ? lower[start] - '0' : -1;
        }

        private static int ProjectCapturedLevel(int capturedLevel, int capturedMin, int capturedMax, int targetMin, int targetMax)
        {
            if (targetMax <= targetMin)
                return targetMin;
            if (capturedMax <= capturedMin)
                return Mathf.Clamp(capturedLevel, targetMin, targetMax);

            float t = Mathf.InverseLerp(capturedMin, capturedMax, capturedLevel);
            return Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(targetMin, targetMax, t)), targetMin, targetMax);
        }
    }
}
