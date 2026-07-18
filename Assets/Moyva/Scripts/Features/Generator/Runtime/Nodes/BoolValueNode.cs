using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Bool Value", "Значення", "Повертає булеве значення (bool) для умовних входів інших нод.")]
    [HidePreview]
    public sealed class BoolValueNode : NodeBase
    {
        [SerializeField]
        [InlineEditable("значення")]
        private bool value;

        public override string Title => "Bool Value";
        public override string Category => "Значення";

        public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<bool>("Value")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            return NodeOutput.Success(value);
        }
    }
}