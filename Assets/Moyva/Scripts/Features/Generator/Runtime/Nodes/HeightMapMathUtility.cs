using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    internal static class HeightMapMathUtility
    {
        internal static float Sample(float[,] map, int x, int y, float fallback = 0f)
        {
            if (map == null)
                return fallback;

            int w = map.GetLength(0);
            int h = map.GetLength(1);
            if (w <= 0 || h <= 0)
                return fallback;

            if (x < 0 || x >= w || y < 0 || y >= h)
                return fallback;

            return map[x, y];
        }

        internal static float Sample01(float[,] map, int x, int y, float fallback = 0f)
        {
            return Mathf.Clamp01(Sample(map, x, y, fallback));
        }

        internal static float[,] BoxBlur(float[,] source, int radius, int iterations)
        {
            if (source == null)
                return null;

            int w = source.GetLength(0);
            int h = source.GetLength(1);
            if (w <= 0 || h <= 0)
                return (float[,])source.Clone();

            radius = Mathf.Max(0, radius);
            iterations = Mathf.Max(0, iterations);

            if (radius == 0 || iterations == 0)
                return (float[,])source.Clone();

            var current = (float[,])source.Clone();

            for (int iter = 0; iter < iterations; iter++)
            {
                var next = new float[w, h];

                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        float sum = 0f;
                        int count = 0;

                        for (int dx = -radius; dx <= radius; dx++)
                        {
                            int nx = x + dx;
                            if (nx < 0 || nx >= w)
                                continue;

                            for (int dy = -radius; dy <= radius; dy++)
                            {
                                int ny = y + dy;
                                if (ny < 0 || ny >= h)
                                    continue;

                                sum += current[nx, ny];
                                count++;
                            }
                        }

                        next[x, y] = count > 0 ? sum / count : current[x, y];
                    }
                }

                current = next;
            }

            return current;
        }
    }
}