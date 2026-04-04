using System;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Nodes
{
    [NodeInfo("Output", "Core")]
    public sealed class OutputNode : NodeBase
    {
        public override string Title => "Output";
        public override string Category => "Core";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("BiomeMap"),
            PortDefinition.Input<string[,]>("ObjectMap"),
            PortDefinition.Input<float[,]>("HeightMap")
        };

        public override PortDefinition[] Outputs => Array.Empty<PortDefinition>();

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var biomeMap = inputs[0] as string[,];
            if (biomeMap == null)
                return NodeOutput.Error("BiomeMap input is required.");

            var objectMap = inputs[1] as string[,]
                ?? new string[biomeMap.GetLength(0), biomeMap.GetLength(1)];
            var heightMap = inputs[2] as float[,]
                ?? new float[biomeMap.GetLength(0), biomeMap.GetLength(1)];

            return NodeOutput.Success(biomeMap, objectMap, heightMap);
        }
    }
}
