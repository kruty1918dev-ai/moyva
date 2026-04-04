using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    [NodeInfo("Water Smooth", "Features")]
    public sealed class WaterSmoothNode : NodeBase
    {
        public override string Title => "Water Smooth";
        public override string Category => "Features";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("BiomeMap"),
            PortDefinition.Input<string[,]>("ObjectMap"),
            PortDefinition.Input<float[,]>("HeightMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("BiomeMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var biomeMap = inputs[0] as string[,];
            if (biomeMap == null)
                return NodeOutput.Error("BiomeMap input is required.");

            var objectMap = inputs[1] as string[,]
                ?? new string[biomeMap.GetLength(0), biomeMap.GetLength(1)];
            var heightMap = inputs[2] as float[,]
                ?? new float[biomeMap.GetLength(0), biomeMap.GetLength(1)];

            var result = (string[,])biomeMap.Clone();
            int w = result.GetLength(0);
            int h = result.GetLength(1);

            var processor = new WaterPostProcessor();
            processor.ApplyFeatures(result, objectMap, heightMap, w, h);

            return NodeOutput.Success(result);
        }
    }
}
