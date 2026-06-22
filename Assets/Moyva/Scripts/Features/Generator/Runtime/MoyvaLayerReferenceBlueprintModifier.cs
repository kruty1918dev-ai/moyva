using System.Collections.Generic;
using GiantGrey.TileWorldCreator;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Службовий TWC-модифікатор, який дає blueprint-шару стартову маску з іншого blueprint-шару.
    /// Використовується компілятором графа для ноди Layer Ref.
    /// </summary>
    public sealed class MoyvaLayerReferenceBlueprintModifier : BlueprintModifier
    {
        private static readonly HashSet<string> ResolvingLayerKeys = new();

        [HideInInspector] public string sourceBlueprintLayerGuid;
        [HideInInspector] public string sourceGraphLayerId;
        [HideInInspector] public string sourceLayerName;

        public override HashSet<Vector2> Execute(HashSet<Vector2> positions, BlueprintLayer layer)
        {
            var fallback = positions ?? new HashSet<Vector2>();
            if (asset == null || string.IsNullOrEmpty(sourceBlueprintLayerGuid))
                return fallback;

            var sourceLayer = asset.GetBlueprintLayerByGuid(sourceBlueprintLayerGuid);
            if (sourceLayer?.allPositions == null)
                return fallback;

            string currentKey = BuildLayerKey(asset, layer?.guid);
            if (!ResolvingLayerKeys.Add(currentKey))
            {
                Debug.LogWarning(
                    $"[MoyvaLayerRef] Circular blueprint layer reference detected on '{layer?.layerName ?? "unknown layer"}'.");
                return fallback;
            }

            try
            {
                EnsureSourceLayerGenerated(sourceLayer, layer);
                return new HashSet<Vector2>(sourceLayer.allPositions);
            }
            finally
            {
                ResolvingLayerKeys.Remove(currentKey);
            }
        }

        private void EnsureSourceLayerGenerated(BlueprintLayer sourceLayer, BlueprintLayer currentLayer)
        {
            if (sourceLayer == null || asset == null || !sourceLayer.isEnabled)
                return;

            string sourceKey = BuildLayerKey(asset, sourceLayer.guid);
            string currentKey = BuildLayerKey(asset, currentLayer?.guid);
            if (sourceKey == currentKey)
                return;

            if (ResolvingLayerKeys.Contains(sourceKey))
            {
                Debug.LogWarning(
                    $"[MoyvaLayerRef] Circular blueprint layer reference detected between '{currentLayer?.layerName ?? "unknown layer"}' and '{sourceLayer.layerName ?? "unknown layer"}'.");
                return;
            }

            sourceLayer.ExecuteLayer(asset, null);
        }

        private static string BuildLayerKey(Configuration configuration, string layerGuid)
        {
            return $"{configuration.GetInstanceID()}:{layerGuid ?? string.Empty}";
        }
    }
}
