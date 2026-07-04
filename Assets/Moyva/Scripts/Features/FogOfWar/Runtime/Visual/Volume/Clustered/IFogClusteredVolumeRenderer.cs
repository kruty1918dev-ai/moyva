using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal interface IFogClusteredVolumeRenderer
    {
        void RebuildFull(FogWorldVisualContext context, IFogStateReader fogService);
        void RebuildClusters(IReadOnlyList<FogClusterKey> dirtyClusters, FogWorldVisualContext context, IFogStateReader fogService);
        void Clear();
        void ConfigureRoot(Transform parent);
    }
}
