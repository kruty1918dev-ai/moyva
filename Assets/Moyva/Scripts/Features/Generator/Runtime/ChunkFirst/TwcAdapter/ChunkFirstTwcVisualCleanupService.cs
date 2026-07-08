using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal sealed class ChunkFirstTwcVisualCleanupService : IChunkFirstTwcVisualCleanupService
    {
        private readonly ChunkFirstBuildDiagnostics _diagnostics;

        public ChunkFirstTwcVisualCleanupService(ChunkFirstBuildDiagnostics diagnostics)
        {
            _diagnostics = diagnostics;
        }

        public void ClearVisualBuildOutput(TileWorldCreatorManager manager)
        {
            if (manager == null)
                return;

            int layerObjects = DestroyLayerObjects(manager);
            int orphanClusters = DestroyOrphanClusters(manager);
            _diagnostics.LogTwcVisualCleanup(layerObjects, orphanClusters);
        }

        private static int DestroyLayerObjects(TileWorldCreatorManager manager)
        {
            var layers = manager.GetComponentsInChildren<LayerIdentifier>(true);
            int destroyed = 0;
            for (int i = 0; i < layers.Length; i++)
            {
                var layer = layers[i];
                if (layer == null || layer.transform == manager.transform)
                    continue;

                DestroyObject(layer.gameObject);
                destroyed++;
            }

            return destroyed;
        }

        private static int DestroyOrphanClusters(TileWorldCreatorManager manager)
        {
            var clusters = manager.GetComponentsInChildren<ClusterIdentifier>(true);
            int destroyed = 0;
            for (int i = 0; i < clusters.Length; i++)
            {
                var cluster = clusters[i];
                if (cluster == null || cluster.GetComponentInParent<LayerIdentifier>(true) != null)
                    continue;

                DestroyObject(cluster.gameObject);
                destroyed++;
            }

            return destroyed;
        }

        private static void DestroyObject(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(gameObject);
            else
                Object.DestroyImmediate(gameObject);
        }
    }
}
