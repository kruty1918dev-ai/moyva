using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo(
        "Bool Value",
        "Values",
        "Повертає булеве значення для умовних входів інших вузлів.",
        StableId = "moyva.values.bool",
        Order = 10,
        PreviewOutput = "out.value")]
    public sealed class BoolValueNode : NodeBase
    {
        [SerializeField]
        [InlineEditable("значення")]
        private bool value;

        public override string Title => "Bool Value";
        public override string Category => "Values";

        public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool>("Value", "out.value")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            return NodeOutput.Success(value);
        }
    }
}
