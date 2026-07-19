using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo(
        "Terrace Map",
        "Height",
        "Квантує float-карту в задану кількість терас зі збереженням її фактичного діапазону.",
        StableId = "moyva.height.terrace-map",
        Order = 40,
        PreviewOutput = "out.map")]
    public sealed class TerraceMapNode : NodeBase
    {
        [SerializeField, Range(2, 64)]
        [Tooltip("Кількість дискретних рівнів терас.")]
        private int _steps = 8;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("0 залишає початкову карту, 1 застосовує повну терасизацію.")]
        private float _strength = 1f;

        public override string Title => "Terrace Map";
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

            float minimum = float.PositiveInfinity;
            float maximum = float.NegativeInfinity;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float value = source[x, y];
                if (!MapNodeUtility.IsFinite(value))
                    continue;
                minimum = Mathf.Min(minimum, value);
                maximum = Mathf.Max(maximum, value);
            }

            var result = new float[width, height];
            float range = maximum - minimum;
            if (!MapNodeUtility.IsFinite(minimum)
                || !MapNodeUtility.IsFinite(maximum))
                return NodeOutput.Success(result);
            if (Mathf.Approximately(range, 0f))
            {
                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    result[x, y] = MapNodeUtility.IsFinite(source[x, y])
                        ? source[x, y]
                        : minimum;
                context?.CountIteration(width * height);
                return NodeOutput.Success(result);
            }

            int intervals = Mathf.Max(1, _steps - 1);
            float strength = Mathf.Clamp01(_strength);
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float value = MapNodeUtility.IsFinite(source[x, y]) ? source[x, y] : minimum;
                float normalized = Mathf.Clamp01((value - minimum) / range);
                float terraced = Mathf.Round(normalized * intervals) / intervals;
                float target = minimum + terraced * range;
                result[x, y] = Mathf.Lerp(value, target, strength);
            }

            context?.CountIteration(width * height);
            return NodeOutput.Success(result);
        }
    }
}
