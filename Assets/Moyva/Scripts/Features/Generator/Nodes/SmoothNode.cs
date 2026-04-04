using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    [NodeInfo("Smooth", "Processing")]
    public sealed class SmoothNode : NodeBase
    {
        [Header("Smooth Settings")]
        [SerializeField, Range(1, 5)] private int _radius = 1;
        [SerializeField, Range(1, 10)] private int _iterations = 1;

        public override string Title => "Smooth";
        public override string Category => "Processing";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("SmoothedMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var source = inputs[0] as float[,];
            if (source == null)
                return NodeOutput.Error("No input HeightMap provided.");

            int width = source.GetLength(0);
            int height = source.GetLength(1);
            var current = (float[,])source.Clone();

            for (int iter = 0; iter < _iterations; iter++)
            {
                var next = new float[width, height];

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        float sum = 0;
                        int count = 0;

                        for (int dx = -_radius; dx <= _radius; dx++)
                        {
                            for (int dy = -_radius; dy <= _radius; dy++)
                            {
                                int nx = x + dx;
                                int ny = y + dy;
                                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                                {
                                    sum += current[nx, ny];
                                    count++;
                                }
                            }
                        }

                        next[x, y] = sum / count;
                    }
                }

                current = next;
            }

            return NodeOutput.Success(current);
        }
    }
}
