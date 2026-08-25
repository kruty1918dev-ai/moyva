using System;
using System.Collections.Generic;
using System.Text;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Jsonization;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed partial class FogBoundaryCurtainRenderer
    {
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

    }
}
