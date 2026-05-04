using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    /// <summary>
    /// Будує композитну Texture2D, яка відтворює фінальний вигляд згенерованої карти:
    /// terrain-шари (WorldLayerData) → об'єкти (ObjectMap) → будівлі (BuildingMap).
    /// Використовує реальні спрайти з реєстрів та CPU-based alpha blending.
    /// </summary>
    internal static class CompositePreviewBuilder
    {
        private const int DefaultPixelsPerTile = 4;

        /// <summary>
        /// Побудувати композитну текстуру.
        /// </summary>
        /// <param name="layers">Шари з WorldLayerData (заповнюються SingleTileLayerNode).</param>
        /// <param name="biomeMap">Карта біомів (string[,]).</param>
        /// <param name="objectMap">Карта об'єктів (string[,]).</param>
        /// <param name="heightMap">Карта висот (float[,]).</param>
        /// <param name="buildingMap">Карта будівель (string[,]).</param>
        /// <param name="tileRegistry">Реєстр тайлів (для фону якщо немає шарів).</param>
        /// <param name="objectRegistry">Реєстр об'єктів карти.</param>
        /// <param name="buildingRegistry">Реєстр будівель.</param>
        /// <param name="pixelsPerTile">Піксеоів на тайл (більше = деталізованіше, але повільніше).</param>
        public static Texture2D Build(
            List<WorldLayerData> layers,
            string[,] biomeMap,
            string[,] objectMap,
            float[,] heightMap,
            string[,] buildingMap,
            TileRegistrySO tileRegistry,
            MapObjectRegistrySO objectRegistry,
            BuildingRegistrySO buildingRegistry,
            int pixelsPerTile = DefaultPixelsPerTile)
        {
            // Визначаємо розмір карти
            int mapW = 0, mapH = 0;

            if (biomeMap != null)
            {
                mapW = biomeMap.GetLength(0);
                mapH = biomeMap.GetLength(1);
            }
            else if (heightMap != null)
            {
                mapW = heightMap.GetLength(0);
                mapH = heightMap.GetLength(1);
            }
            else if (objectMap != null)
            {
                mapW = objectMap.GetLength(0);
                mapH = objectMap.GetLength(1);
            }

            if (mapW <= 0 || mapH <= 0) return null;

            int ppt = Mathf.Max(1, pixelsPerTile);
            int texW = mapW * ppt;
            int texH = mapH * ppt;

            // Обмеження: максимум 1024×1024 для превью
            if (texW > 1024 || texH > 1024)
            {
                float scale = 1024f / Mathf.Max(texW, texH);
                ppt = Mathf.Max(1, Mathf.FloorToInt(ppt * scale));
                texW = mapW * ppt;
                texH = mapH * ppt;
            }

            var canvas = new Color[texW * texH];

            // ════════════════════════════════════════════════
            // 1) Terrain Layers (знизу вверх за SortingOrder)
            // ════════════════════════════════════════════════
            bool anyLayerDrawn = false;
            if (layers != null && layers.Count > 0)
            {
                // Сортуємо шари за SortingOrder (нижчий = раніше малюється)
                var sorted = new List<WorldLayerData>(layers);
                sorted.Sort((a, b) => a.SortingOrder.CompareTo(b.SortingOrder));

                foreach (var layer in sorted)
                {
                    if (layer.TileTexture == null) continue;
                    if (BlitLayerTexture(canvas, texW, texH, layer.TileTexture, mapW, mapH, ppt))
                        anyLayerDrawn = true;
                }
            }

            // Якщо шари існують, але їх не вдалося прочитати (Read/Write off),
            // не лишаємо canvas чорним — підставляємо fallback з biome/height.
            if (!anyLayerDrawn && biomeMap != null && tileRegistry != null)
            {
                // Фолбек: якщо текстурні шари недоступні, малюємо реальні спрайти тайлів із TileRegistry.
                var spriteCache = BuildTileSpriteCache(tileRegistry);
                for (int y = 0; y < mapH; y++)
                {
                    for (int x = 0; x < mapW; x++)
                    {
                        var tileId = biomeMap[x, y];
                        if (string.IsNullOrEmpty(tileId))
                        {
                            FillTileRect(canvas, texW, x, y, ppt, Color.black);
                            continue;
                        }

                        if (spriteCache.TryGetValue(tileId, out var spriteData))
                            StampSprite(canvas, texW, texH, x, y, ppt, spriteData);
                        else
                            FillTileRect(canvas, texW, x, y, ppt, HashColor(tileId));
                    }
                }
            }
            else if (!anyLayerDrawn && heightMap != null)
            {
                // Фолбек: greyscale heightmap
                float min = float.MaxValue, max = float.MinValue;
                for (int y = 0; y < mapH; y++)
                    for (int x = 0; x < mapW; x++)
                    {
                        float v = heightMap[x, y];
                        if (v < min) min = v;
                        if (v > max) max = v;
                    }
                float span = Mathf.Max(0.0001f, max - min);
                for (int y = 0; y < mapH; y++)
                    for (int x = 0; x < mapW; x++)
                    {
                        float t = Mathf.Clamp01((heightMap[x, y] - min) / span);
                        FillTileRect(canvas, texW, x, y, ppt, new Color(t, t, t, 1f));
                    }
            }

            // ════════════════════════════════════════════════
            // 2) Objects (дерева, каміння, річки тощо)
            // ════════════════════════════════════════════════
            if (objectMap != null)
            {
                var objSpriteCache = objectRegistry != null
                    ? BuildObjectSpriteCache(objectRegistry)
                    : null;

                for (int y = 0; y < mapH; y++)
                {
                    for (int x = 0; x < mapW; x++)
                    {
                        var objId = objectMap[x, y];
                        if (string.IsNullOrEmpty(objId)) continue;

                        if (objSpriteCache != null && objSpriteCache.TryGetValue(objId, out var spriteData))
                            StampSprite(canvas, texW, texH, x, y, ppt, spriteData);
                        else
                            StampDot(canvas, texW, texH, x, y, ppt, HashColor(objId));
                    }
                }
            }

            // ════════════════════════════════════════════════
            // 3) Buildings (будівлі)
            // ════════════════════════════════════════════════
            if (buildingMap != null && buildingRegistry != null)
            {
                int bw = buildingMap.GetLength(0);
                int bh = buildingMap.GetLength(1);
                for (int y = 0; y < bh; y++)
                {
                    for (int x = 0; x < bw; x++)
                    {
                        var bldId = buildingMap[x, y];
                        if (string.IsNullOrEmpty(bldId)) continue;

                        var def = buildingRegistry.GetById(bldId);
                        if (def == null)
                        {
                            StampDot(canvas, texW, texH, x, y, ppt, HashColor(bldId));
                            continue;
                        }

                        // Пріоритет: Icon (Sprite), потім Prefab → SpriteRenderer
                        var sprite = def.Icon;
                        if (sprite == null && def.Prefab != null)
                        {
                            var sr = def.Prefab.GetComponentInChildren<SpriteRenderer>(true);
                            if (sr != null) sprite = sr.sprite;
                        }

                        if (sprite != null)
                            StampSprite(canvas, texW, texH, x, y, ppt, GetSpritePixels(sprite, HashColor(bldId)));
                        else
                            StampDot(canvas, texW, texH, x, y, ppt, new Color(0.9f, 0.6f, 0.2f, 1f));
                    }
                }
            }

            // ════════════════════════════════════════════════
            // Фінал: створити текстуру
            // ════════════════════════════════════════════════
            var tex = new Texture2D(texW, texH, TextureFormat.RGBA32, false)
            {
                filterMode = ppt <= 2 ? FilterMode.Point : FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            tex.SetPixels(canvas);
            tex.Apply(false, false);
            return tex;
        }

        // ── Layer Blitting ──────────────────────────────────────────────────

        /// <summary>
        /// Blitить текстуру шару (1px = 1 tile) на canvas з alpha blending.
        /// </summary>
        private static bool BlitLayerTexture(Color[] canvas, int canvasW, int canvasH,
            Texture2D layerTex, int mapW, int mapH, int ppt)
        {
            int srcW = layerTex.width;
            int srcH = layerTex.height;

            Color[] srcPixels;
            try { srcPixels = layerTex.GetPixels(); }
            catch { return false; } // texture not readable

            for (int cy = 0; cy < canvasH; cy++)
            {
                // Яку tile-координату покривають ці пікселі
                int tileY = cy / ppt;
                // Зіставляємо tile → srcPixel
                int srcY = Mathf.Clamp(tileY * srcH / mapH, 0, srcH - 1);

                for (int cx = 0; cx < canvasW; cx++)
                {
                    int tileX = cx / ppt;
                    int srcX = Mathf.Clamp(tileX * srcW / mapW, 0, srcW - 1);

                    Color src = srcPixels[srcY * srcW + srcX];
                    if (src.a < 0.01f) continue; // повністю прозорий — пропускаємо

                    int idx = cy * canvasW + cx;
                    if (src.a >= 0.99f)
                        canvas[idx] = src;
                    else
                        canvas[idx] = AlphaBlend(canvas[idx], src);
                }
            }

            return true;
        }

        // ── Object Sprite Stamping ──────────────────────────────────────────

        private struct SpritePixelData
        {
            public Color[] Pixels;
            public int Width;
            public int Height;
        }

        private static Dictionary<string, SpritePixelData> BuildObjectSpriteCache(MapObjectRegistrySO registry)
        {
            var cache = new Dictionary<string, SpritePixelData>();
            if (registry?.Definitions == null) return cache;

            foreach (var def in registry.Definitions)
            {
                if (def == null || string.IsNullOrEmpty(def.Id)) continue;
                if (def.VisualPrefab == null) continue;

                var sr = def.VisualPrefab.GetComponentInChildren<SpriteRenderer>(true);
                if (sr == null || sr.sprite == null) continue;

                cache[def.Id] = GetSpritePixels(sr.sprite, sr.color);
            }
            return cache;
        }

        private static SpritePixelData GetSpritePixels(Sprite sprite, Color fallbackColor)
        {
            try
            {
                var rect = sprite.textureRect;
                int x = (int)rect.x, y = (int)rect.y;
                int w = Mathf.Max(1, (int)rect.width);
                int h = Mathf.Max(1, (int)rect.height);
                return new SpritePixelData
                {
                    Pixels = sprite.texture.GetPixels(x, y, w, h),
                    Width = w,
                    Height = h
                };
            }
            catch
            {
                // Texture not readable — використовуємо стабільний fallback колір,
                // а не magenta, щоб превью не виглядало «shader error»-фіолетовим.
                var c = fallbackColor;
                c.a = Mathf.Max(0.35f, c.a);
                return new SpritePixelData
                {
                    Pixels = new[] { c },
                    Width = 1,
                    Height = 1
                };
            }
        }

        /// <summary>
        /// Штампує спрайт об'єкта в межах тайлу (x,y) з alpha blending.
        /// </summary>
        private static void StampSprite(Color[] canvas, int canvasW, int canvasH,
            int tileX, int tileY, int ppt, SpritePixelData spriteData)
        {
            int startX = tileX * ppt;
            int startY = tileY * ppt;

            for (int py = 0; py < ppt; py++)
            {
                int cy = startY + py;
                if (cy >= canvasH) break;
                // Зіставляємо piY → sprite pixel
                int srcY = py * spriteData.Height / ppt;

                for (int px = 0; px < ppt; px++)
                {
                    int cx = startX + px;
                    if (cx >= canvasW) break;
                    int srcX = px * spriteData.Width / ppt;

                    Color src = spriteData.Pixels[srcY * spriteData.Width + srcX];
                    if (src.a < 0.05f) continue;

                    int idx = cy * canvasW + cx;
                    canvas[idx] = src.a >= 0.95f ? src : AlphaBlend(canvas[idx], src);
                }
            }
        }

        /// <summary>
        /// Штампує кольорову крапку (коло) по центру тайлу — фолбек коли немає спрайту.
        /// </summary>
        private static void StampDot(Color[] canvas, int canvasW, int canvasH,
            int tileX, int tileY, int ppt, Color color)
        {
            int startX = tileX * ppt;
            int startY = tileY * ppt;
            float center = (ppt - 1) * 0.5f;
            float radius = ppt * 0.35f;
            float r2 = radius * radius;

            for (int py = 0; py < ppt; py++)
            {
                int cy = startY + py;
                if (cy >= canvasH) break;
                for (int px = 0; px < ppt; px++)
                {
                    int cx = startX + px;
                    if (cx >= canvasW) break;

                    float dx = px - center;
                    float dy = py - center;
                    if (dx * dx + dy * dy > r2) continue;

                    int idx = cy * canvasW + cx;
                    canvas[idx] = AlphaBlend(canvas[idx], color);
                }
            }
        }

        // ── Tile Color Helpers ──────────────────────────────────────────────

        private static void FillTileRect(Color[] canvas, int canvasW,
            int tileX, int tileY, int ppt, Color color)
        {
            int startX = tileX * ppt;
            int startY = tileY * ppt;
            for (int py = 0; py < ppt; py++)
            {
                int rowStart = (startY + py) * canvasW + startX;
                for (int px = 0; px < ppt; px++)
                    canvas[rowStart + px] = color;
            }
        }

        private static Dictionary<string, SpritePixelData> BuildTileSpriteCache(TileRegistrySO registry)
        {
            var cache = new Dictionary<string, SpritePixelData>();
            if (registry?.Definitions == null) return cache;

            foreach (var def in registry.Definitions)
            {
                if (def == null || string.IsNullOrEmpty(def.Id)) continue;
                if (def.VisualPrefab == null) continue;

                var sr = def.VisualPrefab.GetComponentInChildren<SpriteRenderer>(true);
                if (sr == null || sr.sprite == null) continue;

                cache[def.Id] = GetSpritePixels(sr.sprite, sr.color);
            }

            return cache;
        }

        // ── Utility ─────────────────────────────────────────────────────────

        private static Color AlphaBlend(Color dst, Color src)
        {
            float sa = src.a;
            float da = dst.a * (1f - sa);
            float oa = sa + da;
            if (oa < 0.001f) return Color.clear;
            return new Color(
                (src.r * sa + dst.r * da) / oa,
                (src.g * sa + dst.g * da) / oa,
                (src.b * sa + dst.b * da) / oa,
                oa);
        }

        private static Color AverageOpaqueColor(Color[] pixels)
        {
            float r = 0, g = 0, b = 0;
            int count = 0;
            foreach (var c in pixels)
            {
                if (c.a < 0.01f) continue;
                r += c.r; g += c.g; b += c.b;
                count++;
            }
            if (count == 0) return Color.white;
            float n = count;
            return new Color(r / n, g / n, b / n, 1f);
        }

        private static Color HashColor(string value)
        {
            if (string.IsNullOrEmpty(value)) return Color.black;
            uint hash = 2166136261;
            for (int i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= 16777619;
            }
            float hue = (hash % 360u) / 360f;
            var c = Color.HSVToRGB(hue, 0.75f, 0.95f);
            c.a = 1f;
            return c;
        }
    }
}
