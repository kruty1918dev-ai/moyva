using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    public enum MaskMorphologyOperation
    {
        Dilate,
        Erode,
        Open,
        Close
    }

    [NodeInfo(
        "Mask Morphology",
        "Modifiers",
        "Розширює, звужує, відкриває або закриває булеву маску круглим ядром заданого радіуса.",
        StableId = "moyva.modifiers.mask-morphology",
        Order = 20,
        PreviewOutput = "out.mask")]
    public sealed class MaskMorphologyNode : NodeBase
    {
        [SerializeField]
        [Tooltip("Морфологічна операція над маскою.")]
        private MaskMorphologyOperation _operation = MaskMorphologyOperation.Dilate;

        [SerializeField, Range(0, 8)]
        [Tooltip("Радіус круглого ядра у клітинах.")]
        private int _radius = 1;

        [SerializeField, Range(1, 8)]
        [Tooltip("Кількість повторень обраної операції.")]
        private int _iterations = 1;

        public override string Title => "Mask Morphology";
        public override string Category => "Modifiers";
        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<bool[,]>("Mask", "in.mask")
        };
        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool[,]>("Mask", "out.mask")
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

            var result = (bool[,])source.Clone();
            int iterations = Mathf.Max(1, _iterations);
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                result = _operation switch
                {
                    MaskMorphologyOperation.Dilate =>
                        MapNodeUtility.Morph(result, dilate: true, _radius),
                    MaskMorphologyOperation.Erode =>
                        MapNodeUtility.Morph(result, dilate: false, _radius),
                    MaskMorphologyOperation.Open =>
                        MapNodeUtility.Morph(
                            MapNodeUtility.Morph(result, dilate: false, _radius),
                            dilate: true,
                            _radius),
                    MaskMorphologyOperation.Close =>
                        MapNodeUtility.Morph(
                            MapNodeUtility.Morph(result, dilate: true, _radius),
                            dilate: false,
                            _radius),
                    _ => result
                };
            }

            context?.CountIteration(width * height * iterations);
            return NodeOutput.Success(result);
        }
    }
}
