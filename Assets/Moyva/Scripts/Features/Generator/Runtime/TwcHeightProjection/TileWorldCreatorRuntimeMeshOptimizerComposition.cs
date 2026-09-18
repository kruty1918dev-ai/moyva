namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class TileWorldCreatorRuntimeMeshOptimizerComposition
    {
        public static ITileWorldCreatorRuntimeMeshOptimizerService Create()
        {
            var stats = new TileWorldCreatorClusterStatsService();
            var combiner = new TileWorldCreatorClusterCombineService(stats);
            return new TileWorldCreatorRuntimeMeshOptimizerService(stats, combiner);
        }
    }
}
