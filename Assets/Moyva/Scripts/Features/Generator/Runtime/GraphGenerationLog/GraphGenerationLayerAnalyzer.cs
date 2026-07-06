using System.Collections.Generic;
using System.Linq;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.Generator.Runtime.Nodes.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphGenerationLayerAnalyzer
    {
        GraphGenerationLayerAnalysis Analyze(
            GraphAsset graph,
            TileWorldCreatorManager manager,
            GeneratorLayerDefinition layer,
            IReadOnlyDictionary<string, CompiledLayerMap> compiledByLayerId,
            IReadOnlyDictionary<string, List<GraphValidationIssue>> issuesByLayerId,
            ISet<string> skippedLayerIds);
    }

    internal sealed class GraphGenerationLayerAnalyzer : IGraphGenerationLayerAnalyzer
    {
        private readonly IGraphGenerationLayerTwcLookup _twcLookup;
        private readonly IGraphGenerationLayerStatusService _status;

        public GraphGenerationLayerAnalyzer(
            IGraphGenerationLayerTwcLookup twcLookup,
            IGraphGenerationLayerStatusService status)
        {
            _twcLookup = twcLookup;
            _status = status;
        }

        public GraphGenerationLayerAnalysis Analyze(
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
            bool skippedByValidation = skippedLayerIds != null && skippedLayerIds.Contains(layer.Id);
            compiledByLayerId.TryGetValue(layer.Id, out var compiled);
            issuesByLayerId.TryGetValue(layer.Id, out var issues);

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
                Status = _status.ResolveStatus(layer, outputNode, compiled, hasRenderableTileOutput, hasObjectOutput, skippedByValidation),
                Reason = _status.ResolveReason(layer, outputNode, tileNodes, compiled, hasRenderableTileOutput, hasObjectOutput, skippedByValidation, issues),
                GeneratedCells = _twcLookup.CountGeneratedCells(manager, compiled),
                BlueprintName = _twcLookup.ResolveBlueprintName(manager, compiled),
                BuildLayerName = _twcLookup.ResolveBuildLayerName(manager?.configuration, layer.BuildLayerKey, compiled?.BlueprintLayerGuid)
            };
        }
    }
}
