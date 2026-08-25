using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    internal sealed class ChunkFirstTwcVisualCleanupService : IChunkFirstTwcVisualCleanupService
    {
        public void ClearVisualBuildOutput(TileWorldCreatorManager manager)
        {
            if (manager == null)
                return;

            DestroyLayerObjects(manager);
            DestroyOrphanClusters(manager);
        }

        private static void DestroyLayerObjects(TileWorldCreatorManager manager)
        {
            var layers = manager.GetComponentsInChildren<LayerIdentifier>(true);
            for (int i = 0; i < layers.Length; i++)
            {
                var layer = layers[i];
                if (layer == null || layer.transform == manager.transform)
                    continue;

                DestroyObject(layer.gameObject);
            }
        }

        private static void DestroyOrphanClusters(TileWorldCreatorManager manager)
        {
            var clusters = manager.GetComponentsInChildren<ClusterIdentifier>(true);
            for (int i = 0; i < clusters.Length; i++)
            {
                var cluster = clusters[i];
                if (cluster == null || cluster.GetComponentInParent<LayerIdentifier>(true) != null)
                    continue;

                DestroyObject(cluster.gameObject);
            }
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
