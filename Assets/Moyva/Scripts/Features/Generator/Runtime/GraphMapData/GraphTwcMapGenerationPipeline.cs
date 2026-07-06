using System;
using System.Collections.Generic;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphTwcMapGenerationPipeline : IGraphTwcMapGenerationPipeline
    {
        private readonly IGraphTwcSeedService _seedService;
        private readonly IGraphTwcMapSizeResolver _sizeResolver;
        private readonly IGraphTwcValidationService _validation;
        private readonly IGraphToConfigurationCompilerService _compiler;
        private readonly IGraphTwcWorldBuildService _worldBuild;
        private readonly IGraphTwcLogicalMapExportService _logicalMapExport;
        private readonly IGraphTwcTerrainHeightPublisher _terrainHeightPublisher;
        private readonly IGraphTwcMapDataDiagnosticsService _diagnostics;
        private readonly IGraphTwcEmptyMapFactory _emptyMapFactory;

        public GraphTwcMapGenerationPipeline(
            IGraphTwcSeedService seedService,
            IGraphTwcMapSizeResolver sizeResolver,
            IGraphTwcValidationService validation,
            IGraphToConfigurationCompilerService compiler,
            IGraphTwcWorldBuildService worldBuild,
            IGraphTwcLogicalMapExportService logicalMapExport,
            IGraphTwcTerrainHeightPublisher terrainHeightPublisher,
            IGraphTwcMapDataDiagnosticsService diagnostics,
            IGraphTwcEmptyMapFactory emptyMapFactory)
        {
            _seedService = seedService;
            _sizeResolver = sizeResolver;
            _validation = validation;
            _compiler = compiler;
            _worldBuild = worldBuild;
            _logicalMapExport = logicalMapExport;
            _terrainHeightPublisher = terrainHeightPublisher;
            _diagnostics = diagnostics;
            _emptyMapFactory = emptyMapFactory;
        }

        public GraphTwcMapGenerationResult Generate(GraphTwcMapGenerationRequest request)
        {
            int seed = _seedService.Resolve(request.Graph);
            GlobalSeed.Set(seed);
            _terrainHeightPublisher.Clear();
            _diagnostics.LogEnter(request.Graph, request.Manager, seed, request.Width, request.Height);
            Vector2Int mapSize = _sizeResolver.Resolve(request.Graph, request.Width, request.Height);

            if (request.Manager == null || request.Manager.configuration == null)
                return FailMissingManager(request, seed, mapSize);

            try
            {
                return GenerateSafe(request, seed, mapSize);
            }
            catch (Exception exception)
            {
                _diagnostics.LogException(exception);
                return _emptyMapFactory.Create(mapSize.x, mapSize.y);
            }
        }

        private GraphTwcMapGenerationResult GenerateSafe(GraphTwcMapGenerationRequest request, int seed, Vector2Int mapSize)
        {
            GraphTwcValidationResult validation = _validation.Validate(request.Graph);
            _diagnostics.LogValidation(validation);
            if (validation.HasGlobalErrors)
                return FailValidation(request, seed, mapSize, validation);

            _diagnostics.LogSkippedLayers(_validation, validation);
            IReadOnlyList<CompiledLayerMap> compiled = Compile(request, seed, mapSize, validation);
            float cellSize = ResolveCellSize(request);
            Bounds bounds = default;
            bool hasBounds = GeneratedWorldBoundsUtility.TryCreateTileWorldBounds(
                request.Manager.transform,
                mapSize.x,
                mapSize.y,
                cellSize,
                out bounds);

            _diagnostics.LogTwcCall(request.Manager);
            long elapsedMs = _worldBuild.Build(request.Manager);
            _diagnostics.EmitLayerLog(request, validation, validation.SkippedLayerIds, seed, mapSize, true, compiled);

            var logicalMap = _logicalMapExport.Export(request.Graph, request.Manager, compiled, mapSize.x, mapSize.y, seed);
            _terrainHeightPublisher.Publish(logicalMap.SurfaceHeights);
            var result = CreateResult(logicalMap, compiled, cellSize, hasBounds, bounds);
            _diagnostics.LogTwcResult(elapsedMs, result.BiomeMap, result.HeightMap, result.ObjectMap);
            _diagnostics.LogExit(mapSize.x, mapSize.y, result.BiomeMap, result.HeightMap, result.ObjectMap, result.BuildingMap);
            return result;
        }

        private IReadOnlyList<CompiledLayerMap> Compile(GraphTwcMapGenerationRequest request, int seed,
            Vector2Int mapSize, GraphTwcValidationResult validation)
        {
            _diagnostics.LogCompileCall(request.Graph, mapSize.x, mapSize.y);
            var compiled = _compiler.Compile(request.Graph, request.Manager, seed, validation.SkippedLayerIds, mapSize);
            _diagnostics.LogCompileResult(request.Manager, compiled, validation.SkippedLayerIds.Count);
            return compiled;
        }

        private GraphTwcMapGenerationResult FailMissingManager(GraphTwcMapGenerationRequest request, int seed, Vector2Int mapSize)
        {
            _diagnostics.LogMissingManager();
            _diagnostics.EmitLayerLog(request, null, null, seed, mapSize, false, request.LastCompiledLayers);
            return _emptyMapFactory.Create(mapSize.x, mapSize.y);
        }

        private GraphTwcMapGenerationResult FailValidation(GraphTwcMapGenerationRequest request, int seed,
            Vector2Int mapSize, GraphTwcValidationResult validation)
        {
            _diagnostics.LogValidationFailed(_validation, validation);
            _diagnostics.EmitLayerLog(request, validation, null, seed, mapSize, false, Array.Empty<CompiledLayerMap>());
            return _emptyMapFactory.Create(mapSize.x, mapSize.y);
        }

        private static GraphTwcMapGenerationResult CreateResult(GraphLogicalTileMap logicalMap,
            IReadOnlyList<CompiledLayerMap> compiled, float cellSize, bool hasBounds, Bounds bounds)
        {
            return new GraphTwcMapGenerationResult
            {
                BiomeMap = logicalMap.TileIds,
                ObjectMap = new string[logicalMap.TileIds.GetLength(0), logicalMap.TileIds.GetLength(1)],
                HeightMap = logicalMap.LayerHeights,
                BuildingMap = new string[logicalMap.TileIds.GetLength(0), logicalMap.TileIds.GetLength(1)],
                CompiledLayers = compiled,
                CellSize = cellSize,
                HasBaseMapWorldBounds = hasBounds,
                BaseMapWorldBounds = bounds
            };
        }

        private static float ResolveCellSize(GraphTwcMapGenerationRequest request)
        {
            return request.Manager.configuration.cellSize > 0.0001f
                ? request.Manager.configuration.cellSize
                : 1f;
        }
    }
}
