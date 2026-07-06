using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class TileWorldCreatorTerrainSideWallFeatureBindings
    {
        public static void Install(DiContainer container)
        {
            if (container.HasBinding<ITileWorldCreatorTerrainSideWallService>())
                return;

            container.Bind<ITileWorldCreatorTerrainSideWallMaterialService>().To<TileWorldCreatorTerrainSideWallMaterialService>().AsSingle();
            container.Bind<ITileWorldCreatorTerrainSideWallComponentService>().To<TileWorldCreatorTerrainSideWallComponentService>().AsSingle();
            container.Bind<ITileWorldCreatorTerrainSideWallEdgeAppender>().To<TileWorldCreatorTerrainSideWallEdgeAppender>().AsSingle();
            container.Bind<ITileWorldCreatorTerrainSideWallMeshBuilder>().To<TileWorldCreatorTerrainSideWallMeshBuilder>().AsSingle();
            container.Bind<ITileWorldCreatorTerrainSideWallDiagnostics>().To<TileWorldCreatorTerrainSideWallDiagnostics>().AsSingle();
            container.Bind<ITileWorldCreatorTerrainSideWallService>().To<TileWorldCreatorTerrainSideWallService>().AsSingle();
        }
    }
}
