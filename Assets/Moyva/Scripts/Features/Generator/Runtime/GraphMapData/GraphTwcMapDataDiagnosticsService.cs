using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphTwcMapDataDiagnosticsService
    {
        void LogEnter(GraphAsset graph, TileWorldCreatorManager manager, int seed, int width, int height);
        void LogValidation(GraphTwcValidationResult validation);
        void LogCompileCall(GraphAsset graph, int width, int height);
        void LogCompileResult(TileWorldCreatorManager manager, IReadOnlyList<CompiledLayerMap> compiled, int skippedCount);
        void LogTwcCall(TileWorldCreatorManager manager);
        void LogTwcResult(long elapsedMs, Array biomeMap, Array heightMap, Array objectMap);
        void LogExit(int width, int height, Array biomeMap, Array heightMap, Array objectMap, Array buildingMap);
        void LogMissingManager();
        void LogValidationFailed(IGraphTwcValidationService validationService, GraphTwcValidationResult validation);
        void LogSkippedLayers(IGraphTwcValidationService validationService, GraphTwcValidationResult validation);
        void LogException(Exception exception);
        void EmitLayerLog(GraphTwcMapGenerationRequest request, GraphTwcValidationResult validation,
            ISet<string> skippedLayerIds, int seed, Vector2Int mapSize, bool success,
            IReadOnlyList<CompiledLayerMap> compiledLayers);
    }

    internal sealed class GraphTwcMapDataDiagnosticsService : IGraphTwcMapDataDiagnosticsService
    {
        private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";
        private readonly IGraphGenerationLayerLogService _layerLog;

        public GraphTwcMapDataDiagnosticsService(IGraphGenerationLayerLogService layerLog)
        {
            _layerLog = layerLog;
        }

        public void LogEnter(GraphAsset graph, TileWorldCreatorManager manager, int seed, int width, int height)
        {
            Debug.Log($"{WorldGenDiagTag} GraphMapData.Generate ENTER frame={Time.frameCount}, graph={(graph != null ? graph.name : "null")}, " +
                      $"seed={seed}, map={width}x{height}, hasGraph={graph != null}, hasTwcManager={manager != null}");
        }

        public void LogValidation(GraphTwcValidationResult validation)
        {
            Debug.Log($"{WorldGenDiagTag} GraphMapData.Validation result={(validation.HasGlobalErrors ? "invalid" : "valid")}, " +
                      $"errors={validation.ErrorCount}, warnings={validation.WarningCount}");
        }

        public void LogCompileCall(GraphAsset graph, int width, int height)
        {
            Debug.Log($"{WorldGenDiagTag} GraphMapData.CALL GraphToConfigurationCompiler.Compile graph={(graph != null ? graph.name : "null")}, map={width}x{height}");
        }

        public void LogCompileResult(TileWorldCreatorManager manager, IReadOnlyList<CompiledLayerMap> compiled, int skippedCount)
        {
            Debug.Log($"{WorldGenDiagTag} GraphMapData.Compile.RESULT config={(manager.configuration != null ? manager.configuration.name : "null")}, " +
                      $"layers={compiled?.Count ?? 0}, renderableLayers={CountRenderableLayers(compiled)}, skippedLayers={skippedCount}");
        }

        public void LogTwcCall(TileWorldCreatorManager manager)
        {
            Debug.Log($"{WorldGenDiagTag} GraphMapData.CALL TWC.GenerateCompleteMap frame={Time.frameCount}, config={(manager.configuration != null ? manager.configuration.name : "null")}");
        }

        public void LogTwcResult(long elapsedMs, Array biomeMap, Array heightMap, Array objectMap)
        {
            Debug.Log($"{WorldGenDiagTag} GraphMapData.TWC.RESULT frame={Time.frameCount}, elapsedMs={elapsedMs}, " +
                      $"hasBiomeMap={biomeMap != null}, hasHeightMap={heightMap != null}, hasObjectMap={objectMap != null}");
        }

        public void LogExit(int width, int height, Array biomeMap, Array heightMap, Array objectMap, Array buildingMap)
        {
            Debug.Log($"{WorldGenDiagTag} GraphMapData.EXIT worldData map={width}x{height}, biomeMap={FormatMapSize(biomeMap)}, " +
                      $"heightMap={FormatMapSize(heightMap)}, objectMap={FormatMapSize(objectMap)}, buildingMap={FormatMapSize(buildingMap)}");
        }

        public void LogMissingManager()
        {
            Debug.LogError("[GraphTwcGenerator] TileWorldCreatorManager/Configuration відсутній. Перевірте MoyvaTileWorldCreatorGraphBinding на TileWorldCreatorManager.");
        }

        public void LogValidationFailed(IGraphTwcValidationService validationService, GraphTwcValidationResult validation)
        {
            Debug.LogError($"[GraphTwcGenerator] Graph validation failed with {validation.GlobalErrors.Count} global error(s):\n{validationService.FormatIssues(validation.GlobalErrors)}");
        }

        public void LogSkippedLayers(IGraphTwcValidationService validationService, GraphTwcValidationResult validation)
        {
            if (validation.SkippedLayerIds.Count > 0)
                Debug.LogWarning($"[GraphTwcGenerator] {validation.SkippedLayerIds.Count} layer(s) skipped because of validation errors:\n{validationService.FormatReport(validation.Report)}");
        }

        public void LogException(Exception exception)
        {
            Debug.LogError($"[GraphTwcGenerator] Помилка генерації: {exception}");
        }

        public void EmitLayerLog(GraphTwcMapGenerationRequest request, GraphTwcValidationResult validation,
            ISet<string> skippedLayerIds, int seed, Vector2Int mapSize, bool success,
            IReadOnlyList<CompiledLayerMap> compiledLayers)
        {
            _layerLog.Emit(new GraphGenerationLayerLogRequest(
                "Runtime GraphTwcMapDataGenerator",
                request.Graph,
                request.Manager,
                compiledLayers,
                validation?.Report,
                skippedLayerIds,
                seed,
                mapSize,
                success));
        }

        private static int CountRenderableLayers(IReadOnlyList<CompiledLayerMap> compiled)
        {
            if (compiled == null)
                return 0;
            int count = 0;
            foreach (var layer in compiled)
                if (layer != null && layer.HasRenderableTileOutput)
                    count++;
            return count;
        }

        private static string FormatMapSize(Array map)
        {
            return map is { Rank: 2 } ? $"{map.GetLength(0)}x{map.GetLength(1)}" : "null";
        }
    }
}
