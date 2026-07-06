using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Nodes;

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

    internal sealed class GraphGenerationLayerStatusService : IGraphGenerationLayerStatusService
    {
        private readonly IGraphGenerationLayerIssueService _issues;

        public GraphGenerationLayerStatusService(IGraphGenerationLayerIssueService issues)
        {
            _issues = issues;
        }

        public string ResolveStatus(GeneratorLayerDefinition layer, OutputNode outputNode, CompiledLayerMap compiled,
            bool hasRenderableTileOutput, bool hasObjectOutput, bool wasSkippedByValidation)
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

        public string ResolveReason(GeneratorLayerDefinition layer, OutputNode outputNode, IReadOnlyList<TileSettingsNode> tileNodes,
            CompiledLayerMap compiled, bool hasRenderableTileOutput, bool hasObjectOutput,
            bool wasSkippedByValidation, IReadOnlyList<GraphValidationIssue> issues)
        {
            if (!layer.Enabled)
                return "Layer disabled in graph.";
            if (wasSkippedByValidation)
                return "Layer has validation error(s): " + _issues.FormatIssueCodes(issues);
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
            return tileNodes.Count == 0
                ? "No Tile Settings node in layer."
                : "Tile Settings nodes exist, but none has a configured TilePreset or flat surface.";
        }
    }
}
