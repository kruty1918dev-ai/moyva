using System.Collections.Generic;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal sealed class ChunkBuildAreaPlanner : IChunkBuildAreaPlanner
    {
        private readonly IMapChunkLayoutService _layout;
        private readonly List<ChunkBuildArea> _areas = new List<ChunkBuildArea>(64);

        public ChunkBuildAreaPlanner(IMapChunkLayoutService layout)
        {
            _layout = layout;
        }

        public IReadOnlyList<ChunkBuildArea> Build(int width, int height, float cellSize, bool hasWorldBounds, Bounds worldBounds, int halo)
        {
            int safeWidth = Mathf.Max(1, width);
            int safeHeight = Mathf.Max(1, height);
            int safeHalo = Mathf.Max(0, halo);

            _layout.Configure(safeWidth, safeHeight, Mathf.Max(0.0001f, cellSize), hasWorldBounds, worldBounds);
            _areas.Clear();

            var chunks = _layout.Chunks;
            for (int i = 0; i < chunks.Count; i++)
            {
                RectInt core = chunks[i].TileRect;
                RectInt sample = Expand(core, safeHalo, safeWidth, safeHeight);
                _areas.Add(new ChunkBuildArea(chunks[i].Coord, core, sample, Mathf.Max(0.0001f, cellSize)));
            }

            return _areas;
        }

        private static RectInt Expand(RectInt rect, int halo, int mapWidth, int mapHeight)
        {
            int xMin = Mathf.Max(0, rect.xMin - halo);
            int yMin = Mathf.Max(0, rect.yMin - halo);
            int xMax = Mathf.Min(mapWidth, rect.xMax + halo);
            int yMax = Mathf.Min(mapHeight, rect.yMax + halo);
            return new RectInt(xMin, yMin, Mathf.Max(0, xMax - xMin), Mathf.Max(0, yMax - yMin));
        }
    }
}
