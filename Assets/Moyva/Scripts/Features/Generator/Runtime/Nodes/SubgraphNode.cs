using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Nodes
{
    [NodeInfo(
        "Subgraph",
        "Subgraphs",
        "Виконує вкладений граф як повторно використовуваний модуль і повертає результат конкретного вихідного шару.",
        StableId = "moyva.subgraphs.subgraph",
        Order = 20,
        PreviewOutput = "out.biome_map")]
    public sealed class SubgraphNode : NodeBase, IAsyncNode
    {
        [SerializeField] private GraphAsset _subgraph;
        [SerializeField, Tooltip("ID шару підграфа, Output якого потрібно повернути. Для графа з одним Output визначається автоматично.")]
        private string _outputLayerId;

        public override string Title => "Subgraph";
        public override string Category => "Subgraphs";

        public GraphAsset Subgraph
        {
            get => _subgraph;
            set => _subgraph = value;
        }

        public string OutputLayerId
        {
            get => _outputLayerId;
            set => _outputLayerId = value;
        }

        public override PortDefinition[] Inputs => new[]
        {
            PortDefinition.OptionalInput<string[,]>("Biome Map", "in.biome_map"),
            PortDefinition.OptionalInput<string[,]>("Object Map", "in.object_map"),
            PortDefinition.OptionalInput<float[,]>("Height Map", "in.height_map"),
            PortDefinition.OptionalInput<string[,]>("Building Map", "in.building_map")
        };

        public override PortDefinition[] Outputs => new[]
        {
            PortDefinition.Output<string[,]>("Biome Map", "out.biome_map"),
            PortDefinition.Output<string[,]>("Object Map", "out.object_map"),
            PortDefinition.Output<float[,]>("Height Map", "out.height_map"),
            PortDefinition.Output<string[,]>("Building Map", "out.building_map")
        };

        public override NodeOutput Execute(object[] inputs, NodeContext context)
        {
            return ExecuteInternal(inputs, context, asynchronous: false)
                .GetAwaiter()
                .GetResult();
        }

        public Task<NodeOutput> ExecuteAsync(
            object[] inputs,
            NodeContext context)
        {
            return ExecuteInternal(inputs, context, asynchronous: true);
        }

        private async Task<NodeOutput> ExecuteInternal(
            object[] inputs,
            NodeContext context,
            bool asynchronous)
        {
            if (_subgraph == null)
                return NodeOutput.Warning("Subgraph is not assigned. Pass-through mode enabled.",
                    ResolveStringMap(inputs, 0, context),
                    ResolveStringMap(inputs, 1, context),
                    ResolveFloatMap(inputs, 2, context),
                    ResolveStringMap(inputs, 3, context));

            if (context == null)
                return NodeOutput.Error("NodeContext is missing.");
            if (_subgraph.Nodes.Contains(this))
                return NodeOutput.Error("Subgraph cannot reference the graph that contains this node.");
            if (!TryResolveOutputLayerId(out string outputLayerId, out string resolveError))
                return NodeOutput.Error(resolveError);

            var inputData = new SubgraphInputData
            {
                BiomeMap = inputs != null && inputs.Length > 0
                    ? inputs[0] as string[,]
                    : null,
                ObjectMap = inputs != null && inputs.Length > 1
                    ? inputs[1] as string[,]
                    : null,
                HeightMap = inputs != null && inputs.Length > 2
                    ? inputs[2] as float[,]
                    : null,
                BuildingMap = inputs != null && inputs.Length > 3
                    ? inputs[3] as string[,]
                    : null
            };

            var requestedLayers = new HashSet<string> { outputLayerId };
            Action<NodeContext> configureChild = child =>
            {
                context.CopyServicesTo(child);
                child.ApplySharedSettings(_subgraph.SharedSettings);
                child.RegisterService(inputData);
            };
            var snapshot = asynchronous
                ? await GraphEvaluationPipeline.EvaluateAsync(
                    _subgraph,
                    context.Seed,
                    context.MapSize,
                    configureContext: configureChild,
                    requestedLayerIds: requestedLayers)
                : GraphEvaluationPipeline.Evaluate(
                    _subgraph,
                    context.Seed,
                    context.MapSize,
                    configureContext: configureChild,
                    requestedLayerIds: requestedLayers);
            if (!snapshot.Success)
            {
                return NodeOutput.Error(
                    $"Subgraph failed: {snapshot.ExecutionResult?.ErrorMessage ?? snapshot.Diagnostics}");
            }

            var output = snapshot.GetLayerOutput<LayerOutputSnapshot>(outputLayerId);
            if (output == null)
                return NodeOutput.Error("Subgraph Output did not produce a LayerOutputSnapshot.");

            return NodeOutput.Success(
                output.BiomeMap ?? ResolveStringMap(inputs, 0, context),
                output.ObjectMap ?? ResolveStringMap(inputs, 1, context),
                output.HeightMap ?? ResolveFloatMap(inputs, 2, context),
                output.BuildingMap ?? ResolveStringMap(inputs, 3, context));
        }

        public bool TryMigrateOutputLayerId(out string error)
        {
            return TryResolveOutputLayerId(out _, out error);
        }

        private bool TryResolveOutputLayerId(
            out string outputLayerId,
            out string error)
        {
            outputLayerId = null;
            error = null;
            if (_subgraph == null)
            {
                error = "Subgraph is not assigned.";
                return false;
            }

            _subgraph.EnsureLayerGraphStates();
            var outputNodes = _subgraph.Nodes
                .OfType<OutputNode>()
                .ToList();
            if (!string.IsNullOrEmpty(_outputLayerId))
            {
                int matches = outputNodes.Count(node =>
                    node.LayerId == _outputLayerId);
                if (matches == 1)
                {
                    outputLayerId = _outputLayerId;
                    return true;
                }

                if (outputNodes.Count != 1)
                {
                    error = matches == 0
                        ? $"Subgraph has no Output in layer '{_outputLayerId}'."
                        : $"Subgraph has multiple Outputs in layer '{_outputLayerId}'.";
                    return false;
                }
            }

            if (outputNodes.Count != 1)
            {
                error =
                    $"Subgraph must contain exactly one Output before automatic migration. Found {outputNodes.Count}.";
                return false;
            }

            _outputLayerId = outputNodes[0].LayerId;
            outputLayerId = _outputLayerId;
            return true;
        }

        private static string[,] ResolveStringMap(object[] inputs, int index, NodeContext context)
        {
            return inputs != null && index >= 0 && index < inputs.Length && inputs[index] is string[,] map
                ? map
                : new string[Mathf.Max(1, context?.MapSize.x ?? 0), Mathf.Max(1, context?.MapSize.y ?? 0)];
        }

        private static float[,] ResolveFloatMap(object[] inputs, int index, NodeContext context)
        {
            return inputs != null && index >= 0 && index < inputs.Length && inputs[index] is float[,] map
                ? map
                : new float[Mathf.Max(1, context?.MapSize.x ?? 0), Mathf.Max(1, context?.MapSize.y ?? 0)];
        }
    }
}
