namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface IGraphLogicalTileMapCellWriter
    {
        void Fill(GraphLogicalTileMap map, GraphLogicalTileLayerData layer);
        void Set(GraphLogicalTileMap map, int x, int y, GraphLogicalTileLayerData layer);
    }
}
