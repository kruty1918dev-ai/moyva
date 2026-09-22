using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Evaluates <see cref="GeneratorMapRecipe"/> layers top-down: each layer's
    /// mask steps run in authored order and produce the layer's final mask.
    /// Finished masks are published so later layers can reference them via
    /// <see cref="LayerMaskReferenceStep"/>.
    /// </summary>
    internal static class GeneratorMaskEvaluator
    {
        /// <summary>
        /// Evaluates all enabled recipe layers in <see cref="GeneratorMapLayer.SortingOrder"/>
        /// order and returns their final masks keyed by layer id.
        /// </summary>
        public static Dictionary<string, bool[,]> EvaluateMasks(
            GeneratorMapRecipe recipe,
            int seed,
            Vector2Int mapSize,
            ISet<string> skippedLayerIds = null,
            float[,] terrainHeightField = null)
        {
            var masks = new Dictionary<string, bool[,]>(System.StringComparer.Ordinal);
            var layers = OrderedLayers(recipe, skippedLayerIds);
            if (layers.Count == 0)
                return masks;

            var safeSize = new Vector2Int(Mathf.Max(1, mapSize.x), Mathf.Max(1, mapSize.y));
            foreach (var layer in layers)
            {
                var context = new GeneratorMaskContext(seed, safeSize, masks, terrainHeightField);
                masks[layer.Id] = EvaluateLayerMask(layer, context);
            }
            return masks;
        }

        public static bool[,] EvaluateLayerMask(
            GeneratorMapLayer layer,
            GeneratorMaskContext context)
        {
            if (layer?.Steps == null || layer.Steps.Count == 0)
                return null;

            bool[,] mask = null;
            foreach (var step in layer.Steps)
            {
                if (step == null || !step.Enabled)
                    continue;

                if (step is TwcModifierMaskStep twcStep)
                {
                    mask = twcStep.IsGenerator
                        ? Combine(mask, twcStep.GenerateMask(context), step.Combine)
                        : twcStep.TransformMask(mask, context);
                    continue;
                }

                if (step is GeneratorMaskSourceStep source)
                {
                    mask = Combine(mask, source.GenerateMask(context), step.Combine);
                    continue;
                }

                if (step is GeneratorMaskTransformStep transform)
                    mask = transform.TransformMask(mask, context);
            }
            return mask;
        }

        public static bool[,] Combine(bool[,] current, bool[,] produced, MaskCombineMode mode)
        {
            if (produced == null)
                return current;
            if (current == null)
                return mode == MaskCombineMode.Subtract || mode == MaskCombineMode.Intersect
                    ? null
                    : produced;
            if (mode == MaskCombineMode.Replace)
                return produced;

            int width = current.GetLength(0);
            int height = current.GetLength(1);
            int w = Mathf.Min(width, produced.GetLength(0));
            int h = Mathf.Min(height, produced.GetLength(1));
            var result = new bool[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                bool b = x < w && y < h && produced[x, y];
                result[x, y] = mode switch
                {
                    MaskCombineMode.Add => current[x, y] || b,
                    MaskCombineMode.Subtract => current[x, y] && !b,
                    MaskCombineMode.Intersect => current[x, y] && b,
                    MaskCombineMode.Xor => current[x, y] != b,
                    _ => current[x, y]
                };
            }
            return result;
        }

        /// <summary>
        /// Evaluates the recipe's object scatter passes against the evaluated
        /// layer masks and returns the generated TWC object layers.
        /// </summary>
        public static IReadOnlyList<ObjectPlacementLayer> CollectObjectPlacements(
            GeneratorMapRecipe recipe,
            IReadOnlyDictionary<string, bool[,]> layerMasks,
            int seed)
        {
            var result = new List<ObjectPlacementLayer>();
            if (recipe?.ObjectPlacements == null || recipe.ObjectPlacements.Count == 0)
                return result;

            string fallbackLayerId = null;
            foreach (var layer in OrderedLayers(recipe, null))
            {
                fallbackLayerId = layer.Id;
                break;
            }

            foreach (var placement in recipe.ObjectPlacements)
            {
                if (placement == null)
                    continue;

                string targetLayerId = !string.IsNullOrWhiteSpace(placement.TargetLayerId)
                    ? placement.TargetLayerId.Trim()
                    : fallbackLayerId;
                if (string.IsNullOrWhiteSpace(targetLayerId)
                    || layerMasks == null
                    || !layerMasks.TryGetValue(targetLayerId, out var placementMask)
                    || placementMask == null)
                    continue;

                bool[,] exclusionMask = null;
                if (!string.IsNullOrWhiteSpace(placement.ExclusionLayerId))
                    layerMasks.TryGetValue(placement.ExclusionLayerId.Trim(), out exclusionMask);

                var layer = BuildObjectPlacementLayer(
                    placement,
                    targetLayerId,
                    placementMask,
                    exclusionMask,
                    seed);
                if (layer != null)
                    result.Add(layer);
            }
            return result;
        }

        private static ObjectPlacementLayer BuildObjectPlacementLayer(
            GeneratorObjectPlacement placement,
            string targetLayerId,
            bool[,] placementMask,
            bool[,] exclusionMask,
            int seed)
        {
            var scatterMask = new ScatterMask(placementMask, exclusionMask);
            var rule = placement.Rule ?? new ObjectPlacementRule();
            var cluster = placement.Cluster ?? new ClusterSettings();

            var candidates = cluster.Enabled
                ? ObjectPlacementScatterUtility.ScatterClustered(scatterMask, cluster, rule, seed)
                : ObjectPlacementScatterUtility.ScatterUniform(scatterMask, rule, seed);

            var layer = new ObjectPlacementLayer(
                string.IsNullOrWhiteSpace(placement.LayerName) ? "Props" : placement.LayerName)
            {
                TargetLayerId = targetLayerId,
                Rule = rule,
                Cluster = cluster
            };

            if (placement.Prefabs != null)
            {
                for (int i = 0; i < placement.Prefabs.Count; i++)
                {
                    var entry = placement.Prefabs[i];
                    if (entry?.Prefab != null)
                        layer.Prefabs.Add(entry);
                }
            }

            var grass = placement.Grass;
            if (grass?.Prefab != null
                && !layer.Prefabs.Exists(entry => entry?.Prefab == grass.Prefab))
            {
                layer.Prefabs.Add(new ObjectPrefabEntry
                {
                    Prefab = grass.Prefab,
                    Weight = 1f,
                    MinScale = 0.85f,
                    MaxScale = 1.15f,
                    RandomYaw = true,
                    AlignToSurface = true,
                    ClusterAffinity = 1f,
                    ColorVariation = grass.Tint
                });
            }

            var filtered = FilterCandidates(candidates, exclusionMask);
            AssignPrefabIndices(filtered, layer.Prefabs, seed);
            layer.Candidates.AddRange(filtered);
            return layer;
        }

        private static List<ScatterCandidate> FilterCandidates(
            List<ScatterCandidate> candidates,
            bool[,] exclude)
        {
            var result = new List<ScatterCandidate>();
            if (candidates == null)
                return result;
            for (int i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (exclude != null
                    && candidate.Cell.x >= 0
                    && candidate.Cell.y >= 0
                    && candidate.Cell.x < exclude.GetLength(0)
                    && candidate.Cell.y < exclude.GetLength(1)
                    && exclude[candidate.Cell.x, candidate.Cell.y])
                    continue;
                result.Add(candidate);
            }
            return result;
        }

        private static void AssignPrefabIndices(
            List<ScatterCandidate> candidates,
            List<ObjectPrefabEntry> prefabs,
            int seed)
        {
            if (candidates == null || prefabs == null || prefabs.Count == 0)
                return;

            float totalWeight = 0f;
            for (int i = 0; i < prefabs.Count; i++)
                totalWeight += Mathf.Max(0f, prefabs[i]?.Weight ?? 0f);
            if (totalWeight <= 0f)
                totalWeight = prefabs.Count;

            for (int i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate.PrefabIndex >= 0)
                    continue;
                var random = new System.Random(
                    unchecked(seed + candidate.Cell.x * 73856093 ^ candidate.Cell.y * 19349663));
                float roll = (float)random.NextDouble() * totalWeight;
                float cumulative = 0f;
                int selected = 0;
                for (int p = 0; p < prefabs.Count; p++)
                {
                    cumulative += Mathf.Max(0f, prefabs[p]?.Weight ?? 0f);
                    if (roll <= cumulative)
                    {
                        selected = p;
                        break;
                    }
                }
                candidates[i] = candidate.WithPrefabIndex(selected);
            }
        }

        internal static List<GeneratorMapLayer> OrderedLayers(
            GeneratorMapRecipe recipe,
            ISet<string> skippedLayerIds)
        {
            var result = new List<GeneratorMapLayer>();
            if (recipe?.Layers == null)
                return result;
            foreach (var layer in recipe.Layers)
            {
                if (layer == null || !layer.Enabled)
                    continue;
                if (skippedLayerIds != null && skippedLayerIds.Contains(layer.Id))
                    continue;
                result.Add(layer);
            }
            result.Sort((a, b) => a.SortingOrder.CompareTo(b.SortingOrder));
            return result;
        }
    }
}
