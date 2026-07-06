using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class WaterLayerMaterialPresetService : IWaterLayerMaterialPresetService
    {
        public void ApplyPreset(IWaterLayerMaterialSettings settings)
        {
            if (settings is not WaterLayerMaterialSettings waterSettings)
                return;

            switch (waterSettings.Preset)
            {
                case WaterPreset.PixelMinimal:
                    ApplyPixelMinimal(waterSettings);
                    break;
                case WaterPreset.PixelStandard:
                    ApplyPixelStandard(waterSettings);
                    break;
                case WaterPreset.PixelHighQuality:
                    ApplyPixelHighQuality(waterSettings);
                    break;
            }
        }

        private static void ApplyPixelMinimal(WaterLayerMaterialSettings s)
        {
            s._pixelSize = 1f; s._pixelMinSize = 1f; s._pixelAspect = Vector2.one;
            s._noiseScale = 8f; s._noiseSpeed = 6f; s._noiseStrength = 0.002f;
            s._usePixelNoise = true; s._usePixelScroll = false; s._usePixelRipple = false;
            s._colorSteps = 8f; s._contourSteps = 2f;
            s._useVoronoiOverlay = true;
            s._voronoiScale = 36f; s._voronoiLineWidth = 0.04f; s._voronoiOpacity = 0.12f;
            s._voronoiJitter = 0.6f; s._voronoiSpeed = 0.2f;
            s._useSceneEdge = true; s._sceneEdgeWidth = 1f;
            s._sceneEdgeThreshold = 0.03f; s._sceneEdgeSharpness = 10f;
            s._contourWidth = 1f; s._contourThreshold = 0.01f;
            s._contourSharpness = 18f; s._contourOpacity = 1f;
            s._lineThreshold = 0.04f; s._lineThickness = 0.9f; s._lineSoftness = 0.01f;
            s._useDitherContour = true; s._ditherStrength = 0.25f;
            s._useColorBleed = false; s._useSpecularEdge = false;
            s._usePixelHighlights = false; s._useMultiBand = false;
        }

        private static void ApplyPixelStandard(WaterLayerMaterialSettings s)
        {
            s._pixelSize = 0.5f; s._pixelMinSize = 0.5f; s._pixelAspect = Vector2.one;
            s._noiseScale = 16f; s._noiseSpeed = 8f; s._noiseStrength = 0.0035f;
            s._usePixelNoise = true;
            s._usePixelScroll = true; s._scrollDir = new Vector4(0.6f, 0.2f, 0f, 0f);
            s._scrollSpeed = 1.5f; s._usePixelRipple = true; s._rippleFreq = 12f;
            s._rippleSpeed = 3f; s._rippleAmp = 0.004f;
            s._colorSteps = 12f; s._contourSteps = 3f;
            s._useVoronoiOverlay = true;
            s._voronoiScale = 48f; s._voronoiLineWidth = 0.06f; s._voronoiOpacity = 0.18f;
            s._voronoiJitter = 0.8f; s._voronoiSpeed = 0.35f;
            s._useSceneEdge = true; s._sceneEdgeWidth = 1f;
            s._sceneEdgeThreshold = 0.02f; s._sceneEdgeSharpness = 12f;
            s._contourWidth = 1.2f; s._contourThreshold = 0.012f;
            s._contourSharpness = 20f; s._contourOpacity = 1f;
            s._lineThreshold = 0.035f; s._lineThickness = 0.85f; s._lineSoftness = 0.008f;
            s._useDitherContour = true; s._ditherStrength = 0.28f;
            s._useColorBleed = true; s._bleedStrength = 0.15f; s._bleedWidth = 1.2f;
            s._useSpecularEdge = true; s._specEdgeThreshold = 0.75f; s._specEdgeIntensity = 0.5f;
            s._usePixelHighlights = false; s._useMultiBand = false;
        }

        private static void ApplyPixelHighQuality(WaterLayerMaterialSettings s)
        {
            s._pixelSize = 0.5f; s._pixelMinSize = 0.25f; s._pixelAspect = Vector2.one;
            s._noiseScale = 64f; s._noiseSpeed = 10f; s._noiseStrength = 0.0045f;
            s._usePixelNoise = true;
            s._usePixelScroll = true; s._scrollSpeed = 2f;
            s._usePixelRipple = true; s._rippleFreq = 14f; s._rippleSpeed = 4f;
            s._rippleAmp = 0.006f; s._colorSteps = 10f; s._contourSteps = 4f;
            s._useVoronoiOverlay = true;
            s._voronoiScale = 96f; s._voronoiLineWidth = 0.03f; s._voronoiOpacity = 0.12f;
            s._voronoiJitter = 1f; s._voronoiSpeed = 0.45f;
            s._useMultiBand = true; s._bandWidthMid = 2f; s._bandWidthOuter = 4f;
            s._contourColorMid = new Color(0.6f, 0.85f, 1f, 0.6f);
            s._contourColorOuter = new Color(0.4f, 0.75f, 1f, 0.3f);
            s._useDitherContour = true; s._ditherStrength = 0.35f;
            s._usePixelHighlights = true; s._highlightDitherThreshold = 0.88f;
            s._highlightIntensity = 0.6f; s._useColorBleed = true;
            s._bleedStrength = 0.25f; s._useSpecularEdge = false;
        }
    }
}
