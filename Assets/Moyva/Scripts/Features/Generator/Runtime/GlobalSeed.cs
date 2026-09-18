using System;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Deterministic seed state shared by Moyva map generation and the Unity
    /// random source. Every recipe compile/generate entry point initializes it
    /// before doing work.
    /// </summary>
    public static class GlobalSeed
    {
        public const int DefaultSeed = 42;
        private static int _current = DefaultSeed;
        public static int Current => _current;

        public static int Normalize(int seed)
        {
            return seed == 0 ? 1 : seed;
        }

        public static void Set(int seed)
        {
            _current = seed;
        }

        /// <summary>
        /// Applies the same normalized seed to Moyva and Unity random sources.
        /// Every compile/generate entry point must use this before doing work.
        /// </summary>
        public static int InitializeDeterministic(int seed)
        {
            int effectiveSeed = Normalize(seed);
            Set(effectiveSeed);
            UnityEngine.Random.InitState(effectiveSeed);
            return effectiveSeed;
        }

        public static Random CreateRandom()
        {
            return new Random(_current);
        }

        public static Random CreateRandom(int salt)
        {
            return new Random(Combine(_current, salt));
        }

        public static Random CreateRandom(string salt)
        {
            return new Random(Combine(_current, StableHash(salt)));
        }

        public static int Combine(int seed, int salt)
        {
            unchecked
            {
                int hash = seed;
                hash = (hash * 397) ^ salt;
                hash ^= (hash << 13);
                hash ^= (hash >> 17);
                hash ^= (hash << 5);
                return hash;
            }
        }

        public static int StableHash(string text)
        {
            unchecked
            {
                int hash = 23;
                if (string.IsNullOrEmpty(text))
                    return hash;
                for (int i = 0; i < text.Length; i++)
                    hash = hash * 31 + text[i];
                return hash;
            }
        }
    }
}
