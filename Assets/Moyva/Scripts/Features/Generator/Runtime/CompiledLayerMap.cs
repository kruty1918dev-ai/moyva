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
    }
}
