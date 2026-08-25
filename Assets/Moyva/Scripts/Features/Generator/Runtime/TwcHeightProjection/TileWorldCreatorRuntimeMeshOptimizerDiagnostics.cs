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
        }

        public void LogCleared(string reason)
        {
        }

        public bool SkipWithWarning(string detail, string reason)
        {
            return false;
        }

        public bool Skip(string detail, string reason)
        {
            return false;
        }

        public void LogCoroutineStart()
        {
        }

        public void LogStarted(TileWorldCreatorRuntimeMeshOptimizerState state, Transform root, int clusterCount, TileWorldCreatorRuntimeMeshOptimizationSummary summary, string reason)
        {
        }

        public void LogProgress(TileWorldCreatorRuntimeMeshOptimizationSummary summary, int clusterCount, float startTime)
        {
        }

        public void LogComplete(TileWorldCreatorRuntimeMeshOptimizerState state, Transform root, TileWorldCreatorRuntimeMeshOptimizationSummary summary, float startTime)
        {
        }

        public void LogCoroutineEnd(Transform root)
        {
        }

        public void LogClusterError(ClusterIdentifier cluster, System.Exception ex)
        {
            Debug.LogError($"{LogTag} Cluster optimization failed. cluster={cluster.clusterID}, name='{cluster.name}', error={ex}");
        }
    }
}
