using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    internal sealed class MapVisualRendererCollector :
        IMapVisualRendererCollector
    {
        private static readonly string[] RootNames =
        {
            "TilesRoot",
            "ObjectsRoot",
            "BuildingsRoot",
            "PlayerBuildingsRoot",
            "LayersRoot",
            "MapVisualChunks",
            "TileWorldCreator",
            "Tile World Creator"
        };

        private readonly List<Renderer> _rootBuffer =
            new List<Renderer>(128);

        private readonly HashSet<Renderer> _deduplication =
            new HashSet<Renderer>();

        public void CollectPreferredRoots(
            List<Renderer> renderers)
        {
            if (renderers == null)
                return;

            renderers.Clear();
            _deduplication.Clear();

            for (int i = 0;
                 i < RootNames.Length;
                 i++)
            {
                GameObject root =
                    GameObject.Find(
                        RootNames[i]);

                if (root == null)
                    continue;

                _rootBuffer.Clear();

                root.GetComponentsInChildren(
                    true,
                    _rootBuffer);

                for (int rendererIndex = 0;
                     rendererIndex < _rootBuffer.Count;
                     rendererIndex++)
                {
                    Renderer renderer =
                        _rootBuffer[rendererIndex];

                    if (renderer != null
                        && _deduplication.Add(renderer))
                    {
                        renderers.Add(renderer);
                    }
                }
            }

            _rootBuffer.Clear();
            _deduplication.Clear();
        }

        public void CollectScene(
            List<Renderer> renderers)
        {
            if (renderers == null)
                return;

            renderers.Clear();

            renderers.AddRange(
                Object.FindObjectsByType<Renderer>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None));
        }
    }
}
