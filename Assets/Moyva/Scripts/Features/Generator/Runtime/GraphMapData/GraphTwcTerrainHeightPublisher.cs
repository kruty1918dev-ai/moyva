using Kruty1918.Moyva.Generator.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphTwcTerrainHeightPublisher
    {
        void Clear();
        void Publish(float[,] surfaceHeightMap);
    }

    internal sealed class GraphTwcTerrainHeightPublisher : IGraphTwcTerrainHeightPublisher
    {
        private readonly IGeneratorTerrainLevelService _terrainLevelService;

        public GraphTwcTerrainHeightPublisher([InjectOptional] IGeneratorTerrainLevelService terrainLevelService = null)
        {
            _terrainLevelService = terrainLevelService;
        }

        public void Clear()
        {
            _terrainLevelService?.Clear();
        }

        public void Publish(float[,] surfaceHeightMap)
        {
            if (_terrainLevelService == null)
                return;

            _terrainLevelService.SetLevelMap(BuildLevelMap(surfaceHeightMap));
            _terrainLevelService.SetSurfaceHeightMap(surfaceHeightMap);
        }

        private static int[,] BuildLevelMap(float[,] surfaceHeightMap)
        {
            if (surfaceHeightMap == null)
                return null;

            int width = surfaceHeightMap.GetLength(0);
            int height = surfaceHeightMap.GetLength(1);
            var levelMap = new int[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                levelMap[x, y] = Mathf.Max(0, Mathf.RoundToInt(surfaceHeightMap[x, y]));

            return levelMap;
        }
    }
}
