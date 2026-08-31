using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Kruty1918.Moyva.Audio.Runtime;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using Zenject;
using Kruty1918.Moyva.Shared.Connectivity;
using Kruty1918.Moyva.Shared.Graphics;
using Kruty1918.Moyva.Shared.Common;
using Kruty1918.Moyva.Shared.Performance;
using Kruty1918.Moyva.Shared.Diagnostics;
using Kruty1918.Moyva.Shared.UI;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.Shared.Graphics
{
    public enum GraphicsQualityProfile
    {
        Auto = 0,
        Performance = 1,
        Balanced = 2,
        Quality = 3,
        Custom = 4
    }

    public struct GraphicsSettingsData
    {
        public GraphicsQualityProfile Profile;
        public int TargetFrameRate;
        public float RenderScale;
        public bool DynamicRenderScale;
        public bool CloseZoomOptimization;
        public int TextureMipmapLimit;
        public int AntiAliasing;
        public bool VSync;
        public bool Shadows;
        public bool AnisotropicFiltering;
        public float LodBias;

        public GraphicsSettingsData(
            GraphicsQualityProfile profile,
            int targetFrameRate,
            float renderScale,
            bool dynamicRenderScale,
            bool closeZoomOptimization,
            int textureMipmapLimit,
            int antiAliasing,
            bool vSync,
            bool shadows,
            bool anisotropicFiltering,
            float lodBias)
        {
            Profile = profile;
            TargetFrameRate = NormalizeFrameRate(targetFrameRate);
            RenderScale = Mathf.Clamp(renderScale, 0.42f, 1f);
            DynamicRenderScale = dynamicRenderScale;
            CloseZoomOptimization = closeZoomOptimization;
            TextureMipmapLimit = Mathf.Clamp(textureMipmapLimit, 0, 3);
            AntiAliasing = NormalizeAntiAliasing(antiAliasing);
            VSync = vSync;
            Shadows = shadows;
            AnisotropicFiltering = anisotropicFiltering;
            LodBias = Mathf.Clamp(lodBias, 0.4f, 2f);
        }

        public static GraphicsSettingsData CreateDefault()
        {
            return ForProfile(GraphicsQualityProfile.Auto, IsMobileRuntime());
        }

        public static GraphicsSettingsData ForProfile(GraphicsQualityProfile profile, bool isMobile)
        {
            switch (profile)
            {
                case GraphicsQualityProfile.Performance:
                    return new GraphicsSettingsData(profile, 60, isMobile ? 0.60f : 0.80f, false, false, isMobile ? 1 : 0, 0, false, false, false, 0.70f);
                case GraphicsQualityProfile.Quality:
                    return new GraphicsSettingsData(profile, 60, isMobile ? 0.90f : 1f, false, false, 0, isMobile ? 0 : 2, false, !isMobile, true, 1.15f);
                case GraphicsQualityProfile.Balanced:
                    return new GraphicsSettingsData(profile, 60, isMobile ? 0.75f : 1f, false, false, 0, 0, false, false, true, 0.90f);
                default:
                    return isMobile
                        ? new GraphicsSettingsData(GraphicsQualityProfile.Auto, 60, 0.75f, false, false, 0, 0, false, false, true, 0.85f)
                        : new GraphicsSettingsData(GraphicsQualityProfile.Auto, 60, 1f, false, false, 0, 0, false, true, true, 1f);
            }
        }

        public GraphicsSettingsData WithProfile(GraphicsQualityProfile profile)
        {
            if (profile == GraphicsQualityProfile.Custom)
            {
                return new GraphicsSettingsData(
                    GraphicsQualityProfile.Custom,
                    TargetFrameRate,
                    RenderScale,
                    DynamicRenderScale,
                    CloseZoomOptimization,
                    TextureMipmapLimit,
                    AntiAliasing,
                    VSync,
                    Shadows,
                    AnisotropicFiltering,
                    LodBias);
            }

            return ForProfile(profile, IsMobileRuntime());
        }
        public GraphicsSettingsData WithTargetFrameRate(int value) => AsCustom(TargetFrameRate: NormalizeFrameRate(value));
        public GraphicsSettingsData WithRenderScale(float value) => AsCustom(RenderScale: Mathf.Clamp(value, 0.42f, 1f));
        public GraphicsSettingsData WithDynamicRenderScale(bool value) => AsCustom(DynamicRenderScale: value);
        public GraphicsSettingsData WithCloseZoomOptimization(bool value) => AsCustom(CloseZoomOptimization: value);
        public GraphicsSettingsData WithTextureMipmapLimit(int value) => AsCustom(TextureMipmapLimit: Mathf.Clamp(value, 0, 3));
        public GraphicsSettingsData WithAntiAliasing(int value) => AsCustom(AntiAliasing: NormalizeAntiAliasing(value));
        public GraphicsSettingsData WithVSync(bool value) => AsCustom(VSync: value);
        public GraphicsSettingsData WithShadows(bool value) => AsCustom(Shadows: value);
        public GraphicsSettingsData WithAnisotropicFiltering(bool value) => AsCustom(AnisotropicFiltering: value);
        public GraphicsSettingsData WithLodBias(float value) => AsCustom(LodBias: Mathf.Clamp(value, 0.4f, 2f));

        private GraphicsSettingsData AsCustom(
            int? TargetFrameRate = null,
            float? RenderScale = null,
            bool? DynamicRenderScale = null,
            bool? CloseZoomOptimization = null,
            int? TextureMipmapLimit = null,
            int? AntiAliasing = null,
            bool? VSync = null,
            bool? Shadows = null,
            bool? AnisotropicFiltering = null,
            float? LodBias = null)
        {
            return new GraphicsSettingsData(
                GraphicsQualityProfile.Custom,
                TargetFrameRate ?? this.TargetFrameRate,
                RenderScale ?? this.RenderScale,
                DynamicRenderScale ?? this.DynamicRenderScale,
                CloseZoomOptimization ?? this.CloseZoomOptimization,
                TextureMipmapLimit ?? this.TextureMipmapLimit,
                AntiAliasing ?? this.AntiAliasing,
                VSync ?? this.VSync,
                Shadows ?? this.Shadows,
                AnisotropicFiltering ?? this.AnisotropicFiltering,
                LodBias ?? this.LodBias);
        }

        private static bool IsMobileRuntime()
        {
#if UNITY_ANDROID || UNITY_IOS
            return true;
#else
            return Application.isMobilePlatform;
#endif
        }

        private static int NormalizeFrameRate(int value)
        {
            return Mathf.Clamp(value, 30, 360);
        }

        private static int NormalizeAntiAliasing(int value)
        {
            if (value >= 4)
                return 4;

            if (value >= 2)
                return 2;

            return 0;
        }
    }

    public interface IGraphicsSettingsService
    {
        GraphicsSettingsData Settings { get; }
        event Action<GraphicsSettingsData> OnSettingsChanged;

        void SetProfile(GraphicsQualityProfile profile);
        void SetTargetFrameRate(int frameRate);
        void SetRenderScale(float renderScale);
        void SetDynamicRenderScale(bool enabled);
        void SetCloseZoomOptimization(bool enabled);
        void SetTextureMipmapLimit(int mipmapLimit);
        void SetAntiAliasing(int antiAliasing);
        void SetVSync(bool enabled);
        void SetShadows(bool enabled);
        void SetAnisotropicFiltering(bool enabled);
        void SetLodBias(float lodBias);
        void ResetToDefaults();
        void ApplyCurrentSettings();
    }

    internal sealed class GraphicsSettingsService : IGraphicsSettingsService, IInitializable
    {
        private const int Version = 1;
        private readonly string _filePath;
        private readonly GraphicsSettingsData _startupDefaults;
        private readonly DeveloperPixelOptimizationSettings _developerPixelOptimization;
        private RenderPipelineAsset _activeRenderPipeline;
        private PropertyInfo _renderScaleProperty;

        public GraphicsSettingsData Settings { get; private set; }
        public event Action<GraphicsSettingsData> OnSettingsChanged;

        public GraphicsSettingsService(
            [InjectOptional] IClientInstanceScope clientScope = null)
        {
            clientScope ??= ClientInstanceScope.Default;
            _filePath = Path.Combine(
                Application.persistentDataPath,
                clientScope.BuildScopedFileName("graphics_settings.dat"));
            _startupDefaults = GraphicsStartupDefaultsProvider.LoadDefaults();
            _developerPixelOptimization = GraphicsStartupDefaultsProvider.LoadDeveloperPixelOptimization();
            Settings = _startupDefaults;
        }

        public void Initialize()
        {
            Settings = LoadOrCreate();
            ApplyCurrentSettings();
            Save(Settings);
            OnSettingsChanged?.Invoke(Settings);
        }

        public void SetProfile(GraphicsQualityProfile profile) => Update(Settings.WithProfile(profile));
        public void SetTargetFrameRate(int frameRate) => Update(Settings.WithTargetFrameRate(frameRate));
        public void SetRenderScale(float renderScale) => Update(Settings.WithRenderScale(renderScale));
        public void SetDynamicRenderScale(bool enabled) => Update(Settings.WithDynamicRenderScale(enabled));
        public void SetCloseZoomOptimization(bool enabled) => Update(Settings.WithCloseZoomOptimization(enabled));
        public void SetTextureMipmapLimit(int mipmapLimit) => Update(Settings.WithTextureMipmapLimit(mipmapLimit));
        public void SetAntiAliasing(int antiAliasing) => Update(Settings.WithAntiAliasing(antiAliasing));
        public void SetVSync(bool enabled) => Update(Settings.WithVSync(enabled));
        public void SetShadows(bool enabled) => Update(Settings.WithShadows(enabled));
        public void SetAnisotropicFiltering(bool enabled) => Update(Settings.WithAnisotropicFiltering(enabled));
        public void SetLodBias(float lodBias) => Update(Settings.WithLodBias(lodBias));
        public void ResetToDefaults() => Update(_startupDefaults);

        public void ApplyCurrentSettings()
        {
            Apply(Settings);
        }

        private void Update(GraphicsSettingsData next)
        {
            if (AreEquivalent(Settings, next))
                return;

            Settings = next;
            ApplyCurrentSettings();
            Save(Settings);
            OnSettingsChanged?.Invoke(Settings);
        }

        private GraphicsSettingsData LoadOrCreate()
        {
            if (!File.Exists(_filePath))
                return _startupDefaults;

            try
            {
                using var stream = File.OpenRead(_filePath);
                using var reader = new BinaryReader(stream);
                int version = reader.ReadInt32();
                if (version != Version)
                    return _startupDefaults;

                return new GraphicsSettingsData(
                    (GraphicsQualityProfile)reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadSingle(),
                    reader.ReadBoolean(),
                    reader.ReadBoolean(),
                    reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadBoolean(),
                    reader.ReadBoolean(),
                    reader.ReadBoolean(),
                    reader.ReadSingle());
            }
            catch (Exception)
            {
                return _startupDefaults;
            }
        }

        private void Save(GraphicsSettingsData settings)
        {
            try
            {
                string directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                using var stream = File.Create(_filePath);
                using var writer = new BinaryWriter(stream);
                writer.Write(Version);
                writer.Write((int)settings.Profile);
                writer.Write(settings.TargetFrameRate);
                writer.Write(settings.RenderScale);
                writer.Write(settings.DynamicRenderScale);
                writer.Write(settings.CloseZoomOptimization);
                writer.Write(settings.TextureMipmapLimit);
                writer.Write(settings.AntiAliasing);
                writer.Write(settings.VSync);
                writer.Write(settings.Shadows);
                writer.Write(settings.AnisotropicFiltering);
                writer.Write(settings.LodBias);
            }
            catch (Exception e)
            {
                Debug.LogError($"[GraphicsSettings] Failed to save graphics settings: {e.Message}");
            }
        }

        private void Apply(GraphicsSettingsData settings)
        {
            var effective = ApplyDeveloperPixelOverride(settings);

            QualitySettings.vSyncCount = effective.VSync ? 1 : 0;
            Application.targetFrameRate = effective.VSync ? -1 : effective.TargetFrameRate;
            OnDemandRendering.renderFrameInterval = 1;
            QualitySettings.antiAliasing = effective.AntiAliasing;
            QualitySettings.shadows = effective.Shadows ? ShadowQuality.HardOnly : ShadowQuality.Disable;
            QualitySettings.shadowDistance = effective.Shadows ? 20f : 0f;
            QualitySettings.realtimeReflectionProbes = false;
            QualitySettings.softParticles = false;
            QualitySettings.softVegetation = false;
            QualitySettings.globalTextureMipmapLimit = effective.TextureMipmapLimit;
            QualitySettings.anisotropicFiltering = effective.AnisotropicFiltering ? AnisotropicFiltering.Enable : AnisotropicFiltering.Disable;
            QualitySettings.lodBias = effective.LodBias;
            QualitySettings.streamingMipmapsActive = true;
            QualitySettings.resolutionScalingFixedDPIFactor = effective.RenderScale;
            TryApplyRenderScale(effective.RenderScale);
        }

        private GraphicsSettingsData ApplyDeveloperPixelOverride(GraphicsSettingsData settings)
        {
            if (!_developerPixelOptimization.Enabled)
                return settings;

            var next = settings.WithRenderScale(Mathf.Min(settings.RenderScale, _developerPixelOptimization.RenderScaleCap));
            next = next.WithTextureMipmapLimit(Mathf.Max(next.TextureMipmapLimit, _developerPixelOptimization.MinimumTextureMipmapLimit));

            if (_developerPixelOptimization.ForceDisableAntiAliasing)
                next = next.WithAntiAliasing(0);

            if (_developerPixelOptimization.ForceDisableAnisotropicFiltering)
                next = next.WithAnisotropicFiltering(false);

            return next;
        }

        private void TryApplyRenderScale(float renderScale)
        {
            var pipeline = QualitySettings.renderPipeline ?? GraphicsSettings.currentRenderPipeline;
            if (pipeline == null)
                return;

            if (_activeRenderPipeline != pipeline || _renderScaleProperty == null)
            {
                _activeRenderPipeline = pipeline;
                _renderScaleProperty = pipeline.GetType().GetProperty("renderScale", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            if (_renderScaleProperty == null || !_renderScaleProperty.CanWrite || _renderScaleProperty.PropertyType != typeof(float))
                return;

            _renderScaleProperty.SetValue(pipeline, Mathf.Clamp(renderScale, 0.42f, 1f));
        }

        private static bool AreEquivalent(GraphicsSettingsData left, GraphicsSettingsData right)
        {
            return left.Profile == right.Profile
                && left.TargetFrameRate == right.TargetFrameRate
                && Mathf.Approximately(left.RenderScale, right.RenderScale)
                && left.DynamicRenderScale == right.DynamicRenderScale
                && left.CloseZoomOptimization == right.CloseZoomOptimization
                && left.TextureMipmapLimit == right.TextureMipmapLimit
                && left.AntiAliasing == right.AntiAliasing
                && left.VSync == right.VSync
                && left.Shadows == right.Shadows
                && left.AnisotropicFiltering == right.AnisotropicFiltering
                && Mathf.Approximately(left.LodBias, right.LodBias);
        }
    }
}
