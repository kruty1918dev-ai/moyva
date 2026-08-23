using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal static class ConstructionVisualBindingsInstaller
    {
        public static void Install(DiContainer container)
        {
            container.Bind<BuildModeGridStateController>().AsSingle();
            container.Bind<ConstructionVisualStyleService>().To<ConstructionVisualStyleService>().AsSingle();
            container.BindInterfacesTo<ConstructionGridGeometryService>().AsSingle();
            container.Bind<ConstructionTileSurfaceOffsetService>().To<ConstructionTileSurfaceOffsetService>().AsSingle();
            container.Bind<ConstructionVisualBoundsAlignmentService>().To<ConstructionVisualBoundsAlignmentService>().AsSingle();
            container.Bind<ConstructionTerrainAlignmentService>().To<ConstructionTerrainAlignmentService>().AsSingle();
            container.Bind<ConstructionVisualFactory>().To<ConstructionVisualFactory>().AsSingle();
            container.Bind<ConstructionVisualRootService>().To<ConstructionVisualRootService>().AsSingle();
            container.Bind<ConstructionPreviewVisualService>().To<ConstructionPreviewVisualService>().AsSingle();
            container.Bind<ConstructionPlacedVisualService>().To<ConstructionPlacedVisualService>().AsSingle();
            container.Bind<IConstructionPlacedVisualLookup>()
                .FromResolveGetter<ConstructionPlacedVisualService>(service => (IConstructionPlacedVisualLookup)service)
                .AsCached();
            container.Bind<ConstructionWallVisualRefreshService>().To<ConstructionWallVisualRefreshService>().AsSingle();
            container.Bind<ConstructionPreviewVisualSignalHandler>().To<ConstructionPreviewVisualSignalHandler>().AsSingle();
            container.Bind<ConstructionPlacedVisualSignalHandler>().To<ConstructionPlacedVisualSignalHandler>().AsSingle();
            container.Bind<ConstructionInfluenceMeshOverlayRenderer>().To<ConstructionInfluenceMeshOverlayRenderer>().AsSingle();
            container.Bind<ConstructionRadiusVisualObjectFactory>().To<ConstructionRadiusVisualObjectFactory>().AsSingle();
            container.Bind<ConstructionInfluenceRadiusVisualService>().To<ConstructionInfluenceRadiusVisualService>().AsSingle();
            container.Bind<ConstructionBuildGridTileFilter>().To<ConstructionBuildGridTileFilter>().AsSingle();
            container.Bind<ConstructionBuildGridTileCollector>().To<ConstructionBuildGridTileCollector>().AsSingle();
            container.Bind<ConstructionBuildGridDiagnostics>().To<ConstructionBuildGridDiagnostics>().AsSingle();
            container.Bind<ConstructionBuildGridOverlayRenderer>().To<ConstructionBuildGridOverlayRenderer>().AsSingle();
            container.Bind<ConstructionBuildGridChunkSurfaceBuilder>().To<ConstructionBuildGridChunkSurfaceBuilder>().AsSingle();
            container.Bind<ConstructionBuildGridChunkSurfaceService>().To<ConstructionBuildGridChunkSurfaceService>().AsSingle();
            container.BindInterfacesAndSelfTo<ConstructionBuildGridOverlayService>().AsSingle();
            container.Bind<ConstructionBlockedFlashService>().To<ConstructionBlockedFlashService>().AsSingle();
        }
    }
}
