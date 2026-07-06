namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class GraphLogicalTileMapServiceFactory
    {
        private static IGraphLogicalTileMapDiagnosticsService _defaultDiagnostics;

        public static IGraphLogicalTileMapBuilderService CreateBuilder()
        {
            return new GraphLogicalTileMapBuilderService(
                new GraphLogicalTileMapTwcLookup(),
                new GraphLogicalTileMapCellWriter());
        }

        public static IGraphLogicalTileMapDiagnosticsService CreateDiagnostics()
        {
            if (_defaultDiagnostics != null)
                return _defaultDiagnostics;

            var metrics = new GraphLogicalTileMapMetricsService();
            _defaultDiagnostics = new GraphLogicalTileMapDiagnosticsService(
                new GraphLogicalTileMapSnapshotFactory(metrics),
                new GraphLogicalTileMapReportFormatter());
            return _defaultDiagnostics;
        }
    }
}
