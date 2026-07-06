using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class GraphGenerationLayerLog
    {
        public const string Tag = "[MoyvaGraphGenerationLayers]";

        public static void Emit(
            string source,
            GraphAsset graph,
            TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiledLayers,
            GraphValidationReport validationReport,
            ISet<string> skippedLayerIds,
            int seed,
            Vector2Int mapSize,
            bool buildLayersWereGenerated,
            Object context = null)
        {
            CreateDefaultService().Emit(CreateRequest(source, graph, manager, compiledLayers, validationReport,
                skippedLayerIds, seed, mapSize, buildLayersWereGenerated, context));
        }

        public static string Build(
            string source,
            GraphAsset graph,
            TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiledLayers,
            GraphValidationReport validationReport,
            ISet<string> skippedLayerIds,
            int seed,
            Vector2Int mapSize,
            bool buildLayersWereGenerated)
        {
            return CreateDefaultService().Build(CreateRequest(source, graph, manager, compiledLayers,
                validationReport, skippedLayerIds, seed, mapSize, buildLayersWereGenerated));
        }

        public static IGraphGenerationLayerLogService CreateDefaultService()
        {
            return GraphGenerationLayerLogFactory.CreateDefault();
        }

        private static GraphGenerationLayerLogRequest CreateRequest(
            string source,
            GraphAsset graph,
            TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiledLayers,
            GraphValidationReport validationReport,
            ISet<string> skippedLayerIds,
            int seed,
            Vector2Int mapSize,
            bool buildLayersWereGenerated,
            Object context = null)
        {
            return new GraphGenerationLayerLogRequest(source, graph, manager, compiledLayers, validationReport,
                skippedLayerIds, seed, mapSize, buildLayersWereGenerated, context);
        }
    }
}
