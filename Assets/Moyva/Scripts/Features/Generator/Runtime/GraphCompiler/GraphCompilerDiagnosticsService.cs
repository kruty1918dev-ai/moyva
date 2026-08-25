using System.Collections.Generic;
using System.Linq;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.Nodes;
using Kruty1918.Moyva.GraphSystem.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphCompilerDiagnosticsService
    {
        void LogEnter(GraphAsset graph, Vector2Int mapSize, int seed);
        void LogLayerOrder(GraphAsset graph, IReadOnlyList<GeneratorLayerDefinition> orderedLayers);
        void LogSkipped(GraphAsset graph, ISet<string> skippedLayerIds);
        void LogResult(GraphAsset graph, Configuration config, int objectLayerCount, int warnings);
    }

    internal sealed class GraphCompilerDiagnosticsService : IGraphCompilerDiagnosticsService
    {
        private const string Tag = "[MoyvaWorldGenDiag]";

        public void LogEnter(GraphAsset graph, Vector2Int mapSize, int seed)
        {
        }

        public void LogLayerOrder(GraphAsset graph, IReadOnlyList<GeneratorLayerDefinition> orderedLayers)
        {
            int outputCount = CountOutputLayers(graph, orderedLayers);
        }

        public void LogSkipped(GraphAsset graph, ISet<string> skippedLayerIds)
        {
            if (skippedLayerIds == null || skippedLayerIds.Count == 0)
                return;

            var names = skippedLayerIds
                .Select(id => graph?.GetLayerById(id)?.Name ?? id);
        }

        public void LogResult(GraphAsset graph, Configuration config, int objectLayerCount, int warnings)
        {
            int buildLayerCount = GraphCompilerLayerAssetUtility.CountBuildLayers(config);
            int tileSettingsCount = graph.Nodes?.OfType<TileSettingsNode>().Count() ?? 0;
        }

        private static int CountOutputLayers(GraphAsset graph, IReadOnlyList<GeneratorLayerDefinition> orderedLayers)
        {
            if (graph == null || orderedLayers == null)
                return 0;

            int count = 0;
            foreach (var layer in orderedLayers)
            {
                if (layer != null && GraphLayerRuntimeSemantics.GetLayerOutputNode(graph, layer.Id) != null)
                    count++;
            }

            return count;
        }
    }
}
