using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.API
{
    public interface IMapVisualChunkRootService
    {
        Transform GetOrCreateRoot(MapChunkCoord coord);
        bool IsChunkRoot(Transform transform);
    }
}
