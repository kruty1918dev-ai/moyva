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

    }
}
