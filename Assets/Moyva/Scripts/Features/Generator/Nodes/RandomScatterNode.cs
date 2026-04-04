using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    [NodeInfo("Random Scatter", "Features")]
    public sealed class RandomScatterNode : NodeBase
    {
        [Header("Scatter Settings")]
        [SerializeField, Range(0f, 1f)] private float _density = 0.1f;
        [SerializeField] private int _seed;
        [SerializeField] private string _objectId = "tree";

        public override string Title => "Random Scatter";
        public override string Category => "Features";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<bool[,]>("Mask"),
            PortDefinition.Input<int>("MapWidth"),
            PortDefinition.Input<int>("MapHeight")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("ObjectMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var mask = inputs[0] as bool[,];
            int width = 0, height = 0;

            if (mask != null)
            {
                width = mask.GetLength(0);
                height = mask.GetLength(1);
            }
            else
            {
                if (inputs[1] is int w) width = w;
                if (inputs[2] is int h) height = h;
            }

            if (width <= 0 || height <= 0)
                return NodeOutput.Error("Cannot determine map size. Provide Mask or MapWidth+MapHeight.");

            var rng = new System.Random(_seed);
            var result = new string[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (mask != null && !mask[x, y]) continue;

                    if (rng.NextDouble() < _density)
                        result[x, y] = _objectId;
                }
            }

            return NodeOutput.Success(result);
        }
    }
}
