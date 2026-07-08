namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Links one gameplay graph layer to the TileWorldCreator blueprint layer it produced.
    /// </summary>
    public sealed class CompiledLayerMap
    {
        public string GraphLayerId;
        public string GridTileId;
        public string BlueprintLayerGuid;
        public string LayerName;
        public int SortingOrder;
        public int GraphLayerOrder;
        public int TerrainPriority;
        public string BuildLayerGuid;
        public string PresetId;
        public string SourceNodeId;
        public bool HasRenderableTileOutput;
    }
}
