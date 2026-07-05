using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;

namespace Kruty1918.Moyva.Construction.Runtime
{
    public sealed class AutoTileVariantResolver : IAutoTileVariantResolver
    {
        private static readonly TopologyCaseType[] Supported =
        {
            TopologyCaseType.CrossIntersection,
            TopologyCaseType.TJunctionOpenNorth,
            TopologyCaseType.TJunctionOpenEast,
            TopologyCaseType.TJunctionOpenSouth,
            TopologyCaseType.TJunctionOpenWest,
            TopologyCaseType.CornerNorthEast,
            TopologyCaseType.CornerNorthWest,
            TopologyCaseType.CornerSouthEast,
            TopologyCaseType.CornerSouthWest,
            TopologyCaseType.Vertical,
            TopologyCaseType.VerticalLeft,
            TopologyCaseType.VerticalRight,
            TopologyCaseType.Horizontal,
            TopologyCaseType.HorizontalTop,
            TopologyCaseType.HorizontalBottom,
            TopologyCaseType.EndNorth,
            TopologyCaseType.EndEast,
            TopologyCaseType.EndSouth,
            TopologyCaseType.EndWest,
            TopologyCaseType.DiagonalNorthEastSouthWest,
            TopologyCaseType.DiagonalNorthWestSouthEast,
            TopologyCaseType.Isolated,
        };

        public IReadOnlyList<TopologyCaseType> SupportedCases => Supported;

        public bool TryResolveId(
            TopologyNeighborMask mask,
            IReadOnlyDictionary<TopologyCaseType, string> caseToId,
            out string resolvedId,
            out TopologyCaseType resolvedCase)
        {
            resolvedId = null;
            resolvedCase = TopologyCaseType.Isolated;

            if (caseToId == null || caseToId.Count == 0)
                return false;

            Span<TopologyCaseType> candidates = stackalloc TopologyCaseType[5];
            int count = BuildCandidates(mask, candidates);

            for (int i = 0; i < count; i++)
            {
                var candidate = candidates[i];
                if (!caseToId.TryGetValue(candidate, out var id) || string.IsNullOrWhiteSpace(id))
                    continue;

                resolvedId = id;
                resolvedCase = candidate;
                return true;
            }

            return false;
        }

        private static int BuildCandidates(TopologyNeighborMask m, Span<TopologyCaseType> candidates)
        {
            int count = 0;

            switch (m.CardinalCount)
            {
                case 4:
                    candidates[count++] = TopologyCaseType.CrossIntersection;
                    candidates[count++] = TopologyCaseType.Horizontal;
                    candidates[count++] = TopologyCaseType.Isolated;
                    return count;

                case 3:
                    if (!m.North)
                    {
                        candidates[count++] = TopologyCaseType.TJunctionOpenNorth;
                        candidates[count++] = TopologyCaseType.Horizontal;
                    }
                    else if (!m.East)
                    {
                        candidates[count++] = TopologyCaseType.TJunctionOpenEast;
                        candidates[count++] = TopologyCaseType.Vertical;
                    }
                    else if (!m.South)
                    {
                        candidates[count++] = TopologyCaseType.TJunctionOpenSouth;
                        candidates[count++] = TopologyCaseType.Horizontal;
                    }
                    else
                    {
                        candidates[count++] = TopologyCaseType.TJunctionOpenWest;
                        candidates[count++] = TopologyCaseType.Vertical;
                    }

                    candidates[count++] = TopologyCaseType.Isolated;
                    return count;

                case 2:
                    if (m.North && m.South)
                    {
                        if ((m.NorthWest || m.SouthWest) && !(m.NorthEast || m.SouthEast))
                            candidates[count++] = TopologyCaseType.VerticalLeft;
                        else if ((m.NorthEast || m.SouthEast) && !(m.NorthWest || m.SouthWest))
                            candidates[count++] = TopologyCaseType.VerticalRight;

                        candidates[count++] = TopologyCaseType.Vertical;
                        candidates[count++] = TopologyCaseType.EndNorth;
                        candidates[count++] = TopologyCaseType.EndSouth;
                        candidates[count++] = TopologyCaseType.Isolated;
                        return count;
                    }

                    if (m.East && m.West)
                    {
                        if ((m.NorthWest || m.NorthEast) && !(m.SouthWest || m.SouthEast))
                            candidates[count++] = TopologyCaseType.HorizontalTop;
                        else if ((m.SouthWest || m.SouthEast) && !(m.NorthWest || m.NorthEast))
                            candidates[count++] = TopologyCaseType.HorizontalBottom;

                        candidates[count++] = TopologyCaseType.Horizontal;
                        candidates[count++] = TopologyCaseType.EndEast;
                        candidates[count++] = TopologyCaseType.EndWest;
                        candidates[count++] = TopologyCaseType.Isolated;
                        return count;
                    }

                    // Atlas-position semantics:
                    // CornerNorthEast means the visual corner is in NE atlas slot,
                    // so logical neighbors are in opposite directions (S+W), etc.
                    if (m.North && m.East)
                        candidates[count++] = TopologyCaseType.CornerSouthWest;
                    else if (m.North && m.West)
                        candidates[count++] = TopologyCaseType.CornerSouthEast;
                    else if (m.South && m.East)
                        candidates[count++] = TopologyCaseType.CornerNorthWest;
                    else
                        candidates[count++] = TopologyCaseType.CornerNorthEast;

                    candidates[count++] = TopologyCaseType.Horizontal;
                    candidates[count++] = TopologyCaseType.Vertical;
                    candidates[count++] = TopologyCaseType.Isolated;
                    return count;

                case 1:
                    if (m.North) candidates[count++] = TopologyCaseType.EndNorth;
                    else if (m.East) candidates[count++] = TopologyCaseType.EndEast;
                    else if (m.South) candidates[count++] = TopologyCaseType.EndSouth;
                    else candidates[count++] = TopologyCaseType.EndWest;

                    candidates[count++] = (m.North || m.South)
                        ? TopologyCaseType.Vertical
                        : TopologyCaseType.Horizontal;

                    candidates[count++] = TopologyCaseType.Isolated;
                    return count;

                default:
                    if (m.NorthEast && m.SouthWest)
                    {
                        candidates[count++] = TopologyCaseType.DiagonalNorthEastSouthWest;
                        candidates[count++] = TopologyCaseType.Isolated;
                        return count;
                    }

                    if (m.NorthWest && m.SouthEast)
                    {
                        candidates[count++] = TopologyCaseType.DiagonalNorthWestSouthEast;
                        candidates[count++] = TopologyCaseType.Isolated;
                        return count;
                    }

                    candidates[count++] = TopologyCaseType.Isolated;
                    return count;
            }
        }
    }
}
