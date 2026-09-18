using System.Collections.Generic;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.Geography
{
    /// <summary>
    /// Deterministic hydrology on the integer level field:
    /// priority-flood depression fill → D8 drainage → flow accumulation →
    /// rivers (with natural tributary merging) and lakes. Every cell drains
    /// to the map border by construction.
    /// </summary>
    internal sealed class HydrologyStage
    {
        internal sealed class Output
        {
            public int[,] Levels;               // possibly adjusted by lakes
            public bool[,] RiverMask;
            public bool[,] LakeMask;
            public float[,] WaterSurface;       // surface Y per water/river/lake cell
            public int[,] FlowParent;           // flattened index of downstream cell, -1 = sink
            public float[,] Accumulation;
            public int RiverCellCount;
            public int LakeCellCount;
        }

        private static readonly int[] Dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static readonly int[] Dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

        internal Output Generate(WorldGenerationRequest request, int[,] levels)
        {
            int w = request.Width;
            int h = request.Height;
            int water = request.WaterLevel;
            var hydro = request.Config.Hydrology;
            int seed = request.Seed;
            var p = request.Config.FindArchetype(request.ArchetypeId());

            var filled = PriorityFlood(levels, w, h, water, seed, out var parent);
            var accumulation = Accumulate(filled, parent, w, h);

            var riverMask = new bool[w, h];
            var lakeMask = new bool[w, h];
            var waterSurface = new float[w, h];
            float step = request.HeightStep;
            float offset = request.WaterSurfaceOffset;

            // Lakes: flooded depressions deeper than threshold, area-capped.
            int lakeCells = MarkLakes(levels, filled, lakeMask, waterSurface, water, w, h, hydro, step, offset);

            // Rivers: land cells whose accumulated upstream drainage exceeds the
            // archetype-scaled threshold. Flow follows the flood parent graph, so
            // tributaries merge by construction.
            float threshold = Mathf.Max(
                2f,
                hydro.RiverAccumulationThreshold / Mathf.Max(0.05f, p.RiverDensity));
            int maxRiverCells = Mathf.Max(8, Mathf.RoundToInt(
                w * h * hydro.RiversPerThousandCells / 1000f * 10f));
            int riverCells = MarkRivers(
                levels, filled, accumulation, riverMask, lakeMask, waterSurface,
                water, request.LandLevel, hydro.RiverMinSourceLevel,
                threshold, maxRiverCells, w, h, step, offset);

            return new Output
            {
                Levels = levels,
                RiverMask = riverMask,
                LakeMask = lakeMask,
                WaterSurface = waterSurface,
                FlowParent = parent,
                Accumulation = accumulation,
                RiverCellCount = riverCells,
                LakeCellCount = lakeCells,
            };
        }

        /// <summary>
        /// Barnes-style priority flood: fills all depressions so every cell can
        /// drain downhill to the border. Records flood parent per cell — that is
        /// the deterministic downstream link used by rivers.
        /// </summary>
        private float[,] PriorityFlood(int[,] levels, int w, int h, int waterLevel, int seed, out int[,] parent)
        {
            var filled = new float[w, h];
            parent = new int[w, h];
            var visited = new bool[w, h];
            var heap = new MinHeap(w * h / 4 + 16);

            // Border cells + all water cells are the flood sources.
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                bool border = x == 0 || y == 0 || x == w - 1 || y == h - 1;
                if (!border && levels[x, y] != waterLevel)
                    continue;
                filled[x, y] = levels[x, y];
                parent[x, y] = -1;
                visited[x, y] = true;
                heap.Push(levels[x, y], x + y * w, DeterministicNoise.HashU32(seed, x, y, 11));
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
                    float nLevel = Mathf.Max(levels[nx, ny], level);
                    filled[nx, ny] = nLevel;
                    parent[nx, ny] = index;
                    heap.Push(nLevel, nx + ny * w, DeterministicNoise.HashU32(seed, nx, ny, 11));
                }
            }

            return filled;
        }

        /// <summary>Flow accumulation: process cells in descending filled order.</summary>
        private float[,] Accumulate(float[,] filled, int[,] parent, int w, int h)
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

        private int MarkLakes(
            int[,] levels, float[,] filled, bool[,] lakeMask, float[,] waterSurface,
            int waterLevel, int w, int h, API.WorldGenerationConfig.HydrologySettings hydro,
            float step, float offset)
        {
            int maxLakeCells = Mathf.RoundToInt(w * h * hydro.LakeMaxFraction);
            int count = 0;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (levels[x, y] == waterLevel)
                {
                    waterSurface[x, y] = waterLevel * step + offset;
                    continue;
                }

                float depth = filled[x, y] - levels[x, y];
                if (depth >= hydro.LakeMinDepth && count < maxLakeCells)
                {
                    lakeMask[x, y] = true;
                    count++;
                    waterSurface[x, y] = filled[x, y] * step + offset;
                }
            }
            return count;
        }

        private int MarkRivers(
            int[,] levels, float[,] filled, float[,] accumulation, bool[,] riverMask,
            bool[,] lakeMask, float[,] waterSurface, int waterLevel, int landLevel,
            int minSourceLevel, float threshold, int maxRiverCells, int w, int h,
            float step, float offset)
        {
            int count = 0;
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (levels[x, y] == waterLevel || riverMask[x, y] || lakeMask[x, y])
                    continue;
                if (accumulation[x, y] < threshold || count >= maxRiverCells)
                    continue;
                if (levels[x, y] < landLevel)
                    continue;

                riverMask[x, y] = true;
                waterSurface[x, y] = levels[x, y] * step + offset;
                count++;
            }
            return count;
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
