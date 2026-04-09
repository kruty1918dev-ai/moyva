namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Маска зайнятості 8 сусідів навколо центральної клітинки.
    /// </summary>
    public readonly struct TopologyNeighborMask
    {
        public TopologyNeighborMask(
            bool north,
            bool northEast,
            bool east,
            bool southEast,
            bool south,
            bool southWest,
            bool west,
            bool northWest)
        {
            North = north;
            NorthEast = northEast;
            East = east;
            SouthEast = southEast;
            South = south;
            SouthWest = southWest;
            West = west;
            NorthWest = northWest;
        }

        public bool North { get; }
        public bool NorthEast { get; }
        public bool East { get; }
        public bool SouthEast { get; }
        public bool South { get; }
        public bool SouthWest { get; }
        public bool West { get; }
        public bool NorthWest { get; }

        public int CardinalCount =>
            (North ? 1 : 0) +
            (East ? 1 : 0) +
            (South ? 1 : 0) +
            (West ? 1 : 0);
    }
}
