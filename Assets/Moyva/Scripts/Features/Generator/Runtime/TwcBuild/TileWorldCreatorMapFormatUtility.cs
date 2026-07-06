using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class TileWorldCreatorMapFormatUtility
    {
        public static string FormatMapSize(System.Array map)
            => map == null ? "null" : map.Rank == 2 ? $"{map.GetLength(0)}x{map.GetLength(1)}" : $"rank{map.Rank}";

        public static string FormatLevelStats(int[,] levelMap)
        {
            if (levelMap == null)
                return "null";

            int width = levelMap.GetLength(0);
            int height = levelMap.GetLength(1);
            if (width <= 0 || height <= 0)
                return $"{width}x{height}, empty";

            int min = int.MaxValue;
            int max = int.MinValue;
            var histogram = new SortedDictionary<int, int>();
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                int value = levelMap[x, y];
                min = Mathf.Min(min, value);
                max = Mathf.Max(max, value);
                histogram.TryGetValue(value, out int count);
                histogram[value] = count + 1;
            }

            var builder = new StringBuilder();
            builder.Append(width).Append('x').Append(height)
                .Append(", min=").Append(min)
                .Append(", max=").Append(max)
                .Append(", histogram=");
            AppendHistogram(builder, histogram, 16);
            AppendLevelSamples(builder, levelMap, width, height);
            return builder.ToString();
        }

        public static string FormatFloatMapStats(float[,] map)
        {
            if (map == null)
                return "null";

            int width = map.GetLength(0);
            int height = map.GetLength(1);
            if (width <= 0 || height <= 0)
                return $"{width}x{height}, empty";

            float min = float.PositiveInfinity;
            float max = float.NegativeInfinity;
            double sum = 0d;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float value = map[x, y];
                min = Mathf.Min(min, value);
                max = Mathf.Max(max, value);
                sum += value;
            }

            double avg = sum / (width * height);
            return $"{width}x{height}, min={min:0.###}, max={max:0.###}, avg={avg:0.###}, samples=(0,0:{map[0, 0]:0.###}), (mid:{map[width / 2, height / 2]:0.###}), (last:{map[width - 1, height - 1]:0.###})";
        }

        public static string FormatPositionBounds(HashSet<Vector2> positions)
        {
            if (positions == null || positions.Count == 0)
                return "empty";

            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;
            int index = 0;
            var samples = new StringBuilder();

            foreach (Vector2 position in positions)
            {
                minX = Mathf.Min(minX, position.x);
                minY = Mathf.Min(minY, position.y);
                maxX = Mathf.Max(maxX, position.x);
                maxY = Mathf.Max(maxY, position.y);
                if (index < 6)
                {
                    if (samples.Length > 0)
                        samples.Append(", ");
                    samples.Append('(').Append(position.x).Append(',').Append(position.y).Append(')');
                }
                index++;
            }

            return $"min=({minX},{minY}), max=({maxX},{maxY}), samples=[{samples}]";
        }

        public static string FormatColor(Color color)
            => $"r={color.r.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}, g={color.g.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}, b={color.b.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}, a={color.a.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}";

        private static void AppendHistogram(StringBuilder builder, SortedDictionary<int, int> histogram, int maxEntries)
        {
            builder.Append('{');
            int index = 0;
            foreach (var pair in histogram)
            {
                if (index > 0)
                    builder.Append(", ");
                if (index >= maxEntries)
                {
                    builder.Append("...");
                    break;
                }

                builder.Append(pair.Key).Append(':').Append(pair.Value);
                index++;
            }
            builder.Append('}');
        }

        private static void AppendLevelSamples(StringBuilder builder, int[,] levelMap, int width, int height)
        {
            builder.Append(", samples=")
                .Append("(0,0:").Append(levelMap[0, 0]).Append(')')
                .Append(", (mid:").Append(levelMap[width / 2, height / 2]).Append(')')
                .Append(", (last:").Append(levelMap[width - 1, height - 1]).Append(')');
        }
    }
}
