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
    internal sealed class FogScreenSpaceTextureUpdater
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

        public void Initialize(
            int width,
            int height,
            FogWorldVisualContext context)
        {
            _width =
                Mathf.Max(
                    1,
                    width);

            _height =
                Mathf.Max(
                    1,
                    height);

            _context =
                context.IsValid
                    ? context.WithSize(
                        _width,
                        _height)
                    : context;

            EnsureTexture(
                _width,
                _height);

            FillBuffer(
                _committedPixels,
                UnexploredValue);

            CopyCommittedToVisual();

            CommitVisualState(
                "Initialize");
        }

        public void SetWorldContext(
            FogWorldVisualContext context)
        {
            SetWorldContextInternal(
                context,
                true);
        }

        public void PreviewRevealArea(
            Vector2Int center,
            int radius,
            FogRevealShape shape,
            bool keepVisible)
        {
            EnsureTexture(
                _width,
                _height);

            /*
             * Preview більше не скидає texture в Unexplored.
             *
             * Старий Fill(UnexploredValue) руйнував синхронізацію:
             * FogOfWarService вже вважав клітинки Visible, але visual
             * buffer повертав їх у Unexplored. Наступний incremental
             * update надсилав лише реально змінені клітинки, тому
             * скинуті preview-клітинки лишались дірками всередині
             * відкритої області.
             */
            CopyCommittedToVisual();

            int safeRadius =
                Mathf.Max(
                    0,
                    radius);

            int previewTouched = 0;
            int previewChanged = 0;

            for (int y =
                     center.y - safeRadius;
                 y <=
                     center.y + safeRadius;
                 y++)
            {
                for (int x =
                         center.x - safeRadius;
                     x <=
                         center.x + safeRadius;
                     x++)
                {
                    var tile =
                        new Vector2Int(
                            x,
                            y);

                    if (!IsInBounds(tile))
                        continue;

                    if (!IsInsideShape(
                            tile,
                            center,
                            safeRadius,
                            shape))
                    {
                        continue;
                    }

                    previewTouched++;

                    int index =
                        tile.x
                        + tile.y * _width;

                    Color32 before =
                        _pixels[index];

                    if (!AreEqual(
                            before,
                            VisibleValue))
                    {
                        previewChanged++;
                    }

                    SetPixelValue(
                        tile,
                        VisibleValue);
                }
            }

            /*
             * keepVisible=true означає union із committed state.
             * Копіювання committed -> visual уже гарантує це.
             *
             * keepVisible=false теж лишається non-destructive:
             * preview не має права змінювати gameplay-authority.
             */
            _previewActive = true;
            _previewSequence++;

            LogStateSynchronization(
                "PreviewRevealArea",
                keepVisible,
                center,
                safeRadius,
                shape,
                previewTouched,
                previewChanged);

            CommitVisualState(
                "PreviewRevealArea");
        }

        public void UpdateDirtyTiles(
            IFogOfWarService fogService,
            IEnumerable<Vector2Int> dirtyTiles)
        {
            if (fogService == null
                || dirtyTiles == null)
            {
                return;
            }

            EnsureTexture(
                _width,
                _height);

            foreach (Vector2Int tile
                     in dirtyTiles)
            {
                if (!IsInBounds(tile))
                    continue;

                SetCommittedPixelValue(
                    tile,
                    EncodeState(
                        fogService.GetFogState(
                            tile)));
            }

            EndPreviewAndCopyCommittedToVisual();

            LogStateSynchronization(
                "UpdateDirtyTiles",
                false,
                default,
                0,
                default,
                0,
                0);

            CommitVisualState(
                "UpdateDirtyTiles");
        }

        public void RequestCellsUpdate(
            IFogOfWarService fogService,
            IReadOnlyList<FogCellVisualChange> changes,
            FogWorldVisualContext context)
        {
            if (context.IsValid)
            {
                SetWorldContextInternal(
                    context,
                    false);
            }

            if (changes == null
                || changes.Count == 0)
            {
                EndPreviewAndCopyCommittedToVisual();

                LogStateSynchronization(
                    "RequestCellsUpdate.Empty",
                    false,
                    default,
                    0,
                    default,
                    0,
                    0);

                CommitVisualState(
                    "RequestCellsUpdate.Empty");

                return;
            }

            EnsureTexture(
                _width,
                _height);

            for (int i = 0;
                 i < changes.Count;
                 i++)
            {
                FogCellVisualChange change =
                    changes[i];

                if (!change.HasVisualDelta
                    || !IsInBounds(
                        change.Cell))
                {
                    continue;
                }

                FogStateType state =
                    fogService != null
                        ? fogService.GetFogState(
                            change.Cell)
                        : change.NewState;

                SetCommittedPixelValue(
                    change.Cell,
                    EncodeState(state));
            }

            EndPreviewAndCopyCommittedToVisual();

            LogStateSynchronization(
                "RequestCellsUpdate",
                false,
                default,
                0,
                default,
                changes.Count,
                0);

            CommitVisualState(
                "RequestCellsUpdate");
        }

        public void RebuildFullVisual(
            IFogOfWarService fogService)
        {
            if (fogService == null)
                return;

            EnsureTexture(
                _width,
                _height);

            for (int y = 0;
                 y < _height;
                 y++)
            {
                for (int x = 0;
                     x < _width;
                     x++)
                {
                    var tile =
                        new Vector2Int(
                            x,
                            y);

                    SetCommittedPixelValue(
                        tile,
                        EncodeState(
                            fogService.GetFogState(
                                tile)));
                }
            }

            EndPreviewAndCopyCommittedToVisual();

            LogStateSynchronization(
                "RebuildFullVisual",
                false,
                default,
                0,
                default,
                _width * _height,
                0);

            CommitVisualState(
                "RebuildFullVisual");
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            Shader.SetGlobalFloat(
                FogEnabledId,
                0f);

            Shader.SetGlobalTexture(
                FogTextureId,
                Texture2D.blackTexture);

            _curtainRenderer.Dispose();

            DestroyTexture();

            _committedPixels = null;
            _pixels = null;
        }

        private void SetWorldContextInternal(
            FogWorldVisualContext context,
            bool commit)
        {
            if (!context.IsValid)
                return;

            bool sizeChanged =
                context.Width != _width
                || context.Height != _height;

            _context =
                context;

            if (sizeChanged)
            {
                _width =
                    Mathf.Max(
                        1,
                        context.Width);

                _height =
                    Mathf.Max(
                        1,
                        context.Height);

                EnsureTexture(
                    _width,
                    _height);

                FillBuffer(
                    _committedPixels,
                    UnexploredValue);

                CopyCommittedToVisual();
            }

            if (commit)
            {
                LogStateSynchronization(
                    "SetWorldContext",
                    false,
                    default,
                    0,
                    default,
                    0,
                    0);

                CommitVisualState(
                    "SetWorldContext");
            }
        }

        private void CommitVisualState(
            string source)
        {
            FogScreenSpaceSettings screenSettings =
                ResolveScreenSettings();

            UploadImmediately();
            PublishShaderGlobals();

            if (screenSettings.UseLegacyWorldCurtain
                && screenSettings.CurtainEnabled)
            {
                _curtainRenderer.Rebuild(
                    _pixels,
                    _width,
                    _height,
                    _context);
            }
            else
            {
                _curtainRenderer.ClearPresentation();
            }

            int shaderStateHash =
                ComputeFogStateHash();

            bool shaderStateChanged =
                shaderStateHash
                != _lastShaderStateHash;

            _lastShaderStateHash =
                shaderStateHash;

            LogShaderDiagnosticsIfNeeded(
                shaderStateChanged,
                shaderStateHash);

            if (ResolveScreenSettings()
                    ?.LogShaderDiagnostics
                == true)
            {
                Debug.Log(
                    StateSyncDiagnosticPrefix
                    + " COMMIT source="
                    + source
                    + " previewActive="
                    + _previewActive
                    + " previewSequence="
                    + _previewSequence
                    + " stateHash="
                    + shaderStateHash);
            }
        }

        private void EnsureTexture(
            int width,
            int height)
        {
            width =
                Mathf.Max(
                    1,
                    width);

            height =
                Mathf.Max(
                    1,
                    height);

            if (_texture != null
                && _texture.width == width
                && _texture.height == height
                && _committedPixels != null
                && _committedPixels.Length
                == width * height
                && _pixels != null
                && _pixels.Length
                == width * height)
            {
                return;
            }

            DestroyTexture();

            _texture =
                new Texture2D(
                    width,
                    height,
                    TextureFormat.RGBA32,
                    false,
                    true)
                {
                    name =
                        "Moyva_FogStateTexture",

                    filterMode =
                        FilterMode.Point,

                    wrapMode =
                        TextureWrapMode.Clamp,

                    anisoLevel =
                        0,

                    hideFlags =
                        HideFlags.DontSave
                };

            _committedPixels =
                new Color32[
                    width * height];

            _pixels =
                new Color32[
                    width * height];

            FillBuffer(
                _committedPixels,
                UnexploredValue);

            CopyCommittedToVisual();

            _previewActive =
                false;

            _isDirty =
                true;
        }

        private void DestroyTexture()
        {
            if (_texture == null)
                return;

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(
                    _texture);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(
                    _texture);
            }

            _texture =
                null;
        }

        private static void FillBuffer(
            Color32[] buffer,
            Color32 value)
        {
            if (buffer == null)
                return;

            for (int i = 0;
                 i < buffer.Length;
                 i++)
            {
                buffer[i] =
                    value;
            }
        }

        private void CopyCommittedToVisual()
        {
            if (_committedPixels == null
                || _pixels == null
                || _committedPixels.Length
                != _pixels.Length)
            {
                return;
            }

            Array.Copy(
                _committedPixels,
                _pixels,
                _committedPixels.Length);

            _isDirty =
                true;
        }

        private void EndPreviewAndCopyCommittedToVisual()
        {
            _previewActive =
                false;

            CopyCommittedToVisual();
        }

        private void SetCommittedPixelValue(
            Vector2Int tile,
            Color32 value)
        {
            SetBufferPixelValue(
                _committedPixels,
                tile,
                value,
                false);
        }

        private void SetPixelValue(
            Vector2Int tile,
            Color32 value)
        {
            SetBufferPixelValue(
                _pixels,
                tile,
                value,
                true);
        }

        private void SetBufferPixelValue(
            Color32[] buffer,
            Vector2Int tile,
            Color32 value,
            bool markVisualDirty)
        {
            if (buffer == null
                || !IsInBounds(tile))
            {
                return;
            }

            int index =
                tile.x
                + tile.y * _width;

            Color32 current =
                buffer[index];

            if (AreEqual(
                    current,
                    value))
            {
                return;
            }

            buffer[index] =
                value;

            if (markVisualDirty)
            {
                _isDirty =
                    true;
            }
        }

        private static bool AreEqual(
            Color32 a,
            Color32 b)
        {
            return a.r == b.r
                   && a.g == b.g
                   && a.b == b.b
                   && a.a == b.a;
        }

        private void LogStateSynchronization(
            string source,
            bool keepVisible,
            Vector2Int previewCenter,
            int previewRadius,
            FogRevealShape previewShape,
            int operationCells,
            int visualChanges)
        {
            FogScreenSpaceSettings screenSettings =
                ResolveScreenSettings();

            if (screenSettings == null
                || (!screenSettings.LogCurtainDiagnostics
                    && !screenSettings.LogShaderDiagnostics))
            {
                return;
            }

            CountBufferStates(
                _committedPixels,
                out int committedVisible,
                out int committedExplored,
                out int committedUnexplored);

            CountBufferStates(
                _pixels,
                out int visualVisible,
                out int visualExplored,
                out int visualUnexplored);

            int mismatches =
                CountBufferMismatches(
                    _committedPixels,
                    _pixels);

            Debug.Log(
                StateSyncDiagnosticPrefix
                + " source="
                + source
                + " previewActive="
                + _previewActive
                + " keepVisible="
                + keepVisible
                + " previewCenter="
                + previewCenter
                + " previewRadius="
                + previewRadius
                + " previewShape="
                + previewShape
                + " operationCells="
                + operationCells
                + " visualChanges="
                + visualChanges
                + " committed[V,E,U]="
                + committedVisible
                + ","
                + committedExplored
                + ","
                + committedUnexplored
                + " visual[V,E,U]="
                + visualVisible
                + ","
                + visualExplored
                + ","
                + visualUnexplored
                + " mismatches="
                + mismatches);
        }

        private static void CountBufferStates(
            Color32[] buffer,
            out int visible,
            out int explored,
            out int unexplored)
        {
            visible = 0;
            explored = 0;
            unexplored = 0;

            if (buffer == null)
                return;

            for (int i = 0;
                 i < buffer.Length;
                 i++)
            {
                Color32 value =
                    buffer[i];

                if (value.g >= 128)
                {
                    unexplored++;
                }
                else if (value.r >= 128)
                {
                    explored++;
                }
                else
                {
                    visible++;
                }
            }
        }

        private static int CountBufferMismatches(
            Color32[] authoritative,
            Color32[] visual)
        {
            if (authoritative == null
                || visual == null
                || authoritative.Length
                != visual.Length)
            {
                return -1;
            }

            int mismatches = 0;

            for (int i = 0;
                 i < authoritative.Length;
                 i++)
            {
                if (!AreEqual(
                        authoritative[i],
                        visual[i]))
                {
                    mismatches++;
                }
            }

            return mismatches;
        }

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


            Shader.SetGlobalFloat(
                DepthAwareStateCloseRadiusId,
                screenSettings.DepthAwareStateCloseRadiusPixels);

            Shader.SetGlobalFloat(
                DepthAwareBoundarySoftnessId,
                screenSettings.DepthAwareBoundarySoftnessPixels);

            Shader.SetGlobalFloat(
                DebugModeId,
                (float)screenSettings.ShaderDebugMode);

            Shader.SetGlobalFloat(
                DebugGridLineWidthId,
                screenSettings.ShaderDebugGridLineWidthPixels);
        }

        private int ComputeFogStateHash()
        {
            if (_pixels == null)
                return 0;

            unchecked
            {
                int hash = 17;

                /*
                 * Повний hash для 80x80 = лише 6400 дешевих ітерацій.
                 * Він виконується тільки під час visual commit.
                 */
                for (int i = 0;
                     i < _pixels.Length;
                     i++)
                {
                    Color32 pixel =
                        _pixels[i];

                    hash =
                        hash * 31
                        + pixel.r;

                    hash =
                        hash * 31
                        + pixel.g;
                }

                return hash;
            }
        }

        private void LogShaderDiagnosticsIfNeeded(
            bool force,
            int stateHash)
        {
            FogScreenSpaceSettings screenSettings =
                ResolveScreenSettings();

            if (!screenSettings.LogShaderDiagnostics)
                return;

            float now =
                Time.realtimeSinceStartup;

            if (!force
                && now - _lastShaderDiagnosticTime
                < screenSettings.DiagnosticLogIntervalSeconds)
            {
                return;
            }

            _lastShaderDiagnosticTime =
                now;

            _shaderDiagnosticSequence++;

            ResolveWorldToGridTransform(
                out Vector3 gridOrigin,
                out Vector4 worldToGrid);

            Camera camera =
                Camera.main;

            var builder =
                new StringBuilder(3072);

            builder.Append(
                "[MOYVA_FOG_SHADER_DIAG] ");

            builder.Append(
                "seq=");

            builder.Append(
                _shaderDiagnosticSequence);

            builder.Append(
                " stateHash=");

            builder.Append(
                stateHash);

            builder.Append(
                " forcedByStateChange=");

            builder.Append(
                force);

            builder.Append(
                " map=");

            builder.Append(
                _width);

            builder.Append(
                "x");

            builder.Append(
                _height);

            builder.Append(
                " debugMode=");

            builder.Append(
                screenSettings.ShaderDebugMode);

            builder.Append(
                " flipY=");

            builder.Append(
                screenSettings.FlipTextureY);

            builder.Append(
                " gridOriginXZ=(");

            builder.Append(
                gridOrigin.x.ToString("F3"));

            builder.Append(
                ",");

            builder.Append(
                gridOrigin.y.ToString("F3"));

            builder.Append(
                ") fallbackPlaneY=");

            builder.Append(
                gridOrigin.z.ToString("F3"));

            builder.Append(
                " transparentFallbackOffsetY=");

            builder.Append(
                screenSettings
                    .TransparentFallbackPlaneOffsetY
                    .ToString("F3"));

            builder.Append(
                " worldToGrid=(");

            builder.Append(
                worldToGrid.x.ToString("F6"));

            builder.Append(
                ",");

            builder.Append(
                worldToGrid.y.ToString("F6"));

            builder.Append(
                ",");

            builder.Append(
                worldToGrid.z.ToString("F6"));

            builder.Append(
                ",");

            builder.Append(
                worldToGrid.w.ToString("F6"));

            builder.Append(
                ") edgeSoftness=");

            builder.Append(
                screenSettings.EdgeSoftness.ToString("F3"));


            builder.Append(
                " presentation=DepthAwareScreenSpace");

            builder.Append(
                " virtualDepth=");

            builder.Append(
                screenSettings.DepthAwareEdgeEnabled);

            builder.Append(
                " closeRadiusPx=");

            builder.Append(
                screenSettings.DepthAwareStateCloseRadiusPixels);

            builder.Append(
                " boundarySoftnessPx=");

            builder.Append(
                screenSettings.DepthAwareBoundarySoftnessPixels
                    .ToString("F2"));

            builder.Append(
                " legacyCurtain=");

            builder.Append(
                screenSettings.UseLegacyWorldCurtain);

            if (camera == null)
            {
                builder.Append(
                    " camera=null");

                Debug.Log(
                    builder.ToString());

                return;
            }

            builder.Append(
                " camera=");

            builder.Append(
                camera.name);

            builder.Append(
                " projection=");

            builder.Append(
                camera.orthographic
                    ? "Orthographic"
                    : "Perspective");

            builder.Append(
                " position=");

            builder.Append(
                FormatVector3(
                    camera.transform.position));

            builder.Append(
                " rotation=");

            builder.Append(
                FormatVector3(
                    camera.transform.eulerAngles));

            builder.Append(
                " fov=");

            builder.Append(
                camera.fieldOfView.ToString("F2"));

            builder.Append(
                " orthoSize=");

            builder.Append(
                camera.orthographicSize.ToString("F2"));

            builder.Append(
                " near=");

            builder.Append(
                camera.nearClipPlane.ToString("F3"));

            builder.Append(
                " far=");

            builder.Append(
                camera.farClipPlane.ToString("F1"));

            builder.Append(
                " pixel=");

            builder.Append(
                camera.pixelWidth);

            builder.Append(
                "x");

            builder.Append(
                camera.pixelHeight);

            builder.AppendLine();

            AppendShaderProbe(
                builder,
                camera,
                "center",
                new Vector2(
                    0.5f,
                    0.5f),
                gridOrigin,
                worldToGrid,
                screenSettings);

            AppendShaderProbe(
                builder,
                camera,
                "upper",
                new Vector2(
                    0.5f,
                    0.8f),
                gridOrigin,
                worldToGrid,
                screenSettings);

            AppendShaderProbe(
                builder,
                camera,
                "lower",
                new Vector2(
                    0.5f,
                    0.2f),
                gridOrigin,
                worldToGrid,
                screenSettings);

            AppendShaderProbe(
                builder,
                camera,
                "left",
                new Vector2(
                    0.2f,
                    0.5f),
                gridOrigin,
                worldToGrid,
                screenSettings);

            AppendShaderProbe(
                builder,
                camera,
                "right",
                new Vector2(
                    0.8f,
                    0.5f),
                gridOrigin,
                worldToGrid,
                screenSettings);

            Debug.Log(
                builder.ToString());
        }

        private void AppendShaderProbe(
            StringBuilder builder,
            Camera camera,
            string label,
            Vector2 viewport,
            Vector3 gridOrigin,
            Vector4 worldToGrid,
            FogScreenSpaceSettings screenSettings)
        {
            Ray ray =
                camera.ViewportPointToRay(
                    new Vector3(
                        viewport.x,
                        viewport.y,
                        0f));

            builder.Append(
                " probe=");

            builder.Append(
                label);

            if (Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    screenSettings.ShaderDiagnosticRaycastDistance,
                    screenSettings.ShaderDiagnosticRaycastMask,
                    QueryTriggerInteraction.Ignore))
            {
                Vector2 grid =
                    ConvertWorldToGrid(
                        hit.point,
                        gridOrigin,
                        worldToGrid,
                        screenSettings.FlipTextureY);

                Vector2Int cell =
                    new Vector2Int(
                        Mathf.RoundToInt(
                            grid.x),
                        Mathf.RoundToInt(
                            grid.y));

                Renderer hitRenderer =
                    hit.collider != null
                        ? hit.collider
                            .GetComponentInParent<Renderer>()
                        : null;

                Material material =
                    hitRenderer != null
                        ? hitRenderer.sharedMaterial
                        : null;

                builder.Append(
                    " hitCollider=");

                builder.Append(
                    hit.collider != null
                        ? hit.collider.name
                        : "unknown");

                builder.Append(
                    " renderer=");

                builder.Append(
                    hitRenderer != null
                        ? hitRenderer.name
                        : "null");

                builder.Append(
                    " shader=");

                builder.Append(
                    material != null
                    && material.shader != null
                        ? material.shader.name
                        : "null");

                builder.Append(
                    " world=");

                builder.Append(
                    FormatVector3(
                        hit.point));

                builder.Append(
                    " grid=(");

                builder.Append(
                    grid.x.ToString("F2"));

                builder.Append(
                    ",");

                builder.Append(
                    grid.y.ToString("F2"));

                builder.Append(
                    ") cell=");

                builder.Append(
                    cell);

                builder.Append(
                    " state=");

                builder.Append(
                    ResolveStateLabel(
                        cell));
            }
            else if (TryIntersectPlane(
                         ray,
                         gridOrigin.z,
                         out Vector3 planePoint))
            {
                Vector2 grid =
                    ConvertWorldToGrid(
                        planePoint,
                        gridOrigin,
                        worldToGrid,
                        screenSettings.FlipTextureY);

                Vector2Int cell =
                    new Vector2Int(
                        Mathf.RoundToInt(
                            grid.x),
                        Mathf.RoundToInt(
                            grid.y));

                builder.Append(
                    " hitCollider=none planeWorld=");

                builder.Append(
                    FormatVector3(
                        planePoint));

                builder.Append(
                    " grid=(");

                builder.Append(
                    grid.x.ToString("F2"));

                builder.Append(
                    ",");

                builder.Append(
                    grid.y.ToString("F2"));

                builder.Append(
                    ") cell=");

                builder.Append(
                    cell);

                builder.Append(
                    " state=");

                builder.Append(
                    ResolveStateLabel(
                        cell));
            }
            else
            {
                builder.Append(
                    " hitCollider=none plane=miss");
            }

            builder.AppendLine();
        }

        private Vector2 ConvertWorldToGrid(
            Vector3 world,
            Vector3 gridOrigin,
            Vector4 worldToGrid,
            bool flipY)
        {
            Vector2 delta =
                new Vector2(
                    world.x - gridOrigin.x,
                    world.z - gridOrigin.y);

            Vector2 grid =
                new Vector2(
                    delta.x * worldToGrid.x
                        + delta.y * worldToGrid.y,

                    delta.x * worldToGrid.z
                        + delta.y * worldToGrid.w);

            if (flipY)
            {
                grid.y =
                    (_height - 1)
                    - grid.y;
            }

            return grid;
        }

        private string ResolveStateLabel(
            Vector2Int cell)
        {
            if (_pixels == null
                || !IsInBounds(
                    cell))
            {
                return "Outside";
            }

            Color32 pixel =
                _pixels[
                    cell.x
                    + cell.y * _width];

            if (pixel.g >= 128)
                return "Unexplored";

            if (pixel.r >= 128)
                return "Explored";

            return "Visible";
        }

        private static bool TryIntersectPlane(
            Ray ray,
            float planeY,
            out Vector3 point)
        {
            point =
                Vector3.zero;

            if (Mathf.Abs(
                    ray.direction.y)
                <= 0.00001f)
            {
                return false;
            }

            float distance =
                (planeY - ray.origin.y)
                / ray.direction.y;

            if (distance <= 0f)
                return false;

            point =
                ray.origin
                + ray.direction * distance;

            return true;
        }

        private static string FormatVector3(
            Vector3 value)
        {
            return "("
                   + value.x.ToString("F3")
                   + ","
                   + value.y.ToString("F3")
                   + ","
                   + value.z.ToString("F3")
                   + ")";
        }

        private float ResolveTransparentFallbackPlaneY(
            float logicalOriginY)
        {
            FogScreenSpaceSettings screenSettings =
                ResolveScreenSettings();

            return logicalOriginY
                   + screenSettings
                       .TransparentFallbackPlaneOffsetY;
        }

        private void ResolveWorldToGridTransform(
            out Vector3 gridOrigin,
            out Vector4 worldToGrid)
        {
            if (_gridProjection != null)
            {
                Vector3 originWorld =
                    _gridProjection.GridToWorld(
                        Vector2Int.zero);

                Vector3 xWorld =
                    _gridProjection.GridToWorld(
                        Vector2Int.right);

                Vector3 yWorld =
                    _gridProjection.GridToWorld(
                        Vector2Int.up);

                Vector2 xBasis =
                    new Vector2(
                        xWorld.x
                            - originWorld.x,
                        xWorld.z
                            - originWorld.z);

                Vector2 yBasis =
                    new Vector2(
                        yWorld.x
                            - originWorld.x,
                        yWorld.z
                            - originWorld.z);

                float determinant =
                    xBasis.x * yBasis.y
                    - xBasis.y * yBasis.x;

                if (Mathf.Abs(
                        determinant)
                    > 0.000001f)
                {
                    float inverseDeterminant =
                        1f / determinant;

                    gridOrigin =
                        new Vector3(
                            originWorld.x,
                            originWorld.z,
                            ResolveTransparentFallbackPlaneY(
                                originWorld.y));

                    worldToGrid =
                        new Vector4(
                            yBasis.y
                                * inverseDeterminant,

                            -yBasis.x
                                * inverseDeterminant,

                            -xBasis.y
                                * inverseDeterminant,

                            xBasis.x
                                * inverseDeterminant);

                    return;
                }
            }

            if (_context.IsValid
                && _context.HasMapWorldBounds)
            {
                Bounds bounds =
                    _context.MapWorldBounds;

                float cellWidth =
                    Mathf.Max(
                        0.0001f,
                        bounds.size.x
                        / Mathf.Max(
                            1,
                            _width));

                float cellDepth =
                    Mathf.Max(
                        0.0001f,
                        bounds.size.z
                        / Mathf.Max(
                            1,
                            _height));

                gridOrigin =
                    new Vector3(
                        bounds.min.x
                            + cellWidth * 0.5f,
                        bounds.min.z
                            + cellDepth * 0.5f,
                        ResolveTransparentFallbackPlaneY(
                            0f));

                worldToGrid =
                    new Vector4(
                        1f / cellWidth,
                        0f,
                        0f,
                        1f / cellDepth);

                return;
            }

            float cellSize =
                _context.IsValid
                    ? Mathf.Max(
                        0.0001f,
                        _context.CellSize)
                    : 1f;

            gridOrigin =
                new Vector3(
                    0f,
                    0f,
                    ResolveTransparentFallbackPlaneY(
                        0f));

            worldToGrid =
                new Vector4(
                    1f / cellSize,
                    0f,
                    0f,
                    1f / cellSize);
        }

        private FogScreenSpaceSettings
            ResolveScreenSettings()
        {
            FogScreenSpaceSettings result =
                _settings != null
                    ? _settings.ScreenSpace
                    : null;

            if (result == null)
            {
                result =
                    new FogScreenSpaceSettings();
            }

            result.EnsureDefaults();

            return result;
        }

        private bool IsInBounds(
            Vector2Int tile)
        {
            return tile.x >= 0
                   && tile.x < _width
                   && tile.y >= 0
                   && tile.y < _height;
        }

        private static Color32 EncodeState(
            FogStateType state)
        {
            switch (state)
            {
                case FogStateType.Visible:
                    return VisibleValue;

                case FogStateType.Explored:
                    return ExploredValue;

                default:
                    return UnexploredValue;
            }
        }

        private static bool IsInsideShape(
            Vector2Int tile,
            Vector2Int center,
            int radius,
            FogRevealShape shape)
        {
            int dx =
                Mathf.Abs(
                    tile.x - center.x);

            int dy =
                Mathf.Abs(
                    tile.y - center.y);

            switch (shape)
            {
                case FogRevealShape.Diamond:
                    return dx + dy <= radius;

                case FogRevealShape.Square:
                    return Mathf.Max(
                               dx,
                               dy)
                           <= radius;

                default:
                    return dx * dx
                               + dy * dy
                           <= radius * radius;
            }
        }
    }
}