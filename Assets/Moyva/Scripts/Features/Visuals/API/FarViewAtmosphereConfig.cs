using System;
using Kruty1918.Moyva.Jsonization;
using UnityEngine;

namespace Kruty1918.Moyva.Visuals
{
    /// <summary>
    /// JSON-конфігурація атмосферного far-view ефекту камери.
    /// Модель пресету: "far-view-atmosphere" (схема moyva.far-view-atmosphere).
    /// Художні параметри лише тут; нормалізований zoom-стан (FarViewWeight)
    /// надходить із ICameraZoomState — пороги переходу живуть у camera-settings.
    /// </summary>
    [Serializable]
    public sealed class FarViewAtmosphereConfig : MoyvaJsonConfigObject
    {
        public bool enabled = true;
        public FarViewHazeSettings haze = new FarViewHazeSettings();
        public FarViewFlattenSettings flatten = new FarViewFlattenSettings();
        public FarViewVeilSettings veil = new FarViewVeilSettings();
        public FarViewPaletteSettings palette = new FarViewPaletteSettings();
        public FarViewEdgeSettings edge = new FarViewEdgeSettings();
        public FarViewQualitySettings quality = new FarViewQualitySettings();

        /// <summary>Clamp/validate all ranges; returns normalized copy.</summary>
        public FarViewAtmosphereConfig Normalize()
        {
            haze = (haze ?? new FarViewHazeSettings()).Normalize();
            flatten = (flatten ?? new FarViewFlattenSettings()).Normalize();
            veil = (veil ?? new FarViewVeilSettings()).Normalize();
            palette = (palette ?? new FarViewPaletteSettings()).Normalize();
            edge = (edge ?? new FarViewEdgeSettings()).Normalize();
            quality = (quality ?? new FarViewQualitySettings()).Normalize();
            return this;
        }
    }

    /// <summary>Depth-aware aerial-perspective haze.</summary>
    [Serializable]
    public sealed class FarViewHazeSettings
    {
        [Tooltip("Холодне світле повітря на далеких поверхнях.")]
        public Color atmosphereColor = new Color(0.62f, 0.72f, 0.84f, 1f);
        [Tooltip("Колір для неба/нескінченності та краю мапи.")]
        public Color skyColor = new Color(0.70f, 0.79f, 0.90f, 1f);
        [Range(0f, 1f)] public float strength = 0.85f;
        [Tooltip("View-distance (units), звідки haze починає наростати.")]
        [Min(0f)] public float depthStart = 18f;
        [Min(0.01f)] public float depthEnd = 130f;
        [Range(0.2f, 4f)] public float gamma = 1.35f;
        [Tooltip("Наскільки небо (far depth) заповнюється атмосферою при full weight.")]
        [Range(0f, 1f)] public float skyFill = 0.75f;

        public FarViewHazeSettings Normalize()
        {
            strength = Mathf.Clamp01(strength);
            depthStart = Mathf.Max(0f, depthStart);
            depthEnd = Mathf.Max(depthStart + 0.01f, depthEnd);
            gamma = Mathf.Clamp(gamma, 0.2f, 4f);
            skyFill = Mathf.Clamp01(skyFill);
            return this;
        }
    }

    /// <summary>
    /// "Map-like" flattening: local-contrast suppression через кілька семплів
    /// навколо пікселя + легка компресія тіней/хайлайтів.
    /// </summary>
    [Serializable]
    public sealed class FarViewFlattenSettings
    {
        [Range(0f, 1f)] public float strength = 0.55f;
        [Tooltip("Радіус локального середнього в пікселях (при renderScale=1).")]
        [Range(0.25f, 4f)] public float radiusPixels = 1.25f;

        public FarViewFlattenSettings Normalize()
        {
            strength = Mathf.Clamp01(strength);
            radiusPixels = Mathf.Clamp(radiusPixels, 0.25f, 4f);
            return this;
        }
    }

