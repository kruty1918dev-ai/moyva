using System.Collections.Generic;
using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal interface IFogClusterMeshRegistry
    {
        FogClusterMeshHandle GetOrCreate(FogClusterKey key);
        bool TryGet(FogClusterKey key, out FogClusterMeshHandle handle);
        void Remove(FogClusterKey key);
        void ClearAll();
        IEnumerable<FogClusterMeshHandle> GetAll();
        void SetRootParent(Transform parent);
    }
}
