using UnityEngine;

namespace Kruty1918.Moyva.Shared.Graphics
{
    [System.Serializable]
    public struct ZoomGraphicsSettings
    {
        public float CloseZoomFullPressureOrthographicSize;
        public float CloseZoomNoPressureOrthographicSize;
        public float CloseZoomFullPressureFieldOfView;
        public float CloseZoomNoPressureFieldOfView;
        public float CloseZoomRenderScaleMin;

        public static ZoomGraphicsSettings CreateDefault()
        {
            return new ZoomGraphicsSettings
            {
                CloseZoomFullPressureOrthographicSize = 7.5f,
                CloseZoomNoPressureOrthographicSize = 18f,
                CloseZoomFullPressureFieldOfView = 22f,
                CloseZoomNoPressureFieldOfView = 45f,
                CloseZoomRenderScaleMin = 0.48f,
            };
        }

        public ZoomGraphicsSettings Normalize()
        {
            float orthoFull = Mathf.Clamp(CloseZoomFullPressureOrthographicSize, 0.1f, 200f);
            float orthoNo = Mathf.Clamp(CloseZoomNoPressureOrthographicSize, orthoFull + 0.1f, 250f);
            float fovFull = Mathf.Clamp(CloseZoomFullPressureFieldOfView, 1f, 179f);
            float fovNo = Mathf.Clamp(CloseZoomNoPressureFieldOfView, fovFull + 0.1f, 179.9f);
            return new ZoomGraphicsSettings
            {
                CloseZoomFullPressureOrthographicSize = orthoFull,
                CloseZoomNoPressureOrthographicSize = orthoNo,
                CloseZoomFullPressureFieldOfView = fovFull,
                CloseZoomNoPressureFieldOfView = fovNo,
                CloseZoomRenderScaleMin = Mathf.Clamp(CloseZoomRenderScaleMin, 0.42f, 1f),
            };
        }
    }

    [System.Serializable]
    public struct DeveloperPixelOptimizationSettings
    {
        public bool Enabled;
        public float RenderScaleCap;
        public int MinimumTextureMipmapLimit;
        public bool ForceDisableAntiAliasing;
        public bool ForceDisableAnisotropicFiltering;

        public static DeveloperPixelOptimizationSettings CreateDefault()
        {
            return new DeveloperPixelOptimizationSettings
            {
                Enabled = false,
                RenderScaleCap = 0.56f,
                MinimumTextureMipmapLimit = 1,
                ForceDisableAntiAliasing = true,
                ForceDisableAnisotropicFiltering = true,
            };
        }

        public DeveloperPixelOptimizationSettings Normalize()
        {
            return new DeveloperPixelOptimizationSettings
            {
                Enabled = Enabled,
                RenderScaleCap = Mathf.Clamp(RenderScaleCap, 0.42f, 1f),
                MinimumTextureMipmapLimit = Mathf.Clamp(MinimumTextureMipmapLimit, 0, 3),
                ForceDisableAntiAliasing = ForceDisableAntiAliasing,
                ForceDisableAnisotropicFiltering = ForceDisableAnisotropicFiltering,
            };
        }
    }

    [CreateAssetMenu(fileName = "MoyvaStartupGraphics", menuName = "Moyva/Graphics/Startup Settings")]
    public sealed class GraphicsStartupSettingsSO : ScriptableObject
    {
        public const string DefaultResourcePath = "MoyvaStartupGraphics";

        [SerializeField] private GraphicsSettingsData _startupSettings = GraphicsSettingsData.CreateDefault();
        [SerializeField] private ZoomGraphicsSettings _zoomSettings = ZoomGraphicsSettings.CreateDefault();
        [SerializeField] private DeveloperPixelOptimizationSettings _developerPixelOptimization = DeveloperPixelOptimizationSettings.CreateDefault();

        public GraphicsSettingsData StartupSettings
        {
            get => _startupSettings.WithProfile(_startupSettings.Profile);
            set => _startupSettings = Normalize(value);
        }

        public ZoomGraphicsSettings ZoomSettings
        {
            get => _zoomSettings.Normalize();
            set => _zoomSettings = value.Normalize();
        }

        public DeveloperPixelOptimizationSettings DeveloperPixelOptimization
        {
            get => _developerPixelOptimization.Normalize();
            set => _developerPixelOptimization = value.Normalize();
        }

        private void OnValidate()
        {
            _startupSettings = Normalize(_startupSettings);
            _zoomSettings = _zoomSettings.Normalize();
            _developerPixelOptimization = _developerPixelOptimization.Normalize();
        }

        private static GraphicsSettingsData Normalize(GraphicsSettingsData value)
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

    public static class GraphicsStartupDefaultsProvider
    {
        public static GraphicsSettingsData LoadDefaults()
        {
            var asset = Resources.Load<GraphicsStartupSettingsSO>(GraphicsStartupSettingsSO.DefaultResourcePath);
            return asset != null ? asset.StartupSettings : GraphicsSettingsData.CreateDefault();
        }

        public static ZoomGraphicsSettings LoadZoomSettings()
        {
            var asset = Resources.Load<GraphicsStartupSettingsSO>(GraphicsStartupSettingsSO.DefaultResourcePath);
            return asset != null ? asset.ZoomSettings : ZoomGraphicsSettings.CreateDefault();
        }

        public static DeveloperPixelOptimizationSettings LoadDeveloperPixelOptimization()
        {
            var asset = Resources.Load<GraphicsStartupSettingsSO>(GraphicsStartupSettingsSO.DefaultResourcePath);
            return asset != null ? asset.DeveloperPixelOptimization : DeveloperPixelOptimizationSettings.CreateDefault();
        }
    }
}