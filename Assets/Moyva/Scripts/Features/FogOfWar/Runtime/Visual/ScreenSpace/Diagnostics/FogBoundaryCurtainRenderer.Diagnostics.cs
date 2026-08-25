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

    }
}
