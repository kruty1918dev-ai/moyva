using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Deterministic noise functions for procedural generation.
    /// Provides stable, repeatable noise values for coordinates and seeds.
    /// </summary>
    internal sealed class DeterministicNoise
    {
        /// <summary>
        /// Generate 2D value noise with the given frequency and seed.
        /// </summary>
        public float ValueNoise2D(int x, int y, int seed, int frequency)
        {
            if (frequency <= 0)
                frequency = 1;

            float fx = (float)x / frequency;
            float fy = (float)y / frequency;

            int x0 = Mathf.FloorToInt(fx);
            int y0 = Mathf.FloorToInt(fy);
            int x1 = x0 + 1;
            int y1 = y0 + 1;

            float sx = SmoothStep(fx - x0);
            float sy = SmoothStep(fy - y0);

            float n00 = HashNoise(x0, y0, seed);
            float n10 = HashNoise(x1, y0, seed);
            float n01 = HashNoise(x0, y1, seed);
            float n11 = HashNoise(x1, y1, seed);

            float nx0 = Mathf.Lerp(n00, n10, sx);
            float nx1 = Mathf.Lerp(n01, n11, sx);

            return Mathf.Lerp(nx0, nx1, sy);
        }

        /// <summary>
        /// Generate fractal Brownian motion (fBm) noise by octaves.
        /// </summary>
        public float FbmNoise2D(int x, int y, int seed, int frequency, int octaves, float persistence)
        {
            float total = 0f;
            float amplitude = 1f;
            float maxValue = 0f;
            int currentFrequency = frequency;

            for (int i = 0; i < octaves; i++)
            {
                total += ValueNoise2D(x, y, seed, currentFrequency) * amplitude;
                maxValue += amplitude;
                amplitude *= persistence;
                currentFrequency *= 2;
            }

            return total / maxValue;
        }

        private static float HashNoise(int x, int y, int seed)
        {
            uint hash = (uint)seed;
            hash = Mix(hash, (uint)x);
            hash = Mix(hash, (uint)y);
            hash = Finalize(hash);
            return (hash % 10000) / 10000f;
        }

        private static uint Mix(uint hash, uint value)
        {
            hash ^= value;
            hash *= 0x51d7346e;
            hash ^= hash >> 16;
            return hash;
        }

        private static uint Finalize(uint hash)
        {
            hash *= 0x85ebca6b;
            hash ^= hash >> 13;
            hash *= 0xc2b2ae35;
            hash ^= hash >> 16;
            return hash;
        }

        private static float SmoothStep(float t)
        {
            return t * t * (3f - 2f * t);
        }
    }
}
