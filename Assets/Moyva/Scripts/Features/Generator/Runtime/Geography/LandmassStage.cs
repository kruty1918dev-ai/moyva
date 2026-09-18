using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Geography
{
    /// <summary>
    /// Produces the macro-scale land/ocean mask for the chosen archetype:
    /// seeded super-ellipse continent centres + domain-warped coastline noise.
    /// Output: landMask[x,y] in [0,1] — &gt;0.5 ≈ land.
    /// </summary>
    internal sealed class LandmassStage
    {
        internal float[,] Generate(WorldGenerationRequest request)
        {
            int w = request.Width;
            int h = request.Height;
            var p = request.Config.FindArchetype(request.ArchetypeId());
            int seed = request.Seed;

            var centres = BuildCentres(w, h, seed, p);
            float halfExtent = Mathf.Min(w, h) * 0.5f;
            float baseRadius = halfExtent * p.CentreRadius;
            float warp = p.CoastWarp;
            float noiseScale = Mathf.Max(6f, halfExtent * 0.35f);

            var mask = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float best = 0f;
                for (int c = 0; c < centres.Length; c++)
                {
                    ref readonly var centre = ref centres[c];
                    float dx = (x - centre.x) / (baseRadius * centre.rx);
                    float dy = (y - centre.y) / (baseRadius * centre.ry);
                    // super-ellipse-ish falloff (exponent ~2.7 → rounded-square landmasses)
                    float d = Mathf.Pow(Mathf.Abs(dx), 2.7f) + Mathf.Pow(Mathf.Abs(dy), 2.7f);
                    d = Mathf.Pow(d, 1f / 2.7f);
                    float local = Mathf.Clamp01(1f - d);
                    local = Smooth(local);
                    if (local > best)
                        best = local;
                }

                float coast = DeterministicNoise.WarpedFbm(
                    seed + 31, x / noiseScale, y / noiseScale, warp / noiseScale, 4);
                mask[x, y] = Mathf.Clamp01(best * 0.75f + (coast - 0.5f) * 0.55f + best * coast * 0.35f);
            }

            NormalizeToTarget(mask, w, h, p.LandRatio);
            return mask;
        }

        /// <summary>Rescale the mask so ~landRatio of cells exceed 0.5.</summary>
        private static void NormalizeToTarget(float[,] mask, int w, int h, float targetLand)
        {
            // Flatten + sort to find the threshold giving the target land ratio.
            int n = w * h;
            var values = new float[n];
            int i = 0;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                values[i++] = mask[x, y];
            System.Array.Sort(values);
            int landIndex = Mathf.Clamp(n - Mathf.RoundToInt(n * targetLand), 0, n - 1);
            float threshold = Mathf.Max(0.02f, values[landIndex]);
            float inv = 1f / Mathf.Max(0.001f, 1f - threshold);
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float v = mask[x, y];
                mask[x, y] = v >= threshold
                    ? 0.5f + (v - threshold) * inv * 0.5f
                    : v / threshold * 0.5f;
            }
        }

        private struct Centre { public float x, y, rx, ry; }

        private static Centre[] BuildCentres(int w, int h, int seed, API.WorldGenerationConfig.ArchetypeParameters p)
        {
            int count = Mathf.Max(1, p.CentreCount);
            var centres = new Centre[count];
            float cx = w * 0.5f;
            float cy = h * 0.5f;
            float ring = Mathf.Min(w, h) * 0.22f;

            if (count == 1)
            {
                centres[0] = JitteredCentre(cx, cy, 1.15f, seed, 0);
                return centres;
            }

            // Ring distribution with deterministic angular jitter — continents spread
            // around the map middle rather than all clustering in one corner.
            float angleStep = Mathf.PI * 2f / count;
            float baseAngle = DeterministicNoise.Hash01(seed, 41, 7) * Mathf.PI * 2f;
            for (int i = 0; i < count; i++)
            {
                float jitterAngle = baseAngle + i * angleStep
                    + DeterministicNoise.Jitter(seed, i, 11, 3) * angleStep * 0.35f;
                float jitterR = ring * (0.55f + DeterministicNoise.Hash01(seed, i, 23) * 0.7f);
                float x = cx + Mathf.Cos(jitterAngle) * jitterR;
                float y = cy + Mathf.Sin(jitterAngle) * jitterR;
                centres[i] = JitteredCentre(x, y, 0.85f + DeterministicNoise.Hash01(seed, i, 37) * 0.55f, seed, i + 1);
            }
            return centres;
        }

        private static Centre JitteredCentre(float x, float y, float scale, int seed, int salt)
        {
            return new Centre
            {
                x = x + DeterministicNoise.Jitter(seed, salt, 3, 5) * 4f,
                y = y + DeterministicNoise.Jitter(seed, salt, 5, 7) * 4f,
                rx = scale * (0.85f + DeterministicNoise.Hash01(seed, salt, 51) * 0.5f),
                ry = scale * (0.85f + DeterministicNoise.Hash01(seed, salt, 67) * 0.5f),
            };
        }

        private static float Smooth(float t) => t * t * (3f - 2f * t);
    }

    internal static class WorldGenerationRequestArchetypeExt
    {
        internal static string ArchetypeId(this WorldGenerationRequest request)
            => WorldArchetypeResolver.IdOf(request.Archetype);
    }
}
