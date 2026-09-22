using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.JsonConfig;
using Kruty1918.Moyva.MapChunks.Runtime;
using Kruty1918.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator
{
    /// <summary>
    /// Scene composition root for world generation. Keep this serialized identity stable.
    /// </summary>
    public sealed class GeneratorInstaller : MonoInstaller
    {
        [Header("Scene Recipe Source")]
        [SerializeField] private GeneratorMapRecipe _mapRecipe;
        [SerializeField] private TileWorldCreatorManager _tileWorldCreatorManager;

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
            GeneratorBindingGroups.InstallMapGeneration(
                Container,
                _mapRecipe,
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
            // A serialized inline recipe without layers is a stale stub left by an older
            // scene serialization (e.g. migrated graph-era data). JSON presets are the
            // source of truth, so treat the stub as missing and resolve from JSON.
            if (_mapRecipe == null || _mapRecipe.Layers == null || _mapRecipe.Layers.Count == 0)
                _mapRecipe = ResolveFallbackRecipe();
            _tileWorldCreatorManager ??= FindFirst<TileWorldCreatorManager>();
            EnsureRuntimeConfiguration();
        }

        private static GeneratorMapRecipe ResolveFallbackRecipe()
        {
            try
            {
                JsonConfigRuntime.EnsureLoaded();
                foreach (var recipe in JsonConfigRuntime.GetAll<GeneratorMapRecipe>())
                {
                    if (recipe != null)
                        return recipe;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning(
                    $"[GeneratorInstaller] Failed to resolve a fallback GeneratorMapRecipe: {ex.Message}");
            }
            return null;
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
            if (_mapRecipe != null && _mapRecipe.TileRegistry != null)
                return _mapRecipe.TileRegistry;

            Debug.LogError(
                "[GeneratorInstaller] TileRegistrySO is missing; using an " +
                "empty runtime registry.",
                this);
            var empty = JsonObjectFactory.Create<TileRegistrySO>();
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
