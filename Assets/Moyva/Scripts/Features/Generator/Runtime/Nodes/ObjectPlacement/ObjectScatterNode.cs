using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement
{
    [NodeInfo(
        "Object Scatter",
        "Objects",
        "Створює детерміновані кандидати розміщення об’єктів із вхідної маски.",
        StableId = "moyva.objects.scatter",
        Order = 20,
        PreviewOutput = "out.candidates")]
    public sealed class ObjectScatterNode : NodeBase
    {
        [SerializeField]
        [Tooltip("Загальні правила розкиду, використовуються перед передачею результату до шару об'єктів.")]
        private ObjectPlacementRule _rule = new();

        public override string Title => "Object Scatter";
        public override string Category => "Objects";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<ScatterMask>("Scatter Mask", "in.scatter_mask")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<List<ScatterCandidate>>("Candidates", "out.candidates")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (inputs == null || inputs.Length == 0 || inputs[0] is not ScatterMask mask)
                return NodeOutput.Error("Scatter Mask input is required.");

            var candidates = ObjectPlacementScatterUtility.ScatterUniform(mask, _rule, context?.Seed ?? 1);
            return NodeOutput.Success(candidates);
        }
    }
}
