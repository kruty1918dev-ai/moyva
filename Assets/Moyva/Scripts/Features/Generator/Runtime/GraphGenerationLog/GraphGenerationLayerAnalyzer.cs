using System.Collections.Generic;
using System.Linq;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using GraphIssue = Kruty1918.Moyva.GraphSystem.API.GraphValidationIssue;
using GraphLayerDefinition = Kruty1918.Moyva.GraphSystem.API.GeneratorLayerDefinition;

namespace Kruty1918.Moyva.Generator.Runtime
{

    internal sealed class GraphGenerationLayerAnalyzer : IGraphGenerationLayerAnalyzer
    {
        private readonly IGraphGenerationLayerTwcLookup _twcLookup;

        public GraphGenerationLayerAnalyzer(
            IGraphGenerationLayerTwcLookup twcLookup,
            IGraphGenerationLayerStatusService status)
        {
            _twcLookup = twcLookup;
        }

        public GraphGenerationLayerAnalysis Analyze(
            GraphAsset graph,
            TileWorldCreatorManager manager,
            GraphLayerDefinition layer,
            IReadOnlyDictionary<string, CompiledLayerMap> compiledByLayerId,
            IReadOnlyDictionary<string, List<GraphIssue>> issuesByLayerId,
            ISet<string> skippedLayerIds)
        {
            if (graph == null || layer == null)
                return null;

            var nodes = graph.GetNodesForLayer(layer.Id);
            var outputNode = GraphLayerRuntimeSemantics.GetLayerOutputNode(graph, layer.Id);
            var tileNodes = nodes.OfType<TileSettingsNode>().ToList();

            bool hasRenderableTileOutput = GraphLayerRuntimeSemantics.HasRenderableTileOutput(graph, layer.Id);
            bool hasObjectOutput = nodes.Any(node => node is ObjectOutputToTWCNode);
            bool skippedByValidation = skippedLayerIds != null && skippedLayerIds.Contains(layer.Id);

            CompiledLayerMap compiled = null;
            if (compiledByLayerId != null)
                compiledByLayerId.TryGetValue(layer.Id, out compiled);

            List<GraphIssue> issues = null;
            if (issuesByLayerId != null)
                issuesByLayerId.TryGetValue(layer.Id, out issues);

            return new GraphGenerationLayerAnalysis
            {
                Layer = layer,
                OutputNode = outputNode,
                TileNodes = tileNodes,
                Issues = issues,
                Compiled = compiled,
                NodeCount = nodes.Count,
                HasRenderableTileOutput = hasRenderableTileOutput,
                HasObjectOutput = hasObjectOutput,

                // One-file fix:
                // We do not call IGraphGenerationLayerStatusService here because it currently expects
                // another GeneratorLayerDefinition / GraphValidationIssue type.
                Status = default,

                Reason = ResolveReason(
                    outputNode,
                    tileNodes,
                    compiled,
                    hasRenderableTileOutput,
                    hasObjectOutput,
                    skippedByValidation,
                    issues),

                GeneratedCells = _twcLookup.CountGeneratedCells(manager, compiled),
                BlueprintName = _twcLookup.ResolveBlueprintName(manager, compiled),
                BuildLayerName = _twcLookup.ResolveBuildLayerName(
                    manager?.configuration,
                    layer.BuildLayerKey,
                    compiled?.BlueprintLayerGuid)
            };
        }

        private static string ResolveReason(
            object outputNode,
            IReadOnlyList<TileSettingsNode> tileNodes,
            CompiledLayerMap compiled,
            bool hasRenderableTileOutput,
            bool hasObjectOutput,
            bool skippedByValidation,
            IReadOnlyList<GraphIssue> issues)
        {
            if (skippedByValidation)
            {
                if (issues != null && issues.Count > 0)
                    return "Skipped by validation: " + string.Join("; ", issues.Select(issue => issue?.ToString()));

                return "Skipped by validation.";
            }

            if (outputNode == null)
                return "Layer has no output node, so it cannot be processed as a final generation layer.";

            if (hasObjectOutput)
                return "Layer has object output and will generate object placement data.";

            if (hasRenderableTileOutput)
            {
                if (tileNodes != null && tileNodes.Count > 0)
                    return "Layer has renderable tile output and tile settings, so it will generate TileWorldCreator tile output.";

                return "Layer has renderable tile output, but no TileSettingsNode was found.";
            }

            if (compiled != null)
                return "Mask/helper layer; compiled as a blueprint mask for references or modifiers, but no terrain tile GameObject is spawned.";

            return "Layer did not produce compiled output.";
        }
    }
}