using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using UnityEngine.Rendering;

using Kruty1918.Moyva.Jsonization;
namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    /// <summary>
    /// Будує одну world-space mesh-стінку вздовж логічної межі:
    ///
    /// revealed cell (Visible або Explored)
    /// проти
    /// Unexplored cell або простору поза картою.
    ///
    /// Геометрія не залежить від depth buffer, FOV чи країв екрана.
    /// </summary>
    internal sealed class FogBoundaryCurtainRenderer
        : IDisposable
    {
        private const string CurtainShaderName =
            "Moyva/FogOfWar/BoundaryCurtain";

        private const string CurtainObjectName =
            "Moyva_FogBoundaryCurtain";

        private const string DiagnosticPrefix =
            "[MOYVA_FOG_CURTAIN_DIAG]";

        private const string MaskDiagnosticPrefix =
            "[MOYVA_FOG_MASK_DIAG]";

        private static readonly int TopColorId =
            Shader.PropertyToID(
                "_TopColor");

        private static readonly int BottomColorId =
            Shader.PropertyToID(
                "_BottomColor");

        private static readonly int TopBandFractionId =
            Shader.PropertyToID(
                "_TopBandFraction");

        private static readonly int GradientPowerId =
            Shader.PropertyToID(
                "_GradientPower");

        private static readonly int DebugVisualModeId =
            Shader.PropertyToID(
                "_DebugVisualMode");

        private static readonly int CullModeId =
            Shader.PropertyToID(
                "_CullMode");


        private static readonly Vector2Int[] Directions =
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.up
        };

        private readonly FogOfWarSettings _settings;
        private readonly IGridProjection _gridProjection;

        private readonly List<Vector3> _vertices =
            new List<Vector3>(4096);

        private readonly List<Vector2> _uvs =
            new List<Vector2>(4096);

        private readonly List<Color32> _colors =
            new List<Color32>(4096);

        private readonly List<DiagnosticSegment> _diagnosticSegments =
            new List<DiagnosticSegment>(1024);

        private readonly List<float> _surfaceOffsetSamples =
            new List<float>(64);

        private readonly RaycastHit[] _surfaceProbeHits =
            new RaycastHit[16];

        private readonly List<int> _triangles =
            new List<int>(6144);

        private GameObject _root;
        private Mesh _mesh;
        private Material _material;

        private bool _shaderErrorLogged;
        private bool _disposed;

        private float _lastDiagnosticTime =
            float.NegativeInfinity;

        private int _rebuildSequence;
        private int _lastBoundaryHash =
            int.MinValue;

        public FogBoundaryCurtainRenderer(
            FogOfWarSettings settings,
            IGridProjection gridProjection)
        {
            _settings = settings;
            _gridProjection = gridProjection;
        }

        public void Rebuild(
            Color32[] fogPixels,
            int width,
            int height,
            FogWorldVisualContext context)
        {
            if (_disposed)
                return;

            FogScreenSpaceSettings screenSettings =
                ResolveSettings();

            bool screenSpaceMode =
                _settings == null
                || _settings.PresentationMode
                == FogVisualPresentationMode.ScreenSpace;

            if (!screenSpaceMode
                || !screenSettings.Enabled
                || !screenSettings.CurtainEnabled
                || fogPixels == null
                || width <= 0
                || height <= 0
                || fogPixels.Length < width * height)
            {
                ClearPresentation();
                return;
            }

            if (!EnsurePresentation())
                return;

            UpdateMaterial(screenSettings);

            ResolveGridBasis(
                width,
                height,
                context,
                out Vector3 gridOrigin,
                out Vector3 xBasis,
                out Vector3 yBasis);

            float xLength =
                HorizontalLength(
                    xBasis);

            float yLength =
                HorizontalLength(
                    yBasis);

            float referenceCellSize =
                Mathf.Max(
                    0.0001f,
                    Mathf.Min(
                        xLength,
                        yLength));

            float hiddenSideOffsetWorld =
                referenceCellSize
                * screenSettings
                    .CurtainHiddenSideOffsetCells;

            float topCapWidthWorld =
                referenceCellSize
                * screenSettings
                    .CurtainTopCapWidthCells;

            float topCapOverlapCells =
                screenSettings
                    .CurtainTopCapCornerOverlapCells;

            /*
             * Top cap тепер є вузьким gameplay lip. Внутрішні
             * прямокутники попереднього pass виникали через visual-mask
             * desync, який уже усунуто committed/preview buffers.
             */
            bool buildTopLip =
                screenSettings.CurtainTopCapEnabled;

            SurfaceCalibrationResult surfaceCalibration =
                ResolveSurfaceCalibration(
                    fogPixels,
                    width,
                    height,
                    context,
                    gridOrigin,
                    xBasis,
                    yBasis,
                    screenSettings);

            float resolvedSurfaceOffsetY =
                surfaceCalibration.OffsetY;

            ResolveLogicalSurfaceRange(
                context,
                width,
                height,
                out float minimumLogicalSurfaceHeight,
                out float maximumLogicalSurfaceHeight);

            float sharedBottomY =
                minimumLogicalSurfaceHeight
                + resolvedSurfaceOffsetY
                - screenSettings.CurtainWorldDepth
                - screenSettings.CurtainBottomPadding;

            _vertices.Clear();
            _uvs.Clear();
            _colors.Clear();
            _triangles.Clear();
            _diagnosticSegments.Clear();

            int revealedCellCount = 0;
            int unexploredCellCount = 0;
            int boundaryEdgeCount = 0;
            int outsideBoundaryCount = 0;
            int topCapCount = 0;

            int perEdgeProbeSuccessCount = 0;
            int perEdgeProbeFailureCount = 0;

            float minimumPerEdgeProbeOffset =
                float.PositiveInfinity;

            float maximumPerEdgeProbeOffset =
                float.NegativeInfinity;

            int leftEdges = 0;
            int rightEdges = 0;
            int downEdges = 0;
            int upEdges = 0;

            long revealedSumX = 0;
            long revealedSumY = 0;

            float minimumTopY =
                float.PositiveInfinity;

            float maximumTopY =
                float.NegativeInfinity;

            float minimumBottomY =
                float.PositiveInfinity;

            float maximumBottomY =
                float.NegativeInfinity;

            for (int y = 0;
                 y < height;
                 y++)
            {
                for (int x = 0;
                     x < width;
                     x++)
                {
                    if (IsUnexplored(
                            fogPixels,
                            width,
                            x,
                            y))
                    {
                        unexploredCellCount++;
                        continue;
                    }

                    revealedCellCount++;
                    revealedSumX += x;
                    revealedSumY += y;

                    var cell =
                        new Vector2Int(
                            x,
                            y);

                    Vector3 cellCenter =
                        ResolveCellCenter(
                            cell,
                            gridOrigin,
                            xBasis,
                            yBasis);

                    float revealedHeight =
                        ResolveSurfaceHeight(
                            context,
                            cell,
                            cellCenter.y);

                    for (int directionIndex = 0;
                         directionIndex < Directions.Length;
                         directionIndex++)
                    {
                        Vector2Int direction =
                            Directions[directionIndex];

                        Vector2Int neighbour =
                            cell + direction;

                        if (!IsUnexplored(
                                fogPixels,
                                width,
                                height,
                                neighbour))
                        {
                            continue;
                        }

                        Vector3 step =
                            direction.x * xBasis
                            + direction.y * yBasis;

                        step.y = 0f;

                        float stepLength =
                            step.magnitude;

                        if (stepLength <= 0.0001f)
                            continue;

                        Vector3 tangent =
                            direction.x != 0
                                ? yBasis
                                : xBasis;

                        tangent.y = 0f;

                        if (HorizontalLength(tangent)
                            <= 0.0001f)
                        {
                            continue;
                        }

                        bool neighbourOutside =
                            !IsInBounds(
                                neighbour,
                                width,
                                height);

                        Vector3 neighbourCenter =
                            cellCenter
                            + step;

                        float hiddenHeight =
                            neighbourOutside
                                ? revealedHeight
                                : ResolveSurfaceHeight(
                                    context,
                                    neighbour,
                                    neighbourCenter.y);

                        /*
                         * Height-aware overlay policy.
                         *
                         * Curtain більше не ховається через camera depth.
                         * Замість цього його верх піднімається до найвищої
                         * реальної поверхні з двох боків boundary:
                         *
                         * revealed surface <-> unexplored surface.
                         *
                         * Тому високий тайл не прорізається чорною стінкою:
                         * сама верхня межа fog і top lip переходять на його Y.
                         */
                        float revealedSurfaceY =
                            revealedHeight
                            + resolvedSurfaceOffsetY;

                        float hiddenSurfaceY =
                            hiddenHeight
                            + resolvedSurfaceOffsetY;

                        bool revealedProbeSucceeded =
                            false;

                        bool hiddenProbeSucceeded =
                            false;

                        if (screenSettings
                            .CurtainPerEdgeSurfaceProbe)
                        {
                            revealedProbeSucceeded =
                                TryProbeActualSurfaceHeight(
                                    cellCenter,
                                    revealedHeight,
                                    screenSettings,
                                    out float actualRevealedSurfaceY);

                            if (revealedProbeSucceeded)
                            {
                                revealedSurfaceY =
                                    actualRevealedSurfaceY;

                                float revealedProbeOffset =
                                    actualRevealedSurfaceY
                                    - revealedHeight;

                                minimumPerEdgeProbeOffset =
                                    Mathf.Min(
                                        minimumPerEdgeProbeOffset,
                                        revealedProbeOffset);

                                maximumPerEdgeProbeOffset =
                                    Mathf.Max(
                                        maximumPerEdgeProbeOffset,
                                        revealedProbeOffset);
                            }

                            if (!neighbourOutside)
                            {
                                hiddenProbeSucceeded =
                                    TryProbeActualSurfaceHeight(
                                        neighbourCenter,
                                        hiddenHeight,
                                        screenSettings,
                                        out float actualHiddenSurfaceY);

                                if (hiddenProbeSucceeded)
                                {
                                    hiddenSurfaceY =
                                        actualHiddenSurfaceY;

                                    float hiddenProbeOffset =
                                        actualHiddenSurfaceY
                                        - hiddenHeight;

                                    minimumPerEdgeProbeOffset =
                                        Mathf.Min(
                                            minimumPerEdgeProbeOffset,
                                            hiddenProbeOffset);

                                    maximumPerEdgeProbeOffset =
                                        Mathf.Max(
                                            maximumPerEdgeProbeOffset,
                                            hiddenProbeOffset);
                                }
                            }

                            if (revealedProbeSucceeded
                                || hiddenProbeSucceeded)
                            {
                                perEdgeProbeSuccessCount++;
                            }
                            else
                            {
                                perEdgeProbeFailureCount++;
                            }
                        }

                        float edgeSurfaceY =
                            Mathf.Max(
                                revealedSurfaceY,
                                hiddenSurfaceY);

                        ResolveBoundaryEndpointKeys(
                            cell,
                            directionIndex,
                            out Vector2Int endpointAKey,
                            out Vector2Int endpointBKey);

                        float edgeTopY =
                            edgeSurfaceY
                            + screenSettings.CurtainTopOffset;

                        float topAY =
                            edgeTopY;

                        float topBY =
                            edgeTopY;

                        /*
                         * Constant-depth curtain.
                         *
                         * Верх сегмента може підніматися до високого
                         * сусіднього тайла, але його вертикальна довжина
                         * не змінюється. Це прибирає довгі чорні стіни,
                         * які раніше тягнулися до global shared bottom.
                         */
                        float localCurtainDepth =
                            Mathf.Max(
                                0.001f,
                                screenSettings.CurtainWorldDepth
                                + screenSettings.CurtainBottomPadding);

                        float bottomAY =
                            topAY
                            - localCurtainDepth;

                        float bottomBY =
                            topBY
                            - localCurtainDepth;

                        Vector3 outward =
                            step / stepLength;

                        /*
                         * Сегмент стоїть на логічній межі та лише на
                         * крихітний epsilon зміщується в Unexplored.
                         * Раніше знак був протилежним і curtain заходив
                         * усередину відкритої клітинки.
                         */
                        Vector3 edgeCenter =
                            cellCenter
                            + step * 0.5f
                            + outward
                                * hiddenSideOffsetWorld;

                        /*
                         * Невеликий overlap закриває мікрощілини між
                         * сусідніми flat-height edges. Він працює лише
                         * в XZ і не створює діагональних Y-переходів.
                         */
                        Vector3 tangentHalf =
                            tangent
                            * (0.5f
                               + screenSettings
                                   .CurtainEdgeOverlapCells);

                        Vector3 topA =
                            edgeCenter
                            - tangentHalf;

                        Vector3 topB =
                            edgeCenter
                            + tangentHalf;

                        topA.y = topAY;
                        topB.y = topBY;

                        Vector3 bottomA = topA;
                        Vector3 bottomB = topB;

                        bottomA.y = bottomAY;
                        bottomB.y = bottomBY;

                        float segmentMinimumTopY =
                            Mathf.Min(
                                topAY,
                                topBY);

                        float segmentMaximumTopY =
                            Mathf.Max(
                                topAY,
                                topBY);

                        float segmentMinimumBottomY =
                            Mathf.Min(
                                bottomAY,
                                bottomBY);

                        float segmentMaximumBottomY =
                            Mathf.Max(
                                bottomAY,
                                bottomBY);

                        boundaryEdgeCount++;

                        if (neighbourOutside)
                            outsideBoundaryCount++;

                        switch (directionIndex)
                        {
                            case 0:
                                leftEdges++;
                                break;

                            case 1:
                                rightEdges++;
                                break;

                            case 2:
                                downEdges++;
                                break;

                            default:
                                upEdges++;
                                break;
                        }

                        minimumTopY =
                            Mathf.Min(
                                minimumTopY,
                                segmentMinimumTopY);

                        maximumTopY =
                            Mathf.Max(
                                maximumTopY,
                                segmentMaximumTopY);

                        minimumBottomY =
                            Mathf.Min(
                                minimumBottomY,
                                segmentMinimumBottomY);

                        maximumBottomY =
                            Mathf.Max(
                                maximumBottomY,
                                segmentMaximumBottomY);

                        Color32 debugColor =
                            ResolveDebugColor(
                                screenSettings,
                                cell,
                                directionIndex,
                                segmentMaximumTopY,
                                revealedHeight,
                                hiddenHeight);

                        AddQuad(
                            bottomA,
                            bottomB,
                            topB,
                            topA,
                            outward,
                            debugColor);

                        if (buildTopLip)
                        {
                            Vector3 capTangentHalf =
                                tangent
                                * (0.5f
                                   + topCapOverlapCells);

                            Vector3 capInnerA =
                                edgeCenter
                                - capTangentHalf;

                            Vector3 capInnerB =
                                edgeCenter
                                + capTangentHalf;

                            Vector3 capOuterA =
                                capInnerA
                                + outward
                                    * topCapWidthWorld;

                            Vector3 capOuterB =
                                capInnerB
                                + outward
                                    * topCapWidthWorld;

                            capInnerA.y =
                                topAY
                                + screenSettings
                                    .CurtainTopCapLift;

                            capOuterA.y =
                                capInnerA.y;

                            capInnerB.y =
                                topBY
                                + screenSettings
                                    .CurtainTopCapLift;

                            capOuterB.y =
                                capInnerB.y;

                            AddTopCapQuad(
                                capInnerA,
                                capInnerB,
                                capOuterB,
                                capOuterA,
                                debugColor);

                            topCapCount++;
                        }

                        _diagnosticSegments.Add(
                            new DiagnosticSegment(
                                cell,
                                neighbour,
                                directionIndex,
                                cellCenter,
                                revealedHeight,
                                hiddenHeight,
                                topAY,
                                topBY,
                                bottomAY,
                                bottomBY,
                                endpointAKey,
                                endpointBKey,
                                neighbourOutside));
                    }
                }
            }

            ApplyMesh();

            Vector2 revealedCenter =
                revealedCellCount > 0
                    ? new Vector2(
                        (float)revealedSumX
                            / revealedCellCount,
                        (float)revealedSumY
                            / revealedCellCount)
                    : new Vector2(
                        (width - 1) * 0.5f,
                        (height - 1) * 0.5f);

            LogDiagnosticsIfNeeded(
                screenSettings,
                fogPixels,
                width,
                height,
                context,
                gridOrigin,
                xBasis,
                yBasis,
                referenceCellSize,
                revealedCellCount,
                unexploredCellCount,
                boundaryEdgeCount,
                outsideBoundaryCount,
                topCapCount,
                buildTopLip,
                surfaceCalibration,
                minimumLogicalSurfaceHeight,
                maximumLogicalSurfaceHeight,
                sharedBottomY,
                leftEdges,
                rightEdges,
                downEdges,
                upEdges,
                minimumTopY,
                maximumTopY,
                minimumBottomY,
                maximumBottomY,
                perEdgeProbeSuccessCount,
                perEdgeProbeFailureCount,
                minimumPerEdgeProbeOffset,
                maximumPerEdgeProbeOffset,
                revealedCenter);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            FogBoundaryCurtainRendererFeature
                .ClearDrawData(
                    _mesh);

            MoyvaJsonObjectFactory.DestroyImmediate(_material);
            MoyvaJsonObjectFactory.DestroyImmediate(_mesh);
            MoyvaJsonObjectFactory.DestroyImmediate(_root);
            _material = null;
            _mesh = null;
            _root = null;

            _vertices.Clear();
            _uvs.Clear();
            _colors.Clear();
            _triangles.Clear();
            _diagnosticSegments.Clear();
            _surfaceOffsetSamples.Clear();
        }

        private bool EnsurePresentation()
        {
            if (_root == null)
            {
                _root =
                    new GameObject(
                        CurtainObjectName)
                    {
                        hideFlags =
                            HideFlags.DontSave
                    };

                _root.transform.SetPositionAndRotation(
                    Vector3.zero,
                    Quaternion.identity);

                _root.transform.localScale =
                    Vector3.one;

                /*
                 * MeshRenderer тут навмисно відсутній.
                 *
                 * Flat Kit Outline та інші Renderer Features працюють
                 * через scene renderer lists. Без MeshRenderer вони
                 * фізично не можуть намалювати curtain повторно.
                 */
            }

            if (_mesh == null)
            {
                _mesh =
                    new Mesh
                    {
                        name =
                            "Moyva_FogBoundaryCurtainMesh",

                        indexFormat =
                            IndexFormat.UInt32,

                        hideFlags =
                            HideFlags.DontSave
                    };

                _mesh.MarkDynamic();
            }

            if (_material == null)
            {
                Shader shader =
                    Shader.Find(
                        CurtainShaderName);

                if (shader == null
                    || !shader.isSupported)
                {
                    if (!_shaderErrorLogged)
                    {
                        _shaderErrorLogged = true;

                        Debug.LogError(
                            "[FogOfWar] Shader '" +
                            CurtainShaderName +
                            "' was not found or is unsupported. " +
                            "Fog boundary curtain is disabled.");
                    }

                    _root.SetActive(false);

                    FogBoundaryCurtainRendererFeature
                        .ClearDrawData(
                            _mesh);

                    return false;
                }

                _material =
                    new Material(shader)
                    {
                        name =
                            "Moyva_FogBoundaryCurtainMaterial",

                        hideFlags =
                            HideFlags.DontSave
                    };

                int passIndex =
                    _material.FindPass(
                        "MoyvaFogCurtain");

                if (passIndex < 0)
                {
                    Debug.LogError(
                        "[MOYVA_FOG_CURTAIN_RENDER] " +
                        "Material does not contain the " +
                        "MoyvaFogCurtain pass.");

                    MoyvaJsonObjectFactory.DestroyImmediate(
                        _material);
                    _material = null;

                    FogBoundaryCurtainRendererFeature
                        .ClearDrawData(
                            _mesh);

                    return false;
                }
            }

            return true;
        }

        private void UpdateMaterial(
            FogScreenSpaceSettings settings)
        {
            /*
             * Top seam matches fullscreen fog; only the lower side darkens.
             */
            Color topColor =
                settings.UnexploredColor;

            topColor.a = 1f;

            Color bottomColor =
                Color.Lerp(
                    settings.UnexploredColor,
                    Color.black,
                    settings.CurtainTopDarkness);

            bottomColor.a = 1f;

            _material.SetColor(
                BottomColorId,
                bottomColor);

            _material.SetColor(
                TopColorId,
                topColor);

            _material.SetFloat(
                TopBandFractionId,
                settings.CurtainTopBandFraction);

            _material.SetFloat(
                GradientPowerId,
                settings.CurtainGradientPower);

            _material.SetFloat(
                DebugVisualModeId,
                (float)settings.CurtainDebugVisualMode);

            bool effectiveDoubleSided =
                settings.CurtainDoubleSided
                || (settings
                        .CurtainForceDoubleSidedInNormalMode
                    && settings.CurtainDebugVisualMode
                        == FogCurtainDebugVisualMode.Off);

            _material.SetFloat(
                CullModeId,
                effectiveDoubleSided
                    ? (float)CullMode.Off
                    : (float)CullMode.Back);

            /*
             * Curtain є гарантованим overlay над world-геометрією.
             * Високі тайли враховуються на етапі побудови mesh через
             * highest-adjacent surface, а не через fragment discard.
             */
        }

        private void ApplyMesh()
        {
            _mesh.Clear(false);

            if (_vertices.Count == 0)
            {
                _root.SetActive(false);

                FogBoundaryCurtainRendererFeature
                    .ClearDrawData(
                        _mesh);

                return;
            }

            _mesh.SetVertices(
                _vertices);

            _mesh.SetUVs(
                0,
                _uvs);

            _mesh.SetColors(
                _colors);

            _mesh.SetTriangles(
                _triangles,
                0,
                true);

            _mesh.RecalculateBounds();

            _root.SetActive(true);

            FogBoundaryCurtainRendererFeature
                .SetDrawData(
                    _mesh,
                    _material,
                    Matrix4x4.identity,
                    true);
        }

        internal void ClearPresentation()
        {
            if (_mesh != null)
                _mesh.Clear(false);

            if (_root != null)
                _root.SetActive(false);

            FogBoundaryCurtainRendererFeature
                .ClearDrawData(
                    _mesh);
        }

        private void AddQuad(
            Vector3 bottomA,
            Vector3 bottomB,
            Vector3 topB,
            Vector3 topA,
            Vector3 outward,
            Color32 debugColor)
        {
            int vertexStart =
                _vertices.Count;

            _vertices.Add(bottomA);
            _vertices.Add(bottomB);
            _vertices.Add(topB);
            _vertices.Add(topA);

            _uvs.Add(
                new Vector2(
                    0f,
                    0f));

            _uvs.Add(
                new Vector2(
                    1f,
                    0f));

            _uvs.Add(
                new Vector2(
                    1f,
                    1f));

            _uvs.Add(
                new Vector2(
                    0f,
                    1f));

            _colors.Add(
                debugColor);

            _colors.Add(
                debugColor);

            _colors.Add(
                debugColor);

            _colors.Add(
                debugColor);

            Vector3 candidateNormal =
                Vector3.Cross(
                    bottomB - bottomA,
                    topB - bottomA);

            bool candidateFacesOutward =
                Vector3.Dot(
                    candidateNormal,
                    outward)
                >= 0f;

            if (candidateFacesOutward)
            {
                _triangles.Add(vertexStart);
                _triangles.Add(vertexStart + 1);
                _triangles.Add(vertexStart + 2);

                _triangles.Add(vertexStart);
                _triangles.Add(vertexStart + 2);
                _triangles.Add(vertexStart + 3);
            }
            else
            {
                _triangles.Add(vertexStart);
                _triangles.Add(vertexStart + 2);
                _triangles.Add(vertexStart + 1);

                _triangles.Add(vertexStart);
                _triangles.Add(vertexStart + 3);
                _triangles.Add(vertexStart + 2);
            }
        }

        private SurfaceCalibrationResult ResolveSurfaceCalibration(
            Color32[] fogPixels,
            int width,
            int height,
            FogWorldVisualContext context,
            Vector3 gridOrigin,
            Vector3 xBasis,
            Vector3 yBasis,
            FogScreenSpaceSettings settings)
        {
            float fallbackOffset =
                settings.CurtainSurfaceOffsetY;

            if (!settings.CurtainAutoCalibrateSurfaceOffset)
            {
                return new SurfaceCalibrationResult(
                    fallbackOffset,
                    0,
                    float.NaN,
                    float.NaN,
                    false);
            }

            _surfaceOffsetSamples.Clear();

            int targetSamples =
                Mathf.Clamp(
                    settings.CurtainSurfaceCalibrationSamples,
                    4,
                    64);

            int totalCells =
                width * height;

            int stride =
                Mathf.Max(
                    1,
                    totalCells
                    / Mathf.Max(
                        1,
                        targetSamples * 3));

            for (int linearIndex = 0;
                 linearIndex < totalCells
                 && _surfaceOffsetSamples.Count
                 < targetSamples;
                 linearIndex += stride)
            {
                int x =
                    linearIndex % width;

                int y =
                    linearIndex / width;

                if (IsUnexplored(
                        fogPixels,
                        width,
                        x,
                        y))
                {
                    continue;
                }

                var cell =
                    new Vector2Int(
                        x,
                        y);

                Vector3 cellCenter =
                    ResolveCellCenter(
                        cell,
                        gridOrigin,
                        xBasis,
                        yBasis);

                float logicalHeight =
                    ResolveSurfaceHeight(
                        context,
                        cell,
                        cellCenter.y);

                if (!TryProbeActualSurfaceHeight(
                        cellCenter,
                        logicalHeight,
                        settings,
                        out float actualHeight))
                {
                    continue;
                }

                float offset =
                    actualHeight
                    - logicalHeight;

                if (Mathf.Abs(offset)
                    > settings
                        .CurtainSurfaceCalibrationMaxOffset)
                {
                    continue;
                }

                _surfaceOffsetSamples.Add(
                    offset);
            }

            /*
             * Якщо stride пропустив revealed-область,
             * виконуємо короткий повний scan до набору мінімум 3 проб.
             */
            if (_surfaceOffsetSamples.Count < 3)
            {
                for (int y = 0;
                     y < height
                     && _surfaceOffsetSamples.Count
                     < targetSamples;
                     y++)
                {
                    for (int x = 0;
                         x < width
                         && _surfaceOffsetSamples.Count
                         < targetSamples;
                         x++)
                    {
                        if (IsUnexplored(
                                fogPixels,
                                width,
                                x,
                                y))
                        {
                            continue;
                        }

                        var cell =
                            new Vector2Int(
                                x,
                                y);

                        Vector3 cellCenter =
                            ResolveCellCenter(
                                cell,
                                gridOrigin,
                                xBasis,
                                yBasis);

                        float logicalHeight =
                            ResolveSurfaceHeight(
                                context,
                                cell,
                                cellCenter.y);

                        if (!TryProbeActualSurfaceHeight(
                                cellCenter,
                                logicalHeight,
                                settings,
                                out float actualHeight))
                        {
                            continue;
                        }

                        float offset =
                            actualHeight
                            - logicalHeight;

                        if (Mathf.Abs(offset)
                            > settings
                                .CurtainSurfaceCalibrationMaxOffset)
                        {
                            continue;
                        }

                        _surfaceOffsetSamples.Add(
                            offset);
                    }
                }
            }

            if (_surfaceOffsetSamples.Count == 0)
            {
                return new SurfaceCalibrationResult(
                    fallbackOffset,
                    0,
                    float.NaN,
                    float.NaN,
                    false);
            }

            _surfaceOffsetSamples.Sort();

            int middle =
                _surfaceOffsetSamples.Count / 2;

            float median =
                (_surfaceOffsetSamples.Count & 1) == 0
                    ? (_surfaceOffsetSamples[middle - 1]
                       + _surfaceOffsetSamples[middle])
                      * 0.5f
                    : _surfaceOffsetSamples[middle];

            return new SurfaceCalibrationResult(
                median,
                _surfaceOffsetSamples.Count,
                _surfaceOffsetSamples[0],
                _surfaceOffsetSamples[
                    _surfaceOffsetSamples.Count - 1],
                true);
        }

        private bool TryProbeActualSurfaceHeight(
            Vector3 cellCenter,
            float logicalHeight,
            FogScreenSpaceSettings settings,
            out float actualHeight)
        {
            actualHeight =
                0f;

            Vector3 rayOrigin =
                new Vector3(
                    cellCenter.x,
                    logicalHeight
                        + settings.CurtainSurfaceProbeHeight,
                    cellCenter.z);

            int hitCount =
                Physics.RaycastNonAlloc(
                    rayOrigin,
                    Vector3.down,
                    _surfaceProbeHits,
                    settings.CurtainSurfaceProbeDistance,
                    settings.CurtainSurfaceProbeMask,
                    QueryTriggerInteraction.Ignore);

            if (hitCount <= 0)
                return false;

            float nearestDistance =
                float.PositiveInfinity;

            bool found =
                false;

            for (int i = 0;
                 i < hitCount;
                 i++)
            {
                RaycastHit hit =
                    _surfaceProbeHits[i];

                if (hit.collider == null)
                    continue;

                if (!IsAcceptedTerrainSurface(
                        hit.collider))
                {
                    continue;
                }

                if (hit.distance >= nearestDistance)
                    continue;

                nearestDistance =
                    hit.distance;

                actualHeight =
                    hit.point.y;

                found =
                    true;
            }

            return found;
        }

        private static bool IsAcceptedTerrainSurface(
            Collider collider)
        {
            Transform current =
                collider != null
                    ? collider.transform
                    : null;

            while (current != null)
            {
                string currentName =
                    current.name;

                if (currentName.IndexOf(
                        "Terrain",
                        StringComparison.OrdinalIgnoreCase)
                    >= 0
                    || currentName.IndexOf(
                        "MapVisualChunk",
                        StringComparison.OrdinalIgnoreCase)
                    >= 0
                    || string.Equals(
                        currentName,
                        "TilesRoot",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                current =
                    current.parent;
            }

            return false;
        }

        private readonly struct SurfaceCalibrationResult
        {
            public readonly float OffsetY;
            public readonly int SampleCount;
            public readonly float Minimum;
            public readonly float Maximum;
            public readonly bool WasCalibrated;

            public SurfaceCalibrationResult(
                float offsetY,
                int sampleCount,
                float minimum,
                float maximum,
                bool wasCalibrated)
            {
                OffsetY =
                    offsetY;

                SampleCount =
                    sampleCount;

                Minimum =
                    minimum;

                Maximum =
                    maximum;

                WasCalibrated =
                    wasCalibrated;
            }
        }

        private void AddTopCapQuad(
            Vector3 innerA,
            Vector3 innerB,
            Vector3 outerB,
            Vector3 outerA,
            Color32 debugColor)
        {
            int vertexStart =
                _vertices.Count;

            _vertices.Add(innerA);
            _vertices.Add(innerB);
            _vertices.Add(outerB);
            _vertices.Add(outerA);

            /*
             * UV.y = 1 для всіх вершин:
             * top-cap завжди використовує темний TopColor.
             */
            _uvs.Add(
                new Vector2(
                    0f,
                    1f));

            _uvs.Add(
                new Vector2(
                    1f,
                    1f));

            _uvs.Add(
                new Vector2(
                    1f,
                    1f));

            _uvs.Add(
                new Vector2(
                    0f,
                    1f));

            _colors.Add(debugColor);
            _colors.Add(debugColor);
            _colors.Add(debugColor);
            _colors.Add(debugColor);

            Vector3 candidateNormal =
                Vector3.Cross(
                    innerB - innerA,
                    outerB - innerA);

            bool candidateFacesUp =
                Vector3.Dot(
                    candidateNormal,
                    Vector3.up)
                >= 0f;

            if (candidateFacesUp)
            {
                _triangles.Add(vertexStart);
                _triangles.Add(vertexStart + 1);
                _triangles.Add(vertexStart + 2);

                _triangles.Add(vertexStart);
                _triangles.Add(vertexStart + 2);
                _triangles.Add(vertexStart + 3);
            }
            else
            {
                _triangles.Add(vertexStart);
                _triangles.Add(vertexStart + 2);
                _triangles.Add(vertexStart + 1);

                _triangles.Add(vertexStart);
                _triangles.Add(vertexStart + 3);
                _triangles.Add(vertexStart + 2);
            }
        }

        private Color32 ResolveDebugColor(
            FogScreenSpaceSettings settings,
            Vector2Int cell,
            int directionIndex,
            float topY,
            float revealedHeight,
            float hiddenHeight)
        {
            switch (settings.CurtainDebugVisualMode)
            {
                case FogCurtainDebugVisualMode
                    .BoundaryDirection:

                    switch (directionIndex)
                    {
                        case 0:
                            return new Color32(
                                255,
                                60,
                                60,
                                255);

                        case 1:
                            return new Color32(
                                60,
                                255,
                                80,
                                255);

                        case 2:
                            return new Color32(
                                70,
                                120,
                                255,
                                255);

                        default:
                            return new Color32(
                                255,
                                220,
                                60,
                                255);
                    }

                case FogCurtainDebugVisualMode
                    .HeightBands:

                    Color heightColor =
                        Color.HSVToRGB(
                            Mathf.Repeat(
                                topY * 0.173f,
                                1f),
                            0.9f,
                            1f);

                    return heightColor;

                case FogCurtainDebugVisualMode
                    .HeightRelation:

                    if (hiddenHeight
                        > revealedHeight + 0.001f)
                    {
                        return new Color32(
                            255,
                            70,
                            200,
                            255);
                    }

                    if (revealedHeight
                        > hiddenHeight + 0.001f)
                    {
                        return new Color32(
                            40,
                            220,
                            255,
                            255);
                    }

                    return new Color32(
                        255,
                        255,
                        255,
                        255);

                case FogCurtainDebugVisualMode
                    .SegmentParity:

                    return ((cell.x
                             + cell.y
                             + directionIndex)
                            & 1) == 0
                        ? new Color32(
                            255,
                            255,
                            255,
                            255)
                        : new Color32(
                            30,
                            30,
                            30,
                            255);

                default:
                    return new Color32(
                        255,
                        255,
                        255,
                        255);
            }
        }

        private void LogDiagnosticsIfNeeded(
            FogScreenSpaceSettings settings,
            Color32[] fogPixels,
            int width,
            int height,
            FogWorldVisualContext context,
            Vector3 gridOrigin,
            Vector3 xBasis,
            Vector3 yBasis,
            float referenceCellSize,
            int revealedCellCount,
            int unexploredCellCount,
            int boundaryEdgeCount,
            int outsideBoundaryCount,
            int topCapCount,
            bool buildDiagnosticTopCaps,
            SurfaceCalibrationResult surfaceCalibration,
            float minimumLogicalSurfaceHeight,
            float maximumLogicalSurfaceHeight,
            float sharedBottomY,
            int leftEdges,
            int rightEdges,
            int downEdges,
            int upEdges,
            float minimumTopY,
            float maximumTopY,
            float minimumBottomY,
            float maximumBottomY,
            int perEdgeProbeSuccessCount,
            int perEdgeProbeFailureCount,
            float minimumPerEdgeProbeOffset,
            float maximumPerEdgeProbeOffset,
            Vector2 revealedCenter)
        {
            if (!settings.LogCurtainDiagnostics)
                return;

            int boundaryHash =
                ComputeBoundaryHash();

            float now =
                Time.realtimeSinceStartup;

            bool sameBoundary =
                boundaryHash
                == _lastBoundaryHash;

            if (sameBoundary
                && now - _lastDiagnosticTime
                < settings.DiagnosticLogIntervalSeconds)
            {
                return;
            }

            _lastBoundaryHash =
                boundaryHash;

            _lastDiagnosticTime =
                now;

            _rebuildSequence++;

            int centerBoundaryCount = 0;

            for (int i = 0;
                 i < _diagnosticSegments.Count;
                 i++)
            {
                DiagnosticSegment segment =
                    _diagnosticSegments[i];

                float centerDistance =
                    Vector2.Distance(
                        new Vector2(
                            segment.Cell.x,
                            segment.Cell.y),
                        revealedCenter);

                if (centerDistance
                    <= settings.DiagnosticCenterRadiusCells)
                {
                    centerBoundaryCount++;
                }
            }

            var builder =
                new StringBuilder(6144);

            builder.Append(
                DiagnosticPrefix);

            builder.Append(
                " SUMMARY rebuild=");

            builder.Append(
                _rebuildSequence);

            builder.Append(
                " map=");

            builder.Append(
                width);

            builder.Append(
                "x");

            builder.Append(
                height);

            builder.Append(
                " revealed=");

            builder.Append(
                revealedCellCount);

            builder.Append(
                " unexplored=");

            builder.Append(
                unexploredCellCount);

            builder.Append(
                " revealedCenter=(");

            builder.Append(
                revealedCenter.x.ToString("F2"));

            builder.Append(
                ",");

            builder.Append(
                revealedCenter.y.ToString("F2"));

            builder.Append(
                ") boundaryEdges=");

            builder.Append(
                boundaryEdgeCount);

            builder.Append(
                " outsideEdges=");

            builder.Append(
                outsideBoundaryCount);

            builder.Append(
                " topLipQuads=");

            builder.Append(
                topCapCount);

            builder.Append(
                " topLipRequested=");

            builder.Append(
                settings.CurtainTopCapEnabled);

            builder.Append(
                " topLipBuilt=");

            builder.Append(
                buildDiagnosticTopCaps);

            bool effectiveDoubleSided =
                settings.CurtainDoubleSided
                || (settings
                        .CurtainForceDoubleSidedInNormalMode
                    && settings.CurtainDebugVisualMode
                        == FogCurtainDebugVisualMode.Off);

            builder.Append(
                " cull=");

            builder.Append(
                effectiveDoubleSided
                    ? "Off"
                    : "Back");

            builder.Append(
                " anchor=");

            builder.Append(
                "HighestAdjacentOverlay");

            builder.Append(
                " perEdgeProbe=");

            builder.Append(
                settings.CurtainPerEdgeSurfaceProbe);

            builder.Append(
                " opaqueDepthOcclusion=");

            builder.Append(
                false);

            builder.Append(
                " opaqueDepthBias=");

            builder.Append(
                "disabled");

            builder.Append(
                " highestAdjacent=");

            builder.Append(
                settings.CurtainUseHighestAdjacentSurface);

            builder.Append(
                " heightTransition=FlatPerEdge");

            builder.Append(
                " edgeOverlapCells=");

            builder.Append(
                settings.CurtainEdgeOverlapCells
                    .ToString("F3"));

            builder.Append(
                " depthMode=ConstantPerEdge");

            builder.Append(
                " segmentDepth=");

            builder.Append(
                (
                    settings.CurtainWorldDepth
                    + settings.CurtainBottomPadding
                ).ToString("F3"));

            builder.Append(
                " sharedBottomLegacyIgnored=");

            builder.Append(
                settings.CurtainUseSharedBottomPlane);

            builder.Append(
                " logicalHeightRange=[");

            builder.Append(
                minimumLogicalSurfaceHeight
                    .ToString("F3"));

            builder.Append(
                ",");

            builder.Append(
                maximumLogicalSurfaceHeight
                    .ToString("F3"));

            builder.Append(
                "] legacySharedBottomY=");

            builder.Append(
                sharedBottomY.ToString("F3"));

            builder.Append(
                " surfaceOffset=");

            builder.Append(
                surfaceCalibration.OffsetY
                    .ToString("F3"));

            builder.Append(
                " surfaceSamples=");

            builder.Append(
                surfaceCalibration.SampleCount);

            builder.Append(
                " surfaceSampleRange=[");

            builder.Append(
                FormatFloat(
                    surfaceCalibration.Minimum));

            builder.Append(
                ",");

            builder.Append(
                FormatFloat(
                    surfaceCalibration.Maximum));

            builder.Append(
                "] surfaceCalibration=");

            builder.Append(
                surfaceCalibration.WasCalibrated
                    ? "Auto"
                    : "Fallback");

            builder.Append(
                " edgeProbeSuccess=");

            builder.Append(
                perEdgeProbeSuccessCount);

            builder.Append(
                " edgeProbeFail=");

            builder.Append(
                perEdgeProbeFailureCount);

            builder.Append(
                " edgeProbeOffsetRange=");

            builder.Append(
                FormatRange(
                    minimumPerEdgeProbeOffset,
                    maximumPerEdgeProbeOffset));

            builder.Append(
                " edgesNearRevealedCenter=");

            builder.Append(
                centerBoundaryCount);

            builder.Append(
                " directions[L,R,D,U]=");

            builder.Append(
                leftEdges);

            builder.Append(
                ",");

            builder.Append(
                rightEdges);

            builder.Append(
                ",");

            builder.Append(
                downEdges);

            builder.Append(
                ",");

            builder.Append(
                upEdges);

            builder.Append(
                " visualMode=");

            builder.Append(
                settings.CurtainDebugVisualMode);

            builder.Append(
                " heightSource=");

            builder.Append(
                _settings != null
                && _settings.Volume != null
                    ? _settings.Volume
                        .HeightSource.ToString()
                    : "default");

            builder.Append(
                " configuredDepth=");

            builder.Append(
                settings.CurtainWorldDepth.ToString("F3"));

            builder.Append(
                " topRange=[");

            builder.Append(
                FormatFloat(
                    minimumTopY));

            builder.Append(
                ",");

            builder.Append(
                FormatFloat(
                    maximumTopY));

            builder.Append(
                "] bottomRange=[");

            builder.Append(
                FormatFloat(
                    minimumBottomY));

            builder.Append(
                ",");

            builder.Append(
                FormatFloat(
                    maximumBottomY));

            builder.Append(
                "] gridOrigin=");

            builder.Append(
                FormatVector3(
                    gridOrigin));

            builder.Append(
                " xBasis=");

            builder.Append(
                FormatVector3(
                    xBasis));

            builder.Append(
                " yBasis=");

            builder.Append(
                FormatVector3(
                    yBasis));

            builder.Append(
                " cellSize=");

            builder.Append(
                referenceCellSize.ToString("F4"));

            builder.Append(
                " meshBounds=");

            builder.Append(
                _mesh != null
                    ? FormatBounds(
                        _mesh.bounds)
                    : "null");

            builder.Append(
                " contextValid=");

            builder.Append(
                context.IsValid);

            builder.AppendLine();

            AppendCenterSegmentLogs(
                builder,
                settings,
                revealedCenter);

            AppendBoundaryComponentLogs(
                builder,
                settings);

            AppendFogMaskAscii(
                builder,
                fogPixels,
                width,
                height,
                revealedCenter,
                settings.DiagnosticMaskRadiusCells);

            Debug.Log(
                builder.ToString(),
                _root);
        }

        private void AppendCenterSegmentLogs(
            StringBuilder builder,
            FogScreenSpaceSettings settings,
            Vector2 revealedCenter)
        {
            _diagnosticSegments.Sort(
                (a, b) =>
                {
                    float aDistance =
                        Vector2.SqrMagnitude(
                            new Vector2(
                                a.Cell.x,
                                a.Cell.y)
                            - revealedCenter);

                    float bDistance =
                        Vector2.SqrMagnitude(
                            new Vector2(
                                b.Cell.x,
                                b.Cell.y)
                            - revealedCenter);

                    return aDistance.CompareTo(
                        bDistance);
                });

            int written = 0;

            for (int i = 0;
                 i < _diagnosticSegments.Count;
                 i++)
            {
                if (written
                    >= settings.DiagnosticMaxSegmentLogs)
                {
                    break;
                }

                DiagnosticSegment segment =
                    _diagnosticSegments[i];

                float centerDistance =
                    Vector2.Distance(
                        new Vector2(
                            segment.Cell.x,
                            segment.Cell.y),
                        revealedCenter);

                if (centerDistance
                    > settings.DiagnosticCenterRadiusCells)
                {
                    continue;
                }

                builder.Append(
                    DiagnosticPrefix);

                builder.Append(
                    " SEGMENT index=");

                builder.Append(
                    i);

                builder.Append(
                    " cell=");

                builder.Append(
                    segment.Cell);

                builder.Append(
                    " neighbour=");

                builder.Append(
                    segment.Neighbour);

                builder.Append(
                    " dir=");

                builder.Append(
                    DirectionLabel(
                        segment.DirectionIndex));

                builder.Append(
                    " outside=");

                builder.Append(
                    segment.NeighbourOutside);

                builder.Append(
                    " cellCenter=");

                builder.Append(
                    FormatVector3(
                        segment.CellCenter));

                builder.Append(
                    " revealedH=");

                builder.Append(
                    segment.RevealedHeight
                        .ToString("F3"));

                builder.Append(
                    " hiddenH=");

                builder.Append(
                    segment.HiddenHeight
                        .ToString("F3"));

                builder.Append(
                    " heightDelta=");

                builder.Append(
                    (segment.HiddenHeight
                     - segment.RevealedHeight)
                    .ToString("F3"));

                builder.Append(
                    " topA=");

                builder.Append(
                    segment.TopAY.ToString("F3"));

                builder.Append(
                    " topB=");

                builder.Append(
                    segment.TopBY.ToString("F3"));

                builder.Append(
                    " cornerDelta=");

                builder.Append(
                    Mathf.Abs(
                        segment.TopAY
                        - segment.TopBY)
                    .ToString("F3"));

                builder.Append(
                    " bottomA=");

                builder.Append(
                    segment.BottomAY.ToString("F3"));

                builder.Append(
                    " bottomB=");

                builder.Append(
                    segment.BottomBY.ToString("F3"));

                builder.Append(
                    " maxDepth=");

                builder.Append(
                    (Mathf.Max(
                         segment.TopAY,
                         segment.TopBY)
                     - Mathf.Min(
                         segment.BottomAY,
                         segment.BottomBY))
                    .ToString("F3"));

                builder.Append(
                    " endpointA=");

                builder.Append(
                    segment.EndpointAKey);

                builder.Append(
                    " endpointB=");

                builder.Append(
                    segment.EndpointBKey);

                builder.Append(
                    " centerDistance=");

                builder.Append(
                    centerDistance.ToString("F2"));

                builder.AppendLine();

                written++;
            }

            if (written == 0)
            {
                builder.Append(
                    DiagnosticPrefix);

                builder.AppendLine(
                    " SEGMENT none-near-revealed-center");
            }
        }

        private int ComputeBoundaryHash()
        {
            unchecked
            {
                int hash = 17;

                for (int i = 0;
                     i < _diagnosticSegments.Count;
                     i++)
                {
                    DiagnosticSegment segment =
                        _diagnosticSegments[i];

                    hash =
                        hash * 31
                        + segment.Cell.GetHashCode();

                    hash =
                        hash * 31
                        + segment.DirectionIndex;

                    hash =
                        hash * 31
                        + Mathf.RoundToInt(
                            segment.TopAY * 100f);

                    hash =
                        hash * 31
                        + Mathf.RoundToInt(
                            segment.TopBY * 100f);

                    hash =
                        hash * 31
                        + segment.EndpointAKey
                            .GetHashCode();

                    hash =
                        hash * 31
                        + segment.EndpointBKey
                            .GetHashCode();
                }

                return hash;
            }
        }

        private static string DirectionLabel(
            int directionIndex)
        {
            switch (directionIndex)
            {
                case 0:
                    return "Left";

                case 1:
                    return "Right";

                case 2:
                    return "Down";

                default:
                    return "Up";
            }
        }

        private static string FormatFloat(
            float value)
        {
            if (float.IsNaN(value)
                || float.IsInfinity(value))
            {
                return "n/a";
            }

            return value.ToString("F3");
        }

        private static string FormatRange(
            float minimum,
            float maximum)
        {
            return "["
                   + FormatFloat(minimum)
                   + ","
                   + FormatFloat(maximum)
                   + "]";
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

        private static string FormatBounds(
            Bounds bounds)
        {
            return "center="
                   + FormatVector3(
                       bounds.center)
                   + " size="
                   + FormatVector3(
                       bounds.size);
        }

        private void ResolveLogicalSurfaceRange(
            FogWorldVisualContext context,
            int width,
            int height,
            out float minimum,
            out float maximum)
        {
            minimum = float.PositiveInfinity;
            maximum = float.NegativeInfinity;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float value = ResolveSurfaceHeight(
                        context,
                        new Vector2Int(x, y),
                        0f);

                    if (!IsFinite(value))
                        continue;

                    minimum = Mathf.Min(minimum, value);
                    maximum = Mathf.Max(maximum, value);
                }
            }

            if (!IsFinite(minimum) || !IsFinite(maximum))
            {
                minimum = 0f;
                maximum = 0f;
            }
        }

        private float ResolveCornerLogicalHeight(
            FogWorldVisualContext context,
            int width,
            int height,
            Vector2Int doubledCorner,
            float fallback)
        {
            float cornerX = doubledCorner.x * 0.5f;
            float cornerY = doubledCorner.y * 0.5f;
            int minimumCellX = Mathf.FloorToInt(cornerX);
            int minimumCellY = Mathf.FloorToInt(cornerY);
            float result = fallback;
            bool found = false;

            for (int offsetY = 0; offsetY <= 1; offsetY++)
            {
                for (int offsetX = 0; offsetX <= 1; offsetX++)
                {
                    var candidate = new Vector2Int(
                        minimumCellX + offsetX,
                        minimumCellY + offsetY);

                    if (!IsInBounds(candidate, width, height))
                        continue;

                    float value = ResolveSurfaceHeight(
                        context,
                        candidate,
                        fallback);

                    if (!IsFinite(value))
                        continue;

                    result = found ? Mathf.Max(result, value) : value;
                    found = true;
                }
            }

            return found ? result : fallback;
        }

        private static void ResolveBoundaryEndpointKeys(
            Vector2Int cell,
            int directionIndex,
            out Vector2Int endpointA,
            out Vector2Int endpointB)
        {
            int centerX = cell.x * 2;
            int centerY = cell.y * 2;

            switch (directionIndex)
            {
                case 0:
                    endpointA = new Vector2Int(centerX - 1, centerY - 1);
                    endpointB = new Vector2Int(centerX - 1, centerY + 1);
                    break;

                case 1:
                    endpointA = new Vector2Int(centerX + 1, centerY - 1);
                    endpointB = new Vector2Int(centerX + 1, centerY + 1);
                    break;

                case 2:
                    endpointA = new Vector2Int(centerX - 1, centerY - 1);
                    endpointB = new Vector2Int(centerX + 1, centerY - 1);
                    break;

                default:
                    endpointA = new Vector2Int(centerX - 1, centerY + 1);
                    endpointB = new Vector2Int(centerX + 1, centerY + 1);
                    break;
            }
        }

        private void AppendBoundaryComponentLogs(
            StringBuilder builder,
            FogScreenSpaceSettings settings)
        {
            List<BoundaryComponentSummary> components =
                BuildBoundaryComponentSummaries();

            int closedInternalLoops = 0;

            for (int i = 0; i < components.Count; i++)
            {
                if (components[i].Closed && components[i].OutsideEdges == 0)
                    closedInternalLoops++;
            }

            builder.Append(DiagnosticPrefix);
            builder.Append(" COMPONENT_SUMMARY count=");
            builder.Append(components.Count);
            builder.Append(" closedInternalLoops=");
            builder.Append(closedInternalLoops);
            builder.AppendLine();

            components.Sort((a, b) => b.EdgeCount.CompareTo(a.EdgeCount));
            int limit = Mathf.Min(settings.DiagnosticMaxComponentLogs, components.Count);

            for (int i = 0; i < limit; i++)
            {
                BoundaryComponentSummary component = components[i];
                builder.Append(DiagnosticPrefix);
                builder.Append(" COMPONENT id=");
                builder.Append(component.Id);
                builder.Append(" edges=");
                builder.Append(component.EdgeCount);
                builder.Append(" outsideEdges=");
                builder.Append(component.OutsideEdges);
                builder.Append(" closed=");
                builder.Append(component.Closed);
                builder.Append(" branchEndpoints=");
                builder.Append(component.BranchEndpointCount);
                builder.Append(" center=(");
                builder.Append(component.Center.x.ToString("F2"));
                builder.Append(",");
                builder.Append(component.Center.y.ToString("F2"));
                builder.Append(") cellBounds=[");
                builder.Append(component.MinimumCell);
                builder.Append("..");
                builder.Append(component.MaximumCell);
                builder.Append("] topRange=[");
                builder.Append(component.MinimumTopY.ToString("F3"));
                builder.Append(",");
                builder.Append(component.MaximumTopY.ToString("F3"));
                builder.Append("] maxCornerDelta=");
                builder.Append(component.MaximumCornerDelta.ToString("F3"));
                builder.AppendLine();
            }
        }

        private List<BoundaryComponentSummary> BuildBoundaryComponentSummaries()
        {
            var result = new List<BoundaryComponentSummary>();
            int segmentCount = _diagnosticSegments.Count;

            if (segmentCount == 0)
                return result;

            var endpointMap = new Dictionary<Vector2Int, List<int>>();

            for (int i = 0; i < segmentCount; i++)
            {
                DiagnosticSegment segment = _diagnosticSegments[i];
                AddEndpointSegment(endpointMap, segment.EndpointAKey, i);
                AddEndpointSegment(endpointMap, segment.EndpointBKey, i);
            }

            var visited = new bool[segmentCount];
            var queue = new Queue<int>();
            int componentId = 0;

            for (int start = 0; start < segmentCount; start++)
            {
                if (visited[start])
                    continue;

                queue.Clear();
                queue.Enqueue(start);
                visited[start] = true;

                int edgeCount = 0;
                int outsideEdges = 0;
                Vector2 centerSum = Vector2.zero;
                Vector2Int minimumCell = new Vector2Int(int.MaxValue, int.MaxValue);
                Vector2Int maximumCell = new Vector2Int(int.MinValue, int.MinValue);
                float minimumTopY = float.PositiveInfinity;
                float maximumTopY = float.NegativeInfinity;
                float maximumCornerDelta = 0f;
                var componentEndpoints = new HashSet<Vector2Int>();

                while (queue.Count > 0)
                {
                    int index = queue.Dequeue();
                    DiagnosticSegment segment = _diagnosticSegments[index];
                    edgeCount++;

                    if (segment.NeighbourOutside)
                        outsideEdges++;

                    centerSum += new Vector2(segment.Cell.x, segment.Cell.y);
                    minimumCell = Vector2Int.Min(minimumCell, segment.Cell);
                    maximumCell = Vector2Int.Max(maximumCell, segment.Cell);
                    minimumTopY = Mathf.Min(minimumTopY, Mathf.Min(segment.TopAY, segment.TopBY));
                    maximumTopY = Mathf.Max(maximumTopY, Mathf.Max(segment.TopAY, segment.TopBY));
                    maximumCornerDelta = Mathf.Max(
                        maximumCornerDelta,
                        Mathf.Abs(segment.TopAY - segment.TopBY));

                    componentEndpoints.Add(segment.EndpointAKey);
                    componentEndpoints.Add(segment.EndpointBKey);
                    EnqueueConnectedSegments(endpointMap, segment.EndpointAKey, visited, queue);
                    EnqueueConnectedSegments(endpointMap, segment.EndpointBKey, visited, queue);
                }

                int branchEndpointCount = 0;
                bool closed = true;

                foreach (Vector2Int endpoint in componentEndpoints)
                {
                    int degree = endpointMap.TryGetValue(endpoint, out List<int> touching)
                        ? touching.Count
                        : 0;

                    if (degree != 2)
                    {
                        closed = false;
                        branchEndpointCount++;
                    }
                }

                result.Add(new BoundaryComponentSummary(
                    componentId,
                    edgeCount,
                    outsideEdges,
                    closed,
                    branchEndpointCount,
                    edgeCount > 0 ? centerSum / edgeCount : Vector2.zero,
                    minimumCell,
                    maximumCell,
                    minimumTopY,
                    maximumTopY,
                    maximumCornerDelta));

                componentId++;
            }

            return result;
        }

        private static void AddEndpointSegment(
            Dictionary<Vector2Int, List<int>> map,
            Vector2Int endpoint,
            int segmentIndex)
        {
            if (!map.TryGetValue(endpoint, out List<int> list))
            {
                list = new List<int>(2);
                map[endpoint] = list;
            }

            list.Add(segmentIndex);
        }

        private static void EnqueueConnectedSegments(
            Dictionary<Vector2Int, List<int>> map,
            Vector2Int endpoint,
            bool[] visited,
            Queue<int> queue)
        {
            if (!map.TryGetValue(endpoint, out List<int> connected))
                return;

            for (int i = 0; i < connected.Count; i++)
            {
                int index = connected[i];

                if (visited[index])
                    continue;

                visited[index] = true;
                queue.Enqueue(index);
            }
        }

        private void AppendFogMaskAscii(
            StringBuilder builder,
            Color32[] fogPixels,
            int width,
            int height,
            Vector2 revealedCenter,
            int radius)
        {
            if (fogPixels == null || fogPixels.Length < width * height)
                return;

            Vector2Int center = new Vector2Int(
                Mathf.RoundToInt(revealedCenter.x),
                Mathf.RoundToInt(revealedCenter.y));

            builder.Append(MaskDiagnosticPrefix);
            builder.Append(" center=");
            builder.Append(center);
            builder.Append(" radius=");
            builder.Append(radius);
            builder.AppendLine(" legend[V=Visible,e=Explored,B=Boundary,#=Unexplored,space=Outside]");

            for (int y = center.y + radius; y >= center.y - radius; y--)
            {
                builder.Append(MaskDiagnosticPrefix);
                builder.Append(" y=");
                builder.Append(y.ToString("D3"));
                builder.Append(" ");

                for (int x = center.x - radius; x <= center.x + radius; x++)
                {
                    if (x < 0 || x >= width || y < 0 || y >= height)
                    {
                        builder.Append(' ');
                        continue;
                    }

                    Color32 pixel = fogPixels[x + y * width];

                    if (pixel.g >= 128)
                    {
                        builder.Append('#');
                        continue;
                    }

                    if (IsRevealedBoundaryCell(fogPixels, width, height, x, y))
                        builder.Append('B');
                    else if (pixel.r >= 128)
                        builder.Append('e');
                    else
                        builder.Append('V');
                }

                builder.AppendLine();
            }
        }

        private static bool IsRevealedBoundaryCell(
            Color32[] fogPixels,
            int width,
            int height,
            int x,
            int y)
        {
            Vector2Int cell = new Vector2Int(x, y);

            for (int i = 0; i < Directions.Length; i++)
            {
                if (IsUnexplored(
                        fogPixels,
                        width,
                        height,
                        cell + Directions[i]))
                {
                    return true;
                }
            }

            return false;
        }

        private readonly struct BoundaryComponentSummary
        {
            public readonly int Id;
            public readonly int EdgeCount;
            public readonly int OutsideEdges;
            public readonly bool Closed;
            public readonly int BranchEndpointCount;
            public readonly Vector2 Center;
            public readonly Vector2Int MinimumCell;
            public readonly Vector2Int MaximumCell;
            public readonly float MinimumTopY;
            public readonly float MaximumTopY;
            public readonly float MaximumCornerDelta;

            public BoundaryComponentSummary(
                int id,
                int edgeCount,
                int outsideEdges,
                bool closed,
                int branchEndpointCount,
                Vector2 center,
                Vector2Int minimumCell,
                Vector2Int maximumCell,
                float minimumTopY,
                float maximumTopY,
                float maximumCornerDelta)
            {
                Id = id;
                EdgeCount = edgeCount;
                OutsideEdges = outsideEdges;
                Closed = closed;
                BranchEndpointCount = branchEndpointCount;
                Center = center;
                MinimumCell = minimumCell;
                MaximumCell = maximumCell;
                MinimumTopY = minimumTopY;
                MaximumTopY = maximumTopY;
                MaximumCornerDelta = maximumCornerDelta;
            }
        }

        private readonly struct DiagnosticSegment
        {
            public readonly Vector2Int Cell;
            public readonly Vector2Int Neighbour;
            public readonly int DirectionIndex;
            public readonly Vector3 CellCenter;
            public readonly float RevealedHeight;
            public readonly float HiddenHeight;
            public readonly float TopAY;
            public readonly float TopBY;
            public readonly float BottomAY;
            public readonly float BottomBY;
            public readonly Vector2Int EndpointAKey;
            public readonly Vector2Int EndpointBKey;
            public readonly bool NeighbourOutside;

            public DiagnosticSegment(
                Vector2Int cell,
                Vector2Int neighbour,
                int directionIndex,
                Vector3 cellCenter,
                float revealedHeight,
                float hiddenHeight,
                float topAY,
                float topBY,
                float bottomAY,
                float bottomBY,
                Vector2Int endpointAKey,
                Vector2Int endpointBKey,
                bool neighbourOutside)
            {
                Cell = cell;
                Neighbour = neighbour;
                DirectionIndex = directionIndex;
                CellCenter = cellCenter;
                RevealedHeight = revealedHeight;
                HiddenHeight = hiddenHeight;
                TopAY = topAY;
                TopBY = topBY;
                BottomAY = bottomAY;
                BottomBY = bottomBY;
                EndpointAKey = endpointAKey;
                EndpointBKey = endpointBKey;
                NeighbourOutside = neighbourOutside;
            }
        }

        private void ResolveGridBasis(
            int width,
            int height,
            FogWorldVisualContext context,
            out Vector3 origin,
            out Vector3 xBasis,
            out Vector3 yBasis)
        {
            if (_gridProjection != null)
            {
                origin =
                    _gridProjection.GridToWorld(
                        Vector2Int.zero);

                xBasis =
                    _gridProjection.GridToWorld(
                        Vector2Int.right)
                    - origin;

                yBasis =
                    _gridProjection.GridToWorld(
                        Vector2Int.up)
                    - origin;

                if (HorizontalLength(xBasis)
                        > 0.0001f
                    && HorizontalLength(yBasis)
                        > 0.0001f)
                {
                    return;
                }
            }

            if (context.IsValid
                && context.HasMapWorldBounds)
            {
                Bounds bounds =
                    context.MapWorldBounds;

                float cellWidth =
                    Mathf.Max(
                        0.0001f,
                        bounds.size.x
                        / Mathf.Max(
                            1,
                            width));

                float cellDepth =
                    Mathf.Max(
                        0.0001f,
                        bounds.size.z
                        / Mathf.Max(
                            1,
                            height));

                origin =
                    new Vector3(
                        bounds.min.x
                            + cellWidth * 0.5f,
                        0f,
                        bounds.min.z
                            + cellDepth * 0.5f);

                xBasis =
                    new Vector3(
                        cellWidth,
                        0f,
                        0f);

                yBasis =
                    new Vector3(
                        0f,
                        0f,
                        cellDepth);

                return;
            }

            float cellSize =
                context.IsValid
                    ? Mathf.Max(
                        0.0001f,
                        context.CellSize)
                    : 1f;

            origin =
                Vector3.zero;

            xBasis =
                new Vector3(
                    cellSize,
                    0f,
                    0f);

            yBasis =
                new Vector3(
                    0f,
                    0f,
                    cellSize);
        }

        private Vector3 ResolveCellCenter(
            Vector2Int cell,
            Vector3 origin,
            Vector3 xBasis,
            Vector3 yBasis)
        {
            if (_gridProjection != null)
            {
                return _gridProjection
                    .GridToWorld(cell);
            }

            return origin
                   + xBasis * cell.x
                   + yBasis * cell.y;
        }

        private float ResolveSurfaceHeight(
            FogWorldVisualContext context,
            Vector2Int cell,
            float fallback)
        {
            if (!context.IsValid)
                return fallback;

            FogVolumeHeightSource source =
                _settings != null
                    && _settings.Volume != null
                    ? _settings.Volume.HeightSource
                    : FogVolumeHeightSource
                        .TerrainLevelMapThenHeightMap;

            switch (source)
            {
                case FogVolumeHeightSource
                    .HeightMapThenTerrainLevelMap:

                    if (TryResolveHeightMapValue(
                            context,
                            cell,
                            out float heightMapValue))
                    {
                        return heightMapValue;
                    }

                    if (TryResolveTerrainLevelValue(
                            context,
                            cell,
                            out float terrainHeightValue))
                    {
                        return terrainHeightValue;
                    }

                    break;

                case FogVolumeHeightSource.Flat:
                    return fallback;

                default:
                    if (TryResolveTerrainLevelValue(
                            context,
                            cell,
                            out terrainHeightValue))
                    {
                        return terrainHeightValue;
                    }

                    if (TryResolveHeightMapValue(
                            context,
                            cell,
                            out heightMapValue))
                    {
                        return heightMapValue;
                    }

                    break;
            }

            return fallback;
        }

        private bool TryResolveTerrainLevelValue(
            FogWorldVisualContext context,
            Vector2Int cell,
            out float height)
        {
            height = 0f;

            int[,] terrainLevels =
                context.TerrainLevelMap;

            if (terrainLevels == null
                || cell.x < 0
                || cell.y < 0
                || cell.x >= terrainLevels.GetLength(0)
                || cell.y >= terrainLevels.GetLength(1))
            {
                return false;
            }

            float heightStep =
                _settings != null
                && _settings.Volume != null
                    ? Mathf.Max(
                        0.001f,
                        _settings.Volume
                            .TerrainLevelHeightStep)
                    : 1f;

            height =
                Mathf.Max(
                    0,
                    terrainLevels[
                        cell.x,
                        cell.y])
                * heightStep;

            return IsFinite(height);
        }

        private static bool TryResolveHeightMapValue(
            FogWorldVisualContext context,
            Vector2Int cell,
            out float height)
        {
            height = 0f;

            float[,] heightMap =
                context.HeightMap;

            if (heightMap == null
                || cell.x < 0
                || cell.y < 0
                || cell.x >= heightMap.GetLength(0)
                || cell.y >= heightMap.GetLength(1))
            {
                return false;
            }

            height =
                heightMap[
                    cell.x,
                    cell.y];

            return IsFinite(height);
        }

        private static bool IsFinite(
            float value)
        {
            return !float.IsNaN(value)
                   && !float.IsInfinity(value);
        }

        private FogScreenSpaceSettings
            ResolveSettings()
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

        private static bool IsUnexplored(
            Color32[] fogPixels,
            int width,
            int x,
            int y)
        {
            int index =
                x + y * width;

            /*
             * G channel:
             * 255 = Unexplored
             * 0   = Explored або Visible
             */
            return fogPixels[index].g >= 128;
        }

        private static bool IsUnexplored(
            Color32[] fogPixels,
            int width,
            int height,
            Vector2Int cell)
        {
            if (!IsInBounds(
                    cell,
                    width,
                    height))
            {
                return true;
            }

            return IsUnexplored(
                fogPixels,
                width,
                cell.x,
                cell.y);
        }

        private static bool IsInBounds(
            Vector2Int cell,
            int width,
            int height)
        {
            return cell.x >= 0
                   && cell.x < width
                   && cell.y >= 0
                   && cell.y < height;
        }

        private static float HorizontalLength(
            Vector3 value)
        {
            return Mathf.Sqrt(
                value.x * value.x
                + value.z * value.z);
        }

        private static void DestroyUnityObject(
            UnityEngine.Object value)
        {
            if (value == null)
                return;

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(
                    value);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(
                    value);
            }
        }
    }
}