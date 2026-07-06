using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorTerrainLevelMapService
    {
        void Ensure(GeneratedWorldData worldData);
        void NormalizeForTileWorldCreator(GeneratedWorldData worldData);
    }

    internal sealed class TileWorldCreatorTerrainLevelMapService : ITileWorldCreatorTerrainLevelMapService
    {
        private const string LogTag = "[MoyvaTWCHeight]";
        private readonly TileWorldCreatorBuildOptions _options;

        public TileWorldCreatorTerrainLevelMapService(ITileWorldCreatorBuildEnvironment environment)
        {
            _options = environment.Options;
        }

        public void Ensure(GeneratedWorldData worldData)
        {
            if (worldData == null || worldData.TerrainLevelMap != null)
                return;

            int[,] levelMap = BuildFallback(worldData);
            if (levelMap == null)
                return;

            worldData.TerrainLevelMap = levelMap;
            Debug.LogWarning($"{LogTag} TerrainLevelMap was missing; applied fallback integer terrain levels from height/biome map. HeightMap={TileWorldCreatorMapFormatUtility.FormatMapSize(worldData.HeightMap)}, BiomeMap={TileWorldCreatorMapFormatUtility.FormatMapSize(worldData.BiomeMap)}, fallbackStats={TileWorldCreatorMapFormatUtility.FormatLevelStats(levelMap)}.");
        }

        public void NormalizeForTileWorldCreator(GeneratedWorldData worldData)
        {
            if (worldData?.TerrainLevelMap == null || worldData.BiomeMap == null)
                return;

            int width = Mathf.Min(worldData.TerrainLevelMap.GetLength(0), worldData.BiomeMap.GetLength(0));
            int height = Mathf.Min(worldData.TerrainLevelMap.GetLength(1), worldData.BiomeMap.GetLength(1));
            if (width <= 0 || height <= 0)
                return;

            FindCapturedLandLevelRange(worldData, width, height, out int capturedMin, out int capturedMax);
            Debug.Log($"{LogTag} NormalizeTerrainLevelsForTileWorldCreator input: size={width}x{height}, capturedLandRange={capturedMin}..{capturedMax}, targetLevels water={_options.WaterTerrainLevel}, shore={_options.ShoreTerrainLevel}, land={_options.LandTerrainLevel}, hill={_options.HillTerrainLevel}, max={_options.MaxTerrainLevel}.");

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                string biomeId = worldData.BiomeMap[x, y];
                bool nearWater = TileWorldCreatorTerrainBiomeClassifier.HasWaterNeighbour(worldData.BiomeMap, x, y, width, height);
                worldData.TerrainLevelMap[x, y] = ResolveVisualTerrainLevel(
                    biomeId,
                    worldData.TerrainLevelMap[x, y],
                    capturedMin,
                    capturedMax,
                    nearWater);
            }
        }

        private int[,] BuildFallback(GeneratedWorldData worldData)
        {
            if (worldData?.BiomeMap == null)
                return null;

            int width = worldData.BiomeMap.GetLength(0);
            int height = worldData.BiomeMap.GetLength(1);
            if (width <= 0 || height <= 0)
                return null;

            return HasSameSize(worldData.HeightMap, width, height)
                ? BuildHeightFallback(worldData, width, height)
                : BuildBiomeFallback(worldData, width, height);
        }

        private int[,] BuildHeightFallback(GeneratedWorldData worldData, int width, int height)
        {
            FindLandHeightRange(worldData, width, height, out float minHeight, out float maxHeight);
            var levelMap = new int[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                string biomeId = worldData.BiomeMap[x, y];
                if (TileWorldCreatorTerrainBiomeClassifier.IsWater(biomeId))
                    levelMap[x, y] = _options.WaterTerrainLevel;
                else if (TileWorldCreatorTerrainBiomeClassifier.IsSand(biomeId)
                         && TileWorldCreatorTerrainBiomeClassifier.HasWaterNeighbour(worldData.BiomeMap, x, y, width, height))
                    levelMap[x, y] = Mathf.Max(_options.ShoreTerrainLevel, _options.WaterTerrainLevel);
                else
                    levelMap[x, y] = ProjectHeight(worldData.HeightMap[x, y], minHeight, maxHeight);
            }

            return levelMap;
        }

        private int[,] BuildBiomeFallback(GeneratedWorldData worldData, int width, int height)
        {
            Debug.LogWarning($"{LogTag} BuildFallbackTerrainLevelMap falls back to biome-only levels because HeightMap size does not match. BiomeMap={width}x{height}, HeightMap={TileWorldCreatorMapFormatUtility.FormatMapSize(worldData.HeightMap)}.");
            var levelMap = new int[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                levelMap[x, y] = ResolveBiomeFallbackLevel(worldData.BiomeMap[x, y]);
            return levelMap;
        }

        private int ResolveVisualTerrainLevel(string biomeId, int capturedLevel, int capturedMin, int capturedMax, bool nearWater)
            => TileWorldCreatorTerrainLevelResolver.ResolveVisualTerrainLevel(biomeId, capturedLevel, capturedMin, capturedMax, nearWater, _options);

        private int ProjectHeight(float height, float minHeight, float maxHeight)
        {
            float t = maxHeight > minHeight ? Mathf.InverseLerp(minHeight, maxHeight, height) : 0f;
            return Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(_options.LandTerrainLevel, _options.MaxTerrainLevel, t)), _options.LandTerrainLevel, _options.MaxTerrainLevel);
        }

        private static bool HasSameSize(System.Array map, int width, int height)
            => map != null && map.Rank == 2 && map.GetLength(0) == width && map.GetLength(1) == height;

        private static int ResolveBiomeFallbackLevel(string biomeId)
            => TileWorldCreatorTerrainLevelResolver.ResolveBiomeFallbackLevel(biomeId);

        private static void FindCapturedLandLevelRange(GeneratedWorldData worldData, int width, int height, out int min, out int max)
            => TileWorldCreatorTerrainLevelResolver.FindCapturedLandLevelRange(worldData, width, height, out min, out max);

        private static void FindLandHeightRange(GeneratedWorldData worldData, int width, int height, out float min, out float max)
            => TileWorldCreatorTerrainLevelResolver.FindLandHeightRange(worldData, width, height, out min, out max);
    }
}
