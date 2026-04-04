using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    [NodeInfo("River Generator", "Features")]
    public sealed class RiverNode : NodeBase
    {
        [Header("River Settings")]
        [SerializeField] private RiverDataConfig _riverConfig;

        public override string Title => "River Generator";
        public override string Category => "Features";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("BiomeMap"),
            PortDefinition.Input<float[,]>("HeightMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("BiomeMap"),
            PortDefinition.Output<string[,]>("ObjectMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var biomeMap = inputs[0] as string[,];
            var heightMap = inputs[1] as float[,];
            if (biomeMap == null || heightMap == null)
                return NodeOutput.Error("BiomeMap and HeightMap inputs are required.");
            if (_riverConfig == null)
                return NodeOutput.Error("RiverDataConfig not assigned.");

            var result = (string[,])biomeMap.Clone();
            int width = result.GetLength(0);
            int height = result.GetLength(1);
            var objectMap = new string[width, height];

            var pathfinder = context.GetService<IRiverPathfinder>();
            var riverGen = new RiverFeatureGenerator(_riverConfig, pathfinder);
            riverGen.ApplyFeatures(result, objectMap, heightMap, width, height);

            return NodeOutput.Success(result, objectMap);
        }
    }
}
