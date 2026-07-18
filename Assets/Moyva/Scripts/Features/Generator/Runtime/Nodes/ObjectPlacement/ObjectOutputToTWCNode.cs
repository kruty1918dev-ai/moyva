using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement
{
    [NodeInfo("Об'єкт Output To TWC", "Розміщення об'єктів", "Реєструє шар розміщення об'єктів, щоб компілятор графа створив TWC Object Build Layer.")]
    [HidePreview]
    public sealed class ObjectOutputToTWCNode : NodeBase
    {
        public override string Title => "Об'єкт Output To TWC";
        public override string Category => "Розміщення об'єктів";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<ObjectPlacementLayer>("Шар об'єктів")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<ObjectPlacementLayer>("Шар об'єктів")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (inputs == null || inputs.Length == 0 || inputs[0] is not ObjectPlacementLayer layer)
                return NodeOutput.Error("Вхідний шар об'єктів є обов'язковим.");

            if (layer.Rule != null && !layer.Rule.UseTWCObjectLayer)
                return NodeOutput.Warning("Шар об'єктів не позначений для TWC виводу.", layer);

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
