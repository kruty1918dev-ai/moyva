using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.API
{
    public interface IMapVisualChunkRootService
    {
        Transform GetOrCreateRoot(MapChunkCoord coord);
        bool IsChunkRoot(Transform transform);
        /// <summary>
        /// Resolves the chunk owning a descendant of a chunk root
        /// (MapChunk_X_Y). False for content outside the chunk hierarchy.
        /// </summary>
        bool TryGetOwnedChunk(Transform transform, out MapChunkCoord coord);
    }
}
