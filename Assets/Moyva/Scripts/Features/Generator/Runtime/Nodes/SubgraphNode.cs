using System.Linq;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo("Підграф", "Макроси", "Виконує вкладений граф як повторно використовуваний модуль. Передає в підграф вхідні мапи через Subgraph Input node.")]
    public sealed class SubgraphNode : NodeBase
    {
        [SerializeField] private GraphAsset _subgraph;

        public override string Title => "Підграф";
        public override string Category => "Макроси";

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.Input<string[,]>("BiomeMap (optional)"),
            PortDefinition.Input<string[,]>("ObjectMap (optional)"),
            PortDefinition.Input<float[,]>("HeightMap (optional)"),
            PortDefinition.Input<string[,]>("BuildingMap (optional)")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("BiomeMap"),
            PortDefinition.Output<string[,]>("ObjectMap"),
            PortDefinition.Output<float[,]>("HeightMap"),
            PortDefinition.Output<string[,]>("BuildingMap")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            if (_subgraph == null)
                return NodeOutput.Warning("Subgraph is not assigned. Pass-through mode enabled.",
                    inputs[0] as string[,], inputs[1] as string[,], inputs[2] as float[,], inputs[3] as string[,]);

            var child = new NodeContext(context.Seed, context.Cancellation, context.Progress)
            {
                MapSize = context.MapSize
            };
            context.CopyServicesTo(child);

            child.RegisterService(new SubgraphInputData
            {
                BiomeMap = inputs[0] as string[,],
                ObjectMap = inputs[1] as string[,],
                HeightMap = inputs[2] as float[,],
                BuildingMap = inputs[3] as string[,]
            });

            var runner = new GraphRunner();
            var result = runner.Execute(_subgraph, child);
            if (!result.Success)
            {
                return NodeOutput.Warning($"Subgraph failed: {result.ErrorMessage}. Pass-through mode enabled.",
                    inputs[0] as string[,], inputs[1] as string[,], inputs[2] as float[,], inputs[3] as string[,]);
            }

            var outputNode = _subgraph.Nodes.FirstOrDefault(n => n is OutputNode);
            if (outputNode == null)
            {
                return NodeOutput.Warning("Subgraph has no Output node. Pass-through mode enabled.",
                    inputs[0] as string[,], inputs[1] as string[,], inputs[2] as float[,], inputs[3] as string[,]);
            }

            var outputs = result.GetOutputs(outputNode.NodeId);
            if (outputs == null || outputs.Length < 4)
            {
                return NodeOutput.Warning("Subgraph output is incomplete. Pass-through mode enabled.",
                    inputs[0] as string[,], inputs[1] as string[,], inputs[2] as float[,], inputs[3] as string[,]);
            }

            return NodeOutput.Success(
                outputs[0] as string[,] ?? (inputs[0] as string[,]),
                outputs[1] as string[,] ?? (inputs[1] as string[,]),
                outputs[2] as float[,] ?? (inputs[2] as float[,]),
                outputs[3] as string[,] ?? (inputs[3] as string[,]));
        }
    }
}
