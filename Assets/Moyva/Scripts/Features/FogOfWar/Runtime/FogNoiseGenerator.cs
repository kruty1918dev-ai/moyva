namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Детермінований CPU Perlin noise generator із підтримкою seed.
    /// Може використовуватися для допоміжних fog visual pattern-ів або preview tooling.
    /// </summary>
    internal sealed class FogNoiseGenerator
    {
        private readonly int _seed;

        /// <summary>
        /// Створює noise generator із фіксованим seed.
        /// </summary>
        /// <param name="seed">Seed для детермінованого результату.</param>
        public FogNoiseGenerator(int seed = 0)
        {
            _seed = seed;
        }

        /// <summary>
        /// Генерує noise map заданого розміру зі значеннями в діапазоні [0..1].
        /// </summary>
        /// <param name="width">Ширина noise map.</param>
        /// <param name="height">Висота noise map.</param>
        /// <param name="scale">Масштаб noise pattern.</param>
        /// <returns>Двовимірний масив noise values.</returns>
        public float[,] Generate(int width, int height, float scale = 1f)
        {
            var map = new float[width, height];

            if (scale <= 0f) scale = 0.0001f;

            // Deterministic offset based on seed
            float offsetX = HashToFloat(_seed, 0) * 10000f;
            float offsetY = HashToFloat(_seed, 1) * 10000f;

            float minVal = float.MaxValue;
            float maxVal = float.MinValue;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float nx = (x + offsetX) / width  * scale;
                    float ny = (y + offsetY) / height * scale;
                    float v  = Perlin(nx, ny);
                    map[x, y] = v;
                    if (v < minVal) minVal = v;
                    if (v > maxVal) maxVal = v;
                }
            }

            // Normalize to [0, 1]
            float range = maxVal - minVal;
            if (range > 0.0001f)
            {
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        map[x, y] = (map[x, y] - minVal) / range;
            }

            return map;
        }

        // ─── Perlin Noise Implementation ─────────────────────────────────────

        private static float Perlin(float x, float y)
        {
            int xi = FloorInt(x);
            int yi = FloorInt(y);

            float xf = x - xi;
            float yf = y - yi;

            float u = Fade(xf);
            float v = Fade(yf);

            float aa = Gradient(Hash(xi,     yi    ), xf,       yf      );
            float ba = Gradient(Hash(xi + 1, yi    ), xf - 1f,  yf      );
            float ab = Gradient(Hash(xi,     yi + 1), xf,       yf - 1f );
            float bb = Gradient(Hash(xi + 1, yi + 1), xf - 1f,  yf - 1f );

            float x1 = Lerp(aa, ba, u);
            float x2 = Lerp(ab, bb, u);
            return (Lerp(x1, x2, v) + 1f) * 0.5f; // remap [-1,1] → [0,1]
        }

        private static float Fade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);

        private static float Lerp(float a, float b, float t) => a + t * (b - a);

        private static float Gradient(int hash, float x, float y)
        {
            int h = hash & 3;
            float u = h < 2 ? x : y;
            float v = h < 2 ? y : x;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        private static int Hash(int x, int y)
        {
            // Simple integer hash
            int h = x * 1619 + y * 31337;
            h = (h ^ (h >> 16)) * unchecked((int)0x45d9f3b);
            h = (h ^ (h >> 16)) * unchecked((int)0x45d9f3b);
            h = h ^ (h >> 16);
            return h & 0xFF;
        }

        private static float HashToFloat(int seed, int idx)
        {
            int h = Hash(seed * 7919, idx * 6271);
            return (h & 0xFF) / 255f;
        }

        private static int FloorInt(float f) => f >= 0 ? (int)f : (int)f - 1;
    }
}
