using System;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement
{
    [NodeInfo("Placement Mask", "Object Placement", "Combines a placement mask and an optional exclude mask into an object scatter mask.")]
    public sealed class PlacementMaskNode : NodeBase, IPreviewableNode
    {
        [NonSerialized] private ScatterMask _lastMask;

        public override string Title => "Placement Mask";
        public override string Category => "Object Placement";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<bool[,]>("Placement"),
            PortDefinition.Input<bool[,]>("Exclude")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<ScatterMask>("Scatter Mask"),
            PortDefinition.Output<bool[,]>("Allowed Mask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var placement = inputs.Length > 0 ? inputs[0] as bool[,] : null;
            var exclude = inputs.Length > 1 ? inputs[1] as bool[,] : null;

            if (placement == null && exclude == null)
            {
                int w = Mathf.Max(1, context?.MapSize.x ?? 0);
                int h = Mathf.Max(1, context?.MapSize.y ?? 0);
                placement = new bool[w, h];
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                        placement[x, y] = true;
                }
            }

            if (placement == null && exclude != null)
            {
                int w = exclude.GetLength(0);
                int h = exclude.GetLength(1);
                placement = new bool[w, h];
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                        placement[x, y] = true;
                }
            }

            if (exclude != null
                && placement != null
                && (placement.GetLength(0) != exclude.GetLength(0)
                    || placement.GetLength(1) != exclude.GetLength(1)))
            {
                return NodeOutput.Error("Placement and Exclude masks must have the same size.");
            }

            _lastMask = new ScatterMask(placement, exclude);
            return NodeOutput.Success(_lastMask, _lastMask.BuildAllowedMask());
        }

        public Texture2D GeneratePreview(int width, int height)
        {
            return _lastMask == null
                ? null
                : ObjectPlacementPreviewUtility.BuildMaskTexture(
                    _lastMask.BuildAllowedMask(),
                    new Color(0.45f, 0.85f, 0.35f, 1f),
                    new Color(0.06f, 0.07f, 0.09f, 1f));
        }
    }
}
