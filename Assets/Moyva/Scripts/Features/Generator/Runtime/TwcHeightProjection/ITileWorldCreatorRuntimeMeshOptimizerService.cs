using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public interface ITileWorldCreatorRuntimeMeshOptimizerService
    {
        void Configure(TileWorldCreatorRuntimeMeshOptimizerState state, MonoBehaviour owner, Transform targetRoot, int clustersPerFrame, bool deactivateSourceObjects);
        void ClearConfiguration(TileWorldCreatorRuntimeMeshOptimizerState state, string reason);
        void RequestOptimizeAfterStable(TileWorldCreatorRuntimeMeshOptimizerState state, MonoBehaviour owner, string reason);
    }
}
