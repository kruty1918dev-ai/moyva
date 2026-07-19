using System;
using System.Collections.Generic;
using System.Linq;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphCompilerObjectPlacementService
    {
        IReadOnlyList<ObjectPlacementLayer> Collect(
            GraphAsset graph,
            int seed,
            Vector2Int mapSize,
            ISet<string> skippedLayerIds,
            GraphEvaluationSnapshot evaluationSnapshot = null);
        void Apply(Configuration config, TileWorldCreatorManager manager,
            IReadOnlyList<ObjectPlacementLayer> objectLayers, IReadOnlyList<CompiledLayerMap> compiledLayers);
    }

    internal sealed class GraphCompilerObjectPlacementService : IGraphCompilerObjectPlacementService
    {
        private readonly IGraphCompilerRuntimeContextFactory _contextFactory;

        public GraphCompilerObjectPlacementService(IGraphCompilerRuntimeContextFactory contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IReadOnlyList<ObjectPlacementLayer> Collect(
            GraphAsset graph,
            int seed,
            Vector2Int mapSize,
            ISet<string> skippedLayerIds,
            GraphEvaluationSnapshot evaluationSnapshot = null)
        {
            if (graph == null || graph.Nodes == null || !graph.Nodes.Any(node => node is ObjectOutputToTWCNode))
                return Array.Empty<ObjectPlacementLayer>();

            graph.EnsureLayerGraphStates();
            var safeMapSize = new Vector2Int(Mathf.Max(1, mapSize.x), Mathf.Max(1, mapSize.y));
            int safeSeed = seed == 0 ? 1 : seed;
            if (evaluationSnapshot == null
                || !evaluationSnapshot.IsCompatibleWith(
                    graph,
                    safeSeed,
                    safeMapSize))
            {
                evaluationSnapshot = GraphEvaluationPipeline.Evaluate(
                    graph,
                    safeSeed,
                    safeMapSize,
                    configureContext: context =>
                        _contextFactory.RegisterServices(context, graph),
                    skippedLayerIds: skippedLayerIds);
            }

            if (!evaluationSnapshot.Success)
                return Array.Empty<ObjectPlacementLayer>();

            var result = new List<ObjectPlacementLayer>();
            var seen = new HashSet<ObjectPlacementLayer>();

            foreach (var layerDef in GetOrderedEnabledLayers(graph, skippedLayerIds))
            {
                var outputNodes = graph.GetNodesForLayer(layerDef.Id)
                    .OfType<ObjectOutputToTWCNode>()
                    .ToArray();
                int before = result.Count;
                for (int i = 0; i < outputNodes.Length; i++)
                {
                    var layer = GetNodeOutput<ObjectPlacementLayer>(
                        evaluationSnapshot,
                        outputNodes[i].NodeId);
                    if (layer == null || !seen.Add(layer))
                        continue;

                    if (string.IsNullOrWhiteSpace(layer.TargetGraphLayerId))
                        layer.TargetGraphLayerId = layerDef.Id;
                    result.Add(layer);
                }

                if (outputNodes.Length > 0 && result.Count == before)
                {
                    Debug.LogWarning(
                        $"[MoyvaObjectPlacement] Layer '{layerDef.Name ?? layerDef.Id}' has Object Output nodes, " +
                        "but the shared evaluation snapshot contains no ObjectPlacementLayer.");
                }
            }

            return result;
        }

        private static T GetNodeOutput<T>(
            GraphEvaluationSnapshot snapshot,
            string nodeId,
            int portIndex = 0)
        {
            var outputs = snapshot?.GetNodeOutputs(nodeId);
            if (outputs == null
                || portIndex < 0
                || portIndex >= outputs.Length)
            {
                return default;
            }

            return outputs[portIndex] is T value
                ? value
                : default;
        }

        public void Apply(Configuration config, TileWorldCreatorManager manager,
            IReadOnlyList<ObjectPlacementLayer> objectLayers, IReadOnlyList<CompiledLayerMap> compiledLayers)
        {
            TWCObjectPlacementAdapter.Apply(config, manager, objectLayers, compiledLayers);
        }

        private static IEnumerable<GeneratorLayerDefinition> GetOrderedEnabledLayers(
            GraphAsset graph,
            ISet<string> skippedLayerIds)
        {
            return graph.Layers
                .Where(layer => layer != null && layer.Enabled)
                .Where(layer => skippedLayerIds == null || !skippedLayerIds.Contains(layer.Id))
                .OrderBy(layer => layer.SortingOrder);
        }
    }
}
