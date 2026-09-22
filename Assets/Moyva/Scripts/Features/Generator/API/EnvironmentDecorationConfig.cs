using System;
using System.Collections.Generic;
using Kruty1918.JsonConfig;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    /// <summary>
    /// Configuration for procedural environment decoration generation.
    /// Controls density, clustering, biome rules, and asset selection for visual-only decorations.
    /// </summary>
    [Serializable]
    public sealed class EnvironmentDecorationConfig : JsonConfigObject
    {
        /// <summary>
        /// Master switch for environment decoration generation.
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// Global density multiplier applied to all decoration types.
        /// </summary>
        [Range(0f, 2f)]
        public float GlobalDensity = 1f;

        /// <summary>
        /// Seed offset to vary decoration patterns without changing map seed.
        /// </summary>
        public int SeedOffset = 0;

        /// <summary>
        /// Maximum number of decoration objects per tile.
        /// </summary>
        [Min(0)]
        public int MaxObjectsPerTile = 3;

        /// <summary>
        /// Cluster strength determines how grouped decorations are.
        /// Higher values create tighter clusters.
        /// </summary>
        [Range(0f, 1f)]
        public float ClusterStrength = 0.7f;

        /// <summary>
        /// Radius in tiles for cluster influence.
        /// </summary>
        [Min(1)]
        public int ClusterRadius = 3;

        /// <summary>
        /// Per-environment-type density settings.
        /// </summary>
        public EnvironmentTypeDensities TypeDensities = new();

        /// <summary>
        /// Biome/terrain-specific multipliers for decoration density.
        /// </summary>
        public BiomeMultipliers BiomeRules = new();

        /// <summary>
        /// Exclusion zones to prevent decoration spawning near important gameplay areas.
        /// </summary>
        public ExclusionZones Exclusions = new();

        /// <summary>
        /// Visual variation settings for spawned decorations.
        /// </summary>
        public VisualVariation VisualVariation = new();

        /// <summary>
        /// Asset variant pools for each environment type.
        /// Maps environment type IDs to lists of map object registry IDs.
        /// </summary>
        [SerializeField] public Dictionary<string, string[]> AssetPools = new();
    }

    [Serializable]
    public sealed class EnvironmentTypeDensities
    {
        /// <summary>
        /// Base probability per tile for tree decorations.
        /// </summary>
        [Range(0f, 1f)]
        public float TreeDensity = 0.15f;

        /// <summary>
        /// Base probability per tile for bush decorations.
        /// </summary>
        [Range(0f, 1f)]
        public float BushDensity = 0.1f;

        /// <summary>
        /// Base probability per tile for grass/flower decorations.
        /// </summary>
        [Range(0f, 1f)]
        public float GrassDensity = 0.2f;

        /// <summary>
        /// Base probability per tile for rock/stone decorations.
        /// </summary>
        [Range(0f, 1f)]
        public float RockDensity = 0.08f;
    }

    [Serializable]
    public sealed class BiomeMultipliers
    {
        /// <summary>
        /// Multiplier for grassland tiles (e.g., "grass").
        /// </summary>
        [Range(0f, 3f)]
        public float Grassland = 1.2f;

        /// <summary>
        /// Multiplier for forest tiles (e.g., "forest-sparse", "forest-dense").
        /// </summary>
        [Range(0f, 3f)]
        public float Forest = 2.0f;

        /// <summary>
        /// Multiplier for hill/mountain tiles (e.g., "hill", "mountain").
        /// </summary>
        [Range(0f, 3f)]
        public float Rocky = 1.5f;

        /// <summary>
        /// Multiplier for sand/coast tiles (e.g., "sand").
        /// </summary>
        [Range(0f, 3f)]
        public float Coast = 0.5f;

        /// <summary>
        /// Multiplier for water tiles (typically very low or zero).
        /// </summary>
        [Range(0f, 3f)]
        public float Water = 0f;
    }

    [Serializable]
    public sealed class ExclusionZones
    {
        /// <summary>
        /// Radius in tiles around buildings where decorations are suppressed.
        /// </summary>
        [Min(0)]
        public int BuildingExclusionRadius = 1;

        /// <summary>
        /// Radius in tiles around settlement centers where decorations are suppressed.
        /// </summary>
        [Min(0)]
        public int SettlementExclusionRadius = 2;

        /// <summary>
        /// Whether to completely suppress decorations on water tiles.
        /// </summary>
        public bool SuppressWaterDecorations = true;
    }

    [Serializable]
    public sealed class VisualVariation
    {
        /// <summary>
        /// Enable random Y-axis rotation for decorations.
        /// </summary>
        public bool EnableRotation = true;

        /// <summary>
        /// Enable random scale variation for decorations.
        /// </summary>
        public bool EnableScaleVariation = true;

        /// <summary>
        /// Minimum scale multiplier (uniform scale).
        /// </summary>
        [Range(0.5f, 1f)]
        public float MinScale = 0.85f;

        /// <summary>
        /// Maximum scale multiplier (uniform scale).
        /// </summary>
        [Range(1f, 1.5f)]
        public float MaxScale = 1.15f;

        /// <summary>
        /// Maximum positional offset in local units within a tile.
        /// </summary>
        [Range(0f, 0.5f)]
        public float MaxPositionOffset = 0.2f;
    }
}
