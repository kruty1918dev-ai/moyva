using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Bool Invert", "Math", "Інвертує булеву маску: true стає false, false стає true. Корисно для швидкого перетворення забороненої зони у дозволену і навпаки.")]
    public sealed class BoolInvertNode : NodeBase
    {
        public override string Title => "Bool Invert";
        public override string Category => "Math";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<bool[,]>("Source")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Mask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var source = inputs[0] as bool[,];
            if (source == null)
                return BoolMaskMathUtility.MissingSourceError();

            int w = source.GetLength(0);
            int h = source.GetLength(1);
            var result = new bool[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                    result[x, y] = !source[x, y];
            }

            return NodeOutput.Success(result);
        }
    }
}
