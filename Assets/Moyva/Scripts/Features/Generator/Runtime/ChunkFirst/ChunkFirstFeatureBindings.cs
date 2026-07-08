using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal static class ChunkFirstFeatureBindings
    {
        public static void Install(DiContainer container)
        {
            if (container == null)
                return;

            container.Bind<ChunkFirstBuildDiagnostics>().AsSingle();
            container.Bind<ChunkFirstCellResolutionDebug>().AsSingle();
            container.Bind<ChunkFirstRuntimeMeshRegistry>().AsSingle();
            container.Bind<IChunkBuildAreaPlanner>().To<ChunkBuildAreaPlanner>().AsSingle();
            container.Bind<ITileNeighborhoodFactory>().To<TileNeighborhoodFactory>().AsSingle();
            container.Bind<ICompositionRuleTable>().To<DefaultCompositionRuleTable>().AsSingle();
            container.Bind<IResolvedTileCompositionResolver>().To<ResolvedTileCompositionResolver>().AsSingle();
            container.Bind<IResolvedTileMeshSource>().To<TwcTileMeshSourceProvider>().AsSingle();
            container.Bind<IChunkFirstTwcVisualCleanupService>().To<ChunkFirstTwcVisualCleanupService>().AsSingle();
            container.Bind<IChunkTerrainMeshBuilder>().To<ChunkTerrainMeshBuilder>().AsSingle();
            container.Bind<IChunkFirstObjectSpawner>().To<ChunkFirstObjectSpawner>().AsSingle();
            container.Bind<IChunkFirstWorldBuildService>().To<ChunkFirstWorldBuildService>().AsSingle();
        }
    }
}
