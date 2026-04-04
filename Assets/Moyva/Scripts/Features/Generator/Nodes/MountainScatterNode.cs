using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    [Serializable]
    public class MountainLayer
    {
        [Range(0f, 1f)] public float MinHeight = 0.65f;
        [Range(0f, 1f)] public float MaxHeight = 1f;
        public string ObjectId = "mountain_large";
        [Range(0f, 1f)] public float Density = 0.4f;
    }

    [NodeInfo("Mountain Scatter", "Features")]
    public sealed class MountainScatterNode : NodeBase
    {
        [Header("Mountain Layers")]
        [SerializeField] private MountainLayer[] _layers = new[]
        {
            new MountainLayer { MinHeight = 0.75f, MaxHeight = 1f, ObjectId = "mountain_large", Density = 0.6f },
            new MountainLayer { MinHeight = 0.65f, MaxHeight = 0.75f, ObjectId = "mountain_small", Density = 0.35f },
            new MountainLayer { MinHeight = 0.55f, MaxHeight = 0.65f, ObjectId = "rock", Density = 0.2f }
        };
        [SerializeField] private int _seed = 42;
        [SerializeField] private bool _avoidExistingObjects = true;

        public override string Title => "Mountain Scatter";
        public override string Category => "Features";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<string[,]>("ObjectMap")
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

            int w = heightMap.GetLength(0);
            int h = heightMap.GetLength(1);

            var existing = inputs[1] as string[,];
            var result = existing != null ? (string[,])existing.Clone() : new string[w, h];

            if (_layers == null || _layers.Length == 0)
                return NodeOutput.Success(result);

            var rng = new System.Random(_seed);

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (_avoidExistingObjects && !string.IsNullOrEmpty(result[x, y]))
                        continue;

                    float height = heightMap[x, y];

                    foreach (var layer in _layers)
                    {
                        if (height < layer.MinHeight || height > layer.MaxHeight)
                            continue;

                        // Density scales with how deep we are in the range
                        float t = Mathf.InverseLerp(layer.MinHeight, layer.MaxHeight, height);
                        float effectiveDensity = layer.Density * Mathf.Lerp(0.5f, 1f, t);

                        if (rng.NextDouble() < effectiveDensity)
                        {
                            result[x, y] = layer.ObjectId;
                            break; // First matching layer wins
                        }
                    }
                }
            }

            return NodeOutput.Success(result);
        }
    }
}
