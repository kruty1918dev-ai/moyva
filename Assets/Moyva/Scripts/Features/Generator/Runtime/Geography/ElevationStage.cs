using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Geography
{
    /// <summary>
    /// Turns the landmask into a continuous elevation field: continental shelf,
    /// ridged mountain chains, rolling hills and carved valleys.
    /// Output: elevation[x,y] in [0,1] (0 = deep ocean, 1 = peaks).
    /// </summary>
    internal sealed class ElevationStage
    {
        internal float[,] Generate(WorldGenerationRequest request, float[,] landMask)
        {
            int w = request.Width;
            int h = request.Height;
            var p = request.Config.FindArchetype(request.ArchetypeId());
            int seed = request.Seed;
            float halfExtent = Mathf.Min(w, h) * 0.5f;

            var mountainField = BuildMountainField(w, h, seed, p, landMask, halfExtent);
            var elevation = new float[w, h];
            float hillScale = Mathf.Max(6f, halfExtent * 0.16f);
            float valleyScale = Mathf.Max(10f, halfExtent * 0.3f);

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float land = Mathf.Clamp01((landMask[x, y] - 0.5f) * 2f);
                float baseElev = land <= 0f
                    ? Mathf.Lerp(0.02f, 0.32f, Mathf.Clamp01(landMask[x, y] * 2f)) // shelf → deep
                    : Mathf.Lerp(0.42f, 0.58f, Mathf.Sqrt(land));                 // coastal → interior

                float hills = DeterministicNoise.WarpedFbm(
                    seed + 101, x / hillScale, y / hillScale, 0.35f, 4);
                float valleys = DeterministicNoise.Fbm(
                    seed + 211, x / valleyScale, y / valleyScale, 3);
                float ridge = mountainField[x, y];

                float elev = baseElev
                    + (hills - 0.5f) * 0.16f * p.HillDensity * Mathf.Max(0.2f, land)
                    + ridge * 0.5f
                    - Mathf.Max(0f, valleys - 0.62f) * 0.22f * Mathf.Max(0.2f, land);

                elevation[x, y] = Mathf.Clamp01(elev);
            }

            return elevation;
        }

        /// <summary>
        /// Coherent mountain ranges: a few seeded tectonic arcs across landmasses,
        /// ridged noise sampled in a band around each arc, gated by mountainDensity.
        /// </summary>
        private float[,] BuildMountainField(
            int w, int h, int seed,
            API.WorldGenerationConfig.ArchetypeParameters p,
            float[,] landMask,
            float halfExtent)
        {
            var field = new float[w, h];
            int rangeCount = Mathf.Max(0, p.MountainRangeCount);
            if (rangeCount == 0 || p.MountainDensity <= 0.001f)
                return field;

            float rangeScale = Mathf.Max(8f, halfExtent * 0.12f);
            float bandWidth = Mathf.Max(4f, halfExtent * 0.1f);

            // Seed a few wandering arcs: each defined by anchor + heading + curvature.
            for (int r = 0; r < rangeCount; r++)
            {
                float ax = w * (0.15f + 0.7f * DeterministicNoise.Hash01(seed, r, 701));
                float ay = h * (0.15f + 0.7f * DeterministicNoise.Hash01(seed, r, 709));
                float heading = DeterministicNoise.Hash01(seed, r, 717) * Mathf.PI * 2f;
                float curvature = (DeterministicNoise.Hash01(seed, r, 719) - 0.5f) * 0.55f;
                float length = halfExtent * (0.8f + DeterministicNoise.Hash01(seed, r, 727) * 1.2f);
                int steps = Mathf.CeilToInt(length);

                float x = ax, y = ay, dir = heading;
                for (int s = 0; s < steps; s++)
                {
                    StampRidge(field, w, h, x, y, bandWidth, seed, r, s, rangeScale, p.MountainDensity);
                    dir += curvature * 0.1f;
                    x += Mathf.Cos(dir);
                    y += Mathf.Sin(dir);
                }
            }
            return field;
        }

        private static void StampRidge(
            float[,] field, int w, int h, float cx, float cy, float band,
            int seed, int range, int step, float rangeScale, float density)
        {
            int radius = Mathf.CeilToInt(band);
            int x0 = Mathf.Max(0, Mathf.FloorToInt(cx) - radius);
            int x1 = Mathf.Min(w - 1, Mathf.FloorToInt(cx) + radius);
            int y0 = Mathf.Max(0, Mathf.FloorToInt(cy) - radius);
            int y1 = Mathf.Min(h - 1, Mathf.FloorToInt(cy) + radius);
            for (int x = x0; x <= x1; x++)
            for (int y = y0; y <= y1; y++)
            {
                float d = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) / band;
                if (d >= 1f)
                    continue;

                float falloff = 1f - d * d;
                float ridge = DeterministicNoise.Ridged(
                    seed + range * 131 + 4001,
                    x / rangeScale,
                    y / rangeScale,
                    4);
                float value = falloff * Mathf.Pow(ridge, 1.4f) * density;
                if (value > field[x, y])
                    field[x, y] = value;
            }
        }
    }
}
