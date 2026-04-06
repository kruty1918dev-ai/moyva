using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Merge ObjectMaps", "Utils", "Об'єднує дві карти об'єктів в одну. Значення з другої карти перекривають першу, якщо в клітинці є непорожній об'єкт, тому нода корисна для пошарового складання декількох систем спавну.")]
    public sealed class MergeObjectMapNode : NodeBase
    {
        public override string Title => "Merge ObjectMaps";
        public override string Category => "Utility";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("ObjectMapA"),
            PortDefinition.Input<string[,]>("ObjectMapB")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("ObjectMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var mapA = inputs[0] as string[,];
            var mapB = inputs[1] as string[,];

            if (mapA == null && mapB == null)
                return NodeOutput.Error("At least one ObjectMap input is required.");

            if (mapA == null) return NodeOutput.Success(mapB);
            if (mapB == null) return NodeOutput.Success(mapA);

            int w = mapA.GetLength(0);
            int h = mapA.GetLength(1);
            var result = (string[,])mapA.Clone();

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!string.IsNullOrEmpty(mapB[x, y]))
                        result[x, y] = mapB[x, y];
                }
            }

            return NodeOutput.Success(result);
        }
    }
}
