using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal readonly struct ChunkBuildArea
    {
        public ChunkBuildArea(MapChunkCoord coord, RectInt coreRect, RectInt sampleRect, float cellSize = 1f)
        {
            Coord = coord;
            CoreRect = coreRect;
            SampleRect = sampleRect;
            CellSize = Mathf.Max(0.0001f, cellSize);
        }

        public MapChunkCoord Coord { get; }
        public RectInt CoreRect { get; }
        public RectInt SampleRect { get; }
        public float CellSize { get; }
    }
}
