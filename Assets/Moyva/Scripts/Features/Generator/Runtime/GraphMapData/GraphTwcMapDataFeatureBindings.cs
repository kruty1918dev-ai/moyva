using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class GraphTwcMapDataFeatureBindings
    {
        public static void Install(DiContainer container)
        {
            container.Bind<IGraphTwcSeedService>().To<GraphTwcSeedService>().AsSingle();
            container.Bind<IGraphTwcMapSizeResolver>().To<GraphTwcMapSizeResolver>().AsSingle();
            container.Bind<IGraphTwcValidationService>().To<GraphTwcValidationService>().AsSingle();
            container.Bind<IGraphTwcLogicalMapExportService>().To<GraphTwcLogicalMapExportService>().AsSingle();
            container.Bind<IGraphTwcTerrainHeightPublisher>().To<GraphTwcTerrainHeightPublisher>().AsSingle();
            container.Bind<IGraphTwcEmptyMapFactory>().To<GraphTwcEmptyMapFactory>().AsSingle();
            container.Bind<IGraphTwcMapGenerationPipeline>().To<GraphTwcMapGenerationPipeline>().AsSingle();
        }
    }
}
