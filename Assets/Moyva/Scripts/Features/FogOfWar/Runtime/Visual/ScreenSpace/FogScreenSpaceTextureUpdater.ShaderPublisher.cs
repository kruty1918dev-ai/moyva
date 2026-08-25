using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogScreenSpaceTextureUpdater
    {
        private void UploadImmediately()
        {
            if (!_isDirty
                || _texture == null
                || _pixels == null)
            {
                return;
            }

            _texture.SetPixels32(
                _pixels);

            _texture.Apply(
                false,
                false);

            _isDirty =
                false;
        }

        private void PublishShaderGlobals()
        {
            FogScreenSpaceSettings screenSettings =
                ResolveScreenSettings();

            ResolveWorldToGridTransform(
                out Vector3 gridOrigin,
                out Vector4 worldToGrid);

            Shader.SetGlobalTexture(
                FogTextureId,
                _texture != null
                    ? _texture
                    : Texture2D.blackTexture);

            Shader.SetGlobalVector(
                FogMapSizeId,
                new Vector4(
                    _width,
                    _height,
                    1f / Mathf.Max(
                        1,
                        _width),
                    1f / Mathf.Max(
                        1,
                        _height)));

            Shader.SetGlobalVector(
                FogGridOriginId,
                new Vector4(
                    gridOrigin.x,
                    gridOrigin.y,
                    gridOrigin.z,
                    0f));

            Shader.SetGlobalVector(
                FogWorldToGridId,
                worldToGrid);

            bool screenSpaceEnabled =
                screenSettings.Enabled
                && (_settings == null
                    || _settings.PresentationMode
                    == FogVisualPresentationMode.ScreenSpace);

            Shader.SetGlobalFloat(
                FogEnabledId,
                screenSpaceEnabled
                    ? 1f
                    : 0f);

            Shader.SetGlobalFloat(
                FogFlipYId,
                screenSettings.FlipTextureY
                    ? 1f
                    : 0f);

            Shader.SetGlobalColor(
                UnexploredColorId,
                screenSettings.UnexploredColor);

            Shader.SetGlobalColor(
                ExploredColorId,
                screenSettings.ExploredColor);

            Shader.SetGlobalFloat(
                UnexploredOpacityId,
                screenSettings.UnexploredOpacity);

            Shader.SetGlobalFloat(
                ExploredOpacityId,
                screenSettings.ExploredOpacity);

            Shader.SetGlobalFloat(
                UnexploredSaturationId,
                screenSettings.UnexploredSaturation);

            Shader.SetGlobalFloat(
                ExploredSaturationId,
                screenSettings.ExploredSaturation);

            Shader.SetGlobalFloat(
                EdgeSoftnessId,
                screenSettings.EdgeSoftness);

            Shader.SetGlobalFloat(
                EdgeNoiseStrengthId,
                screenSettings.EdgeNoiseStrength);

            Shader.SetGlobalFloat(
                DepthAwareEdgeEnabledId,
                screenSettings.DepthAwareEdgeEnabled
                    ? 1f
                    : 0f);

            Shader.SetGlobalColor(
                DepthAwareEdgeColorId,
                screenSettings.DepthAwareEdgeColor);

            Shader.SetGlobalFloat(
                DepthAwareEdgeOpacityId,
                screenSettings.DepthAwareEdgeOpacity);

            Shader.SetGlobalFloat(
                DepthAwareEdgeWorldDepthId,
                screenSettings.DepthAwareEdgeWorldDepth);

            Shader.SetGlobalFloat(
                DepthAwareEdgeMinPixelsId,
                screenSettings.DepthAwareEdgeMinPixels);

            Shader.SetGlobalFloat(
                DepthAwareEdgeMaxPixelsId,
                screenSettings.DepthAwareEdgeMaxPixels);

            Shader.SetGlobalFloat(
                DepthAwareEdgeSamplesId,
                screenSettings.DepthAwareEdgeSamples);

            Shader.SetGlobalFloat(
                DepthAwareEdgeOcclusionBiasId,
                screenSettings.DepthAwareEdgeOcclusionBias);

            Shader.SetGlobalFloat(
                DepthAwareEdgeGradientPowerId,
                screenSettings.DepthAwareEdgeGradientPower);


        }

    }
}
