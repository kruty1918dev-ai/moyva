using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    internal static class MapNodeUtility
    {
        public static bool TryValidate<T>(
            T[,] map,
            NodeContext context,
            string portName,
            out int width,
            out int height,
            out string error)
        {
            width = map?.GetLength(0) ?? 0;
            height = map?.GetLength(1) ?? 0;
            error = null;
            if (map == null)
            {
                error = $"Вхід '{portName}' є обов’язковим.";
                return false;
            }

            int expectedWidth = Mathf.Max(1, context?.MapSize.x ?? 0);
            int expectedHeight = Mathf.Max(1, context?.MapSize.y ?? 0);
            if (width != expectedWidth || height != expectedHeight)
            {
                error =
                    $"Вхід '{portName}' має розмір {width}x{height}, " +
                    $"очікується {expectedWidth}x{expectedHeight}.";
                return false;
            }

            return true;
        }

        public static void ResolveSize(NodeContext context, out int width, out int height)
        {
            width = Mathf.Max(1, context?.MapSize.x ?? 0);
            height = Mathf.Max(1, context?.MapSize.y ?? 0);
        }

        public static int ResolveSeed(NodeContext context, string nodeId)
        {
            return GlobalSeed.Combine(
                context?.Seed ?? GlobalSeed.DefaultSeed,
                GlobalSeed.StableHash(nodeId));
        }

        public static bool IsFinite(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value);

        public static float[,] Normalize(float[,] source)
        {
            int width = source.GetLength(0);
            int height = source.GetLength(1);
            float min = float.PositiveInfinity;
            float max = float.NegativeInfinity;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float value = source[x, y];
                if (!IsFinite(value))
                    continue;
                min = Mathf.Min(min, value);
                max = Mathf.Max(max, value);
            }

            var result = new float[width, height];
            if (!IsFinite(min) || !IsFinite(max))
                return result;

            float range = max - min;
            if (Mathf.Approximately(range, 0f))
                return result;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float value = source[x, y];
                result[x, y] = IsFinite(value)
                    ? Mathf.Clamp01((value - min) / range)
                    : 0f;
            }

            return result;
        }

        public static float[,] Smooth(float[,] source, int radius, int iterations)
        {
            int width = source.GetLength(0);
            int height = source.GetLength(1);
            int safeRadius = Mathf.Max(0, radius);
            int safeIterations = Mathf.Max(1, iterations);
            var current = (float[,])source.Clone();
            if (safeRadius == 0)
                return current;

            for (int iteration = 0; iteration < safeIterations; iteration++)
            {
                var next = new float[width, height];
                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    double sum = 0d;
                    int count = 0;
                    int minX = Mathf.Max(0, x - safeRadius);
                    int maxX = Mathf.Min(width - 1, x + safeRadius);
                    int minY = Mathf.Max(0, y - safeRadius);
                    int maxY = Mathf.Min(height - 1, y + safeRadius);
                    for (int sampleX = minX; sampleX <= maxX; sampleX++)
                    for (int sampleY = minY; sampleY <= maxY; sampleY++)
                    {
                        float value = current[sampleX, sampleY];
                        if (!IsFinite(value))
                            continue;
                        sum += value;
                        count++;
                    }

                    next[x, y] = count > 0 ? (float)(sum / count) : 0f;
                }

                current = next;
            }

            return current;
        }

        public static float[,] DistanceTo(bool[,] source, bool targetValue)
        {
            int width = source.GetLength(0);
            int height = source.GetLength(1);
            int targets = 0;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (source[x, y] == targetValue)
                    targets++;
            }

            var result = new float[width, height];
            if (targets == 0)
            {
                float fallback = Mathf.Sqrt(width * width + height * height);
                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    result[x, y] = fallback;
                return result;
            }

            int maxLength = Mathf.Max(width, height);
            var values = new float[maxLength];
            var transformed = new float[maxLength];
            var vertices = new int[maxLength];
            var boundaries = new float[maxLength + 1];
            var vertical = new float[width, height];
            const float infinity = 1e20f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                    values[y] = source[x, y] == targetValue ? 0f : infinity;

                DistanceTransform1D(values, height, transformed, vertices, boundaries);
                for (int y = 0; y < height; y++)
                    vertical[x, y] = transformed[y];
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    values[x] = vertical[x, y];

                DistanceTransform1D(values, width, transformed, vertices, boundaries);
                for (int x = 0; x < width; x++)
                    result[x, y] = Mathf.Sqrt(Mathf.Max(0f, transformed[x]));
            }

            return result;
        }

        public static bool[,] Morph(bool[,] source, bool dilate, int radius)
        {
            int width = source.GetLength(0);
            int height = source.GetLength(1);
            int safeRadius = Mathf.Max(0, radius);
            var result = new bool[width, height];

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                bool value = !dilate;
                bool decided = false;
                for (int offsetX = -safeRadius; offsetX <= safeRadius && !decided; offsetX++)
                for (int offsetY = -safeRadius; offsetY <= safeRadius; offsetY++)
                {
                    if (offsetX * offsetX + offsetY * offsetY > safeRadius * safeRadius)
                        continue;

                    int sampleX = x + offsetX;
                    int sampleY = y + offsetY;
                    bool sample = sampleX >= 0
                                  && sampleY >= 0
                                  && sampleX < width
                                  && sampleY < height
                                  && source[sampleX, sampleY];
                    if (dilate && sample)
                    {
                        value = true;
                        decided = true;
                        break;
                    }
                    if (!dilate && !sample)
                    {
                        value = false;
                        decided = true;
                        break;
                    }
                }

                result[x, y] = value;
            }

            return result;
        }

        private static void DistanceTransform1D(
            float[] source,
            int length,
            float[] result,
            int[] vertices,
            float[] boundaries)
        {
            int envelope = 0;
            vertices[0] = 0;
            boundaries[0] = float.NegativeInfinity;
            boundaries[1] = float.PositiveInfinity;

            for (int q = 1; q < length; q++)
            {
                float intersection;
                do
                {
                    int vertex = vertices[envelope];
                    intersection =
                        ((source[q] + q * q) - (source[vertex] + vertex * vertex))
                        / (2f * (q - vertex));
                    if (intersection > boundaries[envelope])
                        break;
                    envelope--;
                }
                while (envelope >= 0);

                envelope++;
                vertices[envelope] = q;
                boundaries[envelope] = intersection;
                boundaries[envelope + 1] = float.PositiveInfinity;
            }

            envelope = 0;
            for (int q = 0; q < length; q++)
            {
                while (boundaries[envelope + 1] < q)
                    envelope++;
                float delta = q - vertices[envelope];
                result[q] = delta * delta + source[vertices[envelope]];
            }
        }
    }
}
