using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Static Generator Data", "Data",
        "Stores incoming generator data in IGeneratorDataRegistry under a key, so runtime systems can read it after graph execution.")]
    public sealed class StaticGeneratorDataNode : NodeBase
    {
        [Header("Storage")]
        [Tooltip("Registry key used when Key input is not connected.")]
        [SerializeField] private string _key = "hill-levels";

        public override string Title => "Static Generator Data";
        public override string Category => "Data";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<object>("Data"),
            PortDefinition.Input<string>("Key (optional)")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<object>("Data")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            var data = inputs.Length > 0 ? inputs[0] : null;
            if (data == null)
                return NodeOutput.Error("Data input is required.");

            string key = inputs.Length > 1 && inputs[1] is string inputKey && !string.IsNullOrWhiteSpace(inputKey)
                ? inputKey.Trim()
                : _key?.Trim();

            if (string.IsNullOrWhiteSpace(key))
                return NodeOutput.Error("Generator data key is required.");

            if (!context.TryGetService<IGeneratorDataRegistry>(out var registry))
                return NodeOutput.Error("IGeneratorDataRegistry is not registered in NodeContext.");

            registry.Set(key, data);
            return NodeOutput.Success(data);
        }
    }
}