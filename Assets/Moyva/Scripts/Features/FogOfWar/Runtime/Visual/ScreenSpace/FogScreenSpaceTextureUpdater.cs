using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Публікує двоканальну fog texture для fullscreen pass
    /// і синхронно перебудовує world-space boundary curtain.
    ///
    /// R = hidden:
    ///     1 для Unexplored/Explored
    ///     0 для Visible
    ///
    /// G = unexplored:
    ///     1 лише для Unexplored
    ///     0 для Explored/Visible
    /// </summary>
    internal sealed partial class FogScreenSpaceTextureUpdater
        : IFogVisualUpdater,
          IDisposable
    {
        private static readonly Color32 UnexploredValue =
            new Color32(
                255,
                255,
                0,
                255);

        private static readonly Color32 ExploredValue =
            new Color32(
                255,
                0,
                0,
                255);

        private static readonly Color32 VisibleValue =
            new Color32(
                0,
                0,
                0,
                255);

        private static readonly int FogTextureId =
            Shader.PropertyToID(
                "_MoyvaFogStateTexture");

        private static readonly int FogMapSizeId =
            Shader.PropertyToID(
                "_MoyvaFogMapSize");

        private static readonly int FogGridOriginId =
            Shader.PropertyToID(
                "_MoyvaFogGridOrigin");

        private static readonly int FogWorldToGridId =
            Shader.PropertyToID(
                "_MoyvaFogWorldToGrid");

        private static readonly int FogEnabledId =
            Shader.PropertyToID(
                "_MoyvaFogEnabled");

        private static readonly int FogFlipYId =
            Shader.PropertyToID(
                "_MoyvaFogFlipY");

        private static readonly int UnexploredColorId =
            Shader.PropertyToID(
                "_MoyvaFogUnexploredColor");

        private static readonly int ExploredColorId =
            Shader.PropertyToID(
                "_MoyvaFogExploredColor");

        private static readonly int UnexploredOpacityId =
            Shader.PropertyToID(
                "_MoyvaFogUnexploredOpacity");

        private static readonly int ExploredOpacityId =
            Shader.PropertyToID(
                "_MoyvaFogExploredOpacity");

        private static readonly int UnexploredSaturationId =
            Shader.PropertyToID(
                "_MoyvaFogUnexploredSaturation");

        private static readonly int ExploredSaturationId =
            Shader.PropertyToID(
                "_MoyvaFogExploredSaturation");

        private static readonly int EdgeSoftnessId =
            Shader.PropertyToID(
                "_MoyvaFogEdgeSoftness");

        private static readonly int EdgeNoiseStrengthId =
            Shader.PropertyToID(
                "_MoyvaFogEdgeNoiseStrength");

        private static readonly int DepthAwareEdgeEnabledId =
            Shader.PropertyToID(
                "_MoyvaFogVirtualDepthEnabled");

        private static readonly int DepthAwareEdgeColorId =
            Shader.PropertyToID(
                "_MoyvaFogVirtualDepthColor");

        private static readonly int DepthAwareEdgeOpacityId =
            Shader.PropertyToID(
                "_MoyvaFogVirtualDepthOpacity");

        private static readonly int DepthAwareEdgeWorldDepthId =
            Shader.PropertyToID(
                "_MoyvaFogVirtualDepthWorld");

        private static readonly int DepthAwareEdgeMinPixelsId =
            Shader.PropertyToID(
                "_MoyvaFogVirtualDepthMinPixels");

        private static readonly int DepthAwareEdgeMaxPixelsId =
            Shader.PropertyToID(
                "_MoyvaFogVirtualDepthMaxPixels");

        private static readonly int DepthAwareEdgeSamplesId =
            Shader.PropertyToID(
                "_MoyvaFogVirtualDepthSamples");

        private static readonly int DepthAwareEdgeOcclusionBiasId =
            Shader.PropertyToID(
                "_MoyvaFogVirtualDepthOcclusionBias");

        private static readonly int DepthAwareEdgeGradientPowerId =
            Shader.PropertyToID(
                "_MoyvaFogVirtualDepthGradientPower");

        private static readonly int DepthAwareStateCloseRadiusId =
            Shader.PropertyToID(
                "_MoyvaFogScreenCloseRadiusPixels");

        private static readonly int DepthAwareBoundarySoftnessId =
            Shader.PropertyToID(
                "_MoyvaFogScreenBoundarySoftnessPixels");

        private static readonly int DebugModeId =
            Shader.PropertyToID(
                "_MoyvaFogDebugMode");

        private static readonly int DebugGridLineWidthId =
            Shader.PropertyToID(
                "_MoyvaFogDebugGridLineWidthPixels");

        private readonly FogOfWarSettings _settings;
        private readonly IGridProjection _gridProjection;
        private readonly FogBoundaryCurtainRenderer _curtainRenderer;

        private const string StateSyncDiagnosticPrefix =
            "[MOYVA_FOG_STATE_SYNC]";

        private Texture2D _texture;

        /*
         * _committedPixels = authoritative gameplay state received
         * from IFogOfWarService.
         *
         * _pixels = temporary visual state uploaded to the shader.
         * PreviewRevealArea may modify only this buffer.
         */
        private Color32[] _committedPixels;
        private Color32[] _pixels;

        private FogWorldVisualContext _context;

        private bool _previewActive;
        private int _previewSequence;

        private int _width = 1;
        private int _height = 1;

        private bool _isDirty;
        private bool _disposed;

        private float _lastShaderDiagnosticTime =
            float.NegativeInfinity;

        private int _shaderDiagnosticSequence;

        private int _lastShaderStateHash =
            int.MinValue;

        [Inject]
        public FogScreenSpaceTextureUpdater(
            [InjectOptional] FogOfWarSettings settings = null,
            [InjectOptional] IGridProjection gridProjection = null)
        {
            _settings =
                settings;

            _gridProjection =
                gridProjection;

            _curtainRenderer =
                new FogBoundaryCurtainRenderer(
                    settings,
                    gridProjection);
        }

    }
}
