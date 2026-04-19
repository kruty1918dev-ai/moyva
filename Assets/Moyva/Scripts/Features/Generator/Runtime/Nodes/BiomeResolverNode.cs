using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Biome Resolver", "Generators", "Перетворює базову карту висот у біомну карту за правилами висоти та вологості. Це основна нода, яка вирішує, де буде трава, ліс, болото, суха земля чи інші типи поверхні.")]
    public sealed class BiomeResolverNode : NodeBase
    {
        [Header("Biome Settings")]
        [Tooltip("ScriptableObject із повним набором правил біомів. Саме тут зберігаються діапазони висоти, вологості та fallback-тайл для резолву клітинок.")]
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

            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            var baseTileMap = inputs[1] as string[,] ?? new string[width, height];

            // Keep preview generation alive in editor even if biome config is not assigned yet.
            if (_biomesSettings == null)
                return NodeOutput.Warning("DataBiomesSettings not assigned. Using BaseTileMap as fallback.",
                    (string[,])baseTileMap.Clone());

            var resolver = context.GetService<IBiomeResolver>();
            if (resolver == null)
                return NodeOutput.Warning("IBiomeResolver service is not registered. Using BaseTileMap as fallback.",
                    (string[,])baseTileMap.Clone());

            string[,] result = null;
            resolver.ResolveBiomes(heightMap, (string[,])baseTileMap.Clone(),
                r => result = r);

            return result != null
                ? NodeOutput.Success(result)
                : NodeOutput.Warning("BiomeResolver returned null. Using BaseTileMap as fallback.",
                    (string[,])baseTileMap.Clone());
        }
    }
}
