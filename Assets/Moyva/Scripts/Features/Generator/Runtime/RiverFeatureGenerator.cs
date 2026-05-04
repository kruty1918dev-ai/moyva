using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
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
            var startPoints = CollectRiverStarts(biomes, heights, w, h);
            var waterTargets = CollectWaterTargets(biomes, heights, w, h);

            if (startPoints.Count == 0 || waterTargets.Count == 0)
                return;

            Shuffle(startPoints, GlobalSeed.CreateRandom($"RiverFeatureGenerator:{w}:{h}:{_riversCount}"));

            int generated = 0;
            int attempts = Mathf.Min(startPoints.Count, Mathf.Max(_riversCount * 4, _riversCount));
            for (int attempt = 0; attempt < attempts && generated < _riversCount; attempt++)
            {
                Vector2Int start = startPoints[attempt];
                Vector2Int end = FindNearestWaterTarget(start, waterTargets);

                if (start == end || ManhattanDistance(start, end) < 8)
                    continue;

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
                if (path == null || path.Count < 4)
                    continue;

                foreach (var cell in path)
                {
                    riverUsageMap[cell.x, cell.y]++;

                    if (IsWaterLike(biomes[cell.x, cell.y]))
                        continue;

                    objects[cell.x, cell.y] = _baseObjectId;
                }

                generated++;
            }
        }

        private static List<Vector2Int> CollectRiverStarts(string[,] biomes, float[,] heights, int width, int height)
        {
            var candidates = new List<Vector2Int>();
            float maxHeight = 0f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!IsWaterLike(biomes[x, y]))
                        maxHeight = Mathf.Max(maxHeight, heights[x, y]);
                }
            }

            float primaryThreshold = Mathf.Max(0.56f, maxHeight - 0.18f);
            CollectStartsAboveThreshold(candidates, biomes, heights, width, height, primaryThreshold);

            if (candidates.Count == 0)
                CollectStartsAboveThreshold(candidates, biomes, heights, width, height, 0.45f);

            return candidates;
        }

        private static void CollectStartsAboveThreshold(
            List<Vector2Int> candidates,
            string[,] biomes,
            float[,] heights,
            int width,
            int height,
            float threshold)
        {
            int edgePadding = Mathf.Min(4, Mathf.Max(1, Mathf.Min(width, height) / 12));

            for (int x = edgePadding; x < width - edgePadding; x++)
            {
                for (int y = edgePadding; y < height - edgePadding; y++)
                {
                    if (IsWaterLike(biomes[x, y]))
                        continue;

                    if (heights[x, y] >= threshold)
                        candidates.Add(new Vector2Int(x, y));
                }
            }
        }

        private static List<Vector2Int> CollectWaterTargets(string[,] biomes, float[,] heights, int width, int height)
        {
            var targets = new List<Vector2Int>();
            Vector2Int lowest = Vector2Int.zero;
            float lowestHeight = float.MaxValue;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (heights[x, y] < lowestHeight)
                    {
                        lowestHeight = heights[x, y];
                        lowest = new Vector2Int(x, y);
                    }

                    if (IsWaterLike(biomes[x, y]))
                        targets.Add(new Vector2Int(x, y));
                }
            }

            if (targets.Count == 0)
                targets.Add(lowest);

            return targets;
        }

        private static Vector2Int FindNearestWaterTarget(Vector2Int start, List<Vector2Int> waterTargets)
        {
            Vector2Int best = waterTargets[0];
            int bestDistance = ManhattanDistance(start, best);

            for (int targetIndex = 1; targetIndex < waterTargets.Count; targetIndex++)
            {
                Vector2Int candidate = waterTargets[targetIndex];
                int distance = ManhattanDistance(start, candidate);
                if (distance < bestDistance)
                {
                    best = candidate;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private static int ManhattanDistance(Vector2Int first, Vector2Int second)
        {
            return Mathf.Abs(first.x - second.x) + Mathf.Abs(first.y - second.y);
        }

        private static void Shuffle(List<Vector2Int> points, System.Random rng)
        {
            for (int index = points.Count - 1; index > 0; index--)
            {
                int swapIndex = rng.Next(index + 1);
                (points[index], points[swapIndex]) = (points[swapIndex], points[index]);
            }
        }

        private static bool IsWaterLike(string biomeId)
        {
            if (string.IsNullOrWhiteSpace(biomeId))
                return false;

            string normalized = biomeId.ToLowerInvariant();
            return normalized.Contains("water") || normalized.Contains("sea") || normalized.Contains("lake") || normalized.Contains("ocean");
        }
    }
}