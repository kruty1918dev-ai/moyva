using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.API
{
    public interface IMapChunkLayoutService
    {
        bool IsConfigured { get; }
        int Width { get; }
        int Height { get; }
        int ChunkSize { get; }
        float CellSize { get; }
        IReadOnlyList<MapChunkDescriptor> Chunks { get; }

        void Configure(int width, int height, float cellSize, bool hasWorldBounds, Bounds worldBounds);
        bool TryGetChunkCoord(Vector2Int tile, out MapChunkCoord coord);
        bool TryGetDescriptor(MapChunkCoord coord, out MapChunkDescriptor descriptor);
        int GetChunksOverlapping(Bounds worldBounds, List<MapChunkCoord> results);
    }
}
