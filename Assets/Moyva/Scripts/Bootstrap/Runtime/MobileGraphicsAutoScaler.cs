using System;
using System.Reflection;
using Kruty1918.Moyva.Shared.Graphics;
using Kruty1918.Moyva.Shared.Performance;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Zenject;

namespace Kruty1918.Moyva.Bootstrap.Runtime
{
    internal sealed class MobileGraphicsAutoScaler : IInitializable, ITickable, IDisposable
    {
        private const int FixedGameplayFrameRate = 60;

        private const string MobileQualityName = "Mobile";
        private const string DesktopQualityName = "PC";
        private const float SampleIntervalSeconds = 1f;
        private const float FallbackLowFpsThreshold = 59f;
        private const float FallbackHealthyFpsThreshold = 59.7f;
        private const float FallbackScaleStep = 0.06f;
        private const float FallbackAdjustmentCooldownSeconds = 2f;
        private const int FallbackHealthySamplesBeforeUpscale = 6;
        private const float ZoomScaleRefreshIntervalSeconds = 0.1f;
        private const float CameraColorParityRefreshIntervalSeconds = 0.5f;
        private const float CameraColorParityDiscoveryIntervalSeconds = 4f;

        private static readonly int MobileFillRatePressureId = Shader.PropertyToID("_MoyvaMobileFillRatePressure");

        private readonly int _originalQualityLevel;
        private readonly int _originalTargetFrameRate;
        private readonly int _originalVSyncCount;
        private readonly RenderPipelineAsset _originalRenderPipeline;
        private readonly ZoomGraphicsSettings _zoomGraphicsSettings;
        private readonly DeveloperPixelOptimizationSettings _developerPixelOptimization;
        private readonly RenderScalePolicySettings _renderScalePolicy;
        private readonly MobilePerformanceThresholds _mobileThresholds;
        private readonly DeviceTierGraphicsProfiles _tierProfiles;

        private RenderPipelineAsset _runtimeRenderPipeline;
        private RenderPipelineAsset _activeRenderPipeline;
        private PropertyInfo _renderScaleProperty;
        private MobileGraphicsBudget _budget;
        private bool _isMobileProfile;
        private bool _renderScaleAvailable;
        private bool _initialized;
        private float _currentRenderScale = 1f;
        private float _performanceRenderScale = 1f;
        private float _fillRatePressure;
        private float _nextZoomScaleRefresh;
        private float _sampleElapsed;
        private int _sampleFrames;
        private float _averageFps;
        private float _adjustmentCooldown;
        private int _healthySamples;
        private UnityEngine.Camera _cachedCamera;
        private Type _universalAdditionalCameraDataType;
        private PropertyInfo _allowHdrOutputProperty;
        private bool _colorPipelineLogged;
        private float _nextCameraColorParityRefresh;
        private float _nextCameraColorParityDiscoveryRefresh;
        private bool _cameraColorParityCacheDirty = true;
        private UnityEngine.Camera[] _colorParityCameras = Array.Empty<UnityEngine.Camera>();

        private IGraphicsSettingsService _graphicsSettingsService;

        public MobileGraphicsAutoScaler()
        {
            _originalQualityLevel = QualitySettings.GetQualityLevel();
            _originalTargetFrameRate = Application.targetFrameRate;
            _originalVSyncCount = QualitySettings.vSyncCount;
            _originalRenderPipeline = QualitySettings.renderPipeline;
            _zoomGraphicsSettings = GraphicsStartupDefaultsProvider.LoadZoomSettings();
            _developerPixelOptimization = GraphicsStartupDefaultsProvider.LoadDeveloperPixelOptimization();
            _renderScalePolicy = AdaptivePerformanceDefaultsProvider.LoadRenderScalePolicy();
            _mobileThresholds = AdaptivePerformanceDefaultsProvider.LoadMobileThresholds();
            _tierProfiles = AdaptivePerformanceDefaultsProvider.LoadTierProfiles();
        }

        [Inject]
        private void Construct([InjectOptional] IGraphicsSettingsService graphicsSettingsService)
        {
            _graphicsSettingsService = graphicsSettingsService;
        }

