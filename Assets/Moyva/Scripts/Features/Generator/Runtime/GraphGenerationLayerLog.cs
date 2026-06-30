using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class GraphGenerationLayerLog
    {
        public const string Tag = "[MoyvaGraphGenerationLayers]";

        public static void Emit(
            string source,
            GraphAsset graph,
            TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiledLayers,
            GraphValidationReport validationReport,
            ISet<string> skippedLayerIds,
            int seed,
            Vector2Int mapSize,
            bool buildLayersWereGenerated,
            UnityEngine.Object context = null)
        {
            string log = Build(
                source,
                graph,
                manager,
                compiledLayers,
                validationReport,
                skippedLayerIds,
                seed,
                mapSize,
                buildLayersWereGenerated);

            if (context != null)
                Debug.Log(log, context);
            else
                Debug.Log(log);
        }

        public static string Build(
            string source,
            GraphAsset graph,
            TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiledLayers,
            GraphValidationReport validationReport,
            ISet<string> skippedLayerIds,
            int seed,
            Vector2Int mapSize,
            bool buildLayersWereGenerated)
        {
            var builder = new StringBuilder(4096);
            var compiledByLayerId = compiledLayers?
                .Where(layer => layer != null && !string.IsNullOrEmpty(layer.GraphLayerId))
                .GroupBy(layer => layer.GraphLayerId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal)
                ?? new Dictionary<string, CompiledLayerMap>(StringComparer.Ordinal);
            var issuesByLayerId = GroupIssuesByLayer(validationReport);

            graph?.EnsureLayerGraphStates();
            int totalLayers = graph?.Layers?.Count(layer => layer != null) ?? 0;
            int enabledLayers = graph?.Layers?.Count(layer => layer != null && layer.Enabled) ?? 0;
            int compiledCount = compiledByLayerId.Count;
            int tileOutputCount = compiledByLayerId.Values.Count(layer => layer.HasRenderableTileOutput);
            int skippedCount = totalLayers - compiledCount;

            builder.AppendLine($"{Tag} Graph generation layer report");
            builder.AppendLine($"Source: {ValueOrFallback(source, "unknown")}");
            builder.AppendLine($"Graph: {ValueOrFallback(graph != null ? graph.name : null, "<null>")}");
            builder.AppendLine($"Seed: {seed}");
            builder.AppendLine($"MapSize: {Mathf.Max(1, mapSize.x)}x{Mathf.Max(1, mapSize.y)}");
            builder.AppendLine($"TWC Configuration: {ValueOrFallback(manager?.configuration != null ? manager.configuration.name : null, "<missing>")}");
            builder.AppendLine($"Build Layers Generated: {buildLayersWereGenerated}");
            builder.AppendLine($"Summary: total={totalLayers}, enabled={enabledLayers}, compiledBlueprint={compiledCount}, renderableTileOutput={tileOutputCount}, skipped={Mathf.Max(0, skippedCount)}, validationErrors={validationReport?.ErrorCount ?? 0}, validationWarnings={validationReport?.WarningCount ?? 0}");

            AppendGlobalIssues(builder, validationReport);

            builder.AppendLine("Layers:");
            if (graph?.Layers == null || totalLayers == 0)
            {
                builder.AppendLine("  - <no graph layers>");
                return builder.ToString();
            }

            var orderedLayers = graph.Layers
                .Where(layer => layer != null)
                .OrderBy(layer => layer.SortingOrder)
                .ThenBy(layer => layer.Name, StringComparer.Ordinal)
                .ToList();

            for (int i = 0; i < orderedLayers.Count; i++)
            {
                AppendLayer(
                    builder,
                    graph,
                    manager,
                    orderedLayers[i],
                    compiledByLayerId,
                    issuesByLayerId,
                    skippedLayerIds);
            }

            return builder.ToString();
        }

        private static void AppendLayer(
            StringBuilder builder,
            GraphAsset graph,
            TileWorldCreatorManager manager,
            GeneratorLayerDefinition layer,
            IReadOnlyDictionary<string, CompiledLayerMap> compiledByLayerId,
            IReadOnlyDictionary<string, List<GraphValidationIssue>> issuesByLayerId,
            ISet<string> skippedLayerIds)
        {
            var nodes = graph.GetNodesForLayer(layer.Id);
            var outputNode = GraphLayerRuntimeSemantics.GetLayerOutputNode(graph, layer.Id);
            var tileNodes = nodes.OfType<TileSettingsNode>().ToList();
            bool hasRenderableTileOutput = GraphLayerRuntimeSemantics.HasRenderableTileOutput(graph, layer.Id);
            bool hasObjectOutput = nodes.Any(node => node is ObjectOutputToTWCNode);
            bool wasSkippedByValidation = skippedLayerIds != null && skippedLayerIds.Contains(layer.Id);
            compiledByLayerId.TryGetValue(layer.Id, out var compiled);
            issuesByLayerId.TryGetValue(layer.Id, out var layerIssues);

            string status = ResolveStatus(layer, outputNode, compiled, hasRenderableTileOutput, hasObjectOutput, wasSkippedByValidation);
            string reason = ResolveReason(layer, outputNode, tileNodes, compiled, hasRenderableTileOutput, hasObjectOutput, wasSkippedByValidation, layerIssues);
            int generatedCells = CountGeneratedCells(manager, compiled);
            string blueprintName = ResolveBlueprintName(manager, compiled);
            string buildLayerName = ResolveBuildLayerName(manager?.configuration, layer.BuildLayerKey, compiled?.BlueprintLayerGuid);

            builder.AppendLine(
                $"  - [{status}] order={layer.SortingOrder} name='{ValueOrFallback(layer.Name, "<unnamed>")}' id={layer.Id}");
            builder.AppendLine(
                $"      enabled={layer.Enabled}, output={outputNode?.OutputKind.ToString() ?? "<missing>"}, nodes={nodes.Count}, tileSettings={tileNodes.Count}, objectOutput={hasObjectOutput}");
            builder.AppendLine(
                $"      compiledBlueprint={compiled != null}, blueprint='{ValueOrFallback(blueprintName, "<none>")}', blueprintGuid='{ValueOrFallback(compiled?.BlueprintLayerGuid ?? layer.BlueprintLayerGuid, "<none>")}', buildLayer='{ValueOrFallback(buildLayerName, "<none>")}', buildKey='{ValueOrFallback(layer.BuildLayerKey, "<none>")}'");
            builder.AppendLine(
                $"      gridTileId='{ValueOrFallback(compiled?.GridTileId, "<none>")}', renderableTiles={hasRenderableTileOutput}, generatedCells={FormatGeneratedCells(generatedCells)}");
            builder.AppendLine($"      reason: {reason}");

            if (layerIssues != null && layerIssues.Count > 0)
            {
                for (int i = 0; i < layerIssues.Count; i++)
                    builder.AppendLine($"      validation: {layerIssues[i]}");
            }
        }

        private static string ResolveStatus(
            GeneratorLayerDefinition layer,
            OutputNode outputNode,
            CompiledLayerMap compiled,
            bool hasRenderableTileOutput,
            bool hasObjectOutput,
            bool wasSkippedByValidation)
        {
            if (!layer.Enabled)
                return "SKIPPED_DISABLED";
            if (wasSkippedByValidation)
                return "SKIPPED_VALIDATION";
            if (compiled == null)
                return "NOT_COMPILED";
            if (hasRenderableTileOutput)
                return "WILL_GENERATE_TILES";
            if (hasObjectOutput || outputNode?.OutputKind == LayerOutputKind.Objects)
                return "WILL_GENERATE_OBJECTS";
            if (outputNode?.OutputKind == LayerOutputKind.Masks)
                return "WILL_GENERATE_MASK";
            if (outputNode?.OutputKind == LayerOutputKind.InternalData)
                return "INTERNAL_DATA";
            return "COMPILED_NO_SCENE_TILES";
        }

        private static string ResolveReason(
            GeneratorLayerDefinition layer,
            OutputNode outputNode,
            IReadOnlyList<TileSettingsNode> tileNodes,
            CompiledLayerMap compiled,
            bool hasRenderableTileOutput,
            bool hasObjectOutput,
            bool wasSkippedByValidation,
            IReadOnlyList<GraphValidationIssue> layerIssues)
        {
            if (!layer.Enabled)
                return "Layer disabled in graph.";

            if (wasSkippedByValidation)
                return "Layer has validation error(s): " + FormatIssueCodes(layerIssues);

            if (compiled == null)
                return "Layer was not returned by compiler. Check global validation/configuration state.";

            if (outputNode == null)
                return "No Output node found for this layer.";

            if (hasRenderableTileOutput)
                return "Tile output is enabled and at least one Tile Settings node has a preset or flat surface.";

            if (hasObjectOutput || outputNode.OutputKind == LayerOutputKind.Objects)
                return "Object layer path; scene objects are generated through TWC object build layers, not terrain tile output.";

            if (outputNode.OutputKind == LayerOutputKind.Masks)
                return "Mask/helper layer; compiled as a blueprint mask for Layer Ref/TWC modifiers, but no terrain tile GameObject is spawned.";

            if (outputNode.OutputKind == LayerOutputKind.InternalData)
                return "Internal data layer; available to graph logic, but not rendered as terrain tiles.";

            if (tileNodes.Count == 0)
                return "No Tile Settings node in layer.";

            return "Tile Settings nodes exist, but none has a configured TilePreset or flat surface.";
        }

        private static Dictionary<string, List<GraphValidationIssue>> GroupIssuesByLayer(GraphValidationReport report)
        {
            var result = new Dictionary<string, List<GraphValidationIssue>>(StringComparer.Ordinal);
            if (report?.Issues == null)
                return result;

            foreach (var issue in report.Issues)
            {
                if (issue == null || string.IsNullOrEmpty(issue.LayerId))
                    continue;

                if (!result.TryGetValue(issue.LayerId, out var issues))
                {
                    issues = new List<GraphValidationIssue>();
                    result[issue.LayerId] = issues;
                }

                issues.Add(issue);
            }

            return result;
        }

        private static void AppendGlobalIssues(StringBuilder builder, GraphValidationReport report)
        {
            if (report?.Issues == null)
                return;

            var globalIssues = report.Issues
                .Where(issue => issue != null && string.IsNullOrEmpty(issue.LayerId))
                .ToList();
            if (globalIssues.Count == 0)
                return;

            builder.AppendLine("Global validation:");
            for (int i = 0; i < globalIssues.Count; i++)
                builder.AppendLine($"  - {globalIssues[i]}");
        }

        private static int CountGeneratedCells(TileWorldCreatorManager manager, CompiledLayerMap compiled)
        {
            if (manager == null || compiled == null || string.IsNullOrEmpty(compiled.BlueprintLayerGuid))
                return -1;

            var blueprint = manager.GetBlueprintLayerByGuid(compiled.BlueprintLayerGuid);
            return blueprint?.allPositions?.Count ?? 0;
        }

        private static string ResolveBlueprintName(TileWorldCreatorManager manager, CompiledLayerMap compiled)
        {
            if (compiled == null)
                return null;

            if (manager != null && !string.IsNullOrEmpty(compiled.BlueprintLayerGuid))
            {
                var blueprint = manager.GetBlueprintLayerByGuid(compiled.BlueprintLayerGuid);
                if (!string.IsNullOrWhiteSpace(blueprint?.layerName))
                    return blueprint.layerName;
            }

            return compiled.LayerName;
        }

        private static string ResolveBuildLayerName(Configuration configuration, string buildLayerKey, string blueprintLayerGuid)
        {
            var buildLayer = FindTilesBuildLayer(configuration, buildLayerKey, blueprintLayerGuid);
            return buildLayer?.layerName;
        }

        private static TilesBuildLayer FindTilesBuildLayer(Configuration configuration, string buildLayerKey, string blueprintLayerGuid)
        {
            if (configuration?.buildLayerFolders == null)
                return null;

            for (int folderIndex = 0; folderIndex < configuration.buildLayerFolders.Count; folderIndex++)
            {
                var folder = configuration.buildLayerFolders[folderIndex];
                if (folder?.buildLayers == null)
                    continue;

                for (int layerIndex = 0; layerIndex < folder.buildLayers.Count; layerIndex++)
                {
                    if (folder.buildLayers[layerIndex] is not TilesBuildLayer buildLayer)
                        continue;

                    if (!string.IsNullOrWhiteSpace(buildLayerKey)
                        && string.Equals(buildLayer.guid, buildLayerKey, StringComparison.Ordinal))
                        return buildLayer;

                    if (!string.IsNullOrWhiteSpace(blueprintLayerGuid)
                        && (string.Equals(buildLayer.assignedBlueprintLayerGuid, blueprintLayerGuid, StringComparison.Ordinal)
                            || string.Equals(buildLayer.currentBlueprintLayer?.guid, blueprintLayerGuid, StringComparison.Ordinal)))
                        return buildLayer;
                }
            }

            return null;
        }

        private static string FormatIssueCodes(IReadOnlyList<GraphValidationIssue> issues)
        {
            if (issues == null || issues.Count == 0)
                return "unknown validation error";

            return string.Join(", ", issues.Select(issue => issue.Code));
        }

        private static string FormatGeneratedCells(int count)
        {
            return count < 0 ? "<unknown>" : count.ToString();
        }

        private static string ValueOrFallback(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }
    }
}
