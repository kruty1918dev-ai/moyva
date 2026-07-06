using GiantGrey.TileWorldCreator.Components;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorClusterStatsService : ITileWorldCreatorClusterStatsService
    {
        public TileWorldCreatorClusterStats Collect(Transform root)
        {
            if (root == null)
                return default;

            int rendererComponents = 0;
            int renderableMeshRenderers = 0;
            int meshFiltersWithMesh = 0;
            var meshRenderers = root.GetComponentsInChildren<MeshRenderer>(false);
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                var renderer = meshRenderers[i];
                if (renderer == null)
                    continue;

                rendererComponents++;
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (renderer.enabled && meshFilter != null && meshFilter.sharedMesh != null)
                    renderableMeshRenderers++;
            }

            var meshFilters = root.GetComponentsInChildren<MeshFilter>(false);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i] != null && meshFilters[i].sharedMesh != null)
                    meshFiltersWithMesh++;
            }

            return new TileWorldCreatorClusterStats(rendererComponents, renderableMeshRenderers, meshFiltersWithMesh);
        }

        public int CountClusters(Transform root)
            => root != null ? root.GetComponentsInChildren<ClusterIdentifier>(false).Length : 0;

        public int CountRendererComponents(Transform root)
            => root != null ? root.GetComponentsInChildren<MeshRenderer>(true).Length : 0;

        public int CountRenderableMeshRenderers(Transform root)
            => CountMatchingMeshRenderers(root, requireEnabled: true);

        public int CountMeshFiltersWithMesh(Transform root)
        {
            if (root == null)
                return 0;

            int count = 0;
            var meshFilters = root.GetComponentsInChildren<MeshFilter>(false);
            for (int i = 0; i < meshFilters.Length; i++)
            {
                var meshFilter = meshFilters[i];
                if (meshFilter != null && meshFilter.sharedMesh != null && meshFilter.GetComponentInParent<TileWorldCreatorTerrainSideWallBuilder>() == null)
                    count++;
            }

            return count;
        }

        private static int CountMatchingMeshRenderers(Transform root, bool requireEnabled)
        {
            if (root == null)
                return 0;

            int count = 0;
            var renderers = root.GetComponentsInChildren<MeshRenderer>(false);
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || (requireEnabled && !renderer.enabled))
                    continue;
                if (renderer.GetComponentInParent<TileWorldCreatorTerrainSideWallBuilder>() != null)
                    continue;

                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                    count++;
            }

            return count;
        }
    }
}
