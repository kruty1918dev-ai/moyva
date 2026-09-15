using System;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Deterministic hash functions for procedural generation.
    /// Provides stable, repeatable hash values for coordinates and seeds.
    /// </summary>
    internal static class DeterministicHash
    {
        /// <summary>
        /// Generate a stable hash for a cell coordinate.
        /// </summary>
        public static uint CellHash(int seed, int x, int y, int index)
        {
            uint hash = (uint)seed;
            hash = Mix(hash, (uint)x);
            hash = Mix(hash, (uint)y);
            hash = Mix(hash, (uint)index);
            return Finalize(hash);
        }

        /// <summary>
        /// Generate a stable hash for variant selection.
        /// </summary>
        public static uint VariantHash(int seed, int x, int y, int index, string context)
        {
            uint hash = CellHash(seed, x, y, index);
            if (!string.IsNullOrEmpty(context))
            {
                foreach (char c in context)
                    hash = Mix(hash, (uint)c);
            }
            return Finalize(hash);
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
    }
}
