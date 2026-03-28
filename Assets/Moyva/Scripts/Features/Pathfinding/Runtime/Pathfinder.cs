using System.Collections.Generic;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Pathfinding.Runtime
{
    internal sealed class Pathfinder : IPathfinder
    {
        private readonly IGridService _gridService;
        private readonly ITileSettingsService _tileSettings; // Додано сервіс налаштувань

        public Pathfinder(IGridService gridService, ITileSettingsService tileSettings)
        {
            _gridService = gridService;
            _tileSettings = tileSettings;
        }

        public IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue; 

                    Vector2Int next = new Vector2Int(pos.x + x, pos.y + y);

                    if (_gridService.TryGetTileData(next, out _))
                    {
                        neighbors.Add(next);
                    }
                }
            }
            return neighbors;
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
        {
            if (start == end) return new List<Vector2Int> { start };

            var openSet = new List<Vector2Int> { start };
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var gScore = new Dictionary<Vector2Int, float> { [start] = 0 };
            var fScore = new Dictionary<Vector2Int, float> { [start] = Heuristic(start, end) };

            while (openSet.Count > 0)
            {
                Vector2Int current = GetNodeWithLowestFScore(openSet, fScore);

                if (current == end)
                {
                    return ReconstructPath(cameFrom, current);
                }

                openSet.Remove(current);

                foreach (var neighbor in GetNeighbors(current))
                {
                    if (!_gridService.TryGetTileData(neighbor, out var tileData)) continue;

                    // 1. ПЕРЕВІРКА ОКУПАЦІЇ: Ігноруємо зайняті тайли, щоб обходити перешкоди
                    // Дозволяємо перевірку для цільового тайла (end) та стартового (start)
                    if (tileData.IsOccupied && neighbor != start && neighbor != end)
                    {
                        continue;
                    }

                    // 2. ВРАХУВАННЯ ВАГИ (СТАМІНИ)
                    float tileWeight = _tileSettings.GetTileWeight(tileData.TileTypeId);
                    
                    // Вартість кроку = (базова відстань) * (вага тайла)
                    // Тобто діагональний крок по "болоту" буде коштувати більше, ніж прямий крок по "болоту".
                    float distanceMultiplier = (current.x != neighbor.x && current.y != neighbor.y) ? 1.414f : 1.0f;
                    float stepCost = distanceMultiplier * tileWeight;

                    float tentativeGScore = GetScore(gScore, current) + stepCost;

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

            return new List<Vector2Int>(); // Шлях не знайдено
        }

        private float Heuristic(Vector2Int a, Vector2Int b)
        {
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