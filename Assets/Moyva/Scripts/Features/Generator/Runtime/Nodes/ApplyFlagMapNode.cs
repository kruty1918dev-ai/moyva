using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Apply FlagMap", "Flags", "Накладає FlagMap на TileMap: усі непорожні flag ID стають тимчасовими tile ids, які може читати WFC як окремі тайли.")]
    public sealed class ApplyFlagMapNode : NodeBase
    {
        public override string Title => "Apply FlagMap";
        public override string Category => "Flags";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("TileMap (optional)"),
            PortDefinition.Input<string[,]>("FlagMap")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("TileMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var tileMap = inputs.Length > 0 ? inputs[0] as string[,] : null;
            var flagMap = inputs.Length > 1 ? inputs[1] as string[,] : null;

            if (tileMap == null && flagMap == null)
                return NodeOutput.Error("TileMap or FlagMap is required.");
            if (flagMap == null)
                return NodeOutput.Success(tileMap);
            if (tileMap == null)
                return NodeOutput.Success((string[,])flagMap.Clone());

            int w = tileMap.GetLength(0);
            int h = tileMap.GetLength(1);
            if (flagMap.GetLength(0) != w || flagMap.GetLength(1) != h)
                return NodeOutput.Error("TileMap and FlagMap must have the same size.");

            var result = (string[,])tileMap.Clone();
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!string.IsNullOrEmpty(flagMap[x, y]))
                        result[x, y] = flagMap[x, y];
                }
            }

            return NodeOutput.Success(result);
        }
    }
}