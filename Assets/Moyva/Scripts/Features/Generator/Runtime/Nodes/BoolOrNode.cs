using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Bool Or", "Математика", "Логічне об'єднання двох масок. Результат true там, де true хоча б одна з масок A або B.")]
    public sealed class BoolOrNode : NodeBase
    {
        public override string Title => "Bool Or";
        public override string Category => "Математика";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<bool[,]>("A"),
            PortDefinition.Input<bool[,]>("B")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Маска")
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
