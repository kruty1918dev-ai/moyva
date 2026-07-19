using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public enum DistanceFieldTarget
    {
        ActiveCells,
        InactiveCells
    }

    [NodeInfo(
        "Distance Field",
        "Modifiers",
        "Обчислює евклідову відстань кожної клітини до найближчої активної або неактивної клітини маски.",
        StableId = "moyva.modifiers.distance-field",
        Order = 10,
        PreviewOutput = "out.distance")]
    public sealed class DistanceFieldNode : NodeBase
    {
        [SerializeField]
        [Tooltip("До якого значення маски вимірювати відстань.")]
        private DistanceFieldTarget _target = DistanceFieldTarget.InactiveCells;

        [SerializeField]
        [Tooltip("Нормалізувати відстань до діапазону 0..1 за діагоналлю карти.")]
        private bool _normalize = true;

        public override string Title => "Distance Field";
        public override string Category => "Modifiers";
        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<bool[,]>("Mask", "in.mask")
        };
        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("Distance", "out.distance")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var source = inputs != null && inputs.Length > 0 ? inputs[0] as bool[,] : null;
            if (!MapNodeUtility.TryValidate(
                    source,
                    context,
                    "Mask",
                    out int width,
                    out int height,
                    out string error))
                return NodeOutput.Error(error);

            var result = MapNodeUtility.DistanceTo(
                source,
                _target == DistanceFieldTarget.ActiveCells);
            if (_normalize)
            {
                float diagonal = Mathf.Max(1f, Mathf.Sqrt(width * width + height * height));
                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    result[x, y] = Mathf.Clamp01(result[x, y] / diagonal);
            }

            context?.CountIteration(width * height);
            return NodeOutput.Success(result);
        }
    }
}
