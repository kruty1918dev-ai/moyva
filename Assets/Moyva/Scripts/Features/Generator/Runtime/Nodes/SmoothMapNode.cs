using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo(
        "Smooth Map",
        "Height",
        "Згладжує float-карту детермінованим box-фільтром із заданим радіусом і кількістю проходів.",
        StableId = "moyva.height.smooth-map",
        Order = 30,
        PreviewOutput = "out.map")]
    public sealed class SmoothMapNode : NodeBase
    {
        [SerializeField, Range(0, 8)]
        [Tooltip("Радіус сусідства у клітинах.")]
        private int _radius = 1;

        [SerializeField, Range(1, 8)]
        [Tooltip("Кількість послідовних проходів згладжування.")]
        private int _iterations = 1;

        public override string Title => "Smooth Map";
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

            context?.CountIteration(width * height * Mathf.Max(1, _iterations));
            return NodeOutput.Success(MapNodeUtility.Smooth(source, _radius, _iterations));
        }
    }
}
