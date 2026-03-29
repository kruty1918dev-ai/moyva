using System;
using System.Collections;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class BiomeResolver : IBiomeResolver
    {
        private readonly DataBiomesSettings _settings;

        public BiomeResolver(DataBiomesSettings settings)
        {
            _settings = settings;
        }

        public IEnumerator ResolveBiomesRoutine(float[,] heightMap, string[,] currentMap, Action<string[,]> onComplete)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            // Працюємо з копією або прямо з поточною мапою
            string[,] resultBiomes = currentMap;

            float[,] moistureMap = GenerateMoistureMap(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float h = heightMap[x, y];
                    float m = moistureMap[x, y];

                    string selectedTile = SelectBiome(h, m);

                    // КЛЮЧОВИЙ МОМЕНТ:
                    // Якщо SelectBiome повернув назву — міняємо тайл.
                    // Якщо повернув null — ми ВЗАГАЛІ не чіпаємо resultBiomes[x, y],
                    // там залишається те, що було записано раніше VirtualHeightMapGenerator-ом.
                    if (!string.IsNullOrEmpty(selectedTile))
                    {
                        resultBiomes[x, y] = selectedTile;
                    }
                }

                if (x % 64 == 0) yield return null;
            }

            onComplete?.Invoke(resultBiomes);
        }

        private string SelectBiome(float height, float moisture)
        {
            if (_settings.Biomes == null) return null;

            foreach (var biome in _settings.Biomes)
            {
                if (height >= biome.MinHeight && height <= biome.MaxHeight &&
                    moisture >= biome.MinMoisture && moisture <= biome.MaxMoisture)
                {
                    return biome.TileID;
                }
            }
            return null; // Повертаємо null, щоб нічого не змінювати в існуючій мапі
        }
        private float[,] GenerateMoistureMap(int width, int height)
        {
            float[,] map = new float[width, height];
            float scale = _settings.MoistureScale;
            float offsetX = UnityEngine.Random.Range(0f, 9999f);
            float offsetY = UnityEngine.Random.Range(0f, 9999f);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float xCoord = (float)x / width * scale + offsetX;
                    float yCoord = (float)y / height * scale + offsetY;
                    map[x, y] = Mathf.PerlinNoise(xCoord, yCoord);
                }
            }
            return map;
        }
    }
}