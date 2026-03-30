using System;
using System.Collections;
using System.Linq;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class VirtualHeightMapGenerator : IVirtualHeightMapGenerator
    {
        private readonly HeightLayer[] _sortedLayers;

        public VirtualHeightMapGenerator(HeightMapSettings heightMapSettings)
        {
            if (heightMapSettings == null || heightMapSettings.HeightLayers == null || heightMapSettings.HeightLayers.Length == 0)
            {
                Debug.LogError("[VirtualHeightMapGenerator] Settings or Layers are missing!");
                _sortedLayers = Array.Empty<HeightLayer>();
                return;
            }

            _sortedLayers = heightMapSettings.HeightLayers
                .OrderBy(l => l.MaxHeight)
                .ToArray();
        }

        /// <summary>
        /// Корутина для генерації віртуальної мапи без фризів
        /// </summary>
        public IEnumerator GenerateVirtualHeightMapRoutine(float[,] heightMap, Action<string[,]> onComplete)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            string[,] virtualMap = new string[width, height];

            if (_sortedLayers.Length == 0)
            {
                onComplete?.Invoke(virtualMap);
                yield break;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    virtualMap[x, y] = ResolveTileID(heightMap[x, y]);
                }

                // Кожні 10-20 рядків даємо Unity відрендерити кадр, щоб не було фризу.
                // Можна міняти (x % 20), залежно від розміру мапи.
                if (x % 32 == 0) 
                {
                    yield return null;
                }
            }

            // Повертаємо результат через Callback
            onComplete?.Invoke(virtualMap);
        }

        private string ResolveTileID(float height)
        {
            for (int i = 0; i < _sortedLayers.Length; i++)
            {
                if (height <= _sortedLayers[i].MaxHeight)
                {
                    return _sortedLayers[i].TileID;
                }
            }

            return _sortedLayers.Length > 0 ? _sortedLayers[^1].TileID : null;
        }
    }
}