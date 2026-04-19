using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Noise
{
    internal static class ProceduralNoiseUtility
    {
        public static float Hash01(int x, int y, int seed)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263 + seed * 362437;
                h = (h ^ (h >> 13)) * 1274126177;
                h ^= h >> 16;
                return (h & 0x7fffffff) / 2147483647f;
            }
        }

        public static float SamplePerlin(float x, float y)
        {
            return Mathf.PerlinNoise(x, y);
        }

        public static float SampleSimplexLike(float x, float y, int seed)
        {
            // Lightweight gradient/value-noise approximation suitable for generation previews.
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            float tx = x - x0;
            float ty = y - y0;

            float v00 = Hash01(x0, y0, seed);
            float v10 = Hash01(x1, y0, seed);
            float v01 = Hash01(x0, y1, seed);
            float v11 = Hash01(x1, y1, seed);

            float sx = tx * tx * (3f - 2f * tx);
            float sy = ty * ty * (3f - 2f * ty);

            float ix0 = Mathf.Lerp(v00, v10, sx);
            float ix1 = Mathf.Lerp(v01, v11, sx);
            return Mathf.Lerp(ix0, ix1, sy);
        }

        public static float SampleWorley(float x, float y, int seed, float cellDensity)
        {
            float gx = x * cellDensity;
            float gy = y * cellDensity;

            int cx = Mathf.FloorToInt(gx);
            int cy = Mathf.FloorToInt(gy);
            float best = float.MaxValue;

            for (int ox = -1; ox <= 1; ox++)
            {
                for (int oy = -1; oy <= 1; oy++)
                {
                    int nx = cx + ox;
                    int ny = cy + oy;

                    float fx = Hash01(nx, ny, seed);
                    float fy = Hash01(nx, ny, seed ^ 0x6e624eb7);

                    float px = nx + fx;
                    float py = ny + fy;

                    float dx = px - gx;
                    float dy = py - gy;
                    float d = dx * dx + dy * dy;
                    if (d < best) best = d;
                }
            }

            return 1f - Mathf.Clamp01(Mathf.Sqrt(best));
        }

        public static float SampleFbm(float x, float y, int octaves, float lacunarity, float persistence,
            int seed, bool simplexBase)
        {
            float sum = 0f;
            float amp = 1f;
            float freq = 1f;
            float maxAmp = 0f;

            for (int o = 0; o < octaves; o++)
            {
                float sx = x * freq;
                float sy = y * freq;
                float n = simplexBase
                    ? SampleSimplexLike(sx, sy, seed + o * 977)
                    : SamplePerlin(sx, sy);

                sum += n * amp;
                maxAmp += amp;
                amp *= persistence;
                freq *= lacunarity;
            }

            if (maxAmp <= 0f) return 0f;
            return Mathf.Clamp01(sum / maxAmp);
        }

        public static void Normalize(float[,] map)
        {
            int w = map.GetLength(0);
            int h = map.GetLength(1);

            float min = float.MaxValue;
            float max = float.MinValue;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float v = map[x, y];
                    if (v < min) min = v;
                    if (v > max) max = v;
                }
            }

            float span = Mathf.Max(0.00001f, max - min);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    map[x, y] = Mathf.Clamp01((map[x, y] - min) / span);
                }
            }
        }
    }
}
