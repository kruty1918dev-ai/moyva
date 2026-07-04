using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Signals;
using UnityEngine;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal static class StartingPositionMapUtility
    {
        public static float ResolveHeight(WorldGeneratedDataSignal signal, Vector2Int position)
        {
            if (signal.HeightMap == null)
                return 0f;

            int width = signal.HeightMap.GetLength(0);
            int height = signal.HeightMap.GetLength(1);
            if (position.x < 0 || position.y < 0 || position.x >= width || position.y >= height)
                return 0f;

            return signal.HeightMap[position.x, position.y];
        }

        public static Vector2Int ResolveBaseMapSize(WorldGeneratedDataSignal signal)
        {
            int width = Mathf.Max(0, signal.Width);
            int height = Mathf.Max(0, signal.Height);

            ApplyBaseMapSize(signal.TileMap, ref width, ref height);
            ApplyBaseMapSize(signal.HeightMap, ref width, ref height);
            ApplyBaseMapSize(signal.TerrainLevelMap, ref width, ref height);

            return new Vector2Int(Mathf.Max(1, width), Mathf.Max(1, height));
        }

        public static void ApplyBaseMapSize<T>(T[,] map, ref int width, ref int height)
        {
            if (map == null)
                return;

            int mapWidth = map.GetLength(0);
            int mapHeight = map.GetLength(1);
            if (mapWidth <= 0 || mapHeight <= 0)
                return;

            width = width > 0 ? Mathf.Min(width, mapWidth) : mapWidth;
            height = height > 0 ? Mathf.Min(height, mapHeight) : mapHeight;
        }

        public static Vector2Int ClampToMap(Vector2Int position, int width, int height)
        {
            return new Vector2Int(
                Mathf.Clamp(position.x, 0, Mathf.Max(0, width - 1)),
                Mathf.Clamp(position.y, 0, Mathf.Max(0, height - 1)));
        }

        public static Vector2Int PickRuntimeRandomPoint(
            int width,
            int height,
            int minMarginFromBorder,
            float relativeMarginFactor,
            out int seed)
        {
            seed = CreateRuntimeRandomSeed();
            width = Mathf.Max(0, width);
            height = Mathf.Max(0, height);
            if (width <= 0 || height <= 0)
                return Vector2Int.zero;

            int minSide = Mathf.Min(width, height);
            int relativeMargin = Mathf.FloorToInt(minSide * Mathf.Clamp01(relativeMarginFactor));
            int margin = Mathf.Max(0, Mathf.Max(minMarginFromBorder, relativeMargin));

            int xMin = Mathf.Clamp(margin, 0, width - 1);
            int xMax = Mathf.Clamp(width - margin - 1, xMin, width - 1);
            int yMin = Mathf.Clamp(margin, 0, height - 1);
            int yMax = Mathf.Clamp(height - margin - 1, yMin, height - 1);
            var random = new System.Random(seed);
            return new Vector2Int(
                random.Next(xMin, xMax + 1),
                random.Next(yMin, yMax + 1));
        }

        private static int CreateRuntimeRandomSeed()
        {
            unchecked
            {
                return System.Environment.TickCount ^ System.Guid.NewGuid().GetHashCode() ^ (Time.frameCount * 397);
            }
        }

        public static Vector2Int FindRepairCenter(bool[,] snapshot, int width, int height)
        {
            if (snapshot == null)
                return new Vector2Int(Mathf.Max(0, width / 2), Mathf.Max(0, height / 2));

            long sumX = 0;
            long sumY = 0;
            int count = 0;
            int copyW = Mathf.Min(width, snapshot.GetLength(0));
            int copyH = Mathf.Min(height, snapshot.GetLength(1));

            for (int x = 0; x < copyW; x++)
            {
                for (int y = 0; y < copyH; y++)
                {
                    if (!snapshot[x, y])
                        continue;

                    sumX += x;
                    sumY += y;
                    count++;
                }
            }

            if (count == 0)
                return new Vector2Int(Mathf.Max(0, width / 2), Mathf.Max(0, height / 2));

            return new Vector2Int(
                Mathf.Clamp(Mathf.RoundToInt(sumX / (float)count), 0, Mathf.Max(0, width - 1)),
                Mathf.Clamp(Mathf.RoundToInt(sumY / (float)count), 0, Mathf.Max(0, height - 1)));
        }

        public static bool IsInsideRevealShape(int dx, int dy, int radius, float radiusSqr, FogRevealShape shape)
        {
            return shape switch
            {
                FogRevealShape.Square => Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) <= radius,
                FogRevealShape.Diamond => Mathf.Abs(dx) + Mathf.Abs(dy) <= radius,
                _ => dx * dx + dy * dy <= radiusSqr,
            };
        }
    }
}
