using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
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
        private readonly DeterministicNoise _noise;

        public EnvironmentDecorationGenerator(
            EnvironmentDecorationConfig config,
            IMapObjectRegistryService objectRegistry)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _objectRegistry = objectRegistry ?? throw new ArgumentNullException(nameof(objectRegistry));
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
                if (IsExcludedCell(x, y, worldData))
                    continue;

                float cellDensity = densityMap[x, y];
                if (cellDensity <= 0f)
                    continue;

                string tileId = worldData.BiomeMap?[x, y] ?? worldData.GameplayTileMap?[x, y];
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
            // Check water exclusion
            if (_config.Exclusions.SuppressWaterDecorations)
            {
                string tileId = worldData.BiomeMap?[x, y] ?? worldData.GameplayTileMap?[x, y];
                if (IsWaterTile(tileId))
                    return true;
            }

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
            if (tileId.Contains("hill") || tileId.Contains("mountain") || tileId.Contains("stone"))
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
            for (int i = 0; i < maxObjects; i++)
            {
                float threshold = density / (i + 1);
                if (randomValue < threshold)
                    return i + 1;
            }

            return 0;
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

            float treeThreshold = _config.TypeDensities.TreeDensity;
            float bushThreshold = treeThreshold + _config.TypeDensities.BushDensity;
            float grassThreshold = bushThreshold + _config.TypeDensities.GrassDensity;
            float rockThreshold = grassThreshold + _config.TypeDensities.RockDensity;

            // Adjust based on biome
            if (tileId != null && tileId.ToLowerInvariant().Contains("forest"))
            {
                treeThreshold *= 1.5f;
                grassThreshold *= 0.8f;
            }
            else if (tileId != null && (tileId.ToLowerInvariant().Contains("hill") || 
                     tileId.ToLowerInvariant().Contains("mountain")))
            {
                rockThreshold *= 2.0f;
                treeThreshold *= 0.3f;
            }
            else if (tileId != null && tileId.ToLowerInvariant().Contains("sand"))
            {
                treeThreshold *= 0.1f;
                rockThreshold *= 1.5f;
            }

            if (randomValue < treeThreshold)
                return "tree";
            if (randomValue < bushThreshold)
                return null; // Bush not in current asset pool
            if (randomValue < grassThreshold)
                return null; // Grass not implemented as 3D objects
            if (randomValue < rockThreshold)
                return "rock";

            return null;
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

        private static bool IsWaterTile(string tileId)
        {
            if (string.IsNullOrWhiteSpace(tileId))
                return false;

            tileId = tileId.ToLowerInvariant();
            return tileId.Contains("water") || tileId.Contains("river") || 
                   tileId.Contains("lake") || tileId.Contains("ocean");
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

        public DecorationPlacement(string assetId, Vector3 position, Quaternion rotation, Vector3 scale, int tileX, int tileY)
        {
            AssetId = assetId;
            Position = position;
            Rotation = rotation;
            Scale = scale;
            TileX = tileX;
            TileY = tileY;
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
