using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Geography
{
    /// <summary>
    /// Deterministic, platform-stable noise helpers for world generation.
    /// All values derive from integer hashing — no UnityEngine.Random or
    /// Mathf.PerlinNoise, so results are identical across machines.
    /// </summary>
    internal static class DeterministicNoise
    {
        /// <summary>Integer lattice hash → [0,1). Stable across runs/platforms.</summary>
        internal static float Hash01(int seed, int x, int y)
        {
            unchecked
            {
                uint h = (uint)seed;
                h ^= (uint)x * 0x9E3779B1u;
                h = RotateLeft(h, 13);
                h ^= (uint)y * 0x85EBCA6Bu;
                h = RotateLeft(h, 13);
                h ^= h >> 16;
                h *= 0x7FEB352Du;
                h ^= h >> 15;
                h *= 0x846CA68Bu;
                h ^= h >> 16;
                return (h & 0xFFFFFF) / 16777216f;
            }
        }

        /// <summary>Channel-salted variant of <see cref="Hash01(int,int,int)"/>.</summary>
        internal static float Hash01(int seed, int x, int y, int channel)
            => Hash01(seed ^ (channel * 0x6329), x, y);

        internal static uint HashU32(int seed, int x, int y, int channel)
        {
            unchecked
            {
                uint h = (uint)seed ^ ((uint)channel * 0xA24BAED7u);
                h ^= (uint)x * 0x9E3779B1u;
                h = RotateLeft(h, 13);
                h ^= (uint)y * 0x85EBCA6Bu;
                h = RotateLeft(h, 13);
                h ^= h >> 16;
                h *= 0x7FEB352Du;
                h ^= h >> 15;
                h *= 0x846CA68Bu;
                h ^= h >> 16;
                return h;
            }
        }

        /// <summary>Hash for an arbitrary (x, y, channel, attempt) tuple.</summary>
        internal static int HashInt(int seed, int x, int y, int channel, int attempt = 0)
        {
            unchecked
            {
                uint h = HashU32(seed, x, y, channel) ^ ((uint)attempt * 0xD1B54A35u);
                h ^= h >> 13;
                h *= 0x9E3779B9u;
                h ^= h >> 16;
                return (int)(h & 0x7FFFFFFF);
            }
        }

        /// <summary>Derives a deterministic per-attempt seed.</summary>
        internal static int AttemptSeed(int seed, int attempt)
            => HashInt(seed, 0x5EED, attempt, 0xBEEF) | 1;

        /// <summary>Smooth value noise at fractional coordinates, output [0,1].</summary>
        internal static float Value(float seed, float x, float y)
        {
            int ix = Mathf.FloorToInt(x);
            int iy = Mathf.FloorToInt(y);
            float fx = x - ix;
            float fy = y - iy;
            float sx = fx * fx * (3f - 2f * fx);
            float sy = fy * fy * (3f - 2f * fy);

            float v00 = Hash01((int)seed, ix, iy);
            float v10 = Hash01((int)seed, ix + 1, iy);
            float v01 = Hash01((int)seed, ix, iy + 1);
            float v11 = Hash01((int)seed, ix + 1, iy + 1);

            return Mathf.Lerp(
                Mathf.Lerp(v00, v10, sx),
                Mathf.Lerp(v01, v11, sx),
                sy);
        }

        /// <summary>Fractal Brownian motion over value noise, output roughly [0,1].</summary>
        internal static float Fbm(float seed, float x, float y, int octaves = 4, float lacunarity = 2f, float gain = 0.5f)
        {
            float sum = 0f;
            float amplitude = 1f;
            float frequency = 1f;
            float norm = 0f;
            for (int i = 0; i < octaves; i++)
            {
                sum += Value(seed + i * 1013f, x * frequency, y * frequency) * amplitude;
                norm += amplitude;
                amplitude *= gain;
                frequency *= lacunarity;
            }
            return norm > 0f ? sum / norm : 0f;
        }

        /// <summary>Ridged multifractal — sharp crests for mountain chains. Output [0,1].</summary>
        internal static float Ridged(float seed, float x, float y, int octaves = 4, float lacunarity = 2f, float gain = 0.55f)
        {
            float sum = 0f;
            float amplitude = 0.55f;
            float frequency = 1f;
            float norm = 0f;
            float prev = 1f;
            for (int i = 0; i < octaves; i++)
            {
                float n = Value(seed + i * 3571f, x * frequency, y * frequency);
                float ridge = 1f - Mathf.Abs(2f * n - 1f);
                ridge *= ridge;
                ridge *= prev;
                prev = Mathf.Clamp01(ridge * 2f);
                sum += ridge * amplitude;
                norm += amplitude;
                amplitude *= gain;
                frequency *= lacunarity;
            }
            return norm > 0f ? Mathf.Clamp01(sum / norm) : 0f;
        }

        /// <summary>Domain-warped fbm: coordinates displaced by two independent low-freq fields.</summary>
        internal static float WarpedFbm(float seed, float x, float y, float warpStrength, int octaves = 4)
        {
            float wx = Fbm(seed + 7919f, x, y, 3);
            float wy = Fbm(seed + 104729f, x, y, 3);
            return Fbm(seed, x + (wx - 0.5f) * 2f * warpStrength, y + (wy - 0.5f) * 2f * warpStrength, octaves);
        }

        /// <summary>Deterministic per-cell jitter in [-1,1].</summary>
        internal static float Jitter(int seed, int x, int y, int channel = 0)
            => Hash01(seed ^ (channel * 0x6329), x, y) * 2f - 1f;

        /// <summary>Seeded index pick — stable ordering for shuffles.</summary>
        internal static void Shuffle<T>(int seed, T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = HashInt(seed, i, 0, 0x5F1F) % (i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }

        private static uint RotateLeft(uint value, int bits)
            => (value << bits) | (value >> (32 - bits));
    }
}
