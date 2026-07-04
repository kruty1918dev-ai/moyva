using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal interface IFogVolumePendingWorkRequests
    {
        void RequestFullRebuild();

        void RequestFullRebuild(IFogOfWarService fogService);

        void RequestFullRebuildWhenFogServiceAvailable();

        int RequestDirtyTiles(IFogOfWarService fogService, IEnumerable<Vector2Int> dirtyTiles, out int requested);

        int RequestCellsUpdate(IFogOfWarService fogService, IReadOnlyList<FogCellVisualChange> changes);
    }
}
