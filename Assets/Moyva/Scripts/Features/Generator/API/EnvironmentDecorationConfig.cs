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
        /// Full-model footprint validation and grounding for heavy props.
        /// </summary>
        public FootprintRules Footprint = new();

        /// <summary>
        /// Asset variant pools for each environment type.
        /// Maps environment type IDs to lists of map object registry IDs.
        /// </summary>
        [SerializeField] public Dictionary<string, string[]> AssetPools = new();

        /// <summary>
        /// Optional additive placement layers evaluated after the legacy
        /// weighted type selection. Each layer references an
        /// <see cref="AssetPools"/> entry by <see cref="DecorationLayerRule.Type"/>
        /// and can spawn additional instances per cell, so low vegetation
        /// no longer competes with trees for the shared per-tile cap.
        /// Null or empty keeps legacy behaviour byte-identical.
        /// </summary>
        public DecorationLayerRule[] Layers;
    }

    /// <summary>How a layer relates to water proximity.</summary>
    public enum DecorationWaterAffinity
    {
        /// <summary>Water proximity does not affect the layer.</summary>
        Any = 0,
        /// <summary>Cells within WaterRadius of water are skipped.</summary>
        Avoid = 1,
        /// <summary>Cells within WaterRadius of water get Weight * WaterBoost.</summary>
        Prefer = 2,
        /// <summary>Only cells within WaterRadius of water may spawn.</summary>
        Require = 3,
    }

    /// <summary>How a layer relates to forest tiles.</summary>
    public enum DecorationForestAffinity
    {
        /// <summary>Forest membership does not affect the layer.</summary>
        Any = 0,
        /// <summary>Only forest tiles whose ring neighbours are all forest.</summary>
        Interior = 1,
        /// <summary>Only forest tiles with at least one non-forest neighbour in the ring.</summary>
        Edge = 2,
        /// <summary>Only non-forest tiles.</summary>
        Avoid = 3,
    }

    [Serializable]
    public sealed class DecorationLayerRule
    {
        /// <summary>AssetPools key whose entries provide this layer's visuals.</summary>
        public string Type;

        /// <summary>Per-tile spawn probability before biome/affinity modifiers.</summary>
        [Range(0f, 2f)]
        public float Weight = 0.5f;

        /// <summary>Maximum placements this layer may add to one cell.</summary>
        [Min(1)]
        public int MaxPerTile = 1;

        /// <summary>
        /// Optional per-biome multiplier applied on top of BiomeRules.
        /// Keys: grassland, forest, rocky, coast, water. Missing keys = 1.
        /// </summary>
        [SerializeField] public Dictionary<string, float> BiomeBoost;

        /// <summary>Water proximity behaviour for this layer.</summary>
        public DecorationWaterAffinity WaterAffinity = DecorationWaterAffinity.Any;

        /// <summary>Radius in cells for water proximity checks.</summary>
        [Min(1)]
        public int WaterRadius = 1;

        /// <summary>Weight multiplier when Prefer sees water within WaterRadius.</summary>
        [Range(0f, 5f)]
        public float WaterBoost = 2f;

        /// <summary>Forest membership behaviour for this layer.</summary>
        public DecorationForestAffinity ForestAffinity = DecorationForestAffinity.Any;

        /// <summary>Ring radius used for interior/edge classification.</summary>
        [Min(1)]
        public int ForestEdgeRadius = 1;

        /// <summary>
        /// Extra spawn probability when a tree anchor (legacy tree/stump
        /// placement or a layer with FeedsTreeAffinity) sits within
        /// NearTreeRadius. Weight 0 + NearTreeBoost &gt; 0 means "under trees only".
        /// </summary>
        [Range(0f, 2f)]
        public float NearTreeBoost = 0f;

        /// <summary>Radius in cells for tree-anchor proximity checks.</summary>
        [Min(1)]
        public int NearTreeRadius = 1;

        /// <summary>
        /// Skip cells carrying a gameplay object id (resource POIs, river
        /// channels) in worldData.ObjectMap so decor never overlaps
        /// interactive objects.
        /// </summary>
        public bool SkipObjectCells = true;

        /// <summary>
        /// Heavy-prop rule: skip cells within Exclusions.ShorelineExclusionCells
        /// of water. Light layers should keep this false.
        /// </summary>
        public bool ShorelineExclusion;

        /// <summary>
        /// Run the heavy-footprint resolver so oversized layer props keep
        /// their full footprint on valid ground.
        /// </summary>
        public bool ValidateFootprint;

        /// <summary>
        /// This layer's placements count as tree anchors for other layers'
        /// NearTreeBoost (e.g. saplings).
        /// </summary>
        public bool FeedsTreeAffinity;

        /// <summary>
        /// Maximum local surface-height drop across the cell in meters;
        /// steeper cells are skipped. 0 disables the slope check.
        /// </summary>
        [Min(0f)]
        public float MaxSlopeMeters = 0f;

        /// <summary>Per-layer scale override; 0 inherits VisualVariation.</summary>
        [Range(0f, 3f)]
        public float MinScale = 0f;

        /// <summary>Per-layer scale override; 0 inherits VisualVariation.</summary>
        [Range(0f, 3f)]
        public float MaxScale = 0f;

        /// <summary>Extra height above the resolved surface (e.g. floating flora).</summary>
        public float YOffset = 0f;
    }

    [Serializable]
    public sealed class FootprintRules
    {
        /// <summary>
        /// Validate the full top-down renderer bounds (children + LODs) of
        /// heavy props (trees, stumps, rocks); every cell under the footprint
        /// must stay placeable so crowns never hang over water.
        /// </summary>
        public bool ValidateHeavyFootprints = true;

        /// <summary>
        /// Bounded shift radius in cells applied when a footprint does not
        /// fit at its authored offset; beyond it the prop is skipped.
        /// </summary>
        [Min(0f)]
        public float MaxShiftCells = 0.75f;

        /// <summary>
        /// Maximum surface-height delta tolerated under a footprint before
        /// the prop is shifted or skipped.
        /// </summary>
        [Min(0.01f)]
        public float MaxGroundDeltaMeters = 0.6f;

        /// <summary>
        /// Fraction of the XZ renderer bounds used as the footprint.
        /// 1 = full bounds; smaller values forgive thin crown tips.
        /// </summary>
        [Range(0.3f, 1f)]
        public float FootprintShrink = 0.9f;
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
        /// Base probability per tile for flower decorations.
        /// </summary>
        [Range(0f, 1f)]
        public float FlowerDensity = 0.05f;

        /// <summary>
        /// Base probability per tile for rock/stone decorations.
        /// </summary>
        [Range(0f, 1f)]
        public float RockDensity = 0.08f;

        /// <summary>
        /// Base probability per water tile for water flora (lilies, water plants).
        /// </summary>
        [Range(0f, 1f)]
        public float WaterPlantDensity = 0.1f;
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

        /// <summary>
        /// Radius in cells around water where heavy decorations (trees, rocks,
        /// stumps) are suppressed. Grass and flowers may still spawn.
        /// </summary>
        [Min(0)]
        public int ShorelineExclusionCells = 0;
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

        /// <summary>
        /// Align decorations to the terrain surface normal so props sit
        /// flush on slopes instead of staying world-upright.
        /// </summary>
        public bool AlignToSurface = true;
    }
}
