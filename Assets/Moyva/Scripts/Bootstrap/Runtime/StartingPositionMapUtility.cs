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
