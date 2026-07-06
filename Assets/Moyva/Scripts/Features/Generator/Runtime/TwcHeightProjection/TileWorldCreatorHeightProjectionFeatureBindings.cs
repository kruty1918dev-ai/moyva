using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class TileWorldCreatorHeightProjectionFeatureBindings
    {
        public static void Install(DiContainer container)
        {
            if (container.HasBinding<ITileWorldCreatorHeightProjectionService>())
                return;

            container.Bind<ITileWorldCreatorHeightProjectionDiagnostics>().To<TileWorldCreatorHeightProjectionDiagnostics>().AsSingle();
            container.Bind<ITileWorldCreatorTileTransformCollector>().To<TileWorldCreatorTileTransformCollector>().AsSingle();
            container.Bind<ITileWorldCreatorHeightProjectionOffsetService>().To<TileWorldCreatorHeightProjectionOffsetService>().AsSingle();
            container.Bind<ITileWorldCreatorHeightProjectionApplier>().To<TileWorldCreatorHeightProjectionApplier>().AsSingle();
            container.Bind<ITileWorldCreatorHeightProjectionStableActionService>().To<TileWorldCreatorHeightProjectionStableActionService>().AsSingle();
            container.Bind<ITileWorldCreatorHeightProjectionService>().To<TileWorldCreatorHeightProjectionService>().AsSingle();
            TileWorldCreatorRuntimeMeshOptimizerFeatureBindings.Install(container);
        }
    }
}
