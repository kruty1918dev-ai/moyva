using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    [NodeInfo("Biome Resolver", "Generators")]
    public sealed class BiomeResolverNode : NodeBase
    {
        [Header("Biome Settings")]
        [SerializeField] private DataBiomesSettings _biomesSettings;

        public override string Title => "Biome Resolver";
        public override string Category => "Generators";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap"),
            PortDefinition.Input<string[,]>("BaseTileMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("BiomeMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs[0] as float[,];
            if (heightMap == null)
                return NodeOutput.Error("No input HeightMap provided.");
            if (_biomesSettings == null)
                return NodeOutput.Error("DataBiomesSettings not assigned.");

            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            var baseTileMap = inputs[1] as string[,] ?? new string[width, height];

            var resolver = context.GetService<IBiomeResolver>();
            string[,] result = null;
            resolver.ResolveBiomes(heightMap, (string[,])baseTileMap.Clone(),
                r => result = r);

            return result != null
                ? NodeOutput.Success(result)
                : NodeOutput.Error("BiomeResolver returned null.");
        }
    }
}
