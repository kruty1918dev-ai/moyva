using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal readonly struct TileWorldCreatorTerrainSideWallEdge
    {
        public TileWorldCreatorTerrainSideWallEdge(
            int cellX,
            int cellY,
            int neighbourX,
            int neighbourY,
            string direction,
            Vector3 start,
            Vector3 end)
        {
            CellX = cellX;
            CellY = cellY;
            NeighbourX = neighbourX;
            NeighbourY = neighbourY;
            Direction = direction;
            Start = start;
            End = end;
        }

        public int CellX { get; }
        public int CellY { get; }
        public int NeighbourX { get; }
        public int NeighbourY { get; }
        public string Direction { get; }
        public Vector3 Start { get; }
        public Vector3 End { get; }
    }
}
