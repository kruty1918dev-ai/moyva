using System.Collections.Generic;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Будує Texture2D для menu-only прев'ю світу з фінальних мап генератора.
    /// Не використовує gameplay runtime і не залежить від внутрішніх типів генерації.
    /// </summary>
    public static class MenuWorldPreviewTextureBuilder
    {
        private const int DefaultPixelsPerTile = 4;
        private const int DefaultMaxTextureEdge = 1024;
        private const float ObjectOverlayScale = 1.35f;
        private const float BuildingOverlayScale = 0.9f;
        private const float OverlayAlphaScale = 0.9f;

        public static Texture2D Build(
            MenuWorldPreviewData previewData,
            TileRegistrySO tileRegistry,
            MapObjectRegistrySO objectRegistry = null,
            BuildingRegistrySO buildingRegistry = null,
            int pixelsPerTile = DefaultPixelsPerTile,
            int maxTextureEdge = DefaultMaxTextureEdge)
        {
            if (previewData == null || previewData.Width <= 0 || previewData.Height <= 0)
                return null;

            int mapWidth = previewData.Width;
            int mapHeight = previewData.Height;
            int tilePixelSize = Mathf.Max(1, pixelsPerTile);
            int textureWidth = mapWidth * tilePixelSize;
            int textureHeight = mapHeight * tilePixelSize;
            int maxEdge = Mathf.Max(64, maxTextureEdge);

            if (textureWidth > maxEdge || textureHeight > maxEdge)
            {
                float scale = maxEdge / (float)Mathf.Max(textureWidth, textureHeight);
                tilePixelSize = Mathf.Max(1, Mathf.FloorToInt(tilePixelSize * scale));
                textureWidth = mapWidth * tilePixelSize;
                textureHeight = mapHeight * tilePixelSize;
            }

            var canvas = new Color[textureWidth * textureHeight];
            var tileSpriteCache = BuildTileSpriteCache(tileRegistry, out SpritePixelData fallbackTileSprite);

            DrawTerrain(previewData, canvas, textureWidth, tilePixelSize, tileSpriteCache, fallbackTileSprite);
            DrawObjects(previewData, objectRegistry, canvas, textureWidth, textureHeight, tilePixelSize);
            DrawBuildings(previewData, buildingRegistry, canvas, textureWidth, textureHeight, tilePixelSize);

            var texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            texture.SetPixels(canvas);
            texture.Apply(false, false);
            return texture;
        }

        private static void DrawTerrain(
            MenuWorldPreviewData previewData,
            Color[] canvas,
            int canvasWidth,
            int pixelsPerTile,
            Dictionary<string, SpritePixelData> tileSpriteCache,
            SpritePixelData fallbackTileSprite)
        {
            bool hasBiomeMap = HasAnyStringValue(previewData.BiomeMap);
            if (hasBiomeMap)
            {
                bool hasFallbackSprite = fallbackTileSprite.IsValid;
                int hits = 0, misses = 0;
                var missedIds = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase);

                for (int y = 0; y < previewData.Height; y++)
                {
                    for (int x = 0; x < previewData.Width; x++)
                    {
                        string tileId = NormalizeId(previewData.BiomeMap[x, y]);
                        if (!string.IsNullOrEmpty(tileId) && tileSpriteCache.TryGetValue(tileId, out var spriteData))
                        {
                            hits++;
                            StampSprite(canvas, canvasWidth, x, y, pixelsPerTile, spriteData);
                        }
                        else if (hasFallbackSprite)
                        {
                            misses++;
                            if (!string.IsNullOrEmpty(tileId))
                                missedIds.Add(tileId);
                            StampSprite(canvas, canvasWidth, x, y, pixelsPerTile, fallbackTileSprite);
                        }
                        else
                        {
                            misses++;
                            if (!string.IsNullOrEmpty(tileId))
                                missedIds.Add(tileId);
                            FillTileRect(canvas, canvasWidth, x, y, pixelsPerTile, Color.black);
                        }
                    }
                }

                Debug.Log($"[MenuPreview] Terrain render: {hits} hits, {misses} misses. " +
                    (missedIds.Count > 0 ? $"Unknown tile IDs: [{string.Join(", ", missedIds)}]" : "All IDs resolved."));

                return;
            }

            DrawHeightFallback(previewData, canvas, canvasWidth, pixelsPerTile);
        }

        private static void DrawHeightFallback(
            MenuWorldPreviewData previewData,
            Color[] canvas,
            int canvasWidth,
            int pixelsPerTile)
        {
            if (previewData.HeightMap == null)
            {
                for (int y = 0; y < previewData.Height; y++)
                for (int x = 0; x < previewData.Width; x++)
                    FillTileRect(canvas, canvasWidth, x, y, pixelsPerTile, Color.black);
                return;
            }

            float minValue = float.MaxValue;
            float maxValue = float.MinValue;

            for (int y = 0; y < previewData.Height; y++)
            for (int x = 0; x < previewData.Width; x++)
            {
                float value = previewData.HeightMap[x, y];
                if (value < minValue) minValue = value;
                if (value > maxValue) maxValue = value;
            }

            float span = Mathf.Max(0.0001f, maxValue - minValue);
            for (int y = 0; y < previewData.Height; y++)
            {
                for (int x = 0; x < previewData.Width; x++)
                {
                    float value = previewData.HeightMap[x, y];
                    float t = Mathf.Clamp01((value - minValue) / span);
                    FillTileRect(canvas, canvasWidth, x, y, pixelsPerTile, new Color(t, t, t, 1f));
                }
            }
        }

        private static void DrawObjects(
            MenuWorldPreviewData previewData,
            MapObjectRegistrySO objectRegistry,
            Color[] canvas,
            int canvasWidth,
            int canvasHeight,
            int pixelsPerTile)
        {
            if (!HasAnyStringValue(previewData.ObjectMap))
                return;

            var objectSpriteCache = BuildObjectSpriteCache(objectRegistry);
            int renderedWithSprites = 0;
            int renderedWithDots = 0;
            for (int y = 0; y < previewData.Height; y++)
            {
                for (int x = 0; x < previewData.Width; x++)
                {
                    string objectId = NormalizeId(previewData.ObjectMap[x, y]);
                    if (string.IsNullOrEmpty(objectId))
                        continue;

                    if (TryResolveSpriteData(objectSpriteCache, objectId, out var spriteData))
                    {
                        StampSpriteOverlay(canvas, canvasWidth, canvasHeight, x, y, pixelsPerTile, spriteData, ObjectOverlayScale, OverlayAlphaScale, anchorToBottom: true);
                        renderedWithSprites++;
                    }
                    else
                    {
                        StampDot(canvas, canvasWidth, canvasHeight, x, y, pixelsPerTile, HashColor(objectId, OverlayAlphaScale));
                        renderedWithDots++;
                    }
                }
            }

            if (renderedWithDots > 0)
                Debug.Log($"[MenuPreview] Object overlay fallback dots: {renderedWithDots}, sprite overlays: {renderedWithSprites}.");
        }

        private static void DrawBuildings(
            MenuWorldPreviewData previewData,
            BuildingRegistrySO buildingRegistry,
            Color[] canvas,
            int canvasWidth,
            int canvasHeight,
            int pixelsPerTile)
        {
            if (!HasAnyStringValue(previewData.BuildingMap))
                return;

            if (buildingRegistry == null)
            {
                Debug.LogWarning("[MenuPreview] BuildingMap has values, but BuildingRegistry is not assigned. Building overlay will be skipped.");
                return;
            }

            var buildingSpriteCache = BuildBuildingSpriteCache(buildingRegistry);
            int renderedWithSprites = 0;
            int renderedWithDots = 0;
            for (int y = 0; y < previewData.Height; y++)
            {
                for (int x = 0; x < previewData.Width; x++)
                {
                    string buildingId = NormalizeId(previewData.BuildingMap[x, y]);
                    if (string.IsNullOrEmpty(buildingId))
                        continue;

                    if (TryResolveSpriteData(buildingSpriteCache, buildingId, out var spriteData))
                    {
                        StampSpriteOverlay(canvas, canvasWidth, canvasHeight, x, y, pixelsPerTile, spriteData, BuildingOverlayScale, OverlayAlphaScale, anchorToBottom: true);
                        renderedWithSprites++;
                    }
                    else
                    {
                        StampDot(canvas, canvasWidth, canvasHeight, x, y, pixelsPerTile, HashColor(buildingId, OverlayAlphaScale));
                        renderedWithDots++;
                    }
                }
            }

            if (renderedWithDots > 0)
                Debug.Log($"[MenuPreview] Building overlay fallback dots: {renderedWithDots}, sprite overlays: {renderedWithSprites}.");
        }

        private static bool HasAnyStringValue(string[,] map)
        {
            if (map == null)
                return false;

            int width = map.GetLength(0);
            int height = map.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!string.IsNullOrEmpty(map[x, y]))
                        return true;
                }
            }

            return false;
        }

        private static Dictionary<string, SpritePixelData> BuildTileSpriteCache(TileRegistrySO registry, out SpritePixelData fallbackSprite)
        {
            var cache = new Dictionary<string, SpritePixelData>(System.StringComparer.OrdinalIgnoreCase);
            fallbackSprite = default;

            if (registry?.Definitions == null)
            {
                Debug.LogWarning("[MenuPreview] TileRegistry is null or has no definitions!");
                return cache;
            }

            var failed = new System.Text.StringBuilder();
            foreach (var definition in registry.Definitions)
            {
                string tileId = NormalizeId(definition?.Id);
                if (string.IsNullOrEmpty(tileId))
                    continue;

                if (definition.VisualPrefab == null)
                {
                    failed.Append(tileId).Append("(no prefab) ");
                    continue;
                }

                var spriteRenderer = definition.VisualPrefab.GetComponentInChildren<SpriteRenderer>(true);
                if (spriteRenderer == null || spriteRenderer.sprite == null)
                {
                    failed.Append(tileId).Append("(no sprite) ");
                    continue;
                }

                if (!TryGetSpritePixels(spriteRenderer.sprite, spriteRenderer.color, out var spriteData))
                {
                    failed.Append(tileId).Append("(read fail) ");
                    continue;
                }

                cache[tileId] = spriteData;
                if (!fallbackSprite.IsValid)
                    fallbackSprite = spriteData;
            }

            Debug.Log($"[MenuPreview] Tile sprite cache: {cache.Count}/{registry.Definitions.Length} entries loaded. Keys: [{string.Join(", ", cache.Keys)}]");
            if (failed.Length > 0)
                Debug.LogWarning($"[MenuPreview] Failed to load sprites for: {failed}");

            return cache;
        }

        private static Dictionary<string, SpritePixelData> BuildObjectSpriteCache(MapObjectRegistrySO registry)
        {
            var cache = new Dictionary<string, SpritePixelData>(System.StringComparer.OrdinalIgnoreCase);
            if (registry?.Definitions == null)
                return cache;

            foreach (var definition in registry.Definitions)
            {
                string objectId = NormalizeId(definition?.Id);
                if (string.IsNullOrEmpty(objectId) || definition.VisualPrefab == null)
                    continue;

                var spriteRenderer = definition.VisualPrefab.GetComponentInChildren<SpriteRenderer>(true);
                if (spriteRenderer == null || spriteRenderer.sprite == null)
                    continue;

                if (TryGetSpritePixels(spriteRenderer.sprite, spriteRenderer.color, out var spriteData))
                    cache[objectId] = spriteData;
            }

            return cache;
        }

        private static Dictionary<string, SpritePixelData> BuildBuildingSpriteCache(BuildingRegistrySO registry)
        {
            var cache = new Dictionary<string, SpritePixelData>(System.StringComparer.OrdinalIgnoreCase);
            var definitions = registry?.GetAll();
            if (definitions == null)
                return cache;

            for (int i = 0; i < definitions.Length; i++)
            {
                var definition = definitions[i];
                string buildingId = NormalizeId(definition?.Id);
                if (string.IsNullOrEmpty(buildingId))
                    continue;

                Sprite sprite = definition.Icon;
                Color tint = Color.white;
                if (sprite == null && definition.Prefab != null)
                {
                    var spriteRenderer = definition.Prefab.GetComponentInChildren<SpriteRenderer>(true);
                    if (spriteRenderer != null)
                    {
                        sprite = spriteRenderer.sprite;
                        tint = spriteRenderer.color;
                    }
                }

                if (sprite == null)
                    continue;

                if (TryGetSpritePixels(sprite, tint, out var spriteData))
                    cache[buildingId] = spriteData;
            }

            return cache;
        }

        private static bool TryGetSpritePixels(Sprite sprite, Color tint, out SpritePixelData spriteData)
        {
            spriteData = default;
            if (sprite == null || sprite.texture == null)
                return false;

            Rect rect = sprite.textureRect;
            int x = Mathf.Max(0, Mathf.RoundToInt(rect.x));
            int y = Mathf.Max(0, Mathf.RoundToInt(rect.y));
            int width = Mathf.Max(1, Mathf.RoundToInt(rect.width));
            int height = Mathf.Max(1, Mathf.RoundToInt(rect.height));

            Color[] pixels = null;
            try
            {
                pixels = sprite.texture.GetPixels(x, y, width, height);
            }
            catch
            {
                pixels = TryReadSpritePixelsViaRenderTexture(sprite, x, y, width, height);
            }

            if (pixels == null || pixels.Length == 0)
                return false;

            ApplyTint(pixels, tint);
            spriteData = new SpritePixelData
            {
                Pixels = pixels,
                Width = width,
                Height = height,
                IsValid = true
            };

            return true;
        }

        private static void StampSprite(Color[] canvas, int canvasWidth, int tileX, int tileY, int pixelsPerTile, SpritePixelData spriteData)
        {
            int startX = tileX * pixelsPerTile;
            int startY = tileY * pixelsPerTile;

            for (int py = 0; py < pixelsPerTile; py++)
            {
                int canvasY = startY + py;
                int sourceY = py * spriteData.Height / pixelsPerTile;
                for (int px = 0; px < pixelsPerTile; px++)
                {
                    int canvasX = startX + px;
                    int sourceX = px * spriteData.Width / pixelsPerTile;
                    Color source = spriteData.Pixels[sourceY * spriteData.Width + sourceX];
                    if (source.a < 0.05f)
                        continue;

                    int index = canvasY * canvasWidth + canvasX;
                    canvas[index] = source.a >= 0.95f ? source : AlphaBlend(canvas[index], source);
                }
            }
        }

        private static void StampSpriteOverlay(
            Color[] canvas,
            int canvasWidth,
            int canvasHeight,
            int tileX,
            int tileY,
            int pixelsPerTile,
            SpritePixelData spriteData,
            float overlayScale,
            float alphaScale,
            bool anchorToBottom)
        {
            int targetHeight = Mathf.Max(1, Mathf.RoundToInt(pixelsPerTile * Mathf.Max(overlayScale, 0.1f)));
            int targetWidth = Mathf.Max(1, Mathf.RoundToInt(targetHeight * (spriteData.Width / (float)Mathf.Max(1, spriteData.Height))));

            int startX = tileX * pixelsPerTile + (pixelsPerTile - targetWidth) / 2;
            int startY = anchorToBottom
                ? tileY * pixelsPerTile + pixelsPerTile - targetHeight
                : tileY * pixelsPerTile + (pixelsPerTile - targetHeight) / 2;

            for (int py = 0; py < targetHeight; py++)
            {
                int canvasY = startY + py;
                if (canvasY < 0 || canvasY >= canvasHeight)
                    continue;

                int sourceY = py * spriteData.Height / targetHeight;
                for (int px = 0; px < targetWidth; px++)
                {
                    int canvasX = startX + px;
                    if (canvasX < 0 || canvasX >= canvasWidth)
                        continue;

                    int sourceX = px * spriteData.Width / targetWidth;
                    Color source = spriteData.Pixels[sourceY * spriteData.Width + sourceX];
                    source.a *= alphaScale;
                    if (source.a < 0.05f)
                        continue;

                    int index = canvasY * canvasWidth + canvasX;
                    canvas[index] = source.a >= 0.95f ? source : AlphaBlend(canvas[index], source);
                }
            }
        }

        private static void StampDot(Color[] canvas, int canvasWidth, int canvasHeight, int tileX, int tileY, int pixelsPerTile, Color color)
        {
            int startX = tileX * pixelsPerTile;
            int startY = tileY * pixelsPerTile;
            float center = (pixelsPerTile - 1) * 0.5f;
            float radius = pixelsPerTile * 0.35f;
            float radiusSquared = radius * radius;

            for (int py = 0; py < pixelsPerTile; py++)
            {
                int canvasY = startY + py;
                if (canvasY >= canvasHeight)
                    break;

                for (int px = 0; px < pixelsPerTile; px++)
                {
                    int canvasX = startX + px;
                    if (canvasX >= canvasWidth)
                        break;

                    float deltaX = px - center;
                    float deltaY = py - center;
                    if (deltaX * deltaX + deltaY * deltaY > radiusSquared)
                        continue;

                    int index = canvasY * canvasWidth + canvasX;
                    canvas[index] = AlphaBlend(canvas[index], color);
                }
            }
        }

        private static void FillTileRect(Color[] canvas, int canvasWidth, int tileX, int tileY, int pixelsPerTile, Color color)
        {
            int startX = tileX * pixelsPerTile;
            int startY = tileY * pixelsPerTile;
            for (int py = 0; py < pixelsPerTile; py++)
            {
                int rowStart = (startY + py) * canvasWidth + startX;
                for (int px = 0; px < pixelsPerTile; px++)
                    canvas[rowStart + px] = color;
            }
        }

        private static Color AlphaBlend(Color destination, Color source)
        {
            float sourceAlpha = source.a;
            float destinationAlpha = destination.a * (1f - sourceAlpha);
            float outputAlpha = sourceAlpha + destinationAlpha;
            if (outputAlpha < 0.001f)
                return Color.clear;

            return new Color(
                (source.r * sourceAlpha + destination.r * destinationAlpha) / outputAlpha,
                (source.g * sourceAlpha + destination.g * destinationAlpha) / outputAlpha,
                (source.b * sourceAlpha + destination.b * destinationAlpha) / outputAlpha,
                outputAlpha);
        }

        private static void ApplyTint(Color[] pixels, Color tint)
        {
            if (pixels == null)
                return;

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] *= tint;
        }

        private static Color[] TryReadSpritePixelsViaRenderTexture(Sprite sprite, int x, int y, int width, int height)
        {
            RenderTexture temporary = null;
            RenderTexture previous = RenderTexture.active;
            Texture2D readable = null;

            try
            {
                int textureWidth = Mathf.Max(1, sprite.texture.width);
                int textureHeight = Mathf.Max(1, sprite.texture.height);
                temporary = RenderTexture.GetTemporary(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                Graphics.Blit(sprite.texture, temporary);

                readable = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
                RenderTexture.active = temporary;
                readable.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0, false);
                readable.Apply(false, false);

                int safeX = Mathf.Clamp(x, 0, textureWidth - 1);
                int safeY = Mathf.Clamp(y, 0, textureHeight - 1);
                int safeW = Mathf.Clamp(width, 1, textureWidth - safeX);
                int safeH = Mathf.Clamp(height, 1, textureHeight - safeY);
                return readable.GetPixels(safeX, safeY, safeW, safeH);
            }
            catch
            {
                return null;
            }
            finally
            {
                RenderTexture.active = previous;
                if (readable != null)
                    Object.Destroy(readable);
                if (temporary != null)
                    RenderTexture.ReleaseTemporary(temporary);
            }
        }

        private static string NormalizeId(string id)
        {
            return string.IsNullOrWhiteSpace(id) ? string.Empty : id.Trim();
        }

        private static bool TryResolveSpriteData(Dictionary<string, SpritePixelData> cache, string id, out SpritePixelData spriteData)
        {
            spriteData = default;
            if (cache == null || cache.Count == 0 || string.IsNullOrWhiteSpace(id))
                return false;

            if (cache.TryGetValue(id, out spriteData))
                return true;

            string baseId = GetBaseId(id);
            if (!string.IsNullOrEmpty(baseId) && cache.TryGetValue(baseId, out spriteData))
                return true;

            foreach (var pair in cache)
            {
                if (pair.Key.StartsWith(baseId + "-", System.StringComparison.OrdinalIgnoreCase))
                {
                    spriteData = pair.Value;
                    return true;
                }
            }

            return false;
        }

        private static string GetBaseId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return string.Empty;

            string normalized = id.Trim();
            int separator = normalized.IndexOf('-');
            return separator > 0 ? normalized.Substring(0, separator) : normalized;
        }

        private static Color HashColor(string id, float alpha)
        {
            unchecked
            {
                int hash = 23;
                for (int i = 0; i < id.Length; i++)
                    hash = hash * 31 + id[i];

                float r = ((hash >> 16) & 0xFF) / 255f;
                float g = ((hash >> 8) & 0xFF) / 255f;
                float b = (hash & 0xFF) / 255f;
                var color = Color.Lerp(new Color(0.25f, 0.35f, 0.25f, 1f), new Color(0.95f, 0.9f, 0.55f, 1f), 0.65f);
                color.r = Mathf.Lerp(color.r, r, 0.45f);
                color.g = Mathf.Lerp(color.g, g, 0.45f);
                color.b = Mathf.Lerp(color.b, b, 0.45f);
                color.a = Mathf.Clamp01(alpha);
                return color;
            }
        }

        private struct SpritePixelData
        {
            public Color[] Pixels;
            public int Width;
            public int Height;
            public bool IsValid;
        }
    }
}