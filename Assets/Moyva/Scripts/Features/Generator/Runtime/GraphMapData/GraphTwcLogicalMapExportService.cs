using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphTwcLogicalMapExportService
    {
        GraphLogicalTileMap Export(GraphAsset graph, TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiledLayers, int width, int height);
    }

    internal sealed class GraphTwcLogicalMapExportService : IGraphTwcLogicalMapExportService
    {
        private readonly IGraphLogicalTileMapBuilderService _builder;

        public GraphTwcLogicalMapExportService(IGraphLogicalTileMapBuilderService builder)
        {
            _builder = builder;
        }

        public GraphLogicalTileMap Export(GraphAsset graph, TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiledLayers, int width, int height)
        {
            return _builder.Build(graph, manager, compiledLayers, width, height);
        }
    }
}