        public void Initialize()
        {
            _isMobileProfile = ShouldUseMobileProfile();

            ApplyFrameSettings(GetGraphicsSettings());
            OnDemandRendering.renderFrameInterval = 1;
            Shader.SetGlobalFloat(MobileFillRatePressureId, 0f);
            SceneManager.sceneLoaded += HandleSceneLoaded;

            if (_graphicsSettingsService != null)
                _graphicsSettingsService.OnSettingsChanged += HandleGraphicsSettingsChanged;

            if (_isMobileProfile)
            {
                ApplyMobileProfile();
            }
            else
            {
                ApplyDesktopProfile();
            }

            ApplyCameraColorParity(force: true);
            _initialized = true;
        }

        public void Tick()
        {
            if (!_initialized)
                return;

            ApplyCameraColorParity();

            if (!_isMobileProfile)
                return;

            RefreshFillRatePressure();

            if (!_renderScaleAvailable)
                return;

            if (!GetGraphicsSettings().DynamicRenderScale)
                return;

            var deltaTime = Time.unscaledDeltaTime;
            if (deltaTime <= 0f)
                return;

            _sampleElapsed += deltaTime;
            _sampleFrames++;

            if (_sampleElapsed < SampleIntervalSeconds)
                return;

            var intervalFps = _sampleFrames / _sampleElapsed;
            _averageFps = _averageFps <= 0f ? intervalFps : Mathf.Lerp(_averageFps, intervalFps, 0.45f);
            _sampleElapsed = 0f;
            _sampleFrames = 0;

            if (_adjustmentCooldown > 0f)
            {
                _adjustmentCooldown -= SampleIntervalSeconds;
                return;
            }

            float lowFps = _mobileThresholds.LowFpsThreshold > 0f ? _mobileThresholds.LowFpsThreshold : FallbackLowFpsThreshold;
            float healthyFps = _mobileThresholds.HealthyFpsThreshold > 0f ? _mobileThresholds.HealthyFpsThreshold : FallbackHealthyFpsThreshold;
            int healthySamplesBeforeUpscale = _mobileThresholds.HealthySamplesBeforeUpscale > 0 ? _mobileThresholds.HealthySamplesBeforeUpscale : FallbackHealthySamplesBeforeUpscale;
            float scaleStep = _renderScalePolicy.Step > 0f ? _renderScalePolicy.Step : FallbackScaleStep;

            if (_averageFps < lowFps)
            {
                _healthySamples = 0;
                SetPerformanceRenderScale(_performanceRenderScale - scaleStep, $"avgFps={_averageFps:0.0} below {lowFps:0.0}");
                return;
            }

            if (_averageFps >= healthyFps)
            {
                _healthySamples++;
                if (_healthySamples >= healthySamplesBeforeUpscale)
                {
                    _healthySamples = 0;
                    SetPerformanceRenderScale(_performanceRenderScale + scaleStep, $"avgFps={_averageFps:0.0} stable");
                }

                return;
            }

            _healthySamples = 0;
        }

        public void Dispose()
        {
#if UNITY_EDITOR
            if (_originalQualityLevel >= 0 && _originalQualityLevel < QualitySettings.names.Length)
                QualitySettings.SetQualityLevel(_originalQualityLevel, true);

            QualitySettings.renderPipeline = _originalRenderPipeline;
            Application.targetFrameRate = _originalTargetFrameRate;
            QualitySettings.vSyncCount = _originalVSyncCount;
#endif

            Shader.SetGlobalFloat(MobileFillRatePressureId, 0f);

            if (_graphicsSettingsService != null)
                _graphicsSettingsService.OnSettingsChanged -= HandleGraphicsSettingsChanged;

            SceneManager.sceneLoaded -= HandleSceneLoaded;
            _colorParityCameras = Array.Empty<UnityEngine.Camera>();

            if (_runtimeRenderPipeline == null)
                return;

#if UNITY_EDITOR
            UnityEngine.Object.DestroyImmediate(_runtimeRenderPipeline);
#else
            UnityEngine.Object.Destroy(_runtimeRenderPipeline);
#endif
            _runtimeRenderPipeline = null;
        }

