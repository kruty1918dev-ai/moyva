using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal interface IFogDirtyClusterTracker
    {
        void MarkChanges(IReadOnlyList<FogCellVisualChange> changes, FogWorldVisualContext context);
        IReadOnlyList<FogClusterKey> ConsumeDirtyClusters();
        void Clear();
    }
}
