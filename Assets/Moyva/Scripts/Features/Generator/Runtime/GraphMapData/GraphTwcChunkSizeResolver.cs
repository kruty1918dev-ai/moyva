using Kruty1918.Moyva.MapChunks.API;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphTwcChunkSizeResolver
    {
        int Resolve();
    }

    internal sealed class GraphTwcChunkSizeResolver : IGraphTwcChunkSizeResolver
    {
        private readonly IMapChunkSettingsProvider _chunkSettings;

        public GraphTwcChunkSizeResolver([InjectOptional] IMapChunkSettingsProvider chunkSettings = null)
        {
            _chunkSettings = chunkSettings;
        }

        public int Resolve()
        {
            return _chunkSettings?.ChunkSize ?? TileWorldCreatorChunkBatchingUtility.ResolveSceneChunkSize();
        }
    }
}
