using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphGenerationLayerAnalysis
    {
        public GeneratorLayerDefinition Layer;
        public OutputNode OutputNode;
        public IReadOnlyList<TileSettingsNode> TileNodes;
        public IReadOnlyList<GraphValidationIssue> Issues;
        public CompiledLayerMap Compiled;
        public string Status;
        public string Reason;
        public string BlueprintName;
        public string BuildLayerName;
        public int NodeCount;
        public int GeneratedCells;
        public bool HasRenderableTileOutput;
        public bool HasObjectOutput;
    }
}
