using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement
{
    [NodeInfo("Object Output To TWC", "Object Placement", "Registers an object placement layer so the graph compiler can create a TWC Object Build Layer.")]
    [HidePreview]
    public sealed class ObjectOutputToTWCNode : NodeBase
    {
        public override string Title => "Object Output To TWC";
        public override string Category => "Object Placement";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<ObjectPlacementLayer>("Object Layer")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<ObjectPlacementLayer>("Object Layer")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (inputs == null || inputs.Length == 0 || inputs[0] is not ObjectPlacementLayer layer)
                return NodeOutput.Error("Object Layer input is required.");

            if (layer.Rule != null && !layer.Rule.UseTWCObjectLayer)
                return NodeOutput.Warning("Object layer is not marked for TWC output.", layer);

            if (!context.TryGetService<ObjectPlacementRegistry>(out var registry) || registry == null)
            {
                registry = new ObjectPlacementRegistry();
                context.RegisterService(registry);
            }

            registry.Register(layer);
            return NodeOutput.Success(layer);
        }
    }
}
