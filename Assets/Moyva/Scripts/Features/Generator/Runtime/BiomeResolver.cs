using System;
using System.Collections;
using System.Linq;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class BiomeResolver : IBiomeResolver
    {
        private readonly BiomeData[] _sortedBiomes;

        public BiomeResolver(DataBiomesSettings dataBiomes)
        {
            if (dataBiomes == null || dataBiomes.Biomes == null || dataBiomes.Biomes.Length == 0)
            {
                Debug.LogError("[BiomeResolver] Налаштування біомів порожні або відсутні в DataBiomesSettings!");
                _sortedBiomes = Array.Empty<BiomeData>();
                return;
            }

            _sortedBiomes = dataBiomes.Biomes.OrderBy(b => b.HeightThreshold).ToArray();
        }

        /// <summary>
        /// Корутина для визначення біомів на основі карти висот.
        /// </summary>
        public IEnumerator ResolveBiomesRoutine(float[,] heightMap, Action<string[,]> onComplete)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            string[,] biomes = new string[width, height];

            if (_sortedBiomes.Length == 0)
            {
                onComplete?.Invoke(biomes);
                yield break;
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float heightValue = heightMap[x, y];
                    biomes[x, y] = GetBiomeForHeight(heightValue);
                }

                // Щоб уникнути фризів на великих картах, робимо паузу кожні 64 рядки.
                // Це значення можна налаштувати: менше число = плавніша гра, але довша генерація.
                if (x % 64 == 0)
                {
                    yield return null;
                }
            }

            // Передаємо готову матрицю біомів назад у MapDataGenerator
            onComplete?.Invoke(biomes);
        }

        private string GetBiomeForHeight(float height)
        {
            for (int i = 0; i < _sortedBiomes.Length; i++)
            {
                if (height <= _sortedBiomes[i].HeightThreshold)
                {
                    return _sortedBiomes[i].TileID;
                }
            }

            return _sortedBiomes[^1].TileID;
        }
    }
}