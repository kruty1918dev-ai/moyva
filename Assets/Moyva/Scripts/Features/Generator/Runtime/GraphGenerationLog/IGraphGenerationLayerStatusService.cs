using System.Collections.Generic;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphGenerationLayerStatusService
    {
        string ResolveStatus(GeneratorLayerDefinition layer, OutputNode outputNode, CompiledLayerMap compiled,
            bool hasRenderableTileOutput, bool hasObjectOutput, bool wasSkippedByValidation);

        string ResolveReason(GeneratorLayerDefinition layer, OutputNode outputNode, IReadOnlyList<TileSettingsNode> tileNodes,
            CompiledLayerMap compiled, bool hasRenderableTileOutput, bool hasObjectOutput,
            bool wasSkippedByValidation, IReadOnlyList<GraphValidationIssue> issues);
    }
}
