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
        IReadOnlyList<ObjectPlacementLayer> Collect(GraphAsset graph, int seed, Vector2Int mapSize, ISet<string> skippedLayerIds);
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
            ISet<string> skippedLayerIds)
        {
            if (graph == null || graph.Nodes == null || !graph.Nodes.Any(node => node is ObjectOutputToTWCNode))
                return Array.Empty<ObjectPlacementLayer>();

            graph.EnsureLayerGraphStates();
            var safeMapSize = new Vector2Int(Mathf.Max(1, mapSize.x), Mathf.Max(1, mapSize.y));
            var layerMaskRegistry = CreateLayerMaskRegistry(graph, seed, safeMapSize, skippedLayerIds);
            var runner = new GraphRunner();
            var result = new List<ObjectPlacementLayer>();

            foreach (var layerDef in GetOrderedEnabledLayers(graph, skippedLayerIds))
                CollectLayer(graph, runner, layerDef, seed, safeMapSize, layerMaskRegistry, result);

            return result;
        }

        public void Apply(Configuration config, TileWorldCreatorManager manager,
            IReadOnlyList<ObjectPlacementLayer> objectLayers, IReadOnlyList<CompiledLayerMap> compiledLayers)
        {
            TWCObjectPlacementAdapter.Apply(config, manager, objectLayers, compiledLayers);
        }

        private LayerMaskRegistry CreateLayerMaskRegistry(GraphAsset graph, int seed, Vector2Int mapSize, ISet<string> skippedLayerIds)
        {
            var registry = new LayerMaskRegistry();
            LayerMaskPrewarmUtility.PrewarmAllLayerMasks(
                graph,
                seed == 0 ? 1 : seed,
                mapSize,
                registry,
                context => _contextFactory.RegisterServices(context, graph),
                skippedLayerIds);
            return registry;
        }

        private void CollectLayer(GraphAsset graph, GraphRunner runner, GeneratorLayerDefinition layerDef,
            int seed, Vector2Int mapSize, LayerMaskRegistry registry, List<ObjectPlacementLayer> result)
        {
            var scope = graph.CreateExecutionScope(layerDef.Id);
            bool collectsObjects = scope.Nodes.Any(node => node is ObjectOutputToTWCNode);
            var context = _contextFactory.Create(graph, seed == 0 ? 1 : seed, mapSize, registry);
            var placementRegistry = new ObjectPlacementRegistry();
            context.RegisterService(placementRegistry);

            var execution = runner.Execute(scope, context);
            if (execution == null || !execution.Success)
            {
                Debug.LogWarning($"[MoyvaObjectPlacement] Layer '{layerDef.Name ?? scope.LayerId}' was not executed for object placement masks: {execution?.ErrorMessage ?? "unknown graph execution error"}");
                return;
            }

            if (!collectsObjects)
                return;
            if (placementRegistry.Layers.Count == 0)
                Debug.LogWarning($"[MoyvaObjectPlacement] Layer '{layerDef.Name ?? scope.LayerId}' has Object Output nodes, but no ObjectPlacementLayer was registered.");

            foreach (var layer in placementRegistry.Layers)
            {
                if (layer != null && string.IsNullOrWhiteSpace(layer.TargetGraphLayerId))
                    layer.TargetGraphLayerId = scope.LayerId;
                if (layer != null)
                    result.Add(layer);
            }
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
