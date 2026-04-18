using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Signals
{
    /// <summary>
    /// Swaps sharedMaterial to a per-instance SelectionSprite material on every
    /// SpriteRenderer in the target hierarchy and restores originals on Clear().
    ///
    /// Why per-instance material:
    /// SpriteRenderer injects [PerRendererData] _MainTex via MaterialPropertyBlock,
    /// but does NOT update _MainTex_TexelSize automatically when textures change via MPB.
    /// Without correct TexelSize the outline neighbor-sample math produces edge=0
    /// (no outline visible) or incorrect results.
    /// Creating a per-instance material lets us set _MainTex_TexelSize explicitly
    /// from the sprite's actual texture dimensions — guaranteeing correct outline width.
    /// </summary>
    public sealed class SpriteSelectionHighlighter : IDisposable
    {
        private readonly List<RendererState> _states = new();

        public bool IsActive => _states.Count > 0;

        public void Apply(GameObject rootObject)
        {
            Clear();
            if (rootObject == null)
                return;

            var shader = SelectionHighlightResources.GetSelectionShader();
            if (shader == null)
                return;

            var renderers = rootObject.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                var sr = renderers[i];
                if (sr == null)
                    continue;

                var instanceMat = new Material(shader) { hideFlags = HideFlags.DontSave };
                SelectionHighlightResources.CopyCompatibleProperties(sr.sharedMaterial, instanceMat);
                ConfigureSelectionMaterial(sr, instanceMat);

                _states.Add(new RendererState(sr, sr.sharedMaterial, instanceMat));
                sr.material = instanceMat;
            }
        }

        public void Clear()
        {
            for (int i = _states.Count - 1; i >= 0; i--)
            {
                var s = _states[i];
                if (s.Renderer != null)
                    s.Renderer.material = s.OriginalMaterial;
                if (s.InstanceMaterial != null)
                    UnityEngine.Object.Destroy(s.InstanceMaterial);
            }
            _states.Clear();
        }

        public void Dispose() => Clear();

        private static void ConfigureSelectionMaterial(SpriteRenderer spriteRenderer, Material material)
        {
            var settings = SelectionHighlightResources.GetSettings();

            material.SetFloat("_OutlineSize", settings.OutlineSizePixels);
            material.SetColor("_OutlineColor", settings.OutlineColorPrimary);
            material.SetColor("_OutlineColorSecondary", settings.OutlineColorSecondary);
            material.SetFloat("_AnimationSpeed", settings.AnimationSpeed);
            material.SetFloat("_AnimationMin", settings.AnimationMin);
            material.SetFloat("_AnimationMax", settings.AnimationMax);

            if (spriteRenderer == null || spriteRenderer.sprite == null)
                return;

            var sprite = spriteRenderer.sprite;
            var outlineWidthOs = settings.OutlineSizePixels / Mathf.Max(sprite.pixelsPerUnit, 0.0001f);
            var uvBounds = SelectionHighlightResources.GetSpriteUvMinMax(sprite);
            var bounds = sprite.bounds;
            var texture = sprite.texture;
            var texelSize = texture != null
                ? new Vector4(1f / Mathf.Max(texture.width, 1), 1f / Mathf.Max(texture.height, 1), texture.width, texture.height)
                : new Vector4(1f, 1f, 1f, 1f);

            material.SetFloat("_OutlineWidthOS", outlineWidthOs);
            material.SetVector("_SpriteLocalMin", new Vector4(bounds.min.x, bounds.min.y, 0f, 0f));
            material.SetVector("_SpriteLocalMax", new Vector4(bounds.max.x, bounds.max.y, 0f, 0f));
            material.SetVector("_SpriteUvMinMax", new Vector4(uvBounds.x, uvBounds.y, uvBounds.z, uvBounds.w));
            material.SetVector("_MainTexTexelSize", texelSize);
        }

        private readonly struct RendererState
        {
            public RendererState(SpriteRenderer renderer, Material original, Material instance)
            {
                Renderer         = renderer;
                OriginalMaterial = original;
                InstanceMaterial = instance;
            }
            public SpriteRenderer Renderer         { get; }
            public Material       OriginalMaterial  { get; }
            public Material       InstanceMaterial  { get; }
        }

        internal readonly struct SelectionHighlightRuntimeSettings
        {
            public SelectionHighlightRuntimeSettings(
                float outlineSizePixels,
                Color outlineColorPrimary,
                Color outlineColorSecondary,
                float animationSpeed,
                float animationMin,
                float animationMax)
            {
                OutlineSizePixels = outlineSizePixels;
                OutlineColorPrimary = outlineColorPrimary;
                OutlineColorSecondary = outlineColorSecondary;
                AnimationSpeed = animationSpeed;
                AnimationMin = animationMin;
                AnimationMax = animationMax;
            }

            public float OutlineSizePixels { get; }
            public Color OutlineColorPrimary { get; }
            public Color OutlineColorSecondary { get; }
            public float AnimationSpeed { get; }
            public float AnimationMin { get; }
            public float AnimationMax { get; }
        }
    }

    internal static class SelectionHighlightResources
    {
        private const string ShaderName = "Moyva/2D/SelectionSprite";
        private const string SettingsResourcePath = "SelectionHighlightSettings";
        private static Shader _shader;
        private static SelectionHighlightSettingsSO _settingsAsset;
        private static bool _settingsLoaded;
        private static readonly SpriteSelectionHighlighter.SelectionHighlightRuntimeSettings DefaultSettings = new(
            2f,
            new Color(0.2f, 0.95f, 0.3f, 1f),
            Color.white,
            2.5f,
            0f,
            1f);

        public static Shader GetSelectionShader()
        {
            if (_shader == null)
                _shader = Shader.Find(ShaderName);

            if (_shader == null)
                Debug.LogWarning($"[SelectionHighlight] Shader '{ShaderName}' not found.");

            return _shader;
        }

        public static SpriteSelectionHighlighter.SelectionHighlightRuntimeSettings GetSettings()
        {
            if (!_settingsLoaded)
            {
                _settingsAsset = Resources.Load<SelectionHighlightSettingsSO>(SettingsResourcePath);
                _settingsLoaded = true;
            }

            if (_settingsAsset == null)
                return DefaultSettings;

            return new SpriteSelectionHighlighter.SelectionHighlightRuntimeSettings(
                Mathf.Max(0.25f, _settingsAsset.OutlineSizePixels),
                _settingsAsset.OutlineColorPrimary,
                _settingsAsset.OutlineColorSecondary,
                Mathf.Max(0f, _settingsAsset.AnimationSpeed),
                Mathf.Clamp01(_settingsAsset.AnimationMin),
                Mathf.Clamp01(_settingsAsset.AnimationMax));
        }

        public static void CopyCompatibleProperties(Material source, Material destination)
        {
            if (destination == null || source == null)
                return;

            CopyColor(source, destination, "_Color");
            CopyFloat(source, destination, "_ZWrite");
            CopyTexture(source, destination, "_MaskTex");
            CopyTexture(source, destination, "_NormalMap");
        }

        public static Vector4 GetSpriteUvMinMax(Sprite sprite)
        {
            if (sprite == null)
                return new Vector4(0f, 0f, 1f, 1f);

            var uv = sprite.uv;
            if (uv == null || uv.Length == 0)
                return new Vector4(0f, 0f, 1f, 1f);

            float minX = uv[0].x;
            float minY = uv[0].y;
            float maxX = uv[0].x;
            float maxY = uv[0].y;

            for (int i = 1; i < uv.Length; i++)
            {
                var point = uv[i];
                if (point.x < minX) minX = point.x;
                if (point.y < minY) minY = point.y;
                if (point.x > maxX) maxX = point.x;
                if (point.y > maxY) maxY = point.y;
            }

            return new Vector4(minX, minY, maxX, maxY);
        }

        private static void CopyColor(Material source, Material destination, string propertyName)
        {
            if (source.HasProperty(propertyName) && destination.HasProperty(propertyName))
                destination.SetColor(propertyName, source.GetColor(propertyName));
        }

        private static void CopyFloat(Material source, Material destination, string propertyName)
        {
            if (source.HasProperty(propertyName) && destination.HasProperty(propertyName))
                destination.SetFloat(propertyName, source.GetFloat(propertyName));
        }

        private static void CopyTexture(Material source, Material destination, string propertyName)
        {
            if (source.HasProperty(propertyName) && destination.HasProperty(propertyName))
                destination.SetTexture(propertyName, source.GetTexture(propertyName));
        }
    }
}