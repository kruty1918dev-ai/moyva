using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator
{
    internal static class GeneratorBindingGroups
    {
        public static void InstallGraphEvaluation(
            DiContainer container,
            GraphAsset graph,
            TileWorldCreatorManager twcManager,
            TileRegistrySO tileRegistry,
            MapObjectRegistrySO objectRegistry,
            Object errorContext)
        {
            if (!container.HasBinding<TileRegistrySO>())
                container.BindInstance(tileRegistry).AsSingle();
            container.Bind<IMapObjectRegistryService>()
                .FromInstance(new MapObjectRegistryService(objectRegistry))
                .AsSingle();
            container.Bind<IMapLayerRegistry>()
                .FromInstance(new MapLayerRegistry(tileRegistry, objectRegistry))
                .AsSingle();
            container.Bind<IMapObjectVisualRegistryService>()
                .To<MapObjectVisualRegistryService>()
                .AsSingle();
            container.Bind<IGeneratorDataRegistry>()
                .To<GeneratorDataRegistry>()
                .AsSingle();
            container.BindInterfacesAndSelfTo<GeneratorTerrainLevelService>()
                .AsSingle();
            container.BindInterfacesAndSelfTo<GeneratedTerrainLevelQueryService>()
                .AsSingle();
            container.Bind<ITileWorldCreatorTerrainBuildPolicyService>()
                .To<TileWorldCreatorTerrainBuildPolicyService>()
                .AsSingle();

            GraphLogicalTileMapFeatureBindings.Install(container);
            GraphCompilerFeatureBindings.Install(container);
            GraphTwcMapDataFeatureBindings.Install(container);
            MoyvaTwcGraphBindingFeatureBindings.Install(container);

            if (graph != null && twcManager != null)
            {
                container.Bind<IGraphTwcMapDataEnvironment>()
                    .FromInstance(new GraphTwcMapDataEnvironment(graph, twcManager))
                    .AsSingle();
                container.BindInterfacesAndSelfTo<GraphTwcMapDataState>()
                    .AsSingle();
                container.Bind<GraphTwcMapDataGenerator>().AsSingle();
                container.Bind<IMapDataGenerator>()
                    .To<GraphTwcMapDataGenerator>()
                    .FromResolve();
                return;
            }

            Debug.LogError(
                "[GeneratorInstaller] GraphAsset or TileWorldCreatorManager is " +
                "missing; map generation uses the disabled provider.",
                errorContext);
            container.Bind<IMapDataGenerator>()
                .To<DisabledMapDataGenerator>()
                .AsSingle();
        }

        public static void InstallWorldBuild(
            DiContainer container,
            TileWorldCreatorManager manager,
            TileWorldCreatorIdMappingSO mapping,
            TileWorldCreatorBuildOptions options)
        {
            TileWorldCreatorHeightProjectionFeatureBindings.Install(container);
            TileWorldCreatorTerrainSideWallFeatureBindings.Install(container);
            if (manager == null || mapping == null)
                return;

            var environment = new TileWorldCreatorBuildEnvironment(
                manager,
                mapping,
                options ?? new TileWorldCreatorBuildOptions());
            container.Bind<ITileWorldCreatorBuildEnvironment>()
                .FromInstance(environment)
                .AsSingle();
            ChunkFirstFeatureBindings.Install(container);
            container.Bind<ITileWorldCreatorBlueprintLayerResolver>()
                .To<TileWorldCreatorBlueprintLayerResolver>().AsSingle();
            container.Bind<ITileWorldCreatorTerrainLevelMapService>()
                .To<TileWorldCreatorTerrainLevelMapService>().AsSingle();
            container.Bind<ITileWorldCreatorShoreBandService>()
                .To<TileWorldCreatorShoreBandService>().AsSingle();
            container.Bind<ITileWorldCreatorConfigurationPreparationService>()
                .To<TileWorldCreatorConfigurationPreparationService>().AsSingle();
            container.Bind<ITileWorldCreatorTerrainBuildLayerConfigurationService>()
                .To<TileWorldCreatorTerrainBuildLayerConfigurationService>().AsSingle();
            container.Bind<ITileWorldCreatorLayerPositionCollector>()
                .To<TileWorldCreatorLayerPositionCollector>().AsSingle();
            container.Bind<ITileWorldCreatorLayerPositionApplier>()
                .To<TileWorldCreatorLayerPositionApplier>().AsSingle();
            container.Bind<ITileWorldCreatorBuildExecutionService>()
                .To<TileWorldCreatorBuildExecutionService>().AsSingle();
            container.Bind<ITileWorldCreatorTerrainBaseHeightResolver>()
                .To<TileWorldCreatorTerrainBaseHeightResolver>().AsSingle();
            container.Bind<ITileWorldCreatorTerrainVisualPostProcessor>()
                .To<TileWorldCreatorTerrainVisualPostProcessor>().AsSingle();
            container.Bind<ITileWorldCreatorTerrainHeightPublisher>()
                .To<TileWorldCreatorTerrainHeightPublisher>().AsSingle();
            container.Bind<ITileWorldCreatorWorldBuildBridge>()
                .To<TileWorldCreatorWorldBuildBridge>().AsSingle();
        }

        public static void InstallPresentation(
            DiContainer container,
            WaterLayerMaterialSettings waterSettings)
        {
            MapVisualFeatureBindings.Install(container);
            WaterLayerMaterialFeatureBindings.Install(container);
            MenuWorldPreviewKingdomPlacementFeatureBindings.Install(container);
            MenuWorldPreviewTextureBuilderFeatureBindings.Install(container);

            if (waterSettings != null)
            {
                container.BindInstance(waterSettings).AsSingle();
                container.Bind<IWaterLayerMaterialSettings>()
                    .FromInstance(waterSettings)
                    .AsSingle();
            }

            container.BindInterfacesAndSelfTo<MapVisualInstantiator>()
                .AsSingle()
                .NonLazy();
        }

        public static void InstallStartup(DiContainer container)
        {
            container.BindInterfacesAndSelfTo<GeneratedWorldSaveModule>()
                .AsSingle();
            container.BindInterfacesTo<SaveModuleRegistrar<GeneratedWorldSaveModule>>()
                .AsSingle()
                .NonLazy();
            container.BindInterfacesAndSelfTo<GeneratorWorldStartupBuilder>()
                .AsSingle()
                .NonLazy();
            container.BindExecutionOrder<GeneratorWorldStartupBuilder>(105);
        }
    }
}
