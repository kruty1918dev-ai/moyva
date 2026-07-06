namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class GraphLogicalTileMapCellWriter : IGraphLogicalTileMapCellWriter
    {
        public void Fill(GraphLogicalTileMap map, GraphLogicalTileLayerData layer)
        {
            for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
                Set(map, x, y, layer);
        }

        public void Set(GraphLogicalTileMap map, int x, int y, GraphLogicalTileLayerData layer)
        {
            map.TileIds[x, y] = layer.TileId;
            map.GraphLayerIds[x, y] = layer.GraphLayerId;
            map.LayerNames[x, y] = layer.LayerName;
            map.LayerHeights[x, y] = layer.LayerHeight;
            map.SurfaceHeights[x, y] = layer.SurfaceHeight;
        }
    }
}
