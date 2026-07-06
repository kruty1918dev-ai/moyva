using Kruty1918.Moyva.GraphSystem.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphLogicalTileMapSnapshotFactory
    {
        GraphLogicalTileMapSnapshot Create(string source, GraphAsset graph, int seed, GraphLogicalTileMap map);
    }
}
