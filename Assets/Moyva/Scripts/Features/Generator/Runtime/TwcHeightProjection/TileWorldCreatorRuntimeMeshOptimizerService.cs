using System.Collections;
using GiantGrey.TileWorldCreator.Components;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorRuntimeMeshOptimizerService : ITileWorldCreatorRuntimeMeshOptimizerService
    {
        private readonly ITileWorldCreatorClusterStatsService _stats;
        private readonly ITileWorldCreatorClusterCombineService _combiner;

        public TileWorldCreatorRuntimeMeshOptimizerService(
            ITileWorldCreatorClusterStatsService stats,
            ITileWorldCreatorClusterCombineService combiner)
        {
            _stats = stats;
            _combiner = combiner;
        }

        public void Configure(TileWorldCreatorRuntimeMeshOptimizerState state, MonoBehaviour owner, Transform targetRoot, int clustersPerFrame, bool deactivateSourceObjects)
        {
            state.Reset(targetRoot != null ? targetRoot : owner.transform, clustersPerFrame, deactivateSourceObjects);
        }

        public void ClearConfiguration(TileWorldCreatorRuntimeMeshOptimizerState state, string reason)
        {
            state.TargetRoot = null;
            state.IsOptimizing = false;
            state.HasOptimized = false;
        }

        public void RequestOptimizeAfterStable(TileWorldCreatorRuntimeMeshOptimizerState state, MonoBehaviour owner, string reason)
        {
            if (!CanStart(state))
                return;

            owner.StartCoroutine(OptimizeCoroutine(state));
        }

        private static bool CanStart(TileWorldCreatorRuntimeMeshOptimizerState state)
        {
            if (state.TargetRoot == null)
                return false;
            if (state.HasOptimized)
                return false;
            if (state.IsOptimizing)
                return false;

            return true;
        }

        private IEnumerator OptimizeCoroutine(TileWorldCreatorRuntimeMeshOptimizerState state)
        {
            state.IsOptimizing = true;

            var root = state.TargetRoot;
            var clusters = root.GetComponentsInChildren<ClusterIdentifier>(false);

            int processedThisFrame = 0;
            for (int i = 0; i < clusters.Length; i++)
            {
                ProcessCluster(state, clusters[i]);
                processedThisFrame++;
                if (processedThisFrame >= state.ClustersPerFrame)
                {
                    processedThisFrame = 0;
                    yield return null;
                }
            }

            state.HasOptimized = true;
            state.IsOptimizing = false;
        }

        private void ProcessCluster(TileWorldCreatorRuntimeMeshOptimizerState state, ClusterIdentifier cluster)
        {
            if (cluster == null)
                return;

            var before = _stats.Collect(cluster.transform);
            if (before.MeshFiltersWithMesh == 0)
                return;
            if (before.RenderableMeshRenderers <= 1)
                return;

            TryCombineCluster(state, cluster);
        }

        private void TryCombineCluster(TileWorldCreatorRuntimeMeshOptimizerState state, ClusterIdentifier cluster)
        {
            try
            {
                _combiner.Combine(cluster, state.DeactivateSourceObjects);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex, cluster);
            }
        }
    }
}
