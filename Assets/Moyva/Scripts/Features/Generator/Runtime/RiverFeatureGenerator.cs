using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class RiverFeatureGenerator : IMapFeatureGenerator
    {
        private readonly string _baseObjectId;
        private readonly int _riversCount;
        private readonly float _usedCellPenalty;
        private readonly float _nearRiverPenalty;
        private readonly int _nearRiverRadius;
        private readonly IRiverPathfinder _pathfinder;

        public RiverFeatureGenerator(RiverDataConfig config, IRiverPathfinder pathfinder)
        {
            _baseObjectId = config.BaseObjectId;
            _riversCount = config.RiversCount;
            _usedCellPenalty = config.UsedCellPenalty;
            _nearRiverPenalty = config.NearRiverPenalty;
            _nearRiverRadius = config.NearRiverRadius;
            _pathfinder = pathfinder;
        }

        public RiverFeatureGenerator(
            string baseObjectId,
            int riversCount,
            float usedCellPenalty,
            float nearRiverPenalty,
            int nearRiverRadius,
            IRiverPathfinder pathfinder)
        {
            _baseObjectId = baseObjectId;
            _riversCount = riversCount;
            _usedCellPenalty = usedCellPenalty;
            _nearRiverPenalty = nearRiverPenalty;
            _nearRiverRadius = nearRiverRadius;
            _pathfinder = pathfinder;
        }

        public void ApplyFeatures(string[,] biomes, string[,] objects, float[,] heights, int w, int h)
        {
            var riverUsageMap = new int[w, h];

            // 1. Збираємо всі точки на краях мапи
            var edgePoints = new List<Vector2Int>();
            for (int x = 0; x < w; x++)
            {
                edgePoints.Add(new Vector2Int(x, 0));
                edgePoints.Add(new Vector2Int(x, h - 1));
            }
            for (int y = 1; y < h - 1; y++)
            {
                edgePoints.Add(new Vector2Int(0, y));
                edgePoints.Add(new Vector2Int(w - 1, y));
            }

            // 2. Перемішуємо
            for (int i = edgePoints.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (edgePoints[i], edgePoints[j]) = (edgePoints[j], edgePoints[i]);
            }

            // 3. Знаходимо найнижчу точку на карті
            Vector2Int end = Vector2Int.zero;
            float minH = float.MaxValue;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (heights[x, y] < minH)
                    {
                        minH = heights[x, y];
                        end = new Vector2Int(x, y);
                    }
                }
            }

            // 4. Генеруємо річки від випадкових крайових точок до найнижчої
            int count = Mathf.Min(_riversCount, edgePoints.Count);
            for (int r = 0; r < count; r++)
            {
                Vector2Int start = edgePoints[r];

                var path = _pathfinder.FindRiverPath(
                    start,
                    end,
                    heights,
                    w,
                    h,
                    riverUsageMap,
                    _usedCellPenalty,
                    _nearRiverPenalty,
                    _nearRiverRadius);
                if (path == null || path.Count == 0)
                    continue;

                foreach (var cell in path)
                {
                    riverUsageMap[cell.x, cell.y]++;

                    if (IsWaterLike(biomes[cell.x, cell.y]))
                        continue;

                    objects[cell.x, cell.y] = _baseObjectId;
                }
            }
        }

        private static bool IsWaterLike(string biomeId)
        {
            if (string.IsNullOrWhiteSpace(biomeId))
                return false;

            string normalized = biomeId.ToLowerInvariant();
            return normalized.Contains("water") || normalized.Contains("sea") || normalized.Contains("lake");
        }
    }
}