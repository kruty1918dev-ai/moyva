using System;
using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.GraphSystem.Runtime;
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
            return CompileGraphToConfiguration(EditorSeed);
        }

        public IReadOnlyList<CompiledLayerMap> CompileGraphToConfiguration(int seed)
        {
            if (!CanCompile(out string reason))
            {
                Debug.LogWarning($"[Moyva TWC Graph Binding] Неможливо скомпілювати граф: {reason}", this);
                LastCompiledLayers = Array.Empty<CompiledLayerMap>();
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
            return LastCompiledLayers;
        }

        public void GenerateFromGraph()
        {
            GenerateFromGraph(EditorSeed);
        }

        public void GenerateFromGraph(int seed)
        {
            if (_compileBeforeGenerate)
                CompileGraphToConfiguration(seed);

            if (Manager == null || Manager.configuration == null)
                return;

            if (_generateBuildLayersAfterCompile)
                TileWorldCreatorLayerOcclusionOptimizer.GenerateCompleteMap(Manager);
            else
                Manager.ExecuteBlueprintLayers();
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