    /// <summary>Процедурний шар хмарного повітря (world-anchored, без текстур).</summary>
    [Serializable]
    public sealed class FarViewVeilSettings
    {
        [Range(0f, 1f)] public float strength = 0.30f;
        [Tooltip("Просторовий масштаб шару у світових одиницях (більше = дрібніше).")]
        [Min(0.001f)] public float scale = 0.045f;
        [Tooltip("Швидкість дрейфу (world units/сек уздовж wind direction).")]
        [Min(0f)] public float speed = 0.35f;
        [Tooltip("Висота уявної хмарної площини над землею — дає parallax.")]
        [Min(0f)] public float altitude = 9f;
        [Tooltip("Щільність покриття: 0 = рідкісні клаптики, 1 = суцільна пелена.")]
        [Range(0f, 1f)] public float coverage = 0.55f;
        [Tooltip("Напрямок дрейфу у world XZ (нормалізується).")]
        public Vector2 windDirection = new Vector2(0.8f, 0.6f);

        public FarViewVeilSettings Normalize()
        {
            strength = Mathf.Clamp01(strength);
            scale = Mathf.Max(0.001f, scale);
            speed = Mathf.Max(0f, speed);
            altitude = Mathf.Max(0f, altitude);
            coverage = Mathf.Clamp01(coverage);
            if (windDirection.sqrMagnitude < 0.001f)
                windDirection = new Vector2(0.8f, 0.6f);
            windDirection.Normalize();
            return this;
        }
    }

    /// <summary>Палітрові корекції для map-like читання на далекій камері.</summary>
    [Serializable]
    public sealed class FarViewPaletteSettings
    {
        [Tooltip("Множник насиченості на full far-view.")]
        [Range(0f, 1.5f)] public float saturation = 0.82f;
        [Tooltip("Множник контрасту на full far-view.")]
        [Range(0.5f, 1.5f)] public float contrast = 0.90f;
        [Tooltip("Підйом тіней у бік атмосферного кольору.")]
        [Range(0f, 0.6f)] public float shadowLift = 0.14f;
        [Tooltip("Компресія яскравих хайлайтів.")]
        [Range(0f, 0.8f)] public float highlightCompress = 0.20f;

        public FarViewPaletteSettings Normalize()
        {
            saturation = Mathf.Clamp(saturation, 0f, 1.5f);
            contrast = Mathf.Clamp(contrast, 0.5f, 1.5f);
            shadowLift = Mathf.Clamp(shadowLift, 0f, 0.6f);
            highlightCompress = Mathf.Clamp(highlightCompress, 0f, 0.8f);
            return this;
        }
    }

    /// <summary>М'яке атмосферне затемнення/засвіт по краях кадру.</summary>
    [Serializable]
    public sealed class FarViewEdgeSettings
    {
        [Range(0f, 1f)] public float vignetteStrength = 0.16f;
        [Range(0.5f, 2.5f)] public float vignetteRadius = 1.15f;

        public FarViewEdgeSettings Normalize()
        {
            vignetteStrength = Mathf.Clamp01(vignetteStrength);
            vignetteRadius = Mathf.Clamp(vignetteRadius, 0.5f, 2.5f);
            return this;
        }
    }

    /// <summary>Per-quality-tier параметри процедурного шару.</summary>
    [Serializable]
    public sealed class FarViewQualitySettings
    {
        [Range(1, 3)] public int performanceVeilOctaves = 1;
        [Range(1, 3)] public int balancedVeilOctaves = 2;
        [Range(1, 3)] public int qualityVeilOctaves = 2;
        [Tooltip("Performance tier: простіший flatten (без зонда-семплів).")]
        public bool performanceSimplifiedFlatten = true;
        [Tooltip("Dither для боротьби з banding на градієнтах.")]
        public bool ditherEnabled = true;

        public FarViewQualitySettings Normalize()
        {
            performanceVeilOctaves = Mathf.Clamp(performanceVeilOctaves, 1, 3);
            balancedVeilOctaves = Mathf.Clamp(balancedVeilOctaves, 1, 3);
            qualityVeilOctaves = Mathf.Clamp(qualityVeilOctaves, 1, 3);
            return this;
        }
    }
}
