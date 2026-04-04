using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Nodes
{
    [NodeInfo("Overlay", "Utility")]
    public sealed class OverlayNode : NodeBase
    {
        public override string Title => "Overlay";
        public override string Category => "Utility";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("Base"),
            PortDefinition.Input<string[,]>("Overlay"),
            PortDefinition.Input<bool[,]>("Mask")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("Result")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var baseMap = inputs[0] as string[,];
            var overlay = inputs[1] as string[,];
            if (baseMap == null)
                return NodeOutput.Error("Base input is required.");

            int width = baseMap.GetLength(0);
            int height = baseMap.GetLength(1);
            var result = (string[,])baseMap.Clone();

            if (overlay == null)
                return NodeOutput.Success(result);

            var mask = inputs[2] as bool[,];

            int ow = Mathf.Min(width, overlay.GetLength(0));
            int oh = Mathf.Min(height, overlay.GetLength(1));

            for (int x = 0; x < ow; x++)
            {
                for (int y = 0; y < oh; y++)
                {
                    if (string.IsNullOrEmpty(overlay[x, y])) continue;
                    if (mask != null && !mask[x, y]) continue;
                    result[x, y] = overlay[x, y];
                }
            }

            return NodeOutput.Success(result);
        }
    }
}
