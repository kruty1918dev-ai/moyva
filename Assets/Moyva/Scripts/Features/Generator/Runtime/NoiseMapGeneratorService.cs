using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class NoiseMapGeneratorService : INoiseProvider
    {
        public float[,] GenerateNoiseMap(DataNoiseSettings settings, int width, int height)
        {
            float[,] noiseMap = new float[width, height];

            // Генерація випадкових офсетів для кожної октави, щоб мапа була різною при зміні Seed
            System.Random prng = new System.Random(settings.Seed);
            Vector2[] octaveOffsets = new Vector2[settings.Octaves];
            for (int i = 0; i < settings.Octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + settings.Offset.x;
                float offsetY = prng.Next(-100000, 100000) + settings.Offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            // Центрування маштабування
            float halfWidth = width / 2f;
            float halfHeight = height / 2f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < settings.Octaves; i++)
                    {
                        float sampleX = (x - halfWidth) / settings.Scale * frequency + octaveOffsets[i].x;
                        float sampleY = (y - halfHeight) / settings.Scale * frequency + octaveOffsets[i].y;

                        // Використовуємо стандартний Mathf.PerlinNoise (повертає 0..1)
                        // Переводимо в діапазон -1..1 для кращої роботи октав
                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= settings.Persistance;
                        frequency *= settings.Lacunarity;
                    }

                    if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                    if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

                    noiseMap[x, y] = noiseHeight;
                }
            }

            // Нормалізація значень назад у діапазон 0..1
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
            }

            return noiseMap;
        }

         public float[,] GenerateNoiseMap(NoiseSettings settings, int width, int height, int seed)
        {
            float[,] noiseMap = new float[width, height];

            // Генерація випадкових офсетів для кожної октави, щоб мапа була різною при зміні Seed
            System.Random prng = new System.Random(seed);
            Vector2[] octaveOffsets = new Vector2[settings.octaves];
            for (int i = 0; i < settings.octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + settings.offset.x;
                float offsetY = prng.Next(-100000, 100000) + settings.offset.y;
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            // Центрування маштабування
            float halfWidth = width / 2f;
            float halfHeight = height / 2f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < settings.octaves; i++)
                    {
                        float sampleX = (x - halfWidth) / settings.scale * frequency + octaveOffsets[i].x;
                        float sampleY = (y - halfHeight) / settings.scale * frequency + octaveOffsets[i].y;

                        // Використовуємо стандартний Mathf.PerlinNoise (повертає 0..1)
                        // Переводимо в діапазон -1..1 для кращої роботи октав
                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= settings.persistance;
                        frequency *= settings.lacunarity;
                    }

                    if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                    if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

                    noiseMap[x, y] = noiseHeight;
                }
            }

            // Нормалізація значень назад у діапазон 0..1
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
            }

            return noiseMap;
        }
    }
}