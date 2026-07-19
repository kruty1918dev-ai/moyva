using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo(
        "Remap Map",
        "Height",
        "Лінійно переносить значення float-карти з одного числового діапазону в інший.",
        StableId = "moyva.height.remap-map",
        Order = 20,
        PreviewOutput = "out.map")]
    public sealed class RemapMapNode : NodeBase
    {
        [SerializeField]
        [Tooltip("Нижня межа вхідного діапазону.")]
        private float _inputMinimum;

        [SerializeField]
        [Tooltip("Верхня межа вхідного діапазону.")]
        private float _inputMaximum = 1f;

        [SerializeField]
        [Tooltip("Нижня межа вихідного діапазону.")]
        private float _outputMinimum;

        [SerializeField]
        [Tooltip("Верхня межа вихідного діапазону.")]
        private float _outputMaximum = 1f;

        [SerializeField]
        [Tooltip("Обмежувати нормалізоване значення діапазоном 0..1.")]
        private bool _clamp = true;

        public override string Title => "Remap Map";
        public override string Category => "Height";
        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<float[,]>("Map", "in.map")
        };
        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float[,]>("Map", "out.map")
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

            float inputRange = _inputMaximum - _inputMinimum;
            if (Mathf.Approximately(inputRange, 0f))
                return NodeOutput.Error("Вхідний діапазон Remap Map не може мати нульову довжину.");

            var result = new float[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float value = source[x, y];
                if (!MapNodeUtility.IsFinite(value))
                {
                    result[x, y] = _outputMinimum;
                    continue;
                }

                float t = (value - _inputMinimum) / inputRange;
                if (_clamp)
                    t = Mathf.Clamp01(t);
                result[x, y] = Mathf.LerpUnclamped(_outputMinimum, _outputMaximum, t);
            }

            context?.CountIteration(width * height);
            return NodeOutput.Success(result);
        }
    }
}
