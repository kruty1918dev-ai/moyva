using System.Diagnostics;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphTwcWorldBuildService
    {
        long Build(TileWorldCreatorManager manager);
    }

    internal sealed class GraphTwcWorldBuildService : IGraphTwcWorldBuildService
    {
        private readonly IGraphTwcChunkSizeResolver _chunkSizeResolver;

        public GraphTwcWorldBuildService(IGraphTwcChunkSizeResolver chunkSizeResolver)
        {
            _chunkSizeResolver = chunkSizeResolver;
        }

        public long Build(TileWorldCreatorManager manager)
        {
            if (TileWorldCreatorChunkFirstGuard.IsActive)
            {
                UnityEngine.Debug.LogError("[MoyvaChunkFirst] ExecuteBuildLayers path reached through GraphTwcWorldBuildService during chunk-first mode.");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                throw new System.InvalidOperationException("TWC visual build is forbidden during chunk-first generation.");
#endif
            }

            var stopwatch = Stopwatch.StartNew();
            TileWorldCreatorLayerOcclusionOptimizer.GenerateCompleteMap(manager, _chunkSizeResolver.Resolve());
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
    }
}
