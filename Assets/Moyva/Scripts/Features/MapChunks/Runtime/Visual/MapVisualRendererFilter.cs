using System;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;

namespace Kruty1918.Moyva.MapChunks.Runtime
{
    internal sealed class MapVisualRendererFilter :
        IMapVisualRendererFilter
    {
        private readonly IMapChunkSettingsProvider _settings;

        public MapVisualRendererFilter(
            IMapChunkSettingsProvider settings)
        {
            _settings = settings;
        }

        public bool CanRegister(
            Renderer renderer)
        {
            return IsCommonVisualRenderer(
                renderer);
        }

        public bool CanPartition(
            Renderer renderer,
            IMapVisualChunkRootService roots)
        {
            if (!IsCommonVisualRenderer(
                    renderer))
            {
                return false;
            }

            if (!HasFiniteSpatialBounds(
                    renderer.bounds))
            {
                return false;
            }

            if (roots == null)
                return true;

            for (Transform current = renderer.transform;
                 current != null;
                 current = current.parent)
            {
                if (roots.IsChunkRoot(current))
                    return false;
            }

            return true;
        }

        private bool IsCommonVisualRenderer(
            Renderer renderer)
        {
            if (renderer == null
                || renderer.transform == null)
            {
                return false;
            }

            if (renderer.GetComponentInParent<Canvas>()
                != null)
            {
                return false;
            }

            if (IsTileWorldCreatorHierarchy(
                    renderer.transform))
            {
                return false;
            }

            int bit =
                1 << renderer.gameObject.layer;

            return (
                       _settings
                           .VisualDiscoveryLayerMask
                           .value
                       & bit
                   ) != 0
                   && !HasIgnoredName(
                       renderer.transform);
        }

        private static bool HasFiniteSpatialBounds(
            Bounds bounds)
        {
            Vector3 center =
                bounds.center;

            Vector3 size =
                bounds.size;

            return IsFinite(center.x)
                && IsFinite(center.y)
                && IsFinite(center.z)
                && IsFinite(size.x)
                && IsFinite(size.y)
                && IsFinite(size.z)
                && size.sqrMagnitude > 0.00000001f;
        }

        private static bool IsFinite(
            float value)
        {
            return !float.IsNaN(value)
                && !float.IsInfinity(value);
        }

        private static bool IsTileWorldCreatorHierarchy(
            Transform transform)
        {
            for (Transform current = transform;
                 current != null;
                 current = current.parent)
            {
                string name =
                    current.name;

                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (name.IndexOf(
                        "TileWorldCreator",
                        StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf(
                        "Moyva TWC",
                        StringComparison.OrdinalIgnoreCase) >= 0
                    || IsTwcClusterName(name))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsTwcClusterName(
            string name)
        {
            return name.StartsWith(
                       "Layer_",
                       StringComparison.OrdinalIgnoreCase)
                   && name.IndexOf(
                       "_Cluster_",
                       StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool HasIgnoredName(
            Transform transform)
        {
            var tokens =
                _settings.IgnoredRendererNameTokens;

            for (Transform current = transform;
                 current != null;
                 current = current.parent)
            {
                for (int i = 0;
                     i < tokens.Count;
                     i++)
                {
                    string token =
                        tokens[i];

                    if (!string.IsNullOrWhiteSpace(token)
                        && current.name.IndexOf(
                            token,
                            StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
