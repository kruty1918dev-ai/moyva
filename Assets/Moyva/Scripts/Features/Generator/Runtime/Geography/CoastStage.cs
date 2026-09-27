using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Geography
{
    /// <summary>
    /// Shapes coastlines: variable-width beaches on low coasts, cliff coasts on
    /// elevated edges, shallow-water banding. Produces the beach mask and
    /// adjusts water surfaces for depth differentiation.
    /// </summary>
    internal sealed class CoastStage
    {
        internal bool[,] Generate(
            WorldGenerationRequest request,
            int[,] levels,
            bool[,] lakeMask,
            bool[,] riverMask,
            float[,] waterSurface)
        {
            int w = request.Width;
            int h = request.Height;
            int water = request.WaterLevel;
            int shore = request.ShoreLevel;
            var coast = request.Config.Coast;
            int seed = request.Seed;

            var beach = new bool[w, h];
            int beachWidth = Mathf.Max(0, coast.BeachWidth);

            // Beach: low land touching water. Elevated coasts stay cliffs.
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (levels[x, y] == water || riverMask[x, y] || lakeMask[x, y])
                    continue;
                if (!TerrainLevelStage.TouchesWater(levels, x, y, w, h, water)
                    && !TouchesMask(lakeMask, x, y, w, h)
                    && !TouchesMask(riverMask, x, y, w, h))
                    continue;

                int level = levels[x, y];
                if (level <= shore + 1)
                {
                    beach[x, y] = true;
                    levels[x, y] = shore;
                }
                // level >= hill+1 → cliff coast, no beach
            }

            // Widen the beach inland with noise-gated second ring.
            if (beachWidth > 1)
            {
                var add = new bool[w, h];
                for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    if (beach[x, y] || levels[x, y] == water || levels[x, y] > shore)
                        continue;
                    if (!TouchesMask(beach, x, y, w, h))
                        continue;
                    if (DeterministicNoise.Hash01(seed, x, y + 401) < 0.55f)
                    {
                        add[x, y] = true;
                        levels[x, y] = shore;
                    }
                }
                for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    if (add[x, y]) beach[x, y] = true;
            }

            // Depth bands: water near land is shallower (higher surface).
            // Eligibility is evaluated against a per-ring snapshot — otherwise the
            // same-ring writes would let the band propagate across open water.
            int shallow = Mathf.Max(0, coast.ShallowBandWidth);
            float deepSurface = water * request.HeightStep + request.WaterSurfaceOffset;
            var shallowed = new bool[w, h];
            for (int ring = 0; ring < shallow; ring++)
            {
                var newly = new List<Vector2Int>();
                for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    if (levels[x, y] != water || lakeMask[x, y] || riverMask[x, y] || shallowed[x, y])
                        continue;
                    if (!TouchesLandOrShallowed(levels, shallowed, water, x, y, w, h))
                        continue;
                    newly.Add(new Vector2Int(x, y));
                }

                float depthT = (float)(ring + 1) / (shallow + 1);
                foreach (Vector2Int cell in newly)
                {
                    shallowed[cell.x, cell.y] = true;
                    waterSurface[cell.x, cell.y] = Mathf.Max(
                        waterSurface[cell.x, cell.y],
                        deepSurface + 0.1f * depthT);
                }
            }

            return beach;
        }

        private static bool TouchesMask(bool[,] mask, int x, int y, int w, int h)
        {
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx, ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                if (mask[nx, ny]) return true;
            }
            return false;
        }

        private static bool TouchesLandOrShallowed(
            int[,] levels, bool[,] shallowed, int waterLevel,
            int x, int y, int w, int h)
        {
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx, ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
                if (levels[nx, ny] != waterLevel || shallowed[nx, ny])
                    return true;
            }
            return false;
        }
    }
}
