using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class MapVisualFeatureBindings
    {
        public static void Install(DiContainer container)
        {
            if (!container.HasBinding<IGridProjection>())
                container.Bind<IGridProjection>().To<OrthogonalGridProjection>().AsSingle();

            container.Bind<IMapVisualWorldState>().To<MapVisualWorldState>().AsSingle();
            container.Bind<IMapVisualTileIdResolver>().To<MapVisualTileIdResolver>().AsSingle();
            container.Bind<IMapVisualGridWriter>().To<MapVisualGridWriter>().AsSingle();
            container.Bind<IMapVisualWorldDataFactory>().To<MapVisualWorldDataFactory>().AsSingle();
            container.Bind<IMapVisualWorldSignalPublisher>().To<MapVisualWorldSignalPublisher>().AsSingle();
            container.Bind<IMapVisualWorldBuildOrchestrator>().To<MapVisualWorldBuildOrchestrator>().AsSingle();
        }
    }
}
