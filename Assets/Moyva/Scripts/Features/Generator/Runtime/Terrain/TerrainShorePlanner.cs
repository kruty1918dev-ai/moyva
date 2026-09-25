using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal interface ITerrainShorePlanner
    {
        /// <summary>
        /// Re-types water-adjacent land cells to the configured shore tile and
        /// grades their surfaces down toward the neighbouring water surface.
        /// Runs after relief, inside the same logical-map mutation pass as
        /// passages/routes, so every downstream consumer (compatibility maps,
        /// mesh building, gameplay tile ids) sees the final shoreline.
        /// </summary>
        void Apply(
            LogicalTileMap map,
            TerrainShoreConfig config,
            string[] waterLikeTileIds);
    }

    /// <summary>
    /// Distance-field shoreline pass. Water cells are the ones whose winning
    /// terrain is a surface-only sheet or a water-like tile id, so the band
    /// follows the actual water contour — sea, rivers and lakes alike — rather
    /// than a fixed height threshold. Land within <see cref="TerrainShoreConfig.BandCells"/>
    /// becomes the shore tile and is pulled down to just above the waterline;
    /// a wider blend ring only caps heights so the slope eases into inland
    /// terrain. Cells towering over the water stay untouched (cliff shores).
    /// </summary>
    internal sealed class TerrainShorePlanner : ITerrainShorePlanner
    {
        public void Apply(
            LogicalTileMap map,
            TerrainShoreConfig config,
            string[] waterLikeTileIds)
        {
            if (map == null || config == null || !config.Enabled)
                return;

            int width = map.Width;
            int height = map.Height;
            int band = Mathf.Max(1, config.BandCells);
            int radius = band + Mathf.Max(0, config.BlendCells);
            if (radius <= 0)
                return;

            string shoreTileId = string.IsNullOrWhiteSpace(config.ShoreTileId)
                ? "sand"
                : config.ShoreTileId.Trim();
            string shorePresetId = string.IsNullOrWhiteSpace(config.ShorePresetId)
                ? shoreTileId
                : config.ShorePresetId.Trim();

            var water = new bool[width, height];
            var waterSurface = new float[width, height];
            var winnerIndex = new int[width, height];
            var isLand = new bool[width, height];
            float[,] originalSurface = map.SurfaceHeights;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                TileStackCell stack = map.GetCellStack(x, y);
                int winner = FindMainTerrainIndex(stack);
                winnerIndex[x, y] = winner;
                if (winner < 0)
                    continue;

                TileLayerSample main = stack.Samples[winner];
                if (IsWater(main, waterLikeTileIds))
                {
                    water[x, y] = true;
                    waterSurface[x, y] = IsFinite(main.SurfaceHeight)
                        ? main.SurfaceHeight
                        : main.Height;
                }
                else
                {
                    isLand[x, y] = true;
                }
            }

            // A native shore sample carries the canonical layer identity
            // (blueprint/build guids, preset) so converted cells merge with it
            // seamlessly instead of leaving dual-grid borders inside the band.
            bool donorFound = TryFindShoreDonor(map, shoreTileId, out TileLayerSample donor);

            float lift = Mathf.Max(0f, config.ShoreLiftMeters);
            float rise = Mathf.Max(0.01f, config.RisePerCellMeters);
            float maxDrop = Mathf.Max(0.01f, config.MaxDropToWaterMeters);

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (!isLand[x, y])
                    continue;

                if (!TryResolveWaterProximity(
                        water, waterSurface, width, height, x, y, radius,
                        out int distance, out float level))
                {
                    continue;
                }

                TileStackCell stack = map.GetCellStack(x, y);
                TileLayerSample main = stack.Samples[winnerIndex[x, y]];
                if (main.LayerKind != LayerKind.BaseTerrain)
                    continue;

                float original = IsFinite(main.SurfaceHeight)
                    ? main.SurfaceHeight
                    : originalSurface[x, y];
                if (!IsFinite(original))
                    continue;

                float floor = level + lift;
                // A ledge high above the waterline keeps its cliff look; the
                // beach band does not consume it.
                if (original - floor > maxDrop)
                    continue;

                float cap = floor + Mathf.Max(0, distance - 1) * rise;
                bool convert = distance <= band;
                float target = convert
                    ? Mathf.Clamp(original, floor, cap)
                    : Mathf.Min(original, cap);
                if (!convert && target >= original - 0.0001f)
                    continue;

                ApplyShoreToCell(stack, waterLikeTileIds, donorFound, donor,
                    shoreTileId, shorePresetId, convert, target);
            }
        }

        /// <summary>
        /// Chebyshev distance to the nearest water cell plus that cell's
        /// surface height. Ties prefer the highest adjacent water level so the
        /// shelf tucks under the water sheet rather than undercutting it.
        /// </summary>
        private static bool TryResolveWaterProximity(
            bool[,] water,
            float[,] waterSurface,
            int width,
            int height,
            int x,
            int y,
            int radius,
            out int distance,
            out float level)
        {
            distance = 0;
            level = float.NaN;
            int best = int.MaxValue;
            float bestLevel = float.MinValue;

            int x0 = Mathf.Max(0, x - radius);
            int x1 = Mathf.Min(width - 1, x + radius);
            int y0 = Mathf.Max(0, y - radius);
            int y1 = Mathf.Min(height - 1, y + radius);
            for (int nx = x0; nx <= x1; nx++)
            for (int ny = y0; ny <= y1; ny++)
            {
                if (!water[nx, ny])
                    continue;

                int d = Mathf.Max(Mathf.Abs(nx - x), Mathf.Abs(ny - y));
                float surface = waterSurface[nx, ny];
                if (d < best
                    || d == best && IsFinite(surface) && surface > bestLevel)
                {
                    best = d;
                    bestLevel = surface;
                }
            }

            if (best == int.MaxValue || !IsFinite(bestLevel))
                return false;

            distance = best;
            level = bestLevel;
            return true;
        }

        private static void ApplyShoreToCell(
            TileStackCell stack,
            string[] waterLikeTileIds,
            bool donorFound,
            TileLayerSample donor,
            string shoreTileId,
            string shorePresetId,
            bool convert,
            float target)
        {
            for (int i = 0; i < stack.Samples.Count; i++)
            {
                TileLayerSample sample = stack.Samples[i];
                if (!sample.IsTerrainLike
                    || sample.LayerKind != LayerKind.BaseTerrain
                    || IsWater(sample, waterLikeTileIds))
                {
                    continue;
                }

                TileLayerSample updated = convert
                    ? ToShoreSample(sample, donorFound, donor, shoreTileId, shorePresetId)
                    : sample;
                // Converted cells flatten every land layer to the beach level
                // so the shore tile stays the winner; blend cells only cap
                // heights, preserving each layer's relative order.
                float sampleTarget = convert
                    ? target
                    : Mathf.Min(
                        IsFinite(sample.SurfaceHeight) ? sample.SurfaceHeight : target,
                        target);
                stack.SetAt(i, updated.WithSurfaceHeight(sampleTarget));
            }
        }

        private static TileLayerSample ToShoreSample(
            TileLayerSample sample,
            bool donorFound,
            TileLayerSample donor,
            string shoreTileId,
            string shorePresetId)
        {
            if (donorFound)
            {
                return new TileLayerSample(
                    donor.LayerId,
                    donor.LayerName,
                    donor.BlueprintLayerGuid,
                    donor.BuildLayerGuid,
                    shoreTileId,
                    string.IsNullOrWhiteSpace(donor.PresetId) ? shorePresetId : donor.PresetId,
                    sample.LayerKind,
                    donor.SortingOrder,
                    donor.LayerOrder,
                    donor.TerrainPriority,
                    sample.Height,
                    sample.SurfaceHeight,
                    donor.SourceLayerId,
                    donor.TileGeometryMode,
                    donor.AuthoredClosurePolicy);
            }

            /*
             * Without a native shore donor the converted cell keeps no layer
             * identity of its own: cells converted from different source
             * layers must still merge into one seamless beach, and a foreign
             * build-layer guid would only misdirect preset lookup (a grass
             * layer has no "sand" preset). TileId-based matching resolves
             * both problems — SameTerrainIdentity falls through to TileId
             * when no layer ids/guids are set.
             */
            return new TileLayerSample(
                null,
                sample.LayerName,
                null,
                null,
                shoreTileId,
                shorePresetId,
                sample.LayerKind,
                sample.SortingOrder,
                sample.LayerOrder,
                sample.TerrainPriority,
                sample.Height,
                sample.SurfaceHeight,
                sample.SourceLayerId,
                sample.TileGeometryMode,
                sample.AuthoredClosurePolicy);
        }

        private static bool TryFindShoreDonor(
            LogicalTileMap map,
            string shoreTileId,
            out TileLayerSample donor)
        {
            for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
            {
                TileStackCell stack = map.GetCellStack(x, y);
                if (stack == null)
                    continue;

                for (int i = 0; i < stack.Samples.Count; i++)
                {
                    TileLayerSample sample = stack.Samples[i];
                    if (!sample.IsTerrainLike
                        || sample.TileGeometryMode == TileGeometryMode.SurfaceOnly
                        || !string.Equals(
                            sample.TileId, shoreTileId, System.StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    donor = sample;
                    return true;
                }
            }

            donor = default;
            return false;
        }

        /// <summary>
        /// The winning terrain sample index: highest terrain-like non-overlay
        /// sample, matching the visual composition's main-terrain choice.
        /// </summary>
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
