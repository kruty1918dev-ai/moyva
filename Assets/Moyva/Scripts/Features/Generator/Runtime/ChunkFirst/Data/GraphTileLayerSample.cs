using System;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal readonly struct GraphTileLayerSample
    {
        public GraphTileLayerSample(
            string graphLayerId,
            string graphLayerName,
            string blueprintLayerGuid,
            string buildLayerGuid,
            string tileId,
            string presetId,
            LayerKind layerKind,
            int sortingOrder,
            int graphLayerOrder,
            int terrainPriority,
            float height,
            float surfaceHeight,
            string sourceNodeId)
        {
            GraphLayerId = graphLayerId;
            GraphLayerName = graphLayerName;
            BlueprintLayerGuid = blueprintLayerGuid;
            BuildLayerGuid = buildLayerGuid;
            TileId = tileId;
            PresetId = presetId;
            LayerKind = layerKind;
            SortingOrder = sortingOrder;
            GraphLayerOrder = graphLayerOrder;
            TerrainPriority = terrainPriority;
            Height = height;
            SurfaceHeight = surfaceHeight;
            SourceNodeId = sourceNodeId;
        }

        public string GraphLayerId { get; }
        public string GraphLayerName { get; }
        public string BlueprintLayerGuid { get; }
        public string BuildLayerGuid { get; }
        public string TileId { get; }
        public string PresetId { get; }
        public LayerKind LayerKind { get; }
        public int SortingOrder { get; }
        public int GraphLayerOrder { get; }
        public int TerrainPriority { get; }
        public float Height { get; }
        public float SurfaceHeight { get; }
        public string SourceNodeId { get; }

        public string StableTieBreakKey
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(BuildLayerGuid))
                    return BuildLayerGuid;
                if (!string.IsNullOrWhiteSpace(BlueprintLayerGuid))
                    return BlueprintLayerGuid;
                return GraphLayerId ?? string.Empty;
            }
        }

        public bool IsTerrainLike =>
            LayerKind == LayerKind.BaseTerrain
            || LayerKind == LayerKind.OverlayTerrain
            || LayerKind == LayerKind.Road
            || LayerKind == LayerKind.Shore
            || LayerKind == LayerKind.Cliff;
        public int LayerKindRank => GetKindRank(LayerKind);

        public int CompareTo(GraphTileLayerSample other)
        {
            int result = LayerKindRank.CompareTo(other.LayerKindRank);
            if (result != 0)
                return result;

            result = TerrainPriority.CompareTo(other.TerrainPriority);
            if (result != 0)
                return result;

            result = SortingOrder.CompareTo(other.SortingOrder);
            if (result != 0)
                return result;

            result = GraphLayerOrder.CompareTo(other.GraphLayerOrder);
            if (result != 0)
                return result;

            return string.Compare(StableTieBreakKey, other.StableTieBreakKey, StringComparison.Ordinal);
        }

        private static int GetKindRank(LayerKind kind)
        {
            return kind switch
            {
                LayerKind.BaseTerrain => 800,
                LayerKind.Shore => 700,
                LayerKind.Road => 650,
                LayerKind.Cliff => 600,
                LayerKind.OverlayTerrain => 500,
                LayerKind.Building => 300,
                LayerKind.ObjectSpawn => 200,
                LayerKind.Decoration => 100,
                _ => 0
            };
        }
    }
}
