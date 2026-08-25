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
        }
    }
}
