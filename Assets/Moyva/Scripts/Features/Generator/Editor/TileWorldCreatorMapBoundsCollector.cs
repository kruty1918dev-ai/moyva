using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using GiantGrey.TileWorldCreator.Components;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    internal static class TileWorldCreatorMapBoundsCollector
    {
        private static readonly List<Renderer> Renderers = new(256);

        public static Bounds Collect(TileWorldCreatorManager manager)
        {
            Bounds bounds = default;
            if (manager == null)
                return bounds;

            if (TryCollectFromManager(manager, ref bounds))
                return bounds;

            ClusterIdentifier[] clusters = Object.FindObjectsByType<ClusterIdentifier>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            bool hasBounds = false;
            for (int i = 0; i < clusters.Length; i++)
            {
                ClusterIdentifier cluster = clusters[i];
                if (cluster == null)
                    continue;

                Renderers.Clear();
                cluster.GetComponentsInChildren(true, Renderers);
                if (TryAccumulate(ref bounds))
                    hasBounds = true;
            }

            Renderers.Clear();
            return hasBounds ? bounds : default;
        }

        private static bool TryCollectFromManager(TileWorldCreatorManager manager, ref Bounds bounds)
        {
            Renderers.Clear();
            manager.GetComponentsInChildren(true, Renderers);
            bool hasBounds = TryAccumulate(ref bounds);
            Renderers.Clear();
            return hasBounds;
        }

        private static bool TryAccumulate(ref Bounds bounds)
        {
            bool hasBounds = false;
            for (int i = 0; i < Renderers.Count; i++)
            {
                Renderer renderer = Renderers[i];
                if (!TileWorldCreatorChunkGizmoBoundsCollector.ShouldUse(renderer))
                    continue;

                if (hasBounds)
                    bounds.Encapsulate(renderer.bounds);
                else
                    bounds = renderer.bounds;

                hasBounds = true;
            }

            return hasBounds;
        }
    }
}
