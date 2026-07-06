using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class TileWorldCreatorHeightProjectionUtility
    {
        public static int CountLevelMapCells(int[,] terrainLevelMap)
            => terrainLevelMap == null ? 0 : terrainLevelMap.GetLength(0) * terrainLevelMap.GetLength(1);

        public static string FormatSamples(List<string> samples)
        {
            if (samples == null || samples.Count == 0)
                return "[]";

            var builder = new StringBuilder();
            builder.Append('[');
            for (int i = 0; i < samples.Count; i++)
            {
                if (i > 0)
                    builder.Append(" | ");
                builder.Append(samples[i]);
            }

            return builder.Append(']').ToString();
        }

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

            return FormatLevelStats(width, height, min, max, histogram, levelMap);
        }

        private static string FormatLevelStats(int width, int height, int min, int max, SortedDictionary<int, int> histogram, int[,] levelMap)
        {
            var builder = new StringBuilder();
            builder.Append(width).Append('x').Append(height).Append(", min=").Append(min).Append(", max=").Append(max).Append(", histogram={");
            int index = 0;
            foreach (var pair in histogram)
            {
                if (index > 0)
                    builder.Append(", ");
                builder.Append(pair.Key).Append(':').Append(pair.Value);
                index++;
                if (index >= 16 && histogram.Count > index)
                {
                    builder.Append(", ...");
                    break;
                }
            }

            return builder.Append("}, samples=(0,0:").Append(levelMap[0, 0]).Append("), (mid:")
                .Append(levelMap[width / 2, height / 2]).Append("), (last:")
                .Append(levelMap[width - 1, height - 1]).Append(')').ToString();
        }
    }
}
