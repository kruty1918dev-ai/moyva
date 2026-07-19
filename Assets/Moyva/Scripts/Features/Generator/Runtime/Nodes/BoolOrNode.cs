using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo(
        "Bool Or",
        "Math",
        "Логічне об'єднання двох масок. Результат true там, де true хоча б одна з масок A або B.",
        StableId = "moyva.math.bool-or",
        Order = 30,
        PreviewOutput = "out.mask")]
    public sealed class BoolOrNode : NodeBase
    {
        public override string Title => "Bool Or";
        public override string Category => "Math";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<bool[,]>("A", "in.a"),
            PortDefinition.Input<bool[,]>("B", "in.b")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Mask", "out.mask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var a = inputs[0] as bool[,];
            var b = inputs[1] as bool[,];
            if (!BoolMaskMathUtility.ValidatePair(a, b, out int w, out int h, out var error))
                return NodeOutput.Error(error);

            var result = new bool[w, h];
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                    result[x, y] = a[x, y] || b[x, y];
            }

            return NodeOutput.Success(result);
        }
    }
}
