using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public sealed class TileWorldCreatorRuntimeMeshOptimizerState
    {
        public readonly List<string> Samples = new List<string>(16);
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
            Samples.Clear();
        }

        public void AddSample(string sample)
        {
            if (Samples.Count < 16)
                Samples.Add(sample);
        }
    }
}
