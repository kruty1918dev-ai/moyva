using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    [NodeInfo("Border Frame", "Utility")]
    public sealed class BorderFrameNode : NodeBase
    {
        [Header("Border Settings")]
        [SerializeField, Range(1, 20)] private int _thickness = 2;

        public override string Title => "Border Frame";
        public override string Category => "Utility";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<bool[,]>("Source")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Mask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var source = inputs[0] as bool[,];
            if (source == null)
                return NodeOutput.Error("Source input is required.");

            int w = source.GetLength(0);
            int h = source.GetLength(1);
            var result = new bool[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    bool isBorder = x < _thickness || x >= w - _thickness
                                 || y < _thickness || y >= h - _thickness;
                    result[x, y] = isBorder || source[x, y];
                }
            }

            return NodeOutput.Success(result);
        }
    }
}
