using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class RiverPathfinder : IRiverPathfinder
    {
        public List<Vector2Int> FindRiverPath(Vector2Int start, Vector2Int end, float[,] heightMap, int width, int height)
        {
            var openSet = new List<Vector2Int> { start };
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var gScore = new Dictionary<Vector2Int, float> { [start] = 0 };
            var fScore = new Dictionary<Vector2Int, float> { [start] = Heuristic(start, end) };

            while (openSet.Count > 0)
            {
                Vector2Int current = GetNodeWithLowestFScore(openSet, fScore);

                // Якщо досягли цілі (або сусіднього тайлу цілі)
                if (current == end)
                {
                    return ReconstructPath(cameFrom, current);
                }

                openSet.Remove(current);
                float currentHeight = heightMap[current.x, current.y];

                foreach (var neighbor in GetNeighbors(current, width, height))
                {
                    float neighborHeight = heightMap[neighbor.x, neighbor.y];
                    
                    // Базова відстань (прямо = 1, по діагоналі = 1.414)
                    float distCost = (current.x != neighbor.x && current.y != neighbor.y) ? 1.414f : 1.0f;

                    // КРИТИЧНО ДЛЯ РІЧКИ: Рахуємо перепад висот
                    float heightDiff = neighborHeight - currentHeight;
                    float heightPenalty = 0f;

                    if (heightDiff < 0)
                    {
                        // Вода тече вниз: це ідеально, вартість мінімальна
                        heightPenalty = 0f;
                    }
                    else
                    {
                        // Вода тече вгору або по рівнині: накладаємо величезний штраф.
                        // Чим крутіший підйом, тим більший штраф.
                        heightPenalty = 10f + (heightDiff * 100f); 
                    }

                    // Додаємо трохи випадковості (шуму), щоб річка не була ідеально рівною
                    float meanderNoise = Random.Range(0.0f, 0.5f);

                    float tentativeGScore = GetScore(gScore, current) + distCost + heightPenalty + meanderNoise;

                    if (tentativeGScore < GetScore(gScore, neighbor))
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = tentativeGScore + Heuristic(neighbor, end);

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            return new List<Vector2Int>(); // Не змогли знайти шлях
        }

        private IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos, int width, int height)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>(8);
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    int nx = pos.x + x;
                    int ny = pos.y + y;

                    // Перевірка меж масиву
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        neighbors.Add(new Vector2Int(nx, ny));
                    }
                }
            }
            return neighbors;
        }

        private float Heuristic(Vector2Int a, Vector2Int b)
        {
            // Для річок Чебишов працює добре, бо вода тече в 8 напрямках
            float dx = Mathf.Abs(a.x - b.x);
            float dy = Mathf.Abs(a.y - b.y);
            return (dx + dy) + (1.414f - 2) * Mathf.Min(dx, dy);
        }

        private float GetScore(Dictionary<Vector2Int, float> scores, Vector2Int node)
        {
            return scores.TryGetValue(node, out float score) ? score : float.PositiveInfinity;
        }

        private Vector2Int GetNodeWithLowestFScore(List<Vector2Int> openSet, Dictionary<Vector2Int, float> fScore)
        {
            Vector2Int lowestNode = openSet[0];
            float lowestScore = GetScore(fScore, lowestNode);

            for (int i = 1; i < openSet.Count; i++)
            {
                float score = GetScore(fScore, openSet[i]);
                if (score < lowestScore)
                {
                    lowestScore = score;
                    lowestNode = openSet[i];
                }
            }
            return lowestNode;
        }

        private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
        {
            var path = new List<Vector2Int> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path;
        }
    }
}