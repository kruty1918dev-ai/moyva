using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphToConfigurationCompilerService : IGraphToConfigurationCompilerService
    {
        private readonly IGraphCompilerConfigurationService _configuration;
        private readonly IGraphCompilerDiagnosticsService _diagnostics;
        private readonly IGraphCompilerBlueprintSyncService _blueprints;
        private readonly IGraphCompilerTileBuildLayerSyncService _buildLayers;
        private readonly IGraphCompilerPrecomputedMaskService _masks;
        private readonly IGraphCompilerModifierService _modifiers;
        private readonly IGraphCompilerObjectPlacementService _objects;

        public GraphToConfigurationCompilerService(
            IGraphCompilerConfigurationService configuration,
            IGraphCompilerDiagnosticsService diagnostics,
            IGraphCompilerBlueprintSyncService blueprints,
            IGraphCompilerTileBuildLayerSyncService buildLayers,
            IGraphCompilerPrecomputedMaskService masks,
            IGraphCompilerModifierService modifiers,
            IGraphCompilerObjectPlacementService objects)
        {
            _configuration = configuration;
            _diagnostics = diagnostics;
            _blueprints = blueprints;
            _buildLayers = buildLayers;
            _masks = masks;
            _modifiers = modifiers;
            _objects = objects;
        }

        public List<CompiledLayerMap> Compile(
            GraphAsset graph,
            TileWorldCreatorManager manager,
            int seed,
            ISet<string> skippedLayerIds = null,
            Vector2Int? mapSizeOverride = null)
        {
            var result = new List<CompiledLayerMap>();
            Vector2Int requestedSize = _configuration.ResolveRequestedSize(graph, mapSizeOverride);
            _diagnostics.LogEnter(graph, requestedSize, seed);
            if (graph == null || manager == null || manager.configuration == null)
                return result;

            var config = manager.configuration;
            _configuration.Apply(graph, config, seed, mapSizeOverride);
            var sync = _blueprints.Sync(graph, config, skippedLayerIds);
            _diagnostics.LogLayerOrder(graph, sync.OrderedLayers);
            _buildLayers.Sync(graph, config, manager, sync, skippedLayerIds);

            var mapSize = new Vector2Int(config.width, config.height);
            var masks = _masks.Build(graph, seed, mapSize, skippedLayerIds);
            int topologyWarnings = CompileModifiers(graph, config, sync, masks);

            _blueprints.DisableUnused(sync.ExistingLayers, sync.UsedLayerGuids);
            var objectLayers = _objects.Collect(graph, seed, mapSize, skippedLayerIds);
            _objects.Apply(config, manager, objectLayers, sync.CompiledLayers);
            _diagnostics.LogSkipped(graph, skippedLayerIds);
            _diagnostics.LogResult(graph, config, objectLayers?.Count ?? 0, topologyWarnings);
            return sync.CompiledLayers;
        }

        private int CompileModifiers(GraphAsset graph, GiantGrey.TileWorldCreator.Configuration config,
            GraphCompilerBlueprintSyncResult sync, IReadOnlyDictionary<string, bool[,]> masks)
        {
            int warnings = 0;
            foreach (var layerDef in sync.OrderedLayers)
            {
                if (!sync.BlueprintByGraphLayerId.TryGetValue(layerDef.Id, out var blueprint) || blueprint == null)
                    continue;

                var plan = TopologicalSorter.BuildPlan(graph.CreateExecutionScope(layerDef.Id));
                if (!plan.Success)
                {
                    Debug.LogWarning($"[GraphToConfigurationCompiler] Layer '{layerDef.Name}' skipped while compiling TWC modifiers: {plan.ErrorMessage}");
                    warnings++;
                    continue;
                }

                _modifiers.Append(blueprint, layerDef.Id, plan.NodesInExecutionOrder, config,
                    sync.BlueprintGuidByGraphLayerId, graph, masks);
            }

            return warnings;
        }
    }
}
