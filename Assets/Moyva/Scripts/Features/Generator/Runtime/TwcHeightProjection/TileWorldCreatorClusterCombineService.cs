using GiantGrey.TileWorldCreator.Components;
using GiantGrey.TileWorldCreator.Utilities;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorClusterCombineService : ITileWorldCreatorClusterCombineService
    {
        private readonly ITileWorldCreatorClusterStatsService _stats;

        public TileWorldCreatorClusterCombineService(ITileWorldCreatorClusterStatsService stats)
        {
            _stats = stats;
        }

        public TileWorldCreatorClusterCombineResult Combine(ClusterIdentifier cluster, bool deactivateSourceObjects)
        {
            var before = _stats.Collect(cluster.transform);
            var combiner = cluster.GetComponent<MeshCombiner>();
            if (combiner == null)
                combiner = cluster.gameObject.AddComponent<MeshCombiner>();

            ConfigureCombiner(combiner, deactivateSourceObjects);
            combiner.CombineMeshes(false);

            var after = _stats.Collect(cluster.transform);
            var combinedMesh = cluster.GetComponent<MeshFilter>()?.sharedMesh;
            if (combinedMesh != null)
                combinedMesh.name = $"Moyva TWC Combined Cluster {cluster.clusterID}";

            bool combined = after.RenderableMeshRenderers < before.RenderableMeshRenderers && combinedMesh != null;
            return new TileWorldCreatorClusterCombineResult(
                before,
                after,
                combinedMesh != null ? combinedMesh.vertexCount : 0,
                combinedMesh != null ? combinedMesh.subMeshCount : 0,
                combined);
        }

        private static void ConfigureCombiner(MeshCombiner combiner, bool deactivateSourceObjects)
        {
            combiner.CreateMultiMaterialMesh = true;
            combiner.CombineInactiveChildren = false;
            combiner.DestroyCombinedChildren = false;
            combiner.DeactivateCombinedChildren = deactivateSourceObjects;
            combiner.DeactivateCombinedChildrenMeshRenderers = !deactivateSourceObjects;
            combiner.GenerateUVMap = false;
        }
    }
}
