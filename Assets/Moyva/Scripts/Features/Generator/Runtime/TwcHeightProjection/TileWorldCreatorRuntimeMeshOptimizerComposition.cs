namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class TileWorldCreatorRuntimeMeshOptimizerComposition
    {
        public static ITileWorldCreatorRuntimeMeshOptimizerService Create()
        {
            var stats = new TileWorldCreatorClusterStatsService();
            var combiner = new TileWorldCreatorClusterCombineService(stats);
            var diagnostics = new TileWorldCreatorRuntimeMeshOptimizerDiagnostics();
            return new TileWorldCreatorRuntimeMeshOptimizerService(stats, combiner, diagnostics);
        }
    }
}
