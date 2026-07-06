using Kruty1918.Moyva.Grid.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MenuPreviewSpritePixelSource : IMenuPreviewSpritePixelSource
    {
        public bool TryFromPrefab(GameObject prefab, string stableId, MoyvaProjectSettingsSO settings, out MenuPreviewSpriteData data)
        {
            data = default;
            if (prefab == null)
                return false;

            if (MoyvaPrefabPreviewRenderer.TryRenderMeshPrefabPreview(prefab, settings, out var preview)
                && preview.IsValid)
            {
                data = new MenuPreviewSpriteData(preview.Pixels, preview.Width, preview.Height);
                return true;
            }

            var spriteRenderer = TryGetPrimarySpriteRenderer(prefab);
            if (spriteRenderer != null && TryFromSprite(spriteRenderer.sprite, spriteRenderer.color, out data))
                return true;

            return TryGetPrefabRepresentativeColor(prefab, stableId, out var color)
                && CreateSolid(color, out data);
        }

        public bool TryFromSprite(Sprite sprite, Color tint, out MenuPreviewSpriteData data)
        {
            data = default;
            if (sprite == null || sprite.texture == null)
                return false;

            Rect rect = sprite.textureRect;
            int x = Mathf.Max(0, Mathf.RoundToInt(rect.x));
            int y = Mathf.Max(0, Mathf.RoundToInt(rect.y));
            int width = Mathf.Max(1, Mathf.RoundToInt(rect.width));
            int height = Mathf.Max(1, Mathf.RoundToInt(rect.height));

            Color[] pixels;
            try
            {
                pixels = sprite.texture.GetPixels(x, y, width, height);
            }
            catch
            {
                pixels = MenuPreviewSpriteTextureReader.TryReadViaRenderTexture(sprite, x, y, width, height);
            }

            if (pixels == null || pixels.Length == 0)
                return false;

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] *= tint;

            data = new MenuPreviewSpriteData(pixels, width, height);
            return true;
        }

        private static SpriteRenderer TryGetPrimarySpriteRenderer(GameObject prefab)
        {
            var renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
            SpriteRenderer best = null;
            int bestLayer = int.MinValue;
            int bestOrder = int.MinValue;
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || renderer.sprite == null)
                    continue;
                if (best != null && renderer.sortingLayerID < bestLayer)
                    continue;
                if (best != null && renderer.sortingLayerID == bestLayer && renderer.sortingOrder < bestOrder)
                    continue;

                best = renderer;
                bestLayer = renderer.sortingLayerID;
                bestOrder = renderer.sortingOrder;
            }

            return best;
        }

        private static bool TryGetPrefabRepresentativeColor(GameObject prefab, string stableId, out Color color)
        {
            var renderers = prefab.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                var material = renderers[i] != null ? renderers[i].sharedMaterial : null;
                if (material != null && material.HasProperty("_BaseColor"))
                {
                    color = material.GetColor("_BaseColor");
                    color.a = Mathf.Max(0.6f, color.a);
                    return true;
                }
                if (material != null && material.HasProperty("_Color"))
                {
                    color = material.color;
                    color.a = Mathf.Max(0.6f, color.a);
                    return true;
                }
            }

            color = MenuPreviewColorUtility.HashColor(stableId, 0.9f);
            return true;
        }

        private static bool CreateSolid(Color color, out MenuPreviewSpriteData data)
        {
            color.a = Mathf.Max(0.35f, color.a);
            data = new MenuPreviewSpriteData(new[] { color }, 1, 1);
            return true;
        }
    }
}
