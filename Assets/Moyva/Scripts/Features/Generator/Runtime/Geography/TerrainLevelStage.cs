using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Geography
{
    /// <summary>
    /// Quantizes continuous elevation into integer terrain levels — the real
    /// stepped-terrace structure the chunk-first renderer extrudes. Cells
    /// adjacent to water are pinned to shore level so beaches read correctly.
    /// </summary>
    internal sealed class TerrainLevelStage
    {
        internal int[,] Quantize(WorldGenerationRequest request, float[,] elevation, float[,] landMask)
        {
            int w = request.Width;
            int h = request.Height;
            int water = request.WaterLevel;
            int shore = request.ShoreLevel;
            int land = request.LandLevel;
            int max = request.MaxLevel;
            int seed = request.Seed;

            const float seaLevel = 0.36f;
            var levels = new int[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (elevation[x, y] < seaLevel || landMask[x, y] < 0.5f)
                {
                    levels[x, y] = water;
                    continue;
                }

                float landT = Mathf.InverseLerp(seaLevel, 1f, elevation[x, y]);
                float jittered = Mathf.Clamp01(
                    landT + DeterministicNoise.Jitter(seed, x, y, 97) * 0.09f);
                int level = Mathf.Clamp(
                    Mathf.RoundToInt(Mathf.Lerp(land, max, Mathf.Pow(jittered, 1.15f))),
                    land, max);
                levels[x, y] = level;
            }

            // Coastal land cells settle to shore level; elevated cliffs stay high.
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (levels[x, y] == water || !TouchesWater(levels, x, y, w, h, water))
                    continue;
                if (levels[x, y] <= land + 1)
                    levels[x, y] = shore;
            }

            return levels;
        }

        internal static bool TouchesWater(int[,] levels, int x, int y, int w, int h, int waterLevel)
        {
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx, ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                if (levels[nx, ny] == waterLevel)
                    return true;
            }
            return false;
        }
    }
}
