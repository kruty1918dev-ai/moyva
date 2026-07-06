using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class GraphGenerationLayerLogFeatureBindings
    {
        public static void Install(DiContainer container)
        {
            container.Bind<IGraphGenerationLayerIssueService>().To<GraphGenerationLayerIssueService>().AsSingle();
            container.Bind<IGraphGenerationLayerTwcLookup>().To<GraphGenerationLayerTwcLookup>().AsSingle();
            container.Bind<IGraphGenerationLayerStatusService>().To<GraphGenerationLayerStatusService>().AsSingle();
            container.Bind<IGraphGenerationLayerAnalyzer>().To<GraphGenerationLayerAnalyzer>().AsSingle();
            container.Bind<IGraphGenerationLayerReportBuilder>().To<GraphGenerationLayerReportBuilder>().AsSingle();
            container.Bind<IGraphGenerationLayerLogService>().To<GraphGenerationLayerLogService>().AsSingle();
        }
    }
}
