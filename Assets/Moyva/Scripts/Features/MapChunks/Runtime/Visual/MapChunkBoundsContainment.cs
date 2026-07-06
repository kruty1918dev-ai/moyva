using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    internal static class MapChunkBoundsContainment
    {
        public static bool ContainsRendererXZ(IMapChunkLayoutService layout, Bounds rendererBounds, MapChunkCoord coord)
        {
            if (layout == null || !layout.TryGetDescriptor(coord, out MapChunkDescriptor descriptor))
                return false;

            float tolerance = Mathf.Max(0.001f, layout.CellSize * 0.25f);
            Bounds chunkBounds = descriptor.WorldBounds;
            return rendererBounds.min.x >= chunkBounds.min.x - tolerance
                && rendererBounds.max.x <= chunkBounds.max.x + tolerance
                && rendererBounds.min.z >= chunkBounds.min.z - tolerance
                && rendererBounds.max.z <= chunkBounds.max.z + tolerance;
        }
    }
}
