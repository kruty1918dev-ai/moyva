namespace Kruty1918.Moyva.Generator.Runtime
{
    internal readonly struct TileWorldCreatorClusterCombineResult
    {
        public TileWorldCreatorClusterCombineResult(TileWorldCreatorClusterStats before, TileWorldCreatorClusterStats after, int vertices, int subMeshes, bool combined)
        {
            Before = before;
            After = after;
            Vertices = vertices;
            SubMeshes = subMeshes;
            Combined = combined;
        }

        public TileWorldCreatorClusterStats Before { get; }
        public TileWorldCreatorClusterStats After { get; }
        public int Vertices { get; }
        public int SubMeshes { get; }
        public bool Combined { get; }
    }
}
