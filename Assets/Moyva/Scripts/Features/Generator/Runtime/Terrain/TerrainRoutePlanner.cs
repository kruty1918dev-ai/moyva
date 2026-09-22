using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TerrainRoutePlan
    {
        public readonly List<Vector2Int> RoadCells = new();
        public readonly List<Vector2Int> FootpathCells = new();
    }

    internal interface ITerrainRoutePlanner
    {
        /// <summary>
        /// Deterministic routes between seeded anchors. Stairs are cheap, plain
        /// ledges above the walk limit are not traversable for routes.
        /// </summary>
        TerrainRoutePlan Plan(
            float[,] surfaces,
            TerrainPassagePlan passages,
            TerrainRouteConfig config,
            int seed,
            float autoStepMaxMeters);
    }

    /// <summary>
    /// Anchors: seeded random cells on valid terrain sorted deterministically
    /// around the map centroid, connected in order by A* (4-neighbour).
    /// </summary>
    internal sealed class TerrainRoutePlanner : ITerrainRoutePlanner
    {
        private const float Epsilon = 0.01f;

        private static readonly Vector2Int[] Directions =
        {
            new(0, 1), new(1, 0), new(0, -1), new(-1, 0),
        };

        public TerrainRoutePlan Plan(
            float[,] surfaces,
            TerrainPassagePlan passages,
            TerrainRouteConfig config,
            int seed,
            float autoStepMaxMeters)
        {
            var plan = new TerrainRoutePlan();
            if (surfaces == null || config == null || !config.Enabled)
                return plan;

            int width = surfaces.GetLength(0);
            int height = surfaces.GetLength(1);
            var anchors = PickAnchors(surfaces, width, height, config, seed);
            if (anchors.Count < 2)
                return plan;

            var stairSteps = BuildStairStepSet(passages);
            int roadRoutes = Mathf.RoundToInt((anchors.Count - 1) * Mathf.Clamp01(config.RoadFraction));

            for (int i = 0; i + 1 < anchors.Count; i++)
            {
                var route = FindPath(
                    surfaces, width, height, anchors[i], anchors[i + 1],
                    stairSteps, config, autoStepMaxMeters);
                if (route == null)
                    continue;

                var target = i < roadRoutes ? plan.RoadCells : plan.FootpathCells;
                foreach (Vector2Int cell in route)
                {
                    if (!target.Contains(cell))
                        target.Add(cell);
                }
            }

            return plan;
        }

        private static List<Vector2Int> PickAnchors(
            float[,] surfaces,
            int width,
            int height,
            TerrainRouteConfig config,
            int seed)
        {
            var random = new System.Random(seed + config.SeedSalt);
            int wanted = Mathf.Max(2, config.AnchorCount);
            var anchors = new List<Vector2Int>(wanted);
            var tried = new HashSet<Vector2Int>();
            int attempts = width * height * 2;

            while (anchors.Count < wanted && attempts-- > 0)
            {
                var cell = new Vector2Int(random.Next(width), random.Next(height));
                if (!tried.Add(cell))
                    continue;
                if (!IsFinite(surfaces[cell.x, cell.y]))
                    continue;
                anchors.Add(cell);
            }

            // Deterministic chain: sort by polar angle around the centroid so
            // consecutive anchors are spatial neighbours, not random pairs.
            var centroid = new Vector2(width * 0.5f, height * 0.5f);
            anchors.Sort((a, b) =>
            {
                float aa = Mathf.Atan2(a.y - centroid.y, a.x - centroid.x);
                float bb = Mathf.Atan2(b.y - centroid.y, b.x - centroid.x);
                int byAngle = aa.CompareTo(bb);
                return byAngle != 0
                    ? byAngle
                    : (a.x * 100003 + a.y).CompareTo(b.x * 100003 + b.y);
            });
            return anchors;
        }

        private static List<Vector2Int> FindPath(
            float[,] surfaces,
            int width,
            int height,
            Vector2Int start,
            Vector2Int goal,
            HashSet<long> stairSteps,
            TerrainRouteConfig config,
            float autoStepMax)
        {
            if (!IsFinite(surfaces[start.x, start.y]) || !IsFinite(surfaces[goal.x, goal.y]))
                return null;

            var open = new List<Vector2Int> { start };
            var gScore = new Dictionary<Vector2Int, float> { [start] = 0f };
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var closed = new HashSet<Vector2Int>();

            while (open.Count > 0)
            {
                int bestIndex = 0;
                float bestScore = float.MaxValue;
                for (int i = 0; i < open.Count; i++)
                {
                    Vector2Int cell = open[i];
                    float f = gScore[cell]
                              + Mathf.Abs(goal.x - cell.x)
                              + Mathf.Abs(goal.y - cell.y);
                    if (f < bestScore)
                    {
                        bestScore = f;
                        bestIndex = i;
                    }
                }

                Vector2Int current = open[bestIndex];
                open.RemoveAt(bestIndex);
                if (current == goal)
                    return Reconstruct(cameFrom, current);
                if (!closed.Add(current))
                    continue;

                float currentY = surfaces[current.x, current.y];
                for (int d = 0; d < Directions.Length; d++)
                {
                    Vector2Int next = current + Directions[d];
                    if (next.x < 0 || next.y < 0 || next.x >= width || next.y >= height)
                        continue;
                    float nextY = surfaces[next.x, next.y];
                    if (!IsFinite(nextY))
                        continue;

                    bool isStair = stairSteps.Contains(
                        TerrainPassageStore.PackStep(current, next));
                    float delta = Mathf.Abs(nextY - currentY);
                    if (!isStair && delta > autoStepMax + Epsilon)
                        continue;

                    float stepCost = 1f
                        + config.HeightPenaltyPerMeter * delta
                        + (isStair ? -0.4f : 0f);
                    float tentative = gScore[current] + stepCost;
                    if (gScore.TryGetValue(next, out float known) && tentative >= known)
                        continue;

                    gScore[next] = tentative;
                    cameFrom[next] = current;
                    if (!closed.Contains(next) && !open.Contains(next))
                        open.Add(next);
                }
            }

            return null;
        }

        private static List<Vector2Int> Reconstruct(
            Dictionary<Vector2Int, Vector2Int> cameFrom,
            Vector2Int current)
        {
            var path = new List<Vector2Int> { current };
            while (cameFrom.TryGetValue(current, out Vector2Int previous))
            {
                current = previous;
                path.Add(current);
            }
            path.Reverse();
            return path;
        }

        private static HashSet<long> BuildStairStepSet(TerrainPassagePlan plan)
        {
            var set = new HashSet<long>();
            if (plan == null)
                return set;
            foreach ((Vector2Int a, Vector2Int b) in plan.EnumerateStairSteps())
                set.Add(TerrainPassageStore.PackStep(a, b));
            return set;
        }

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
