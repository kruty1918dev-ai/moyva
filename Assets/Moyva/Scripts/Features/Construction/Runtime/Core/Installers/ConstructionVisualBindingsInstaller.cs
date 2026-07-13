
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal static class ConstructionVisualBindingsInstaller
    {
        public static void Install(DiContainer container)
        {
            container.Bind<BuildModeGridStateController>().AsSingle();
            container.Bind<IConstructionVisualStyleService>().To<ConstructionVisualStyleService>().AsSingle();
            container.BindInterfacesTo<ConstructionGridGeometryService>().AsSingle();
            container.Bind<IConstructionTileSurfaceOffsetService>().To<ConstructionTileSurfaceOffsetService>().AsSingle();
            container.Bind<IConstructionVisualBoundsAlignmentService>().To<ConstructionVisualBoundsAlignmentService>().AsSingle();
            container.Bind<IConstructionTerrainAlignmentService>().To<ConstructionTerrainAlignmentService>().AsSingle();
            container.Bind<IConstructionVisualFactory>().To<ConstructionVisualFactory>().AsSingle();
            container.Bind<IConstructionVisualRootService>().To<ConstructionVisualRootService>().AsSingle();
            container.Bind<IConstructionPreviewVisualService>().To<ConstructionPreviewVisualService>().AsSingle();
            container.Bind<IConstructionPlacedVisualService>().To<ConstructionPlacedVisualService>().AsSingle();
            container.Bind<IConstructionPlacedVisualLookup>()
                .FromResolveGetter<IConstructionPlacedVisualService>(service => (IConstructionPlacedVisualLookup)service)
                .AsCached();
            container.Bind<IConstructionWallVisualRefreshService>().To<ConstructionWallVisualRefreshService>().AsSingle();
            container.Bind<IConstructionPreviewVisualSignalHandler>().To<ConstructionPreviewVisualSignalHandler>().AsSingle();
            container.Bind<IConstructionPlacedVisualSignalHandler>().To<ConstructionPlacedVisualSignalHandler>().AsSingle();
            container.Bind<IConstructionInfluenceMeshOverlayRenderer>().To<ConstructionInfluenceMeshOverlayRenderer>().AsSingle();
            container.Bind<IConstructionRadiusVisualObjectFactory>().To<ConstructionRadiusVisualObjectFactory>().AsSingle();
            container.Bind<IConstructionInfluenceRadiusVisualService>().To<ConstructionInfluenceRadiusVisualService>().AsSingle();
            container.Bind<IConstructionBuildGridTileFilter>().To<ConstructionBuildGridTileFilter>().AsSingle();
            container.Bind<IConstructionBuildGridTileCollector>().To<ConstructionBuildGridTileCollector>().AsSingle();
            container.Bind<IConstructionBuildGridDiagnostics>().To<ConstructionBuildGridDiagnostics>().AsSingle();
            container.Bind<IConstructionBuildGridOverlayRenderer>().To<ConstructionBuildGridOverlayRenderer>().AsSingle();
            container.Bind<IConstructionBuildGridChunkSurfaceBuilder>().To<ConstructionBuildGridChunkSurfaceBuilder>().AsSingle();
            container.Bind<IConstructionBuildGridChunkSurfaceService>().To<ConstructionBuildGridChunkSurfaceService>().AsSingle();
            container.Bind<IConstructionBuildGridOverlayService>().To<ConstructionBuildGridOverlayService>().AsSingle();
            container.Bind<IConstructionBlockedFlashService>().To<ConstructionBlockedFlashService>().AsSingle();
        }
    }
}
