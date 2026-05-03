using System;
using System.Collections;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
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

        public void ResolveBiomes(float[,] heightMap, string[,] currentMap, Action<string[,]> onComplete)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            // Якщо вхідної мапи немає/розмір не збігається — працюємо з порожньою,
            // але далі гарантуємо заповнення через DefaultTileID.
            string[,] resultBiomes = currentMap;
            if (resultBiomes == null ||
                resultBiomes.GetLength(0) != width ||
                resultBiomes.GetLength(1) != height)
            {
                resultBiomes = new string[width, height];
            }

            string defaultTile = GetSafeDefaultTile();

            float[,] moistureMap = GenerateMoistureMap(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float h = heightMap[x, y];
                    float m = moistureMap[x, y];

                    string selectedTile = SelectBiome(h, m);

                    if (!string.IsNullOrEmpty(selectedTile))
                        resultBiomes[x, y] = selectedTile;
                    else if (string.IsNullOrEmpty(resultBiomes[x, y]))
                        resultBiomes[x, y] = defaultTile;
                }
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
            return null;
        }

        private string GetSafeDefaultTile()
        {
            if (_settings == null || string.IsNullOrWhiteSpace(_settings.DefaultTileID))
                return "grass";

            return _settings.DefaultTileID;
        }

        private float[,] GenerateMoistureMap(int width, int height)
        {
            float[,] map = new float[width, height];
            float scale = _settings.MoistureScale;
            var rng = GlobalSeed.CreateRandom("BiomeResolver:Moisture");
            float offsetX = (float)rng.NextDouble() * 9999f;
            float offsetY = (float)rng.NextDouble() * 9999f;

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