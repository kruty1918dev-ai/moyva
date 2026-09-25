using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITerrainReliefFieldPlanner
    {
        /// <summary>
        /// Deterministic per-cell terrain height in meters, or null when the
        /// recipe disables relief. Values are quantized to
        /// <see cref="TerrainReliefConfig.QuantumMeters"/>.
        /// </summary>
        float[,] Build(int seed, Vector2Int mapSize, TerrainReliefConfig config);
    }

    /// <summary>
    /// Builds the recipe's terrain relief field: seeded fbm noise normalized to
    /// the map, quantized into fixed-height terraces, then smoothed so single
    /// cell spikes collapse into their surrounding plateau.
    /// </summary>
    internal sealed class TerrainReliefPlanner : ITerrainReliefFieldPlanner
    {
        public float[,] Build(int seed, Vector2Int mapSize, TerrainReliefConfig config)
        {
            if (config == null || !config.Enabled)
                return null;

            int width = Mathf.Max(1, mapSize.x);
            int height = Mathf.Max(1, mapSize.y);
            float quantum = Mathf.Max(0.01f, config.QuantumMeters);
            int maxSteps = Mathf.Max(0, config.MaxSteps);

            float[,] noise = BuildNoise(seed + config.SeedSalt, width, height, config,
                out float minValue, out float maxValue);

            var field = new float[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float normalized = Mathf.InverseLerp(minValue, maxValue, noise[x, y]);
                normalized = Mathf.Pow(normalized, Mathf.Max(1f, config.HeightExponent));
                int step = Mathf.Clamp(
                    Mathf.RoundToInt(normalized * maxSteps),
                    0,
                    maxSteps);
                field[x, y] = step * quantum;
            }

            int iterations = Mathf.Max(0, config.SmoothingIterations);
            for (int i = 0; i < iterations; i++)
                field = SmoothLevels(field, width, height, quantum);

            int ridgePasses = Mathf.Max(0, config.RidgeErosionIterations);
            for (int i = 0; i < ridgePasses; i++)
                field = ErodeRidges(field, width, height, quantum);

            if (config.MinPlateauCells > 0)
                field = CollapseSmallPlateaus(field, width, height, quantum, config.MinPlateauCells);

            if (config.BumpMinSpacingCells > 0 || config.MaxBumpCount > 0)
                field = SuppressBumps(
                    field, width, height, quantum,
                    config.BumpMinSpacingCells, config.MaxBumpCount);

            return field;
        }

        private static float[,] BuildNoise(
            int seed,
            int width,
            int height,
            TerrainReliefConfig config,
            out float minValue,
            out float maxValue)
        {
            float scale = Mathf.Max(0.0001f, config.NoiseScale);
            int octaves = Mathf.Max(1, config.Octaves);
            float persistence = Mathf.Clamp(config.Persistence, 0.01f, 1f);
            float lacunarity = Mathf.Max(1f, config.Lacunarity);

            var octaveOffsets = new Vector2[octaves];
            var random = new System.Random(seed);
            for (int i = 0; i < octaves; i++)
            {
                octaveOffsets[i] = new Vector2(
                    random.Next(-100000, 100000) + config.Offset.x,
                    random.Next(-100000, 100000) + config.Offset.y);
            }

            minValue = float.MaxValue;
            maxValue = float.MinValue;
            var noise = new float[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float amplitude = 1f;
                float frequency = 1f;
                float value = 0f;
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = x / scale * frequency + octaveOffsets[i].x;
                    float sampleY = y / scale * frequency + octaveOffsets[i].y;
                    value += Mathf.PerlinNoise(sampleX, sampleY) * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                noise[x, y] = value;
                if (value < minValue) minValue = value;
                if (value > maxValue) maxValue = value;
            }
            return noise;
        }

        /// <summary>
        /// Majority smoothing over quantized terrace levels: a cell adopts the
        /// most common level among itself and its 4-neighbours. Ties keep the
        /// current level, so plateaus stay put.
        /// </summary>
        private static float[,] SmoothLevels(float[,] field, int width, int height, float quantum)
        {
            var result = new float[width, height];
            var votes = new int[16];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                System.Array.Clear(votes, 0, votes.Length);
                int current = Mathf.RoundToInt(field[x, y] / quantum);
                int bestLevel = current;
                int bestVotes = 0;

                Accumulate(field, x, y, width, height, quantum, votes);
                Accumulate(field, x - 1, y, width, height, quantum, votes);
                Accumulate(field, x + 1, y, width, height, quantum, votes);
                Accumulate(field, x, y - 1, width, height, quantum, votes);
                Accumulate(field, x, y + 1, width, height, quantum, votes);

                for (int level = 0; level < votes.Length; level++)
                {
                    // Strictly greater keeps the current level on ties.
                    if (votes[level] > bestVotes)
                    {
                        bestVotes = votes[level];
                        bestLevel = level;
                    }
                }
                result[x, y] = bestLevel * quantum;
            }
            return result;
        }

        private static void Accumulate(
            float[,] field, int x, int y, int width, int height, float quantum, int[] votes)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
                return;
            int level = Mathf.Clamp(Mathf.RoundToInt(field[x, y] / quantum), 0, votes.Length - 1);
            votes[level]++;
        }

        /// <summary>
        /// Lowers every cell standing above all four neighbours by one terrace
        /// step. Repeated passes erode spikes and knife-edge ridges into their
        /// plateau without ever digging below the surround level.
        /// </summary>
        private static float[,] ErodeRidges(float[,] field, int width, int height, float quantum)
        {
            var result = (float[,])field.Clone();
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                int level = Mathf.RoundToInt(field[x, y] / quantum);
                if (level <= 0)
                    continue;

                int minNeighbor = int.MaxValue;
                AccumulateMin(field, x - 1, y, width, height, quantum, ref minNeighbor);
                AccumulateMin(field, x + 1, y, width, height, quantum, ref minNeighbor);
                AccumulateMin(field, x, y - 1, width, height, quantum, ref minNeighbor);
                AccumulateMin(field, x, y + 1, width, height, quantum, ref minNeighbor);

                if (minNeighbor != int.MaxValue && level > minNeighbor)
                    result[x, y] = (level - 1) * quantum;
            }
            return result;
        }

        private static void AccumulateMin(
            float[,] field, int x, int y, int width, int height, float quantum, ref int min)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
                return;
            int level = Mathf.Max(0, Mathf.RoundToInt(field[x, y] / quantum));
            if (level < min)
                min = level;
        }

        /// <summary>
        /// Connected equal-level regions smaller than the minimum adopt the
        /// most common neighbouring level. Runs until stable so cascades of
        /// small terraces merge fully.
        /// </summary>
        private static float[,] CollapseSmallPlateaus(
            float[,] field, int width, int height, float quantum, int minCells)
        {
            for (int pass = 0; pass < 8; pass++)
            {
                var labels = LabelComponents(field, width, height, quantum, out int count);
                if (count == 0)
                    return field;

                var sizes = new int[count];
                var neighborVotes = new Dictionary<int, int>[count];
                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    int label = labels[x, y];
                    sizes[label]++;
                }

                bool changed = false;
                var result = (float[,])field.Clone();
                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    int label = labels[x, y];
                    if (sizes[label] >= minCells)
                        continue;

                    neighborVotes[label] ??= new Dictionary<int, int>();
                    AccumulateNeighborLevel(field, labels, x - 1, y, width, height, quantum, label, neighborVotes[label]);
                    AccumulateNeighborLevel(field, labels, x + 1, y, width, height, quantum, label, neighborVotes[label]);
                    AccumulateNeighborLevel(field, labels, x, y - 1, width, height, quantum, label, neighborVotes[label]);
                    AccumulateNeighborLevel(field, labels, x, y + 1, width, height, quantum, label, neighborVotes[label]);
                }

                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    int label = labels[x, y];
                    if (sizes[label] >= minCells)
                        continue;
                    if (neighborVotes[label] == null || neighborVotes[label].Count == 0)
                        continue;

                    int bestLevel = -1;
                    int bestCount = 0;
                    foreach (var pair in neighborVotes[label])
                    {
                        // Strictly greater keeps the lowest level on ties,
                        // which prefers settling into the lower terrace.
                        if (pair.Value > bestCount || (pair.Value == bestCount && pair.Key < bestLevel))
                        {
                            bestCount = pair.Value;
                            bestLevel = pair.Key;
                        }
                    }

                    if (bestLevel >= 0)
                    {
                        result[x, y] = bestLevel * quantum;
                        changed = true;
                    }
                }

                field = result;
                if (!changed)
                    return field;
            }

            return field;
        }

        private static void AccumulateNeighborLevel(
            float[,] field, int[,] labels, int x, int y, int width, int height,
            float quantum, int ownLabel, Dictionary<int, int> votes)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
                return;
            if (labels[x, y] == ownLabel)
                return;
            int level = Mathf.Max(0, Mathf.RoundToInt(field[x, y] / quantum));
            votes[level] = votes.TryGetValue(level, out int c) ? c + 1 : 1;
        }

        /// <summary>
        /// Elevated bumps (components higher than every neighbour) are kept in
        /// descending level/size order; a bump too close to a kept one or past
        /// the count cap collapses to its highest neighbour level.
        /// </summary>
        private static float[,] SuppressBumps(
            float[,] field, int width, int height, float quantum,
            int minSpacingCells, int maxBumpCount)
        {
            var labels = LabelComponents(field, width, height, quantum, out int count);
            if (count == 0)
                return field;

            var componentLevel = new int[count];
            var componentSize = new int[count];
            var centroidX = new float[count];
            var centroidY = new float[count];
            var maxNeighborLevel = new int[count];
            for (int i = 0; i < count; i++)
                maxNeighborLevel[i] = -1;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                int label = labels[x, y];
                componentLevel[label] = Mathf.Max(0, Mathf.RoundToInt(field[x, y] / quantum));
                componentSize[label]++;
                centroidX[label] += x;
                centroidY[label] += y;
            }

            for (int i = 0; i < count; i++)
            {
                centroidX[i] /= componentSize[i];
                centroidY[i] /= componentSize[i];
            }

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                int label = labels[x, y];
                AccumulateNeighborBumpLevel(field, labels, x - 1, y, width, height, quantum, label, componentLevel, maxNeighborLevel);
                AccumulateNeighborBumpLevel(field, labels, x + 1, y, width, height, quantum, label, componentLevel, maxNeighborLevel);
                AccumulateNeighborBumpLevel(field, labels, x, y - 1, width, height, quantum, label, componentLevel, maxNeighborLevel);
                AccumulateNeighborBumpLevel(field, labels, x, y + 1, width, height, quantum, label, componentLevel, maxNeighborLevel);
            }

            // A bump is a component strictly above every neighbour.
            var bumps = new List<int>();
            for (int i = 0; i < count; i++)
            {
                if (maxNeighborLevel[i] >= 0 && componentLevel[i] > maxNeighborLevel[i])
                    bumps.Add(i);
            }
            if (bumps.Count == 0)
                return field;

            bumps.Sort((a, b) =>
            {
                int byLevel = componentLevel[b].CompareTo(componentLevel[a]);
                return byLevel != 0 ? byLevel : componentSize[b].CompareTo(componentSize[a]);
            });

            var suppressed = new HashSet<int>();
            int kept = 0;
            var keptCentroids = new List<Vector2>();
            foreach (int bump in bumps)
            {
                bool suppress = false;
                if (maxBumpCount > 0 && kept >= maxBumpCount)
                {
                    suppress = true;
                }
                else if (minSpacingCells > 0)
                {
                    var centroid = new Vector2(centroidX[bump], centroidY[bump]);
                    foreach (var keptCentroid in keptCentroids)
                    {
                        float distance = Mathf.Max(
                            Mathf.Abs(centroid.x - keptCentroid.x),
                            Mathf.Abs(centroid.y - keptCentroid.y));
                        if (distance < minSpacingCells)
                        {
                            suppress = true;
                            break;
                        }
                    }
                }

                if (suppress)
                {
                    suppressed.Add(bump);
                }
                else
                {
                    kept++;
                    keptCentroids.Add(new Vector2(centroidX[bump], centroidY[bump]));
                }
            }

            if (suppressed.Count == 0)
                return field;

            var result = (float[,])field.Clone();
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                int label = labels[x, y];
                if (suppressed.Contains(label) && maxNeighborLevel[label] >= 0)
                    result[x, y] = maxNeighborLevel[label] * quantum;
            }
            return result;
        }

        private static void AccumulateNeighborBumpLevel(
            float[,] field, int[,] labels, int x, int y, int width, int height,
            float quantum, int ownLabel, int[] componentLevel, int[] maxNeighborLevel)
        {
            if (x < 0 || y < 0 || x >= width || y >= height)
                return;
            if (labels[x, y] == ownLabel)
                return;
            int level = Mathf.Max(0, Mathf.RoundToInt(field[x, y] / quantum));
            if (level > maxNeighborLevel[ownLabel])
                maxNeighborLevel[ownLabel] = level;
        }

        /// <summary>4-connected components of equal terrace level.</summary>
        private static int[,] LabelComponents(
            float[,] field, int width, int height, float quantum, out int count)
        {
            var labels = new int[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                labels[x, y] = -1;

            var queue = new Queue<Vector2Int>();
            int label = 0;
            for (int sx = 0; sx < width; sx++)
            for (int sy = 0; sy < height; sy++)
            {
                if (labels[sx, sy] >= 0)
                    continue;

                int level = Mathf.Max(0, Mathf.RoundToInt(field[sx, sy] / quantum));
                labels[sx, sy] = label;
                queue.Enqueue(new Vector2Int(sx, sy));
                while (queue.Count > 0)
                {
                    var cell = queue.Dequeue();
                    TryLabelNeighbor(field, labels, width, height, quantum, level, label, cell.x - 1, cell.y, queue);
                    TryLabelNeighbor(field, labels, width, height, quantum, level, label, cell.x + 1, cell.y, queue);
                    TryLabelNeighbor(field, labels, width, height, quantum, level, label, cell.x, cell.y - 1, queue);
                    TryLabelNeighbor(field, labels, width, height, quantum, level, label, cell.x, cell.y + 1, queue);
                }
                label++;
            }

            count = label;
            return labels;
        }

        private static void TryLabelNeighbor(
            float[,] field, int[,] labels, int width, int height, float quantum,
            int level, int label, int x, int y, Queue<Vector2Int> queue)
        {
            if (x < 0 || y < 0 || x >= width || y >= height || labels[x, y] >= 0)
                return;
            if (Mathf.Max(0, Mathf.RoundToInt(field[x, y] / quantum)) != level)
                return;
            labels[x, y] = label;
            queue.Enqueue(new Vector2Int(x, y));
        }
    }
}
