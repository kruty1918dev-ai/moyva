using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class GraphCompilerFeatureBindings
    {
        public static void Install(DiContainer container)
        {
            container.Bind<IGraphCompilerRuntimeContextFactory>().To<GraphCompilerRuntimeContextFactory>().AsSingle();
            container.Bind<IGraphCompilerMaskUtility>().To<GraphCompilerMaskUtility>().AsSingle();
            container.Bind<IGraphCompilerTileBuildLayerLookup>().To<GraphCompilerTileBuildLayerLookup>().AsSingle();
            container.Bind<IGraphCompilerConfigurationService>().To<GraphCompilerConfigurationService>().AsSingle();
            container.Bind<IGraphCompilerDiagnosticsService>().To<GraphCompilerDiagnosticsService>().AsSingle();
            container.Bind<IGraphCompilerBlueprintSyncService>().To<GraphCompilerBlueprintSyncService>().AsSingle();
            container.Bind<IGraphCompilerTileBuildLayerSyncService>().To<GraphCompilerTileBuildLayerSyncService>().AsSingle();
            container.Bind<IGraphCompilerPrecomputedMaskService>().To<GraphCompilerPrecomputedMaskService>().AsSingle();
            container.Bind<IGraphCompilerModifierService>().To<GraphCompilerModifierService>().AsSingle();
            container.Bind<IGraphCompilerObjectPlacementService>().To<GraphCompilerObjectPlacementService>().AsSingle();
            container.Bind<IGraphToConfigurationCompilerService>().To<GraphToConfigurationCompilerService>().AsSingle();
        }
    }
}
