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
        Dictionary<string, bool[,]> Build(
            GraphAsset graph,
            int seed,
            Vector2Int mapSize,
            ISet<string> skippedLayerIds,
            GraphEvaluationSnapshot evaluationSnapshot = null);
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
            ISet<string> skippedLayerIds,
            GraphEvaluationSnapshot evaluationSnapshot = null)
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
                return masksByLayerId;

            foreach (var layer in orderedLayers)
                TryBuildLayerMask(
                    graph,
                    layer,
                    safeMapSize,
                    evaluationSnapshot,
                    masksByLayerId);

            return masksByLayerId;
        }

        private void TryBuildLayerMask(
            GraphAsset graph,
            GeneratorLayerDefinition layer,
            Vector2Int mapSize,
            GraphEvaluationSnapshot evaluationSnapshot,
            Dictionary<string, bool[,]> masksByLayerId)
        {
            var outputNode = GraphLayerRuntimeSemantics.GetLayerOutputNode(graph, layer.Id);
            if (outputNode == null || !ShouldPrecompute(outputNode.OutputKind))
                return;

            if (evaluationSnapshot == null)
            {
                Debug.LogWarning(
                    $"[GraphToConfigurationCompiler] Layer '{layer.Name}' graph-output mask was not precomputed: " +
                    "evaluation snapshot is missing.");
                return;
            }

            var mask = ExtractBestMask(
                outputNode,
                evaluationSnapshot,
                mapSize);
            if (mask != null)
                masksByLayerId[layer.Id] = mask;
        }

        private bool[,] ExtractBestMask(
            OutputNode outputNode,
            GraphEvaluationSnapshot evaluationSnapshot,
            Vector2Int mapSize)
        {
            var snapshot =
                evaluationSnapshot.GetNodeArtifact<LayerOutputSnapshot>(
                    outputNode.NodeId);
            return _maskUtility.ExtractOutputMask(
                snapshot,
                outputNode.OutputKind,
                mapSize.x,
                mapSize.y);
        }

        private static bool ShouldPrecompute(LayerOutputKind outputKind)
        {
            return outputKind == LayerOutputKind.Tiles || outputKind == LayerOutputKind.Masks;
        }
    }
}
