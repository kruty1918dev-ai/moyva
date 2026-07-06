using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITileWorldCreatorClusterStatsService
    {
        TileWorldCreatorClusterStats Collect(Transform root);
        int CountClusters(Transform root);
        int CountRendererComponents(Transform root);
        int CountRenderableMeshRenderers(Transform root);
        int CountMeshFiltersWithMesh(Transform root);
    }
}
