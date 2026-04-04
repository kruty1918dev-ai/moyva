using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    [NodeInfo("Terrace", "Processing")]
    public sealed class TerraceNode : NodeBase
    {
        [Header("Terrace Settings")]
        [SerializeField, Range(2, 20)] private int _levels = 5;
        [SerializeField, Range(0f, 1f)] private float _sharpness = 0.8f;

        public override string Title => "Terrace";
        public override string Category => "Processing";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("TerracedMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var source = inputs[0] as float[,];
            if (source == null)
                return NodeOutput.Error("HeightMap input is required.");

            int w = source.GetLength(0);
            int h = source.GetLength(1);
            var result = new float[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    float v = source[x, y];
                    float stepped = Mathf.Round(v * _levels) / _levels;
                    result[x, y] = Mathf.Lerp(v, stepped, _sharpness);
                }
            }

            return NodeOutput.Success(result);
        }
    }
}
