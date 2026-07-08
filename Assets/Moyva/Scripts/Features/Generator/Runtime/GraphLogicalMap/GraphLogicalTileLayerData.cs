using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal readonly struct GraphLogicalTileLayerData
    {
        public GraphLogicalTileLayerData(
            string graphLayerId,
            string layerName,
            string tileId,
            float layerHeight,
            float surfaceHeight,
            string blueprintLayerGuid = null,
            string buildLayerGuid = null,
            string presetId = null,
            LayerKind layerKind = LayerKind.BaseTerrain,
            int sortingOrder = 0,
            int graphLayerOrder = 0,
            int terrainPriority = 0,
            string sourceNodeId = null)
        {
            GraphLayerId = graphLayerId;
            LayerName = layerName;
            TileId = tileId;
            LayerHeight = layerHeight;
            SurfaceHeight = surfaceHeight;
            BlueprintLayerGuid = blueprintLayerGuid;
            BuildLayerGuid = buildLayerGuid;
            PresetId = presetId;
            LayerKind = layerKind;
            SortingOrder = sortingOrder;
            GraphLayerOrder = graphLayerOrder;
            TerrainPriority = terrainPriority;
            SourceNodeId = sourceNodeId;
        }

        public string GraphLayerId { get; }
        public string LayerName { get; }
        public string TileId { get; }
        public float LayerHeight { get; }
        public float SurfaceHeight { get; }
        public string BlueprintLayerGuid { get; }
        public string BuildLayerGuid { get; }
        public string PresetId { get; }
        public LayerKind LayerKind { get; }
        public int SortingOrder { get; }
        public int GraphLayerOrder { get; }
        public int TerrainPriority { get; }
        public string SourceNodeId { get; }

        public GraphTileLayerSample ToSample()
        {
            return new GraphTileLayerSample(
                GraphLayerId,
                LayerName,
                BlueprintLayerGuid,
                BuildLayerGuid,
                TileId,
                PresetId,
                LayerKind,
                SortingOrder,
                GraphLayerOrder,
                TerrainPriority,
                LayerHeight,
                SurfaceHeight,
                SourceNodeId);
        }
    }
}
