using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement
{
    [NodeInfo(
        "Edge Mask",
        "Masks",
        "Створює м’яку зважену смугу біля краю острова або маски рельєфу.",
        StableId = "moyva.masks.edge",
        Order = 10,
        PreviewOutput = "out.scatter_mask")]
    public sealed class EdgeMaskNode : NodeBase
    {
        [SerializeField, Min(0)]
        [InlineEditable("Відстань")]
        [Tooltip("Повна інтенсивність на відстані від краю маски, у клітинах.")]
        private int _distanceFromEdge = 2;

        [SerializeField, Min(0)]
        [InlineEditable("Спад")]
        [Tooltip("Додаткова м’яка відстань спаду, у клітинах.")]
        private int _falloff = 2;

        [SerializeField]
        [Tooltip("Виводить інтерір біля краю замість смуги краю.")]
        private bool _invert;

        public override string Title => "Edge Mask";
        public override string Category => "Masks";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<bool[,]>("Source", "in.source")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Mask", "out.mask"),
            PortDefinition.Output<ScatterMask>("Scatter Mask", "out.scatter_mask")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (inputs == null || inputs.Length == 0 || inputs[0] is not bool[,] source)
                return NodeOutput.Error("Маска джерела є обов'язковою.");

            int w = source.GetLength(0);
            int h = source.GetLength(1);
            var scatterMask = new ScatterMask(source);
            int[] distances = ObjectPlacementScatterUtility.BuildEdgeDistanceMap(scatterMask, out _);
            var mask = new bool[w, h];
            var weights = new float[w, h];
            int fullDistance = Mathf.Max(0, _distanceFromEdge);
            int falloff = Mathf.Max(0, _falloff);

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    if (!source[x, y])
                        continue;

                    int distance = distances[y * w + x];
                    float weight;
                    if (distance <= fullDistance)
                    {
                        weight = 1f;
                    }
                    else if (falloff > 0 && distance <= fullDistance + falloff)
                    {
                        weight = 1f - ((distance - fullDistance) / (float)falloff);
                    }
                    else
                    {
                        weight = 0f;
                    }

                    if (_invert)
                        weight = 1f - weight;

                    weights[x, y] = Mathf.Clamp01(weight);
                    mask[x, y] = weights[x, y] > 0.001f;
                }
            }

            return NodeOutput.Success(mask, new ScatterMask(mask, null, weights));
        }
    }
}
