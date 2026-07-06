using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    internal sealed class MapVisualRendererCollector : IMapVisualRendererCollector
    {
        private static readonly string[] RootNames =
        {
            "TilesRoot", "ObjectsRoot", "BuildingsRoot", "PlayerBuildingsRoot",
            "LayersRoot", "MapVisualChunks", "TileWorldCreator", "Tile World Creator"
        };

        private readonly List<Renderer> _rootBuffer = new(128);

        public void CollectPreferredRoots(List<Renderer> renderers)
        {
            if (renderers == null)
                return;

            renderers.Clear();
            for (int i = 0; i < RootNames.Length; i++)
            {
                var root = GameObject.Find(RootNames[i]);
                if (root == null)
                    continue;

                _rootBuffer.Clear();
                root.GetComponentsInChildren(true, _rootBuffer);
                renderers.AddRange(_rootBuffer);
            }
        }

        public void CollectScene(List<Renderer> renderers)
        {
            if (renderers == null)
                return;

            renderers.Clear();
            renderers.AddRange(Object.FindObjectsByType<Renderer>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None));
        }
    }
}
