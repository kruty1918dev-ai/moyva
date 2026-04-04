using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    [NodeInfo("Height To Tile", "Converters")]
    public sealed class HeightToTileNode : NodeBase
    {
        [Header("Height Map Settings")]
        [SerializeField] private HeightMapSettings _heightMapSettings;

        public override string Title => "Height To Tile";
        public override string Category => "Converters";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("HeightMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("TileMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var heightMap = inputs[0] as float[,];
            if (heightMap == null)
                return NodeOutput.Error("No input HeightMap provided.");
            if (_heightMapSettings == null)
                return NodeOutput.Error("HeightMapSettings not assigned.");

            var generator = context.GetService<IVirtualHeightMapGenerator>();
            string[,] result = null;
            generator.GenerateVirtualHeightMap(heightMap, r => result = r);

            return result != null
                ? NodeOutput.Success(result)
                : NodeOutput.Error("VirtualHeightMapGenerator returned null.");
        }
    }
}
