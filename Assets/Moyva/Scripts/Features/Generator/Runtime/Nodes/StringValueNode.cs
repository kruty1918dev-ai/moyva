using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("String Value", "Значення", "Повертає рядок (string) для текстових входів, зокрема формул у функціональних нодах.")]
    [HidePreview]
    public sealed class StringValueNode : NodeBase
    {
        [SerializeField]
        [TextArea(2, 6)]
        private string value = string.Empty;

        public override string Title => "String Value";
        public override string Category => "Значення";

        public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string>("Value")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            return NodeOutput.Success(value ?? string.Empty);
        }
    }
}