using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using UnityEditor;
using UnityEngine;

namespace Kruty1918.Moyva.Editor.Shared
{
    public readonly struct AdaptiveSpritePixelData
    {
        public AdaptiveSpritePixelData(Color[] pixels, int width, int height)
        {
            Pixels = pixels;
            Width = width;
            Height = height;
        }

        public Color[] Pixels { get; }
        public int Width { get; }
        public int Height { get; }
        public bool IsValid => Pixels != null && Pixels.Length > 0 && Width > 0 && Height > 0;

        public Color AverageOpaqueColor(Color fallback)
        {
            if (!IsValid)
                return fallback;

            float r = 0f;
            float g = 0f;
            float b = 0f;
            int count = 0;
            for (int i = 0; i < Pixels.Length; i++)
            {
                Color pixel = Pixels[i];
                if (pixel.a < 0.01f)
                    continue;

                r += pixel.r;
                g += pixel.g;
                b += pixel.b;
                count++;
            }

            if (count == 0)
                return fallback;

            float scale = 1f / count;
            return new Color(r * scale, g * scale, b * scale, 1f);
        }
    }

    public static class AdaptivePrefabPreviewUtility
    {
        private const double SettingsCacheTtl = 1.0;
        private static MoyvaProjectSettingsSO _cachedSettings;
        private static MoyvaProjectSettingsSO _runtimeFallbackSettings;
        private static double _settingsCacheTime;
        private static readonly Dictionary<string, Texture2D> _meshPreviewTextureCache = new();

        public static MoyvaProjectSettingsSO ProjectSettings
        {
            get
            {
                double now = EditorApplication.timeSinceStartup;
                if (_cachedSettings != null && now - _settingsCacheTime < SettingsCacheTtl)
                    return _cachedSettings;

                _settingsCacheTime = now;
                _cachedSettings = AssetDatabase.LoadAssetAtPath<MoyvaProjectSettingsSO>(MoyvaProjectSettingsSO.DefaultAssetPath);
                if (_cachedSettings != null)
                {
                    _cachedSettings.Normalize();
                    return _cachedSettings;
                }

                _runtimeFallbackSettings ??= MoyvaProjectSettingsSO.CreateRuntimeDefault();
                _runtimeFallbackSettings.Normalize();
                return _runtimeFallbackSettings;
            }
        }

        public static GridProjectionMode ProjectionMode => ProjectSettings.DefaultProjectionMode;
        public static GridRenderMode RenderMode => ProjectSettings.DefaultRenderMode;

        public static bool UsesProjectedGrid(GridProjectionMode projectionMode)
        {
            return projectionMode == GridProjectionMode.Isometric2D
                || projectionMode == GridProjectionMode.Isometric3DPreview
                || projectionMode == GridProjectionMode.HexPointy2D
                || projectionMode == GridProjectionMode.HexFlat2D
                || projectionMode == GridProjectionMode.Orthographic3D;
        }

        public static bool Uses3DPreview(MoyvaProjectSettingsSO settings = null)
        {
            settings ??= ProjectSettings;
            return settings.DefaultProjectionMode == GridProjectionMode.Orthographic3D
                || settings.DefaultProjectionMode == GridProjectionMode.Isometric3DPreview
                || settings.DefaultRenderMode == GridRenderMode.Mesh3D
                || settings.DefaultRenderMode == GridRenderMode.Mesh3DPreview;
        }

        public static string DescribeCurrentMode()
        {
            var settings = ProjectSettings;
            return $"{settings.DefaultProjectionMode} / {settings.DefaultRenderMode}";
        }

        public static bool TryGetPrimarySprite(GameObject prefab, out Sprite sprite, out Color tint)
        {
            sprite = null;
            tint = Color.white;

            var renderer = TryGetPrimarySpriteRenderer(prefab);
            if (renderer == null || renderer.sprite == null)
                return false;

            sprite = renderer.sprite;
            tint = renderer.color;
            return true;
        }

        public static SpriteRenderer TryGetPrimarySpriteRenderer(GameObject prefab)
        {
            if (prefab == null)
                return null;

            var renderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
            SpriteRenderer best = null;
            int bestSortingLayer = int.MinValue;
            int bestSortingOrder = int.MinValue;
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || renderer.sprite == null)
                    continue;

                if (best == null
                    || renderer.sortingLayerID > bestSortingLayer
                    || (renderer.sortingLayerID == bestSortingLayer && renderer.sortingOrder >= bestSortingOrder))
                {
                    best = renderer;
                    bestSortingLayer = renderer.sortingLayerID;
                    bestSortingOrder = renderer.sortingOrder;
                }
            }

