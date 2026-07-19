using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo(
        "Normalize Map",
        "Height",
        "Лінійно нормалізує всі скінченні значення float-карти до діапазону 0..1 без зміни її розміру.",
        StableId = "moyva.height.normalize-map",
        Order = 10,
        PreviewOutput = "out.map")]
    public sealed class NormalizeMapNode : NodeBase
    {
        public override string Title => "Normalize Map";
        public override string Category => "Height";
        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("Map", "in.map")
        };
        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("Map", "out.map")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var source = inputs != null && inputs.Length > 0 ? inputs[0] as float[,] : null;
            if (!MapNodeUtility.TryValidate(
                    source,
                    context,
                    "Map",
                    out int width,
                    out int height,
                    out string error))
                return NodeOutput.Error(error);

            context?.CountIteration(width * height);
            return NodeOutput.Success(MapNodeUtility.Normalize(source));
        }
    }
}
