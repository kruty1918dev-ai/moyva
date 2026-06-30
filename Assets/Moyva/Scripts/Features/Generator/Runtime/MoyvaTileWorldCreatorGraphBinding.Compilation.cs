using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.GraphSystem.Runtime;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Компіляція графа у TileWorldCreator <see cref="Configuration"/> та повна генерація мапи.
    /// </summary>
    public sealed partial class MoyvaTileWorldCreatorGraphBinding
    {
        public IReadOnlyList<CompiledLayerMap> CompileGraphToConfiguration()
        {
            return CompileGraphToConfiguration(ResolveGenerationSeed());
        }

        public IReadOnlyList<CompiledLayerMap> CompileGraphToConfiguration(int seed)
        {
            return CompileGraphToConfiguration(seed, true);
        }

        private IReadOnlyList<CompiledLayerMap> CompileGraphToConfiguration(int seed, bool emitLayerLog)
        {
            if (!CanCompile(out string reason))
            {
                Debug.LogWarning($"[Moyva TWC Graph Binding] Неможливо скомпілювати граф: {reason}", this);
                LastCompiledLayers = Array.Empty<CompiledLayerMap>();
                if (emitLayerLog)
                {
                    GraphGenerationLayerLog.Emit(
                        "Graph Binding Compile",
                        _graphAsset,
                        Manager,
                        LastCompiledLayers,
                        null,
                        null,
                        NormalizeSeed(seed),
                        ResolveGraphMapSize(),
                        false,
                        this);
                }
                return LastCompiledLayers;
            }

            var validator = new GraphValidator();
            var report = validator.ValidateDetailed(_graphAsset);
            var globalErrors = GetGlobalValidationErrors(report);
            if (globalErrors.Count > 0)
            {
                Debug.LogWarning(
                    $"[Moyva TWC Graph Binding] Неможливо скомпілювати граф: {globalErrors.Count} global validation error(s).\n{FormatValidationIssues(globalErrors)}",
                    this);
                LastCompiledLayers = Array.Empty<CompiledLayerMap>();
                if (emitLayerLog)
                {
                    GraphGenerationLayerLog.Emit(
                        "Graph Binding Compile",
                        _graphAsset,
                        Manager,
                        LastCompiledLayers,
                        report,
                        null,
                        NormalizeSeed(seed),
                        ResolveGraphMapSize(),
                        false,
                        this);
                }
                return LastCompiledLayers;
            }

            var skippedLayerIds = GetInvalidLayerIds(report);
            if (skippedLayerIds.Count > 0)
            {
                Debug.LogWarning(
                    $"[Moyva TWC Graph Binding] {skippedLayerIds.Count} layer(s) skipped because of validation errors.\n{FormatValidationReport(report)}",
                    this);
            }

            int normalizedSeed = NormalizeSeed(seed);
            LastCompiledLayers = GraphToConfigurationCompiler.Compile(_graphAsset, Manager, normalizedSeed, skippedLayerIds);
            if (emitLayerLog)
            {
                GraphGenerationLayerLog.Emit(
                    "Graph Binding Compile",
                    _graphAsset,
                    Manager,
                    LastCompiledLayers,
                    report,
                    skippedLayerIds,
                    normalizedSeed,
                    ResolveGraphMapSize(),
                    false,
                    this);
            }
            return LastCompiledLayers;
        }

        public void GenerateFromGraph()
        {
            GenerateFromGraph(ResolveGenerationSeed());
        }

        public void GenerateFromGraph(int seed)
        {
            if (_isGenerating)
            {
                Debug.LogWarning("[Moyva TWC Graph Binding] Генерація вже виконується, повторний запуск пропущено.", this);
                return;
            }

            _isGenerating = true;
            int normalizedSeed = NormalizeSeed(seed);
            try
            {
                if (_compileBeforeGenerate)
                    CompileGraphToConfiguration(normalizedSeed, false);

                if (Manager == null || Manager.configuration == null)
                {
                    GraphGenerationLayerLog.Emit(
                        "Graph Binding Generate",
                        _graphAsset,
                        Manager,
                        LastCompiledLayers,
                        null,
                        null,
                        normalizedSeed,
                        ResolveGraphMapSize(),
                        false,
                        this);
                    return;
                }

                if (_generateBuildLayersAfterCompile)
                    TileWorldCreatorLayerOcclusionOptimizer.GenerateCompleteMap(Manager);
                else
                    TileWorldCreatorLayerOcclusionOptimizer.GenerateBlueprintMap(Manager);

                var mapSize = ResolveGraphMapSize();
                var logicalMap = GraphLogicalTileMapBuilder.Build(
                    _graphAsset,
                    Manager,
                    LastCompiledLayers,
                    mapSize.x,
                    mapSize.y);
                GraphLogicalTileMapDiagnostics.EmitAndCompare(
                    "Scene build mask",
                    _graphAsset,
                    normalizedSeed,
                    logicalMap,
                    this);

                var validator = new GraphValidator();
                GraphValidationReport report = _graphAsset != null
                    ? validator.ValidateDetailed(_graphAsset)
                    : null;
                GraphGenerationLayerLog.Emit(
                    "Graph Binding Generate",
                    _graphAsset,
                    Manager,
                    LastCompiledLayers,
                    report,
                    GetInvalidLayerIds(report),
                    normalizedSeed,
                    ResolveGraphMapSize(),
                    _generateBuildLayersAfterCompile,
                    this);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Moyva TWC Graph Binding] Помилка генерації мапи: {ex}", this);
            }
            finally
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.ClearProgressBar();
#endif
                _isGenerating = false;
            }
        }

        private Vector2Int ResolveGraphMapSize()
        {
            if (_graphAsset?.SharedSettings != null && _graphAsset.SharedSettings.HasMapSize)
                return _graphAsset.SharedSettings.MapSize;

            var configuration = Manager != null ? Manager.configuration : null;
            if (configuration != null)
                return new Vector2Int(Mathf.Max(1, configuration.width), Mathf.Max(1, configuration.height));

            return new Vector2Int(1, 1);
        }

        private int ResolveGenerationSeed()
        {
            if (GameLaunchContext.TryGetSeed(out int launchSeed))
                return NormalizeSeed(launchSeed);

            if (_graphAsset?.Nodes != null)
            {
                foreach (var node in _graphAsset.Nodes)
                {
                    if (node is ISeedProvider seedProvider)
                        return NormalizeSeed(seedProvider.Seed);
                }
            }

            return EditorSeed;
        }

        private static string FormatValidationReport(Kruty1918.Moyva.GraphSystem.API.GraphValidationReport report)
        {
            if (report == null || report.Issues.Count == 0)
                return string.Empty;

            return FormatValidationIssues(report.Issues);
        }

        private static string FormatValidationIssues(IEnumerable<Kruty1918.Moyva.GraphSystem.API.GraphValidationIssue> issues)
        {
            var builder = new System.Text.StringBuilder();
            foreach (var issue in issues)
                builder.AppendLine($"  - {issue}");
            return builder.ToString();
        }

        private static List<Kruty1918.Moyva.GraphSystem.API.GraphValidationIssue> GetGlobalValidationErrors(
            Kruty1918.Moyva.GraphSystem.API.GraphValidationReport report)
        {
            var result = new List<Kruty1918.Moyva.GraphSystem.API.GraphValidationIssue>();
            if (report == null)
                return result;

            foreach (var issue in report.Issues)
            {
                if (issue.Severity == Kruty1918.Moyva.GraphSystem.API.ValidationSeverity.Error
                    && string.IsNullOrEmpty(issue.LayerId))
                    result.Add(issue);
            }

            return result;
        }

        private static HashSet<string> GetInvalidLayerIds(
            Kruty1918.Moyva.GraphSystem.API.GraphValidationReport report)
        {
            var result = new HashSet<string>();
            if (report == null)
                return result;

            foreach (var issue in report.Issues)
            {
                if (issue.Severity == Kruty1918.Moyva.GraphSystem.API.ValidationSeverity.Error
                    && !string.IsNullOrEmpty(issue.LayerId))
                    result.Add(issue.LayerId);
            }

            return result;
        }
    }
}
