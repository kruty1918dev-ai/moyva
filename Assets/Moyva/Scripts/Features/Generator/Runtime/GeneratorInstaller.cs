using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
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

        public override void InstallBindings()
        {
            ResolveSceneReferences();

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

            BindMapDataGenerator();
            BindTileWorldCreatorBridge();

            if (_waterLayerMaterialSettings != null)
                Container.BindInstance(_waterLayerMaterialSettings).AsSingle();

            Container.BindInterfacesAndSelfTo<MapVisualInstantiator>()
                .AsSingle()
                .NonLazy();
            Container.BindInterfacesAndSelfTo<GeneratedWorldSaveModule>()
                .AsSingle();
            Container.BindInterfacesTo<SaveModuleRegistrar<GeneratedWorldSaveModule>>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesTo<GeneratorWorldStartupBuilder>()
                .AsSingle()
                .NonLazy();
            Container.BindExecutionOrder<GeneratorWorldStartupBuilder>(105);
        }

        private void BindMapDataGenerator()
        {
            if (_graphAsset != null && _tileWorldCreatorManager != null)
            {
                Container.Bind<GraphTwcMapDataGenerator>()
                    .AsSingle()
                    .WithArguments(_graphAsset, _tileWorldCreatorManager);
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

            Container.Bind<TileWorldCreatorWorldBuildBridge>()
                .AsSingle()
                .WithArguments(
                    _tileWorldCreatorManager,
                    _tileWorldCreatorMapping,
                    _tileWorldCreatorBuildOptions ?? new TileWorldCreatorBuildOptions());
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
            private readonly MapVisualInstantiator _mapVisualInstantiator;

            public GeneratorWorldStartupBuilder(MapVisualInstantiator mapVisualInstantiator)
            {
                _mapVisualInstantiator = mapVisualInstantiator;
            }

            public void Initialize()
            {
                if (_mapVisualInstantiator == null)
                    return;

                if (_mapVisualInstantiator.TryGetCurrentWorldData(out _))
                    return;

                if (!ShouldBuildWorldOnStartup())
                    return;

                Debug.Log($"[GeneratorStartup] Building world for launch mode '{GameLaunchContext.Mode}'.");
                _mapVisualInstantiator.BuildWorld();
            }

            private static bool ShouldBuildWorldOnStartup()
            {
                GameLaunchContext.EnsureNotExpired();

                switch (GameLaunchContext.Mode)
                {
                    case GameLaunchMode.DirectGameplayTest:
                    case GameLaunchMode.MenuNewGame:
                    case GameLaunchMode.MenuLoadGame:
                    case GameLaunchMode.MenuMultiplayerGame:
                        return true;
                    default:
                        return false;
                }
            }
        }
    }
}
