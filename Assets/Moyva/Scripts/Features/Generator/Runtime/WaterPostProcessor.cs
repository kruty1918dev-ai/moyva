using System;
using System.Collections;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal class WaterPostProcessor : IMapFeatureGenerator
    {
        private readonly string _waterTileId = "water";

        public IEnumerator ApplyFeaturesRoutine(string[,] biomes, string[,] objects, float[,] heights, int w, int h)
        {
            string[,] resultBiomes = (string[,])biomes.Clone();
            bool changed = false;

            // Прохід для заповнення дрібних дірок (Радіус 1, 8 сусідів)
            for (int x = 1; x < w - 1; x++)
            {
                for (int y = 1; y < h - 1; y++)
                {
                    if (biomes[x, y] == _waterTileId) continue;

                    int neighbors = CountWaterNeighbors(biomes, x, y, w, h, 2);
                    
                    // Якщо 4 або більше з 8 сусідів — вода, заповнюємо. 
                    // Це закриває діагональні розриви та "шахівку".
                    if (neighbors >= 5)
                    {
                        resultBiomes[x, y] = _waterTileId;
                        changed = true;
                    }
                }
                if (x % 64 == 0) yield return null;
            }

            // Оновлюємо основний масив перед другим проходом, якщо треба ще агресивніше
            if (changed) CopyArray(resultBiomes, biomes, w, h);

            // ОПЦІЙНО: Другий прохід з радіусом 2 для з'єднання озер через 2 тайли трави
            for (int x = 2; x < w - 2; x++)
            {
                for (int y = 2; y < h - 2; y++)
                {
                    if (biomes[x, y] == _waterTileId) continue;

                    int neighbors = CountWaterNeighbors(biomes, x, y, w, h, 2);
                    
                    // В радіусі 2 всього 24 сусіда. Якщо хоча б половина (12) — вода,
                    // це гарантовано з'єднає два близьких озера.
                    if (neighbors >= 12) 
                    {
                        resultBiomes[x, y] = _waterTileId;
                        changed = true;
                    }
                }
                if (x % 64 == 0) yield return null;
            }

            if (changed) CopyArray(resultBiomes, biomes, w, h);
        }

        private int CountWaterNeighbors(string[,] map, int x, int y, int w, int h, int radius)
        {
            int count = 0;
            for (int ix = -radius; ix <= radius; ix++)
            {
                for (int iy = -radius; iy <= radius; iy++)
                {
                    if (ix == 0 && iy == 0) continue;
                    int nx = x + ix;
                    int ny = y + iy;

                    if (nx >= 0 && nx < w && ny >= 0 && ny < h)
                    {
                        if (map[nx, ny] == _waterTileId) count++;
                    }
                }
            }
            return count;
        }

        private void CopyArray(string[,] source, string[,] target, int w, int h)
        {
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    target[x, y] = source[x, y];
        }
    }
}