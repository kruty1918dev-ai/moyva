using System.Linq;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Centralizes graph-layer runtime semantics so editor preview, TWC compilation,
    /// and scene generation agree on which layers produce renderable terrain.
    /// </summary>
    public static class GraphLayerRuntimeSemantics
    {
        public static bool HasRenderableTileOutput(GraphAsset graph, string layerId)
        {
            return IsTileOutputLayer(graph, layerId)
                && TileSettingsNode.HasRenderableTiles(graph, layerId);
        }

        public static bool IsTileOutputLayer(GraphAsset graph, string layerId)
        {
            var outputNode = GetLayerOutputNode(graph, layerId);
            return outputNode != null && outputNode.OutputKind == LayerOutputKind.Tiles;
        }

        public static OutputNode GetLayerOutputNode(GraphAsset graph, string layerId)
        {
            if (graph == null || string.IsNullOrEmpty(layerId))
                return null;

            graph.EnsureLayerGraphStates();
            return graph.GetNodesForLayer(layerId)
                .OfType<OutputNode>()
                .FirstOrDefault();
        }
    }
}
