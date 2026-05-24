using System.Collections.Generic;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Pathfinding.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Pathfinding.Runtime
{
    internal sealed class Pathfinder : IPathfinder
    {
        private readonly IGridService _gridService;
        private readonly ITileSettingsService _tileSettings;
        private readonly IObjectsMapService _objectsMapService;
        private readonly INeighborhoodStrategy _neighborhoodStrategy;

        public Pathfinder(IGridService gridService, ITileSettingsService tileSettings, IObjectsMapService objectsMapService)
            : this(gridService, tileSettings, objectsMapService, new MooreNeighborhoodStrategy())
        {
        }

        [Inject]
        public Pathfinder(
            IGridService gridService,
            ITileSettingsService tileSettings,
            IObjectsMapService objectsMapService,
            [InjectOptional] INeighborhoodStrategy neighborhoodStrategy)
        {
            _gridService = gridService;
            _tileSettings = tileSettings;
            _objectsMapService = objectsMapService;
            _neighborhoodStrategy = neighborhoodStrategy ?? new MooreNeighborhoodStrategy();
        }

        public IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos)
        {
            return _neighborhoodStrategy.GetNeighbors(pos, _gridService);
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
                    if (!_gridService.TryGetTileData(neighbor, out var tileTypeId)) continue;
                    if (string.IsNullOrEmpty(tileTypeId)) continue;

                    // 1. ПЕРЕВІРКА ОКУПАЦІЇ: зайняті тайли не можна використовувати в маршруті.
                    // Стартовий тайл ігноруємо, бо на ньому вже стоїть поточний юніт.
                    if (_objectsMapService.IsOccupied(neighbor) && neighbor != start)
                    {
                        continue;
                    }

                    // 2. ВРАХУВАННЯ ВАГИ (СТАМІНИ)
                    float tileWeight = _tileSettings.GetTileWeight(tileTypeId);
                    
                    float stepCost = _neighborhoodStrategy.GetStepCost(current, neighbor) * tileWeight;

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
            return _neighborhoodStrategy.EstimateDistance(a, b);
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