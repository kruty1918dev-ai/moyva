using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo(
        "Subgraph Input",
        "Subgraphs",
        "Точка входу підграфа. Повертає карти, передані з батьківського вузла Subgraph.",
        StableId = "moyva.subgraphs.input",
        Order = 10,
        PreviewOutput = "out.biome_map")]
    public sealed class SubgraphInputNode : NodeBase
    {
        public override string Title => "Subgraph Input";
        public override string Category => "Subgraphs";

        public override PortDefinition[] Inputs => System.Array.Empty<PortDefinition>();
        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("Biome Map", "out.biome_map"),
            PortDefinition.Output<string[,]>("Object Map", "out.object_map"),
            PortDefinition.Output<float[,]>("Height Map", "out.height_map"),
            PortDefinition.Output<string[,]>("Building Map", "out.building_map")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (context == null
                || !context.TryGetService(out SubgraphInputData data)
                || data == null)
            {
                ResolveSize(context, out int width, out int height);
                return NodeOutput.Warning(
                    "SubgraphInputData service not found. Empty exact-size maps were used.",
                    new string[width, height],
                    new string[width, height],
                    new float[width, height],
                    new string[width, height]);
            }

            ResolveSize(context, out int w, out int h);
            return NodeOutput.Success(
                data.BiomeMap ?? new string[w, h],
                data.ObjectMap ?? new string[w, h],
                data.HeightMap ?? new float[w, h],
                data.BuildingMap ?? new string[w, h]);
        }

        private static void ResolveSize(
            NodeContext context,
            out int width,
            out int height)
        {
            width = System.Math.Max(1, context?.MapSize.x ?? 0);
            height = System.Math.Max(1, context?.MapSize.y ?? 0);
        }
    }
}
