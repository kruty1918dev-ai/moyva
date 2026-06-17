using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement
{
    [NodeInfo("Cluster Scatter", "Object Placement", "Creates Bad North-style grouped scatter candidates from a placement mask.")]
    public sealed class ClusterScatterNode : NodeBase, IPreviewableNode
    {
        [SerializeField]
        [Tooltip("Cluster shape and density settings.")]
        private ClusterSettings _cluster = new();

        [SerializeField]
        [Tooltip("Per-candidate scatter rules. Density and min distance are applied inside each cluster.")]
        private ObjectPlacementRule _rule = new()
        {
            Density = 0.75f,
            MinDistance = 1f,
            Jitter = 0.25f,
            ScaleRandomization = new Vector2(0.85f, 1.15f)
        };

        [NonSerialized] private ScatterMask _lastMask;
        [NonSerialized] private List<ScatterCandidate> _lastCandidates;

        public override string Title => "Cluster Scatter";
        public override string Category => "Object Placement";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<ScatterMask>("Scatter Mask")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<List<ScatterCandidate>>("Candidates")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (inputs == null || inputs.Length == 0 || inputs[0] is not ScatterMask mask)
                return NodeOutput.Error("Scatter Mask input is required.");

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
