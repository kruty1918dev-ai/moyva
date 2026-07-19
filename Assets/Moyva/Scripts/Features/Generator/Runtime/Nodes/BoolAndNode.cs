using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo(
        "Bool And",
        "Math",
        "Логічний перетин двох масок. Результат true лише там, де A і B одночасно true.",
        StableId = "moyva.math.bool-and",
        Order = 20,
        PreviewOutput = "out.mask")]
    public sealed class BoolAndNode : NodeBase
    {
        public override string Title => "Bool And";
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
                    result[x, y] = a[x, y] && b[x, y];
            }

            return NodeOutput.Success(result);
        }
    }
}
