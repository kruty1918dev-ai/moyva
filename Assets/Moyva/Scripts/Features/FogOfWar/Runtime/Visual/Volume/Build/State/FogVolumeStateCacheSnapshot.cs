namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal readonly struct FogVolumeStateCacheSnapshot
    {
        public FogVolumeStateCacheSnapshot(int unexploredCellCount, int exploredCellCount, string runtimeLayerSignature)
        {
            UnexploredCellCount = unexploredCellCount;
            ExploredCellCount = exploredCellCount;
            RuntimeLayerSignature = runtimeLayerSignature;
        }

        public int UnexploredCellCount { get; }
        public int ExploredCellCount { get; }
        public string RuntimeLayerSignature { get; }
    }
}
