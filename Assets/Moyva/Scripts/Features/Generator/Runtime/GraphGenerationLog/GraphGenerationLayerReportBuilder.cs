using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphGenerationLayerReportBuilder : IGraphGenerationLayerReportBuilder
    {
        private readonly IGraphGenerationLayerIssueService _issues;
        private readonly IGraphGenerationLayerAnalyzer _analyzer;

        public GraphGenerationLayerReportBuilder(
            IGraphGenerationLayerIssueService issues,
            IGraphGenerationLayerAnalyzer analyzer)
        {
            _issues = issues;
            _analyzer = analyzer;
        }

        public string Build(GraphGenerationLayerLogRequest request)
        {
            var builder = new StringBuilder(4096);
            var compiledByLayerId = CreateCompiledMap(request.CompiledLayers);
            var issuesByLayerId = _issues.GroupByLayer(request.ValidationReport);
            request.Graph?.EnsureLayerGraphStates();
            AppendHeader(builder, request, compiledByLayerId);
            AppendGlobalIssues(builder, request.ValidationReport);
            AppendLayers(builder, request, compiledByLayerId, issuesByLayerId);
            return builder.ToString();
        }

        private void AppendHeader(StringBuilder builder, GraphGenerationLayerLogRequest request,
            IReadOnlyDictionary<string, CompiledLayerMap> compiledByLayerId)
        {
            int total = request.Graph?.Layers?.Count(layer => layer != null) ?? 0;
            int enabled = request.Graph?.Layers?.Count(layer => layer != null && layer.Enabled) ?? 0;
            int tileOutput = compiledByLayerId.Values.Count(layer => layer.HasRenderableTileOutput);
            builder.AppendLine($"{GraphGenerationLayerLog.Tag} Graph generation layer report");
            builder.AppendLine($"Source: {GraphGenerationLayerLogText.ValueOrFallback(request.Source, "unknown")}");
            builder.AppendLine($"Graph: {GraphGenerationLayerLogText.ValueOrFallback(request.Graph != null ? request.Graph.name : null, "<null>")}");
            builder.AppendLine($"Seed: {request.Seed}");
            builder.AppendLine($"MapSize: {Mathf.Max(1, request.MapSize.x)}x{Mathf.Max(1, request.MapSize.y)}");
            builder.AppendLine($"TWC Configuration: {GraphGenerationLayerLogText.ValueOrFallback(request.Manager?.configuration != null ? request.Manager.configuration.name : null, "<missing>")}");
            builder.AppendLine($"Build Layers Generated: {request.BuildLayersWereGenerated}");
            builder.AppendLine($"Summary: total={total}, enabled={enabled}, compiledBlueprint={compiledByLayerId.Count}, " +
                               $"renderableTileOutput={tileOutput}, skipped={Mathf.Max(0, total - compiledByLayerId.Count)}, " +
                               $"validationErrors={request.ValidationReport?.ErrorCount ?? 0}, validationWarnings={request.ValidationReport?.WarningCount ?? 0}");
        }

        private void AppendGlobalIssues(StringBuilder builder, GraphValidationReport report)
        {
            var globalIssues = _issues.GetGlobalIssues(report);
            if (globalIssues.Count == 0)
                return;

            builder.AppendLine("Global validation:");
            for (int i = 0; i < globalIssues.Count; i++)
                builder.AppendLine($"  - {globalIssues[i]}");
        }

        private void AppendLayers(StringBuilder builder, GraphGenerationLayerLogRequest request,
            IReadOnlyDictionary<string, CompiledLayerMap> compiledByLayerId,
            IReadOnlyDictionary<string, List<GraphValidationIssue>> issuesByLayerId)
        {
            builder.AppendLine("Layers:");
            if (request.Graph?.Layers == null || request.Graph.Layers.Count == 0)
            {
                builder.AppendLine("  - <no graph layers>");
                return;
            }

            foreach (var layer in OrderedLayers(request.Graph))
                AppendLayer(builder, _analyzer.Analyze(request.Graph, request.Manager, layer, compiledByLayerId, issuesByLayerId, request.SkippedLayerIds));
        }

        private static IEnumerable<GeneratorLayerDefinition> OrderedLayers(GraphAsset graph)
        {
            return graph.Layers
                .Where(layer => layer != null)
                .OrderBy(layer => layer.SortingOrder)
                .ThenBy(layer => layer.Name, StringComparer.Ordinal);
        }

        private static Dictionary<string, CompiledLayerMap> CreateCompiledMap(IReadOnlyList<CompiledLayerMap> compiledLayers)
        {
            return compiledLayers?
                .Where(layer => layer != null && !string.IsNullOrEmpty(layer.GraphLayerId))
                .GroupBy(layer => layer.GraphLayerId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal)
                ?? new Dictionary<string, CompiledLayerMap>(StringComparer.Ordinal);
        }

        private static void AppendLayer(StringBuilder builder, GraphGenerationLayerAnalysis analysis)
        {
            var layer = analysis.Layer;
            builder.AppendLine($"  - [{analysis.Status}] order={layer.SortingOrder} name='{GraphGenerationLayerLogText.ValueOrFallback(layer.Name, "<unnamed>")}' id={layer.Id}");
            builder.AppendLine($"      enabled={layer.Enabled}, output={analysis.OutputNode?.OutputKind.ToString() ?? "<missing>"}, nodes={analysis.NodeCount}, tileSettings={analysis.TileNodes.Count}, objectOutput={analysis.HasObjectOutput}");
            builder.AppendLine($"      compiledBlueprint={analysis.Compiled != null}, blueprint='{GraphGenerationLayerLogText.ValueOrFallback(analysis.BlueprintName, "<none>")}', blueprintGuid='{GraphGenerationLayerLogText.ValueOrFallback(analysis.Compiled?.BlueprintLayerGuid ?? layer.BlueprintLayerGuid, "<none>")}', buildLayer='{GraphGenerationLayerLogText.ValueOrFallback(analysis.BuildLayerName, "<none>")}', buildKey='{GraphGenerationLayerLogText.ValueOrFallback(layer.BuildLayerKey, "<none>")}'");
            builder.AppendLine($"      gridTileId='{GraphGenerationLayerLogText.ValueOrFallback(analysis.Compiled?.GridTileId, "<none>")}', renderableTiles={analysis.HasRenderableTileOutput}, generatedCells={GraphGenerationLayerLogText.FormatGeneratedCells(analysis.GeneratedCells)}");
            builder.AppendLine($"      reason: {analysis.Reason}");
            AppendLayerIssues(builder, analysis.Issues);
        }

        private static void AppendLayerIssues(StringBuilder builder, IReadOnlyList<GraphValidationIssue> issues)
        {
            if (issues == null)
                return;

            for (int i = 0; i < issues.Count; i++)
                builder.AppendLine($"      validation: {issues[i]}");
        }
    }
}
