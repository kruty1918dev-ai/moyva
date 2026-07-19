using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo(
        "String Value",
        "Values",
        "Повертає рядок для текстових входів, зокрема формул у функціональних вузлах.",
        StableId = "moyva.values.string",
        Order = 40,
        PreviewOutput = "out.value")]
    public sealed class StringValueNode : NodeBase
    {
        [SerializeField]
        [TextArea(2, 6)]
        private string value = string.Empty;

        public override string Title => "String Value";
        public override string Category => "Values";

        public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string>("Value", "out.value")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            return NodeOutput.Success(value ?? string.Empty);
        }
    }
}
