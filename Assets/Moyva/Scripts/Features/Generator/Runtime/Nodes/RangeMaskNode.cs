using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo(
        "Range Mask",
        "Masks",
        "Вибирає клітини float-карти, значення яких входять до заданого включного діапазону.",
        StableId = "moyva.masks.range",
        Order = 50,
        PreviewOutput = "out.mask")]
    public sealed class RangeMaskNode : NodeBase
    {
        [SerializeField]
        [Tooltip("Нижня межа включного діапазону.")]
        private float _minimum;

        [SerializeField]
        [Tooltip("Верхня межа включного діапазону.")]
        private float _maximum = 1f;

        [SerializeField]
        [Tooltip("Інвертувати результат вибору.")]
        private bool _invert;

        public override string Title => "Range Mask";
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

            float minimum = Mathf.Min(_minimum, _maximum);
            float maximum = Mathf.Max(_minimum, _maximum);
            var result = new bool[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float value = source[x, y];
                bool inside = MapNodeUtility.IsFinite(value)
                              && value >= minimum
                              && value <= maximum;
                result[x, y] = _invert ? !inside : inside;
            }

            context?.CountIteration(width * height);
            return NodeOutput.Success(result);
        }
    }
}
