using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class GraphLogicalTileMapBuilder
    {
        public static GraphLogicalTileMap Build(
            GraphAsset graph,
            TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiled,
            int width,
            int height)
        {
            return GraphLogicalTileMapServiceFactory.CreateBuilder()
                .Build(graph, manager, compiled, width, height);
        }
    }
}
