using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class TileWorldCreatorRuntimeMeshOptimizerFeatureBindings
    {
        public static void Install(DiContainer container)
        {
            if (container.HasBinding<ITileWorldCreatorRuntimeMeshOptimizerService>())
                return;

            container.Bind<ITileWorldCreatorClusterStatsService>().To<TileWorldCreatorClusterStatsService>().AsSingle();
            container.Bind<ITileWorldCreatorClusterCombineService>().To<TileWorldCreatorClusterCombineService>().AsSingle();
            container.Bind<TileWorldCreatorRuntimeMeshOptimizerDiagnostics>().AsSingle();
            container.Bind<ITileWorldCreatorRuntimeMeshOptimizerService>().To<TileWorldCreatorRuntimeMeshOptimizerService>().AsSingle();
        }
    }
}
