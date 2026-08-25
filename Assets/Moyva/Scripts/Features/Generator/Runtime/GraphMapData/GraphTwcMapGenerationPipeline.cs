using System;
using System.Collections.Generic;
using Kruty1918.Moyva.GraphSystem.API;
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
        private readonly IGraphTwcLogicalMapExportService _logicalMapExport;
        private readonly IGraphTwcTerrainHeightPublisher _terrainHeightPublisher;
        private readonly IGraphTwcEmptyMapFactory _emptyMapFactory;

        public GraphTwcMapGenerationPipeline(
            IGraphTwcSeedService seedService,
            IGraphTwcMapSizeResolver sizeResolver,
            IGraphTwcValidationService validation,
            IGraphToConfigurationCompilerService compiler,
            IGraphTwcLogicalMapExportService logicalMapExport,
            IGraphTwcTerrainHeightPublisher terrainHeightPublisher,
            IGraphTwcEmptyMapFactory emptyMapFactory)
        {
            _seedService = seedService;
            _sizeResolver = sizeResolver;
            _validation = validation;
            _compiler = compiler;
            _logicalMapExport = logicalMapExport;
            _terrainHeightPublisher = terrainHeightPublisher;
            _emptyMapFactory = emptyMapFactory;
        }

        public GraphTwcMapGenerationResult Generate(GraphTwcMapGenerationRequest request)
        {
            int seed = GlobalSeed.InitializeDeterministic(_seedService.Resolve(request.Graph));
            _terrainHeightPublisher.Clear();
            Vector2Int mapSize = _sizeResolver.Resolve(request.Graph, request.Width, request.Height);

            if (request.Manager == null || request.Manager.configuration == null)
            {
                Debug.LogError("[GraphTwcGenerator] TileWorldCreatorManager configuration is missing.");
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

        private GraphTwcMapGenerationResult GenerateSafe(
    GraphTwcMapGenerationRequest request,
    int seed,
    Vector2Int mapSize)
        {
            GraphTwcValidationResult validation =
                _validation.Validate(request.Graph);

            if (validation.HasGlobalErrors)
                return FailValidation(mapSize, validation);

            IReadOnlyList<CompiledLayerMap> compiled =
                Compile(
                    request,
                    seed,
                    mapSize,
                    validation);

            float cellSize =
                ResolveCellSize(request);

            Bounds bounds = default;

            bool hasBounds =
                GeneratedWorldBoundsUtility
                    .TryCreateTileWorldBounds(
                        request.Manager.transform,
                        mapSize.x,
                        mapSize.y,
                        cellSize,
                        out bounds);

            request.Manager.ExecuteBlueprintLayers();

            GraphLogicalTileMap logicalMap =
                _logicalMapExport.Export(
                    request.Graph,
                    request.Manager,
                    compiled,
                    mapSize.x,
                    mapSize.y);

            _terrainHeightPublisher.Publish(
                logicalMap.SurfaceHeights);

            GraphTwcMapGenerationResult result =
                CreateResult(
                    logicalMap,
                    compiled,
                    cellSize,
                    hasBounds,
                    bounds);

            return result;
        }

        private IReadOnlyList<CompiledLayerMap> Compile(GraphTwcMapGenerationRequest request, int seed,
            Vector2Int mapSize, GraphTwcValidationResult validation)
        {
            return _compiler.Compile(request.Graph, request.Manager, seed, validation.SkippedLayerIds, mapSize);
        }

        private GraphTwcMapGenerationResult FailValidation(Vector2Int mapSize, GraphTwcValidationResult validation)
        {
            Debug.LogError($"[GraphTwcGenerator] Graph validation failed with {validation.GlobalErrors.Count} global error(s).");
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
                LogicalMap = logicalMap,
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
