using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public enum ThresholdComparison
    {
        GreaterOrEqual,
        LessOrEqual
    }

    [NodeInfo(
        "Threshold Mask",
        "Masks",
        "Перетворює float-карту на булеву маску порівнянням кожної клітини з порогом.",
        StableId = "moyva.masks.threshold",
        Order = 40,
        PreviewOutput = "out.mask")]
    public sealed class ThresholdMaskNode : NodeBase
    {
        [SerializeField]
        [Tooltip("Порогове значення.")]
        private float _threshold = 0.5f;

        [SerializeField]
        [Tooltip("Напрямок порівняння з порогом.")]
        private ThresholdComparison _comparison = ThresholdComparison.GreaterOrEqual;

        public override string Title => "Threshold Mask";
        public override string Category => "Masks";
        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("Map", "in.map")
        };
        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Mask", "out.mask")
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

            var result = new bool[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float value = source[x, y];
                result[x, y] = MapNodeUtility.IsFinite(value)
                               && (_comparison == ThresholdComparison.GreaterOrEqual
                                   ? value >= _threshold
                                   : value <= _threshold);
            }

            context?.CountIteration(width * height);
            return NodeOutput.Success(result);
        }
    }
}
