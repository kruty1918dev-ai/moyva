namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphLogicalTileMapReportFormatter
    {
        string BuildSummary(GraphLogicalTileMapSnapshot snapshot);
        string BuildComparison(GraphLogicalTileMapSnapshot current, GraphLogicalTileMapSnapshot other, out int mismatchCount);
    }
}
