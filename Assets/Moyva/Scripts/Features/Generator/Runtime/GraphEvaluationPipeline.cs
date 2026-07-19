using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Єдиний deterministic evaluation для runtime, node preview і layer preview.
    /// Кожен layer scope виконується не більше одного разу; Layer Reference
    /// dependencies виконуються раніше за споживача.
    /// </summary>
    public static class GraphEvaluationPipeline
    {
        public static GraphEvaluationSnapshot Evaluate(
            GraphAsset graph,
            int seed,
            Vector2Int mapSize,
            long revision = 0,
            Action<NodeContext> configureContext = null,
            ISet<string> skippedLayerIds = null,
            ISet<string> requestedLayerIds = null)
        {
            return EvaluateInternal(
                    graph,
                    seed,
                    mapSize,
                    revision,
                    configureContext,
                    skippedLayerIds,
                    requestedLayerIds,
                    asynchronous: false)
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Runs the exact same evaluation as <see cref="Evaluate"/>, but yields
        /// between nodes on Unity's synchronization context. Unity/TWC node work
        /// remains on the main thread while the editor can process input/repaint.
        /// </summary>
        public static Task<GraphEvaluationSnapshot> EvaluateAsync(
            GraphAsset graph,
            int seed,
            Vector2Int mapSize,
            long revision = 0,
            Action<NodeContext> configureContext = null,
            ISet<string> skippedLayerIds = null,
            ISet<string> requestedLayerIds = null)
        {
            return EvaluateInternal(
                graph,
                seed,
                mapSize,
                revision,
                configureContext,
                skippedLayerIds,
                requestedLayerIds,
                asynchronous: true);
        }

        private static async Task<GraphEvaluationSnapshot> EvaluateInternal(
            GraphAsset graph,
            int seed,
            Vector2Int mapSize,
            long revision,
            Action<NodeContext> configureContext,
            ISet<string> skippedLayerIds,
            ISet<string> requestedLayerIds,
            bool asynchronous)
        {
            int effectiveSeed = GlobalSeed.Normalize(seed);
            var safeMapSize = new Vector2Int(
                Mathf.Max(1, mapSize.x),
                Mathf.Max(1, mapSize.y));
            if (graph == null)
            {
                return new GraphEvaluationSnapshot(
                    null,
                    effectiveSeed,
                    safeMapSize,
                    revision,
                    diagnostics: "GraphAsset is null.",
                    sourceGraph: null);
            }

            graph.EnsureLayerGraphStates();
            var diagnostics = new List<string>();
            var layers = graph.Layers
                .Where(layer => layer != null && layer.Enabled)
                .Where(layer =>
                    skippedLayerIds == null
                    || !skippedLayerIds.Contains(layer.Id))
                .OrderBy(layer => layer.SortingOrder)
                .ThenBy(layer => layer.Name, StringComparer.Ordinal)
                .ToArray();
            var duplicateLayerIds = layers
                .Where(layer => !string.IsNullOrEmpty(layer.Id))
                .GroupBy(layer => layer.Id, StringComparer.Ordinal)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();
            if (duplicateLayerIds.Length > 0)
            {
                string error =
                    "Enabled graph layers contain duplicate IDs: "
                    + string.Join(", ", duplicateLayerIds);
                return new GraphEvaluationSnapshot(
                    GraphExecutionResult.Failure(error),
                    effectiveSeed,
                    safeMapSize,
                    revision,
                    diagnostics: error,
                    sourceGraph: graph);
            }

            var layersById = layers
                .Where(layer => !string.IsNullOrEmpty(layer.Id))
                .ToDictionary(layer => layer.Id, StringComparer.Ordinal);
            var runner = new GraphRunner();
            var registry = new LayerMaskRegistry();
            var results = new List<GraphExecutionResult>(layers.Length);
            var visited = new HashSet<string>(StringComparer.Ordinal);
            var visiting = new HashSet<string>(StringComparer.Ordinal);
            var rootLayers = requestedLayerIds == null
                ? layers
                : layers
                    .Where(layer => requestedLayerIds.Contains(layer.Id))
                    .ToArray();

            if (requestedLayerIds != null)
            {
                foreach (string requestedLayerId in requestedLayerIds)
                {
                    if (!string.IsNullOrEmpty(requestedLayerId)
                        && !layersById.ContainsKey(requestedLayerId))
                    {
                        diagnostics.Add(
                            $"Requested subgraph layer '{requestedLayerId}' is missing, disabled, or skipped.");
                    }
                }
            }

            for (int i = 0; i < rootLayers.Length; i++)
            {
                await EvaluateLayerAsync(
                    graph,
                    rootLayers[i].Id,
                    effectiveSeed,
                    safeMapSize,
                    configureContext,
                    layersById,
                    runner,
                    registry,
                    results,
                    visited,
                    visiting,
                    diagnostics,
                    asynchronous);
            }

            var combined = GraphExecutionResult.Combine(results);
            var layerOutputs = new Dictionary<string, object>(
                StringComparer.Ordinal);
            var compiledLayerMatrices = new Dictionary<string, bool[,]>(
                StringComparer.Ordinal);
            foreach (string evaluatedLayerId in visited)
            {
                var outputNode = GraphLayerRuntimeSemantics.GetLayerOutputNode(
                    graph,
                    evaluatedLayerId);
                var artifact = outputNode != null
                    ? combined.GetArtifact(outputNode.NodeId)
                    : null;
                if (artifact != null)
                    layerOutputs[evaluatedLayerId] = artifact;
                if (artifact is ILayerMaskArtifact layerMaskArtifact
                    && layerMaskArtifact.LayerMask != null)
                {
                    compiledLayerMatrices[evaluatedLayerId] =
                        layerMaskArtifact.LayerMask;
                }
            }

            if (combined.Success && diagnostics.Count > 0)
            {
                combined = GraphExecutionResult.Failure(
                    string.Join("\n", diagnostics));
            }

            return new GraphEvaluationSnapshot(
                combined,
                effectiveSeed,
                safeMapSize,
                revision,
                compiledLayerMatrices,
                diagnostics: diagnostics.Count == 0
                    ? null
                    : string.Join("\n", diagnostics),
                layerOutputs: layerOutputs,
                sourceGraph: graph);
        }

        private static async Task EvaluateLayerAsync(
            GraphAsset graph,
            string layerId,
            int seed,
            Vector2Int mapSize,
            Action<NodeContext> configureContext,
            IReadOnlyDictionary<string, GeneratorLayerDefinition> layersById,
            GraphRunner runner,
            LayerMaskRegistry registry,
            List<GraphExecutionResult> results,
            HashSet<string> visited,
            HashSet<string> visiting,
            List<string> diagnostics,
            bool asynchronous)
        {
            if (string.IsNullOrEmpty(layerId) || visited.Contains(layerId))
                return;
            if (!layersById.ContainsKey(layerId))
                return;
            if (!visiting.Add(layerId))
            {
                diagnostics.Add(
                    $"Circular Layer Mask Reference detected at layer '{layerId}'.");
                return;
            }

            try
            {
                var scope = graph.CreateExecutionScope(layerId);
                foreach (string dependencyId in EnumerateLayerDependencies(scope))
                {
                    await EvaluateLayerAsync(
                        graph,
                        dependencyId,
                        seed,
                        mapSize,
                        configureContext,
                        layersById,
                        runner,
                        registry,
                        results,
                        visited,
                        visiting,
                        diagnostics,
                        asynchronous);
                }

                var context = new NodeContext(seed)
                {
                    MapSize = mapSize
                };
                if (graph.SharedSettings != null)
                {
                    context.ApplySharedSettings(graph.SharedSettings);
                    context.RegisterService(graph.SharedSettings);
                }
                if (graph.TileRegistry != null)
                    context.RegisterService(graph.TileRegistry);

                configureContext?.Invoke(context);
                context.RegisterService(registry);

                var result = asynchronous
                    ? await runner.ExecuteAsync(scope, context)
                    : runner.Execute(scope, context);
                results.Add(result);
                if (result == null || !result.Success)
                {
                    diagnostics.Add(
                        $"Layer '{layersById[layerId].Name}' failed: " +
                        $"{result?.ErrorMessage ?? "unknown evaluation error"}");
                }

                visited.Add(layerId);
            }
            finally
            {
                visiting.Remove(layerId);
            }
        }

        private static IEnumerable<string> EnumerateLayerDependencies(
            GraphExecutionScope scope)
        {
            if (scope?.Nodes == null)
                yield break;

            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < scope.Nodes.Count; i++)
            {
                if (scope.Nodes[i] is not LayerMaskReferenceNode reference)
                    continue;
                if (!string.IsNullOrEmpty(reference.SourceLayerId)
                    && seen.Add(reference.SourceLayerId))
                    yield return reference.SourceLayerId;
            }
        }
    }
}
