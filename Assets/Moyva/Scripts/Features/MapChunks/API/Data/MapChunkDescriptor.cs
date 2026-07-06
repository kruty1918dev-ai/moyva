using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.API
{
    public readonly struct MapChunkDescriptor
    {
        public MapChunkDescriptor(
            MapChunkCoord coord,
            RectInt tileRect,
            Bounds worldBounds)
        {
            Coord = coord;
            TileRect = tileRect;
            WorldBounds = worldBounds;
        }

        public MapChunkCoord Coord { get; }
        public RectInt TileRect { get; }
        public Bounds WorldBounds { get; }

        public int TileCount => Mathf.Max(0, TileRect.width) * Mathf.Max(0, TileRect.height);
    }
}
