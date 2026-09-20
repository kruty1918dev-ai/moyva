using System;
using Kruty1918.Moyva.Camera.API;
using Kruty1918.Moyva.Shared.Graphics;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Visuals
{
    /// <summary>
    /// Publishes far-view atmosphere shader globals from the shared camera
    /// zoom state (<see cref="ICameraZoomState.FarViewWeight"/>) and the JSON
    /// <see cref="FarViewAtmosphereConfig"/>. The renderer feature only reads
    /// globals; all artistic parameters live in the JSON preset.
    /// Quality tier maps GraphicsQualityProfile onto the shader keyword
    /// MOYVA_FARVIEW_QUALITY_LOW (fewer taps / single noise octave).
    /// </summary>
    public sealed class FarViewAtmosphereDriver
        : IInitializable, ITickable, IDisposable
    {
        public const string LowQualityKeyword = "MOYVA_FARVIEW_QUALITY_LOW";
        private const float PushEpsilon = 0.0005f;
        private const float DitherAmount = 1.5f / 255f;

        private static readonly int WeightId =
            Shader.PropertyToID("_MoyvaFarViewWeight");
        private static readonly int HazeColorId =
            Shader.PropertyToID("_MoyvaFarViewHazeColor");
        private static readonly int SkyColorId =
            Shader.PropertyToID("_MoyvaFarViewSkyColor");
        private static readonly int HazeId =
            Shader.PropertyToID("_MoyvaFarViewHaze");
        private static readonly int PaletteId =
            Shader.PropertyToID("_MoyvaFarViewPalette");
        private static readonly int FlattenId =
            Shader.PropertyToID("_MoyvaFarViewFlatten");
        private static readonly int VeilId =
            Shader.PropertyToID("_MoyvaFarViewVeil");
        private static readonly int Veil2Id =
            Shader.PropertyToID("_MoyvaFarViewVeil2");
        private static readonly int WindId =
            Shader.PropertyToID("_MoyvaFarViewWind");
        private static readonly int DebugId =
            Shader.PropertyToID("_MoyvaFarViewDebug");
        private static readonly int DepthAvailableId =
            Shader.PropertyToID("_MoyvaFarViewDepthAvailable");

        private readonly ICameraZoomState _zoomState;
        private readonly FarViewAtmosphereConfig _config;
        private readonly IGraphicsSettingsService _graphicsSettings;

        private float _lastPushedWeight = float.NaN;
        private bool _disposed;

        public FarViewAtmosphereDriver(
            [InjectOptional] ICameraZoomState zoomState = null,
            [InjectOptional] FarViewAtmosphereConfig config = null,
            [InjectOptional] IGraphicsSettingsService graphicsSettings = null)
        {
            _zoomState = zoomState;
            _config = config;
            _graphicsSettings = graphicsSettings;
        }

        public void Initialize()
        {
            PushStaticParameters();
            ApplyQualityTier();
            if (_graphicsSettings != null)
                _graphicsSettings.OnSettingsChanged += OnGraphicsSettingsChanged;
        }

        public void Tick()
        {
            if (_disposed)
                return;

            float weight = ResolveWeight();
            if (Mathf.Abs(weight - _lastPushedWeight) <= PushEpsilon)
                return;

            Shader.SetGlobalFloat(WeightId, weight);
            _lastPushedWeight = weight;
        }

        public void Dispose()
        {
            _disposed = true;
            if (_graphicsSettings != null)
                _graphicsSettings.OnSettingsChanged -= OnGraphicsSettingsChanged;

            Shader.SetGlobalFloat(WeightId, 0f);
            Shader.SetGlobalFloat(DebugId, 0f);
            Shader.DisableKeyword(LowQualityKeyword);
        }

        /// <summary>Current effect weight; 0 when config disabled or no zoom state.</summary>
        internal float ResolveWeight()
        {
            if (_config != null && !_config.enabled)
                return 0f;

            return _zoomState != null
                ? Mathf.Clamp01(_zoomState.FarViewWeight)
                : 0f;
        }

        private void OnGraphicsSettingsChanged(GraphicsSettingsData _)
            => ApplyQualityTier();

        internal void ApplyQualityTier()
        {
            GraphicsQualityProfile profile = _graphicsSettings != null
                ? _graphicsSettings.Settings.Profile
                : GraphicsQualityProfile.Auto;

            bool low = profile == GraphicsQualityProfile.Performance
                || (profile == GraphicsQualityProfile.Auto
                    && Application.isMobilePlatform);

            if (low)
                Shader.EnableKeyword(LowQualityKeyword);
            else
                Shader.DisableKeyword(LowQualityKeyword);
        }

        private void PushStaticParameters()
        {
            bool linear = QualitySettings.activeColorSpace == ColorSpace.Linear;
            FarViewAtmosphereConfig cfg =
                (_config ?? new FarViewAtmosphereConfig()).Normalize();

            Color hazeColor = linear
                ? cfg.haze.atmosphereColor.linear
                : cfg.haze.atmosphereColor;
            Color skyColor = linear
                ? cfg.haze.skyColor.linear
                : cfg.haze.skyColor;

            Shader.SetGlobalColor(HazeColorId, hazeColor);
            Shader.SetGlobalColor(SkyColorId, skyColor);
            Shader.SetGlobalVector(HazeId, new Vector4(
                cfg.haze.strength,
                cfg.haze.depthStart,
                cfg.haze.depthEnd,
                cfg.haze.gamma));
            Shader.SetGlobalVector(PaletteId, new Vector4(
                cfg.palette.saturation,
                cfg.palette.contrast,
                cfg.palette.shadowLift,
                cfg.palette.highlightCompress));
            Shader.SetGlobalVector(FlattenId, new Vector4(
                cfg.flatten.strength,
                cfg.flatten.radiusPixels,
                0f,
                0f));
            Shader.SetGlobalVector(VeilId, new Vector4(
                cfg.veil.strength,
                cfg.veil.scale,
                cfg.veil.speed,
                cfg.veil.altitude));
            Shader.SetGlobalVector(Veil2Id, new Vector4(
                cfg.veil.coverage,
                0f,
                cfg.edge.vignetteStrength,
                cfg.edge.vignetteRadius));
            Shader.SetGlobalVector(WindId, new Vector4(
                cfg.veil.windDirection.x,
                cfg.veil.windDirection.y,
                cfg.quality.ditherEnabled ? DitherAmount : 0f,
                cfg.haze.skyFill));

            Shader.SetGlobalFloat(DepthAvailableId, 0f);
            Shader.SetGlobalFloat(DebugId, 0f);
            Shader.SetGlobalFloat(WeightId, ResolveWeight());
            _lastPushedWeight = ResolveWeight();
        }
    }
}
