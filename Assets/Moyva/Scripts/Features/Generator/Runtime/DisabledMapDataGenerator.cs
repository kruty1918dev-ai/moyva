using System;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class DisabledMapDataGenerator : IMapDataGenerator
    {
        public void GenerateMapData(int width, int height, Action<string[,], string[,], float[,], string[,]> onComplete)
        {
            int safeWidth = Mathf.Max(1, width);
            int safeHeight = Mathf.Max(1, height);
            var biomeMap = new string[safeWidth, safeHeight];
            var objectMap = new string[safeWidth, safeHeight];
            var heightMap = new float[safeWidth, safeHeight];
            var buildingMap = new string[safeWidth, safeHeight];

            int seed = Kruty1918.SaveSystem.GameLaunchContext.Seed;
            if (seed == 0)
                seed = safeWidth * 73856093 ^ safeHeight * 19349663;
            var random = new System.Random(seed);
            float centerX = (safeWidth - 1) * 0.5f;
            float centerY = (safeHeight - 1) * 0.5f;
            float maxDistance = Mathf.Max(1f, Mathf.Max(centerX, centerY));

            for (int x = 0; x < safeWidth; x++)
            for (int y = 0; y < safeHeight; y++)
            {
                float edgeDistance = Mathf.Max(Mathf.Abs(x - centerX), Mathf.Abs(y - centerY)) / maxDistance;
                float noise = (float)random.NextDouble();
                bool water = edgeDistance > 0.86f && noise < Mathf.InverseLerp(0.86f, 1f, edgeDistance);
                bool hill = !water && noise > 0.78f;
                bool forest = !water && !hill && noise > 0.58f;

                biomeMap[x, y] = water ? "water" : hill ? "hill" : forest ? "forest-sparse" : GeneratedWorldDataDefaults.FallbackTileId;
                heightMap[x, y] = water ? 0f : hill ? 2f : 1f;
            }

            Debug.LogWarning(
                "[Generator] GeneratorMapRecipe or TileWorldCreatorManager is missing. " +
                $"Generated fallback playable map {safeWidth}x{safeHeight}.");

            onComplete?.Invoke(
                biomeMap,
                objectMap,
                heightMap,
                buildingMap);
        }
    }
}
