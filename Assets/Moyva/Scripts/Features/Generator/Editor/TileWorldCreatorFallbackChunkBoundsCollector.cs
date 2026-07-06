using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Editor
{
    internal static class TileWorldCreatorFallbackChunkBoundsCollector
    {
        private static readonly List<Renderer> Renderers = new(256);
        private static readonly Dictionary<Vector2Int, Bounds> BoundsByChunk = new();

        public static void CollectManagerChunks(TileWorldCreatorManager manager, List<Bounds> results)
        {
            BoundsByChunk.Clear();
            if (!TryCollectRenderers(manager, out Bounds allBounds))
                return;

            float chunkWorldSize = ResolveChunkWorldSize(manager);
            Vector3 origin = allBounds.min;
            for (int i = 0; i < Renderers.Count; i++)
            {
                Renderer renderer = Renderers[i];
                Vector2Int coord = ToChunkCoord(renderer.bounds.center, origin, chunkWorldSize);
                if (BoundsByChunk.TryGetValue(coord, out Bounds bounds))
                    bounds.Encapsulate(renderer.bounds);
                else
                    bounds = renderer.bounds;

                BoundsByChunk[coord] = bounds;
            }

            foreach (Bounds bounds in BoundsByChunk.Values)
                results.Add(bounds);

            ClearScratch();
        }

        public static bool TryCollectChunk(TileWorldCreatorManager manager, Vector3 selectedCenter, out Bounds bounds)
        {
            bounds = default;
            if (!TryCollectRenderers(manager, out Bounds allBounds))
                return false;

            float chunkWorldSize = ResolveChunkWorldSize(manager);
            Vector3 origin = allBounds.min;
            Vector2Int selectedCoord = ToChunkCoord(selectedCenter, origin, chunkWorldSize);
            bool found = false;

            for (int i = 0; i < Renderers.Count; i++)
            {
                Renderer renderer = Renderers[i];
                if (ToChunkCoord(renderer.bounds.center, origin, chunkWorldSize) != selectedCoord)
                    continue;

                if (found)
                    bounds.Encapsulate(renderer.bounds);
                else
                    bounds = renderer.bounds;

                found = true;
            }

            ClearScratch();
            return found;
        }

        private static bool TryCollectRenderers(TileWorldCreatorManager manager, out Bounds bounds)
        {
            bounds = default;
            if (manager == null)
                return false;

            Renderers.Clear();
            manager.GetComponentsInChildren(true, Renderers);
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

        private static float ResolveChunkWorldSize(TileWorldCreatorManager manager)
        {
            Configuration config = manager != null ? manager.configuration : null;
            float cellSize = config != null && config.cellSize > 0.0001f ? config.cellSize : 1f;
            int chunkSize = config != null ? Mathf.Max(1, config.clusterCellSize) : 1;
            return Mathf.Max(0.0001f, cellSize * chunkSize);
        }

        private static Vector2Int ToChunkCoord(Vector3 center, Vector3 origin, float chunkWorldSize)
        {
            return new Vector2Int(
                Mathf.FloorToInt((center.x - origin.x) / chunkWorldSize),
                Mathf.FloorToInt((center.z - origin.z) / chunkWorldSize));
        }

        private static void ClearScratch()
        {
            Renderers.Clear();
            BoundsByChunk.Clear();
        }
    }
}
