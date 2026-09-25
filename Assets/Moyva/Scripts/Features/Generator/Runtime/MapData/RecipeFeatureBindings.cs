using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Installs the recipe-driven map-generation feature: mask evaluation is a
    /// static utility, while the compiler, logical-map export and pipeline are
    /// container-bound services.
    /// </summary>
    internal static class RecipeFeatureBindings
    {
        public static void Install(DiContainer container)
        {
            container.Bind<ILogicalTileMapTwcLookup>().To<LogicalTileMapTwcLookup>().AsSingle();
            container.Bind<ILogicalTileMapCellWriter>().To<LogicalTileMapCellWriter>().AsSingle();
            container.Bind<ILogicalTileMapBuilderService>().To<LogicalTileMapBuilderService>().AsSingle();

            container.Bind<IRecipeToConfigurationCompilerService>()
                .To<RecipeToConfigurationCompilerService>().AsSingle();

            container.Bind<IMapSeedService>().To<MapSeedService>().AsSingle();
            container.Bind<IMapSizeResolver>().To<MapSizeResolver>().AsSingle();
            container.Bind<IMapRecipeValidationService>().To<MapRecipeValidationService>().AsSingle();
            container.Bind<IMapLogicalMapExportService>().To<MapLogicalMapExportService>().AsSingle();
            container.Bind<ITerrainHeightPublisher>().To<TerrainHeightPublisher>().AsSingle();
            container.Bind<IEmptyMapFactory>().To<EmptyMapFactory>().AsSingle();
            container.Bind<IMapGenerationPipeline>().To<MapGenerationPipeline>().AsSingle();

            container.BindInterfacesAndSelfTo<TerrainPassageStore>()
                .AsSingle();
            container.BindInterfacesAndSelfTo<RecipeHydrologyStore>()
                .AsSingle();
            container.Bind<ITerrainReliefFieldPlanner>().To<TerrainReliefPlanner>().AsSingle();
            container.Bind<ITerrainPassagePlanner>().To<TerrainPassagePlanner>().AsSingle();
            container.Bind<ITerrainRoutePlanner>().To<TerrainRoutePlanner>().AsSingle();
            container.Bind<ITerrainShorePlanner>().To<TerrainShorePlanner>().AsSingle();
            container.Bind<ITerrainPlanApplier>().To<TerrainPlanApplicationService>().AsSingle();
        }
    }
}
