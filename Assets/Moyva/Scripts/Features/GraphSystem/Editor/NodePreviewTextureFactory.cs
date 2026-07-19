using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Editor.Shared;
using Kruty1918.Moyva.Generator.Runtime.ObjectPlacement;
using Kruty1918.Moyva.GraphSystem.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.GraphSystem.Editor
{
    internal static class NodePreviewTextureFactory
    {
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
            bool heatmap = false,
            GraphSharedSettings sharedSettings = null)
        {
            ownsTexture = false;
            status = "No map output";

            if (outputs == null || outputs.Length == 0)
            {
                status = "No outputs";
                return null;
            }

            string scalarStatus = null;
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

                if (output is ScatterMask scatterMask)
                {
                    status =
                        $"Scatter mask • {scatterMask.Width}x{scatterMask.Height} • " +
                        $"{CountAllowed(scatterMask)} active • 1 px/tile";
                    ownsTexture = true;
                    return BuildScatterMaskTexture(scatterMask);
                }

                if (output is IReadOnlyList<ScatterCandidate> candidates)
                {
                    int width = Mathf.Max(1, requestedWidth);
                    int height = Mathf.Max(1, requestedHeight);
                    status =
                        $"Candidates • {width}x{height} • {candidates.Count} placed • 1 px/tile";
                    ownsTexture = true;
                    return BuildCandidateTexture(candidates, width, height);
                }

                if (output is ObjectPlacementLayer objectLayer)
                {
                    int width = Mathf.Max(1, requestedWidth);
                    int height = Mathf.Max(1, requestedHeight);
                    status =
                        $"Object layer • {width}x{height} • {objectLayer.Candidates.Count} placed • 1 px/tile";
                    ownsTexture = true;
                    return BuildCandidateTexture(objectLayer.Candidates, width, height);
                }

                if (output is float[,] floatMap)
                {
                    status = BuildLogicalStatus("Height/float map", floatMap);
                    ownsTexture = true;
                    return BuildFloatTexture(floatMap, requestedWidth, requestedHeight, heatmap);
                }

                if (output is string[,] tileMap)
                {
                    bool hasSprites = tileRegistry != null;
                    status = BuildLogicalStatus(hasSprites ? "Tile map (colors)" : "Tile/biome map", tileMap);
                    ownsTexture = true;
                    return hasSprites
                        ? BuildSpriteColorTexture(tileMap, requestedWidth, requestedHeight, tileRegistry)
                        : BuildStringTexture(tileMap, requestedWidth, requestedHeight);
                }

                if (output is bool[,] maskMap)
                {
                    status = BuildLogicalStatus("Mask map", maskMap);
                    ownsTexture = true;
                    return BuildBoolTexture(maskMap, requestedWidth, requestedHeight, heatmap);
                }

                if (output is int[,] intMap)
                {
                    status = BuildLogicalStatus("Int map", intMap);
                    ownsTexture = true;
                    return BuildIntTexture(intMap, requestedWidth, requestedHeight, heatmap);
                }

                scalarStatus ??= TryDescribeScalar(output);
            }

            status = scalarStatus ?? "Unsupported output type";
            return null;
        }

        private static Texture2D BuildProjectedFloatTexture(float[,] source, int requestedWidth, int requestedHeight, bool heatmap, GraphSharedSettings sharedSettings)
        {
            int width = source.GetLength(0);
            int height = source.GetLength(1);
            return ProjectedMapPreviewRenderer.Render(width, height, requestedWidth, requestedHeight, sharedSettings,
                (x, y) =>
                {
                    float t = Mathf.Clamp01(source[x, y]);
                    return heatmap ? EvaluateHeatColor(t) : new Color(t, t, t, 1f);
                });
        }

        private static Texture2D BuildProjectedBoolTexture(bool[,] source, int requestedWidth, int requestedHeight, bool heatmap, GraphSharedSettings sharedSettings)
        {
            int width = source.GetLength(0);
            int height = source.GetLength(1);
            Color falseColor = heatmap
                ? new Color(0.08f, 0.10f, 0.30f, 1f)
                : new Color(0.10f, 0.12f, 0.16f, 1f);
            Color trueColor = heatmap
                ? new Color(1f, 0.2f, 0.2f, 1f)
                : new Color(0.38f, 0.86f, 0.48f, 1f);

            return ProjectedMapPreviewRenderer.Render(width, height, requestedWidth, requestedHeight, sharedSettings,
                (x, y) => source[x, y] ? trueColor : falseColor);
        }

        private static Texture2D BuildProjectedIntTexture(int[,] source, int requestedWidth, int requestedHeight, bool heatmap, GraphSharedSettings sharedSettings)
        {
            int width = source.GetLength(0);
            int height = source.GetLength(1);
            int min = int.MaxValue;
            int max = int.MinValue;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int value = source[x, y];
                    if (value < min) min = value;
                    if (value > max) max = value;
                }
            }

            float span = Mathf.Max(1f, max - min);
            return ProjectedMapPreviewRenderer.Render(width, height, requestedWidth, requestedHeight, sharedSettings,
                (x, y) =>
                {
                    float t = Mathf.Clamp01((source[x, y] - min) / span);
                    return heatmap ? EvaluateHeatColor(t) : new Color(t, t, t, 1f);
                });
        }

        private static Texture2D BuildProjectedStringTexture(string[,] source, int requestedWidth, int requestedHeight, GraphSharedSettings sharedSettings)
        {
            int width = source.GetLength(0);
            int height = source.GetLength(1);
            return ProjectedMapPreviewRenderer.Render(width, height, requestedWidth, requestedHeight, sharedSettings,
                (x, y) => StringToColor(source[x, y]));
        }

        private static Texture2D BuildProjectedSpriteColorTexture(string[,] source, int requestedWidth, int requestedHeight, TileRegistrySO registry, GraphSharedSettings sharedSettings)
        {
            EnsureSpriteColorCache(registry);

            int width = source.GetLength(0);
            int height = source.GetLength(1);
            return ProjectedMapPreviewRenderer.Render(width, height, requestedWidth, requestedHeight, sharedSettings,
                (x, y) =>
                {
                    string tileId = source[x, y];
                    if (string.IsNullOrEmpty(tileId))
                        return Color.black;

                    if (_spriteColorCache != null && _spriteColorCache.TryGetValue(tileId, out var color))
                        return color;

                    return StringToColor(tileId);
                });
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
            Color falseColor = heatmap
                ? new Color(0.08f, 0.10f, 0.30f, 1f)
                : new Color(0.10f, 0.12f, 0.16f, 1f);
            Color trueColor = heatmap
                ? new Color(1f, 0.2f, 0.2f, 1f)
                : new Color(0.38f, 0.86f, 0.48f, 1f);
            for (int y = 0; y < th; y++)
            {
                int sy = y * sh / th;
                for (int x = 0; x < tw; x++)
                {
                    int sx = x * sw / tw;
                    tex.SetPixel(x, y, source[sx, sy] ? trueColor : falseColor);
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

            GetSize(source, requestedWidth, requestedHeight, out var tw, out var th, out _, out _);

            var tex = CreateTexture(tw, th);
            for (int y = 0; y < th; y++)
            {
                for (int x = 0; x < tw; x++)
                {
                    var tileId = source[x, y];

                    Color c;
                    if (string.IsNullOrEmpty(tileId))
                    {
                        c = Color.black;
                    }
                    else if (_spriteColorCache != null && _spriteColorCache.TryGetValue(tileId, out c))
                    {
                        // One logical pixel represents the tile, so use its averaged color.
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

        private static Texture2D BuildScatterMaskTexture(ScatterMask mask)
        {
            int width = Mathf.Max(1, mask?.Width ?? 0);
            int height = Mathf.Max(1, mask?.Height ?? 0);
            var texture = CreateTexture(width, height);
            var off = new Color(0.035f, 0.04f, 0.055f, 1f);
            var on = new Color(0.70f, 0.92f, 0.48f, 1f);

            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float weight = mask?.GetWeight(x, y) ?? 0f;
                texture.SetPixel(x, y, Color.Lerp(off, on, weight));
            }

            texture.Apply(false, false);
            return texture;
        }

        private static Texture2D BuildCandidateTexture(
            IReadOnlyList<ScatterCandidate> candidates,
            int width,
            int height)
        {
            var texture = CreateTexture(width, height);
            var background = new Color(0.035f, 0.04f, 0.055f, 1f);
            var point = new Color(0.78f, 0.95f, 0.45f, 1f);
            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = background;

            if (candidates != null)
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    var cell = candidates[i].Cell;
                    if (cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height)
                        pixels[cell.y * width + cell.x] = point;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            return texture;
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

                if (!AdaptivePrefabPreviewUtility.TryGetPrimarySprite(def.VisualPrefab, out var sprite, out var tint))
                    continue;

                if (AdaptivePrefabPreviewUtility.TryGetSpritePixels(sprite, tint, StringToColor(def.Id), out var spriteData))
                {
                    _spriteColorCache[def.Id] = spriteData.AverageOpaqueColor(tint);
                    _spritePxCache[def.Id] = (spriteData.Pixels, spriteData.Width, spriteData.Height);
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

        private static void ApplyTint(Color[] pixels, Color tint)
        {
            if (pixels == null || pixels.Length == 0)
                return;

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] *= tint;
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

            // Logical node previews are never resampled: one texture pixel is one map cell.
            targetWidth = Mathf.Max(1, sourceWidth);
            targetHeight = Mathf.Max(1, sourceHeight);
        }

        private static string BuildLogicalStatus(string label, Array source)
        {
            return source == null || source.Rank != 2
                ? label
                : $"{label} • {source.GetLength(0)}x{source.GetLength(1)} • " +
                  $"{CountActive(source)} active • 1 px/tile";
        }

        private static int CountActive(Array source)
        {
            if (source == null || source.Rank != 2)
                return 0;

            int count = 0;
            int width = source.GetLength(0);
            int height = source.GetLength(1);
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                object value = source.GetValue(x, y);
                bool active = value switch
                {
                    bool boolean => boolean,
                    float number => !float.IsNaN(number) && !float.IsInfinity(number)
                                    && !Mathf.Approximately(number, 0f),
                    int number => number != 0,
                    string text => !string.IsNullOrEmpty(text),
                    _ => value != null
                };
                if (active)
                    count++;
            }

            return count;
        }

        private static int CountAllowed(ScatterMask mask)
        {
            if (mask == null)
                return 0;

            int count = 0;
            for (int x = 0; x < mask.Width; x++)
            for (int y = 0; y < mask.Height; y++)
            {
                if (mask.IsAllowed(x, y))
                    count++;
            }

            return count;
        }

        private static string TryDescribeScalar(object value)
        {
            return value switch
            {
                bool boolean => $"Value • {boolean}",
                byte or sbyte or short or ushort or int or uint or long or ulong =>
                    $"Value • {value}",
                float number => $"Value • {number:0.###}",
                double number => $"Value • {number:0.###}",
                decimal number => $"Value • {number:0.###}",
                string text => $"Value • {text}",
                Enum enumValue => $"Value • {enumValue}",
                Vector2 vector => $"Value • ({vector.x:0.###}, {vector.y:0.###})",
                Vector3 vector =>
                    $"Value • ({vector.x:0.###}, {vector.y:0.###}, {vector.z:0.###})",
                Vector2Int vector => $"Value • ({vector.x}, {vector.y})",
                Vector3Int vector => $"Value • ({vector.x}, {vector.y}, {vector.z})",
                _ => null
            };
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
