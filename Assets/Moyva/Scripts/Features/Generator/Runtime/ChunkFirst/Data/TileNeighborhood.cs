namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal readonly struct TileNeighborhood
    {
        public TileNeighborhood(
            TileStackCell center,
            TileStackCell north,
            TileStackCell east,
            TileStackCell south,
            TileStackCell west,
            TileStackCell northEast,
            TileStackCell southEast,
            TileStackCell southWest,
            TileStackCell northWest)
        {
            Center = center;
            North = north;
            East = east;
            South = south;
            West = west;
            NorthEast = northEast;
            SouthEast = southEast;
            SouthWest = southWest;
            NorthWest = northWest;
        }

        public TileStackCell Center { get; }
        public TileStackCell North { get; }
        public TileStackCell East { get; }
        public TileStackCell South { get; }
        public TileStackCell West { get; }
        public TileStackCell NorthEast { get; }
        public TileStackCell SouthEast { get; }
        public TileStackCell SouthWest { get; }
        public TileStackCell NorthWest { get; }
    }
}
