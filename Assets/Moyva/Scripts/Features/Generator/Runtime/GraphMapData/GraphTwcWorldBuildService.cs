using System.Diagnostics;
using GiantGrey.TileWorldCreator;

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
            var stopwatch = Stopwatch.StartNew();
            TileWorldCreatorLayerOcclusionOptimizer.GenerateCompleteMap(manager, _chunkSizeResolver.Resolve());
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
    }
}
