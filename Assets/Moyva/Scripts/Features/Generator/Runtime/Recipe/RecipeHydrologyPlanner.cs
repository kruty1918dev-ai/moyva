using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;
using GeographyNoise = Kruty1918.Moyva.Generator.Runtime.Geography.DeterministicNoise;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Result of <see cref="RecipeHydrologyPlanner"/>: masks plus per-cell data
    /// consumers need (water surface heights in meters, flow parents for
    /// waterfall/ford queries).
    /// </summary>
    internal sealed class RecipeHydrologyPlan
    {
        public bool[,] RiverMask;
        public bool[,] LakeMask;
        public bool[,] WaterfallMask;
        /// <summary>Water surface height in meters per river/lake/sink cell; NaN elsewhere.</summary>
        public float[,] WaterSurface;
        /// <summary>Channel/lake bed height in meters per water cell; NaN elsewhere.</summary>
        public float[,] BedHeight;
        /// <summary>Flooded terrain level per cell (priority-flood output).</summary>
        public float[,] Filled;
        /// <summary>Flattened index of the downstream cell; -1 at sinks.</summary>
        public int[,] FlowParent;
        public float[,] Accumulation;
        /// <summary>Drop threshold for waterfall edges (meters).</summary>
        public float WaterfallMinDropMeters;
        public int RiverCellCount;
        public int LakeCellCount;
    }

    /// <summary>
    /// Deterministic hydrology on the recipe relief field (meters):
    /// priority-flood depression fill, D8 drainage via flood parents, flow
    /// accumulation, then lake and river marking. Sinks are the configured
    /// open-water layer mask plus the map border, so every cell drains.
    /// </summary>
    internal static class RecipeHydrologyPlanner
    {
        private static readonly int[] Dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static readonly int[] Dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

        public static RecipeHydrologyPlan Build(
            float[,] terrain,
            bool[,] sinkMask,
            Vector2Int mapSize,
            RecipeHydrologyConfig config,
            int seed)
        {
            int w = Mathf.Max(1, mapSize.x);
            int h = Mathf.Max(1, mapSize.y);
            var empty = EmptyPlan(w, h);
            if (terrain == null || config == null || !config.Enabled)
                return empty;
            if (terrain.GetLength(0) != w || terrain.GetLength(1) != h)
                return empty;

            var filled = PriorityFlood(terrain, sinkMask, w, h, seed,
                config.SinkMaxMeters, out var parent);
            var accumulation = Accumulate(filled, parent, w, h);
            var riverMask = new bool[w, h];
            var lakeMask = new bool[w, h];
            var waterfallMask = new bool[w, h];
            var waterSurface = new float[w, h];
            var bedHeight = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                waterSurface[x, y] = float.NaN;
                bedHeight[x, y] = float.NaN;
            }

            float offset = config.WaterSurfaceOffsetMeters;
            int lakeCells = MarkLakes(
                terrain, filled, sinkMask, lakeMask, waterSurface, bedHeight,
                w, h, config, offset);
            int riverCells = MarkRivers(
                terrain, filled, accumulation, riverMask, lakeMask, sinkMask,
                waterSurface, bedHeight, w, h, config, offset);
            MarkWaterfalls(filled, parent, riverMask, lakeMask,
                waterfallMask, w, h, config.WaterfallMinDropMeters);

            return new RecipeHydrologyPlan
            {
                RiverMask = riverMask,
                LakeMask = lakeMask,
                WaterfallMask = waterfallMask,
                WaterSurface = waterSurface,
                BedHeight = bedHeight,
                Filled = filled,
                FlowParent = parent,
                Accumulation = accumulation,
                WaterfallMinDropMeters = config.WaterfallMinDropMeters,
                RiverCellCount = riverCells,
                LakeCellCount = lakeCells,
            };
        }

        /// <summary>
        /// Merges plans built for different sink keys into one plan: masks are
        /// OR-ed and per-cell data takes the first finite value (plans are
        /// evaluated against the same field, so conflicts are benign).
        /// </summary>
        public static RecipeHydrologyPlan Merge(IEnumerable<RecipeHydrologyPlan> plans)
        {
            if (plans == null)
                return null;
            RecipeHydrologyPlan first = null;
            foreach (var plan in plans)
            {
                if (plan?.RiverMask == null)
                    continue;
                if (first == null)
                {
                    first = plan;
                    continue;
                }
                MergeInto(first, plan);
            }
            return first;
        }

        private static void MergeInto(RecipeHydrologyPlan target, RecipeHydrologyPlan source)
        {
            int w = Mathf.Min(
                target.RiverMask.GetLength(0),
                source.RiverMask.GetLength(0));
            int h = Mathf.Min(
                target.RiverMask.GetLength(1),
                source.RiverMask.GetLength(1));
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (source.RiverMask[x, y] && !target.RiverMask[x, y])
                {
                    target.RiverMask[x, y] = true;
                    target.RiverCellCount++;
                }
                if (source.LakeMask[x, y] && !target.LakeMask[x, y])
                {
                    target.LakeMask[x, y] = true;
                    target.LakeCellCount++;
                }
                if (source.WaterfallMask[x, y])
                    target.WaterfallMask[x, y] = true;
                if (float.IsNaN(target.WaterSurface[x, y])
                    && !float.IsNaN(source.WaterSurface[x, y]))
                    target.WaterSurface[x, y] = source.WaterSurface[x, y];
                if (float.IsNaN(target.BedHeight[x, y])
                    && !float.IsNaN(source.BedHeight[x, y]))
                    target.BedHeight[x, y] = source.BedHeight[x, y];
                if (target.FlowParent[x, y] < 0 && source.FlowParent[x, y] >= 0)
                    target.FlowParent[x, y] = source.FlowParent[x, y];
            }
            if (target.WaterfallMinDropMeters <= 0f)
                target.WaterfallMinDropMeters = source.WaterfallMinDropMeters;
        }

        private static RecipeHydrologyPlan EmptyPlan(int w, int h)
        {
            return new RecipeHydrologyPlan
            {
                RiverMask = new bool[w, h],
                LakeMask = new bool[w, h],
                WaterfallMask = new bool[w, h],
                WaterSurface = new float[w, h],
                BedHeight = new float[w, h],
                Filled = new float[w, h],
                FlowParent = new int[w, h],
                Accumulation = new float[w, h],
            };
        }

        /// <summary>
        /// Sinks are the configured open-water layer mask plus the map border.
        /// The low-terrain clause is only a fallback for recipes without a
        /// sink layer: applying it alongside a mask would turn every interior
        /// lowland into a drain endpoint and stop flow accumulation before
        /// rivers can form.
        /// </summary>
        private static bool IsSink(bool[,] sinkMask, float[,] terrain,
            float sinkMaxMeters, int x, int y, int w, int h)
        {
            if (x == 0 || y == 0 || x == w - 1 || y == h - 1)
                return true;
            if (sinkMask != null)
                return x >= 0 && y >= 0
                       && x < sinkMask.GetLength(0)
                       && y < sinkMask.GetLength(1)
                       && sinkMask[x, y];
            return terrain != null && terrain[x, y] <= sinkMaxMeters;
        }

        /// <summary>
        /// Barnes-style priority flood seeded from sinks and the border: fills
        /// depressions so every cell drains downhill to a sink. Records the
        /// flood parent per cell — the deterministic downstream link.
        /// </summary>
        private static float[,] PriorityFlood(
            float[,] terrain, bool[,] sinkMask, int w, int h, int seed,
            float sinkMaxMeters, out int[,] parent)
        {
            var filled = new float[w, h];
            parent = new int[w, h];
            var visited = new bool[w, h];
            var heap = new MinHeap(w * h / 4 + 16);

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (!IsSink(sinkMask, terrain, sinkMaxMeters, x, y, w, h))
                    continue;
                filled[x, y] = terrain[x, y];
                parent[x, y] = -1;
                visited[x, y] = true;
                heap.Push(terrain[x, y], x + y * w,
                    GeographyNoise.HashU32(seed, x, y, 11));
            }

            while (heap.Count > 0)
            {
                heap.Pop(out float level, out int index);
                int cx = index % w;
                int cy = index / w;
                for (int d = 0; d < 8; d++)
                {
                    int nx = cx + Dx[d];
                    int ny = cy + Dy[d];
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h || visited[nx, ny])
                        continue;

                    visited[nx, ny] = true;
                    float nLevel = Mathf.Max(terrain[nx, ny], level);
                    filled[nx, ny] = nLevel;
                    parent[nx, ny] = index;
                    heap.Push(nLevel, nx + ny * w,
                        GeographyNoise.HashU32(seed, nx, ny, 11));
                }
            }

            return filled;
        }

        /// <summary>Flow accumulation: process cells in descending filled order.</summary>
        private static float[,] Accumulate(float[,] filled, int[,] parent, int w, int h)
        {
            var acc = new float[w, h];
            var order = new int[w * h];
            for (int i = 0; i < order.Length; i++) order[i] = i;
            System.Array.Sort(order, (a, b) =>
                filled[b % w, b / w].CompareTo(filled[a % w, a / w]));

            for (int i = 0; i < order.Length; i++)
            {
                int index = order[i];
                int x = index % w;
                int y = index / w;
                acc[x, y] += 1f;
                int p = parent[x, y];
                if (p >= 0)
                    acc[p % w, p / w] += acc[x, y];
            }
            return acc;
        }

        private static int MarkLakes(
            float[,] terrain, float[,] filled, bool[,] sinkMask,
            bool[,] lakeMask, float[,] waterSurface, float[,] bedHeight,
            int w, int h, RecipeHydrologyConfig config, float offset)
        {
            int maxLakeCells = Mathf.RoundToInt(w * h * config.LakeMaxFraction);
            int count = 0;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (IsSink(sinkMask, terrain, config.SinkMaxMeters, x, y, w, h))
                {
                    waterSurface[x, y] = terrain[x, y] + offset;
                    bedHeight[x, y] = terrain[x, y];
                    continue;
                }

                float depth = filled[x, y] - terrain[x, y];
                if (depth >= config.LakeMinDepthMeters && count < maxLakeCells)
                {
                    lakeMask[x, y] = true;
                    count++;
                    waterSurface[x, y] = filled[x, y] + offset;
                    // Lake bed is the real depression floor.
                    bedHeight[x, y] = Mathf.Min(
                        terrain[x, y],
                        filled[x, y] + offset - 0.05f);
                }
            }
            return count;
        }

        private static int MarkRivers(
            float[,] terrain, float[,] filled, float[,] accumulation,
            bool[,] riverMask, bool[,] lakeMask, bool[,] sinkMask,
            float[,] waterSurface, float[,] bedHeight, int w, int h,
            RecipeHydrologyConfig config, float offset)
        {
            int maxRiverCells = Mathf.Max(4, Mathf.RoundToInt(
                w * h * config.RiverMaxFraction));
            float threshold = Mathf.Max(2f, config.RiverAccumulationThreshold);
            float depth = Mathf.Max(0.05f, config.ChannelDepthMeters);
            int count = 0;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (count >= maxRiverCells)
                    break;
                if (IsSink(sinkMask, terrain, config.SinkMaxMeters, x, y, w, h)
                    || riverMask[x, y]
                    || lakeMask[x, y])
                    continue;
                if (accumulation[x, y] < threshold)
                    continue;
                if (terrain[x, y] < config.RiverMinSourceMeters)
                    continue;

                riverMask[x, y] = true;
                waterSurface[x, y] = filled[x, y] + offset;
                // Bed is the deeper of the carved channel and the real floor:
                // on flooded segments the submerged terrain lies below the
                // nominal channel depth and is the true water bottom.
                bedHeight[x, y] = Mathf.Min(
                    terrain[x, y],
                    waterSurface[x, y] - depth);
                count++;
            }
            return count;
        }

        /// <summary>
        /// Water cells (rivers, lake cells and lake outflows) whose flooded
        /// level drops at least <paramref name="minDrop"/> into the downstream
        /// cell mark a waterfall edge. The flooded level — not raw terrain —
        /// is compared so flat lake floors never fake a drop inside the basin.
        /// </summary>
        private static void MarkWaterfalls(
            float[,] filled, int[,] parent, bool[,] riverMask,
            bool[,] lakeMask,
            bool[,] waterfallMask, int w, int h, float minDrop)
        {
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (!riverMask[x, y] && !lakeMask[x, y])
                    continue;
                int p = parent[x, y];
                if (p < 0)
                    continue;
                int px = p % w;
                int py = p / w;
                // Map-border cells are drainage exits, not rendered water —
                // a fall into an invisible drain would draw a strip hanging
                // off the world edge.
                if (px == 0 || py == 0 || px == w - 1 || py == h - 1)
                    continue;
                float drop = filled[x, y] - filled[px, py];
                if (drop >= minDrop)
                    waterfallMask[x, y] = true;
            }
        }

        /// <summary>Binary min-heap on (level, tiebreak) — deterministic ordering.</summary>
        private struct MinHeap
        {
            private float[] _keys;
            private int[] _cells;
            private uint[] _tie;
            public int Count { get; private set; }

            public MinHeap(int capacity)
            {
                _keys = new float[capacity];
                _cells = new int[capacity];
                _tie = new uint[capacity];
                Count = 0;
            }

            public void Push(float key, int cell, uint tie)
            {
                if (Count == _keys.Length)
                {
                    System.Array.Resize(ref _keys, _keys.Length * 2);
                    System.Array.Resize(ref _cells, _cells.Length * 2);
                    System.Array.Resize(ref _tie, _tie.Length * 2);
                }
                int i = Count++;
                _keys[i] = key;
                _cells[i] = cell;
                _tie[i] = tie;
                while (i > 0)
                {
                    int p = (i - 1) >> 1;
                    if (Less(i, p)) { Swap(i, p); i = p; }
                    else break;
                }
            }

            public void Pop(out float key, out int cell)
            {
                key = _keys[0];
                cell = _cells[0];
                Count--;
                _keys[0] = _keys[Count];
                _cells[0] = _cells[Count];
                _tie[0] = _tie[Count];
                int i = 0;
                while (true)
                {
                    int l = i * 2 + 1, r = l + 1, best = i;
                    if (l < Count && Less(l, best)) best = l;
                    if (r < Count && Less(r, best)) best = r;
                    if (best == i) break;
                    Swap(i, best);
                    i = best;
                }
            }

            private bool Less(int a, int b)
            {
                if (_keys[a] != _keys[b]) return _keys[a] < _keys[b];
                return _tie[a] < _tie[b];
            }

            private void Swap(int a, int b)
            {
                (_keys[a], _keys[b]) = (_keys[b], _keys[a]);
                (_cells[a], _cells[b]) = (_cells[b], _cells[a]);
                (_tie[a], _tie[b]) = (_tie[b], _tie[a]);
            }
        }
    }
}
