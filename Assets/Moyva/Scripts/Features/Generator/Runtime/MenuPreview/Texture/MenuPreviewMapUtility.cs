using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class MenuPreviewMapUtility
    {
        public static bool HasAnyValue(string[,] map)
        {
            if (map == null)
                return false;

            int width = map.GetLength(0);
            int height = map.GetLength(1);
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (!string.IsNullOrEmpty(map[x, y]))
                    return true;
            }

            return false;
        }

        public static Vector2 CalculateHeightRange(MenuWorldPreviewData data)
        {
            if (data?.HeightMap == null)
                return new Vector2(0f, 1f);

            float min = float.MaxValue;
            float max = float.MinValue;
            for (int y = 0; y < data.Height; y++)
            for (int x = 0; x < data.Width; x++)
            {
                float value = data.HeightMap[x, y];
                if (value < min) min = value;
                if (value > max) max = value;
            }

            return min == float.MaxValue || max == float.MinValue
                ? new Vector2(0f, 1f)
                : new Vector2(min, max);
        }
    }
}
