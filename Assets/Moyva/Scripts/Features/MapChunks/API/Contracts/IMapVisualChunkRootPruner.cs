using System.Collections.Generic;

namespace Kruty1918.Moyva.MapChunks.API
{
    public interface IMapVisualChunkRootPruner
    {
        int RemoveRootsOutside(IReadOnlyCollection<MapChunkCoord> validCoords);
    }
}
