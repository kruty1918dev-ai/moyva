using UnityEngine;
using Kruty1918.Moyva.Shared.Graphics;

namespace Kruty1918.Moyva.Shared.Performance
{
    [System.Serializable]
    public struct FrameBudgetSettings
    {
        public float TargetFps;
        public float CpuFrameBudgetMs;
        public float GpuFrameBudgetMs;
        public float AlertCpuFrameMs;
        public float AlertGpuFrameMs;
        public int PercentileWindow;

        public static FrameBudgetSettings CreateDefault()
        {
            return new FrameBudgetSettings
            {
                TargetFps = 60f,
                CpuFrameBudgetMs = 16.6f,
                GpuFrameBudgetMs = 16.6f,
                AlertCpuFrameMs = 19f,
                AlertGpuFrameMs = 19f,
                PercentileWindow = 180,
            };
        }

        public FrameBudgetSettings Normalize()
        {
            return new FrameBudgetSettings
            {
                TargetFps = Mathf.Clamp(TargetFps, 30f, 120f),
                CpuFrameBudgetMs = Mathf.Clamp(CpuFrameBudgetMs, 6f, 45f),
                GpuFrameBudgetMs = Mathf.Clamp(GpuFrameBudgetMs, 6f, 45f),
                AlertCpuFrameMs = Mathf.Clamp(AlertCpuFrameMs, CpuFrameBudgetMs, 60f),
                AlertGpuFrameMs = Mathf.Clamp(AlertGpuFrameMs, GpuFrameBudgetMs, 60f),
                PercentileWindow = Mathf.Clamp(PercentileWindow, 30, 600),
            };
        }
    }

    [System.Serializable]
    public struct RenderScalePolicySettings
    {
        public float MinimumScale;
        public float MaximumScale;
        public float Step;
        public float CooldownSeconds;

        public static RenderScalePolicySettings CreateDefault()
        {
            return new RenderScalePolicySettings
            {
                MinimumScale = 0.42f,
                MaximumScale = 1f,
                Step = 0.06f,
                CooldownSeconds = 2f,
            };
        }

        public RenderScalePolicySettings Normalize()
        {
            float min = Mathf.Clamp(MinimumScale, 0.42f, 1f);
            float max = Mathf.Clamp(MaximumScale, min, 1f);
            return new RenderScalePolicySettings
            {
                MinimumScale = min,
                MaximumScale = max,
                Step = Mathf.Clamp(Step, 0.01f, 0.2f),
                CooldownSeconds = Mathf.Clamp(CooldownSeconds, 0f, 10f),
            };
        }
    }

    [System.Serializable]
    public struct MobilePerformanceThresholds
    {
        public float LowFpsThreshold;
        public float HealthyFpsThreshold;
        public int HealthySamplesBeforeUpscale;

        public static MobilePerformanceThresholds CreateDefault()
        {
            return new MobilePerformanceThresholds
            {
                LowFpsThreshold = 59f,
                HealthyFpsThreshold = 59.7f,
                HealthySamplesBeforeUpscale = 6,
            };
        }

        public MobilePerformanceThresholds Normalize()
        {
            float low = Mathf.Clamp(LowFpsThreshold, 10f, 120f);
            float healthy = Mathf.Clamp(HealthyFpsThreshold, low, 120f);
            return new MobilePerformanceThresholds
            {
                LowFpsThreshold = low,
                HealthyFpsThreshold = healthy,
                HealthySamplesBeforeUpscale = Mathf.Clamp(HealthySamplesBeforeUpscale, 1, 30),
            };
        }
    }

    [System.Serializable]
    public struct DeviceTierGraphicsProfiles
    {
        public GraphicsSettingsData Low;
        public GraphicsSettingsData Mid;
        public GraphicsSettingsData High;

        public static DeviceTierGraphicsProfiles CreateDefault()
        {
            return new DeviceTierGraphicsProfiles
            {
                Low = GraphicsSettingsData.ForProfile(GraphicsQualityProfile.Performance, true),
                Mid = GraphicsSettingsData.ForProfile(GraphicsQualityProfile.Balanced, true),
                High = GraphicsSettingsData.ForProfile(GraphicsQualityProfile.Quality, true),
            };
        }

        public DeviceTierGraphicsProfiles Normalize()
        {
            return new DeviceTierGraphicsProfiles
            {
                Low = NormalizeGraphics(Low),
                Mid = NormalizeGraphics(Mid),
                High = NormalizeGraphics(High),
            };
        }

        private static GraphicsSettingsData NormalizeGraphics(GraphicsSettingsData value)
        {
            return new GraphicsSettingsData(
                value.Profile,
                value.TargetFrameRate,
                value.RenderScale,
                value.DynamicRenderScale,
                value.CloseZoomOptimization,
                value.TextureMipmapLimit,
                value.AntiAliasing,
                value.VSync,
                value.Shadows,
                value.AnisotropicFiltering,
                value.LodBias);
        }
    }

    [System.Serializable]
    public struct PrewarmSettings
    {
        public bool WarmupAllShaders;
        public string[] ShaderResourcePaths;
        public string[] MaterialResourcePaths;
        public string[] CriticalSpriteAtlasResourcePaths;

        public static PrewarmSettings CreateDefault()
        {
            return new PrewarmSettings
            {
                WarmupAllShaders = false,
                ShaderResourcePaths = new string[0],
                MaterialResourcePaths = new string[0],
                CriticalSpriteAtlasResourcePaths = new string[0],
            };
        }
    }

