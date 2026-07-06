using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.MapChunks.Runtime;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator
{
    /// <summary>
    /// Restores the gameplay world-generation bindings used by the scene.
    /// The scene already references this script GUID, so keep this class in this namespace.
    /// </summary>
    public sealed class GeneratorInstaller : MonoInstaller
    {
        private const string GeneratorBootDiagTag = "[MoyvaGeneratorBootDiag]";
        private IWorldGenerationDiagnostics _worldDiagnostics;

        [Header("Scene Graph Source")]
        [SerializeField] private MoyvaTileWorldCreatorGraphBinding _graphBinding;
        [SerializeField] private TileWorldCreatorManager _tileWorldCreatorManager;
        [SerializeField] private GraphAsset _graphAsset;

        [Header("Registries")]
        [SerializeField] private TileRegistrySO _tileRegistry;
        [SerializeField] private MapObjectRegistrySO _mapObjectRegistry;
        [SerializeField] private TileWorldCreatorIdMappingSO _tileWorldCreatorMapping;

        [Header("TWC Runtime")]
        [SerializeField] private TileWorldCreatorBuildOptions _tileWorldCreatorBuildOptions = new TileWorldCreatorBuildOptions();
        [SerializeField] private WaterLayerMaterialSettings _waterLayerMaterialSettings;

        [Inject]
        public void Construct([InjectOptional] IWorldGenerationDiagnostics worldDiagnostics = null)
        {
            _worldDiagnostics = worldDiagnostics;
        }

        public override void InstallBindings()
        {
            Debug.Log(
                $"{GeneratorBootDiagTag} GeneratorInstaller.InstallBindings ENTER scene={gameObject.scene.name}, " +
                $"mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}");
            ResolveSceneReferences();
            MapChunkFeatureBindings.Install(Container);
            _worldDiagnostics?.GeneratorInstallerInstalled(
                $"scene={gameObject.scene.name}, graph={(_graphAsset != null ? _graphAsset.name : "null")}, twc={_tileWorldCreatorManager != null}");
            Debug.Log(
                $"[MoyvaWorldGenDiag] GeneratorInstaller.InstallBindings scene={gameObject.scene.name}, " +
                $"hasGraphBinding={_graphBinding != null}, hasTwcManager={_tileWorldCreatorManager != null}, graphAsset={(_graphAsset != null ? _graphAsset.name : "null")}, " +
                $"hasTileRegistry={_tileRegistry != null}, hasObjectRegistry={_mapObjectRegistry != null}, hasTwcMapping={_tileWorldCreatorMapping != null}.");

            var tileRegistry = ResolveTileRegistry();
            var mapObjectRegistry = _mapObjectRegistry;

            if (!Container.HasBinding<TileRegistrySO>())
                Container.BindInstance(tileRegistry).AsSingle();
            Container.Bind<IMapObjectRegistryService>()
                .FromInstance(new MapObjectRegistryService(mapObjectRegistry))
                .AsSingle();
            Container.Bind<IMapLayerRegistry>()
                .FromInstance(new MapLayerRegistry(tileRegistry, mapObjectRegistry))
                .AsSingle();
            Container.Bind<IMapObjectVisualRegistryService>()
                .To<MapObjectVisualRegistryService>()
                .AsSingle();

            Container.Bind<IGeneratorDataRegistry>()
                .To<GeneratorDataRegistry>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<GeneratorTerrainLevelService>()
                .AsSingle();
            Container.Bind<IGeneratedTerrainLevelQuery>()
                .To<GeneratedTerrainLevelQueryService>()
                .AsSingle();
            Container.Bind<ITileWorldCreatorTerrainBuildPolicyService>()
                .To<TileWorldCreatorTerrainBuildPolicyService>()
                .AsSingle();
            GraphGenerationLayerLogFeatureBindings.Install(Container);
            GraphLogicalTileMapFeatureBindings.Install(Container);
            GraphCompilerFeatureBindings.Install(Container);
            GraphTwcMapDataFeatureBindings.Install(Container);
            MoyvaTwcGraphBindingFeatureBindings.Install(Container);
            TileWorldCreatorHeightProjectionFeatureBindings.Install(Container);
            TileWorldCreatorTerrainSideWallFeatureBindings.Install(Container);
            MapVisualFeatureBindings.Install(Container);
            WaterLayerMaterialFeatureBindings.Install(Container);
            MenuWorldPreviewKingdomPlacementFeatureBindings.Install(Container);
            MenuWorldPreviewTextureBuilderFeatureBindings.Install(Container);

            BindMapDataGenerator();
            BindTileWorldCreatorBridge();
            Debug.Log(
                $"{GeneratorBootDiagTag} GeneratorInstaller mapVisual={true}, graphGenerator={_graphAsset != null && _tileWorldCreatorManager != null}, " +
                $"settings={_tileWorldCreatorBuildOptions != null}, graph={(_graphAsset != null ? _graphAsset.name : "null")}, " +
                $"graphBinding={_graphBinding != null}, twcManager={_tileWorldCreatorManager != null}");

            if (_waterLayerMaterialSettings != null)
            {
                Container.BindInstance(_waterLayerMaterialSettings).AsSingle();
                Container.Bind<IWaterLayerMaterialSettings>()
                    .FromInstance(_waterLayerMaterialSettings)
                    .AsSingle();
            }

            Container.BindInterfacesAndSelfTo<MapVisualInstantiator>()
                .AsSingle()
                .NonLazy();
            Container.BindInterfacesAndSelfTo<GeneratedWorldSaveModule>()
                .AsSingle();
            Container.BindInterfacesTo<SaveModuleRegistrar<GeneratedWorldSaveModule>>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<GeneratorWorldStartupBuilder>()
                .AsSingle()
                .NonLazy();
            Container.BindExecutionOrder<GeneratorWorldStartupBuilder>(105);
            Debug.Log(
                $"{GeneratorBootDiagTag} GeneratorInstaller bound GeneratorWorldStartupBuilder interfaces=IInitializable,self, " +
                $"lifetime=AsSingle, nonLazy=true");
        }

        private new void Start()
        {
            if (!Application.isPlaying)
                return;

            bool hasStartupBinding = Container != null && Container.HasBinding<GeneratorWorldStartupBuilder>();
            Debug.Log(
                $"{GeneratorBootDiagTag} GeneratorInstaller.Start fallbackCheck scene={gameObject.scene.name}, " +
                $"hasStartupBinding={hasStartupBinding}, mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}");

            if (!hasStartupBinding)
                return;

            var startupBuilder = Container.Resolve<GeneratorWorldStartupBuilder>();
            startupBuilder.EnsureStartedFromFallback();
        }

        private void BindMapDataGenerator()
        {
            if (_graphAsset != null && _tileWorldCreatorManager != null)
            {
                Container.Bind<IGraphTwcMapDataEnvironment>()
                    .FromInstance(new GraphTwcMapDataEnvironment(_graphAsset, _tileWorldCreatorManager))
                    .AsSingle();
                Container.BindInterfacesAndSelfTo<GraphTwcMapDataState>()
                    .AsSingle();
                Container.Bind<GraphTwcMapDataGenerator>()
                    .AsSingle();
                Container.Bind<IMapDataGenerator>()
                    .To<GraphTwcMapDataGenerator>()
                    .FromResolve();
                return;
            }

            Debug.LogError("[GeneratorInstaller] GraphAsset або TileWorldCreatorManager відсутні. Runtime генерація перейде у disabled fallback.", this);
            Container.Bind<IMapDataGenerator>()
                .To<DisabledMapDataGenerator>()
                .AsSingle();
        }

        private void BindTileWorldCreatorBridge()
        {
            if (_tileWorldCreatorManager == null || _tileWorldCreatorMapping == null)
                return;

            var buildOptions = _tileWorldCreatorBuildOptions ?? new TileWorldCreatorBuildOptions();
            var environment = new TileWorldCreatorBuildEnvironment(
                _tileWorldCreatorManager,
                _tileWorldCreatorMapping,
                buildOptions);

            Container.Bind<ITileWorldCreatorBuildEnvironment>().FromInstance(environment).AsSingle();
            Container.Bind<ITileWorldCreatorBlueprintLayerResolver>().To<TileWorldCreatorBlueprintLayerResolver>().AsSingle();
            Container.Bind<ITileWorldCreatorTerrainLevelMapService>().To<TileWorldCreatorTerrainLevelMapService>().AsSingle();
            Container.Bind<ITileWorldCreatorShoreBandService>().To<TileWorldCreatorShoreBandService>().AsSingle();
            Container.Bind<ITileWorldCreatorConfigurationPreparationService>().To<TileWorldCreatorConfigurationPreparationService>().AsSingle();
            Container.Bind<ITileWorldCreatorTerrainBuildLayerConfigurationService>().To<TileWorldCreatorTerrainBuildLayerConfigurationService>().AsSingle();
            Container.Bind<ITileWorldCreatorLayerPositionCollector>().To<TileWorldCreatorLayerPositionCollector>().AsSingle();
            Container.Bind<ITileWorldCreatorLayerPositionApplier>().To<TileWorldCreatorLayerPositionApplier>().AsSingle();
            Container.Bind<ITileWorldCreatorBuildDiagnosticsService>().To<TileWorldCreatorBuildDiagnosticsService>().AsSingle();
            Container.Bind<ITileWorldCreatorBuildExecutionService>().To<TileWorldCreatorBuildExecutionService>().AsSingle();
            Container.Bind<ITileWorldCreatorTerrainBaseHeightResolver>().To<TileWorldCreatorTerrainBaseHeightResolver>().AsSingle();
            Container.Bind<ITileWorldCreatorTerrainVisualPostProcessor>().To<TileWorldCreatorTerrainVisualPostProcessor>().AsSingle();
            Container.Bind<ITileWorldCreatorTerrainHeightPublisher>().To<TileWorldCreatorTerrainHeightPublisher>().AsSingle();
            Container.Bind<ITileWorldCreatorWorldBuildBridge>().To<TileWorldCreatorWorldBuildBridge>().AsSingle();
        }

        private void ResolveSceneReferences()
        {
            if (_graphBinding == null)
                _graphBinding = FindFirst<MoyvaTileWorldCreatorGraphBinding>();

            if (_graphBinding != null)
            {
                _tileWorldCreatorManager ??= _graphBinding.Manager;
                _graphAsset ??= _graphBinding.GraphAsset;
            }

            if (_tileWorldCreatorManager == null)
                _tileWorldCreatorManager = FindFirst<TileWorldCreatorManager>();
        }

        private TileRegistrySO ResolveTileRegistry()
        {
            if (_tileRegistry != null)
                return _tileRegistry;

            if (_graphAsset != null && _graphAsset.TileRegistry != null)
                return _graphAsset.TileRegistry;

            Debug.LogError("[GeneratorInstaller] TileRegistrySO не знайдено. Створено runtime empty registry, але gameplay tile definitions будуть порожні.", this);
            var empty = ScriptableObject.CreateInstance<TileRegistrySO>();
            empty.name = "RuntimeEmptyTileRegistry";
            return empty;
        }

        private static T FindFirst<T>() where T : Object
        {
            var results = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            return results != null && results.Length > 0 ? results[0] : null;
        }

        private sealed class GeneratorWorldStartupBuilder : IInitializable
        {
            private const string GeneratorBootDiagTag = "[MoyvaGeneratorBootDiag]";
            private const string PolicyDiagTag = "[MoyvaStartPolicyDiag]";
            private const string DirectDiagTag = "[MoyvaDirectStartDiag]";
            private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";
            private readonly MapVisualInstantiator _mapVisualInstantiator;
            private readonly IWorldGenerationDiagnostics _worldDiagnostics;
            private bool _buildTriggered;

            public GeneratorWorldStartupBuilder(
                MapVisualInstantiator mapVisualInstantiator,
                [InjectOptional] IWorldGenerationDiagnostics worldDiagnostics = null)
            {
                _mapVisualInstantiator = mapVisualInstantiator;
                _worldDiagnostics = worldDiagnostics;
                Debug.Log(
                    $"{GeneratorBootDiagTag} GeneratorStartup.Construct mapVisual={_mapVisualInstantiator != null}, " +
                    $"mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}");
                Debug.Log(
                    $"{WorldGenDiagTag} GeneratorStartup.Construct mapVisual={_mapVisualInstantiator != null}, " +
                    $"settings={(_mapVisualInstantiator != null ? _mapVisualInstantiator.DiagnosticMapDataGeneratorTypeName : "null")}, " +
                    $"mode={GameLaunchContext.Mode}, hasWorldSettings={GameLaunchContext.HasWorldSettings}, " +
                    $"graph={(_mapVisualInstantiator != null ? _mapVisualInstantiator.DiagnosticGraphName : "null")}, " +
                    $"hasGeneratorSettings={(_mapVisualInstantiator != null && _mapVisualInstantiator.HasSceneGeneratorConfiguration)}, " +
                    $"hasSharedMapSize={(_mapVisualInstantiator != null && _mapVisualInstantiator.HasSharedMapSize)}");
            }

            public void Initialize()
            {
                AttemptStartup("IInitializable");
            }

            public void EnsureStartedFromFallback()
            {
                AttemptStartup("GeneratorInstaller.Start");
            }

            private void AttemptStartup(string source)
            {
                _worldDiagnostics?.GeneratorStartupInitialized(
                    $"source={source}, frame={UnityEngine.Time.frameCount}, mode={GameLaunchContext.Mode}");
                UnityEngine.Debug.Log(
                    $"{GeneratorBootDiagTag} GeneratorStartup.Initialize ENTER frame={UnityEngine.Time.frameCount}, mode={GameLaunchContext.Mode}, " +
                    $"hasWorldSettings={GameLaunchContext.HasWorldSettings}, autoLoad={GameLaunchContext.IsAutoLoadEnabled()}, maxPlayers={GameLaunchContext.MaxPlayers}, " +
                    $"source={source}, buildTriggered={_buildTriggered}");
                UnityEngine.Debug.Log($"{DirectDiagTag} GeneratorStartup.Initialize enter mode={GameLaunchContext.Mode}, hasInstantiator={_mapVisualInstantiator != null}.");
                UnityEngine.Debug.Log($"{PolicyDiagTag} GeneratorStartup.Initialize enter mode={GameLaunchContext.Mode}, hasInstantiator={_mapVisualInstantiator != null}.");
                UnityEngine.Debug.Log(
                    $"{WorldGenDiagTag} GeneratorStartup.Initialize ENTER frame={UnityEngine.Time.frameCount}, mode={GameLaunchContext.Mode}, " +
                    $"hasWorldSettings={GameLaunchContext.HasWorldSettings}, maxPlayers={GameLaunchContext.MaxPlayers}, " +
                    $"autoLoad={GameLaunchContext.IsAutoLoadEnabled()}, saveSlot={GameLaunchContext.SaveSlot}");

                bool hasInstantiator = _mapVisualInstantiator != null;
                bool hasCurrentWorld = false;
                bool hasPendingWorld = false;
                bool hasGeneratorSettings = false;
                bool hasGraph = false;
                bool hasSharedMapSize = false;
                bool sceneDirectTest = GameLaunchContext.Mode == GameLaunchMode.DirectGameplayTest;
                string mapDataGeneratorType = "null";
                string graphName = "null";
                string gridSize = "0x0";

                if (hasInstantiator)
                {
                    hasCurrentWorld = _mapVisualInstantiator.TryGetCurrentWorldData(out _);
                    hasPendingWorld = _mapVisualInstantiator.HasPendingWorldData;
                    hasGeneratorSettings = _mapVisualInstantiator.HasSceneGeneratorConfiguration;
                    hasGraph = _mapVisualInstantiator.HasGraphGenerator;
                    hasSharedMapSize = _mapVisualInstantiator.HasSharedMapSize;
                    mapDataGeneratorType = _mapVisualInstantiator.DiagnosticMapDataGeneratorTypeName;
                    graphName = _mapVisualInstantiator.DiagnosticGraphName;
                    Vector2Int grid = _mapVisualInstantiator.DiagnosticGridSize;
                    gridSize = $"{grid.x}x{grid.y}";
                }

                bool shouldBuild = ShouldBuildWorldOnStartup(hasInstantiator, hasCurrentWorld, out string reason);
                UnityEngine.Debug.Log(
                    $"{GeneratorBootDiagTag} GeneratorStartup.Decision shouldBuild={shouldBuild}, reason={reason}, " +
                    $"hasMapVisual={hasInstantiator}, hasGraph={hasGraph}, hasGeneratorSettings={hasGeneratorSettings}, " +
                    $"hasWorldSettings={GameLaunchContext.HasWorldSettings}, mode={GameLaunchContext.Mode}");
                UnityEngine.Debug.Log(
                    $"{WorldGenDiagTag} GeneratorStartup.Decision shouldBuild={shouldBuild}, reason={reason}, mode={GameLaunchContext.Mode}, " +
                    $"hasWorldSettings={GameLaunchContext.HasWorldSettings}, autoLoad={GameLaunchContext.IsAutoLoadEnabled()}, " +
                    $"hasPendingWorld={hasPendingWorld}, hasGeneratorSettings={hasGeneratorSettings}, hasGraph={hasGraph}, " +
                    $"hasSharedMapSize={hasSharedMapSize}, sceneDirectTest={sceneDirectTest}, grid={gridSize}, graph={graphName}, " +
                    $"mapDataGenerator={mapDataGeneratorType}");
                UnityEngine.Debug.Log($"{DirectDiagTag} GeneratorStartup.Initialize shouldBuild={shouldBuild}, mode={GameLaunchContext.Mode}.");
                UnityEngine.Debug.Log($"{PolicyDiagTag} GeneratorStartup.Initialize shouldBuild={shouldBuild}, mode={GameLaunchContext.Mode}.");

                if (!shouldBuild)
                {
                    UnityEngine.Debug.LogWarning($"{GeneratorBootDiagTag} GeneratorStartup.SKIP BuildWorld reason={reason}");
                    UnityEngine.Debug.LogWarning($"{WorldGenDiagTag} GeneratorStartup.SKIP BuildWorld reason={reason}");
                    _worldDiagnostics?.ReportStartup();
                    return;
                }

                if (_buildTriggered)
                {
                    UnityEngine.Debug.Log($"{GeneratorBootDiagTag} GeneratorStartup.SKIP BuildWorld reason=already-triggered");
                    return;
                }

                _buildTriggered = true;
                Debug.Log($"[GeneratorStartup] Building world for launch mode '{GameLaunchContext.Mode}'.");
                string buildSource = hasPendingWorld
                    ? "pending-save"
                    : sceneDirectTest
                        ? "direct-test"
                        : "new";
                _worldDiagnostics?.MapVisualBuildWorldCalled($"source={buildSource}, caller=GeneratorStartup");
                UnityEngine.Debug.Log($"{GeneratorBootDiagTag} GeneratorStartup.CALL MapVisual.BuildWorld");
                UnityEngine.Debug.Log($"{WorldGenDiagTag} GeneratorStartup.CALL MapVisualInstantiator.BuildWorld source={buildSource}");
                _mapVisualInstantiator.BuildWorld();
                UnityEngine.Debug.Log($"{WorldGenDiagTag} GeneratorStartup.EXIT BuildWorldReturned frame={UnityEngine.Time.frameCount}, time={UnityEngine.Time.realtimeSinceStartup:F3}");
            }

            private static bool ShouldBuildWorldOnStartup(bool hasInstantiator, bool hasCurrentWorld, out string reason)
            {
                if (!hasInstantiator)
                {
                    reason = "no-map-visual-instantiator";
                    return false;
                }

                if (hasCurrentWorld)
                {
                    reason = "world-already-present";
                    return false;
                }

                GameLaunchContext.EnsureNotExpired();

                switch (GameLaunchContext.Mode)
                {
                    case GameLaunchMode.DirectGameplayTest:
                        reason = "mode-direct-gameplay-test";
                        return true;
                    case GameLaunchMode.MenuNewGame:
                        reason = "mode-menu-new-game";
                        return true;
                    case GameLaunchMode.MenuLoadGame:
                        reason = "mode-menu-load-game";
                        return true;
                    case GameLaunchMode.MenuMultiplayerGame:
                        reason = "mode-menu-multiplayer-game";
                        return true;
                    default:
                        reason = $"mode-not-supported:{GameLaunchContext.Mode}";
                        return false;
                }
            }
        }
    }
}