        private void ApplyMobileProfile()
        {
            var tier = DetectMobileTier();
            _budget = MobileGraphicsBudget.ForTier(tier);
            var settings = GetEffectiveGraphicsSettings();
            if (settings.Profile == GraphicsQualityProfile.Auto)
                settings = ResolveTierBootstrapSettings(tier);

            SetQualityLevelByName(MobileQualityName);
            ConfigureQualitySettings(_budget, settings);
            CreateRuntimeRenderPipelineClone();
            ConfigureRuntimePipelineColorParity();
            _renderScaleAvailable = RefreshRenderScaleAccess();

            _performanceRenderScale = Mathf.Clamp(settings.RenderScale, _budget.MinimumRenderScale, _budget.MaximumRenderScale);
            RefreshFillRatePressure(force: true);

            if (_renderScaleAvailable)
                ApplyResolvedRenderScale($"mobile {tier} startup", log: true, startCooldown: false);

            Debug.Log($"[MobileGraphics] Mobile profile active. tier={tier}, profile={settings.Profile}, targetFps={settings.TargetFrameRate}, renderScale={_currentRenderScale:0.00}, dynamic={settings.DynamicRenderScale}, closeZoom={settings.CloseZoomOptimization}, memory={SystemInfo.systemMemorySize}MB, gpuMemory={SystemInfo.graphicsMemorySize}MB, cpu={SystemInfo.processorCount}, gpu='{SystemInfo.graphicsDeviceName}'.");
        }

        private void ApplyDesktopProfile()
        {
            var settings = GetGraphicsSettings();
            SetQualityLevelByName(DesktopQualityName);
            ApplyFrameSettings(settings);
            Debug.Log($"[MobileGraphics] Desktop profile active. profile={settings.Profile}, targetFps={settings.TargetFrameRate}, quality='{QualitySettings.names[QualitySettings.GetQualityLevel()]}'.");
        }

        private void ConfigureQualitySettings(MobileGraphicsBudget budget, GraphicsSettingsData settings)
        {
            ApplyFrameSettings(settings);
            QualitySettings.antiAliasing = settings.AntiAliasing;
            QualitySettings.shadows = settings.Shadows ? ShadowQuality.HardOnly : ShadowQuality.Disable;
            QualitySettings.shadowDistance = settings.Shadows ? 20f : 0f;
            QualitySettings.realtimeReflectionProbes = false;
            QualitySettings.softParticles = false;
            QualitySettings.softVegetation = false;
            QualitySettings.lodBias = settings.LodBias;
            QualitySettings.maximumLODLevel = budget.MaximumLodLevel;
            QualitySettings.globalTextureMipmapLimit = settings.TextureMipmapLimit;
            QualitySettings.anisotropicFiltering = settings.AnisotropicFiltering ? AnisotropicFiltering.Enable : AnisotropicFiltering.Disable;
            QualitySettings.particleRaycastBudget = budget.ParticleRaycastBudget;
            QualitySettings.streamingMipmapsActive = true;
            QualitySettings.streamingMipmapsMemoryBudget = budget.StreamingMipmapsMemoryBudget;
            QualitySettings.streamingMipmapsRenderersPerFrame = budget.StreamingMipmapsRenderersPerFrame;
            QualitySettings.resolutionScalingFixedDPIFactor = Mathf.Clamp(settings.RenderScale, 0.42f, 1f);
        }

        private void CreateRuntimeRenderPipelineClone()
        {
            var sourcePipeline = QualitySettings.renderPipeline ?? GraphicsSettings.currentRenderPipeline;
            if (sourcePipeline == null)
            {
                Debug.LogWarning("[MobileGraphics] No active render pipeline asset found; dynamic render scale is disabled.");
                return;
            }

            _runtimeRenderPipeline = UnityEngine.Object.Instantiate(sourcePipeline);
            _runtimeRenderPipeline.name = $"{sourcePipeline.name}_RuntimeAutoScaled";
            QualitySettings.renderPipeline = _runtimeRenderPipeline;
        }

