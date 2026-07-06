using GiantGrey.TileWorldCreator.Components;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorRuntimeMeshOptimizationSummary
    {
        public TileWorldCreatorRuntimeMeshOptimizationSummary(ITileWorldCreatorClusterStatsService stats, Transform root)
        {
            RendererComponentsBefore = stats.CountRendererComponents(root);
            RenderableRenderersBefore = stats.CountRenderableMeshRenderers(root);
            MeshFiltersBefore = stats.CountMeshFiltersWithMesh(root);
        }

        public int RendererComponentsBefore { get; }
        public int RenderableRenderersBefore { get; }
        public int MeshFiltersBefore { get; }
        public int RendererComponentsAfter { get; private set; }
        public int RenderableRenderersAfter { get; private set; }
        public int MeshFiltersAfter { get; private set; }
        public int ProcessedClusters;
        public int CombinedClusters;
        public int SkippedSmallClusters;
        public int SkippedNoMeshClusters;
        public int UnchangedClusters;
        public int FailedClusters;
        public int SourceRenderersHidden;
        public int TotalVertices;

        public void RegisterResult(
            ClusterIdentifier cluster,
            TileWorldCreatorClusterCombineResult result,
            TileWorldCreatorRuntimeMeshOptimizerState state)
        {
            if (result.Combined)
            {
                CombinedClusters++;
                SourceRenderersHidden += result.Before.RenderableMeshRenderers - result.After.RenderableMeshRenderers;
                TotalVertices += result.Vertices;
                state.AddSample($"cluster={cluster.clusterID} renderable {result.Before.RenderableMeshRenderers}->{result.After.RenderableMeshRenderers}, meshFilters={result.Before.MeshFiltersWithMesh}, vertices={result.Vertices}, subMeshes={result.SubMeshes}");
                return;
            }

            UnchangedClusters++;
            state.AddSample($"UNCHANGED cluster={cluster.clusterID} renderable {result.Before.RenderableMeshRenderers}->{result.After.RenderableMeshRenderers}, meshFilters={result.Before.MeshFiltersWithMesh}, hasCombinedMesh={result.Vertices > 0}");
        }

        public void CaptureAfter(ITileWorldCreatorClusterStatsService stats, Transform root)
        {
            RendererComponentsAfter = stats.CountRendererComponents(root);
            RenderableRenderersAfter = stats.CountRenderableMeshRenderers(root);
            MeshFiltersAfter = stats.CountMeshFiltersWithMesh(root);
        }
    }
}
