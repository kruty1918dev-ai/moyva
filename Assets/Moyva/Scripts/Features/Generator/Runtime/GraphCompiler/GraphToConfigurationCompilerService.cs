using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphToConfigurationCompilerService
        : IGraphToConfigurationCompilerService
    {
        private readonly IGraphCompilerConfigurationService _configuration;
        private readonly IGraphCompilerBlueprintSyncService _blueprints;
        private readonly IGraphCompilerTileBuildLayerSyncService _buildLayers;
        private readonly IGraphCompilerPrecomputedMaskService _masks;
        private readonly IGraphCompilerModifierService _modifiers;
        private readonly IGraphCompilerObjectPlacementService _objects;

        public GraphToConfigurationCompilerService(
            IGraphCompilerConfigurationService configuration,
            IGraphCompilerBlueprintSyncService blueprints,
            IGraphCompilerTileBuildLayerSyncService buildLayers,
            IGraphCompilerPrecomputedMaskService masks,
            IGraphCompilerModifierService modifiers,
            IGraphCompilerObjectPlacementService objects)
        {
            _configuration = configuration;
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
            Vector2Int? mapSizeOverride = null,
            GraphEvaluationSnapshot evaluationSnapshot = null)
        {
            int effectiveSeed =
                GlobalSeed.Normalize(seed);

            using var randomScope =
                new GraphRandomScope(effectiveSeed);

            var result =
                new List<CompiledLayerMap>();

            Vector2Int requestedSize =
                _configuration.ResolveRequestedSize(
                    graph,
                    mapSizeOverride);

            if (graph == null
                || manager == null
                || manager.configuration == null)
            {
                return result;
            }

            Configuration config =
                manager.configuration;

            _configuration.Apply(
                graph,
                config,
                effectiveSeed,
                mapSizeOverride);

            var mapSize =
                new Vector2Int(
                    config.width,
                    config.height);

            if (!IsCompatibleSnapshot(
                    evaluationSnapshot,
                    graph,
                    effectiveSeed,
                    mapSize))
            {
                evaluationSnapshot =
                    GraphEvaluationPipeline.Evaluate(
                        graph,
                        effectiveSeed,
                        mapSize,
                        configureContext: context =>
                            context.RegisterService<
                                IGeneratorDataRegistry>(
                                new GeneratorDataRegistry()),
                        skippedLayerIds:
                            skippedLayerIds);
            }

            if (evaluationSnapshot == null
                || !evaluationSnapshot.Success)
            {
                Debug.LogError(
                    "[GraphToConfigurationCompiler] " +
                    "Graph evaluation failed; " +
                    "TWC blueprint/build-layer compilation was aborted. " +
                    (evaluationSnapshot
                         ?.ExecutionResult
                         ?.ErrorMessage
                     ?? evaluationSnapshot
                         ?.Diagnostics
                     ?? "Snapshot is missing."));

                return result;
            }

            GraphCompilerBlueprintSyncResult sync =
                _blueprints.Sync(
                    graph,
                    config,
                    skippedLayerIds);

            _buildLayers.Sync(
                graph,
                config,
                manager,
                sync,
                skippedLayerIds);

            Dictionary<string, bool[,]> masks =
                _masks.Build(
                    graph,
                    effectiveSeed,
                    mapSize,
                    skippedLayerIds,
                    evaluationSnapshot);

            CompileModifiers(graph, config, sync, masks);

            _blueprints.DisableUnused(
                sync.ExistingLayers,
                sync.UsedLayerGuids);

            var objectLayers =
                _objects.Collect(
                    graph,
                    effectiveSeed,
                    mapSize,
                    skippedLayerIds,
                    evaluationSnapshot);

            _objects.Apply(
                config,
                manager,
                objectLayers,
                sync.CompiledLayers);

            return sync.CompiledLayers;
        }

        private static bool IsCompatibleSnapshot(
            GraphEvaluationSnapshot snapshot,
            GraphAsset graph,
            int seed,
            Vector2Int mapSize)
        {
            return snapshot != null
                   && snapshot.IsCompatibleWith(
                       graph,
                       seed,
                       mapSize);
        }

        private void CompileModifiers(
            GraphAsset graph,
            Configuration config,
            GraphCompilerBlueprintSyncResult sync,
            IReadOnlyDictionary<string, bool[,]> masks)
        {
            foreach (var layerDef in sync.OrderedLayers)
            {
                if (!sync.BlueprintByGraphLayerId.TryGetValue(
                        layerDef.Id,
                        out var blueprint)
                    || blueprint == null)
                {
                    continue;
                }

                var plan =
                    TopologicalSorter.BuildPlan(
                        graph.CreateExecutionScope(
                            layerDef.Id));

                if (!plan.Success)
                    continue;

                _modifiers.Append(
                    blueprint,
                    layerDef.Id,
                    plan.NodesInExecutionOrder,
                    config,
                    sync.BlueprintGuidByGraphLayerId,
                    graph,
                    masks);
            }

        }
    }
}
