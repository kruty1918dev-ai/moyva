using Kruty1918.Moyva.Construction.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal interface IWallPrefabResolver
    {
        GameObject ResolvePrefab(WallCollectionDefinition collection, string buildingId, TopologyNeighborMask mask);
    }
}
