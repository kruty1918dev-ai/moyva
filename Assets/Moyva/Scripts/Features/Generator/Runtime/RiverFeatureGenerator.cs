using System.Collections;
using System.Collections.Generic;
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
            // 1. Шукаємо НАЙВИЩУ точку ТІЛЬКИ на КРАЮ (Старт)
            Vector2Int start = new(0, 0);
            float maxEdgeH = -1f;

            for (int x = 0; x < w; x++) { CheckEdge(x, 0); CheckEdge(x, h - 1); }
            for (int y = 1; y < h - 1; y++) { CheckEdge(0, y); CheckEdge(w - 1, y); }

            void CheckEdge(int x, int y)
            {
                if (heights[x, y] > maxEdgeH) { maxEdgeH = heights[x, y]; start = new(x, y); }
            }

            // 2. Шукаємо НАЙНИЖЧУ точку на ВСІЙ КАРТІ (Фініш)
            Vector2Int end = new(0, 0);
            float minTotalH = 2f;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (heights[x, y] < minTotalH) { minTotalH = heights[x, y]; end = new(x, y); }
                }
            }

            // 3. Запускаємо шлях
            var path = _pathfinder.FindRiverPath(start, end, biomes, heights, w, h, _config);

            if (path != null)
            {
                for (int i = 0; i < path.Count; i++)
                {
                    Vector2Int curr = path[i];

                    // Отримуємо назву біому в нижньому регістрі для надійної перевірки
                    string currentBiome = biomes[curr.x, curr.y].ToLower();

                    // Перевірка: не малюємо річку на воді або на переході трава-вода
                    if (currentBiome == "water" || currentBiome == "grass-water")
                        continue;

                    Vector2Int? prev = i > 0 ? path[i - 1] : null;
                    Vector2Int? next = i < path.Count - 1 ? path[i + 1] : null;

                    objects[curr.x, curr.y] = DetermineTile(prev, curr, next);
                }
            }
            yield return null;
        }

        private string DetermineTile(Vector2Int? p, Vector2Int c, Vector2Int? n)
        {
            // d1 - звідки прийшла, d2 - куди йде
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