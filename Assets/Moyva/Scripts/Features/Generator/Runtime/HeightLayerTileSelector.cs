using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal static class HeightLayerTileSelector
    {
        internal static string ResolveTileId(HeightLayer[] layers, float heightValue, int x, int y, int seed)
        {
            if (layers == null || layers.Length == 0)
                return string.Empty;

            for (int layerIndex = 0; layerIndex < layers.Length; layerIndex++)
            {
                var layer = layers[layerIndex];
                if (heightValue >= layer.MinHeight && heightValue <= layer.MaxHeight)
                    return SelectTileId(layer, x, y, seed);
            }

            return SelectTileId(layers[^1], x, y, seed);
        }

        internal static string SelectTileId(HeightLayer layer, int x, int y, int seed)
        {
            int variantCount = layer.WeightedVariants?.Length ?? 0;
            bool hasLegacy = layer.VariantTileIDs != null && layer.VariantTileIDs.Length > 0;

            if (variantCount == 0 && !hasLegacy)
                return layer.TileID;

            float roll = (PositiveHash(seed, x, y) % 100000) / 100000f;
            float cumulative = 0f;

            if (!string.IsNullOrEmpty(layer.TileID))
            {
                cumulative += Mathf.Clamp01(layer.TileIDChance);
                if (roll < cumulative)
                    return layer.TileID;
            }

            if (variantCount > 0)
            {
                for (int i = 0; i < variantCount; i++)
                {
                    var entry = layer.WeightedVariants[i];
                    if (entry == null || string.IsNullOrEmpty(entry.TileID))
                        continue;

                    cumulative += Mathf.Clamp01(entry.Chance);
                    if (roll < cumulative)
                        return entry.TileID;
                }
            }
            else
            {
                float legacyEach = Mathf.Clamp01((1f - Mathf.Clamp01(layer.TileIDChance)) / layer.VariantTileIDs.Length);
                for (int i = 0; i < layer.VariantTileIDs.Length; i++)
                {
                    string variantId = layer.VariantTileIDs[i];
                    if (string.IsNullOrEmpty(variantId))
                        continue;

                    cumulative += legacyEach;
                    if (roll < cumulative)
                        return variantId;
                }
            }

            return string.IsNullOrEmpty(layer.TileID) ? string.Empty : layer.TileID;
        }

        private static int PositiveHash(int seed, int x, int y)
        {
            unchecked
            {
                uint h = 2166136261u;
                h = (h ^ (uint)seed) * 16777619u;
                h = (h ^ (uint)x) * 16777619u;
                h = (h ^ (uint)y) * 16777619u;
                h ^= h >> 17;
                h *= 0xbf58476du;
                h ^= h >> 31;
                h *= 0x94d049bbu;
                h ^= h >> 16;
                return (int)(h & 0x7FFFFFFFu);
            }
        }
    }
}