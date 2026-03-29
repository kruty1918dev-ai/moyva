using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class RiverPathfinder : IRiverPathfinder
    {
        public List<Vector2Int> FindRiverPath(Vector2Int start, Vector2Int end, string[,] biomeMap, float[,] heightMap, int width, int height, RiverDataConfig config)
        {
            var openSet = new List<Vector2Int> { start };
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var gScore = new Dictionary<Vector2Int, float> { [start] = 0 };
            var fScore = new Dictionary<Vector2Int, float> { [start] = Vector2Int.Distance(start, end) };

            while (openSet.Count > 0)
            {
                Vector2Int current = GetLowestF(openSet, fScore);
                if (current == end) return Reconstruct(cameFrom, current);

                openSet.Remove(current);

                foreach (var next in GetNeighbors(current, width, height))
                {
                    float hCurr = heightMap[current.x, current.y];
                    float hNext = heightMap[next.x, next.y];
                    float diff = hNext - hCurr;

                    // ШТРАФИ:
                    // 1. Величезний штраф за рух вгору (річка не тече вгору)
                    float heightCost = (diff > 0) ? (diff * 5000f) : (diff * 20f); 
                    
                    // 2. ЗВИВИСТІСТЬ (Meander): 
                    // Додаємо випадкове значення до кожного кроку, щоб шлях був ламаним
                    float meanderNoise = Random.Range(0f, 3.0f);

                    float tentG = gScore[current] + 1.0f + heightCost + meanderNoise;

                    if (tentG < GetVal(gScore, next))
                    {
                        cameFrom[next] = current;
                        gScore[next] = tentG;
                        // Евристика Мангеттена для 4 напрямків
                        fScore[next] = tentG + Mathf.Abs(next.x - end.x) + Mathf.Abs(next.y - end.y);
                        if (!openSet.Contains(next)) openSet.Add(next);
                    }
                }
            }
            return null;
        }

        private IEnumerable<Vector2Int> GetNeighbors(Vector2Int p, int w, int h)
        {
            if (p.x > 0) yield return new(p.x - 1, p.y);
            if (p.x < w - 1) yield return new(p.x + 1, p.y);
            if (p.y > 0) yield return new(p.x, p.y - 1);
            if (p.y < h - 1) yield return new(p.x, p.y + 1);
        }

        private float GetVal(Dictionary<Vector2Int, float> d, Vector2Int k) => d.TryGetValue(k, out float v) ? v : float.MaxValue;

        private Vector2Int GetLowestF(List<Vector2Int> set, Dictionary<Vector2Int, float> f)
        {
            Vector2Int best = set[0];
            foreach (var p in set) if (GetVal(f, p) < GetVal(f, best)) best = p;
            return best;
        }

        private List<Vector2Int> Reconstruct(Dictionary<Vector2Int, Vector2Int> cf, Vector2Int c)
        {
            var p = new List<Vector2Int> { c };
            while (cf.ContainsKey(c)) { c = cf[c]; p.Add(c); }
            p.Reverse();
            return p;
        }
    }
}