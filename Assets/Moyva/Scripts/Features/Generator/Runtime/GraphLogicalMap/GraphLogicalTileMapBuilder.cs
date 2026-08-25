using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class GraphLogicalTileMapBuilder
    {
        private static readonly IGraphLogicalTileMapBuilderService Builder =
            new GraphLogicalTileMapBuilderService(
                new GraphLogicalTileMapTwcLookup(),
                new GraphLogicalTileMapCellWriter());

        public static GraphLogicalTileMap Build(
            GraphAsset graph,
            TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiled,
            int width,
            int height)
        {
            return Builder.Build(graph, manager, compiled, width, height);
        }
    }
}
