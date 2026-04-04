using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    [Serializable]
    public class ScatterLayer
    {
        public string ObjectId = "tree";
        [Range(0f, 1f)] public float MinHeight = 0.2f;
        [Range(0f, 1f)] public float MaxHeight = 0.5f;
        [Range(0f, 1f)] public float Density = 0.3f;
        [Range(0f, 1f)] public float ClusterStrength = 0.5f;
    }

    [NodeInfo("Multi-Layer Scatter", "Features")]
    public sealed class MultiLayerScatterNode : NodeBase
    {
        [Header("Scatter Layers")]
        [SerializeField] private ScatterLayer[] _layers = new[]
        {
            new ScatterLayer { ObjectId = "tree_oak", MinHeight = 0.3f, MaxHeight = 0.5f, Density = 0.25f, ClusterStrength = 0.6f },
            new ScatterLayer { ObjectId = "bush", MinHeight = 0.2f, MaxHeight = 0.45f, Density = 0.15f, ClusterStrength = 0.3f },
            new ScatterLayer { ObjectId = "rock_small", MinHeight = 0.5f, MaxHeight = 0.7f, Density = 0.2f, ClusterStrength = 0.2f },
            new ScatterLayer { ObjectId = "flower", MinHeight = 0.25f, MaxHeight = 0.4f, Density = 0.1f, ClusterStrength = 0.1f }
        };
        [SerializeField] private int _seed = 42;
        [SerializeField] private bool _avoidExistingObjects = true;

        public override string Title => "Multi-Layer Scatter";
        public override string Category => "Features";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<string[,]>("ObjectMap"),
            PortDefinition.Input<bool[,]>("Mask")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("ObjectMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs[0] as float[,];
            if (heightMap == null)
                return NodeOutput.Error("HeightMap input is required.");

            var mask = inputs[2] as bool[,];

            int w = heightMap.GetLength(0);
            int h = heightMap.GetLength(1);

            var existing = inputs[1] as string[,];
            var result = existing != null ? (string[,])existing.Clone() : new string[w, h];

            if (_layers == null || _layers.Length == 0)
                return NodeOutput.Success(result);

            var rng = new System.Random(_seed);

            // Generate cluster noise per layer
            var clusterMaps = new float[_layers.Length][,];
            for (int i = 0; i < _layers.Length; i++)
            {
                clusterMaps[i] = GenerateClusterNoise(w, h, _seed + i * 13);
            }

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (_avoidExistingObjects && !string.IsNullOrEmpty(result[x, y]))
                        continue;
                    if (mask != null && !mask[x, y])
                        continue;

                    float height = heightMap[x, y];

                    for (int i = 0; i < _layers.Length; i++)
                    {
                        var layer = _layers[i];
                        if (height < layer.MinHeight || height > layer.MaxHeight)
                            continue;

                        // Cluster-influenced density
                        float clusterInfluence = Mathf.Lerp(1f, clusterMaps[i][x, y], layer.ClusterStrength);
                        float effectiveDensity = layer.Density * clusterInfluence;

                        if (rng.NextDouble() < effectiveDensity)
                        {
                            result[x, y] = layer.ObjectId;
                            break; // First placed layer wins
                        }
                    }
                }
            }

            return NodeOutput.Success(result);
        }

        private static float[,] GenerateClusterNoise(int w, int h, int seed)
        {
            var result = new float[w, h];
            float offsetX = seed * 0.73f;
            float offsetY = seed * 1.17f;
            float scale = 0.12f;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float nx = x * scale + offsetX;
                    float ny = y * scale + offsetY;
                    // Multi-octave for more natural clusters
                    float v = Mathf.PerlinNoise(nx, ny) * 0.6f
                            + Mathf.PerlinNoise(nx * 2f, ny * 2f) * 0.3f
                            + Mathf.PerlinNoise(nx * 4f, ny * 4f) * 0.1f;
                    result[x, y] = Mathf.Clamp01(v);
                }
            }
            return result;
        }
    }
}
