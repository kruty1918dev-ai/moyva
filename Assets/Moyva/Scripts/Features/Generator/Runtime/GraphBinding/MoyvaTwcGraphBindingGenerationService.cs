using System;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.MapChunks.Runtime;
using UnityEngine;
using Zenject;

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

        public void GenerateFromGraph(IMoyvaTwcGraphBindingContext context)
        {
            GenerateFromGraph(context, _resolver.ResolveSeed(context));
        }

        public void GenerateFromGraph(IMoyvaTwcGraphBindingContext context, int seed)
        {
            if (!TryEnterGeneration(context, "Генерація вже виконується, повторний запуск пропущено."))
                return;

            int normalizedSeed = _resolver.NormalizeSeed(seed);
            try
            {
                GenerateInternal(context, normalizedSeed);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Moyva TWC Graph Binding] Помилка генерації мапи: {ex}", context.LogContext);
            }
            finally
            {
                MoyvaTwcGraphEditorProgress.Clear();
                context.SetGenerating(false);
            }
        }

        private void GenerateInternal(IMoyvaTwcGraphBindingContext context, int seed)
        {
            if (context.CompileBeforeGenerate)
                _compiler.Compile(context, seed, false);

            if (context.Manager == null || context.Manager.configuration == null)
            {
                EmitGenerateLog(context, null, null, seed);
                return;
            }

            if (context.GenerateBuildLayersAfterCompile)
                GenerateChunkFirstMap(context, seed);
            else
                TileWorldCreatorLayerOcclusionOptimizer.GenerateBlueprintMap(context.Manager);

            EmitLogicalMapDiagnostics(context, seed);
            var report = _validation.Validate(context.GraphAsset);
            EmitGenerateLog(context, report, _validation.GetInvalidLayerIds(report), seed);
        }

        private void EmitLogicalMapDiagnostics(IMoyvaTwcGraphBindingContext context, int seed)
        {
            var mapSize = _resolver.ResolveMapSize(context);
            var logicalMap = GraphLogicalTileMapBuilder.Build(
                context.GraphAsset,
                context.Manager,
                context.LastCompiledLayers,
                mapSize.x,
                mapSize.y);

            GraphLogicalTileMapDiagnostics.EmitAndCompare(
                "Scene build mask",
                context.GraphAsset,
                seed,
                logicalMap,
                context.LogContext);
        }

        private void GenerateChunkFirstMap(IMoyvaTwcGraphBindingContext context, int seed)
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
                return;
            }

            if (_worldBuild == null)
            {
                BuildStandaloneChunkFirst(manager, CreateWorldData(context, seed, logicalMap, mapSize));
                _twcVisualCleanup.ClearVisualBuildOutput(manager);
                return;
            }

            _worldBuild.Build(CreateWorldData(context, seed, logicalMap, mapSize));
            _twcVisualCleanup.ClearVisualBuildOutput(manager);
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

        private void BuildStandaloneChunkFirst(GiantGrey.TileWorldCreator.TileWorldCreatorManager manager, GeneratedWorldData worldData)
        {
            var mapping = ScriptableObject.CreateInstance<TileWorldCreatorIdMappingSO>();
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
                builder.Build(
                    worldData,
                    manager.configuration,
                    new TileWorldCreatorTerrainBuildPolicyResult(
                        TileWorldCreatorTerrainBuildMode.ChunkFirstCompositeMesh,
                        settings.ChunkSize,
                        true));
            }
            finally
            {
                DestroyUnityObject(mapping);
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

        private void EmitGenerateLog(IMoyvaTwcGraphBindingContext context, GraphValidationReport report, System.Collections.Generic.HashSet<string> skipped, int seed)
        {
            GraphGenerationLayerLog.Emit(
                "Graph Binding Generate",
                context.GraphAsset,
                context.Manager,
                context.LastCompiledLayers,
                report,
                skipped,
                seed,
                _resolver.ResolveMapSize(context),
                context.GenerateBuildLayersAfterCompile,
                context.LogContext);
        }

        private static bool TryEnterGeneration(IMoyvaTwcGraphBindingContext context, string warning)
        {
            if (context.IsGenerating)
            {
                Debug.LogWarning($"[Moyva TWC Graph Binding] {warning}", context.LogContext);
                return false;
            }

            context.SetGenerating(true);
            return true;
        }
    }
}
