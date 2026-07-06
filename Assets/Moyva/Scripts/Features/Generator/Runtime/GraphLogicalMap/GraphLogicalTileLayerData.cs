namespace Kruty1918.Moyva.Generator.Runtime
{
    internal readonly struct GraphLogicalTileLayerData
    {
        public GraphLogicalTileLayerData(
            string graphLayerId,
            string layerName,
            string tileId,
            float layerHeight,
            float surfaceHeight)
        {
            GraphLayerId = graphLayerId;
            LayerName = layerName;
            TileId = tileId;
            LayerHeight = layerHeight;
            SurfaceHeight = surfaceHeight;
        }

        public string GraphLayerId { get; }
        public string LayerName { get; }
        public string TileId { get; }
        public float LayerHeight { get; }
        public float SurfaceHeight { get; }
    }
}
