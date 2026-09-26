using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime.ChunkFirst
{
    /// <summary>
    /// Computes the shared seabed height field: per-cell bed heights that drop
    /// with world-space distance to land, plus the (w+1)x(h+1) corner lattice
    /// chunks share so adjacent patches always agree on boundary heights.
    /// Pure data — no Unity objects — so it can be verified in EditMode tests.
    /// </summary>
    internal static class SeabedFieldPlanner
    {
        private static readonly int[] Dx8 = { -1, 0, 1, -1, 1, -1, 0, 1 };
        private static readonly int[] Dy8 = { -1, -1, -1, 0, 0, 1, 1, 1 };

        /// <summary>
        /// <paramref name="isWater"/>: cells whose water sheet actually renders.
        /// <paramref name="waterY"/>: rendered water surface per water cell.
        /// <paramref name="landY"/>: published surface height per land cell
        /// (only sampled on non-water cells).
        /// <paramref name="kind"/>: hydrology water kind per cell.
        /// </summary>
        public static Field Build(
            int w,
            int h,
            float cellSize,
            bool[,] isWater,
            float[,] waterY,
            float[,] landY,
            RecipeWaterKind[,] kind,
            RecipeSeabedConfig config)
        {
            var field = new Field(w, h, cellSize);
            if (isWater == null || waterY == null || config == null)
                return field;

            var dist = ComputeShoreDistance(w, h, cellSize, isWater);
            field.ShoreDistance = dist;

            var bedY = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                bedY[x, y] = float.NaN;
                if (!isWater[x, y])
                    continue;
                float surface = waterY[x, y];
                bedY[x, y] = IsFinite(surface)
                    ? surface - DepthAt(kind[x, y], dist[x, y], config)
                    : surface;
            }
            field.BedY = bedY;

            field.CornerY = BuildCornerLattice(w, h, cellSize, isWater, waterY,
                landY, kind, dist, config);
            return field;
        }

        /// <summary>
        /// One shared value per lattice vertex: the lowest of every touching
        /// water cell's target and every touching land surface, so shore
        /// vertices stitch to the real shoreline and chunk borders agree.
        /// </summary>
        private static float[,] BuildCornerLattice(
            int w, int h, float cellSize,
            bool[,] isWater, float[,] waterY, float[,] landY,
            RecipeWaterKind[,] kind, float[,] dist, RecipeSeabedConfig config)
        {
            var corner = new float[w + 1, h + 1];
            for (int vx = 0; vx <= w; vx++)
            for (int vy = 0; vy <= h; vy++)
            {
                bool touchesLand = false;
                float vertexDist = 0f;
                int waterCount = 0;
                float minY = float.MaxValue;
                for (int i = 0; i < 4; i++)
                {
                    int cx = vx - (i & 1);
                    int cy = vy - (i >> 1);
                    if (cx < 0 || cy < 0 || cx >= w || cy >= h)
                        continue;
                    if (isWater[cx, cy])
                    {
                        waterCount++;
                        vertexDist += dist[cx, cy];
                    }
                    else
                    {
                        touchesLand = true;
                        if (landY != null && IsFinite(landY[cx, cy]))
                            minY = Mathf.Min(minY, landY[cx, cy]);
                    }
                }

                if (waterCount == 0)
                {
                    corner[vx, vy] = float.NaN;
                    continue;
                }

                vertexDist = touchesLand ? 0f : vertexDist / waterCount;
                for (int i = 0; i < 4; i++)
                {
                    int cx = vx - (i & 1);
                    int cy = vy - (i >> 1);
                    if (cx < 0 || cy < 0 || cx >= w || cy >= h || !isWater[cx, cy])
                        continue;
                    float surface = waterY[cx, cy];
                    if (!IsFinite(surface))
                        continue;
                    float target = touchesLand
                        ? surface - config.ShoreRecessMeters
                        : surface - DepthAt(kind[cx, cy], vertexDist, config);
                    minY = Mathf.Min(minY, target);
                }
                corner[vx, vy] = minY == float.MaxValue ? float.NaN : minY;
            }
            return corner;
        }

        /// <summary>
        /// World-space distance to the closest water cell that touches land.
        /// Multi-source Dijkstra over water cells only: distance can never
        /// cross land into another water body.
        /// </summary>
        private static float[,] ComputeShoreDistance(
            int w, int h, float cellSize, bool[,] isWater)
        {
            var dist = new float[w, h];
            var heap = new SimpleMinHeap(w * h);
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                dist[x, y] = float.PositiveInfinity;
                if (isWater[x, y] && TouchesLand(x, y, w, h, isWater))
                {
                    dist[x, y] = 0f;
                    heap.Push(x, y, 0f);
                }
            }

            float ortho = Mathf.Max(0.0001f, cellSize);
            float diag = ortho * Mathf.Sqrt(2f);
            while (heap.Count > 0)
            {
                heap.Pop(out int cx, out int cy, out float d);
                if (d > dist[cx, cy] + 0.0001f)
                    continue;
                for (int i = 0; i < 8; i++)
                {
                    int nx = cx + Dx8[i];
                    int ny = cy + Dy8[i];
                    if (nx < 0 || ny < 0 || nx >= w || ny >= h || !isWater[nx, ny])
                        continue;
                    bool diagonal = Dx8[i] != 0 && Dy8[i] != 0;
                    float nd = d + (diagonal ? diag : ortho);
                    if (nd + 0.0001f < dist[nx, ny])
                    {
                        dist[nx, ny] = nd;
                        heap.Push(nx, ny, nd);
                    }
                }
            }

            // A body with no land boundary (e.g. an all-water map) gets the
            // maximum distance so the profile still reaches full depth.
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                if (isWater[x, y] && float.IsPositiveInfinity(dist[x, y]))
                    dist[x, y] = float.MaxValue;
            return dist;
        }

        private static bool TouchesLand(int x, int y, int w, int h, bool[,] isWater)
        {
            for (int i = 0; i < 8; i++)
            {
                int nx = x + Dx8[i];
                int ny = y + Dy8[i];
                if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                    continue;
                if (!isWater[nx, ny])
                    return true;
            }
            return false;
        }

        /// <summary>Seabed depth below the water surface for one water cell.</summary>
        public static float DepthAt(RecipeWaterKind kind, float distanceMeters, RecipeSeabedConfig config)
        {
            float maxDepth = kind switch
            {
                RecipeWaterKind.River => config.MaxDepthRiverMeters,
                RecipeWaterKind.Lake => config.MaxDepthLakeMeters,
                _ => config.MaxDepthSeaMeters,
            };
            float falloff = config.FalloffMeters *
                            (kind == RecipeWaterKind.River ? config.RiverFalloffScale : 1f);
            float t = Mathf.Clamp01(
                Mathf.Max(0f, distanceMeters - config.ShallowShelfMeters)
                / Mathf.Max(0.0001f, falloff));
            float exponent = Mathf.Max(0.01f, config.DepthCurveExponent);
            // Never fully coplanar with the water sheet: even inside the
            // shallow shelf the bed recesses a few centimeters.
            return Mathf.Max(0.05f,
                Mathf.Max(0.05f, maxDepth) * Mathf.Pow(t, exponent));
        }

        private static bool IsFinite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);

        internal sealed class Field
        {
            public Field(int width, int height, float cellSize)
            {
                Width = width;
                Height = height;
                CellSize = cellSize;
            }

            public int Width { get; }
            public int Height { get; }
            public float CellSize { get; }
            /// <summary>Seabed top Y per water cell; NaN on land.</summary>
            public float[,] BedY;
            /// <summary>Shared vertex lattice (w+1)x(h+1); NaN without adjacent water.</summary>
            public float[,] CornerY;
            /// <summary>Meters to the nearest land-touching water cell.</summary>
            public float[,] ShoreDistance;

            public bool TryGetBedY(Vector2Int cell, out float y)
            {
                y = float.NaN;
                if (BedY == null || cell.x < 0 || cell.y < 0 || cell.x >= Width || cell.y >= Height)
                    return false;
                y = BedY[cell.x, cell.y];
                return IsFinite(y);
            }

            public bool TryGetCornerY(int vx, int vy, out float y)
            {
                y = float.NaN;
                if (CornerY == null || vx < 0 || vy < 0 || vx > Width || vy > Height)
                    return false;
                y = CornerY[vx, vy];
                return IsFinite(y);
            }

            /// <summary>
            /// Smooth field normal at a lattice vertex from corner-lattice
            /// central differences; identical for every chunk that reads it.
            /// </summary>
            public Vector3 GetCornerNormal(int vx, int vy)
            {
                if (CornerY == null
                    || vx < 0 || vy < 0 || vx > Width || vy > Height
                    || !IsFinite(CornerY[vx, vy]))
                {
                    return Vector3.up;
                }
                float cs = Mathf.Max(0.0001f, CellSize);
                float left = SampleCorner(vx - 1, vy, vx, vy);
                float right = SampleCorner(vx + 1, vy, vx, vy);
                float down = SampleCorner(vx, vy - 1, vx, vy);
                float up = SampleCorner(vx, vy + 1, vx, vy);
                return new Vector3(-(right - left) / (2f * cs), 1f,
                    -(up - down) / (2f * cs)).normalized;
            }

            /// <summary>
            /// Smooth normal for a water cell's fan center: central differences
            /// over bed heights, land cells contributing their real surface so
            /// the shoreward tilt reads correctly.
            /// </summary>
            public Vector3 GetCenterNormal(Vector2Int cell, bool[,] isWater, float[,] landY)
            {
                if (BedY == null
                    || cell.x < 0 || cell.y < 0 || cell.x >= Width || cell.y >= Height
                    || !IsFinite(BedY[cell.x, cell.y]))
                {
                    return Vector3.up;
                }
                float cs = Mathf.Max(0.0001f, CellSize);
                int cx = cell.x, cy = cell.y;
                float left = SampleBed(cx - 1, cy);
                float right = SampleBed(cx + 1, cy);
                float down = SampleBed(cx, cy - 1);
                float up = SampleBed(cx, cy + 1);
                return new Vector3(-(right - left) / (2f * cs), 1f,
                    -(up - down) / (2f * cs)).normalized;

                float SampleBed(int x, int y)
                {
                    if (x < 0 || y < 0 || x >= Width || y >= Height)
                        return BedY[cx, cy];
                    if (isWater == null || isWater[x, y])
                        return IsFinite(BedY[x, y]) ? BedY[x, y] : BedY[cx, cy];
                    return landY != null && IsFinite(landY[x, y])
                        ? landY[x, y]
                        : BedY[cx, cy];
                }
            }

            private float SampleCorner(int vx, int vy, int fx, int fy)
            {
                if (vx < 0 || vy < 0 || vx > Width || vy > Height || !IsFinite(CornerY[vx, vy]))
                    return CornerY[fx, fy];
                return CornerY[vx, vy];
            }
        }

        /// <summary>Minimal binary min-heap over flattened cell indices.</summary>
        private sealed class SimpleMinHeap
        {
            private int[] _cells;
            private float[] _keys;
            public int Count { get; private set; }

            public SimpleMinHeap(int capacity)
            {
                _cells = new int[Mathf.Max(16, capacity)];
                _keys = new float[_cells.Length];
            }

            public void Push(int x, int y, float key)
            {
                if (Count == _cells.Length)
                {
                    System.Array.Resize(ref _cells, _cells.Length * 2);
                    System.Array.Resize(ref _keys, _cells.Length);
                }
                int i = Count++;
                _cells[i] = (y << 16) | (x & 0xFFFF);
                _keys[i] = key;
                while (i > 0)
                {
                    int p = (i - 1) >> 1;
                    if (_keys[p] <= _keys[i]) break;
                    (_cells[p], _cells[i]) = (_cells[i], _cells[p]);
                    (_keys[p], _keys[i]) = (_keys[i], _keys[p]);
                    i = p;
                }
            }

            public void Pop(out int x, out int y, out float key)
            {
                int cell = _cells[0];
                key = _keys[0];
                x = cell & 0xFFFF;
                y = cell >> 16;
                Count--;
                if (Count > 0)
                {
                    _cells[0] = _cells[Count];
                    _keys[0] = _keys[Count];
                    int i = 0;
                    while (true)
                    {
                        int l = i * 2 + 1, r = l + 1, m = i;
                        if (l < Count && _keys[l] < _keys[m]) m = l;
                        if (r < Count && _keys[r] < _keys[m]) m = r;
                        if (m == i) break;
                        (_cells[m], _cells[i]) = (_cells[i], _cells[m]);
                        (_keys[m], _keys[i]) = (_keys[i], _keys[m]);
                        i = m;
                    }
                }
            }
        }
    }
}
