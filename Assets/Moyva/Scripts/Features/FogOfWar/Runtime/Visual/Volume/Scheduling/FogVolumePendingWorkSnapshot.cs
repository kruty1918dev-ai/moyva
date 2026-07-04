using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal readonly struct FogVolumePendingWorkSnapshot
    {
        public FogVolumePendingWorkSnapshot(
            bool hasPendingWork,
            bool fullRebuildRequested,
            IFogOfWarService fogService,
            IReadOnlyCollection<Vector2Int> pendingDirtyTiles,
            IReadOnlyList<FogCellVisualChange> pendingCellChanges)
        {
            HasPendingWork = hasPendingWork;
            FullRebuildRequested = fullRebuildRequested;
            FogService = fogService;
            PendingDirtyTiles = pendingDirtyTiles;
            PendingCellChanges = pendingCellChanges;
        }

        public bool HasPendingWork { get; }

        public bool FullRebuildRequested { get; }

        public IFogOfWarService FogService { get; }

        public IReadOnlyCollection<Vector2Int> PendingDirtyTiles { get; }

        public IReadOnlyList<FogCellVisualChange> PendingCellChanges { get; }

        public int DirtyTileCount => PendingDirtyTiles?.Count ?? 0;

        public int CellChangeCount => PendingCellChanges?.Count ?? 0;
    }
}