    [System.Serializable]
    public struct GcMonitorSettings
    {
        public bool Enabled;
        public float SampleIntervalSeconds;
        public long BurstThresholdBytes;

        public static GcMonitorSettings CreateDefault()
        {
            return new GcMonitorSettings
            {
                Enabled = true,
                SampleIntervalSeconds = 2f,
                BurstThresholdBytes = 512 * 1024,
            };
        }

        public GcMonitorSettings Normalize()
        {
            return new GcMonitorSettings
            {
                Enabled = Enabled,
                SampleIntervalSeconds = Mathf.Clamp(SampleIntervalSeconds, 0.25f, 10f),
                BurstThresholdBytes = (long)Mathf.Clamp(BurstThresholdBytes, 16 * 1024, 8 * 1024 * 1024),
            };
        }
    }

    [System.Serializable]
    public struct HotspotSamplingSettings
    {
        public bool Enabled;
        public float SampleIntervalSeconds;
        public float HotspotThresholdMs;

        public static HotspotSamplingSettings CreateDefault()
        {
            return new HotspotSamplingSettings
            {
                Enabled = true,
                SampleIntervalSeconds = 10f,
                HotspotThresholdMs = 3f,
            };
        }

        public HotspotSamplingSettings Normalize()
        {
            return new HotspotSamplingSettings
            {
                Enabled = Enabled,
                SampleIntervalSeconds = Mathf.Clamp(SampleIntervalSeconds, 2f, 60f),
                HotspotThresholdMs = Mathf.Clamp(HotspotThresholdMs, 0.25f, 20f),
            };
        }
    }

    [CreateAssetMenu(fileName = "MoyvaAdaptivePerformance", menuName = "Moyva/Performance/Adaptive Performance Settings")]
    public sealed class AdaptivePerformanceSettingsSO : ScriptableObject
    {
        public const string DefaultResourcePath = "MoyvaAdaptivePerformance";

        [SerializeField] private FrameBudgetSettings _frameBudget = FrameBudgetSettings.CreateDefault();
        [SerializeField] private RenderScalePolicySettings _renderScalePolicy = RenderScalePolicySettings.CreateDefault();
        [SerializeField] private MobilePerformanceThresholds _mobileThresholds = MobilePerformanceThresholds.CreateDefault();
        [SerializeField] private DeviceTierGraphicsProfiles _tierProfiles = DeviceTierGraphicsProfiles.CreateDefault();
        [SerializeField] private PrewarmSettings _prewarmSettings = PrewarmSettings.CreateDefault();
        [SerializeField] private GcMonitorSettings _gcMonitor = GcMonitorSettings.CreateDefault();
        [SerializeField] private HotspotSamplingSettings _hotspotSampling = HotspotSamplingSettings.CreateDefault();

        public FrameBudgetSettings FrameBudget => _frameBudget.Normalize();
        public RenderScalePolicySettings RenderScalePolicy => _renderScalePolicy.Normalize();
        public MobilePerformanceThresholds MobileThresholds => _mobileThresholds.Normalize();
        public DeviceTierGraphicsProfiles TierProfiles => _tierProfiles.Normalize();
        public PrewarmSettings Prewarm => _prewarmSettings;
        public GcMonitorSettings GcMonitor => _gcMonitor.Normalize();
        public HotspotSamplingSettings HotspotSampling => _hotspotSampling.Normalize();

        private void OnValidate()
        {
            _frameBudget = _frameBudget.Normalize();
            _renderScalePolicy = _renderScalePolicy.Normalize();
            _mobileThresholds = _mobileThresholds.Normalize();
            _tierProfiles = _tierProfiles.Normalize();
            _gcMonitor = _gcMonitor.Normalize();
            _hotspotSampling = _hotspotSampling.Normalize();
        }
    }

    public static class AdaptivePerformanceDefaultsProvider
    {
        public static AdaptivePerformanceSettingsSO LoadAsset()
        {
            return Resources.Load<AdaptivePerformanceSettingsSO>(AdaptivePerformanceSettingsSO.DefaultResourcePath);
        }

        public static FrameBudgetSettings LoadFrameBudget()
        {
            var asset = LoadAsset();
            return asset != null ? asset.FrameBudget : FrameBudgetSettings.CreateDefault();
        }

        public static RenderScalePolicySettings LoadRenderScalePolicy()
        {
            var asset = LoadAsset();
            return asset != null ? asset.RenderScalePolicy : RenderScalePolicySettings.CreateDefault();
        }

        public static MobilePerformanceThresholds LoadMobileThresholds()
        {
            var asset = LoadAsset();
            return asset != null ? asset.MobileThresholds : MobilePerformanceThresholds.CreateDefault();
        }

        public static DeviceTierGraphicsProfiles LoadTierProfiles()
        {
            var asset = LoadAsset();
            return asset != null ? asset.TierProfiles : DeviceTierGraphicsProfiles.CreateDefault();
        }

        public static PrewarmSettings LoadPrewarmSettings()
        {
            var asset = LoadAsset();
            return asset != null ? asset.Prewarm : PrewarmSettings.CreateDefault();
        }

        public static GcMonitorSettings LoadGcMonitorSettings()
        {
            var asset = LoadAsset();
            return asset != null ? asset.GcMonitor : GcMonitorSettings.CreateDefault();
        }

        public static HotspotSamplingSettings LoadHotspotSampling()
        {
            var asset = LoadAsset();
            return asset != null ? asset.HotspotSampling : HotspotSamplingSettings.CreateDefault();
        }
    }
}
