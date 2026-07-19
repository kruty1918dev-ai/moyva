using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo(
        "Slope Mask",
        "Masks",
        "Обчислює локальний нахил height-карти у градусах і вибирає клітини в заданому діапазоні.",
        StableId = "moyva.masks.slope",
        Order = 60,
        PreviewOutput = "out.mask")]
    public sealed class SlopeMaskNode : NodeBase
    {
        [SerializeField, Range(0f, 90f)]
        [Tooltip("Мінімальний кут нахилу у градусах.")]
        private float _minimumSlope;

        [SerializeField, Range(0f, 90f)]
        [Tooltip("Максимальний кут нахилу у градусах.")]
        private float _maximumSlope = 35f;

        [SerializeField, Min(0.0001f)]
        [Tooltip("Горизонтальний розмір однієї клітини для обчислення кута.")]
        private float _cellSize = 1f;

        public override string Title => "Slope Mask";
        public override string Category => "Masks";
        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("Height Map", "in.height_map")
        };
        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Mask", "out.mask"),
            PortDefinition.Output<float[,]>("Slope", "out.slope")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var source = inputs != null && inputs.Length > 0 ? inputs[0] as float[,] : null;
            if (!MapNodeUtility.TryValidate(
                    source,
                    context,
                    "Height Map",
                    out int width,
                    out int height,
                    out string error))
                return NodeOutput.Error(error);

            float minimum = Mathf.Min(_minimumSlope, _maximumSlope);
            float maximum = Mathf.Max(_minimumSlope, _maximumSlope);
            float cellSize = Mathf.Max(0.0001f, _cellSize);
            var slopes = new float[width, height];
            var mask = new bool[width, height];

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                int left = Mathf.Max(0, x - 1);
                int right = Mathf.Min(width - 1, x + 1);
                int down = Mathf.Max(0, y - 1);
                int up = Mathf.Min(height - 1, y + 1);
                float dx = (FiniteOrZero(source[right, y]) - FiniteOrZero(source[left, y]))
                           / Mathf.Max(cellSize, (right - left) * cellSize);
                float dy = (FiniteOrZero(source[x, up]) - FiniteOrZero(source[x, down]))
                           / Mathf.Max(cellSize, (up - down) * cellSize);
                float slope = Mathf.Atan(Mathf.Sqrt(dx * dx + dy * dy)) * Mathf.Rad2Deg;
                slopes[x, y] = slope;
                mask[x, y] = slope >= minimum && slope <= maximum;
            }

            context?.CountIteration(width * height);
            return NodeOutput.Success(mask, slopes);
        }

        private static float FiniteOrZero(float value) =>
            MapNodeUtility.IsFinite(value) ? value : 0f;
    }
}
