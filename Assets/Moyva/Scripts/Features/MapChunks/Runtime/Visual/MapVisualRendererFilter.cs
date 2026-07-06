using System;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    internal sealed class MapVisualRendererFilter : IMapVisualRendererFilter
    {
        private readonly IMapChunkSettingsProvider _settings;

        public MapVisualRendererFilter(IMapChunkSettingsProvider settings)
        {
            _settings = settings;
        }

        public bool CanRegister(Renderer renderer)
        {
            return IsCommonVisualRenderer(renderer);
        }

        public bool CanPartition(Renderer renderer, IMapVisualChunkRootService roots)
        {
            if (!IsCommonVisualRenderer(renderer))
                return false;

            return roots == null
                   || !roots.IsChunkRoot(renderer.transform)
                   && !roots.IsChunkRoot(renderer.transform.parent);
        }

        private bool IsCommonVisualRenderer(Renderer renderer)
        {
            if (renderer == null || renderer.transform == null)
                return false;
            if (renderer.GetComponentInParent<Canvas>() != null)
                return false;
            if (IsTileWorldCreatorHierarchy(renderer.transform))
                return false;

            int bit = 1 << renderer.gameObject.layer;
            return (_settings.VisualDiscoveryLayerMask.value & bit) != 0
                   && !HasIgnoredName(renderer.transform);
        }

        private static bool IsTileWorldCreatorHierarchy(Transform transform)
        {
            for (var current = transform; current != null; current = current.parent)
            {
                string name = current.name;
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (name.IndexOf("TileWorldCreator", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("Moyva TWC", StringComparison.OrdinalIgnoreCase) >= 0
                    || IsTwcClusterName(name))
                    return true;
            }

            return false;
        }

        private static bool IsTwcClusterName(string name)
        {
            return name.StartsWith("Layer_", StringComparison.OrdinalIgnoreCase)
                   && name.IndexOf("_Cluster_", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool HasIgnoredName(Transform transform)
        {
            var tokens = _settings.IgnoredRendererNameTokens;
            for (var current = transform; current != null; current = current.parent)
            for (int i = 0; i < tokens.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(tokens[i])
                    && current.name.IndexOf(tokens[i], StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }
    }
}
