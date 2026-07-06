using System.Collections.Generic;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphTwcLogicalMapExportService
    {
        GraphLogicalTileMap Export(GraphAsset graph, TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiledLayers, int width, int height, int seed);
    }

    internal sealed class GraphTwcLogicalMapExportService : IGraphTwcLogicalMapExportService
    {
        private readonly IGraphLogicalTileMapBuilderService _builder;
        private readonly IGraphLogicalTileMapDiagnosticsService _diagnostics;

        public GraphTwcLogicalMapExportService(
            IGraphLogicalTileMapBuilderService builder,
            IGraphLogicalTileMapDiagnosticsService diagnostics)
        {
            _builder = builder;
            _diagnostics = diagnostics;
        }

        public GraphLogicalTileMap Export(GraphAsset graph, TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiledLayers, int width, int height, int seed)
        {
            var logicalMap = _builder.Build(graph, manager, compiledLayers, width, height);
            _diagnostics.EmitAndCompare("Scene build mask", graph, seed, logicalMap);
            return logicalMap;
        }
    }
}
