using System;

namespace Kruty1918.Moyva.Marketing.Planning
{
    /// <summary>Deterministic RNG for planners. Never touches UnityEngine.Random
    /// global state.</summary>
    public sealed class MarketingRng
    {
        private readonly Random _random;

        public MarketingRng(int seed)
        {
            _random = new Random(seed == 0 ? 1 : seed);
        }

        public int Next(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);
        public float NextFloat() => (float)_random.NextDouble();
        public float Range(float min, float max) => min + (float)_random.NextDouble() * (max - min);

        /// <summary>Pick a weighted index; weights[i] &lt;= 0 excludes the item.</summary>
        public int WeightedPick(float[] weights)
        {
            float total = 0f;
            for (int i = 0; i < weights.Length; i++)
                if (weights[i] > 0f) total += weights[i];
            if (total <= 0f)
                return -1;
            double roll = _random.NextDouble() * total;
            for (int i = 0; i < weights.Length; i++)
            {
                if (weights[i] <= 0f) continue;
                roll -= weights[i];
                if (roll <= 0) return i;
            }
            for (int i = weights.Length - 1; i >= 0; i--)
                if (weights[i] > 0f) return i;
            return -1;
        }
    }
}
