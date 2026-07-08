using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using Kruty1918.Moyva.Generator.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    internal static class TileWorldCreatorChunkGizmoBoundsCollector
    {
        private const string MapVisualChunksRootName = "MapVisualChunks";
        private const string MapChunkPrefix = "MapChunk_";
        private static readonly List<Renderer> Renderers = new(128);

        public static void CollectManagerChunks(TileWorldCreatorManager manager, List<Bounds> results)
        {
            results.Clear();
            if (manager == null)
                return;

            if (TryCollectMapVisualChunks(results))
                return;

            if (TileWorldCreatorLogicalChunkBoundsUtility.TryCollectAll(manager, results))
                return;

            ClusterIdentifier[] clusters = manager.GetComponentsInChildren<ClusterIdentifier>(true);
            for (int i = 0; i < clusters.Length; i++)
            {
                if (TryResolveVisualBounds(clusters[i]?.transform, out Bounds bounds))
                    results.Add(bounds);
            }

            if (results.Count == 0)
                TileWorldCreatorFallbackChunkBoundsCollector.CollectManagerChunks(manager, results);
        }

        public static bool TryCollectSelectedChunk(Transform selected, out Bounds bounds)
        {
            bounds = default;
            if (selected == null)
                return false;

            if (TryFindMapVisualChunkRoot(selected, out var chunkRoot))
                return TryResolveVisualBounds(chunkRoot, out bounds);

            TileWorldCreatorManager manager = selected.GetComponentInParent<TileWorldCreatorManager>();
            if (manager == null)
                manager = Object.FindFirstObjectByType<TileWorldCreatorManager>(FindObjectsInactive.Include);

            ClusterIdentifier cluster = selected.GetComponentInParent<ClusterIdentifier>();
            if (cluster != null)
            {
                if (manager != null && TileWorldCreatorLogicalChunkBoundsUtility.TryCollectAt(manager, cluster.transform.position, out bounds))
                    return true;

                return TryResolveVisualBounds(cluster.transform, out bounds);
            }

            Renderer renderer = selected.GetComponentInChildren<Renderer>();
            if (manager == null || renderer == null)
                return false;

            if (TileWorldCreatorLogicalChunkBoundsUtility.TryCollectAt(manager, renderer.bounds.center, out bounds))
                return true;

            return TileWorldCreatorFallbackChunkBoundsCollector.TryCollectChunk(manager, renderer.bounds.center, out bounds);
        }

        public static bool TryCollectSelectedActualBounds(Transform selected, out Bounds bounds)
        {
            bounds = default;
            if (selected == null)
                return false;

            if (TryFindMapVisualChunkRoot(selected, out var chunkRoot))
                return TryResolveVisualBounds(chunkRoot, out bounds);

            ClusterIdentifier cluster = selected.GetComponentInParent<ClusterIdentifier>();
            if (cluster != null)
                return TryResolveVisualBounds(cluster.transform, out bounds);

            return TryResolveVisualBounds(selected, out bounds);
        }

        private static bool TryCollectMapVisualChunks(List<Bounds> results)
        {
            var rootObject = GameObject.Find(MapVisualChunksRootName);
            Transform root = rootObject != null ? rootObject.transform : null;
            if (root == null)
                return false;

            int before = results.Count;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child == null || !child.name.StartsWith(MapChunkPrefix, System.StringComparison.Ordinal))
                    continue;

                if (TryResolveVisualBounds(child, out var bounds))
                    results.Add(bounds);
            }

            return results.Count > before;
        }

        private static bool TryFindMapVisualChunkRoot(Transform selected, out Transform chunkRoot)
        {
            chunkRoot = null;
            for (Transform current = selected; current != null; current = current.parent)
            {
                if (!current.name.StartsWith(MapChunkPrefix, System.StringComparison.Ordinal))
                    continue;

                if (current.parent == null
                    || !string.Equals(current.parent.name, MapVisualChunksRootName, System.StringComparison.Ordinal))
                    continue;

                chunkRoot = current;
                return true;
            }

            return false;
        }

        private static bool TryResolveVisualBounds(Transform root, out Bounds bounds)
        {
            bounds = default;
            if (!TryCollectRenderers(root, out bounds))
                return false;

            ClearScratch();
            return bounds.size.x > 0.001f && bounds.size.z > 0.001f;
        }

        private static bool TryCollectRenderers(Transform root, out Bounds bounds)
        {
            bounds = default;
            if (root == null)
                return false;

            Renderers.Clear();
            root.GetComponentsInChildren(true, Renderers);
            bool hasBounds = false;
            for (int i = 0; i < Renderers.Count; i++)
            {
                Renderer renderer = Renderers[i];
                if (!ShouldUse(renderer))
                    continue;

                if (hasBounds)
                    bounds.Encapsulate(renderer.bounds);
                else
                    bounds = renderer.bounds;

                hasBounds = true;
            }

            return hasBounds;
        }

        internal static bool ShouldUse(Renderer renderer)
        {
            return renderer != null
                && renderer.enabled
                && renderer.GetComponentInParent<Canvas>() == null
                && renderer.GetComponentInParent<TileWorldCreatorTerrainSideWallBuilder>() == null;
        }

        private static void ClearScratch()
        {
            Renderers.Clear();
        }
    }
}
