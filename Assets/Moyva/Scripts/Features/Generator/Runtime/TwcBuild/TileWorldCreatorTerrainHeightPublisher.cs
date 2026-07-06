using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorTerrainHeightPublisher
    {
        void Publish(GeneratedWorldData worldData, Configuration configuration);
    }

    internal sealed class TileWorldCreatorTerrainHeightPublisher : ITileWorldCreatorTerrainHeightPublisher
    {
        private readonly IGeneratorTerrainLevelService _terrainLevelService;
        private readonly ITileWorldCreatorBuildEnvironment _environment;
        private readonly ITileWorldCreatorTerrainBaseHeightResolver _baseHeightResolver;

        public TileWorldCreatorTerrainHeightPublisher(
            ITileWorldCreatorBuildEnvironment environment,
            ITileWorldCreatorTerrainBaseHeightResolver baseHeightResolver,
            IGeneratorTerrainLevelService terrainLevelService = null)
        {
            _environment = environment;
            _baseHeightResolver = baseHeightResolver;
            _terrainLevelService = terrainLevelService;
        }

        public void Publish(GeneratedWorldData worldData, Configuration configuration)
        {
            if (_terrainLevelService == null || worldData == null)
                return;

            if (worldData.TerrainLevelMap != null)
                _terrainLevelService.SetLevelMap(worldData.TerrainLevelMap);

            float[,] surfaceHeightMap = BuildSurfaceHeightMap(worldData, configuration);
            if (surfaceHeightMap != null)
                _terrainLevelService.SetSurfaceHeightMap(surfaceHeightMap);
        }

        private float[,] BuildSurfaceHeightMap(GeneratedWorldData worldData, Configuration configuration)
        {
            int width = Mathf.Max(0, worldData.Width);
            int height = Mathf.Max(0, worldData.Height);
            if (width <= 0 || height <= 0)
                return null;

            var surfaceHeightMap = new float[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float baseHeight = ResolveFallbackSurfaceBaseHeight(worldData, x, y);
                if (TryGetBiomeId(worldData.BiomeMap, x, y, out string biomeId)
                    && _environment.Mapping.TryResolveTerrainLayer(biomeId, out var mapping))
                {
                    baseHeight = _baseHeightResolver.ResolveMappedTerrainBaseHeight(configuration, mapping);
                }

                surfaceHeightMap[x, y] = baseHeight + ResolveIntegerTerrainHeightOffset(worldData, x, y);
            }

            return surfaceHeightMap;
        }

        private float ResolveIntegerTerrainHeightOffset(GeneratedWorldData worldData, int x, int y)
        {
            if (!_environment.Options.ApplyIntegerTerrainHeights || worldData?.TerrainLevelMap == null)
                return 0f;

            if (x < 0 || x >= worldData.TerrainLevelMap.GetLength(0)
                || y < 0 || y >= worldData.TerrainLevelMap.GetLength(1))
            {
                return 0f;
            }

            return Mathf.Max(0, worldData.TerrainLevelMap[x, y]) * _environment.Options.TerrainHeightStep;
        }

        private static float ResolveFallbackSurfaceBaseHeight(GeneratedWorldData worldData, int x, int y)
        {
            if (worldData?.HeightMap == null
                || x < 0 || x >= worldData.HeightMap.GetLength(0)
                || y < 0 || y >= worldData.HeightMap.GetLength(1))
            {
                return 0f;
            }

            float value = worldData.HeightMap[x, y];
            return float.IsNaN(value) || float.IsInfinity(value) ? 0f : value;
        }

        private static bool TryGetBiomeId(string[,] biomeMap, int x, int y, out string biomeId)
        {
            biomeId = null;
            if (biomeMap == null
                || x < 0 || x >= biomeMap.GetLength(0)
                || y < 0 || y >= biomeMap.GetLength(1))
            {
                return false;
            }

            biomeId = biomeMap[x, y];
            return !string.IsNullOrWhiteSpace(biomeId);
        }
    }
}
