using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Assigns one reserved Rendering Layer bit only to physical
    /// terrain/water surfaces. Decorative world overlays are excluded
    /// before the FogSurfaceDepth renderer list is built.
    /// </summary>
    internal static class FogSurfaceAutoTagger
    {
        private const float RefreshIntervalSeconds = 0.5f;

        private static readonly Dictionary<Renderer, uint>
            OriginalMasks =
                new Dictionary<Renderer, uint>();

        private static readonly List<Renderer>
            RemovalBuffer =
                new List<Renderer>();

        private static float _nextRefreshTime =
            float.NegativeInfinity;

        private static int _lastSceneHandle =
            int.MinValue;

        private static uint _activeBit;

        public static int IncludedCount =>
            OriginalMasks.Count;

        public static void RefreshIfNeeded(
            uint renderingLayerBit,
            string[] includeTokens,
            string[] excludeTokens)
        {
            if (!Application.isPlaying
                || renderingLayerBit == 0u)
            {
                return;
            }

            float now =
                Time.realtimeSinceStartup;

            int sceneHandle =
                SceneManager.GetActiveScene().handle;

            bool bitChanged =
                _activeBit != renderingLayerBit;

            bool sceneChanged =
                _lastSceneHandle != sceneHandle;

            if (!bitChanged
                && !sceneChanged
                && now < _nextRefreshTime)
            {
                return;
            }

            if (bitChanged)
            {
                RestoreAll();
            }

            _activeBit =
                renderingLayerBit;

            _lastSceneHandle =
                sceneHandle;

            _nextRefreshTime =
                now + RefreshIntervalSeconds;

            Renderer[] renderers =
                UnityEngine.Object.FindObjectsByType<Renderer>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);

            var seen =
                new HashSet<Renderer>();

            for (int i = 0;
                 i < renderers.Length;
                 i++)
            {
                Renderer renderer =
                    renderers[i];

                if (renderer == null)
                    continue;

                bool include =
                    ShouldInclude(
                        renderer,
                        includeTokens,
                        excludeTokens);

                if (include)
                {
                    seen.Add(renderer);

                    if (!OriginalMasks.ContainsKey(renderer))
                    {
                        OriginalMasks.Add(
                            renderer,
                            renderer.renderingLayerMask);
                    }

                    renderer.renderingLayerMask =
                        OriginalMasks[renderer]
                        | renderingLayerBit;
                }
                else
                {
                    RestoreRendererIfTracked(
                        renderer);
                }
            }

            RemovalBuffer.Clear();

            foreach (KeyValuePair<Renderer, uint> pair
                     in OriginalMasks)
            {
                Renderer renderer =
                    pair.Key;

                if (renderer == null
                    || !seen.Contains(renderer))
                {
                    RemovalBuffer.Add(renderer);
                }
            }

            for (int i = 0;
                 i < RemovalBuffer.Count;
                 i++)
            {
                RestoreRendererIfTracked(
                    RemovalBuffer[i]);
            }

            RemovalBuffer.Clear();
        }

        public static void RestoreAll()
        {
            foreach (KeyValuePair<Renderer, uint> pair
                     in OriginalMasks)
            {
                Renderer renderer =
                    pair.Key;

                if (renderer != null)
                {
                    renderer.renderingLayerMask =
                        pair.Value;
                }
            }

            OriginalMasks.Clear();
            RemovalBuffer.Clear();

            _activeBit = 0u;
            _lastSceneHandle = int.MinValue;
            _nextRefreshTime =
                float.NegativeInfinity;
        }

        private static void RestoreRendererIfTracked(
            Renderer renderer)
        {
            if (renderer == null)
                return;

            if (!OriginalMasks.TryGetValue(
                    renderer,
                    out uint originalMask))
            {
                return;
            }

            renderer.renderingLayerMask =
                originalMask;

            OriginalMasks.Remove(renderer);
        }

        private static bool ShouldInclude(
            Renderer renderer,
            string[] includeTokens,
            string[] excludeTokens)
        {
            if (!renderer.enabled
                || !renderer.gameObject.activeInHierarchy)
            {
                return false;
            }

            if (renderer.GetComponentInParent<Canvas>()
                != null)
            {
                return false;
            }

            if (renderer.GetComponentInParent<
                    FogSurfaceExclusion>()
                != null)
            {
                return false;
            }

            if (renderer.GetComponentInParent<
                    FogSurfaceContributor>()
                != null)
            {
                return true;
            }

            string classificationText =
                BuildClassificationText(
                    renderer);

            if (ContainsAny(
                    classificationText,
                    excludeTokens))
            {
                return false;
            }

            return ContainsAny(
                classificationText,
                includeTokens);
        }

        private static string BuildClassificationText(
            Renderer renderer)
        {
            var builder =
                new StringBuilder(256);

            Transform current =
                renderer.transform;

            int hierarchyDepth = 0;

            while (current != null
                   && hierarchyDepth < 10)
            {
                builder.Append(
                    current.name);

                builder.Append('|');

                current =
                    current.parent;

                hierarchyDepth++;
            }

            Material[] materials =
                renderer.sharedMaterials;

            if (materials != null)
            {
                for (int i = 0;
                     i < materials.Length;
                     i++)
                {
                    Material material =
                        materials[i];

                    if (material == null)
                        continue;

                    builder.Append(
                        material.name);

                    builder.Append('|');

                    if (material.shader != null)
                    {
                        builder.Append(
                            material.shader.name);

                        builder.Append('|');
                    }
                }
            }

            return builder.ToString();
        }

        private static bool ContainsAny(
            string source,
            string[] tokens)
        {
            if (string.IsNullOrEmpty(source)
                || tokens == null)
            {
                return false;
            }

            for (int i = 0;
                 i < tokens.Length;
                 i++)
            {
                string token =
                    tokens[i];

                if (string.IsNullOrWhiteSpace(token))
                    continue;

                if (source.IndexOf(
                        token,
                        StringComparison.OrdinalIgnoreCase)
                    >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}