using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement
{
    [NodeInfo("Object Scatter", "Object Placement", "Creates non-clustered scatter candidates from a placement mask.")]
    public sealed class ObjectScatterNode : NodeBase, IPreviewableNode
    {
        [SerializeField]
        [Tooltip("General scatter rules used before the result is sent to an object layer.")]
        private ObjectPlacementRule _rule = new();

        [NonSerialized] private ScatterMask _lastMask;
        [NonSerialized] private List<ScatterCandidate> _lastCandidates;

        public override string Title => "Object Scatter";
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
            _lastCandidates = ObjectPlacementScatterUtility.ScatterUniform(mask, _rule, context?.Seed ?? 1);
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
