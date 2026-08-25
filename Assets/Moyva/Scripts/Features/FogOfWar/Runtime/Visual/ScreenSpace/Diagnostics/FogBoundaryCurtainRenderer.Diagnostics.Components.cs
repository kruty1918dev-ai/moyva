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

    }
}
