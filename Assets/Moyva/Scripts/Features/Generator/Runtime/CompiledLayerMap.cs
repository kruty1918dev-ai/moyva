namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Links one recipe layer to the TileWorldCreator blueprint layer it produced.
    /// </summary>
    public sealed class CompiledLayerMap
    {
        public string LayerId;
        public string GridTileId;
        public string BlueprintLayerGuid;
        public string LayerName;
        public int SortingOrder;
        public int LayerOrder;
        public int TerrainPriority;
        public string BuildLayerGuid;
        public string PresetId;
        public string SourceLayerId;
        public bool HasRenderableTileOutput;

        /// <summary>
        /// Optional per-cell surface height override (meters, NaN where absent)
        /// registered by mask steps such as <see cref="API.HydrologyMaskStep"/>.
        /// </summary>
        public float[,] SurfaceHeightOverride;

        /// <summary>
        /// Optional per-cell bed height override (meters, NaN where absent)
        /// paired with <see cref="SurfaceHeightOverride"/> for water layers:
        /// channel/lake floor below the sheet, used for depth queries and
        /// downstream terrain shaping.
        /// </summary>
        public float[,] BedHeightOverride;
    }
}
