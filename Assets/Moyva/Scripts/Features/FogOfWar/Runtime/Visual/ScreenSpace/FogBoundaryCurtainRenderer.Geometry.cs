using Kruty1918.Moyva.FogOfWar.API;
using UnityEngine;

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

            _vertices.Clear();
            _uvs.Clear();
            _triangles.Clear();

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

                        if (screenSettings
                            .CurtainPerEdgeSurfaceProbe)
                        {
                            if (TryProbeActualSurfaceHeight(
                                    cellCenter,
                                    revealedHeight,
                                    screenSettings,
                                    out float actualRevealedSurfaceY))
                            {
                                revealedSurfaceY =
                                    actualRevealedSurfaceY;
                            }

                            if (!neighbourOutside
                                && TryProbeActualSurfaceHeight(
                                    neighbourCenter,
                                    hiddenHeight,
                                    screenSettings,
                                    out float actualHiddenSurfaceY))
                            {
                                hiddenSurfaceY =
                                    actualHiddenSurfaceY;
                            }
                        }

                        float edgeSurfaceY =
                            Mathf.Max(
                                revealedSurfaceY,
                                hiddenSurfaceY);

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

                        AddQuad(
                            bottomA,
                            bottomB,
                            topB,
                            topA,
                            outward);

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
                                capOuterA);
                        }
                    }
                }
            }

            ApplyMesh();
        }

    }
}
