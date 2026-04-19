using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Subgraph Input", "Macros", "Точка входу для підграфа. Повертає мапи, передані з батьківського графа через Subgraph Node.")]
    public sealed class SubgraphInputNode : NodeBase
    {
        public override string Title => "Subgraph Input";
        public override string Category => "Macros";

        public override PortDefinition[] Inputs => System.Array.Empty<PortDefinition>();
        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("BiomeMap"),
            PortDefinition.Output<string[,]>("ObjectMap"),
            PortDefinition.Output<float[,]>("HeightMap"),
            PortDefinition.Output<string[,]>("BuildingMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (!context.TryGetService(out SubgraphInputData data) || data == null)
                return NodeOutput.Warning("SubgraphInputData service not found.", null, null, null, null);

            return NodeOutput.Success(data.BiomeMap, data.ObjectMap, data.HeightMap, data.BuildingMap);
        }
    }
}
