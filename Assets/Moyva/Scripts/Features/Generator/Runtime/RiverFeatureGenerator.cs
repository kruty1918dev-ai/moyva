using System.Collections;
using System.Collections.Generic;
using System.Linq; // Додаємо для сортування
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal class RiverFeatureGenerator : IMapFeatureGenerator
    {
        private readonly RiverDataConfig _config;
        private readonly IRiverPathfinder _pathfinder;

        public RiverFeatureGenerator(RiverDataConfig config, IRiverPathfinder pathfinder)
        {
            _config = config;
            _pathfinder = pathfinder;
        }

       public IEnumerator ApplyFeaturesRoutine(string[,] biomes, string[,] objects, float[,] heights, int w, int h)
{
    // 1. Збираємо всі точки на краях мапи
    List<Vector2Int> edgePoints = new List<Vector2Int>();
    for (int x = 0; x < w; x++) { edgePoints.Add(new(x, 0)); edgePoints.Add(new(x, h - 1)); }
    for (int y = 1; y < h - 1; y++) { edgePoints.Add(new(0, y)); edgePoints.Add(new(w - 1, y)); }

    // 2. Сортуємо крайові точки за висотою (від найвищої до найнижчої)
    var sortedEdges = edgePoints.OrderByDescending(p => heights[p.x, p.y]).ToList();

    // 3. Знаходимо найнижчу точку на всій карті (Фініш)
    Vector2Int end = new(0, 0);
    float minTotalH = 2f;
    for (int x = 0; x < w; x++) {
        for (int y = 0; y < h; y++) {
            if (heights[x, y] < minTotalH) { minTotalH = heights[x, y]; end = new(x, y); }
        }
    }

    List<Vector2Int> chosenStarts = new List<Vector2Int>();
    int minDistance = 50; // Мінімальна відстань між витоками

    // Генеруємо річки
    for (int r = 0; r < _config.RiversCount; r++)
    {
        Vector2Int start = Vector2Int.zero;
        bool foundValidStart = false;

        // Шукаємо точку серед відсортованих країв, яка задовольняє дистанцію
        // Перевіряємо перші 100 найвищих точок (щоб був вибір і не брати низини)
        int searchPool = Mathf.Min(100, sortedEdges.Count);
        
        // Спробуємо знайти точку, яка далеко від інших
        for (int i = 0; i < searchPool; i++)
        {
            // Беремо випадкову точку з верхівки списку (щоб не завжди найвищу)
            int randomIndex = Random.Range(0, searchPool);
            Vector2Int candidate = sortedEdges[randomIndex];

            bool tooClose = false;
            foreach (var existingStart in chosenStarts)
            {
                if (Vector2Int.Distance(candidate, existingStart) < minDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                start = candidate;
                foundValidStart = true;
                break;
            }
        }

        // Якщо за 100 спроб не знайшли ідеальну відстань, беремо просто наступну за висотою (фолбек)
        if (!foundValidStart && sortedEdges.Count > 0)
        {
            start = sortedEdges[0];
            foundValidStart = true;
        }

        if (foundValidStart)
        {
            chosenStarts.Add(start);
            sortedEdges.Remove(start);

            // 4. Прокладаємо шлях
            var path = _pathfinder.FindRiverPath(start, end, biomes, heights, w, h, _config);

            if (path != null)
            {
                for (int i = 0; i < path.Count; i++)
                {
                    Vector2Int curr = path[i];
                    string currentBiome = biomes[curr.x, curr.y].ToLower();
                    
                    if (currentBiome == "water" || currentBiome == "grass-water") 
                        continue;

                    Vector2Int? prev = i > 0 ? path[i - 1] : null;
                    Vector2Int? next = i < path.Count - 1 ? path[i + 1] : null;
                    
                    objects[curr.x, curr.y] = DetermineTile(prev, curr, next);
                }
            }
        }
        
        yield return null; 
    }
}
        private string DetermineTile(Vector2Int? p, Vector2Int c, Vector2Int? n)
        {
            Vector2Int d1 = p.HasValue ? (p.Value - c) : (c - (n ?? c + Vector2Int.up));
            Vector2Int d2 = n.HasValue ? (n.Value - c) : (c - (p ?? c + Vector2Int.down));

            if (d1.x == 0 && d2.x == 0) return _config.VerticalTiles[0];
            if (d1.y == 0 && d2.y == 0) return _config.HorizontalTiles[0];

            if ((d1.y == -1 && d2.x == 1) || (d1.x == 1 && d2.y == -1)) return _config.CornerBottomRightTiles[0];
            if ((d1.y == -1 && d2.x == -1) || (d1.x == -1 && d2.y == -1)) return _config.CornerBottomLeftTiles[0];
            if ((d1.y == 1 && d2.x == 1) || (d1.x == 1 && d2.y == 1)) return _config.CornerTopRightTiles[0];
            if ((d1.y == 1 && d2.x == -1) || (d1.x == -1 && d2.y == 1)) return _config.CornerTopLeftTiles[0];

            return _config.HorizontalTiles[0];
        }
    }
}