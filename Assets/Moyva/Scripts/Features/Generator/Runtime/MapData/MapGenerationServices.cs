using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.SaveSystem;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IMapGenerationEnvironment
    {
        GeneratorMapRecipe Recipe { get; }
        TileWorldCreatorManager Manager { get; }
    }

    internal sealed class MapGenerationEnvironment : IMapGenerationEnvironment
    {
        public MapGenerationEnvironment(GeneratorMapRecipe recipe, TileWorldCreatorManager manager)
        {
            Recipe = recipe;
            Manager = manager;
        }

        public GeneratorMapRecipe Recipe { get; }
        public TileWorldCreatorManager Manager { get; }
    }

    internal interface IMapGenerationDiagnostics
    {
        IReadOnlyList<CompiledLayerMap> LastCompiledLayers { get; }
        LogicalTileMap LastLogicalMap { get; }
        TerrainPassagePlan LastPassages { get; }
        float LastCellSize { get; }
        bool TryGetLastBaseMapWorldBounds(out Bounds bounds);
    }

    internal interface IMapGenerationState : IMapGenerationDiagnostics
    {
        void Apply(MapGenerationResult result);
    }

    internal sealed class MapGenerationState : IMapGenerationState
    {
        private bool _hasLastBaseMapWorldBounds;
        private Bounds _lastBaseMapWorldBounds;

        public IReadOnlyList<CompiledLayerMap> LastCompiledLayers { get; private set; }
        public LogicalTileMap LastLogicalMap { get; private set; }
        public TerrainPassagePlan LastPassages { get; private set; }
        public float LastCellSize { get; private set; } = 1f;

        public bool TryGetLastBaseMapWorldBounds(out Bounds bounds)
        {
            bounds = _lastBaseMapWorldBounds;
            return _hasLastBaseMapWorldBounds;
        }

        public void Apply(MapGenerationResult result)
        {
            if (result == null)
                return;
            if (result.CompiledLayers != null)
                LastCompiledLayers = result.CompiledLayers;
            LastLogicalMap = result.LogicalMap;
            LastPassages = result.TerrainPassages;
            LastCellSize = result.CellSize > 0.0001f ? result.CellSize : 1f;
            _hasLastBaseMapWorldBounds = result.HasBaseMapWorldBounds;
            _lastBaseMapWorldBounds = result.BaseMapWorldBounds;
        }
    }

    internal readonly struct MapGenerationRequest
    {
        public MapGenerationRequest(
            GeneratorMapRecipe recipe,
            TileWorldCreatorManager manager,
            int width,
            int height,
            IReadOnlyList<CompiledLayerMap> lastCompiledLayers,
            int? seedOverride = null)
        {
            Recipe = recipe;
            Manager = manager;
            Width = width;
            Height = height;
            LastCompiledLayers = lastCompiledLayers;
            SeedOverride = seedOverride;
        }

        public GeneratorMapRecipe Recipe { get; }
        public int? SeedOverride { get; }
        public TileWorldCreatorManager Manager { get; }
        public int Width { get; }
        public int Height { get; }
        public IReadOnlyList<CompiledLayerMap> LastCompiledLayers { get; }
    }

    internal sealed class MapGenerationResult
    {
        public string[,] BiomeMap;
        public string[,] ObjectMap;
        public float[,] HeightMap;
        public string[,] BuildingMap;
        public LogicalTileMap LogicalMap;
        public TerrainPassagePlan TerrainPassages;
        public IReadOnlyList<CompiledLayerMap> CompiledLayers;
        public float CellSize = 1f;
        public bool HasBaseMapWorldBounds;
        public Bounds BaseMapWorldBounds;
    }

    internal interface IMapSeedService
    {
        int Resolve(GeneratorMapRecipe recipe);
    }

    internal sealed class MapSeedService : IMapSeedService
    {
        public int Resolve(GeneratorMapRecipe recipe)
        {
            if (GameLaunchContext.TryGetSeed(out int launchSeed))
                return GlobalSeed.Normalize(launchSeed);
            if (recipe != null && recipe.Seed != 0)
                return GlobalSeed.Normalize(recipe.Seed);
            return GlobalSeed.DefaultSeed;
        }
    }

    internal interface IMapSizeResolver
    {
        Vector2Int Resolve(GeneratorMapRecipe recipe, int requestedWidth, int requestedHeight);
    }

    internal sealed class MapSizeResolver : IMapSizeResolver
    {
        public Vector2Int Resolve(GeneratorMapRecipe recipe, int requestedWidth, int requestedHeight)
        {
            Vector2Int requested;
            var shared = recipe?.SharedSettings;
            if (GameLaunchContext.TryGetWorldDimensions(out int launchWidth, out int launchHeight))
                requested = Clamp(launchWidth, launchHeight);
            else if (shared != null && shared.HasMapSize)
                requested = Clamp(shared.MapWidth, shared.MapHeight);
            else
                requested = Clamp(requestedWidth, requestedHeight);
            return MapChunkSizePolicy.CropMapSize(requested.x, requested.y);
        }

        private static Vector2Int Clamp(int width, int height)
        {
            return new Vector2Int(Mathf.Max(1, width), Mathf.Max(1, height));
        }
    }

    internal interface IMapRecipeValidationService
    {
        MapRecipeValidationResult Validate(GeneratorMapRecipe recipe);
    }

    internal sealed class MapRecipeValidationService : IMapRecipeValidationService
    {
        public MapRecipeValidationResult Validate(GeneratorMapRecipe recipe)
        {
            var result = GeneratorMapRecipeValidator.Validate(recipe);
            foreach (var warning in result.Warnings)
                Debug.LogWarning($"[RecipeGenerator] {warning}");
            return result;
        }
    }

    internal interface IEmptyMapFactory
    {
        MapGenerationResult Create(int width, int height);
    }

    internal sealed class EmptyMapFactory : IEmptyMapFactory
    {
        public MapGenerationResult Create(int width, int height)
        {
            var biomeMap = new string[width, height];
            var heightMap = new float[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                biomeMap[x, y] = GeneratedWorldDataDefaults.FallbackTileId;
                heightMap[x, y] = GeneratedWorldDataDefaults.FallbackLandHeight;
            }
            return new MapGenerationResult
            {
                BiomeMap = biomeMap,
                ObjectMap = new string[width, height],
                HeightMap = heightMap,
                BuildingMap = new string[width, height]
            };
        }
    }

    internal interface ITerrainHeightPublisher
    {
        void Clear();
        void Publish(float[,] surfaceHeightMap);
    }

    internal sealed class TerrainHeightPublisher : ITerrainHeightPublisher
    {
        private readonly IGeneratorTerrainLevelService _terrainLevelService;

        public TerrainHeightPublisher([InjectOptional] IGeneratorTerrainLevelService terrainLevelService = null)
        {
            _terrainLevelService = terrainLevelService;
        }

        public void Clear()
        {
            _terrainLevelService?.Clear();
        }

        public void Publish(float[,] surfaceHeightMap)
        {
            if (_terrainLevelService == null)
                return;
            _terrainLevelService.SetLevelMap(BuildLevelMap(surfaceHeightMap));
            _terrainLevelService.SetSurfaceHeightMap(surfaceHeightMap);
        }

        private static int[,] BuildLevelMap(float[,] surfaceHeightMap)
        {
            if (surfaceHeightMap == null)
                return null;
            int width = surfaceHeightMap.GetLength(0);
            int height = surfaceHeightMap.GetLength(1);
            var levelMap = new int[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                levelMap[x, y] = Mathf.Max(0, Mathf.RoundToInt(surfaceHeightMap[x, y]));
            return levelMap;
        }
    }

    internal interface IMapLogicalMapExportService
    {
        LogicalTileMap Export(GeneratorMapRecipe recipe, TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiledLayers, int width, int height);
    }

    internal sealed class MapLogicalMapExportService : IMapLogicalMapExportService
    {
        private readonly ILogicalTileMapBuilderService _builder;

        public MapLogicalMapExportService(ILogicalTileMapBuilderService builder)
        {
            _builder = builder;
        }

        public LogicalTileMap Export(GeneratorMapRecipe recipe, TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiledLayers, int width, int height)
        {
            return _builder.Build(recipe, manager, compiledLayers, width, height);
        }
    }

    internal interface IMapGenerationPipeline
    {
        MapGenerationResult Generate(MapGenerationRequest request);
    }

    /// <summary>
    /// Runtime map-generation pipeline: resolves seed and size, validates the
    /// recipe, compiles it into the TWC configuration, executes the blueprint
    /// layers, exports the logical tile map and publishes terrain heights.
    /// </summary>
    internal sealed class MapGenerationPipeline : IMapGenerationPipeline
    {
        private readonly IMapSeedService _seedService;
        private readonly IMapSizeResolver _sizeResolver;
        private readonly IMapRecipeValidationService _validation;
        private readonly IRecipeToConfigurationCompilerService _compiler;
        private readonly IMapLogicalMapExportService _logicalMapExport;
        private readonly ITerrainHeightPublisher _terrainHeightPublisher;
        private readonly IEmptyMapFactory _emptyMapFactory;
        private readonly ITerrainReliefFieldPlanner _reliefPlanner;
        private readonly ITerrainPlanApplier _terrainPlanApplier;

        public MapGenerationPipeline(
            IMapSeedService seedService,
            IMapSizeResolver sizeResolver,
            IMapRecipeValidationService validation,
            IRecipeToConfigurationCompilerService compiler,
            IMapLogicalMapExportService logicalMapExport,
            ITerrainHeightPublisher terrainHeightPublisher,
            IEmptyMapFactory emptyMapFactory,
            [InjectOptional] ITerrainReliefFieldPlanner reliefPlanner = null,
            [InjectOptional] ITerrainPlanApplier terrainPlanApplier = null)
        {
            _seedService = seedService;
            _sizeResolver = sizeResolver;
            _validation = validation;
            _compiler = compiler;
            _logicalMapExport = logicalMapExport;
            _terrainHeightPublisher = terrainHeightPublisher;
            _emptyMapFactory = emptyMapFactory;
            _reliefPlanner = reliefPlanner;
            _terrainPlanApplier = terrainPlanApplier;
        }

        public MapGenerationResult Generate(MapGenerationRequest request)
        {
            int seed = GlobalSeed.InitializeDeterministic(request.SeedOverride ?? _seedService.Resolve(request.Recipe));
            _terrainHeightPublisher.Clear();
            Vector2Int mapSize = request.SeedOverride.HasValue
                ? new Vector2Int(request.Width, request.Height)
                : _sizeResolver.Resolve(request.Recipe, request.Width, request.Height);
            if (request.Manager == null || request.Manager.configuration == null)
            {
                Debug.LogError("[RecipeGenerator] TileWorldCreatorManager configuration is missing.");
                return _emptyMapFactory.Create(mapSize.x, mapSize.y);
            }
            try
            {
                return GenerateSafe(request, seed, mapSize);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return _emptyMapFactory.Create(mapSize.x, mapSize.y);
            }
        }

        private MapGenerationResult GenerateSafe(
            MapGenerationRequest request,
            int seed,
            Vector2Int mapSize)
        {
            MapRecipeValidationResult validation = _validation.Validate(request.Recipe);
            if (validation.HasGlobalErrors)
                return FailValidation(mapSize, validation);

            float[,] reliefField =
                _reliefPlanner?.Build(seed, mapSize, request.Recipe?.TerrainRelief);
            IReadOnlyList<CompiledLayerMap> compiled =
                _compiler.Compile(
                    request.Recipe,
                    request.Manager,
                    seed,
                    validation.SkippedLayerIds,
                    mapSize,
                    reliefField);
            float cellSize = ResolveCellSize(request);
            bool hasBounds =
                GeneratedWorldBoundsUtility.TryCreateTileWorldBounds(
                    request.Manager.transform,
                    mapSize.x,
                    mapSize.y,
                    cellSize,
                    out Bounds bounds);
            request.Manager.ExecuteBlueprintLayers();
            LogicalTileMap logicalMap =
                _logicalMapExport.Export(
                    request.Recipe,
                    request.Manager,
                    compiled,
                    mapSize.x,
                    mapSize.y);
            TerrainPassagePlan passages = _terrainPlanApplier?.Apply(
                logicalMap,
                request.Recipe,
                reliefField,
                seed);
            _terrainHeightPublisher.Publish(logicalMap.SurfaceHeights);
            var result = CreateResult(logicalMap, compiled, cellSize, hasBounds, bounds);
            result.TerrainPassages = passages;
            return result;
        }

        private MapGenerationResult FailValidation(Vector2Int mapSize, MapRecipeValidationResult validation)
        {
            Debug.LogError(
                $"[RecipeGenerator] Recipe validation failed with {validation.GlobalErrors.Count} global error(s):\n" +
                string.Join("\n", validation.GlobalErrors));
            return _emptyMapFactory.Create(mapSize.x, mapSize.y);
        }

        private static MapGenerationResult CreateResult(LogicalTileMap logicalMap,
            IReadOnlyList<CompiledLayerMap> compiled, float cellSize, bool hasBounds, Bounds bounds)
        {
            return new MapGenerationResult
            {
                BiomeMap = logicalMap.TileIds,
                ObjectMap = new string[logicalMap.TileIds.GetLength(0), logicalMap.TileIds.GetLength(1)],
                HeightMap = logicalMap.LayerHeights,
                BuildingMap = new string[logicalMap.TileIds.GetLength(0), logicalMap.TileIds.GetLength(1)],
                LogicalMap = logicalMap,
                CompiledLayers = compiled,
                CellSize = cellSize,
                HasBaseMapWorldBounds = hasBounds,
                BaseMapWorldBounds = bounds
            };
        }

        private static float ResolveCellSize(MapGenerationRequest request)
        {
            return request.Manager.configuration.cellSize > 0.0001f
                ? request.Manager.configuration.cellSize
                : 1f;
        }
    }
}
