using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.GraphSystem.API;
using GraphIssue = Kruty1918.Moyva.GraphSystem.API.GraphValidationIssue;
using GraphLayerDefinition = Kruty1918.Moyva.GraphSystem.API.GeneratorLayerDefinition;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphGenerationLayerAnalyzer
    {
        GraphGenerationLayerAnalysis Analyze(
            GraphAsset graph,
            TileWorldCreatorManager manager,
            GraphLayerDefinition layer,
            IReadOnlyDictionary<string, CompiledLayerMap> compiledByLayerId,
            IReadOnlyDictionary<string, List<GraphIssue>> issuesByLayerId,
            ISet<string> skippedLayerIds);
    }
}
