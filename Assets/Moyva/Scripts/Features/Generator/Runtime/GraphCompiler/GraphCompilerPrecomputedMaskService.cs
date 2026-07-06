using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphCompilerPrecomputedMaskService
    {
        Dictionary<string, bool[,]> Build(GraphAsset graph, int seed, Vector2Int mapSize, ISet<string> skippedLayerIds);
    }

    internal sealed class GraphCompilerPrecomputedMaskService : IGraphCompilerPrecomputedMaskService
    {
        private readonly IGraphCompilerRuntimeContextFactory _contextFactory;
        private readonly IGraphCompilerMaskUtility _maskUtility;

        public GraphCompilerPrecomputedMaskService(
            IGraphCompilerRuntimeContextFactory contextFactory,
            IGraphCompilerMaskUtility maskUtility)
        {
            _contextFactory = contextFactory;
            _maskUtility = maskUtility;
        }

        public Dictionary<string, bool[,]> Build(
            GraphAsset graph,
            int seed,
            Vector2Int mapSize,
            ISet<string> skippedLayerIds)
        {
            var masksByLayerId = new Dictionary<string, bool[,]>(StringComparer.Ordinal);
            if (graph == null || graph.Nodes == null)
                return masksByLayerId;

            graph.EnsureLayerGraphStates();
            var orderedLayers = graph.Layers?
                .Where(layer => layer != null && layer.Enabled)
                .Where(layer => skippedLayerIds == null || !skippedLayerIds.Contains(layer.Id))
                .OrderBy(layer => layer.SortingOrder)
                .ToList();
            if (orderedLayers == null || orderedLayers.Count == 0)
                return masksByLayerId;

            int safeSeed = seed == 0 ? 1 : seed;
            var safeMapSize = new Vector2Int(Mathf.Max(1, mapSize.x), Mathf.Max(1, mapSize.y));
            var layerMaskRegistry = CreateLayerMaskRegistry(graph, safeSeed, safeMapSize, skippedLayerIds);
            var runner = new GraphRunner();

            foreach (var layer in orderedLayers)
                TryBuildLayerMask(graph, runner, layer, safeSeed, safeMapSize, layerMaskRegistry, masksByLayerId);

            return masksByLayerId;
        }

        private LayerMaskRegistry CreateLayerMaskRegistry(
            GraphAsset graph,
            int seed,
            Vector2Int mapSize,
            ISet<string> skippedLayerIds)
        {
            var registry = new LayerMaskRegistry();
            LayerMaskPrewarmUtility.PrewarmAllLayerMasks(
                graph,
                seed,
                mapSize,
                registry,
                context => _contextFactory.RegisterServices(context, graph),
                skippedLayerIds);
            return registry;
        }

        private void TryBuildLayerMask(GraphAsset graph, GraphRunner runner, GeneratorLayerDefinition layer,
            int seed, Vector2Int mapSize, LayerMaskRegistry registry, Dictionary<string, bool[,]> masksByLayerId)
        {
            var outputNode = GraphLayerRuntimeSemantics.GetLayerOutputNode(graph, layer.Id);
            if (outputNode == null || !ShouldPrecompute(outputNode.OutputKind))
                return;

            var context = _contextFactory.Create(graph, seed, mapSize, registry);
            var result = runner.Execute(graph.CreateExecutionScope(layer.Id), context);
            if (result == null || !result.Success)
            {
                Debug.LogWarning($"[GraphToConfigurationCompiler] Layer '{layer.Name}' graph-output mask was not precomputed: {result?.ErrorMessage ?? "unknown graph execution error"}");
                return;
            }

            var mask = ExtractBestMask(graph, layer.Id, outputNode, result, mapSize, registry);
            if (mask != null)
                masksByLayerId[layer.Id] = mask;
        }

        private bool[,] ExtractBestMask(GraphAsset graph, string layerId, OutputNode outputNode,
            GraphExecutionResult result, Vector2Int mapSize, LayerMaskRegistry registry)
        {
            var outputs = result.GetOutputs(outputNode.NodeId);
            var mask = _maskUtility.ExtractOutputMask(outputs, outputNode.OutputKind, mapSize.x, mapSize.y)
                       ?? _maskUtility.ExtractConnectedMask(graph, outputNode, result, mapSize.x, mapSize.y);
            if (mask == null && outputNode.OutputKind == LayerOutputKind.Tiles)
                mask = _maskUtility.ExtractTileSettingsMask(graph, layerId, result, mapSize.x, mapSize.y);
            if (mask == null && registry.TryGetLatestMask(layerId, out var registeredMask))
                mask = _maskUtility.Normalize(registeredMask, mapSize.x, mapSize.y);
            return mask;
        }

        private static bool ShouldPrecompute(LayerOutputKind outputKind)
        {
            return outputKind == LayerOutputKind.Tiles || outputKind == LayerOutputKind.Masks;
        }
    }
}
