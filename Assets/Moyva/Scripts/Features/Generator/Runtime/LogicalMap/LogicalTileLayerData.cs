using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal readonly struct LogicalTileLayerData
    {
        public LogicalTileLayerData(
            string layerId,
            string layerName,
            string tileId,
            float layerHeight,
            float surfaceHeight,
            string blueprintLayerGuid = null,
            string buildLayerGuid = null,
            string presetId = null,
            LayerKind layerKind = LayerKind.BaseTerrain,
            int sortingOrder = 0,
            int layerOrder = 0,
            int terrainPriority = 0,
            string sourceLayerId = null,
            TileGeometryMode tileGeometryMode = TileGeometryMode.SolidTerrain,
            AuthoredClosurePolicy authoredClosurePolicy = AuthoredClosurePolicy.PreserveAuthored)
        {
            LayerId = layerId;
            LayerName = layerName;
            TileId = tileId;
            LayerHeight = layerHeight;
            SurfaceHeight = surfaceHeight;
            BlueprintLayerGuid = blueprintLayerGuid;
            BuildLayerGuid = buildLayerGuid;
            PresetId = presetId;
            LayerKind = layerKind;
            SortingOrder = sortingOrder;
            LayerOrder = layerOrder;
            TerrainPriority = terrainPriority;
            SourceLayerId = sourceLayerId;
            TileGeometryMode = tileGeometryMode;
            AuthoredClosurePolicy = authoredClosurePolicy;
        }

        public string LayerId { get; }
        public string LayerName { get; }
        public string TileId { get; }
        public float LayerHeight { get; }
        public float SurfaceHeight { get; }
        public string BlueprintLayerGuid { get; }
        public string BuildLayerGuid { get; }
        public string PresetId { get; }
        public LayerKind LayerKind { get; }
        public int SortingOrder { get; }
        public int LayerOrder { get; }
        public int TerrainPriority { get; }
        public string SourceLayerId { get; }
        public TileGeometryMode TileGeometryMode { get; }
        public AuthoredClosurePolicy AuthoredClosurePolicy { get; }

        /// <summary>Copies the data with a different surface height (per-cell override).</summary>
        public LogicalTileLayerData WithSurfaceHeight(float surfaceHeight)
            => WithHeights(LayerHeight, surfaceHeight);

        /// <summary>Copies the data with different layer (bed) and surface heights.</summary>
        public LogicalTileLayerData WithHeights(float layerHeight, float surfaceHeight)
        {
            return new LogicalTileLayerData(
                LayerId,
                LayerName,
                TileId,
                layerHeight,
                surfaceHeight,
                BlueprintLayerGuid,
                BuildLayerGuid,
                PresetId,
                LayerKind,
                SortingOrder,
                LayerOrder,
                TerrainPriority,
                SourceLayerId,
                TileGeometryMode,
                AuthoredClosurePolicy);
        }

        public TileLayerSample ToSample()
        {
            return new TileLayerSample(
                LayerId,
                LayerName,
                BlueprintLayerGuid,
                BuildLayerGuid,
                TileId,
                PresetId,
                LayerKind,
                SortingOrder,
                LayerOrder,
                TerrainPriority,
                LayerHeight,
                SurfaceHeight,
                SourceLayerId,
                TileGeometryMode,
                AuthoredClosurePolicy);
        }
    }
}
