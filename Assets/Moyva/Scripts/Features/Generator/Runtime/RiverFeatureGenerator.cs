using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal class RiverFeatureGenerator : IMapFeatureGenerator
    {
        private readonly RiverDataConfig _riverConfig;
        private readonly IRiverPathfinder _pathfinder;

        public RiverFeatureGenerator(RiverDataConfig riverConfig, IRiverPathfinder pathfinder)
        {
            _riverConfig = riverConfig;
            _pathfinder = pathfinder;
        }

        public IEnumerator ApplyFeaturesRoutine(string[,] biomeMap, float[,] heightMap, int width, int height)
        {
            var potentialStarts = GetPointsInRange(heightMap, width, height, _riverConfig.StartHeightRange);
            var potentialEnds = GetPointsInRange(heightMap, width, height, _riverConfig.EndHeightRange);

            if (potentialStarts.Count == 0 || potentialEnds.Count == 0)
            {
                Debug.LogWarning("[RiverGenerator] Не знайдено точок у заданих діапазонах висот.");
                yield break;
            }

            for (int i = 0; i < _riverConfig.RiversCount; i++)
            {
                Vector2Int startPoint = potentialStarts[Random.Range(0, potentialStarts.Count)];
                Vector2Int endPoint = potentialEnds[Random.Range(0, potentialEnds.Count)];

                yield return null;

                List<Vector2Int> riverPath = _pathfinder.FindRiverPath(startPoint, endPoint, heightMap, width, height);

                if (riverPath != null && riverPath.Count > 0)
                {
                    ApplyRiverBrush(biomeMap, width, height, riverPath);
                }

                yield return null;
            }
        }

        private void ApplyRiverBrush(string[,] biomeMap, int mapWidth, int mapHeight, List<Vector2Int> path)
        {
            // 1. Сортуємо від найбільшого до найменшого для черговості малювання (Берег -> Русло)
            var sortedLayers = _riverConfig.WidthLayers.OrderByDescending(l => l.Radius).ToList();

            // 2. Створюємо словник пріоритетів: 
            // Чим менший радіус (вужчий шар), тим вищий пріоритет.
            // Наприклад: Sand (R=5) -> Priority 0, Water (R=1) -> Priority 2.
            Dictionary<string, int> riverTilePriority = new Dictionary<string, int>();
            for (int i = 0; i < sortedLayers.Count; i++)
            {
                // Оскільки список відсортований за спаданням радіусу (index 0 - найбільший),
                // ми просто даємо пріоритет рівний індексу.
                // Тоді останній (найвужчий) шар матиме найбільший індекс (найвищий пріоритет).
                riverTilePriority[sortedLayers[i].TileID] = i;
            }

            // Кешуємо контекст до початку роботи річки
            Dictionary<Vector2Int, string> originalContext = path.ToDictionary(p => p, p => biomeMap[p.x, p.y]);

            foreach (var layer in sortedLayers)
            {
                int radius = Mathf.CeilToInt(layer.Radius);
                float sqrRadius = layer.Radius * layer.Radius;
                int currentLayerPriority = riverTilePriority[layer.TileID];

                foreach (var point in path)
                {
                    string centerTile = originalContext[point];

                    // Якщо центр шляху — перешкода для цього шару, пропускаємо цей сегмент
                    if (IsTileInList(centerTile, layer.ObstacleTileIDs)) continue;

                    for (int x = -radius; x <= radius; x++)
                    {
                        for (int y = -radius; y <= radius; y++)
                        {
                            int tx = point.x + x;
                            int ty = point.y + y;

                            if (tx < 0 || tx >= mapWidth || ty < 0 || ty >= mapHeight) continue;

                            if (x * x + y * y <= sqrRadius)
                            {
                                string tileOnMap = biomeMap[tx, ty];

                                // ЛОГІКА ПРІОРИТЕТІВ:
                                if (riverTilePriority.TryGetValue(tileOnMap, out int existingPriority))
                                {
                                    // Якщо на мапі вже є річковий тайл, і його пріоритет ВИЩИЙ за наш
                                    // (наприклад, там вже вода (2), а ми хочемо покласти пісок (0)),
                                    // то ми НЕ перекриваємо його.
                                    if (existingPriority > currentLayerPriority)
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    // Якщо це звичайний тайл мапи (трава, камінь), 
                                    // перевіряємо, чи він не є перешкодою.
                                    if (IsTileInList(tileOnMap, layer.ObstacleTileIDs))
                                    {
                                        continue;
                                    }
                                }

                                // Малюємо: або поверх слабшого річкового тайлу, або поверх дозволеного біому
                                biomeMap[tx, ty] = layer.TileID;
                            }
                        }
                    }
                }
            }
        }

        private bool IsTileInList(string tileId, string[] list)
        {
            if (list == null || string.IsNullOrEmpty(tileId)) return false;
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == tileId) return true;
            }
            return false;
        }

        private List<Vector2Int> GetPointsInRange(float[,] heightMap, int width, int height, Vector2 range)
        {
            List<Vector2Int> points = new List<Vector2Int>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float h = heightMap[x, y];
                    if (h >= range.x && h <= range.y) points.Add(new Vector2Int(x, y));
                }
            }
            return points;
        }
    }
}