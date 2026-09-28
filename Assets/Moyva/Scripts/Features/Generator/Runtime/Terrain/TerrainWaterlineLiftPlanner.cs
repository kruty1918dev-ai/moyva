using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITerrainWaterlineLiftPlanner
    {
        /// <summary>
        /// Scans the logical map for land cells adjacent to water whose
        /// winning surface still sits below the actual neighbouring water
        /// surface, then raises them just above the waterline. Runs after
        /// the shore pass and before passages/routes, inside the same
        /// logical-map mutation pass, so mesh, colliders and terrain
        /// queries all see the corrected heights. Returns the correction
        /// matrix; null when disabled.
        /// </summary>
        WaterlineCorrectionPlan Apply(
            LogicalTileMap map,
            TerrainWaterlineLiftConfig config,
            string[] waterLikeTileIds);
    }

    /// <summary>
    /// Correction matrix produced by the waterline guard. All maps use the
    /// project's [x, y] cell coordinates; NaN marks cells a channel does
    /// not apply to.
    /// </summary>
    internal sealed class WaterlineCorrectionPlan
    {
        public WaterlineCorrectionPlan(int width, int height)
        {
            RequiredSurface = CreateNaNMap(width, height);
            OriginalSurface = CreateNaNMap(width, height);
            TargetSurface = CreateNaNMap(width, height);
            Corrected = new bool[width, height];
            Smoothed = new bool[width, height];
        }

        /// <summary>
        /// Per-cell minimum surface that clears every adjacent water surface
        /// by the configured lift. NaN where no water neighbour exists.
        /// </summary>
        public float[,] RequiredSurface { get; }

        /// <summary>Winner surface captured before the pass ran.</summary>
        public float[,] OriginalSurface { get; }

        /// <summary>Surface the pass wrote back (corrected and smoothed cells).</summary>
        public float[,] TargetSurface { get; }

        /// <summary>Land cells that sat below the waterline and were lifted.</summary>
        public bool[,] Corrected { get; }

        /// <summary>Unaffected land cells eased upward for a smooth transition.</summary>
        public bool[,] Smoothed { get; }

        /// <summary>8-connected clusters of corrected cells, in scan order.</summary>
        public readonly List<List<Vector2Int>> Groups = new List<List<Vector2Int>>();

        private static float[,] CreateNaNMap(int width, int height)
        {
            var map = new float[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = float.NaN;
            return map;
        }
    }

    /// <summary>
    /// Waterline guard pass. The shore pass only re-grades the configured
    /// band and only BaseTerrain winners, so cells can stay below the
    /// actual water sheet — shallow-flooded shelf cells under
    /// LakeMinDepthMeters, band gaps from partial coverage, non-terrain
    /// winners, or a disabled shore pass entirely. Those cells make the
    /// mesh render a water quad plus a vertical water wall over dry land.
    ///
    /// The pass computes every target from one immutable snapshot, so the
    /// result never depends on scan order: the matrix holds the required
    /// surface per cell, then corrections apply in a second sweep, then a
    /// narrow BFS ring eases the step outward. No RNG — identical input
    /// produces identical output.
    /// </summary>
    internal sealed class TerrainWaterlineLiftPlanner : ITerrainWaterlineLiftPlanner
    {
        private const float Epsilon = 0.0001f;

        public WaterlineCorrectionPlan Apply(
            LogicalTileMap map,
            TerrainWaterlineLiftConfig config,
            string[] waterLikeTileIds)
        {
            if (map == null || config == null || !config.Enabled)
                return null;

            int width = map.Width;
            int height = map.Height;
            float lift = Mathf.Max(0.005f, config.LiftMeters);
            float join = Mathf.Max(0f, config.TerraceJoinMeters);
            int rings = Mathf.Max(0, config.TransitionCells);
            float rise = Mathf.Max(0.01f, config.TransitionRiseMeters);

            var plan = new WaterlineCorrectionPlan(width, height);

            // Snapshot: winner index, water flag and winner surface per cell.
            // Every later decision reads only this pass, never live stacks.
            var winnerIndex = new int[width, height];
            var isWater = new bool[width, height];
            float[,] surface = plan.OriginalSurface;
            float[,] projected = map.SurfaceHeights;
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                TileStackCell stack = map.GetCellStack(x, y);
                int winner = FindMainTerrainIndex(stack);
                winnerIndex[x, y] = winner;
                if (winner < 0)
                    continue;

                TileLayerSample main = stack.Samples[winner];
                surface[x, y] = IsFinite(main.SurfaceHeight)
                    ? main.SurfaceHeight
                    : IsFinite(main.Height)
                        ? main.Height
                        : projected[x, y];
                isWater[x, y] = IsWater(main, waterLikeTileIds);
            }

            // Matrix: the minimum surface clearing every adjacent water cell.
            // The highest neighbouring sheet dominates, matching the mesh
            // resolver which picks the max SurfaceOnly surface around a cell.
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (isWater[x, y] || winnerIndex[x, y] < 0 || !IsFinite(surface[x, y]))
                    continue;

                float maxWater = float.MinValue;
                bool found = false;
                for (int nx = Mathf.Max(0, x - 1); nx <= Mathf.Min(width - 1, x + 1); nx++)
                for (int ny = Mathf.Max(0, y - 1); ny <= Mathf.Min(height - 1, y + 1); ny++)
                {
                    if (nx == x && ny == y)
                        continue;
                    if (!isWater[nx, ny] || !IsFinite(surface[nx, ny]))
                        continue;
                    found = true;
                    if (surface[nx, ny] > maxWater)
                        maxWater = surface[nx, ny];
                }
                if (!found)
                    continue;

                plan.RequiredSurface[x, y] = maxWater + lift;
                if (surface[x, y] < plan.RequiredSurface[x, y] - Epsilon)
                    plan.Corrected[x, y] = true;
            }

            CollectGroups(plan, width, height);

            // Targets: the minimal lift, or a join to an existing stable
            // terrace when one sits within TerraceJoinMeters — both read from
            // the snapshot so cells never race each other.
            float[,] final = (float[,])surface.Clone();
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (!plan.Corrected[x, y])
                    continue;

                float floor = plan.RequiredSurface[x, y];
                float anchor = float.MaxValue;
                for (int nx = Mathf.Max(0, x - 1); nx <= Mathf.Min(width - 1, x + 1); nx++)
                for (int ny = Mathf.Max(0, y - 1); ny <= Mathf.Min(height - 1, y + 1); ny++)
                {
                    if (nx == x && ny == y)
                        continue;
                    if (isWater[nx, ny] || winnerIndex[nx, ny] < 0
                        || plan.Corrected[nx, ny] || !IsFinite(surface[nx, ny])
                        || surface[nx, ny] < floor - Epsilon)
                    {
                        continue;
                    }
                    if (surface[nx, ny] < anchor)
                        anchor = surface[nx, ny];
                }

                final[x, y] = anchor != float.MaxValue && anchor - floor <= join
                    ? anchor
                    : floor;
                plan.TargetSurface[x, y] = final[x, y];
            }

            // Transition rings: BFS from corrected cells across land only.
            // Ring r eases toward its inner neighbours so a deep lift becomes
            // a ramp instead of a wall; cells already high enough stay put.
            if (rings > 0)
            {
                var ring = new int[width, height];
                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    ring[x, y] = -1;

                var frontier = new List<Vector2Int>();
                for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    if (!plan.Corrected[x, y])
                        continue;
                    ring[x, y] = 0;
                    frontier.Add(new Vector2Int(x, y));
                }

                for (int r = 1; r <= rings && frontier.Count > 0; r++)
                {
                    var next = new List<Vector2Int>();
                    foreach (Vector2Int cell in frontier)
                    {
                        for (int nx = Mathf.Max(0, cell.x - 1); nx <= Mathf.Min(width - 1, cell.x + 1); nx++)
                        for (int ny = Mathf.Max(0, cell.y - 1); ny <= Mathf.Min(height - 1, cell.y + 1); ny++)
                        {
                            if (nx == cell.x && ny == cell.y)
                                continue;
                            if (ring[nx, ny] >= 0 || isWater[nx, ny]
                                || winnerIndex[nx, ny] < 0 || !IsFinite(surface[nx, ny]))
                            {
                                continue;
                            }
                            ring[nx, ny] = r;
                            next.Add(new Vector2Int(nx, ny));
                        }
                    }

                    // Same-ring cells read pre-ring finals, keeping the
                    // outcome independent of the order they are visited in.
                    foreach (Vector2Int cell in next)
                    {
                        float innerMin = float.MaxValue;
                        for (int nx = Mathf.Max(0, cell.x - 1); nx <= Mathf.Min(width - 1, cell.x + 1); nx++)
                        for (int ny = Mathf.Max(0, cell.y - 1); ny <= Mathf.Min(height - 1, cell.y + 1); ny++)
                        {
                            if (nx == cell.x && ny == cell.y)
                                continue;
                            if (ring[nx, ny] >= 0 && ring[nx, ny] < r
                                && IsFinite(final[nx, ny]) && final[nx, ny] < innerMin)
                            {
                                innerMin = final[nx, ny];
                            }
                        }
                        if (innerMin == float.MaxValue)
                            continue;

                        float eased = innerMin - rise;
                        if (eased > final[cell.x, cell.y] + Epsilon)
                        {
                            final[cell.x, cell.y] = eased;
                            plan.Smoothed[cell.x, cell.y] = true;
                            plan.TargetSurface[cell.x, cell.y] = eased;
                        }
                    }

                    frontier = next;
                }
            }

            // Apply: shift the whole non-water column by the winner's delta so
            // overlays/decorations ride the lifted surface and buried water
            // sheets stay under it.
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (isWater[x, y] || winnerIndex[x, y] < 0
                    || !IsFinite(plan.TargetSurface[x, y]))
                {
                    continue;
                }

                float delta = plan.TargetSurface[x, y] - surface[x, y];
                if (delta <= Epsilon)
                    continue;

                TileStackCell stack = map.GetCellStack(x, y);
                for (int i = 0; i < stack.Samples.Count; i++)
                {
                    TileLayerSample sample = stack.Samples[i];
                    if (IsWater(sample, waterLikeTileIds))
                        continue;

                    float h = IsFinite(sample.Height) ? sample.Height + delta : sample.Height;
                    float s = IsFinite(sample.SurfaceHeight) ? sample.SurfaceHeight + delta : h;
                    stack.SetAt(i, sample.WithHeights(h, s));
                }
            }

            return plan;
        }

        private static void CollectGroups(
            WaterlineCorrectionPlan plan,
            int width,
            int height)
        {
            var visited = new bool[width, height];
            var queue = new Queue<Vector2Int>();
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (!plan.Corrected[x, y] || visited[x, y])
                    continue;

                var group = new List<Vector2Int>();
                visited[x, y] = true;
                queue.Enqueue(new Vector2Int(x, y));
                while (queue.Count > 0)
                {
                    Vector2Int cell = queue.Dequeue();
                    group.Add(cell);
                    for (int nx = Mathf.Max(0, cell.x - 1); nx <= Mathf.Min(width - 1, cell.x + 1); nx++)
                    for (int ny = Mathf.Max(0, cell.y - 1); ny <= Mathf.Min(height - 1, cell.y + 1); ny++)
                    {
                        if (nx == cell.x && ny == cell.y)
                            continue;
                        if (visited[nx, ny] || !plan.Corrected[nx, ny])
                            continue;
                        visited[nx, ny] = true;
                        queue.Enqueue(new Vector2Int(nx, ny));
                    }
                }
                plan.Groups.Add(group);
            }
        }

        private static int FindMainTerrainIndex(TileStackCell stack)
        {
            if (stack == null || stack.IsEmpty)
                return -1;

            int bestIndex = -1;
            for (int i = 0; i < stack.Samples.Count; i++)
            {
                TileLayerSample candidate = stack.Samples[i];
                if (!candidate.IsTerrainLike
                    || candidate.LayerKind == LayerKind.OverlayTerrain)
                {
                    continue;
                }

                if (bestIndex < 0
                    || stack.Samples[bestIndex].CompareTo(candidate) <= 0)
                {
                    bestIndex = i;
                }
            }

            return bestIndex;
        }

        private static bool IsWater(TileLayerSample sample, string[] waterLikeTileIds)
        {
            if (sample.TileGeometryMode == TileGeometryMode.SurfaceOnly)
                return true;
            if (waterLikeTileIds == null || string.IsNullOrWhiteSpace(sample.TileId))
                return false;

            for (int i = 0; i < waterLikeTileIds.Length; i++)
            {
                if (string.Equals(
                    waterLikeTileIds[i], sample.TileId,
                    System.StringComparison.OrdinalIgnoreCase))
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
