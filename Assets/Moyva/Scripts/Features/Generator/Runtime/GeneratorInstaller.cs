using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Jsonization;
using Kruty1918.Moyva.MapChunks.Runtime;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator
{
    /// <summary>
    /// Scene composition root for world generation. Keep this serialized identity stable.
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
        [SerializeField] private TileWorldCreatorBuildOptions _tileWorldCreatorBuildOptions =
            new TileWorldCreatorBuildOptions();
        [SerializeField] private WaterLayerMaterialSettings _waterLayerMaterialSettings;

        public override void InstallBindings()
        {
            GameLaunchContext.EnsureDirectGameplayTestFallback();
            ResolveSceneReferences();
            MapChunkFeatureBindings.Install(Container);

            var tileRegistry = ResolveTileRegistry();
            GeneratorBindingGroups.InstallGraphEvaluation(
                Container,
                _graphAsset,
                _tileWorldCreatorManager,
                tileRegistry,
                _mapObjectRegistry,
                this);
            GeneratorBindingGroups.InstallWorldBuild(
                Container,
                _tileWorldCreatorManager,
                _tileWorldCreatorMapping,
                _tileWorldCreatorBuildOptions);
            GeneratorBindingGroups.InstallPresentation(
                Container,
                _waterLayerMaterialSettings);
            GeneratorBindingGroups.InstallStartup(Container);
        }

        private new void Start()
        {
            if (!Application.isPlaying
                || Container == null
                || !Container.HasBinding<GeneratorWorldStartupBuilder>())
            {
                return;
            }

            Container.Resolve<GeneratorWorldStartupBuilder>()
                .EnsureStartedFromFallback();
        }

        private void ResolveSceneReferences()
        {
            _graphBinding ??= FindFirst<MoyvaTileWorldCreatorGraphBinding>();
            if (_graphBinding != null)
            {
                _tileWorldCreatorManager ??= _graphBinding.Manager;
                _graphAsset ??= _graphBinding.GraphAsset;
            }

            _tileWorldCreatorManager ??= FindFirst<TileWorldCreatorManager>();
            EnsureRuntimeConfiguration();
        }

        private void EnsureRuntimeConfiguration()
        {
            if (_tileWorldCreatorManager == null
                || _tileWorldCreatorManager.configuration != null)
            {
                return;
            }

            var configuration = ScriptableObject.CreateInstance<Configuration>();
            configuration.name = "Moyva Runtime TWC Configuration";
            _tileWorldCreatorManager.configuration = configuration;
        }

        private TileRegistrySO ResolveTileRegistry()
        {
            if (_tileRegistry != null)
                return _tileRegistry;
            if (_graphAsset != null && _graphAsset.TileRegistry != null)
                return _graphAsset.TileRegistry;

            Debug.LogError(
                "[GeneratorInstaller] TileRegistrySO is missing; using an " +
                "empty runtime registry.",
                this);
            var empty = MoyvaJsonObjectFactory.Create<TileRegistrySO>();
            empty.name = "RuntimeEmptyTileRegistry";
            return empty;
        }

        private static T FindFirst<T>() where T : Object
        {
            var results = Object.FindObjectsByType<T>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            return results != null && results.Length > 0
                ? results[0]
                : null;
        }
    }
}
