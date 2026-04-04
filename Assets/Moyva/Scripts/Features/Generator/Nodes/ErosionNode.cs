using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    [NodeInfo("Erosion", "Processing")]
    public sealed class ErosionNode : NodeBase
    {
        [Header("Erosion Settings")]
        [SerializeField, Range(0.001f, 0.1f)] private float _erosionRate = 0.01f;
        [SerializeField, Range(1, 50)] private int _iterations = 10;
        [SerializeField, Range(0f, 1f)] private float _talusAngle = 0.05f;

        public override string Title => "Erosion";
        public override string Category => "Processing";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("ErodedMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var source = inputs[0] as float[,];
            if (source == null)
                return NodeOutput.Error("No input HeightMap provided.");

            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var map = (float[,])source.Clone();

            // Thermal erosion
            for (int iter = 0; iter < _iterations; iter++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    for (int y = 1; y < height - 1; y++)
                    {
                        float center = map[x, y];
                        float maxDiff = 0f;
                        int lx = x, ly = y;

                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                if (dx == 0 && dy == 0) continue;
                                float diff = center - map[x + dx, y + dy];
                                if (diff > maxDiff)
                                {
                                    maxDiff = diff;
                                    lx = x + dx;
                                    ly = y + dy;
                                }
                            }
                        }

                        if (maxDiff > _talusAngle)
                        {
                            float transfer = _erosionRate * (maxDiff - _talusAngle);
                            map[x, y] -= transfer;
                            map[lx, ly] += transfer;
                        }
                    }
                }
            }

            return NodeOutput.Success(map);
        }
    }
}
