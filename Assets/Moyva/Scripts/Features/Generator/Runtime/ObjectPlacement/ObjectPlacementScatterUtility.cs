using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.Noise;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ObjectPlacement
{
    internal static class ObjectPlacementScatterUtility
    {
        public static List<ScatterCandidate> ScatterUniform(
            ScatterMask mask,
            ObjectPlacementRule rule,
            int baseSeed)
        {
            var result = new List<ScatterCandidate>();
            if (mask == null || rule == null)
                return result;

            int w = mask.Width;
            int h = mask.Height;
            if (w <= 0 || h <= 0)
                return result;

            int seed = GlobalSeed.Combine(baseSeed, rule.RandomSeed);
            float minDistanceSqr = rule.MinDistance * rule.MinDistance;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!mask.IsAllowed(x, y))
                        continue;

                    float hash = ProceduralNoiseUtility.Hash01(x, y, seed);
                    float noise = ProceduralNoiseUtility.SampleSimplexLike(
                        (x + 0.5f) * 0.17f,
                        (y + 0.5f) * 0.17f,
                        seed ^ 0x359d12ef);
                    float probability = Mathf.Clamp01(rule.Density * mask.GetWeight(x, y));
                    probability *= Mathf.Lerp(0.65f, 1.35f, noise);

                    if (hash > probability)
                        continue;

                    var candidate = CreateCandidate(x, y, rule, seed);
                    if (!PassesMinDistance(result, candidate.Cell, minDistanceSqr))
                        continue;

                    result.Add(candidate);
                }
            }

            return result;
        }

        public static List<ScatterCandidate> ScatterClustered(
            ScatterMask mask,
            ClusterSettings cluster,
            ObjectPlacementRule rule,
            int baseSeed)
        {
            var result = new List<ScatterCandidate>();
            if (mask == null || cluster == null || rule == null)
                return result;

            int w = mask.Width;
            int h = mask.Height;
            if (w <= 0 || h <= 0)
                return result;

            var allowed = CollectAllowedCells(mask);
            if (allowed.Count == 0)
                return result;

            int seed = GlobalSeed.Combine(baseSeed, rule.RandomSeed ^ 0x51a83f1d);
            int[] edgeDistances = BuildEdgeDistanceMap(mask, out int maxDistance);
            var random = new System.Random(seed);
            var centers = PickClusterCenters(allowed, edgeDistances, maxDistance, cluster, random, w);
            float minDistanceSqr = rule.MinDistance * rule.MinDistance;

            for (int i = 0; i < centers.Count; i++)
            {
                var center = centers[i];
                int radius = Mathf.Max(1, cluster.ClusterRadius);
                int minX = Mathf.Max(0, center.x - radius);
                int maxX = Mathf.Min(w - 1, center.x + radius);
                int minY = Mathf.Max(0, center.y - radius);
                int maxY = Mathf.Min(h - 1, center.y + radius);

                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        if (!mask.IsAllowed(x, y))
                            continue;

                        int distanceToEdge = edgeDistances[y * w + x];
                        if (cluster.AvoidCliffEdgeDistance > 0f &&
                            distanceToEdge < cluster.AvoidCliffEdgeDistance)
                            continue;

                        float dx = x - center.x;
                        float dy = y - center.y;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        if (dist > radius)
                            continue;

                        float radial = 1f - Mathf.Clamp01(dist / radius);
                        float noise = ProceduralNoiseUtility.SampleSimplexLike(
                            (x + seed * 0.001f) * cluster.NoiseScale,
                            (y - seed * 0.001f) * cluster.NoiseScale,
                            seed + i * 977);
                        if (noise < cluster.NoiseThreshold)
                            continue;

                        float edgeWeight = EdgeWeight(distanceToEdge, maxDistance, cluster.EdgePreference);
                        float probability = cluster.ClusterDensity * rule.Density * radial * edgeWeight * mask.GetWeight(x, y);
                        probability = Mathf.Clamp01(probability);

                        if (random.NextDouble() > probability)
                            continue;

                        var candidate = CreateCandidate(x, y, rule, seed + i * 131);
                        if (!PassesMinDistance(result, candidate.Cell, minDistanceSqr))
                            continue;

                        result.Add(candidate);
                    }
                }
            }

            return result;
        }

        public static int[] BuildEdgeDistanceMap(ScatterMask mask, out int maxDistance)
        {
            maxDistance = 0;
            int w = Mathf.Max(1, mask?.Width ?? 0);
            int h = Mathf.Max(1, mask?.Height ?? 0);
            var distances = new int[w * h];
            const int large = 1_000_000;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool edge = !mask.IsAllowed(x, y) || HasBlockedNeighbor(mask, x, y);
                    distances[y * w + x] = edge ? 0 : large;
                }
            }

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * w + x;
                    int best = distances[idx];
                    if (x > 0)
                        best = Mathf.Min(best, distances[idx - 1] + 1);
                    if (y > 0)
                        best = Mathf.Min(best, distances[idx - w] + 1);
                    distances[idx] = best;
                }
            }

            for (int y = h - 1; y >= 0; y--)
            {
                for (int x = w - 1; x >= 0; x--)
                {
                    int idx = y * w + x;
                    int best = distances[idx];
                    if (x + 1 < w)
                        best = Mathf.Min(best, distances[idx + 1] + 1);
                    if (y + 1 < h)
                        best = Mathf.Min(best, distances[idx + w] + 1);
                    distances[idx] = best;
                    if (mask.IsAllowed(x, y) && best < large)
                        maxDistance = Mathf.Max(maxDistance, best);
                }
            }

            maxDistance = Mathf.Max(1, maxDistance);
            return distances;
        }

        public static bool[,] BuildEdgeBandMask(bool[,] source, int distanceFromEdge, int falloff, bool invert)
        {
            if (source == null)
                return new bool[1, 1];

            var scatterMask = new ScatterMask(source);
            int[] distances = BuildEdgeDistanceMap(scatterMask, out _);
            int w = source.GetLength(0);
            int h = source.GetLength(1);
            int maxDistance = Mathf.Max(0, distanceFromEdge + falloff);
            var result = new bool[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    bool inBand = source[x, y] && distances[y * w + x] <= maxDistance;
                    result[x, y] = invert ? source[x, y] && !inBand : inBand;
                }
            }

            return result;
        }

        private static List<Vector2Int> CollectAllowedCells(ScatterMask mask)
        {
            var allowed = new List<Vector2Int>();
            for (int x = 0; x < mask.Width; x++)
            {
                for (int y = 0; y < mask.Height; y++)
                {
                    if (mask.IsAllowed(x, y))
                        allowed.Add(new Vector2Int(x, y));
                }
            }

            return allowed;
        }

        private static List<Vector2Int> PickClusterCenters(
            List<Vector2Int> allowed,
            int[] edgeDistances,
            int maxDistance,
            ClusterSettings cluster,
            System.Random random,
            int width)
        {
            var centers = new List<Vector2Int>();
            int count = Mathf.Max(1, cluster.ClusterCount);
            int candidateTries = Mathf.Clamp(6 + Mathf.RoundToInt(cluster.EdgePreference * 10f), 4, 20);

            for (int i = 0; i < count; i++)
            {
                Vector2Int best = allowed[random.Next(allowed.Count)];
                float bestScore = -1f;

                for (int t = 0; t < candidateTries; t++)
                {
                    var cell = allowed[random.Next(allowed.Count)];
                    int distance = edgeDistances[cell.y * width + cell.x];
                    float edgeScore = 1f - Mathf.Clamp01(distance / (float)Mathf.Max(1, maxDistance));
                    float roll = (float)random.NextDouble();
                    float score = Mathf.Lerp(roll, edgeScore, cluster.EdgePreference);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = cell;
                    }
                }

                centers.Add(best);
            }

            return centers;
        }

        private static ScatterCandidate CreateCandidate(int x, int y, ObjectPlacementRule rule, int seed)
        {
            float ox = ProceduralNoiseUtility.Hash01(x, y, seed ^ 0x12345) * 2f - 1f;
            float oy = ProceduralNoiseUtility.Hash01(x, y, seed ^ 0x98765) * 2f - 1f;
            var offset = new Vector2(ox, oy);
            if (offset.sqrMagnitude > 1f)
                offset.Normalize();
            offset *= Mathf.Max(0f, rule.Jitter);

            float rotation = (ProceduralNoiseUtility.Hash01(x, y, seed ^ 0x518af) * 2f - 1f)
                * Mathf.Max(0f, rule.RotationRandomization);

            float scaleMin = Mathf.Min(rule.ScaleRandomization.x, rule.ScaleRandomization.y);
            float scaleMax = Mathf.Max(rule.ScaleRandomization.x, rule.ScaleRandomization.y);
            float scale = Mathf.Lerp(
                Mathf.Max(0.01f, scaleMin),
                Mathf.Max(0.01f, scaleMax),
                ProceduralNoiseUtility.Hash01(x, y, seed ^ 0x35591));

            return new ScatterCandidate(new Vector2Int(x, y), offset, 1f, rotation, scale);
        }

        private static bool PassesMinDistance(
            List<ScatterCandidate> existing,
            Vector2Int cell,
            float minDistanceSqr)
        {
            if (minDistanceSqr <= 0f)
                return true;

            for (int i = 0; i < existing.Count; i++)
            {
                Vector2 delta = existing[i].Cell - cell;
                if (delta.sqrMagnitude < minDistanceSqr)
                    return false;
            }

            return true;
        }

        private static bool HasBlockedNeighbor(ScatterMask mask, int x, int y)
        {
            for (int ox = -1; ox <= 1; ox++)
            {
                for (int oy = -1; oy <= 1; oy++)
                {
                    if (ox == 0 && oy == 0)
                        continue;

                    if (!mask.IsAllowed(x + ox, y + oy))
                        return true;
                }
            }

            return false;
        }

        private static float EdgeWeight(int distanceToEdge, int maxDistance, float edgePreference)
        {
            if (edgePreference <= 0f)
                return 1f;

            float edge = 1f - Mathf.Clamp01(distanceToEdge / (float)Mathf.Max(1, maxDistance));
            return Mathf.Lerp(1f, Mathf.Lerp(0.35f, 1.5f, edge), Mathf.Clamp01(edgePreference));
        }
    }
}
