using System.Collections.Generic;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphLogicalTileMapSnapshot
    {
        public string Source;
        public string GraphName;
        public int Seed;
        public int Width;
        public int Height;
        public ulong LayerHash;
        public ulong TileHash;
        public int EmptyCount;
        public int OccupiedCount;
        public int SameValueRegions;
        public int EmptyRegions;
        public int EdgeTransitions;
        public string[,] LayerGrid;
        public string[,] TileGrid;
        public Dictionary<string, int> LayerCounts;
        public Dictionary<string, int> TileCounts;
    }
}
