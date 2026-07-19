using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo(
        "Int Value",
        "Values",
        "Повертає ціле число для параметричних входів інших вузлів.",
        StableId = "moyva.values.int",
        Order = 30,
        PreviewOutput = "out.value")]
    public sealed class IntValueNode : NodeBase
    {
        [SerializeField]
        [InlineEditable("значення")]
        private int value;

        public override string Title => "Int Value";
        public override string Category => "Values";

        public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<int>("Value", "out.value")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            return NodeOutput.Success(value);
        }
    }
}
