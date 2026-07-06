namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class GraphGenerationLayerLogFactory
    {
        public static IGraphGenerationLayerLogService CreateDefault()
        {
            var issues = new GraphGenerationLayerIssueService();
            var twcLookup = new GraphGenerationLayerTwcLookup();
            var status = new GraphGenerationLayerStatusService(issues);
            var analyzer = new GraphGenerationLayerAnalyzer(twcLookup, status);
            var builder = new GraphGenerationLayerReportBuilder(issues, analyzer);
            return new GraphGenerationLayerLogService(builder);
        }
    }
}
