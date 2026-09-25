using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Generates procedural environment decorations (trees, rocks, bushes) using deterministic noise and clustering.
    /// Decorations are presentational only and do not affect gameplay state.
    /// </summary>
    internal sealed class EnvironmentDecorationGenerator
    {
        private readonly EnvironmentDecorationConfig _config;
        private readonly IMapObjectRegistryService _objectRegistry;
        private readonly ITerrainPlacementPolicy _placementPolicy;
        private readonly EnvironmentObjectPlacementResolver _placementResolver;
        private readonly DeterministicNoise _noise;

        public EnvironmentDecorationGenerator(
            EnvironmentDecorationConfig config,
            IMapObjectRegistryService objectRegistry,
            [Zenject.InjectOptional] ITerrainPlacementPolicy placementPolicy = null,
            [Zenject.InjectOptional] EnvironmentObjectPlacementResolver placementResolver = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _objectRegistry = objectRegistry ?? throw new ArgumentNullException(nameof(objectRegistry));
            _placementPolicy = placementPolicy;
            _placementResolver = placementResolver;
            _noise = new DeterministicNoise();
        }

        /// <summary>
        /// Generate decoration placement data for the given world.
        /// </summary>
        public DecorationPlacementResult Generate(GeneratedWorldData worldData)
        {
            if (!_config.Enabled || worldData == null)
                return DecorationPlacementResult.Empty;

            int width = worldData.Width;
            int height = worldData.Height;
            int seed = worldData.Seed + _config.SeedOffset;

            var placements = new List<DecorationPlacement>();
            var densityMap = GenerateDensityMap(worldData, seed);

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                string tileId = ResolveTileId(x, y, worldData);
                bool waterCell = IsWaterTile(tileId) || HasWaterSheet(x, y, worldData);

                // Water flora is the only decoration allowed on water cells;
                // land props never spawn on water regardless of flags.
                if (waterCell)
                {
                    TryGenerateWaterFlora(x, y, seed, tileId, worldData, placements);
                    continue;
                }

                // Tile-tagged spawn blocks (shore sand) reject every land
                // decoration regardless of configured densities.
                if (!AllowsDecorationTile(tileId))
                    continue;

                if (IsExcludedCell(x, y, worldData))
                    continue;

                float cellDensity = densityMap[x, y];
                if (cellDensity <= 0f)
                    continue;

                float biomeMultiplier = GetBiomeMultiplier(tileId);
                float adjustedDensity = cellDensity * _config.GlobalDensity * biomeMultiplier;

                int objectCount = CalculateObjectCount(adjustedDensity, seed, x, y);
                for (int i = 0; i < objectCount; i++)
                {
                    if (TryGenerateDecoration(x, y, seed, i, tileId, worldData, out var placement))
                        placements.Add(placement);
                }
            }

            return new DecorationPlacementResult(placements);
        }

        private float[,] GenerateDensityMap(GeneratedWorldData worldData, int seed)
        {
            int width = worldData.Width;
            int height = worldData.Height;
            var densityMap = new float[width, height];

            // Generate base noise for clustering
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float noiseValue = _noise.ValueNoise2D(x, y, seed, _config.ClusterRadius);
                densityMap[x, y] = Mathf.Clamp01(noiseValue);
            }

            // Apply cluster strength
            if (_config.ClusterStrength > 0f)
            {
                densityMap = ApplyClusterSmoothing(densityMap, width, height, _config.ClusterRadius, _config.ClusterStrength);
            }

            return densityMap;
        }

        private float[,] ApplyClusterSmoothing(float[,] map, int width, int height, int radius, float strength)
        {
            var smoothed = new float[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float sum = 0f;
                int count = 0;

                for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        sum += map[nx, ny];
                        count++;
                    }
                }

                float neighborAverage = count > 0 ? sum / count : map[x, y];
                smoothed[x, y] = Mathf.Lerp(map[x, y], neighborAverage, strength);
            }

            return smoothed;
        }

        private bool IsExcludedCell(int x, int y, GeneratedWorldData worldData)
        {
            string excludedTileId = ResolveTileId(x, y, worldData);
            if (IsRoadTile(excludedTileId))
                return true;

            // Check building exclusion
            if (_config.Exclusions.BuildingExclusionRadius > 0 && worldData.BuildingMap != null)
            {
                if (IsNearBuilding(x, y, worldData.BuildingMap, _config.Exclusions.BuildingExclusionRadius))
                    return true;
            }

            return false;
        }

        private bool IsNearBuilding(int x, int y, string[,] buildingMap, int radius)
        {
            int width = buildingMap.GetLength(0);
            int height = buildingMap.GetLength(1);

            for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;
                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    if (!string.IsNullOrWhiteSpace(buildingMap[nx, ny]))
                        return true;
                }
            }

            return false;
        }

        private float GetBiomeMultiplier(string tileId)
        {
            if (string.IsNullOrWhiteSpace(tileId))
                return 1f;

            tileId = tileId.ToLowerInvariant();

            if (tileId.Contains("forest"))
                return _config.BiomeRules.Forest;
            if (IsRockyTile(tileId))
                return _config.BiomeRules.Rocky;
            if (tileId.Contains("sand") || tileId.Contains("coast") || tileId.Contains("beach"))
                return _config.BiomeRules.Coast;
            if (tileId.Contains("water") || tileId.Contains("river") || tileId.Contains("lake"))
                return _config.BiomeRules.Water;
            if (tileId.Contains("grass"))
                return _config.BiomeRules.Grassland;

            return 1f;
        }

        private int CalculateObjectCount(float density, int seed, int x, int y)
        {
            if (density <= 0f)
                return 0;

            uint hash = DeterministicHash.CellHash(seed, x, y, 0);
            float randomValue = (hash % 10000) / 10000f;

            int maxObjects = _config.MaxObjectsPerTile;
            int count = 0;
            for (int i = 0; i < maxObjects; i++)
            {
                float threshold = density / (i + 1);
                if (randomValue < threshold)
                    count = i + 1;
                else
                    break;
            }

            return count;
        }

        private bool TryGenerateDecoration(
            int x, int y, int seed, int index, string tileId,
            GeneratedWorldData worldData, out DecorationPlacement placement)
        {
            placement = default;

            // Determine decoration type based on biome and random selection
            string decorationType = SelectDecorationType(tileId, seed, x, y, index);
            if (string.IsNullOrEmpty(decorationType))
                return false;

            // Shoreline buffer: heavy props stay off the water edge while
            // grass and flowers may grow right up to it.
            if (_config.Exclusions.ShorelineExclusionCells > 0
                && IsHeavyDecoration(decorationType)
                && IsNearWater(x, y, worldData, _config.Exclusions.ShorelineExclusionCells))
            {
                return false;
            }

            // Select specific asset variant
            if (!_config.AssetPools.TryGetValue(decorationType, out var assetPool) || assetPool.Length == 0)
                return false;

            uint variantHash = DeterministicHash.VariantHash(seed, x, y, index, decorationType);
            string assetId = assetPool[variantHash % assetPool.Length];

            // Verify asset exists in registry
            if (!_objectRegistry.TryGetDefinition(assetId, out var definition))
                return false;

            // Calculate visual variation
            Vector3 position = CalculatePosition(x, y, worldData, seed, index);
            Quaternion rotation = CalculateRotation(seed, x, y, index);
            Vector3 scale = CalculateScale(seed, x, y, index);

            // Oversized props must fit every cell under their footprint; the
            // resolver shifts them within a bounded radius or rejects the
            // spawn when no nearby position keeps the model on valid land.
            if (RequiresFootprintValidation(decorationType)
                && !TryResolveFootprint(
                    definition.VisualPrefab, ref position, ref x, ref y,
                    rotation, scale, worldData))
            {
                return false;
            }

            placement = new DecorationPlacement(
                assetId,
                position,
                rotation,
                scale,
                x,
                y);

            return true;
        }

        private string SelectDecorationType(string tileId, int seed, int x, int y, int index)
        {
            uint hash = DeterministicHash.CellHash(seed, x, y, index);
            float randomValue = (hash % 10000) / 10000f;

            float treeWeight = _config.TypeDensities.TreeDensity;
            float bushWeight = _config.TypeDensities.BushDensity;
            float grassWeight = _config.TypeDensities.GrassDensity;
            float flowerWeight = _config.TypeDensities.FlowerDensity;
            float rockWeight = _config.TypeDensities.RockDensity;

            string lowerTileId = tileId?.ToLowerInvariant();
            bool isForest = lowerTileId != null && lowerTileId.Contains("forest");

            // Adjust per-type weights based on biome before building the
            // cumulative distribution so thresholds stay ordered.
            if (isForest)
            {
                treeWeight *= 1.5f;
                grassWeight *= 0.8f;
            }
            else if (lowerTileId != null && IsRockyTile(lowerTileId))
            {
                // Rocky terrain grows no trees; the bush pool uses tree models.
                treeWeight = 0f;
                bushWeight = 0f;
                grassWeight *= 0.4f;
                rockWeight *= 3.0f;
            }
            else if (lowerTileId != null && lowerTileId.Contains("sand"))
            {
                treeWeight *= 0.1f;
                rockWeight *= 1.5f;
            }

            // Cut stumps appear only in forests, carved out of the tree band.
            float stumpWeight = 0f;
            if (isForest && HasPool("stump"))
            {
                stumpWeight = treeWeight * 0.15f;
                treeWeight -= stumpWeight;
            }

            // A type with no configured pool contributes no probability mass.
            if (!HasPool("tree"))
                treeWeight = 0f;
            if (!HasPool("bush"))
                bushWeight = 0f;
            if (!HasPool("grass"))
                grassWeight = 0f;
            if (!HasPool("flower"))
                flowerWeight = 0f;
            if (!HasPool("rock"))
                rockWeight = 0f;

            float stumpThreshold = treeWeight + stumpWeight;
            float bushThreshold = stumpThreshold + bushWeight;
            float grassThreshold = bushThreshold + grassWeight;
            float flowerThreshold = grassThreshold + flowerWeight;
            float rockThreshold = flowerThreshold + rockWeight;

            if (randomValue < treeWeight)
                return "tree";
            if (randomValue < stumpThreshold)
                return "stump";
            if (randomValue < bushThreshold)
                return "bush";
            if (randomValue < grassThreshold)
                return "grass";
            if (randomValue < flowerThreshold)
                return "flower";
            if (randomValue < rockThreshold)
                return "rock";

            return null;
        }

        /// <summary>
        /// Water flora (lilies, water plants) spawn sparsely on water cells
        /// and float just above the sheet surface via <c>YOffset</c>.
        /// </summary>
        private void TryGenerateWaterFlora(
            int x, int y, int seed, string tileId,
            GeneratedWorldData worldData, List<DecorationPlacement> placements)
        {
            const string type = "waterplant";
            float density = _config.TypeDensities.WaterPlantDensity * _config.GlobalDensity;
            if (density <= 0f || !HasPool(type))
                return;

            if (_config.Exclusions.BuildingExclusionRadius > 0
                && worldData.BuildingMap != null
                && IsNearBuilding(x, y, worldData.BuildingMap, _config.Exclusions.BuildingExclusionRadius))
            {
                return;
            }

            uint hash = DeterministicHash.CellHash(seed, x, y, 0);
            if ((hash % 10000) / 10000f >= density)
                return;

            var pool = _config.AssetPools[type];
            string assetId = pool[DeterministicHash.VariantHash(seed, x, y, 0, type) % pool.Length];
            if (!_objectRegistry.TryGetDefinition(assetId, out _))
                return;

            placements.Add(new DecorationPlacement(
                assetId,
                CalculatePosition(x, y, worldData, seed, 0),
                CalculateRotation(seed, x, y, 0),
                CalculateScale(seed, x, y, 0),
                x,
                y,
                WaterFloraSurfaceOffset));
        }

        private const float WaterFloraSurfaceOffset = 0.03f;

        private static bool IsHeavyDecoration(string decorationType)
            => decorationType == "tree"
               || decorationType == "stump"
               || decorationType == "rock";

        private static bool RequiresFootprintValidation(string decorationType)
            => IsHeavyDecoration(decorationType) || decorationType == "bush";

        private bool TryResolveFootprint(
            GameObject prefab, ref Vector3 position, ref int x, ref int y,
            Quaternion rotation, Vector3 scale, GeneratedWorldData worldData)
        {
            if (_placementResolver == null
                || _config.Footprint == null
                || !_config.Footprint.ValidateHeavyFootprints
                || worldData == null)
            {
                return true;
            }

            float cellSize = worldData.CellSize > 0.0001f ? worldData.CellSize : 1f;
            var request = new EnvironmentObjectPlacementResolver.Request(
                prefab,
                new Vector3(position.x * cellSize, 0f, position.z * cellSize),
                rotation,
                scale,
                worldData.Width,
                worldData.Height,
                cellSize,
                cell => IsFootprintCellAcceptable(cell, worldData),
                cell => ResolveSurfaceHeight(worldData, cell),
                _config.Footprint.MaxShiftCells,
                _config.Footprint.MaxGroundDeltaMeters,
                _config.Footprint.FootprintShrink);

            if (!_placementResolver.TryResolve(request, out Vector3 resolved))
                return false;

            position = new Vector3(
                resolved.x / cellSize,
                position.y,
                resolved.z / cellSize);
            x = Mathf.Clamp(Mathf.FloorToInt(position.x), 0, worldData.Width - 1);
            y = Mathf.Clamp(Mathf.FloorToInt(position.z), 0, worldData.Height - 1);
            return true;
        }

        // A footprint cell is acceptable only on valid non-water land; road
        // and building checks stay on the anchor cell so crowns may pass
        // above paths while trunks keep their authored clearances.
        private bool IsFootprintCellAcceptable(Vector2Int cell, GeneratedWorldData worldData)
        {
            string tileId = ResolveTileId(cell.x, cell.y, worldData);
            return !IsWaterTile(tileId)
                   && !HasWaterSheet(cell.x, cell.y, worldData)
                   && AllowsDecorationTile(tileId);
        }

        private static float ResolveSurfaceHeight(GeneratedWorldData worldData, Vector2Int cell)
        {
            float[,] heights = worldData?.LogicalTileMap?.SurfaceHeights;
            if (heights == null
                || cell.x < 0 || cell.y < 0
                || cell.x >= heights.GetLength(0) || cell.y >= heights.GetLength(1))
            {
                return float.NaN;
            }

            return heights[cell.x, cell.y];
        }

        private bool IsNearWater(int x, int y, GeneratedWorldData worldData, int radius)
        {
            for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;
                if (nx < 0 || ny < 0 || nx >= worldData.Width || ny >= worldData.Height)
                    continue;
                if (IsWaterTile(ResolveTileId(nx, ny, worldData))
                    || HasWaterSheet(nx, ny, worldData))
                {
                    return true;
                }
            }

            return false;
        }

        private bool AllowsDecorationTile(string tileId)
            => _placementPolicy == null
               || _placementPolicy.AllowsPlacement(
                   tileId,
                   TerrainPlacementOperation.Decoration);

        private bool HasPool(string decorationType)
        {
            return _config.AssetPools.TryGetValue(decorationType, out var pool)
                && pool != null && pool.Length > 0;
        }

        private Vector3 CalculatePosition(int x, int y, GeneratedWorldData worldData, int seed, int index)
        {
            float height = 0f;
            if (worldData.HeightMap != null && x < worldData.Width && y < worldData.Height)
            {
                height = worldData.HeightMap[x, y];
            }

            Vector3 basePosition = new Vector3(x, height, y);

            if (_config.VisualVariation.MaxPositionOffset > 0f)
            {
                uint hash = DeterministicHash.VariantHash(seed, x, y, index, "position");
                float offsetX = ((hash % 2000) / 1000f - 1f) * _config.VisualVariation.MaxPositionOffset;
                float offsetZ = ((hash / 2000 % 2000) / 1000f - 1f) * _config.VisualVariation.MaxPositionOffset;
                basePosition += new Vector3(offsetX, 0f, offsetZ);
            }

            return basePosition;
        }

        private Quaternion CalculateRotation(int seed, int x, int y, int index)
        {
            if (!_config.VisualVariation.EnableRotation)
                return Quaternion.identity;

            uint hash = DeterministicHash.VariantHash(seed, x, y, index, "rotation");
            float angle = (hash % 360);
            return Quaternion.Euler(0f, angle, 0f);
        }

        private Vector3 CalculateScale(int seed, int x, int y, int index)
        {
            if (!_config.VisualVariation.EnableScaleVariation)
                return Vector3.one;

            uint hash = DeterministicHash.VariantHash(seed, x, y, index, "scale");
            float t = (hash % 10000) / 10000f;
            float scale = Mathf.Lerp(_config.VisualVariation.MinScale, _config.VisualVariation.MaxScale, t);
            return Vector3.one * scale;
        }

        // GameplayTileMap carries the resolved terrain id; BiomeMap can still
        // hold pre-resolution markers such as "sand-shore-band".
        private static string ResolveTileId(int x, int y, GeneratedWorldData worldData)
        {
            string gameplay = worldData.GameplayTileMap?[x, y];
            return !string.IsNullOrWhiteSpace(gameplay)
                ? gameplay
                : worldData.BiomeMap?[x, y];
        }

        private static bool IsRockyTile(string tileId)
        {
            return tileId.Contains("hill")
                || tileId.Contains("mountain")
                || tileId.Contains("stone")
                || tileId.Contains("rock")
                || tileId.Contains("cliff");
        }

        private static bool IsRoadTile(string tileId)
        {
            if (string.IsNullOrWhiteSpace(tileId))
                return false;

            tileId = tileId.ToLowerInvariant();
            return tileId.Contains("road")
                || tileId.Contains("footpath")
                || tileId.Contains("path")
                || tileId.Contains("street");
        }

        private static bool IsWaterTile(string tileId)
        {
            if (string.IsNullOrWhiteSpace(tileId))
                return false;

            tileId = tileId.ToLowerInvariant();
            return tileId.Contains("water") || tileId.Contains("river") ||
                   tileId.Contains("lake") || tileId.Contains("ocean");
        }

        /// <summary>
        /// River and lake cells keep their land gameplay id while carrying a
        /// surface-only water sheet in the stack; the sheet counts as water
        /// for placement, matching the object spawner's water-cell rule.
        /// </summary>
        private static bool HasWaterSheet(int x, int y, GeneratedWorldData worldData)
        {
            var stack = worldData?.LogicalTileMap?.GetCellStack(x, y);
            if (stack == null)
                return false;

            for (int i = 0; i < stack.Samples.Count; i++)
            {
                var sample = stack.Samples[i];
                if (sample.IsTerrainLike
                    && sample.TileGeometryMode == TileGeometryMode.SurfaceOnly)
                {
                    return true;
                }
            }

            return false;
        }
    }

    internal readonly struct DecorationPlacement
    {
        public readonly string AssetId;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 Scale;
        public readonly int TileX;
        public readonly int TileY;
        /// <summary>Extra height above the resolved surface (e.g. water flora).</summary>
        public readonly float YOffset;

        public DecorationPlacement(string assetId, Vector3 position, Quaternion rotation, Vector3 scale, int tileX, int tileY, float yOffset = 0f)
        {
            AssetId = assetId;
            Position = position;
            Rotation = rotation;
            Scale = scale;
            TileX = tileX;
            TileY = tileY;
            YOffset = yOffset;
        }
    }

    internal sealed class DecorationPlacementResult
    {
        public static readonly DecorationPlacementResult Empty = new DecorationPlacementResult(Array.Empty<DecorationPlacement>());

        public readonly DecorationPlacement[] Placements;

        public DecorationPlacementResult(IReadOnlyList<DecorationPlacement> placements)
        {
            Placements = placements?.ToArray() ?? Array.Empty<DecorationPlacement>();
        }

        public int Count => Placements.Length;
    }
}
