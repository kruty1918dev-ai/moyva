using GiantGrey.TileWorldCreator.Components;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorRuntimeMeshOptimizerDiagnostics
    {
        private const string LogTag = "[MoyvaTWCHeight:MeshOptimize]";
        private const string WorldGenDiagTag = "[MoyvaWorldGenDiag]";

        public void LogConfigured(TileWorldCreatorRuntimeMeshOptimizerState state, ITileWorldCreatorClusterStatsService stats)
        {
            var root = state.TargetRoot;
            Debug.Log($"{LogTag} Configure root='{root.name}', clustersPerFrame={state.ClustersPerFrame}, deactivateSourceObjects={state.DeactivateSourceObjects}, clusters={stats.CountClusters(root)}, rendererComponents={stats.CountRendererComponents(root)}, renderableMeshRenderers={stats.CountRenderableMeshRenderers(root)}, meshFiltersWithMesh={stats.CountMeshFiltersWithMesh(root)}.");
        }

        public void LogCleared(string reason)
        {
            Debug.Log($"{LogTag} Cleared optimizer configuration. reason='{reason}'.");
        }

        public bool SkipWithWarning(string detail, string reason)
        {
            Debug.LogWarning($"{LogTag} Optimization skipped: {detail}. reason='{reason}'.");
            return false;
        }

        public bool Skip(string detail, string reason)
        {
            Debug.Log($"{LogTag} Optimization skipped: {detail}. reason='{reason}'.");
            return false;
        }

        public void LogCoroutineStart()
        {
            Debug.Log($"{WorldGenDiagTag} TWCBuild.COROUTINE.START frame={Time.frameCount}");
        }

        public void LogStarted(TileWorldCreatorRuntimeMeshOptimizerState state, Transform root, int clusterCount, TileWorldCreatorRuntimeMeshOptimizationSummary summary, string reason)
        {
            Debug.Log($"{LogTag} Optimization started. reason='{reason}', root='{root.name}', clusters={clusterCount}, rendererComponentsBefore={summary.RendererComponentsBefore}, renderableMeshRenderersBefore={summary.RenderableRenderersBefore}, meshFiltersWithMeshBefore={summary.MeshFiltersBefore}, clustersPerFrame={state.ClustersPerFrame}, deactivateSourceObjects={state.DeactivateSourceObjects}.");
        }

        public void LogProgress(TileWorldCreatorRuntimeMeshOptimizationSummary summary, int clusterCount, float startTime)
        {
            Debug.Log($"{LogTag} Optimization progress: processed={summary.ProcessedClusters}/{clusterCount}, combined={summary.CombinedClusters}, hiddenSourceRenderers={summary.SourceRenderersHidden}, elapsed={Time.realtimeSinceStartup - startTime:0.###}s.");
        }

        public void LogComplete(TileWorldCreatorRuntimeMeshOptimizerState state, Transform root, TileWorldCreatorRuntimeMeshOptimizationSummary summary, float startTime)
        {
            Debug.Log($"{LogTag} Optimization complete. root='{root.name}', processedClusters={summary.ProcessedClusters}, combinedClusters={summary.CombinedClusters}, skippedSmallClusters={summary.SkippedSmallClusters}, skippedNoMeshClusters={summary.SkippedNoMeshClusters}, unchangedClusters={summary.UnchangedClusters}, failedClusters={summary.FailedClusters}, rendererComponents {summary.RendererComponentsBefore}->{summary.RendererComponentsAfter}, renderableMeshRenderers {summary.RenderableRenderersBefore}->{summary.RenderableRenderersAfter}, meshFiltersWithMesh {summary.MeshFiltersBefore}->{summary.MeshFiltersAfter}, hiddenSourceRenderers={summary.SourceRenderersHidden}, combinedVertices={summary.TotalVertices}, elapsed={Time.realtimeSinceStartup - startTime:0.###}s, samples={TileWorldCreatorHeightProjectionUtility.FormatSamples(state.Samples)}.");
        }

        public void LogCoroutineEnd(Transform root)
        {
            Debug.Log($"{WorldGenDiagTag} TWCBuild.COROUTINE.END frame={Time.frameCount}, childrenAfterCoroutine={root.childCount}");
        }

        public void LogClusterError(ClusterIdentifier cluster, System.Exception ex)
        {
            Debug.LogError($"{LogTag} Cluster optimization failed. cluster={cluster.clusterID}, name='{cluster.name}', error={ex}");
        }
    }
}
