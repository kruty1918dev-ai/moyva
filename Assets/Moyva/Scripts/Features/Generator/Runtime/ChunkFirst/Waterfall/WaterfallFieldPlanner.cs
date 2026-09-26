using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    /// <summary>
    /// Detects rendered water-to-water drop edges and merges contiguous
    /// same-direction edges of one ledge into waterfall fronts. Pure data:
    /// the service feeds rendered sheet heights and the hydrology water
    /// mask; chunk borders are irrelevant because fronts anchor to cells.
    /// </summary>
    internal static class WaterfallFieldPlanner
    {
        /// <summary>Same 8-neighbour order the legacy strip emitter used.</summary>
        public static readonly Vector2Int[] Dirs =
        {
            new Vector2Int(-1, 0), new Vector2Int(1, 0),
            new Vector2Int(0, -1), new Vector2Int(0, 1),
            new Vector2Int(-1, -1), new Vector2Int(1, -1),
            new Vector2Int(-1, 1), new Vector2Int(1, 1),
        };

        /// <summary>Max height variation allowed inside one merged front.</summary>
        public const float FrontHeightTolerance = 0.3f;

        public sealed class Edge
        {
            public Vector2Int Cell;
            public int DirIndex;
            public Vector2Int Dir;
            public Vector2Int Lower;
            public float TopY;
            public float BottomY;
        }

        /// <summary>
        /// One physical ledge: contiguous drop edges sharing a direction and
        /// near-equal top/bottom heights. <see cref="Anchor"/> is the
        /// deterministic owner cell used for chunk ownership and stable ids.
        /// </summary>
        public sealed class Front
        {
            public Vector2Int Dir;
            public Vector2Int Anchor;
            public readonly List<Edge> Edges = new List<Edge>();
            public float TopY = float.MinValue;
            public float BottomY = float.MaxValue;

            public int WidthCells => Edges.Count;
            public float Drop => TopY - BottomY;

            /// <summary>Cell-space centre of the front span (edge midpoints).</summary>
            public Vector3 Center
            {
                get
                {
                    if (Edges.Count == 0)
                        return Vector3.zero;
                    float sx = 0f, sz = 0f;
                    for (int i = 0; i < Edges.Count; i++)
                    {
                        var e = Edges[i];
                        sx += e.Cell.x + e.Dir.x * 0.5f;
                        sz += e.Cell.y + e.Dir.y * 0.5f;
                    }
                    return new Vector3(sx / Edges.Count, 0f, sz / Edges.Count);
                }
            }
        }

        public sealed class Field
        {
            public int Width;
            public int Height;
            public float CellSize;
            public float MinDropMeters;
            public readonly List<Front> Fronts = new List<Front>();
            private readonly HashSet<long> _covered = new HashSet<long>();

            public void MarkCovered(Edge edge)
                => _covered.Add(Key(edge.Cell, edge.DirIndex));

            /// <summary>True when a curtain covers the (cell, dir) drop edge.</summary>
            public bool IsCovered(Vector2Int cell, Vector2Int dir)
            {
                for (int i = 0; i < Dirs.Length; i++)
                {
                    if (Dirs[i] == dir)
                        return _covered.Contains(Key(cell, i));
                }
                return false;
            }

            private static long Key(Vector2Int cell, int dirIndex)
                => ((long)(uint)cell.x << 32) ^ ((long)(uint)cell.y << 8) ^ dirIndex;
        }

        /// <param name="waterSheet">Cell renders a water sheet (SurfaceOnly).</param>
        /// <param name="surfaces">Rendered surface height per cell, NaN when absent.</param>
        /// <param name="waterTarget">Cell owns a hydrology water surface (river/lake/sink).</param>
        public static Field Build(
            int width,
            int height,
            float cellSize,
            bool[,] waterSheet,
            float[,] surfaces,
            bool[,] waterTarget,
            float minDropMeters)
        {
            var field = new Field
            {
                Width = width,
                Height = height,
                CellSize = cellSize,
                MinDropMeters = minDropMeters,
            };
            if (width <= 0 || height <= 0 || waterSheet == null
                || surfaces == null || waterTarget == null
                || minDropMeters <= 0.0001f)
            {
                return field;
            }

            var edges = new List<Edge>[Dirs.Length];
            for (int i = 0; i < Dirs.Length; i++)
                edges[i] = new List<Edge>();

            var hasEdge = new bool[width, height, Dirs.Length];

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (!waterSheet[x, y])
                    continue;
                float topY = surfaces[x, y];
                if (!IsFinite(topY))
                    continue;

                var cell = new Vector2Int(x, y);
                for (int i = 0; i < Dirs.Length; i++)
                {
                    var d = Dirs[i];
                    var lower = new Vector2Int(x + d.x, y + d.y);
                    if (lower.x < 0 || lower.y < 0 || lower.x >= width || lower.y >= height)
                        continue;
                    // Hydrology gate: the pour must land on water; a dry
                    // cliff below a wet tile is not a waterfall.
                    if (!waterTarget[lower.x, lower.y])
                        continue;
                    float bottomY = surfaces[lower.x, lower.y];
                    if (!IsFinite(bottomY) || topY - bottomY < minDropMeters)
                        continue;

                    var edge = new Edge
                    {
                        Cell = cell,
                        DirIndex = i,
                        Dir = d,
                        Lower = lower,
                        TopY = topY,
                        BottomY = bottomY,
                    };
                    edges[i].Add(edge);
                    hasEdge[x, y, i] = true;
                }
            }

            SuppressCorneredDiagonals(edges, hasEdge);
            MergeFronts(edges, field);
            return field;
        }

        /*
         * A diagonal drop edge is the corner view of the same D8 pour event
         * when either (a) the upper cell itself pours over both orthogonal
         * flanks at a similar level — the two ortho curtains already tile
         * the corner wedge — or (b) one of the two flank cells adjacent to
         * both upper and lower pours orthogonally into that same lower
         * cell. Emitting the diagonal too would draw a second fall across
         * the same ledge.
         */
        private static void SuppressCorneredDiagonals(
            List<Edge>[] edges,
            bool[,,] hasEdge)
        {
            for (int i = 4; i < Dirs.Length; i++)
            {
                var d = Dirs[i];
                int ownX = OrthoIndex(new Vector2Int(d.x, 0));
                int ownY = OrthoIndex(new Vector2Int(0, d.y));
                for (int k = edges[i].Count - 1; k >= 0; k--)
                {
                    var e = edges[i][k];
                    bool suppress = false;

                    // (a) Corner pour: both own ortho faces exist and land
                    // near the diagonal's lower surface.
                    if (hasEdge[e.Cell.x, e.Cell.y, ownX]
                        && hasEdge[e.Cell.x, e.Cell.y, ownY])
                    {
                        float b1 = BottomFor(e.Cell, ownX, edges);
                        float b2 = BottomFor(e.Cell, ownY, edges);
                        suppress = IsFinite(b1) && IsFinite(b2)
                            && Mathf.Abs(b1 - e.BottomY) <= FrontHeightTolerance
                            && Mathf.Abs(b2 - e.BottomY) <= FrontHeightTolerance;
                    }

                    // (b) Flank pour: the ortho neighbour between upper and
                    // lower already covers this ledge face into the same
                    // lower cell. O1 + (0,dy) == L and O2 + (dx,0) == L
                    // hold by construction.
                    if (!suppress)
                    {
                        var o1 = new Vector2Int(e.Cell.x + d.x, e.Cell.y);
                        var o2 = new Vector2Int(e.Cell.x, e.Cell.y + d.y);
                        if (InBounds(o1, hasEdge)
                            && hasEdge[o1.x, o1.y, ownY]
                            && Mathf.Abs(BottomFor(o1, ownY, edges) - e.BottomY)
                                <= FrontHeightTolerance)
                        {
                            suppress = true;
                        }
                        else if (InBounds(o2, hasEdge)
                            && hasEdge[o2.x, o2.y, ownX]
                            && Mathf.Abs(BottomFor(o2, ownX, edges) - e.BottomY)
                                <= FrontHeightTolerance)
                        {
                            suppress = true;
                        }
                    }

                    if (suppress)
                    {
                        hasEdge[e.Cell.x, e.Cell.y, i] = false;
                        edges[i].RemoveAt(k);
                    }
                }
            }
        }

        private static bool InBounds(Vector2Int cell, bool[,,] hasEdge)
            => cell.x >= 0 && cell.y >= 0
               && cell.x < hasEdge.GetLength(0)
               && cell.y < hasEdge.GetLength(1);

        private static int OrthoIndex(Vector2Int dir)
        {
            for (int i = 0; i < 4; i++)
            {
                if (Dirs[i] == dir)
                    return i;
            }
            return -1;
        }

        private static float BottomFor(Vector2Int cell, int dirIndex, List<Edge>[] edges)
        {
            var list = edges[dirIndex];
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Cell == cell)
                    return list[i].BottomY;
            }
            return float.NaN;
        }

        /*
         * Group edges by direction, then run along the front tangent:
         * consecutive cells one tangent step apart, on the same pour line
         * (equal projection on the fall direction) with similar heights.
         */
        private static void MergeFronts(List<Edge>[] edges, Field field)
        {
            for (int i = 0; i < Dirs.Length; i++)
            {
                var list = edges[i];
                if (list.Count == 0)
                    continue;

                var d = Dirs[i];
                var tangent = new Vector2Int(-d.y, d.x);
                int tangentStep = tangent.x * tangent.x + tangent.y * tangent.y;
                list.Sort((a, b) =>
                {
                    int sa = a.Cell.x * tangent.x + a.Cell.y * tangent.y;
                    int sb = b.Cell.x * tangent.x + b.Cell.y * tangent.y;
                    int cmp = sa.CompareTo(sb);
                    if (cmp != 0)
                        return cmp;
                    int pa = a.Cell.x * d.x + a.Cell.y * d.y;
                    int pb = b.Cell.x * d.x + b.Cell.y * d.y;
                    return pa.CompareTo(pb);
                });

                Front run = null;
                int prevS = 0, prevP = 0;
                for (int k = 0; k < list.Count; k++)
                {
                    var e = list[k];
                    int s = e.Cell.x * tangent.x + e.Cell.y * tangent.y;
                    int p = e.Cell.x * d.x + e.Cell.y * d.y;
                    bool continues = run != null
                        && s - prevS == tangentStep
                        && p == prevP
                        && Mathf.Abs(e.TopY - run.TopY) <= FrontHeightTolerance
                        && Mathf.Abs(e.BottomY - run.BottomY) <= FrontHeightTolerance;
                    if (!continues)
                    {
                        run = new Front { Dir = d };
                        field.Fronts.Add(run);
                    }
                    run.Edges.Add(e);
                    run.TopY = Mathf.Max(run.TopY, e.TopY);
                    run.BottomY = Mathf.Min(run.BottomY, e.BottomY);
                    run.Anchor = run.Edges[run.Edges.Count / 2].Cell;
                    field.MarkCovered(e);
                    prevS = s;
                    prevP = p;
                }
            }
        }

        private static bool IsFinite(float v)
            => !float.IsNaN(v) && !float.IsInfinity(v);
    }
}
