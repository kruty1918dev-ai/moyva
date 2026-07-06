using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class GraphLogicalTileMapFeatureBindings
    {
        public static void Install(DiContainer container)
        {
            container.Bind<IGraphLogicalTileMapTwcLookup>().To<GraphLogicalTileMapTwcLookup>().AsSingle();
            container.Bind<IGraphLogicalTileMapCellWriter>().To<GraphLogicalTileMapCellWriter>().AsSingle();
            container.Bind<IGraphLogicalTileMapBuilderService>().To<GraphLogicalTileMapBuilderService>().AsSingle();
            container.Bind<IGraphLogicalTileMapMetricsService>().To<GraphLogicalTileMapMetricsService>().AsSingle();
            container.Bind<IGraphLogicalTileMapSnapshotFactory>().To<GraphLogicalTileMapSnapshotFactory>().AsSingle();
            container.Bind<IGraphLogicalTileMapReportFormatter>().To<GraphLogicalTileMapReportFormatter>().AsSingle();
            container.Bind<IGraphLogicalTileMapDiagnosticsService>().To<GraphLogicalTileMapDiagnosticsService>().AsSingle();
        }
    }
}
