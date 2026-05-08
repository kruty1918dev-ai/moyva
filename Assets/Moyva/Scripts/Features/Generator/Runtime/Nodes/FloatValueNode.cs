using System;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Float Value", "Values", "Повертає число з плаваючою крапкою (float) для параметричних входів інших нод.")]
    [HidePreview]
    public sealed class FloatValueNode : NodeBase
    {
        [SerializeField]
        [InlineEditable("value")]
        private float value;

        public override string Title => "Float Value";
        public override string Category => "Values";

        public override PortDefinition[] Inputs => Array.Empty<PortDefinition>();

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<float>("Value")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            return NodeOutput.Success(value);
        }
    }
}