using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Geography
{
    /// <summary>
    /// Quantizes continuous elevation into integer terrain levels — the real
    /// stepped-terrace structure the chunk-first renderer extrudes. The mapping
    /// is a pure function of elevation (no per-cell jitter): equal heights map
    /// to equal levels, so neighbouring chunks and repeated seeds agree exactly.
    /// Cells adjacent to water are pinned to shore level so beaches read
    /// correctly, and isolated single-tile spikes/pits are snapped to a
    /// unanimous surrounding level by an explicit local rule.
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
                int level = Mathf.Clamp(
                    Mathf.RoundToInt(Mathf.Lerp(land, max, Mathf.Pow(landT, 1.15f))),
                    land, max);
                levels[x, y] = level;
            }

            FlattenIsolatedSteps(levels, w, h, water);

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

        /// <summary>
        /// Explicit single-tile-step rule: a non-water cell surrounded entirely
        /// by cells of one identical non-water level snaps to that level. Only
        /// unanimous neighbourhoods are touched — mixed neighbourhoods keep
        /// their tactical height differences (no global smoothing).
        /// </summary>
        private static void FlattenIsolatedSteps(int[,] levels, int w, int h, int waterLevel)
        {
            var snapped = (int[,])levels.Clone();
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (levels[x, y] == waterLevel)
                    continue;

                int neighbour = -1;
                bool unanimous = true;
                int seen = 0;
                for (int dx = -1; dx <= 1 && unanimous; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx, ny = y + dy;
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                    int nl = levels[nx, ny];
                    if (nl == waterLevel) { unanimous = false; break; }
                    seen++;
                    if (neighbour < 0) neighbour = nl;
                    else if (nl != neighbour) { unanimous = false; break; }
                }
                // Need a real neighbourhood (interior or thick-enough edge) so a
                // lone land pillar at a map corner isn't silently levelled.
                if (unanimous && seen >= 3 && neighbour != levels[x, y])
                    snapped[x, y] = neighbour;
            }
            System.Array.Copy(snapped, levels, levels.Length);
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
