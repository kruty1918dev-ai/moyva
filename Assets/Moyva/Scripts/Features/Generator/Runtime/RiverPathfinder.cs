using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class RiverPathfinder : IRiverPathfinder
    {
        private const float UphillPenalty = 5000f;
        private const float DownhillReward = 20f;
        private const float MeanderStrength = 2f;

        public List<Vector2Int> FindRiverPath(Vector2Int start, Vector2Int end, float[,] heightMap, int width, int height)
        {
            return FindRiverPath(start, end, heightMap, width, height, null, 0f, 0f, 0);
        }

        public List<Vector2Int> FindRiverPath(
            Vector2Int start,
            Vector2Int end,
            float[,] heightMap,
            int width,
            int height,
            int[,] riverUsageMap,
            float usedCellPenalty,
            float nearRiverPenalty,
            int nearRiverRadius)
        {
            var openSet = new List<Vector2Int> { start };
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var gScore = new Dictionary<Vector2Int, float> { [start] = 0 };
            var fScore = new Dictionary<Vector2Int, float> { [start] = Heuristic(start, end) };
            var rng = GlobalSeed.CreateRandom();

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

                    float heightCost = diff > 0f
                        ? diff * UphillPenalty
                        : diff * DownhillReward;

                    float reuseCost = 0f;
                    if (riverUsageMap != null)
                    {
                        int usage = riverUsageMap[next.x, next.y];
                        if (usage > 0)
                            reuseCost += usage * usedCellPenalty;

                        if (nearRiverPenalty > 0f && nearRiverRadius > 0)
                        {
                            int nearCount = CountUsedCellsAround(next, width, height, riverUsageMap, nearRiverRadius);
                            reuseCost += nearCount * nearRiverPenalty;
                        }
                    }

                    float meanderNoise = (float)rng.NextDouble() * MeanderStrength;
                    float tentG = gScore[current] + 1f + heightCost + reuseCost + meanderNoise;

                    if (tentG < GetVal(gScore, next))
                    {
                        cameFrom[next] = current;
                        gScore[next] = tentG;
                        fScore[next] = tentG + Heuristic(next, end);
                        if (!openSet.Contains(next)) openSet.Add(next);
                    }
                }
            }

            return null;
        }

        private static int CountUsedCellsAround(Vector2Int p, int w, int h, int[,] usageMap, int radius)
        {
            int count = 0;
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (dx == 0 && dy == 0)
                        continue;

                    int nx = p.x + dx;
                    int ny = p.y + dy;
                    if (nx < 0 || nx >= w || ny < 0 || ny >= h)
                        continue;

                    if (usageMap[nx, ny] > 0)
                        count++;
                }
            }

            return count;
        }

        private static float Heuristic(Vector2Int current, Vector2Int end)
        {
            return Mathf.Abs(current.x - end.x) + Mathf.Abs(current.y - end.y);
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