        private bool RefreshRenderScaleAccess()
        {
            var pipeline = QualitySettings.renderPipeline ?? GraphicsSettings.currentRenderPipeline;
            if (pipeline == null)
                return false;

            if (_activeRenderPipeline == pipeline && _renderScaleProperty != null)
                return true;

            var property = pipeline.GetType().GetProperty("renderScale", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null || !property.CanRead || !property.CanWrite || property.PropertyType != typeof(float))
            {
                Debug.LogWarning($"[MobileGraphics] Render pipeline '{pipeline.GetType().Name}' does not expose writable renderScale; dynamic render scale is disabled.");
                return false;
            }

            _activeRenderPipeline = pipeline;
            _renderScaleProperty = property;
            _currentRenderScale = Mathf.Clamp((float)_renderScaleProperty.GetValue(pipeline), _budget.MinimumRenderScale, _budget.MaximumRenderScale);
            return true;
        }

        private void RefreshFillRatePressure(bool force = false)
        {
            if (!GetGraphicsSettings().CloseZoomOptimization)
            {
                if (Mathf.Abs(_fillRatePressure) > 0.001f || force)
                {
                    _fillRatePressure = 0f;
                    Shader.SetGlobalFloat(MobileFillRatePressureId, 0f);
                    if (_renderScaleAvailable)
                        ApplyResolvedRenderScale("close zoom optimization disabled", log: false, startCooldown: false);
                }

                return;
            }

            if (!force && Time.unscaledTime < _nextZoomScaleRefresh)
                return;

            _nextZoomScaleRefresh = Time.unscaledTime + ZoomScaleRefreshIntervalSeconds;

            float previousPressure = _fillRatePressure;
            _fillRatePressure = ResolveFillRatePressure();
            Shader.SetGlobalFloat(MobileFillRatePressureId, _fillRatePressure);

            if (_renderScaleAvailable && (force || Mathf.Abs(previousPressure - _fillRatePressure) >= 0.025f))
                ApplyResolvedRenderScale($"zoomPressure={_fillRatePressure:0.00}", log: false, startCooldown: false);
        }

        private float ResolveFillRatePressure()
        {
            var camera = ResolveActiveCamera();
            if (camera == null)
                return 0f;

            if (camera.orthographic)
            {
                float size = Mathf.Max(0.01f, camera.orthographicSize);
                return 1f - Mathf.Clamp01(Mathf.InverseLerp(
                    _zoomGraphicsSettings.CloseZoomFullPressureOrthographicSize,
                    _zoomGraphicsSettings.CloseZoomNoPressureOrthographicSize,
                    size));
            }

            float fieldOfView = Mathf.Max(0.01f, camera.fieldOfView);
            return 1f - Mathf.Clamp01(Mathf.InverseLerp(
                _zoomGraphicsSettings.CloseZoomFullPressureFieldOfView,
                _zoomGraphicsSettings.CloseZoomNoPressureFieldOfView,
                fieldOfView));
        }

        private UnityEngine.Camera ResolveActiveCamera()
        {
            if (_cachedCamera != null && _cachedCamera.isActiveAndEnabled)
                return _cachedCamera;

            _cachedCamera = UnityEngine.Camera.main;
            if (_cachedCamera != null && _cachedCamera.isActiveAndEnabled)
                return _cachedCamera;

            var cameras = UnityEngine.Object.FindObjectsByType<UnityEngine.Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i] == null || !cameras[i].isActiveAndEnabled)
                    continue;

