using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class TileWorldCreatorTileTransformCollector : ITileWorldCreatorTileTransformCollector
    {
        public int Collect(Transform root, List<TileWorldCreatorTileTransformSample> buffer, HashSet<int> collectedIds, out int skippedSideWallRenderers)
        {
            skippedSideWallRenderers = 0;
            var renderers = root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
                CollectRenderer(root, renderers[i], buffer, collectedIds, ref skippedSideWallRenderers);

            return renderers.Length;
        }

        private static void CollectRenderer(
            Transform root,
            Renderer renderer,
            List<TileWorldCreatorTileTransformSample> buffer,
            HashSet<int> collectedIds,
            ref int skippedSideWallRenderers)
        {
            if (renderer == null)
                return;
            if (renderer.GetComponentInParent<TileWorldCreatorTerrainSideWallBuilder>() != null)
            {
                skippedSideWallRenderers++;
                return;
            }

            Transform tileRoot = ResolveTileRoot(root, renderer.transform);
            if (tileRoot == root || !collectedIds.Add(tileRoot.GetInstanceID()))
                return;

            buffer.Add(new TileWorldCreatorTileTransformSample(tileRoot, renderer.bounds.center));
        }

        private static Transform ResolveTileRoot(Transform root, Transform source)
        {
            var t = source;
            while (t.parent != null && t.parent.parent != null && t.parent.parent.parent != null && t.parent.parent.parent != root)
                t = t.parent;

            return t;
        }
    }
}
