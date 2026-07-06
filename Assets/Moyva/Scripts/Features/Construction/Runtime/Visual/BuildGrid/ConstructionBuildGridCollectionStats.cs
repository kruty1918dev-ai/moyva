namespace Kruty1918.Moyva.Construction.Runtime
{
    internal readonly struct ConstructionBuildGridCollectionStats
    {
        public ConstructionBuildGridCollectionStats(
            int positionsScanned,
            int positionsWithTileData,
            int missingSurfaceData,
            int filteredOut,
            int skippedEntries,
            int entriesCreated)
        {
            PositionsScanned = positionsScanned;
            PositionsWithTileData = positionsWithTileData;
            MissingSurfaceData = missingSurfaceData;
            FilteredOut = filteredOut;
            SkippedEntries = skippedEntries;
            EntriesCreated = entriesCreated;
        }

        public int PositionsScanned { get; }
        public int PositionsWithTileData { get; }
        public int MissingSurfaceData { get; }
        public int FilteredOut { get; }
        public int SkippedEntries { get; }
        public int EntriesCreated { get; }
    }
}
