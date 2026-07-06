using GiantGrey.TileWorldCreator;
using Kruty1918.Moyva.MapChunks.Runtime;
using UnityEngine;
using System.Text;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class TileWorldCreatorChunkBatchingUtility
    {
        private const string LogTag = "[MoyvaTWCChunks]";

        public static int ResolveSceneChunkSize()
        {
            var settings = UnityEngine.Object.FindFirstObjectByType<MapChunkSceneSettings>(FindObjectsInactive.Include);
            return settings != null ? settings.ChunkSize : 0;
        }

        public static void Apply(Configuration configuration, int chunkSizeTiles, bool forceMergeTiles, string source)
        {
            if (configuration == null || chunkSizeTiles <= 0)
            {
                if (configuration != null)
                {
                    Debug.LogWarning(
                        $"{LogTag} Chunk-aligned batching skipped: source={source}, requestedChunkSize={chunkSizeTiles}, " +
                        $"configClusterSize={configuration.clusterCellSize}, configMergeTiles={configuration.mergeTiles}, " +
                        $"activeTileLayers={CountActiveTileLayers(configuration)}.");
                }

                return;
            }

            int clusterSize = Mathf.Max(1, chunkSizeTiles);
            int activeTileLayers = 0;
            int meshOverrideCount = 0;
            int mergeOverrideCount = 0;

            if (configuration.buildLayerFolders != null)
            {
                for (int folderIndex = 0; folderIndex < configuration.buildLayerFolders.Count; folderIndex++)
                {
                    var folder = configuration.buildLayerFolders[folderIndex];
                    if (folder?.buildLayers == null)
                        continue;

                    for (int layerIndex = 0; layerIndex < folder.buildLayers.Count; layerIndex++)
                    {
                        if (folder.buildLayers[layerIndex] is not TilesBuildLayer buildLayer || !buildLayer.isEnabled)
                            continue;

                        activeTileLayers++;
                        if (!buildLayer.meshGenerationOverride)
                        {
                            buildLayer.meshGenerationOverride = true;
                            meshOverrideCount++;
                        }

                        if (buildLayer.mergeTiles != forceMergeTiles)
                        {
                            buildLayer.mergeTiles = forceMergeTiles;
                            mergeOverrideCount++;
                        }
                    }
                }
            }

            bool mergeChanged = configuration.mergeTiles != forceMergeTiles;
            if (mergeChanged)
                configuration.mergeTiles = forceMergeTiles;

            bool clusterChanged = configuration.clusterCellSize != clusterSize;
            if (clusterChanged)
                configuration.clusterCellSize = clusterSize;

            if (mergeChanged || clusterChanged || meshOverrideCount > 0 || mergeOverrideCount > 0)
            {
                Debug.Log(
                    $"{LogTag} Chunk-aligned batching applied: source={source}, chunk={clusterSize}x{clusterSize} tiles, " +
                    $"mergeTiles={configuration.mergeTiles}, activeTileLayers={activeTileLayers}, " +
                    $"meshOverridesEnabled={meshOverrideCount}, layerMergeOverridesEnabled={mergeOverrideCount}, " +
                    $"layers={DescribeActiveTileLayers(configuration)}.");
            }
        }

        public static string DescribeActiveTileLayers(Configuration configuration)
        {
            if (configuration?.buildLayerFolders == null)
                return "[]";

            var builder = new StringBuilder();
            builder.Append('[');
            int logged = 0;
            int total = 0;

            for (int folderIndex = 0; folderIndex < configuration.buildLayerFolders.Count; folderIndex++)
            {
                var folder = configuration.buildLayerFolders[folderIndex];
                if (folder?.buildLayers == null)
                    continue;

                for (int layerIndex = 0; layerIndex < folder.buildLayers.Count; layerIndex++)
                {
                    if (folder.buildLayers[layerIndex] is not TilesBuildLayer buildLayer || !buildLayer.isEnabled)
                        continue;

                    total++;
                    if (logged < 10)
                    {
                        if (logged > 0)
                            builder.Append(" | ");

                        builder.Append(buildLayer.layerName)
                            .Append(":merge=").Append(buildLayer.mergeTiles)
                            .Append(",meshOverride=").Append(buildLayer.meshGenerationOverride)
                            .Append(",dual=").Append(buildLayer.useDualGrid);
                        logged++;
                    }
                }
            }

            if (total > logged)
                builder.Append(" | ... total=").Append(total);

            builder.Append(']');
            return builder.ToString();
        }

        private static int CountActiveTileLayers(Configuration configuration)
        {
            if (configuration?.buildLayerFolders == null)
                return 0;

            int count = 0;
            for (int folderIndex = 0; folderIndex < configuration.buildLayerFolders.Count; folderIndex++)
            {
                var folder = configuration.buildLayerFolders[folderIndex];
                if (folder?.buildLayers == null)
                    continue;

                for (int layerIndex = 0; layerIndex < folder.buildLayers.Count; layerIndex++)
                {
                    if (folder.buildLayers[layerIndex] is TilesBuildLayer buildLayer && buildLayer.isEnabled)
                        count++;
                }
            }

            return count;
        }
    }
}
