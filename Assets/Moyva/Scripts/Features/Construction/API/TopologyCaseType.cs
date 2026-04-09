namespace Kruty1918.Moyva.Construction.API
{
    /// <summary>
    /// Топологічні випадки для автоматичного підбору варіанту об'єкта по сусідах.
    /// </summary>
    public enum TopologyCaseType
    {
        Isolated = 0,

        CrossIntersection,

        TJunctionOpenNorth,
        TJunctionOpenEast,
        TJunctionOpenSouth,
        TJunctionOpenWest,

        CornerNorthEast,
        CornerNorthWest,
        CornerSouthEast,
        CornerSouthWest,

        Vertical,
        VerticalLeft,
        VerticalRight,

        Horizontal,
        HorizontalTop,
        HorizontalBottom,

        EndNorth,
        EndEast,
        EndSouth,
        EndWest,

        DiagonalNorthEastSouthWest,
        DiagonalNorthWestSouthEast,
    }
}
