using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Detects candidate corridors where existing terrain could carry water
    /// between two separate water regions. Read-only: consumes the exported
    /// logical map after the terrain plan is applied and returns a pure-data
    /// plan — it never mutates tiles, heights or water.
    ///
    /// Flow: 4-connected water regions → one multi-source Voronoi BFS finds
    /// region pairs close enough to matter → per pair a deterministic
    /// bottleneck search returns the lowest-saddle land corridor → the corridor
    /// is measured against the endpoint water surfaces and classified.
    /// Fully deterministic: fixed iteration order, no RNG, no seed.
    /// </summary>
    internal static class WaterChannelCandidatePlanner
    {
        private const float Epsilon = 0.001f;

        // Fixed 4-neighbour order so BFS/search ties resolve deterministically.
        private static readonly int[] Dx = { -1, 1, 0, 0 };
        private static readonly int[] Dy = { 0, 0, -1, 1 };

        /// <summary>
        /// Extracts the water/surface view from a finished logical map: a cell
        /// counts as water when its winning compatibility sample renders a
        /// water sheet (surface-only terrain) or carries a water-like tile id —
        /// the same test the shoreline pass applies to the map winner.
        /// </summary>
        public static WaterChannelCandidatePlan Build(
            LogicalTileMap map,
            string[] waterLikeTileIds,
            RecipeChannelDetectionConfig config)
        {
            if (map == null || config == null || !config.Enabled)
                return new WaterChannelCandidatePlan();

            int width = map.Width;
            int height = map.Height;
            var wet = new bool[width, height];
            var surface = new float[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                surface[x, y] = float.NaN;
                TileStackCell stack = map.GetCellStack(x, y);
                if (stack == null
                    || !stack.TryGetTopCompatibilitySample(out TileLayerSample winner)
                    || !winner.IsTerrainLike)
                {
                    continue;
                }

                surface[x, y] = IsFinite(winner.SurfaceHeight)
                    ? winner.SurfaceHeight
                    : IsFinite(winner.Height) ? winner.Height : float.NaN;
                wet[x, y] = winner.TileGeometryMode == TileGeometryMode.SurfaceOnly
                    || IsWaterId(winner.TileId, waterLikeTileIds);
            }

            return Build(wet, surface, config);
        }

        /// <summary>
        /// Core detection over plain masks: <paramref name="wet"/> marks water
        /// cells, <paramref name="surface"/> is the rendered surface in meters
        /// (water surface for wet cells, terrain for land). Land cells with a
        /// non-finite surface are treated as untraversable barriers.
        /// </summary>
        public static WaterChannelCandidatePlan Build(
            bool[,] wet,
            float[,] surface,
            RecipeChannelDetectionConfig config)
        {
            var plan = new WaterChannelCandidatePlan();
            if (wet == null || surface == null || config == null || !config.Enabled)
                return plan;

            int width = wet.GetLength(0);
            int height = wet.GetLength(1);
            if (surface.GetLength(0) != width || surface.GetLength(1) != height)
                return plan;

            int maxLength = Mathf.Max(2, config.MaxCorridorLengthCells);
            int minRegionCells = Mathf.Max(1, config.MinRegionCells);

            // Water regions: 4-connected components — water sheets only share
            // faces across cardinal edges, matching how channels would join.
            var region = new int[width * height];
            for (int i = 0; i < region.Length; i++)
                region[i] = -1;
            var regions = LabelWaterRegions(wet, surface, region, width, height);
            plan.Regions = regions.ToArray();
            if (regions.Count < 2)
                return plan;

            var traversable = new bool[width * height];
            for (int i = 0; i < traversable.Length; i++)
                traversable[i] = !wet[X(i, width), Y(i, width)]
                    && IsFinite(surface[X(i, width), Y(i, width)]);

            // Land components: needed once to measure whether flooding a
            // corridor would sever a landmass (isthmus/land-bridge impact).
            var landComp = new int[width * height];
            var landMembers = LabelLandComponents(traversable, landComp, width, height);

            // Frontier: traversable cells touching each region's water.
            var frontier = BuildFrontiers(wet, region, traversable, regions.Count, width, height);

            // Coarse stage: one Voronoi BFS discovers which region pairs can
            // meet within corridor range at all — no all-pairs frontier scans.
            var pairs = DiscoverPairs(
                wet, region, traversable, regions, minRegionCells, maxLength, width, height);

            pairs.Sort((a, b) =>
            {
                int byGap = a.Gap.CompareTo(b.Gap);
                return byGap != 0 ? byGap : a.Key.CompareTo(b.Key);
            });
            int budget = Mathf.Max(1, config.MaxPairEvaluations);
            if (pairs.Count > budget)
                pairs.RemoveRange(budget, pairs.Count - budget);

            var search = new CorridorSearch(
                wet, surface, region, traversable, width, height, maxLength,
                Mathf.Max(64, config.MaxVisitedCells));

            foreach (PairHit pair in pairs)
            {
                if (pair.Gap > maxLength)
                    break; // sorted by gap: the rest are only farther apart

                if (frontier[pair.A] == null || frontier[pair.A].Count == 0
                    || frontier[pair.B] == null || frontier[pair.B].Count == 0)
                {
                    plan.Rejections.Add(new WaterChannelRejection(
                        pair.A, pair.B, WaterChannelRejectReason.NoCorridor));
                    continue;
                }

                Evaluate(
                    search, wet, surface, region, landComp, landMembers,
                    regions, frontier, pair.A, pair.B, config, plan);
            }

            plan.Candidates.Sort(CompareCandidates);
            plan.Rejections.Sort((a, b) =>
            {
                int byA = a.RegionA.CompareTo(b.RegionA);
                if (byA != 0) return byA;
                int byB = a.RegionB.CompareTo(b.RegionB);
                return byB != 0 ? byB : a.Reason.CompareTo(b.Reason);
            });
            return plan;
        }

        private static int X(int index, int width) => index % width;
        private static int Y(int index, int width) => index / width;

        // ------------------------------------------------------------------
        // Regions, land components, frontiers
        // ------------------------------------------------------------------

        private static List<WaterChannelRegionInfo> LabelWaterRegions(
            bool[,] wet, float[,] surface, int[] region, int width, int height)
        {
            var regions = new List<WaterChannelRegionInfo>();
            var queue = new Queue<int>();
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                int start = y * width + x;
                if (!wet[x, y] || region[start] >= 0)
                    continue;

                int id = regions.Count;
                float min = float.PositiveInfinity;
                float max = float.NegativeInfinity;
                int count = 0;
                region[start] = id;
                queue.Enqueue(start);
                while (queue.Count > 0)
                {
                    int c = queue.Dequeue();
                    count++;
                    float s = surface[X(c, width), Y(c, width)];
                    if (IsFinite(s))
                    {
                        if (s < min) min = s;
                        if (s > max) max = s;
                    }

                    for (int d = 0; d < 4; d++)
                    {
                        int nx = X(c, width) + Dx[d];
                        int ny = Y(c, width) + Dy[d];
                        if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                            continue;
                        int n = ny * width + nx;
                        if (wet[nx, ny] && region[n] < 0)
                        {
                            region[n] = id;
                            queue.Enqueue(n);
                        }
                    }
                }

                regions.Add(new WaterChannelRegionInfo
                {
                    Id = id,
                    CellCount = count,
                    MinSurfaceMeters = IsFinite(min) ? min : float.NaN,
                    MaxSurfaceMeters = IsFinite(max) ? max : float.NaN,
                });
            }

            return regions;
        }

        private static List<List<int>> LabelLandComponents(
            bool[] traversable, int[] landComp, int width, int height)
        {
            var members = new List<List<int>>();
            var queue = new Queue<int>();
            for (int i = 0; i < landComp.Length; i++)
                landComp[i] = -1;
            for (int i = 0; i < traversable.Length; i++)
            {
                if (!traversable[i] || landComp[i] >= 0)
                    continue;

                int id = members.Count;
                var cells = new List<int>();
                landComp[i] = id;
                queue.Enqueue(i);
                while (queue.Count > 0)
                {
                    int c = queue.Dequeue();
                    cells.Add(c);
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = X(c, width) + Dx[d];
                        int ny = Y(c, width) + Dy[d];
                        if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                            continue;
                        int n = ny * width + nx;
                        if (traversable[n] && landComp[n] < 0)
                        {
                            landComp[n] = id;
                            queue.Enqueue(n);
                        }
                    }
                }

                members.Add(cells);
            }

            return members;
        }

        private static List<int>[] BuildFrontiers(
            bool[,] wet, int[] region, bool[] traversable, int regionCount,
            int width, int height)
        {
            var frontier = new List<int>[regionCount];
            for (int i = 0; i < traversable.Length; i++)
            {
                if (!traversable[i])
                    continue;

                // Up to four adjacent water neighbours; collect distinct
                // region ids without a hash set.
                int r0 = -1, r1 = -1, r2 = -1, r3 = -1, found = 0;
                for (int d = 0; d < 4; d++)
                {
                    int nx = X(i, width) + Dx[d];
                    int ny = Y(i, width) + Dy[d];
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                        continue;
                    int n = ny * width + nx;
                    if (!wet[nx, ny])
                        continue;
                    int r = region[n];
                    if (r < 0 || r == r0 || r == r1 || r == r2 || r == r3)
                        continue;
                    if (found == 0) r0 = r;
                    else if (found == 1) r1 = r;
                    else if (found == 2) r2 = r;
                    else r3 = r;
                    found++;
                }

                for (int k = 0; k < found; k++)
                {
                    int r = k == 0 ? r0 : k == 1 ? r1 : k == 2 ? r2 : r3;
                    (frontier[r] ??= new List<int>()).Add(i);
                }
            }

            return frontier;
        }

        // ------------------------------------------------------------------
        // Coarse pair discovery: multi-source Voronoi BFS
        // ------------------------------------------------------------------

        private sealed class PairHit
        {
            public long Key;
            public int A;
            public int B;
            public int Gap;
        }

        private static List<PairHit> DiscoverPairs(
            bool[,] wet,
            int[] region,
            bool[] traversable,
            List<WaterChannelRegionInfo> regions,
            int minRegionCells,
            int maxLength,
            int width,
            int height)
        {
            var claims = new int[width * height];
            var dist = new int[width * height];
            for (int i = 0; i < claims.Length; i++)
            {
                claims[i] = -1;
                dist[i] = -1;
            }

            var queue = new Queue<int>();
            var pairMap = new Dictionary<long, PairHit>();

            // Seed with every water cell; queue order is scan order, so ties in
            // Voronoi ownership resolve deterministically to the lower id.
            for (int i = 0; i < claims.Length; i++)
            {
                int r = region[i];
                if (r < 0)
                    continue;
                claims[i] = r;
                dist[i] = 0;
                queue.Enqueue(i);
            }

            while (queue.Count > 0)
            {
                int c = queue.Dequeue();
                int rc = claims[c];
                int dc = dist[c];
                for (int d = 0; d < 4; d++)
                {
                    int nx = X(c, width) + Dx[d];
                    int ny = Y(c, width) + Dy[d];
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                        continue;
                    int n = ny * width + nx;

                    if (wet[nx, ny])
                    {
                        int rn = region[n];
                        if (rn >= 0 && rn != rc)
                            RecordPair(pairMap, rc, rn, dc, regions, minRegionCells);
                        continue;
                    }

                    if (!traversable[n])
                        continue;

                    if (claims[n] < 0)
                    {
                        claims[n] = rc;
                        dist[n] = dc + 1;
                        // Fronts deeper than the corridor budget cannot produce
                        // an accepted pair; no point expanding beyond them.
                        if (dist[n] < maxLength)
                            queue.Enqueue(n);
                        continue;
                    }

                    if (claims[n] != rc)
                        RecordPair(pairMap, rc, claims[n], dc + dist[n], regions, minRegionCells);
                }
            }

            return new List<PairHit>(pairMap.Values);
        }

        private static void RecordPair(
            Dictionary<long, PairHit> pairs,
            int ra,
            int rb,
            int gap,
            List<WaterChannelRegionInfo> regions,
            int minRegionCells)
        {
            int a = Mathf.Min(ra, rb);
            int b = Mathf.Max(ra, rb);
            if (regions[a].CellCount < minRegionCells
                || regions[b].CellCount < minRegionCells)
            {
                return;
            }

            long key = (long)a * 65536L + b;
            if (pairs.TryGetValue(key, out PairHit hit))
            {
                if (gap < hit.Gap)
                    hit.Gap = gap;
                return;
            }

            pairs[key] = new PairHit { Key = key, A = a, B = b, Gap = gap };
        }

        // ------------------------------------------------------------------
        // Fine stage: lexicographic bottleneck search + evaluation
        // ------------------------------------------------------------------

        /// <summary>
        /// Per-pair corridor search. The cost key is (peak surface, length):
        /// the natural spill point is the lowest saddle, and among equal
        /// saddles the shorter corridor needs less work. Both parts are
        /// monotone along any extension, so the priority-first scan settles
        /// each cell with its optimal key.
        /// </summary>
        private sealed class CorridorSearch
        {
            private readonly float[] _peak;
            private readonly int[] _length;
            private readonly int[] _parent;
            private readonly byte[] _state; // 0 unseen, 1 open, 2 closed
            private readonly List<int> _touched = new();
            private readonly List<int> _open = new();

            private readonly float[,] _surface;
            private readonly int[] _region;
            private readonly bool[] _traversable;
            private readonly bool[,] _wet;
            private readonly int _width;
            private readonly int _height;
            private readonly int _maxLength;
            private readonly int _maxVisited;

            public CorridorSearch(
                bool[,] wet,
                float[,] surface,
                int[] region,
                bool[] traversable,
                int width,
                int height,
                int maxLength,
                int maxVisited)
            {
                _wet = wet;
                _surface = surface;
                _region = region;
                _traversable = traversable;
                _width = width;
                _height = height;
                _maxLength = maxLength;
                _maxVisited = maxVisited;
                int cells = width * height;
                _peak = new float[cells];
                _length = new int[cells];
                _parent = new int[cells];
                _state = new byte[cells];
            }

            public bool OutOfBudget { get; private set; }

            public List<int> Find(
                List<int> sources,
                int targetRegion,
                RectInt bounds)
            {
                OutOfBudget = false;
                foreach (int i in _touched)
                    _state[i] = 0;
                _touched.Clear();
                _open.Clear();

                foreach (int s in sources)
                {
                    if (_state[s] != 0)
                        continue;
                    _state[s] = 1;
                    _peak[s] = _surface[X(s, _width), Y(s, _width)];
                    _length[s] = 0;
                    _parent[s] = -1;
                    _open.Add(s);
                    _touched.Add(s);
                }

                int visited = 0;
                while (_open.Count > 0)
                {
                    int bestIndex = 0;
                    for (int i = 1; i < _open.Count; i++)
                    {
                        int candidate = _open[i];
                        int current = _open[bestIndex];
                        if (_peak[candidate] < _peak[current] - Epsilon
                            || (Mathf.Abs(_peak[candidate] - _peak[current]) <= Epsilon
                                && (_length[candidate] < _length[current]
                                    || (_length[candidate] == _length[current]
                                        && candidate < current))))
                        {
                            bestIndex = i;
                        }
                    }

                    int c = _open[bestIndex];
                    _open.RemoveAt(bestIndex);
                    if (_state[c] == 2)
                        continue;
                    _state[c] = 2;
                    visited++;

                    if (TouchesRegion(c, targetRegion))
                    {
                        var path = new List<int>();
                        for (int p = c; p >= 0; p = _parent[p])
                            path.Add(p);
                        path.Reverse();
                        return path;
                    }

                    if (visited >= _maxVisited)
                    {
                        OutOfBudget = true;
                        continue;
                    }
                    if (_length[c] >= _maxLength)
                        continue;

                    int cx = X(c, _width);
                    int cy = Y(c, _width);
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = cx + Dx[d];
                        int ny = cy + Dy[d];
                        if (nx < bounds.xMin || ny < bounds.yMin
                            || nx >= bounds.xMax || ny >= bounds.yMax)
                        {
                            continue;
                        }

                        int n = ny * _width + nx;
                        if (!_traversable[n] || _state[n] == 2)
                            continue;

                        float peak = Mathf.Max(_peak[c], _surface[nx, ny]);
                        int length = _length[c] + 1;
                        if (_state[n] == 0)
                        {
                            _state[n] = 1;
                            _peak[n] = peak;
                            _length[n] = length;
                            _parent[n] = c;
                            _open.Add(n);
                            _touched.Add(n);
                            continue;
                        }

                        // Equal keys keep the first parent — pop order is
                        // deterministic, so ties never flip between runs.
                        bool better = peak < _peak[n] - Epsilon
                            || (Mathf.Abs(peak - _peak[n]) <= Epsilon
                                && length < _length[n]);
                        if (better)
                        {
                            _peak[n] = peak;
                            _length[n] = length;
                            _parent[n] = c;
                        }
                    }
                }

                return null;
            }

            private bool TouchesRegion(int cell, int targetRegion)
            {
                int x = X(cell, _width);
                int y = Y(cell, _width);
                for (int d = 0; d < 4; d++)
                {
                    int nx = x + Dx[d];
                    int ny = y + Dy[d];
                    if (nx < 0 || ny < 0 || nx >= _width || ny >= _height)
                        continue;
                    if (_wet[nx, ny] && _region[ny * _width + nx] == targetRegion)
                        return true;
                }

                return false;
            }
        }

        private static void Evaluate(
            CorridorSearch search,
            bool[,] wet,
            float[,] surface,
            int[] region,
            int[] landComp,
            List<List<int>> landMembers,
            List<WaterChannelRegionInfo> regions,
            List<int>[] frontier,
            int a,
            int b,
            RecipeChannelDetectionConfig config,
            WaterChannelCandidatePlan plan)
        {
            int width = wet.GetLength(0);
            int height = wet.GetLength(1);
            int maxLength = Mathf.Max(2, config.MaxCorridorLengthCells);

            RectInt bounds = PairBounds(
                region, a, b, Mathf.Max(0, config.FrontierMarginCells), width, height);
            List<int> path = search.Find(frontier[a], b, bounds);
            if (path == null)
            {
                plan.Rejections.Add(new WaterChannelRejection(a, b,
                    search.OutOfBudget
                        ? WaterChannelRejectReason.SearchBudgetExceeded
                        : WaterChannelRejectReason.NoCorridor));
                return;
            }

            if (path.Count > maxLength)
            {
                plan.Rejections.Add(new WaterChannelRejection(
                    a, b, WaterChannelRejectReason.CorridorTooLong));
                return;
            }

            // Endpoint water levels: the corridor is fed by the highest
            // adjacent water surface at each end (rivers slope, lakes don't).
            float levelA = AdjacentWaterSurface(
                wet, surface, region, path[0], a, width, height);
            float levelB = AdjacentWaterSurface(
                wet, surface, region, path[path.Count - 1], b, width, height);
            if (!IsFinite(levelA))
                levelA = regions[a].MaxSurfaceMeters;
            if (!IsFinite(levelB))
                levelB = regions[b].MaxSurfaceMeters;
            if (!IsFinite(levelA) || !IsFinite(levelB))
            {
                plan.Rejections.Add(new WaterChannelRejection(
                    a, b, WaterChannelRejectReason.MissingWaterSurface));
                return;
            }

            // Water flows from the higher end; equal levels keep the lower
            // region id as source so results are orientation-stable.
            bool forward = levelA >= levelB - Epsilon;
            int sourceRegion = forward ? a : b;
            int targetRegion = forward ? b : a;
            float sourceLevel = Mathf.Max(levelA, levelB);
            float targetLevel = Mathf.Min(levelA, levelB);
            if (!forward)
                path.Reverse();

            float saddle = float.NegativeInfinity;
            float volume = 0f;
            int aboveSource = 0;
            for (int i = 0; i < path.Count; i++)
            {
                float s = surface[X(path[i], width), Y(path[i], width)];
                if (s > saddle)
                    saddle = s;
                float cut = s - sourceLevel;
                if (cut > 0f)
                {
                    volume += cut;
                    aboveSource++;
                }
            }

            float maxCut = saddle - sourceLevel;
            var candidate = new WaterChannelCandidate
            {
                SourceRegionId = sourceRegion,
                TargetRegionId = targetRegion,
                SourceWaterCell = AdjacentWaterCell(
                    wet, surface, region, path[0], sourceRegion, width, height),
                TargetWaterCell = AdjacentWaterCell(
                    wet, surface, region, path[path.Count - 1], targetRegion, width, height),
                LandCells = ToCells(path, width),
                SourceSurfaceMeters = sourceLevel,
                TargetSurfaceMeters = targetLevel,
                SaddleMeters = saddle,
                LengthCells = path.Count,
                CellsAboveSourceLevel = aboveSource,
                ExcavationVolumeMeters = volume,
                MaxCellCutMeters = Mathf.Max(0f, maxCut),
            };

            if (maxCut > config.MajorCorrectionMaxCutMeters + Epsilon)
            {
                candidate.Status = WaterChannelCandidateStatus.Rejected;
                candidate.RejectReason = WaterChannelRejectReason.RidgeTooHigh;
            }
            else if (volume > config.MaxExcavationVolumeMeters + Epsilon)
            {
                candidate.Status = WaterChannelCandidateStatus.Rejected;
                candidate.RejectReason = WaterChannelRejectReason.ExcavationTooLarge;
            }
            else
            {
                candidate.RejectReason = WaterChannelRejectReason.None;
                candidate.Status = maxCut <= Epsilon
                    ? WaterChannelCandidateStatus.Natural
                    : maxCut <= config.MinorCorrectionMaxCutMeters + Epsilon
                        ? WaterChannelCandidateStatus.MinorCorrection
                        : WaterChannelCandidateStatus.MajorCorrection;
                MeasureLandSplit(path, landComp, landMembers, width, height, candidate);
            }

            plan.Candidates.Add(candidate);
        }

        /// <summary>
        /// Counts the land components left if the corridor's cells were
        /// flooded: severing an isthmus raises the fragment count above one.
        /// Diagnostics only — a natural strait through a neck is exactly the
        /// kind of geography this detector exists to find, so severance is a
        /// measured property, not a rejection.
        /// </summary>
        private static void MeasureLandSplit(
            List<int> path,
            int[] landComp,
            List<List<int>> landMembers,
            int width,
            int height,
            WaterChannelCandidate candidate)
        {
            int comp = landComp[path[0]];
            if (comp < 0)
                return;

            var blocked = new HashSet<int>(path);
            var mark = new HashSet<int>();
            var queue = new Queue<int>();
            int fragments = 0;
            int smallest = int.MaxValue;
            foreach (int cell in landMembers[comp])
            {
                if (blocked.Contains(cell) || mark.Contains(cell))
                    continue;

                int size = 0;
                fragments++;
                mark.Add(cell);
                queue.Enqueue(cell);
                while (queue.Count > 0)
                {
                    int c = queue.Dequeue();
                    size++;
                    for (int d = 0; d < 4; d++)
                    {
                        int nx = X(c, width) + Dx[d];
                        int ny = Y(c, width) + Dy[d];
                        if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                            continue;
                        int n = ny * width + nx;
                        if (blocked.Contains(n) || landComp[n] != comp || !mark.Add(n))
                            continue;
                        queue.Enqueue(n);
                    }
                }

                if (size < smallest)
                    smallest = size;
            }

            candidate.LandFragmentCount = fragments;
            candidate.SmallestLandFragmentCells = fragments > 1 ? smallest : 0;
        }

        private static Vector2Int[] ToCells(List<int> path, int width)
        {
            var cells = new Vector2Int[path.Count];
            for (int i = 0; i < path.Count; i++)
                cells[i] = new Vector2Int(X(path[i], width), Y(path[i], width));
            return cells;
        }

        private static float AdjacentWaterSurface(
            bool[,] wet, float[,] surface, int[] region,
            int landCell, int targetRegion, int width, int height)
        {
            int x = X(landCell, width);
            int y = Y(landCell, width);
            float best = float.NaN;
            for (int d = 0; d < 4; d++)
            {
                int nx = x + Dx[d];
                int ny = y + Dy[d];
                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    continue;
                if (!wet[nx, ny] || region[ny * width + nx] != targetRegion)
                    continue;
                float s = surface[nx, ny];
                if (IsFinite(s) && (!IsFinite(best) || s > best))
                    best = s;
            }

            return best;
        }

        private static Vector2Int AdjacentWaterCell(
            bool[,] wet, float[,] surface, int[] region,
            int landCell, int targetRegion, int width, int height)
        {
            int x = X(landCell, width);
            int y = Y(landCell, width);
            float best = float.NegativeInfinity;
            var cell = new Vector2Int(-1, -1);
            for (int d = 0; d < 4; d++)
            {
                int nx = x + Dx[d];
                int ny = y + Dy[d];
                if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                    continue;
                if (!wet[nx, ny] || region[ny * width + nx] != targetRegion)
                    continue;
                float s = surface[nx, ny];
                if (cell.x < 0 || (IsFinite(s) && s > best))
                {
                    cell = new Vector2Int(nx, ny);
                    if (IsFinite(s))
                        best = s;
                }
            }

            return cell;
        }

        private static RectInt PairBounds(
            int[] region, int a, int b, int margin, int width, int height)
        {
            int x0 = width, y0 = height, x1 = 0, y1 = 0;
            for (int i = 0; i < region.Length; i++)
            {
                int r = region[i];
                if (r != a && r != b)
                    continue;
                int x = X(i, width);
                int y = Y(i, width);
                if (x < x0) x0 = x;
                if (x > x1) x1 = x;
                if (y < y0) y0 = y;
                if (y > y1) y1 = y;
            }

            x0 = Mathf.Max(0, x0 - margin);
            y0 = Mathf.Max(0, y0 - margin);
            x1 = Mathf.Min(width - 1, x1 + margin);
            y1 = Mathf.Min(height - 1, y1 + margin);
            return new RectInt(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
        }

        private static int CompareCandidates(WaterChannelCandidate x, WaterChannelCandidate y)
        {
            int byStatus = x.Status.CompareTo(y.Status);
            if (byStatus != 0) return byStatus;
            int byVolume = x.ExcavationVolumeMeters.CompareTo(y.ExcavationVolumeMeters);
            if (byVolume != 0) return byVolume;
            int byLength = x.LengthCells.CompareTo(y.LengthCells);
            if (byLength != 0) return byLength;
            int bySource = x.SourceRegionId.CompareTo(y.SourceRegionId);
            return bySource != 0 ? bySource : x.TargetRegionId.CompareTo(y.TargetRegionId);
        }

        private static bool IsWaterId(string tileId, string[] waterLikeTileIds)
        {
            if (waterLikeTileIds == null || string.IsNullOrWhiteSpace(tileId))
                return false;
            for (int i = 0; i < waterLikeTileIds.Length; i++)
            {
                if (string.Equals(
                    waterLikeTileIds[i], tileId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
