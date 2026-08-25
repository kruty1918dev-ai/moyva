using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public sealed class TileWorldCreatorRuntimeMeshOptimizerState
    {
        public Transform TargetRoot;
        public int ClustersPerFrame = 4;
        public bool DeactivateSourceObjects;
        public bool IsOptimizing;
        public bool HasOptimized;

        public void Reset(Transform targetRoot, int clustersPerFrame, bool deactivateSourceObjects)
        {
            TargetRoot = targetRoot;
            ClustersPerFrame = Mathf.Clamp(clustersPerFrame, 1, 64);
            DeactivateSourceObjects = deactivateSourceObjects;
            IsOptimizing = false;
            HasOptimized = false;
        }
    }
}
