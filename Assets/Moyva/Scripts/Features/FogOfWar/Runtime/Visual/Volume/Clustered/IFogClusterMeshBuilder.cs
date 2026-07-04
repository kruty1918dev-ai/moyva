using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal interface IFogClusterMeshBuilder
    {
        void RebuildCluster(
            FogClusterKey key,
            Mesh mesh,
            FogWorldVisualContext context,
            IFogStateReader fogService);
    }
}
