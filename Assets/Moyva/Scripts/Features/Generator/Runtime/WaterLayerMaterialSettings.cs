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
    internal sealed class WaterLayerMaterialSettings
    {
        [SerializeField] private WaterPreset _preset = WaterPreset.PixelMinimal;

        [Header("LOD")]
        [SerializeField, Range(0f, 4f)] private float _mipBias;
        [SerializeField, Range(0f, 2f)] private float _zoomLodStrength = 1f;

        [Header("Contour Sources")]
        [SerializeField] private bool _useContourAlpha;
        [SerializeField] private bool _useSceneEdge = true;
        [SerializeField] private bool _useDepthEdge = true;
        [SerializeField] private bool _useShoreDepth = true;

        [Header("Contour")]
        [SerializeField] private Color _contourColor = new(0.78f, 0.92f, 1f, 1f);
        [SerializeField, Range(0.5f, 16f)] private float _contourWidth = 1f;
        [SerializeField, Range(0f, 0.5f)] private float _contourThreshold = 0.01f;
        [SerializeField, Range(1f, 48f)] private float _contourSharpness = 18f;
        [SerializeField, Range(0f, 1f)] private float _contourOpacity = 1f;

        [Header("Line Style")]
        [SerializeField, Range(0f, 1f)] private float _lineThreshold = 0.04f;
        [SerializeField, Range(0.01f, 1f)] private float _lineThickness = 0.9f;
        [SerializeField, Range(0.001f, 0.5f)] private float _lineSoftness = 0.01f;

        [Header("Voronoi Overlay")]
        [SerializeField] private bool _useVoronoiOverlay = true;
        [SerializeField] private Color _voronoiColor = new(0.78f, 0.9f, 1f, 1f);
        [SerializeField, Range(1f, 128f)] private float _voronoiScale = 36f;
        [SerializeField, Range(0.005f, 0.5f)] private float _voronoiLineWidth = 0.04f;
        [SerializeField, Range(0.001f, 0.5f)] private float _voronoiSoftness = 0.04f;
        [SerializeField, Range(0f, 1f)] private float _voronoiOpacity = 0.12f;
        [SerializeField, Range(0f, 1f)] private float _voronoiJitter = 0.6f;
        [SerializeField, Range(0f, 8f)] private float _voronoiSpeed = 0.2f;
        [SerializeField] private Vector4 _voronoiFlowDir = new(1f, 0.35f, 0f, 0f);

        [Header("Multi Band")]
        [SerializeField] private bool _useMultiBand;
        [SerializeField] private Color _contourColorMid = new(0.6f, 0.85f, 1f, 0.6f);
        [SerializeField] private Color _contourColorOuter = new(0.4f, 0.75f, 1f, 0.3f);
        [SerializeField, Range(1.5f, 4f)] private float _bandWidthMid = 2f;
        [SerializeField, Range(2.5f, 8f)] private float _bandWidthOuter = 4f;

        [Header("Dither Contour")]
        [SerializeField] private bool _useDitherContour = true;
        [SerializeField, Range(0f, 1f)] private float _ditherStrength = 0.25f;

        [Header("Scene Edge")]
        [SerializeField, Range(0.5f, 4f)] private float _sceneEdgeWidth = 1f;
        [SerializeField, Range(0f, 0.25f)] private float _sceneEdgeThreshold = 0.03f;
        [SerializeField, Range(1f, 48f)] private float _sceneEdgeSharpness = 10f;
        [SerializeField, Range(0f, 1f)] private float _sceneEdgeOpacity = 1f;

        [Header("Shore Depth")]
        [SerializeField, Range(0.05f, 8f)] private float _shoreDepthDistance = 1.5f;
        [SerializeField, Range(0.001f, 0.5f)] private float _shoreEdgeWidth = 0.08f;

        [Header("Pixel")]
        [SerializeField] private bool _usePixelNoise = true;
        [SerializeField] private bool _usePixelScroll;
        [SerializeField, Range(0.125f, 16f)] private float _pixelSize = 1f;
        [SerializeField] private Vector2 _pixelAspect = new(1f, 1f);
        [SerializeField, Range(0.0625f, 4f)] private float _pixelMinSize = 1f;
        [SerializeField, Range(1f, 128f)] private float _noiseScale = 8f;
        [SerializeField, Range(1f, 30f)] private float _noiseSpeed = 6f;
        [SerializeField, Range(0f, 0.02f)] private float _noiseStrength = 0.002f;
        [SerializeField, Range(2f, 16f)] private float _colorSteps = 8f;
        [SerializeField, Range(1f, 12f)] private float _contourSteps = 2f;
        [SerializeField] private Vector4 _scrollDir = new(1f, 0.3f, 0f, 0f);
        [SerializeField, Range(0f, 16f)] private float _scrollSpeed = 2f;

        [Header("Pixel Ripple")]
        [SerializeField] private bool _usePixelRipple;
        [SerializeField, Range(1f, 40f)] private float _rippleFreq = 10f;
        [SerializeField, Range(0f, 10f)] private float _rippleSpeed = 3f;
        [SerializeField, Range(0f, 0.02f)] private float _rippleAmp = 0.005f;

        [Header("Pixel Highlights")]
        [SerializeField] private bool _usePixelHighlights;
        [SerializeField, Range(0.5f, 1f)] private float _highlightDitherThreshold = 0.85f;
        [SerializeField, Range(0f, 2f)] private float _highlightIntensity = 0.6f;

        [Header("Color Bleed")]
        [SerializeField] private bool _useColorBleed;
        [SerializeField, Range(0f, 1f)] private float _bleedStrength = 0.25f;
        [SerializeField, Range(0.5f, 4f)] private float _bleedWidth = 1.5f;

        [Header("Specular Edge")]
        [SerializeField] private bool _useSpecularEdge;
        [SerializeField, Range(0f, 1f)] private float _specEdgeThreshold = 0.7f;
        [SerializeField, Range(0f, 2f)] private float _specEdgeIntensity = 0.8f;
        [SerializeField] private Color _specEdgeColor = Color.white;

        public void ApplyPreset()
        {
            switch (_preset)
            {
                case WaterPreset.PixelMinimal: ApplyPixelMinimal(); break;
                case WaterPreset.PixelStandard: ApplyPixelStandard(); break;
                case WaterPreset.PixelHighQuality: ApplyPixelHighQuality(); break;
            }
        }

        private void ApplyPixelMinimal()
        {
            _pixelSize = 1f; _pixelMinSize = 1f; _pixelAspect = Vector2.one;
            _noiseScale = 8f; _noiseSpeed = 6f; _noiseStrength = 0.002f;
            _usePixelNoise = true; _usePixelScroll = false; _usePixelRipple = false;
            _colorSteps = 8f; _contourSteps = 2f;
            _useVoronoiOverlay = true;
            _voronoiScale = 36f; _voronoiLineWidth = 0.04f; _voronoiOpacity = 0.12f;
            _voronoiJitter = 0.6f; _voronoiSpeed = 0.2f;
            _useSceneEdge = true; _sceneEdgeWidth = 1f;
            _sceneEdgeThreshold = 0.03f; _sceneEdgeSharpness = 10f;
            _contourWidth = 1f; _contourThreshold = 0.01f;
            _contourSharpness = 18f; _contourOpacity = 1f;
            _lineThreshold = 0.04f; _lineThickness = 0.9f; _lineSoftness = 0.01f;
            _useDitherContour = true; _ditherStrength = 0.25f;
            _useColorBleed = false; _useSpecularEdge = false;
            _usePixelHighlights = false; _useMultiBand = false;
        }

        private void ApplyPixelStandard()
        {
            _pixelSize = 0.5f; _pixelMinSize = 0.5f; _pixelAspect = Vector2.one;
            _noiseScale = 16f; _noiseSpeed = 8f; _noiseStrength = 0.0035f;
            _usePixelNoise = true;
            _usePixelScroll = true; _scrollDir = new Vector4(0.6f, 0.2f, 0f, 0f); _scrollSpeed = 1.5f;
            _usePixelRipple = true; _rippleFreq = 12f; _rippleSpeed = 3f; _rippleAmp = 0.004f;
            _colorSteps = 12f; _contourSteps = 3f;
            _useVoronoiOverlay = true;
            _voronoiScale = 48f; _voronoiLineWidth = 0.06f; _voronoiOpacity = 0.18f;
            _voronoiJitter = 0.8f; _voronoiSpeed = 0.35f;
            _useSceneEdge = true; _sceneEdgeWidth = 1f;
            _sceneEdgeThreshold = 0.02f; _sceneEdgeSharpness = 12f;
            _contourWidth = 1.2f; _contourThreshold = 0.012f;
            _contourSharpness = 20f; _contourOpacity = 1f;
            _lineThreshold = 0.035f; _lineThickness = 0.85f; _lineSoftness = 0.008f;
            _useDitherContour = true; _ditherStrength = 0.28f;
            _useColorBleed = true; _bleedStrength = 0.15f; _bleedWidth = 1.2f;
            _useSpecularEdge = true; _specEdgeThreshold = 0.75f; _specEdgeIntensity = 0.5f;
            _usePixelHighlights = false; _useMultiBand = false;
        }

        private void ApplyPixelHighQuality()
        {
            _pixelSize = 0.5f; _pixelMinSize = 0.25f; _pixelAspect = Vector2.one;
            _noiseScale = 64f; _noiseSpeed = 10f; _noiseStrength = 0.0045f;
            _usePixelNoise = true;
            _usePixelScroll = true; _scrollSpeed = 2f;
            _usePixelRipple = true; _rippleFreq = 14f; _rippleSpeed = 4f; _rippleAmp = 0.006f;
            _colorSteps = 10f; _contourSteps = 4f;
            _useVoronoiOverlay = true;
            _voronoiScale = 96f; _voronoiLineWidth = 0.03f; _voronoiOpacity = 0.12f;
            _voronoiJitter = 1f; _voronoiSpeed = 0.45f;
            _useMultiBand = true; _bandWidthMid = 2f; _bandWidthOuter = 4f;
            _contourColorMid = new Color(0.6f, 0.85f, 1f, 0.6f);
            _contourColorOuter = new Color(0.4f, 0.75f, 1f, 0.3f);
            _useDitherContour = true; _ditherStrength = 0.35f;
            _usePixelHighlights = true; _highlightDitherThreshold = 0.88f; _highlightIntensity = 0.6f;
            _useColorBleed = true; _bleedStrength = 0.25f;
            _useSpecularEdge = false;
        }

        public void ApplyTo(Material material)
        {
            if (material == null)
                return;

            SetFloatIf(material, "_MipBias", _mipBias);
            SetFloatIf(material, "_ZoomLodStrength", _zoomLodStrength);

            SetFloatIf(material, "_UseContourAlpha", _useContourAlpha ? 1f : 0f);
            SetFloatIf(material, "_UseSceneEdge", _useSceneEdge ? 1f : 0f);
            SetFloatIf(material, "_UseDepthEdge", _useDepthEdge ? 1f : 0f);
            SetFloatIf(material, "_UseShoreDepth", _useShoreDepth ? 1f : 0f);

            SetColorIf(material, "_ContourColor", _contourColor);
            SetFloatIf(material, "_ContourWidth", _contourWidth);
            SetFloatIf(material, "_ContourThreshold", _contourThreshold);
            SetFloatIf(material, "_ContourSharpness", _contourSharpness);
            SetFloatIf(material, "_ContourOpacity", _contourOpacity);

            SetFloatIf(material, "_LineThreshold", _lineThreshold);
            SetFloatIf(material, "_LineThickness", _lineThickness);
            SetFloatIf(material, "_LineSoftness", _lineSoftness);

            SetFloatIf(material, "_UseVoronoiOverlay", _useVoronoiOverlay ? 1f : 0f);
            SetColorIf(material, "_VoronoiColor", _voronoiColor);
            SetFloatIf(material, "_VoronoiScale", _voronoiScale);
            SetFloatIf(material, "_VoronoiLineWidth", _voronoiLineWidth);
            SetFloatIf(material, "_VoronoiSoftness", _voronoiSoftness);
            SetFloatIf(material, "_VoronoiOpacity", _voronoiOpacity);
            SetFloatIf(material, "_VoronoiJitter", _voronoiJitter);
            SetFloatIf(material, "_VoronoiSpeed", _voronoiSpeed);
            SetVectorIf(material, "_VoronoiFlowDir", _voronoiFlowDir);

            SetFloatIf(material, "_UseMultiBand", _useMultiBand ? 1f : 0f);
            SetColorIf(material, "_ContourColorMid", _contourColorMid);
            SetColorIf(material, "_ContourColorOuter", _contourColorOuter);
            SetFloatIf(material, "_BandWidthMid", _bandWidthMid);
            SetFloatIf(material, "_BandWidthOuter", _bandWidthOuter);

            SetFloatIf(material, "_UseDitherContour", _useDitherContour ? 1f : 0f);
            SetFloatIf(material, "_DitherStrength", _ditherStrength);

            SetFloatIf(material, "_SceneEdgeWidth", _sceneEdgeWidth);
            SetFloatIf(material, "_SceneEdgeThreshold", _sceneEdgeThreshold);
            SetFloatIf(material, "_SceneEdgeSharpness", _sceneEdgeSharpness);
            SetFloatIf(material, "_SceneEdgeOpacity", _sceneEdgeOpacity);

            SetFloatIf(material, "_ShoreDepthDistance", _shoreDepthDistance);
            SetFloatIf(material, "_ShoreEdgeWidth", _shoreEdgeWidth);

            SetFloatIf(material, "_UsePixelNoise", _usePixelNoise ? 1f : 0f);
            SetFloatIf(material, "_UsePixelScroll", _usePixelScroll ? 1f : 0f);
            SetFloatIf(material, "_PixelSize", _pixelSize);
            SetVectorIf(material, "_PixelAspect", new Vector4(_pixelAspect.x, _pixelAspect.y, 0f, 0f));
            SetFloatIf(material, "_PixelMinSize", _pixelMinSize);
            SetFloatIf(material, "_NoiseScale", _noiseScale);
            SetFloatIf(material, "_NoiseSpeed", _noiseSpeed);
            SetFloatIf(material, "_NoiseStrength", _noiseStrength);
            SetFloatIf(material, "_ColorSteps", _colorSteps);
            SetFloatIf(material, "_ContourSteps", _contourSteps);
            SetVectorIf(material, "_ScrollDir", _scrollDir);
            SetFloatIf(material, "_ScrollSpeed", _scrollSpeed);

            SetFloatIf(material, "_UsePixelRipple", _usePixelRipple ? 1f : 0f);
            SetFloatIf(material, "_RippleFreq", _rippleFreq);
            SetFloatIf(material, "_RippleSpeed", _rippleSpeed);
            SetFloatIf(material, "_RippleAmp", _rippleAmp);

            SetFloatIf(material, "_UsePixelHighlights", _usePixelHighlights ? 1f : 0f);
            SetFloatIf(material, "_HighlightDitherThreshold", _highlightDitherThreshold);
            SetFloatIf(material, "_HighlightIntensity", _highlightIntensity);

            SetFloatIf(material, "_UseColorBleed", _useColorBleed ? 1f : 0f);
            SetFloatIf(material, "_BleedStrength", _bleedStrength);
            SetFloatIf(material, "_BleedWidth", _bleedWidth);

            SetFloatIf(material, "_UseSpecularEdge", _useSpecularEdge ? 1f : 0f);
            SetFloatIf(material, "_SpecEdgeThreshold", _specEdgeThreshold);
            SetFloatIf(material, "_SpecEdgeIntensity", _specEdgeIntensity);
            SetColorIf(material, "_SpecEdgeColor", _specEdgeColor);
        }

        private static void SetFloatIf(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
                material.SetFloat(propertyName, value);
        }

        private static void SetColorIf(Material material, string propertyName, Color value)
        {
            if (material.HasProperty(propertyName))
                material.SetColor(propertyName, value);
        }

        private static void SetVectorIf(Material material, string propertyName, Vector4 value)
        {
            if (material.HasProperty(propertyName))
                material.SetVector(propertyName, value);
        }
    }
}