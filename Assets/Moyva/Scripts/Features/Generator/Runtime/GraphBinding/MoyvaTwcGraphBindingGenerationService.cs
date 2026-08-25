using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.MapChunks.Runtime;
using UnityEngine;
using Zenject;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MoyvaTwcGraphBindingGenerationService : IMoyvaTwcGraphBindingGenerationService
    {
        private readonly IMoyvaTwcGraphBindingResolver _resolver;
        private readonly IMoyvaTwcGraphCompileService _compiler;
        private readonly IMoyvaTwcGraphValidationService _validation;
        private readonly ITileWorldCreatorWorldBuildBridge _worldBuild;
        private readonly IChunkFirstTwcVisualCleanupService _twcVisualCleanup;

        public MoyvaTwcGraphBindingGenerationService(
            IMoyvaTwcGraphBindingResolver resolver,
            IMoyvaTwcGraphCompileService compiler,
            IMoyvaTwcGraphValidationService validation,
            [InjectOptional] ITileWorldCreatorWorldBuildBridge worldBuild = null,
            [InjectOptional] IChunkFirstTwcVisualCleanupService twcVisualCleanup = null)
        {
            _resolver = resolver;
            _compiler = compiler;
            _validation = validation;
            _worldBuild = worldBuild;
            _twcVisualCleanup = twcVisualCleanup ?? new ChunkFirstTwcVisualCleanupService(new ChunkFirstBuildDiagnostics());
        }

        public bool GenerateFromGraph(IMoyvaTwcGraphBindingContext context)
        {
            return GenerateFromGraph(context, _resolver.ResolveSeed(context));
        }

        public bool GenerateFromGraph(IMoyvaTwcGraphBindingContext context, int seed)
        {
            if (!TryEnterGeneration(context))
                return false;

            int normalizedSeed = _resolver.NormalizeSeed(seed);
            try
            {
                return GenerateInternal(context, normalizedSeed);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Moyva TWC Graph Binding] Помилка генерації мапи: {ex}", context.LogContext);
                return false;
            }
            finally
            {
                MoyvaTwcGraphEditorProgress.Clear();
                context.SetGenerating(false);
            }
        }

        private bool GenerateInternal(IMoyvaTwcGraphBindingContext context, int seed)
        {
            seed = GlobalSeed.InitializeDeterministic(seed);

            if (context.CompileBeforeGenerate)
            {
                if (!CanCompileForGeneration(context))
                    return false;

                IReadOnlyList<CompiledLayerMap> compiled =
                    _compiler.Compile(context, seed);
                if (context.GenerateBuildLayersAfterCompile
                    && !HasAuthoritativeRenderableLayer(context, compiled))
                {
                    Debug.LogError(
                        "[Moyva TWC Graph Binding] Generation stopped: graph compilation produced no enabled authoritative renderable layer. Existing companion output was not built.",
                        context.LogContext);
                    return false;
                }
            }

            if (context.GenerateBuildLayersAfterCompile
                && !HasAuthoritativeRenderableLayer(
                    context,
                    context.LastCompiledLayers))
            {
                Debug.LogError(
                    "[Moyva TWC Graph Binding] Generation stopped: no enabled authoritative renderable layer is available. Existing companion output was not built.",
                    context.LogContext);
                return false;
            }

            if (context.Manager == null || context.Manager.configuration == null)
                return false;

            bool succeeded;
            if (context.GenerateBuildLayersAfterCompile)
                succeeded = GenerateChunkFirstMap(context, seed);
            else
            {
                TileWorldCreatorLayerOcclusionOptimizer.GenerateBlueprintMap(context.Manager);
                succeeded = true;
            }

            return succeeded;
        }

        private bool CanCompileForGeneration(
            IMoyvaTwcGraphBindingContext context)
        {
            if (!_validation.CanCompile(context, out string reason))
            {
                Debug.LogError(
                    $"[Moyva TWC Graph Binding] Generation stopped before compilation: {reason}",
                    context?.LogContext);
                return false;
            }

            GraphValidationReport report =
                _validation.Validate(context.GraphAsset);
            System.Collections.Generic.List<GraphValidationIssue> globalErrors =
                _validation.GetGlobalErrors(report);
            if (globalErrors == null || globalErrors.Count == 0)
                return true;

            Debug.LogError(
                $"[Moyva TWC Graph Binding] Generation stopped before compilation: graph validation contains {globalErrors.Count} global error(s). Native or stale companion output was not built.",
                context.LogContext);
            return false;
        }

        private static bool HasAuthoritativeRenderableLayer(
            IMoyvaTwcGraphBindingContext context,
            IReadOnlyList<CompiledLayerMap> compiled)
        {
            if (context?.GraphAsset == null || compiled == null)
                return false;

            for (int i = 0; i < compiled.Count; i++)
            {
                CompiledLayerMap layer = compiled[i];
                if (layer == null
                    || !layer.HasRenderableTileOutput
                    || string.IsNullOrWhiteSpace(layer.GraphLayerId)
                    || string.IsNullOrWhiteSpace(layer.BlueprintLayerGuid))
                {
                    continue;
                }

                GeneratorLayerDefinition definition =
                    context.GraphAsset.GetLayerById(layer.GraphLayerId);
                if (definition != null && definition.Enabled)
                    return true;
            }

            return false;
        }

        private bool GenerateChunkFirstMap(IMoyvaTwcGraphBindingContext context, int seed)
        {
            var manager = context.Manager;
            _twcVisualCleanup.ClearVisualBuildOutput(manager);
            manager.ExecuteBlueprintLayers();

            var mapSize = _resolver.ResolveMapSize(context);
            var logicalMap = GraphLogicalTileMapBuilder.Build(
                context.GraphAsset,
                manager,
                context.LastCompiledLayers,
                mapSize.x,
                mapSize.y);
            if (logicalMap == null)
            {
                Debug.LogError("[Moyva TWC Graph Binding] Chunk-first generation failed: logical tile stack map is missing.", context.LogContext);
                return false;
            }

            if (_worldBuild == null)
            {
                TileWorldCreatorWorldBuildResult result =
                    BuildStandaloneChunkFirst(manager, CreateWorldData(context, seed, logicalMap, mapSize));
                _twcVisualCleanup.ClearVisualBuildOutput(manager);
                return result.Succeeded;
            }

            TileWorldCreatorWorldBuildResult worldBuildResult =
                _worldBuild.Build(CreateWorldData(context, seed, logicalMap, mapSize));
            _twcVisualCleanup.ClearVisualBuildOutput(manager);
            return worldBuildResult.Succeeded;
        }

        private GeneratedWorldData CreateWorldData(
            IMoyvaTwcGraphBindingContext context,
            int seed,
            GraphLogicalTileMap logicalMap,
            Vector2Int mapSize)
        {
            var manager = context.Manager;
            float cellSize = ResolveCellSize(manager);
            bool hasBounds = GeneratedWorldBoundsUtility.TryCreateTileWorldBounds(
                manager.transform,
                mapSize.x,
                mapSize.y,
                cellSize,
                out var bounds);

            return new GeneratedWorldData
            {
                Width = mapSize.x,
                Height = mapSize.y,
                BiomeMap = logicalMap.TileIds,
                ObjectMap = new string[mapSize.x, mapSize.y],
                HeightMap = logicalMap.LayerHeights,
                BuildingMap = new string[mapSize.x, mapSize.y],
                LogicalTileMap = logicalMap,
                CompiledLayers = context.LastCompiledLayers,
                ForceChunkFirstCompositeBuild = true,
                CellSize = cellSize,
                HasBaseMapWorldBounds = hasBounds,
                BaseMapWorldBounds = bounds,
                Seed = seed
            };
        }

        private TileWorldCreatorWorldBuildResult BuildStandaloneChunkFirst(
            GiantGrey.TileWorldCreator.TileWorldCreatorManager manager,
            GeneratedWorldData worldData)
        {
            var mapping = MoyvaJsonObjectFactory.Create<TileWorldCreatorIdMappingSO>();
            mapping.name = "RuntimeEmptyTileWorldCreatorIdMapping";
            try
            {
                IMapChunkSettingsProvider settings = ResolveChunkSettings();
                var layout = new MapChunkLayoutService(settings);
                var roots = new MapVisualChunkRootService();
                var registry = new MapVisualChunkRegistry();
                var diagnostics = new ChunkFirstBuildDiagnostics();
                var meshRegistry = new ChunkFirstRuntimeMeshRegistry();
                var environment = new TileWorldCreatorBuildEnvironment(
                    manager,
                    mapping,
                    new TileWorldCreatorBuildOptions());
                var builder = new ChunkFirstWorldBuildService(
                    environment,
                    settings,
                    roots,
                    registry,
                    new ChunkBuildAreaPlanner(layout),
                    new TileNeighborhoodFactory(),
                    new ResolvedTileCompositionResolver(),
                    new TwcTileMeshSourceProvider(environment),
                    _twcVisualCleanup,
                    new ChunkTerrainMeshBuilder(meshRegistry, diagnostics),
                    new ChunkFirstObjectSpawner(environment, layout, roots),
                    meshRegistry,
                    diagnostics);
                return builder.Build(
                    worldData,
                    manager.configuration,
                    new TileWorldCreatorTerrainBuildPolicyResult(
                        TileWorldCreatorTerrainBuildMode.ChunkFirstCompositeMesh,
                        settings.ChunkSize,
                        true));
            }
            finally
            {
                MoyvaJsonObjectFactory.DestroyImmediate(mapping);
            }
        }

        private static IMapChunkSettingsProvider ResolveChunkSettings()
        {
            var sceneSettings = UnityEngine.Object.FindFirstObjectByType<MapChunkSceneSettings>(FindObjectsInactive.Include);
            return sceneSettings != null ? sceneSettings : new DefaultMapChunkSettingsProvider();
        }

        private static void DestroyUnityObject(UnityEngine.Object instance)
        {
            if (instance == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(instance);
            else
                UnityEngine.Object.DestroyImmediate(instance);
        }

        private static float ResolveCellSize(GiantGrey.TileWorldCreator.TileWorldCreatorManager manager)
        {
            var configuration = manager != null ? manager.configuration : null;
            return configuration != null && configuration.cellSize > 0.0001f
                ? configuration.cellSize
                : 1f;
        }

        private static bool TryEnterGeneration(IMoyvaTwcGraphBindingContext context)
        {
            if (context.IsGenerating)
            {
                return false;
            }

            context.SetGenerating(true);
            return true;
        }
    }
}
