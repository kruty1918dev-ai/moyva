using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Compatibility facade for legacy editor/test call sites.
    /// Runtime code should receive <see cref="IGraphToConfigurationCompilerService"/> from Zenject.
    /// </summary>
    public static class GraphToConfigurationCompiler
    {
        public static IGraphToConfigurationCompilerService CreateDefaultService()
        {
            return GraphCompilerServiceFactory.CreateDefault();
        }

        public static List<CompiledLayerMap> Compile(
            GraphAsset graph,
            TileWorldCreatorManager manager,
            int seed,
            ISet<string> skippedLayerIds = null)
        {
            return Compile(graph, manager, seed, skippedLayerIds, null);
        }

        public static List<CompiledLayerMap> Compile(
            GraphAsset graph,
            TileWorldCreatorManager manager,
            int seed,
            ISet<string> skippedLayerIds,
            Vector2Int? mapSizeOverride)
        {
            return Compile(
                graph,
                manager,
                seed,
                skippedLayerIds,
                mapSizeOverride,
                null);
        }

        public static List<CompiledLayerMap> Compile(
            GraphAsset graph,
            TileWorldCreatorManager manager,
            int seed,
            ISet<string> skippedLayerIds,
            Vector2Int? mapSizeOverride,
            GraphEvaluationSnapshot evaluationSnapshot)
        {
            return CreateDefaultService().Compile(
                graph,
                manager,
                seed,
                skippedLayerIds,
                mapSizeOverride,
                evaluationSnapshot);
        }
    }
}
