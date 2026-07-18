using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement
{
    [NodeInfo("Кластерний розкид", "Розміщення об'єктів", "Створює групові кандидати розкиду у стилі Bad North з маски розміщення.")]
    public sealed class ClusterScatterNode : NodeBase, IPreviewableNode
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

        [NonSerialized] private ScatterMask _lastMask;
        [NonSerialized] private List<ScatterCandidate> _lastCandidates;

        public override string Title => "Кластерний розкид";
        public override string Category => "Розміщення об'єктів";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<ScatterMask>("Маска розкиду")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<List<ScatterCandidate>>("Кандидати")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (inputs == null || inputs.Length == 0 || inputs[0] is not ScatterMask mask)
                return NodeOutput.Error("Вхідна маска розкиду є обов'язковою.");

            _lastMask = mask;
            _lastCandidates = _cluster.Enabled
                ? ObjectPlacementScatterUtility.ScatterClustered(mask, _cluster, _rule, context?.Seed ?? 1)
                : ObjectPlacementScatterUtility.ScatterUniform(mask, _rule, context?.Seed ?? 1);

            return NodeOutput.Success(_lastCandidates);
        }

        public Texture2D GeneratePreview(int width, int height)
        {
            return _lastMask == null
                ? null
                : ObjectPlacementPreviewUtility.BuildScatterTexture(_lastMask, _lastCandidates);
        }
    }
}
