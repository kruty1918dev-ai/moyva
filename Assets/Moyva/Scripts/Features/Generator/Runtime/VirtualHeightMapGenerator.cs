using System;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class VirtualHeightMapGenerator : IVirtualHeightMapGenerator
    {
        private readonly HeightLayer[] _layers;

        public VirtualHeightMapGenerator(HeightMapSettings heightMapSettings)
        {
            if (heightMapSettings == null || heightMapSettings.HeightLayers == null || heightMapSettings.HeightLayers.Length == 0)
            {
                Debug.LogError("[VirtualHeightMapGenerator] Settings or Layers are missing!");
                _layers = Array.Empty<HeightLayer>();
                return;
            }

            _layers = heightMapSettings.HeightLayers;
        }

        /// <summary>
        /// Генерує віртуальну мапу висот, яка є матрицею рядків (TypeId) на основі вхідної матриці висот (float).
        /// </summary>
        public void GenerateVirtualHeightMap(float[,] heightMap, Action<string[,]> onComplete)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            string[,] virtualMap = new string[width, height];

            if (_layers.Length == 0)
            {
                onComplete?.Invoke(virtualMap);
                return;
            }

            int seed = GlobalSeed.Current;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    virtualMap[x, y] = HeightLayerTileSelector.ResolveTileId(_layers, heightMap[x, y], x, y, seed);
                }
            }

            // Повертаємо результат через Callback
            onComplete?.Invoke(virtualMap);
        }
    }
}