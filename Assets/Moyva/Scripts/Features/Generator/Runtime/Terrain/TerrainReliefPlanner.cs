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
            float range = Mathf.Max(0.0001f, maxValue - minValue);
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float normalized = Mathf.InverseLerp(minValue, maxValue, noise[x, y]);
                int step = Mathf.Clamp(
                    Mathf.RoundToInt(normalized * maxSteps),
                    0,
                    maxSteps);
                field[x, y] = step * quantum;
            }

            int iterations = Mathf.Max(0, config.SmoothingIterations);
            for (int i = 0; i < iterations; i++)
                field = SmoothLevels(field, width, height, quantum);

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
    }
}
