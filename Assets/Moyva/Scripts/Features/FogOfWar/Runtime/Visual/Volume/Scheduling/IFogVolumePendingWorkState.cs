using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal interface IFogVolumePendingWorkState
    {
        bool HasPendingWork { get; }

        bool FullRebuildRequested { get; }

        IFogOfWarService FogService { get; }

        IReadOnlyCollection<Vector2Int> PendingDirtyTiles { get; }

        IReadOnlyList<FogCellVisualChange> PendingCellChanges { get; }

        int DirtyTileCount { get; }

        int CellChangeCount { get; }

        FogVolumePendingWorkSnapshot Snapshot { get; }
    }
}
