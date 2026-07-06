using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal enum WaterPreset
    {
        Custom,
        PixelMinimal,
        PixelStandard,
        PixelHighQuality
    }

    [System.Serializable]
    internal sealed class WaterLayerMaterialSettings : IWaterLayerMaterialSettings
    {
        [SerializeField] private WaterPreset _preset = WaterPreset.PixelMinimal;

        [Header("LOD")]
        [SerializeField, Range(0f, 4f)] internal float _mipBias;
        [SerializeField, Range(0f, 2f)] internal float _zoomLodStrength = 1f;

        [Header("Contour Sources")]
        [SerializeField] internal bool _useContourAlpha;
        [SerializeField] internal bool _useSceneEdge = true;
        [SerializeField] internal bool _useDepthEdge = true;
        [SerializeField] internal bool _useShoreDepth = true;

        [Header("Contour")]
        [SerializeField] internal Color _contourColor = new(0.78f, 0.92f, 1f, 1f);
        [SerializeField, Range(0.5f, 16f)] internal float _contourWidth = 1f;
        [SerializeField, Range(0f, 0.5f)] internal float _contourThreshold = 0.01f;
        [SerializeField, Range(1f, 48f)] internal float _contourSharpness = 18f;
        [SerializeField, Range(0f, 1f)] internal float _contourOpacity = 1f;

        [Header("Line Style")]
        [SerializeField, Range(0f, 1f)] internal float _lineThreshold = 0.04f;
        [SerializeField, Range(0.01f, 1f)] internal float _lineThickness = 0.9f;
        [SerializeField, Range(0.001f, 0.5f)] internal float _lineSoftness = 0.01f;

        [Header("Voronoi Overlay")]
        [SerializeField] internal bool _useVoronoiOverlay = true;
        [SerializeField] internal Color _voronoiColor = new(0.78f, 0.9f, 1f, 1f);
        [SerializeField, Range(1f, 128f)] internal float _voronoiScale = 36f;
        [SerializeField, Range(0.005f, 0.5f)] internal float _voronoiLineWidth = 0.04f;
        [SerializeField, Range(0.001f, 0.5f)] internal float _voronoiSoftness = 0.04f;
        [SerializeField, Range(0f, 1f)] internal float _voronoiOpacity = 0.12f;
        [SerializeField, Range(0f, 1f)] internal float _voronoiJitter = 0.6f;
        [SerializeField, Range(0f, 8f)] internal float _voronoiSpeed = 0.2f;
        [SerializeField] internal Vector4 _voronoiFlowDir = new(1f, 0.35f, 0f, 0f);

        [Header("Multi Band")]
        [SerializeField] internal bool _useMultiBand;
        [SerializeField] internal Color _contourColorMid = new(0.6f, 0.85f, 1f, 0.6f);
        [SerializeField] internal Color _contourColorOuter = new(0.4f, 0.75f, 1f, 0.3f);
        [SerializeField, Range(1.5f, 4f)] internal float _bandWidthMid = 2f;
        [SerializeField, Range(2.5f, 8f)] internal float _bandWidthOuter = 4f;

        [Header("Dither Contour")]
        [SerializeField] internal bool _useDitherContour = true;
        [SerializeField, Range(0f, 1f)] internal float _ditherStrength = 0.25f;

        [Header("Scene Edge")]
        [SerializeField, Range(0.5f, 4f)] internal float _sceneEdgeWidth = 1f;
        [SerializeField, Range(0f, 0.25f)] internal float _sceneEdgeThreshold = 0.03f;
        [SerializeField, Range(1f, 48f)] internal float _sceneEdgeSharpness = 10f;
        [SerializeField, Range(0f, 1f)] internal float _sceneEdgeOpacity = 1f;

        [Header("Shore Depth")]
        [SerializeField, Range(0.05f, 8f)] internal float _shoreDepthDistance = 1.5f;
        [SerializeField, Range(0.001f, 0.5f)] internal float _shoreEdgeWidth = 0.08f;

        [Header("Pixel")]
        [SerializeField] internal bool _usePixelNoise = true;
        [SerializeField] internal bool _usePixelScroll;
        [SerializeField, Range(0.125f, 16f)] internal float _pixelSize = 1f;
        [SerializeField] internal Vector2 _pixelAspect = new(1f, 1f);
        [SerializeField, Range(0.0625f, 4f)] internal float _pixelMinSize = 1f;
        [SerializeField, Range(1f, 128f)] internal float _noiseScale = 8f;
        [SerializeField, Range(1f, 30f)] internal float _noiseSpeed = 6f;
        [SerializeField, Range(0f, 0.02f)] internal float _noiseStrength = 0.002f;
        [SerializeField, Range(2f, 16f)] internal float _colorSteps = 8f;
        [SerializeField, Range(1f, 12f)] internal float _contourSteps = 2f;
        [SerializeField] internal Vector4 _scrollDir = new(1f, 0.3f, 0f, 0f);
        [SerializeField, Range(0f, 16f)] internal float _scrollSpeed = 2f;

        [Header("Pixel Ripple")]
        [SerializeField] internal bool _usePixelRipple;
        [SerializeField, Range(1f, 40f)] internal float _rippleFreq = 10f;
        [SerializeField, Range(0f, 10f)] internal float _rippleSpeed = 3f;
        [SerializeField, Range(0f, 0.02f)] internal float _rippleAmp = 0.005f;

        [Header("Pixel Highlights")]
        [SerializeField] internal bool _usePixelHighlights;
        [SerializeField, Range(0.5f, 1f)] internal float _highlightDitherThreshold = 0.85f;
        [SerializeField, Range(0f, 2f)] internal float _highlightIntensity = 0.6f;

        [Header("Color Bleed")]
        [SerializeField] internal bool _useColorBleed;
        [SerializeField, Range(0f, 1f)] internal float _bleedStrength = 0.25f;
        [SerializeField, Range(0.5f, 4f)] internal float _bleedWidth = 1.5f;

        [Header("Specular Edge")]
        [SerializeField] internal bool _useSpecularEdge;
        [SerializeField, Range(0f, 1f)] internal float _specEdgeThreshold = 0.7f;
        [SerializeField, Range(0f, 2f)] internal float _specEdgeIntensity = 0.8f;
        [SerializeField] internal Color _specEdgeColor = Color.white;

        public WaterPreset Preset
        {
            get => _preset;
            set => _preset = value;
        }
    }
}
