using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement
{
    [NodeInfo(
        "Cluster Scatter",
        "Objects",
        "Створює детерміновані групові кандидати розміщення з вхідної маски.",
        StableId = "moyva.objects.cluster-scatter",
        Order = 30,
        PreviewOutput = "out.candidates")]
    public sealed class ClusterScatterNode : NodeBase
    {
        [SerializeField]
        [Tooltip("Налаштування форми та густини кластерів.")]
        private ClusterSettings _cluster = new();

        [SerializeField]
        [Tooltip("Правила розкиду для кожного кандидата. Густина та мінімальна відстань застосовуються всередині кожного кластеру.")]
        private ObjectPlacementRule _rule = new()
        {
            Density = 0.75f,
            MinDistance = 1f,
            Jitter = 0.25f,
            ScaleRandomization = new Vector2(0.85f, 1.15f)
        };

        public override string Title => "Cluster Scatter";
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
                return NodeOutput.Error("Вхідна маска розкиду є обов'язковою.");

            var candidates = _cluster.Enabled
                ? ObjectPlacementScatterUtility.ScatterClustered(mask, _cluster, _rule, context?.Seed ?? 1)
                : ObjectPlacementScatterUtility.ScatterUniform(mask, _rule, context?.Seed ?? 1);

            return NodeOutput.Success(candidates);
        }
    }
}
