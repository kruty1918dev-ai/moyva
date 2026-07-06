using System.Collections;
using GiantGrey.TileWorldCreator.Components;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorRuntimeMeshOptimizerService : ITileWorldCreatorRuntimeMeshOptimizerService
    {
        private readonly ITileWorldCreatorClusterStatsService _stats;
        private readonly ITileWorldCreatorClusterCombineService _combiner;
        private readonly TileWorldCreatorRuntimeMeshOptimizerDiagnostics _diagnostics;

        public TileWorldCreatorRuntimeMeshOptimizerService(
            ITileWorldCreatorClusterStatsService stats,
            ITileWorldCreatorClusterCombineService combiner,
            TileWorldCreatorRuntimeMeshOptimizerDiagnostics diagnostics)
        {
            _stats = stats;
            _combiner = combiner;
            _diagnostics = diagnostics;
        }

        public void Configure(TileWorldCreatorRuntimeMeshOptimizerState state, MonoBehaviour owner, Transform targetRoot, int clustersPerFrame, bool deactivateSourceObjects)
        {
            state.Reset(targetRoot != null ? targetRoot : owner.transform, clustersPerFrame, deactivateSourceObjects);
            _diagnostics.LogConfigured(state, _stats);
        }

        public void ClearConfiguration(TileWorldCreatorRuntimeMeshOptimizerState state, string reason)
        {
            state.TargetRoot = null;
            state.IsOptimizing = false;
            state.HasOptimized = false;
            _diagnostics.LogCleared(reason);
        }

        public void RequestOptimizeAfterStable(TileWorldCreatorRuntimeMeshOptimizerState state, MonoBehaviour owner, string reason)
        {
            if (!CanStart(state, reason))
                return;

            _diagnostics.LogCoroutineStart();
            owner.StartCoroutine(OptimizeCoroutine(state, reason));
        }

        private bool CanStart(TileWorldCreatorRuntimeMeshOptimizerState state, string reason)
        {
            if (state.TargetRoot == null)
                return _diagnostics.SkipWithWarning("target root is null", reason);
            if (state.HasOptimized)
                return _diagnostics.Skip("already optimized this build", reason);
            if (state.IsOptimizing)
                return _diagnostics.Skip("optimizer is already running", reason);

            return true;
        }

        private IEnumerator OptimizeCoroutine(TileWorldCreatorRuntimeMeshOptimizerState state, string reason)
        {
            state.IsOptimizing = true;
            state.Samples.Clear();

            var root = state.TargetRoot;
            var clusters = root.GetComponentsInChildren<ClusterIdentifier>(false);
            var summary = new TileWorldCreatorRuntimeMeshOptimizationSummary(_stats, root);
            float start = Time.realtimeSinceStartup;
            _diagnostics.LogStarted(state, root, clusters.Length, summary, reason);

            int processedThisFrame = 0;
            for (int i = 0; i < clusters.Length; i++)
            {
                ProcessCluster(state, clusters[i], summary);
                processedThisFrame++;
                if (processedThisFrame >= state.ClustersPerFrame)
                {
                    processedThisFrame = 0;
                    _diagnostics.LogProgress(summary, clusters.Length, start);
                    yield return null;
                }
            }

            state.HasOptimized = true;
            state.IsOptimizing = false;
            summary.CaptureAfter(_stats, root);
            _diagnostics.LogComplete(state, root, summary, start);
            _diagnostics.LogCoroutineEnd(root);
        }

        private void ProcessCluster(TileWorldCreatorRuntimeMeshOptimizerState state, ClusterIdentifier cluster, TileWorldCreatorRuntimeMeshOptimizationSummary summary)
        {
            if (cluster == null)
                return;

            summary.ProcessedClusters++;
            var before = _stats.Collect(cluster.transform);
            if (before.MeshFiltersWithMesh == 0)
            {
                summary.SkippedNoMeshClusters++;
                return;
            }
            if (before.RenderableMeshRenderers <= 1)
            {
                summary.SkippedSmallClusters++;
                return;
            }

            TryCombineCluster(state, cluster, summary);
        }

        private void TryCombineCluster(TileWorldCreatorRuntimeMeshOptimizerState state, ClusterIdentifier cluster, TileWorldCreatorRuntimeMeshOptimizationSummary summary)
        {
            try
            {
                var result = _combiner.Combine(cluster, state.DeactivateSourceObjects);
                summary.RegisterResult(cluster, result, state);
            }
            catch (System.Exception ex)
            {
                summary.FailedClusters++;
                _diagnostics.LogClusterError(cluster, ex);
            }
        }
    }
}