                _cachedCamera = cameras[i];
                return _cachedCamera;
            }

            return null;
        }

        private void SetPerformanceRenderScale(float requestedScale, string reason)
        {
            float minScale = Mathf.Max(_budget.MinimumRenderScale, _renderScalePolicy.MinimumScale);
            float maxScale = Mathf.Min(_budget.MaximumRenderScale, _renderScalePolicy.MaximumScale);
            var nextScale = Mathf.Clamp(requestedScale, minScale, maxScale);
            if (Mathf.Abs(nextScale - _performanceRenderScale) < 0.001f)
                return;

            _performanceRenderScale = nextScale;
            ApplyResolvedRenderScale(reason, log: true, startCooldown: true);
        }

        private void ApplyResolvedRenderScale(string reason, bool log, bool startCooldown)
        {
            var settings = GetEffectiveGraphicsSettings();
            float closeZoomLimit = settings.CloseZoomOptimization
                ? Mathf.Lerp(_budget.MaximumRenderScale, ResolveCloseZoomRenderScale(), _fillRatePressure)
                : _budget.MaximumRenderScale;
            float baselineScale = settings.DynamicRenderScale ? _performanceRenderScale : settings.RenderScale;
            float requestedScale = Mathf.Min(baselineScale, closeZoomLimit);
            SetRenderScale(requestedScale, reason, log, startCooldown);
        }

        private float ResolveCloseZoomRenderScale()
        {
            return Mathf.Clamp(_zoomGraphicsSettings.CloseZoomRenderScaleMin, _budget.MinimumRenderScale, _budget.MaximumRenderScale);
        }

        private void HandleGraphicsSettingsChanged(GraphicsSettingsData settings)
        {
            ApplyFrameSettings(settings);

            if (!_initialized)
                return;

            if (_isMobileProfile)
            {
                var effectiveSettings = ResolveEffectiveMobileSettings(settings, _budget);
                ConfigureQualitySettings(_budget, effectiveSettings);
                _performanceRenderScale = Mathf.Clamp(effectiveSettings.RenderScale, _budget.MinimumRenderScale, _budget.MaximumRenderScale);
                _averageFps = 0f;
                _sampleElapsed = 0f;
                _sampleFrames = 0;
                _healthySamples = 0;
                RefreshFillRatePressure(force: true);
                if (_renderScaleAvailable)
                    ApplyResolvedRenderScale("user graphics settings", log: true, startCooldown: false);

                ConfigureRuntimePipelineColorParity();
                ApplyCameraColorParity(force: true);
            }
            else
            {
                SetQualityLevelByName(DesktopQualityName);
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _cachedCamera = null;
            _cameraColorParityCacheDirty = true;
            ApplyCameraColorParity(force: true);
        }

        private void ConfigureRuntimePipelineColorParity()
        {
            if (!_isMobileProfile)
                return;

            var pipeline = QualitySettings.renderPipeline ?? GraphicsSettings.currentRenderPipeline;
            if (pipeline == null)
                return;

            bool supportsHdr = TrySetBooleanProperty(pipeline, "supportsHDR", true);
            if (!_colorPipelineLogged)
            {
                _colorPipelineLogged = true;
                Debug.Log($"[MobileGraphics] Color parity active. colorSpace={QualitySettings.activeColorSpace}, pipeline={pipeline.GetType().Name}, supportsHDR={supportsHdr}.");
            }
        }

        private void ApplyCameraColorParity(bool force = false)
        {
            if (!_isMobileProfile)
                return;

            if (!force && Time.unscaledTime < _nextCameraColorParityRefresh)
                return;

            _nextCameraColorParityRefresh = Time.unscaledTime + CameraColorParityRefreshIntervalSeconds;

            if (force || _cameraColorParityCacheDirty || Time.unscaledTime >= _nextCameraColorParityDiscoveryRefresh)
                RefreshCameraColorParityCache();

            var cameras = _colorParityCameras;
            for (int cameraIndex = 0; cameraIndex < cameras.Length; cameraIndex++)
            {
                var camera = cameras[cameraIndex];
                if (camera == null)
                {
                    _cameraColorParityCacheDirty = true;
                    continue;
                }

                camera.allowHDR = true;
                TrySetCameraAllowHdrOutput(camera, false);
            }
        }

        private void RefreshCameraColorParityCache()
        {
            _colorParityCameras = UnityEngine.Object.FindObjectsByType<UnityEngine.Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            _cameraColorParityCacheDirty = false;
            _nextCameraColorParityDiscoveryRefresh = Time.unscaledTime + CameraColorParityDiscoveryIntervalSeconds;
        }

        private bool TrySetCameraAllowHdrOutput(UnityEngine.Camera camera, bool value)
        {
            var cameraDataType = ResolveUniversalAdditionalCameraDataType();
            if (cameraDataType == null)
                return false;

            var cameraData = camera.GetComponent(cameraDataType);
            if (cameraData == null)
                return false;

            _allowHdrOutputProperty ??= cameraDataType.GetProperty("allowHDROutput", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (_allowHdrOutputProperty == null || !_allowHdrOutputProperty.CanWrite || _allowHdrOutputProperty.PropertyType != typeof(bool))
                return false;

            _allowHdrOutputProperty.SetValue(cameraData, value);
            return true;
        }

        private Type ResolveUniversalAdditionalCameraDataType()
        {
            return _universalAdditionalCameraDataType ??= Type.GetType("UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
        }

        private static bool TrySetBooleanProperty(object target, string propertyName, bool value)
        {
            if (target == null)
                return false;

            var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null || !property.CanWrite || property.PropertyType != typeof(bool))
                return false;

            property.SetValue(target, value);
            return true;
        }

        private GraphicsSettingsData GetGraphicsSettings()
        {
            return _graphicsSettingsService != null ? _graphicsSettingsService.Settings : GraphicsSettingsData.CreateDefault();
        }

        private GraphicsSettingsData GetEffectiveGraphicsSettings()
        {
            var settings = GetGraphicsSettings();
            var effective = _isMobileProfile ? ResolveEffectiveMobileSettings(settings, _budget) : settings;
            return ApplyDeveloperPixelOverride(effective);
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

        private GraphicsSettingsData ResolveTierBootstrapSettings(MobileDeviceTier tier)
        {
            switch (tier)
            {
                case MobileDeviceTier.Low:
                    return _tierProfiles.Low;
                case MobileDeviceTier.High:
                    return _tierProfiles.High;
                default:
                    return _tierProfiles.Mid;
            }
        }

        private static GraphicsSettingsData ResolveEffectiveMobileSettings(GraphicsSettingsData settings, MobileGraphicsBudget budget)
        {
            if (settings.Profile != GraphicsQualityProfile.Auto)
                return settings;

            return new GraphicsSettingsData(
                settings.Profile,
                settings.TargetFrameRate,
                budget.InitialRenderScale,
                settings.DynamicRenderScale,
                settings.CloseZoomOptimization,
                budget.TextureMipmapLimit,
                0,
                settings.VSync,
                false,
                budget.AnisotropicFiltering == AnisotropicFiltering.Enable,
                budget.LodBias);
        }

        private static void ApplyFrameSettings(GraphicsSettingsData settings)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = FixedGameplayFrameRate;
            OnDemandRendering.renderFrameInterval = 1;
        }

        private void SetRenderScale(float requestedScale, string reason, bool log, bool startCooldown)
        {
            if (!RefreshRenderScaleAccess())
                return;

            float minScale = Mathf.Max(_budget.MinimumRenderScale, _renderScalePolicy.MinimumScale);
            float maxScale = Mathf.Min(_budget.MaximumRenderScale, _renderScalePolicy.MaximumScale);
            var nextScale = Mathf.Clamp(requestedScale, minScale, maxScale);
            if (Mathf.Abs(nextScale - _currentRenderScale) < 0.001f)
                return;

            var previousScale = _currentRenderScale;
            _renderScaleProperty.SetValue(_activeRenderPipeline, nextScale);
            _currentRenderScale = nextScale;
            if (startCooldown)
                _adjustmentCooldown = _renderScalePolicy.CooldownSeconds > 0f ? _renderScalePolicy.CooldownSeconds : FallbackAdjustmentCooldownSeconds;

            if (log)
                Debug.Log($"[MobileGraphics] Render scale {previousScale:0.00} -> {nextScale:0.00} ({reason}, zoomPressure={_fillRatePressure:0.00}).");
        }

        private static void SetQualityLevelByName(string qualityName)
        {
            var qualityNames = QualitySettings.names;
            for (var index = 0; index < qualityNames.Length; index++)
            {
                if (!string.Equals(qualityNames[index], qualityName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (QualitySettings.GetQualityLevel() != index)
                    QualitySettings.SetQualityLevel(index, true);

                return;
            }

            Debug.LogWarning($"[MobileGraphics] Quality level '{qualityName}' not found. Current quality '{qualityNames[QualitySettings.GetQualityLevel()]}' will be used.");
        }

        private static bool ShouldUseMobileProfile()
        {
#if UNITY_ANDROID || UNITY_IOS
            return true;
#else
            return Application.isMobilePlatform;
#endif
        }

        private static MobileDeviceTier DetectMobileTier()
        {
            var systemMemory = SystemInfo.systemMemorySize;
            var graphicsMemory = SystemInfo.graphicsMemorySize;
            var processorCount = SystemInfo.processorCount;
            var shaderLevel = SystemInfo.graphicsShaderLevel;

            if ((systemMemory > 0 && systemMemory <= 3072) ||
                (graphicsMemory > 0 && graphicsMemory <= 1024) ||
                processorCount <= 4 ||
                shaderLevel < 45)
            {
                return MobileDeviceTier.Low;
            }

            if ((systemMemory <= 0 || systemMemory >= 6144) &&
                (graphicsMemory <= 0 || graphicsMemory >= 2048) &&
                processorCount >= 8 &&
                shaderLevel >= 50)
            {
                return MobileDeviceTier.High;
            }

            return MobileDeviceTier.Mid;
        }

        private enum MobileDeviceTier
        {
            Low,
            Mid,
            High
        }

        private readonly struct MobileGraphicsBudget
        {
            public readonly float InitialRenderScale;
            public readonly float MinimumRenderScale;
            public readonly float MaximumRenderScale;
            public readonly float CloseZoomRenderScale;
            public readonly float LodBias;
            public readonly float FixedDpiFactor;
            public readonly int MaximumLodLevel;
            public readonly int TextureMipmapLimit;
            public readonly int ParticleRaycastBudget;
            public readonly int StreamingMipmapsMemoryBudget;
            public readonly int StreamingMipmapsRenderersPerFrame;
            public readonly AnisotropicFiltering AnisotropicFiltering;

            private MobileGraphicsBudget(
                float initialRenderScale,
                float minimumRenderScale,
                float maximumRenderScale,
                float closeZoomRenderScale,
                float lodBias,
                float fixedDpiFactor,
                int maximumLodLevel,
                int textureMipmapLimit,
                int particleRaycastBudget,
                int streamingMipmapsMemoryBudget,
                int streamingMipmapsRenderersPerFrame,
                AnisotropicFiltering anisotropicFiltering)
            {
                InitialRenderScale = initialRenderScale;
                MinimumRenderScale = minimumRenderScale;
                MaximumRenderScale = maximumRenderScale;
                CloseZoomRenderScale = closeZoomRenderScale;
                LodBias = lodBias;
                FixedDpiFactor = fixedDpiFactor;
                MaximumLodLevel = maximumLodLevel;
                TextureMipmapLimit = textureMipmapLimit;
                ParticleRaycastBudget = particleRaycastBudget;
                StreamingMipmapsMemoryBudget = streamingMipmapsMemoryBudget;
                StreamingMipmapsRenderersPerFrame = streamingMipmapsRenderersPerFrame;
                AnisotropicFiltering = anisotropicFiltering;
            }

            public static MobileGraphicsBudget ForTier(MobileDeviceTier tier)
            {
                switch (tier)
                {
                    case MobileDeviceTier.Low:
                        return new MobileGraphicsBudget(0.65f, 0.42f, 1f, 0.42f, 0.75f, 0.75f, 1, 1, 32, 192, 8, AnisotropicFiltering.Disable);
                    case MobileDeviceTier.High:
                        return new MobileGraphicsBudget(0.82f, 0.42f, 1f, 0.55f, 1f, 1f, 0, 0, 64, 384, 16, AnisotropicFiltering.Enable);
                    default:
                        return new MobileGraphicsBudget(0.72f, 0.42f, 1f, 0.48f, 0.85f, 0.85f, 0, 0, 48, 256, 12, AnisotropicFiltering.Enable);
                }
            }
        }
    }
}