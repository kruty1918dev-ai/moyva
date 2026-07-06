using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorShoreBandService
    {
        void Expand(GeneratedWorldData worldData);
    }

    internal sealed class TileWorldCreatorShoreBandService : ITileWorldCreatorShoreBandService
    {
        private const string LogTag = "[MoyvaTWCHeight]";
        private readonly TileWorldCreatorBuildOptions _options;

        public TileWorldCreatorShoreBandService(ITileWorldCreatorBuildEnvironment environment)
        {
            _options = environment.Options;
        }

        public void Expand(GeneratedWorldData worldData)
        {
            string[,] biomeMap = worldData?.BiomeMap;
            if (biomeMap == null)
                return;

            int width = biomeMap.GetLength(0);
            int height = biomeMap.GetLength(1);
            string shoreId = _options.ShoreBandTileId;
            if (width == 0 || height == 0 || string.IsNullOrWhiteSpace(shoreId))
                return;

            bool[,] originalIsWater = new bool[width, height];
            bool[,] shoreBand = new bool[width, height];
            CountOriginalBiomes(biomeMap, originalIsWater, width, height, out int waterCount, out int sandCount);
            int convertedCount = ConvertLandSideShore(biomeMap, originalIsWater, shoreBand, width, height, shoreId);
            MarkExistingSandShoreBand(biomeMap, originalIsWater, shoreBand, width, height);
            int raisedLevelCount = RaiseShoreLevelsToWater(worldData.TerrainLevelMap, shoreBand, originalIsWater, width, height);
            Debug.Log($"{LogTag} ExpandSandShoreBand: size={width}x{height}, shoreId='{shoreId}', originalWater={waterCount}, originalSand={sandCount}, convertedLandToShore={convertedCount}, finalShoreBand={CountTrue(shoreBand, width, height)}, raisedLevelCells={raisedLevelCount}, levels={TileWorldCreatorMapFormatUtility.FormatLevelStats(worldData.TerrainLevelMap)}.");
        }

        private static void CountOriginalBiomes(string[,] biomeMap, bool[,] originalIsWater, int width, int height, out int waterCount, out int sandCount)
        {
            waterCount = 0;
            sandCount = 0;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                originalIsWater[x, y] = TileWorldCreatorTerrainBiomeClassifier.IsWater(biomeMap[x, y]);
                if (originalIsWater[x, y])
                    waterCount++;
                if (TileWorldCreatorTerrainBiomeClassifier.IsSand(biomeMap[x, y]))
                    sandCount++;
            }
        }

        private static int ConvertLandSideShore(string[,] biomeMap, bool[,] originalIsWater, bool[,] shoreBand, int width, int height, string shoreId)
        {
            int convertedCount = 0;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (originalIsWater[x, y] || TileWorldCreatorTerrainBiomeClassifier.IsSand(biomeMap[x, y]))
                    continue;
                if (!HasNeighbour(originalIsWater, x, y, width, height, waterValue: true))
                    continue;

                biomeMap[x, y] = shoreId;
                shoreBand[x, y] = true;
                convertedCount++;
            }

            return convertedCount;
        }

        private static void MarkExistingSandShoreBand(string[,] biomeMap, bool[,] originalIsWater, bool[,] shoreBand, int width, int height)
        {
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (TileWorldCreatorTerrainBiomeClassifier.IsSand(biomeMap[x, y])
                    && HasNeighbour(originalIsWater, x, y, width, height, waterValue: true))
                    shoreBand[x, y] = true;
        }

        private static int RaiseShoreLevelsToWater(int[,] levelMap, bool[,] shoreBand, bool[,] originalIsWater, int width, int height)
        {
            if (levelMap == null || levelMap.GetLength(0) != width || levelMap.GetLength(1) != height)
                return 0;

            int changedCount = 0;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (!shoreBand[x, y])
                    continue;

                int targetLevel = ResolveWaterNeighbourLevel(levelMap, originalIsWater, x, y, width, height);
                if (levelMap[x, y] != targetLevel)
                    changedCount++;
                levelMap[x, y] = targetLevel;
            }

            return changedCount;
        }

        private static int ResolveWaterNeighbourLevel(int[,] levelMap, bool[,] originalIsWater, int x, int y, int width, int height)
        {
            int targetLevel = Mathf.Max(0, levelMap[x, y]);
            for (int offsetX = -1; offsetX <= 1; offsetX++)
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                if (offsetX == 0 && offsetY == 0)
                    continue;

                int nx = x + offsetX;
                int ny = y + offsetY;
                if (nx >= 0 && ny >= 0 && nx < width && ny < height && originalIsWater[nx, ny])
                    targetLevel = Mathf.Max(targetLevel, levelMap[nx, ny]);
            }

            return targetLevel;
        }

        private static bool HasNeighbour(bool[,] map, int x, int y, int width, int height, bool waterValue)
        {
            for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;
                int nx = x + dx;
                int ny = y + dy;
                if (nx >= 0 && ny >= 0 && nx < width && ny < height && map[nx, ny] == waterValue)
                    return true;
            }

            return false;
        }

        private static int CountTrue(bool[,] values, int width, int height)
        {
            int count = 0;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (values[x, y])
                    count++;
            return count;
        }
    }
}
