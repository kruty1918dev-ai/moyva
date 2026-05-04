using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    internal static class NodePreviewTextureFactory
    {
        private const int DefaultSize = 128;

        /// <summary>
        /// Кеш усередненого кольору спрайта за tile ID (fallback коли текстура нечитаєма).
        /// </summary>
        private static Dictionary<string, Color> _spriteColorCache;
        /// <summary>
        /// Кеш реальних піксельних даних спрайта за tile ID.
        /// </summary>
        private static Dictionary<string, (Color[] px, int w, int h)> _spritePxCache;
        private static TileRegistrySO _cachedRegistry;

        public static Texture2D TryBuild(
            object[] outputs,
            int requestedWidth,
            int requestedHeight,
            out bool ownsTexture,
            out string status,
            TileRegistrySO tileRegistry = null,
            bool heatmap = false)
        {
            ownsTexture = false;
            status = "No map output";

            if (outputs == null || outputs.Length == 0)
            {
                status = "No outputs";
                return null;
            }

            for (int i = 0; i < outputs.Length; i++)
            {
                var output = outputs[i];
                if (output == null) continue;

                if (output is Texture2D tex)
                {
                    status = "Texture output";
                    ownsTexture = false;
                    return tex;
                }

                if (output is float[,] floatMap)
                {
                    status = "Height/float map";
                    ownsTexture = true;
                    return BuildFloatTexture(floatMap, requestedWidth, requestedHeight, heatmap);
                }

                if (output is string[,] tileMap)
                {
                    bool hasSprites = tileRegistry != null;
                    status = hasSprites ? "Tile map (sprites)" : "Tile/biome map";
                    ownsTexture = true;
                    return hasSprites
                        ? BuildSpriteColorTexture(tileMap, requestedWidth, requestedHeight, tileRegistry)
                        : BuildStringTexture(tileMap, requestedWidth, requestedHeight);
                }

                if (output is bool[,] maskMap)
                {
                    status = "Mask map";
                    ownsTexture = true;
                    return BuildBoolTexture(maskMap, requestedWidth, requestedHeight, heatmap);
                }

                if (output is int[,] intMap)
                {
                    status = "Int map";
                    ownsTexture = true;
                    return BuildIntTexture(intMap, requestedWidth, requestedHeight, heatmap);
                }
            }

            status = "Unsupported output type";
            return null;
        }

        private static Texture2D BuildFloatTexture(float[,] source, int requestedWidth, int requestedHeight, bool heatmap)
        {
            GetSize(source, requestedWidth, requestedHeight, out var tw, out var th, out var sw, out var sh);

            var tex = CreateTexture(tw, th);
            for (int y = 0; y < th; y++)
            {
                int sy = y * sh / th;
                for (int x = 0; x < tw; x++)
                {
                    int sx = x * sw / tw;
                    float t = Mathf.Clamp01(source[sx, sy]);
                    tex.SetPixel(x, y, heatmap ? EvaluateHeatColor(t) : new Color(t, t, t, 1f));
                }
            }

            tex.Apply(false, false);
            return tex;
        }

        private static Texture2D BuildBoolTexture(bool[,] source, int requestedWidth, int requestedHeight, bool heatmap)
        {
            GetSize(source, requestedWidth, requestedHeight, out var tw, out var th, out var sw, out var sh);

            var tex = CreateTexture(tw, th);
            for (int y = 0; y < th; y++)
            {
                int sy = y * sh / th;
                for (int x = 0; x < tw; x++)
                {
                    int sx = x * sw / tw;
                    if (heatmap)
                        tex.SetPixel(x, y, source[sx, sy] ? new Color(1f, 0.2f, 0.2f, 1f) : new Color(0.1f, 0.1f, 0.35f, 1f));
                    else
                        tex.SetPixel(x, y, source[sx, sy] ? Color.white : Color.black);
                }
            }

            tex.Apply(false, false);
            return tex;
        }

        private static Texture2D BuildIntTexture(int[,] source, int requestedWidth, int requestedHeight, bool heatmap)
        {
            GetSize(source, requestedWidth, requestedHeight, out var tw, out var th, out var sw, out var sh);

            int min = int.MaxValue;
            int max = int.MinValue;
            for (int x = 0; x < sw; x++)
            {
                for (int y = 0; y < sh; y++)
                {
                    int value = source[x, y];
                    if (value < min) min = value;
                    if (value > max) max = value;
                }
            }

            float span = Mathf.Max(1f, max - min);
            var tex = CreateTexture(tw, th);
            for (int y = 0; y < th; y++)
            {
                int sy = y * sh / th;
                for (int x = 0; x < tw; x++)
                {
                    int sx = x * sw / tw;
                    float t = Mathf.Clamp01((source[sx, sy] - min) / span);
                    tex.SetPixel(x, y, heatmap ? EvaluateHeatColor(t) : new Color(t, t, t, 1f));
                }
            }

            tex.Apply(false, false);
            return tex;
        }

        private static Texture2D BuildStringTexture(string[,] source, int requestedWidth, int requestedHeight)
        {
            GetSize(source, requestedWidth, requestedHeight, out var tw, out var th, out var sw, out var sh);

            var tex = CreateTexture(tw, th);
            for (int y = 0; y < th; y++)
            {
                int sy = y * sh / th;
                for (int x = 0; x < tw; x++)
                {
                    int sx = x * sw / tw;
                    tex.SetPixel(x, y, StringToColor(source[sx, sy]));
                }
            }

            tex.Apply(false, false);
            return tex;
        }

        /// <summary>
        /// Будує текстуру, використовуючи реальні кольори спрайтів тайлів із TileRegistrySO.
        /// Кожен тайл-id map → знаходимо VisualPrefab → SpriteRenderer → усереднений колір.
        /// Якщо спрайт не знайдено — фалбек на hash-based колір.
        /// </summary>
        private static Texture2D BuildSpriteColorTexture(string[,] source, int requestedWidth, int requestedHeight, TileRegistrySO registry)
        {
            EnsureSpriteColorCache(registry);

            GetSize(source, requestedWidth, requestedHeight, out var tw, out var th, out var sw, out var sh);

            var tex = CreateTexture(tw, th);
            for (int y = 0; y < th; y++)
            {
                // Floating-point tile coordinate in source space
                float tileYf = (float)y * sh / th;
                int sy = Mathf.Clamp((int)tileYf, 0, sh - 1);
                // UV within the tile (0..1)
                float vWithin = tileYf - sy;

                for (int x = 0; x < tw; x++)
                {
                    float tileXf = (float)x * sw / tw;
                    int sx = Mathf.Clamp((int)tileXf, 0, sw - 1);
                    float uWithin = tileXf - sx;

                    var tileId = source[sx, sy];

                    Color c;
                    if (string.IsNullOrEmpty(tileId))
                    {
                        c = Color.black;
                    }
                    else if (_spritePxCache != null && _spritePxCache.TryGetValue(tileId, out var pd))
                    {
                        // Sample actual sprite pixel using within-tile UV
                        int spx = Mathf.Clamp((int)(uWithin * pd.w), 0, pd.w - 1);
                        int spy = Mathf.Clamp((int)(vWithin * pd.h), 0, pd.h - 1);
                        c = pd.px[spy * pd.w + spx];
                        // Blend transparent sprite pixels with averaged fallback color
                        if (c.a < 0.05f && _spriteColorCache.TryGetValue(tileId, out var avg))
                            c = avg;
                    }
                    else if (_spriteColorCache != null && _spriteColorCache.TryGetValue(tileId, out c))
                    {
                        // Fallback: averaged color (texture not readable)
                    }
                    else
                    {
                        c = StringToColor(tileId);
                    }

                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply(false, false);
            return tex;
        }

        private static void EnsureSpriteColorCache(TileRegistrySO registry)
        {
            if (registry == _cachedRegistry && _spriteColorCache != null)
                return;

            _cachedRegistry = registry;
            _spriteColorCache = new Dictionary<string, Color>();
            _spritePxCache = new Dictionary<string, (Color[], int, int)>();

            if (registry == null || registry.Definitions == null) return;

            foreach (var def in registry.Definitions)
            {
                if (def == null || string.IsNullOrEmpty(def.Id)) continue;
                if (def.VisualPrefab == null) continue;

                var sr = def.VisualPrefab.GetComponentInChildren<SpriteRenderer>(true);
                if (sr == null || sr.sprite == null) continue;

                var sprite = sr.sprite;
                try
                {
                    var srcRect = sprite.textureRect;
                    int srcX = (int)srcRect.x;
                    int srcY = (int)srcRect.y;
                    int srcW = Mathf.Max(1, (int)srcRect.width);
                    int srcH = Mathf.Max(1, (int)srcRect.height);

                    Color[] pixels = sprite.texture.GetPixels(srcX, srcY, srcW, srcH);
                    _spriteColorCache[def.Id] = AverageColor(pixels);
                    _spritePxCache[def.Id] = (pixels, srcW, srcH);
                }
                catch
                {
                    // Texture not readable — use tint color as fallback
                    _spriteColorCache[def.Id] = sr.color;
                }
            }
        }

        private static Color AverageColor(Color[] pixels)
        {
            if (pixels == null || pixels.Length == 0) return Color.white;

            float r = 0, g = 0, b = 0, a = 0;
            int count = 0;
            foreach (var c in pixels)
            {
                if (c.a < 0.01f) continue; // skip transparent
                r += c.r;
                g += c.g;
                b += c.b;
                a += c.a;
                count++;
            }

            if (count == 0) return Color.white;
            float n = count;
            return new Color(r / n, g / n, b / n, Mathf.Clamp01(a / n));
        }

        private static Texture2D CreateTexture(int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            return tex;
        }

        private static Color EvaluateHeatColor(float t)
        {
            t = Mathf.Clamp01(t);
            if (t < 0.25f) return Color.Lerp(new Color(0f, 0.05f, 0.25f), new Color(0f, 0.45f, 1f), t / 0.25f);
            if (t < 0.5f) return Color.Lerp(new Color(0f, 0.45f, 1f), new Color(0.1f, 0.9f, 0.4f), (t - 0.25f) / 0.25f);
            if (t < 0.75f) return Color.Lerp(new Color(0.1f, 0.9f, 0.4f), new Color(1f, 0.95f, 0.2f), (t - 0.5f) / 0.25f);
            return Color.Lerp(new Color(1f, 0.95f, 0.2f), new Color(1f, 0.2f, 0.1f), (t - 0.75f) / 0.25f);
        }

        private static void GetSize(Array source, int requestedWidth, int requestedHeight,
            out int targetWidth, out int targetHeight, out int sourceWidth, out int sourceHeight)
        {
            sourceWidth = source.GetLength(0);
            sourceHeight = source.GetLength(1);

            targetWidth = Mathf.Clamp(requestedWidth > 0 ? requestedWidth : DefaultSize, 32, 256);
            targetHeight = Mathf.Clamp(requestedHeight > 0 ? requestedHeight : DefaultSize, 32, 256);
        }

        private static Color StringToColor(string value)
        {
            if (string.IsNullOrEmpty(value))
                return Color.black;

            uint hash = 2166136261;
            for (int i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= 16777619;
            }

            float hue = (hash % 360u) / 360f;
            var color = Color.HSVToRGB(hue, 0.75f, 0.95f);
            color.a = 1f;
            return color;
        }
    }
}