            return best;
        }

        public static void DrawPrefabOrSprite(Rect rect, GameObject prefab, Sprite explicitSprite = null, Color? explicitTint = null, bool framed = false)
        {
            if (framed)
                GUI.Box(rect, GUIContent.none);

            Rect padded = Inset(rect, framed ? 4f : 0f);
            var settings = ProjectSettings;

            if (Uses3DPreview(settings) && prefab != null)
            {
                if (TryGetMeshPreviewTexture(prefab, settings, out var meshPreview))
                {
                    GUI.DrawTexture(padded, meshPreview, ScaleMode.ScaleToFit, true);
                    return;
                }

                Texture preview = AssetPreview.GetAssetPreview(prefab) ?? AssetPreview.GetMiniThumbnail(prefab);
                if (preview != null)
                {
                    GUI.DrawTexture(padded, preview, ScaleMode.ScaleToFit, true);
                    return;
                }
            }

            Sprite sprite = explicitSprite;
            Color tint = explicitTint ?? Color.white;
            if (sprite == null && TryGetPrimarySprite(prefab, out var prefabSprite, out var prefabTint))
            {
                sprite = prefabSprite;
                tint = prefabTint;
            }

            if (sprite != null && sprite.texture != null)
            {
                DrawProjectionFootprint(padded, settings);
                DrawSprite(padded, sprite, tint, preserveAspect: true);
                return;
            }

            if (prefab != null)
            {
                Texture fallback = AssetPreview.GetAssetPreview(prefab) ?? AssetPreview.GetMiniThumbnail(prefab);
                if (fallback != null)
                    GUI.DrawTexture(padded, fallback, ScaleMode.ScaleToFit, true);
            }
        }

        private static bool TryGetMeshPreviewTexture(GameObject prefab, MoyvaProjectSettingsSO settings, out Texture2D texture)
        {
            texture = null;
            if (prefab == null || settings == null || !settings.EnableMeshPrefabPreviews)
                return false;

            string key = BuildMeshPreviewCacheKey(prefab, settings);
            if (_meshPreviewTextureCache.TryGetValue(key, out texture) && texture != null)
                return true;

            if (!MoyvaPrefabPreviewRenderer.TryRenderMeshPrefabPreview(prefab, settings, out var pixelData) || !pixelData.IsValid)
                return false;

            texture = new Texture2D(pixelData.Width, pixelData.Height, TextureFormat.RGBA32, settings.GeneratePreviewMipmaps, true)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = settings.PreviewFilterMode,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels(pixelData.Pixels);
            texture.Apply(settings.GeneratePreviewMipmaps, false);

            if (_meshPreviewTextureCache.Count > 128)
                ClearMeshPreviewTextureCache();

            _meshPreviewTextureCache[key] = texture;
            return true;
        }

        private static string BuildMeshPreviewCacheKey(GameObject prefab, MoyvaProjectSettingsSO settings)
        {
            return string.Join("|",
                prefab.GetInstanceID(),
                settings.DefaultProjectionMode,
                settings.DefaultRenderMode,
                settings.ResolvePreviewTextureSize(),
                settings.ResolvePreviewPadding().ToString("0.###"),
                settings.GeneratePreviewMipmaps,
                settings.PreviewFilterMode,
                settings.ResolvePreviewCameraEuler(),
                settings.PreviewLightEuler,
                settings.ResolvePreviewLightIntensity().ToString("0.###"));
        }

        private static void ClearMeshPreviewTextureCache()
        {
            foreach (var entry in _meshPreviewTextureCache)
            {
                if (entry.Value != null)
                    Object.DestroyImmediate(entry.Value);
            }

            _meshPreviewTextureCache.Clear();
        }

        public static void DrawSprite(Rect rect, Sprite sprite, Color tint, bool preserveAspect = true)
        {
            if (sprite == null || sprite.texture == null)
                return;

            Texture2D texture = sprite.texture;
            Rect textureRect = sprite.textureRect;
            Rect uv = new Rect(
                textureRect.x / texture.width,
                textureRect.y / texture.height,
                textureRect.width / texture.width,
                textureRect.height / texture.height);

            Rect drawRect = preserveAspect ? FitRect(rect, textureRect.width / Mathf.Max(1f, textureRect.height)) : rect;
            Color previous = GUI.color;
            GUI.color = previous * tint;
            GUI.DrawTextureWithTexCoords(drawRect, texture, uv, true);
            GUI.color = previous;
        }

        public static bool TryGetSpritePixels(Sprite sprite, Color tint, Color fallbackColor, out AdaptiveSpritePixelData spriteData)
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
            {
                Color fallback = fallbackColor;
                fallback.a = Mathf.Max(0.35f, fallback.a);
                pixels = new[] { fallback };
                width = 1;
                height = 1;
            }

            ApplyTint(pixels, tint);
            spriteData = new AdaptiveSpritePixelData(pixels, width, height);
            return spriteData.IsValid;
        }

        private static void DrawProjectionFootprint(Rect rect, MoyvaProjectSettingsSO settings)
        {
            if (rect.width < 24f || rect.height < 24f || !UsesProjectedGrid(settings.DefaultProjectionMode))
                return;

            Color fill = EditorGUIUtility.isProSkin
                ? new Color(0.32f, 0.42f, 0.48f, 0.16f)
                : new Color(0.20f, 0.31f, 0.36f, 0.14f);
            Color line = EditorGUIUtility.isProSkin
                ? new Color(0.72f, 0.82f, 0.86f, 0.28f)
                : new Color(0.08f, 0.18f, 0.22f, 0.25f);

            Handles.BeginGUI();
            Handles.color = fill;
            Vector3[] points = settings.DefaultProjectionMode == GridProjectionMode.HexPointy2D
                || settings.DefaultProjectionMode == GridProjectionMode.HexFlat2D
                    ? BuildHex(rect, settings.DefaultProjectionMode == GridProjectionMode.HexPointy2D)
                    : BuildDiamond(rect, settings.DefaultProjectionMode == GridProjectionMode.Orthographic3D);
            Handles.DrawAAConvexPolygon(points);
            Handles.color = line;
            Handles.DrawAAPolyLine(1.5f, Close(points));
            Handles.EndGUI();
        }

        private static Vector3[] BuildDiamond(Rect rect, bool lifted)
        {
            float centerX = rect.x + rect.width * 0.5f;
            float centerY = rect.y + rect.height * (lifted ? 0.68f : 0.72f);
            float halfWidth = rect.width * 0.36f;
            float halfHeight = rect.height * (lifted ? 0.16f : 0.20f);
            return new[]
            {
                new Vector3(centerX, centerY - halfHeight),
                new Vector3(centerX + halfWidth, centerY),
                new Vector3(centerX, centerY + halfHeight),
                new Vector3(centerX - halfWidth, centerY),
            };
        }

        private static Vector3[] BuildHex(Rect rect, bool pointy)
        {
            float radius = Mathf.Min(rect.width, rect.height) * 0.28f;
            Vector2 center = new Vector2(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.62f);
            float startDegrees = pointy ? 30f : 0f;
            var points = new Vector3[6];
            for (int i = 0; i < points.Length; i++)
            {
                float radians = Mathf.Deg2Rad * (startDegrees + i * 60f);
                points[i] = new Vector3(center.x + Mathf.Cos(radians) * radius, center.y + Mathf.Sin(radians) * radius);
            }

            return points;
        }

        private static Vector3[] Close(Vector3[] points)
        {
            var closed = new Vector3[points.Length + 1];
            for (int i = 0; i < points.Length; i++)
                closed[i] = points[i];
            closed[points.Length] = points[0];
            return closed;
        }

        private static Rect FitRect(Rect rect, float aspect)
        {
            if (aspect <= 0f || rect.width <= 0f || rect.height <= 0f)
                return rect;

            float rectAspect = rect.width / Mathf.Max(1f, rect.height);
            if (rectAspect > aspect)
            {
                float width = rect.height * aspect;
                return new Rect(rect.x + (rect.width - width) * 0.5f, rect.y, width, rect.height);
            }

            float height = rect.width / aspect;
            return new Rect(rect.x, rect.y + (rect.height - height) * 0.5f, rect.width, height);
        }

        private static Rect Inset(Rect rect, float amount)
        {
            return new Rect(rect.x + amount, rect.y + amount, Mathf.Max(1f, rect.width - amount * 2f), Mathf.Max(1f, rect.height - amount * 2f));
        }

        private static Color[] TryReadSpritePixelsViaRenderTexture(Sprite sprite, int x, int y, int width, int height)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture temporary = null;
            Texture2D readable = null;
            try
            {
                int textureWidth = Mathf.Max(1, sprite.texture.width);
                int textureHeight = Mathf.Max(1, sprite.texture.height);
                temporary = RenderTexture.GetTemporary(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                Graphics.Blit(sprite.texture, temporary);

                RenderTexture.active = temporary;
                readable = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
                readable.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0, false);
                readable.Apply(false, false);

                int safeX = Mathf.Clamp(x, 0, textureWidth - 1);
                int safeY = Mathf.Clamp(y, 0, textureHeight - 1);
                int safeWidth = Mathf.Clamp(width, 1, textureWidth - safeX);
                int safeHeight = Mathf.Clamp(height, 1, textureHeight - safeY);
                return readable.GetPixels(safeX, safeY, safeWidth, safeHeight);
            }
            catch
            {
                return null;
            }
            finally
            {
                RenderTexture.active = previous;
                if (temporary != null)
                    RenderTexture.ReleaseTemporary(temporary);
                if (readable != null)
                    Object.DestroyImmediate(readable);
            }
        }

        private static void ApplyTint(Color[] pixels, Color tint)
        {
            if (pixels == null)
                return;

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] *= tint;
        }
    }
}