using System.Collections.Generic;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphLogicalTileMapBuilderService
    {
        GraphLogicalTileMap Build(
            GraphAsset graph,
            TileWorldCreatorManager manager,
            IReadOnlyList<CompiledLayerMap> compiled,
            int width,
            int height);
    }
}
