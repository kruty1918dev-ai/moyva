using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class WaterLayerMaterialApplier : IWaterLayerMaterialApplier
    {
        private readonly IWaterLayerMaterialPropertyWriter _writer;

        public WaterLayerMaterialApplier(IWaterLayerMaterialPropertyWriter writer)
        {
            _writer = writer;
        }

        public void Apply(Material material, IWaterLayerMaterialSettings settings)
        {
            if (material == null || settings is not WaterLayerMaterialSettings waterSettings)
                return;

            ApplyContour(material, waterSettings);
            ApplyVoronoi(material, waterSettings);
            ApplySceneDepth(material, waterSettings);
            ApplyPixel(material, waterSettings);
            ApplyDetailEffects(material, waterSettings);
        }

        private void ApplyContour(Material material, WaterLayerMaterialSettings s)
        {
            _writer.SetFloat(material, "_MipBias", s._mipBias);
            _writer.SetFloat(material, "_ZoomLodStrength", s._zoomLodStrength);
            _writer.SetFloat(material, "_UseContourAlpha", s._useContourAlpha ? 1f : 0f);
            _writer.SetFloat(material, "_UseSceneEdge", s._useSceneEdge ? 1f : 0f);
            _writer.SetFloat(material, "_UseDepthEdge", s._useDepthEdge ? 1f : 0f);
            _writer.SetColor(material, "_ContourColor", s._contourColor);
            _writer.SetFloat(material, "_ContourWidth", s._contourWidth);
            _writer.SetFloat(material, "_ContourThreshold", s._contourThreshold);
            _writer.SetFloat(material, "_ContourSharpness", s._contourSharpness);
            _writer.SetFloat(material, "_ContourOpacity", s._contourOpacity);
            _writer.SetFloat(material, "_LineThreshold", s._lineThreshold);
            _writer.SetFloat(material, "_LineThickness", s._lineThickness);
            _writer.SetFloat(material, "_LineSoftness", s._lineSoftness);
        }

        private void ApplyVoronoi(Material material, WaterLayerMaterialSettings s)
        {
            _writer.SetFloat(material, "_UseVoronoiOverlay", s._useVoronoiOverlay ? 1f : 0f);
            _writer.SetColor(material, "_VoronoiColor", s._voronoiColor);
            _writer.SetFloat(material, "_VoronoiScale", s._voronoiScale);
            _writer.SetFloat(material, "_VoronoiLineWidth", s._voronoiLineWidth);
            _writer.SetFloat(material, "_VoronoiSoftness", s._voronoiSoftness);
            _writer.SetFloat(material, "_VoronoiOpacity", s._voronoiOpacity);
            _writer.SetFloat(material, "_VoronoiJitter", s._voronoiJitter);
            _writer.SetFloat(material, "_VoronoiSpeed", s._voronoiSpeed);
            _writer.SetVector(material, "_VoronoiFlowDir", s._voronoiFlowDir);
            _writer.SetFloat(material, "_UseMultiBand", s._useMultiBand ? 1f : 0f);
            _writer.SetColor(material, "_ContourColorMid", s._contourColorMid);
            _writer.SetColor(material, "_ContourColorOuter", s._contourColorOuter);
            _writer.SetFloat(material, "_BandWidthMid", s._bandWidthMid);
            _writer.SetFloat(material, "_BandWidthOuter", s._bandWidthOuter);
        }

        private void ApplySceneDepth(Material material, WaterLayerMaterialSettings s)
        {
            _writer.SetFloat(material, "_UseDitherContour", s._useDitherContour ? 1f : 0f);
            _writer.SetFloat(material, "_DitherStrength", s._ditherStrength);
            _writer.SetFloat(material, "_SceneEdgeWidth", s._sceneEdgeWidth);
            _writer.SetFloat(material, "_SceneEdgeThreshold", s._sceneEdgeThreshold);
            _writer.SetFloat(material, "_SceneEdgeSharpness", s._sceneEdgeSharpness);
            _writer.SetFloat(material, "_SceneEdgeOpacity", s._sceneEdgeOpacity);
            _writer.SetFloat(material, "_UseShoreDepth", s._useShoreDepth ? 1f : 0f);
            _writer.SetFloat(material, "_ShoreDepthDistance", s._shoreDepthDistance);
            _writer.SetFloat(material, "_ShoreEdgeWidth", s._shoreEdgeWidth);
        }

        private void ApplyPixel(Material material, WaterLayerMaterialSettings s)
        {
            _writer.SetFloat(material, "_UsePixelNoise", s._usePixelNoise ? 1f : 0f);
            _writer.SetFloat(material, "_UsePixelScroll", s._usePixelScroll ? 1f : 0f);
            _writer.SetFloat(material, "_PixelSize", s._pixelSize);
            _writer.SetVector(material, "_PixelAspect", new Vector4(s._pixelAspect.x, s._pixelAspect.y, 0f, 0f));
            _writer.SetFloat(material, "_PixelMinSize", s._pixelMinSize);
            _writer.SetFloat(material, "_NoiseScale", s._noiseScale);
            _writer.SetFloat(material, "_NoiseSpeed", s._noiseSpeed);
            _writer.SetFloat(material, "_NoiseStrength", s._noiseStrength);
            _writer.SetFloat(material, "_ColorSteps", s._colorSteps);
            _writer.SetFloat(material, "_ContourSteps", s._contourSteps);
            _writer.SetVector(material, "_ScrollDir", s._scrollDir);
            _writer.SetFloat(material, "_ScrollSpeed", s._scrollSpeed);
        }

        private void ApplyDetailEffects(Material material, WaterLayerMaterialSettings s)
        {
            _writer.SetFloat(material, "_UsePixelRipple", s._usePixelRipple ? 1f : 0f);
            _writer.SetFloat(material, "_RippleFreq", s._rippleFreq);
            _writer.SetFloat(material, "_RippleSpeed", s._rippleSpeed);
            _writer.SetFloat(material, "_RippleAmp", s._rippleAmp);
            _writer.SetFloat(material, "_UsePixelHighlights", s._usePixelHighlights ? 1f : 0f);
            _writer.SetFloat(material, "_HighlightDitherThreshold", s._highlightDitherThreshold);
            _writer.SetFloat(material, "_HighlightIntensity", s._highlightIntensity);
            _writer.SetFloat(material, "_UseColorBleed", s._useColorBleed ? 1f : 0f);
            _writer.SetFloat(material, "_BleedStrength", s._bleedStrength);
            _writer.SetFloat(material, "_BleedWidth", s._bleedWidth);
            _writer.SetFloat(material, "_UseSpecularEdge", s._useSpecularEdge ? 1f : 0f);
            _writer.SetFloat(material, "_SpecEdgeThreshold", s._specEdgeThreshold);
            _writer.SetFloat(material, "_SpecEdgeIntensity", s._specEdgeIntensity);
            _writer.SetColor(material, "_SpecEdgeColor", s._specEdgeColor);
        }
    }
}
