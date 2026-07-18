using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Generator
{
    public sealed class GraphPreviewFullMapMaskNode : NodeBase
    {
        public override string Title => "Full Map Mask";
        public override string Category => "Tests";
        public override PortDefinition[] Inputs => System.Array.Empty<PortDefinition>();
        public override PortDefinition[] Outputs =>
            new[] { PortDefinition.Output<bool[,]>("Mask") };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            int width = Mathf.Max(1, context?.MapSize.x ?? 0);
            int height = Mathf.Max(1, context?.MapSize.y ?? 0);
            var mask = new bool[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                mask[x, y] = true;

            return NodeOutput.Success(mask);
        }
    }
}
