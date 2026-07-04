using System.Collections.Generic;

namespace Kruty1918.Moyva.FogOfWar.API
{
    /// <summary>
    /// Діагностичний опис одного clustered fog update batch-а.
    /// </summary>
    public readonly struct FogClusterUpdateBatch
    {
        public FogClusterUpdateBatch(
            IReadOnlyList<FogCellVisualChange> changes,
            IReadOnlyList<FogClusterKey> dirtyClusters,
            bool requiresFullRebuild,
            string fullRebuildReason)
        {
            Changes = changes;
            DirtyClusters = dirtyClusters;
            RequiresFullRebuild = requiresFullRebuild;
            FullRebuildReason = fullRebuildReason;
        }

        public IReadOnlyList<FogCellVisualChange> Changes { get; }
        public IReadOnlyList<FogClusterKey> DirtyClusters { get; }
        public bool RequiresFullRebuild { get; }
        public string FullRebuildReason { get; }
    }
}
