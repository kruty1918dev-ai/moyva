using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    /// <summary>
    /// Recipe-authored deterministic terrain relief: an fbm height field
    /// quantized to fixed terraces. Computed before mask compilation so layers
    /// can select terrain by level via <see cref="TerrainLevelMaskStep"/>, then
    /// applied to every terrain sample as a per-cell surface height.
    /// </summary>
    [System.Serializable]
    public sealed class TerrainReliefConfig
    {
        public bool Enabled;

        [Tooltip("Terrace height step in meters. 0.25 matches the atlas tile contract.")]
        [Min(0.01f)] public float QuantumMeters = 0.25f;

        [Tooltip("Number of terrace steps above the minimum field height.")]
        [Min(0)] public int MaxSteps = 5;

        [Tooltip("Exponent applied before terrace quantization. Values above 1 reserve high ground for peaks.")]
        [Min(1f)] public float HeightExponent = 1f;

        [Tooltip("Noise scale in cells — larger values produce wider plateaus.")]
        [Min(0.0001f)] public float NoiseScale = 26f;

        [Range(1, 8)] public int Octaves = 4;
        [Range(0.01f, 1f)] public float Persistence = 0.5f;
        [Min(1f)] public float Lacunarity = 2f;
        public Vector2 Offset = Vector2.zero;

        [Tooltip("Majority-smoothing passes over quantized levels; removes single-cell noise spikes.")]
        [Range(0, 8)] public int SmoothingIterations = 2;

        [Tooltip("Terraces covering fewer cells than this collapse into the surrounding level. 0 = off.")]
        [Min(0)] public int MinPlateauCells = 0;

        [Tooltip("Minimum distance between elevated bumps; closer bumps are suppressed. 0 = off.")]
        [Min(0)] public int BumpMinSpacingCells = 0;

        [Tooltip("Maximum elevated bumps kept per map; excess bumps collapse. 0 = unlimited.")]
        [Min(0)] public int MaxBumpCount = 0;

        [Tooltip("Passes that lower cells standing above all four neighbours; erodes spikes and knife ridges.")]
        [Range(0, 8)] public int RidgeErosionIterations = 0;

        [Tooltip("Seed salt so relief does not correlate with layer masks sharing the map seed.")]
        public int SeedSalt = 9137;
    }

    /// <summary>
    /// Recipe-authored stair passage planning. After relief is applied the
    /// planner cuts stair corridors across ledges whose drop exceeds the
    /// direct-walk limit.
    /// </summary>
    [System.Serializable]
    public sealed class TerrainPassageConfig
    {
        public bool Enabled;

        [Tooltip("Atlas theme used for generated stair modules (e.g. stone).")]
        public string StairThemeId = "stone";

        [Tooltip("Semantic tile id written to stair cells.")]
        public string StairTileId = "stair";

        [Tooltip("Rise covered by one stair module.")]
        [Min(0.01f)] public float ModuleRiseMeters = 0.25f;

        [Tooltip("Minimum ledge drop that receives a stair flight.")]
        [Min(0.01f)] public float MinLedgeDropMeters = 0.5f;

        [Tooltip("Largest ledge drop a single flight may cover.")]
        [Min(0.01f)] public float MaxLedgeDropMeters = 1f;

        [Tooltip("Minimum distance in cells between stair entrances along a ledge.")]
        [Min(1)] public int MinEntranceSpacingCells = 3;

        [Tooltip("Maximum number of flights per map.")]
        [Min(0)] public int MaxFlights = 64;
    }

    /// <summary>
    /// Recipe-authored route planning: seeded anchors on traversable terrain,
    /// connected by A* paths that prefer flat ground and stair passages, then
    /// written to the logical map as road/footpath overlay tiles.
    /// </summary>
    [System.Serializable]
    public sealed class TerrainRouteConfig
    {
        public bool Enabled;

        [Tooltip("Number of seeded route anchor cells.")]
        [Min(2)] public int AnchorCount = 6;

        [Tooltip("Tile/preset id used for primary route cells.")]
        public string RoadTileId = "road";

        [Tooltip("Tile/preset id used for secondary route cells.")]
        public string FootpathTileId = "footpath";

        [Tooltip("Surface offset applied to route overlay tiles so they render above terrain.")]
        [Min(0f)] public float OverlaySurfaceOffsetMeters = 0.012f;

        [Tooltip("Extra path cost per meter of height change; higher values keep roads on flat ground.")]
        [Min(0f)] public float HeightPenaltyPerMeter = 4f;

        [Tooltip("Fraction of routes rendered as roads; the rest become footpaths.")]
        [Range(0f, 1f)] public float RoadFraction = 0.6f;

        [Tooltip("Seed salt so routes do not correlate with relief or layer masks.")]
        public int SeedSalt = 4177;
    }

    /// <summary>
    /// Recipe-authored shoreline planning. After relief is applied, land cells
    /// close to actual water (sea, rivers, lakes) are re-typed to the shore
    /// tile and graded down toward the neighbouring water surface so the
    /// waterline reads as a smooth continuous beach instead of a height step.
    /// </summary>
    [System.Serializable]
    public sealed class TerrainShoreConfig
    {
        public bool Enabled;

        [Tooltip("Semantic tile/preset id written to water-adjacent land cells.")]
        public string ShoreTileId = "sand";

        [Tooltip("Preset id used for converted shore cells when no native shore layer exists. Defaults to ShoreTileId.")]
        public string ShorePresetId;

        [Tooltip("Land cells within this Chebyshev distance of water convert to the shore tile.")]
        [Min(1)] public int BandCells = 2;

        [Tooltip("Extra ring beyond the band that is only graded down toward the water without changing tile type.")]
        [Min(0)] public int BlendCells = 3;

        [Tooltip("Beach surface lift above the adjacent water surface in meters.")]
        [Range(0.005f, 0.5f)] public float ShoreLiftMeters = 0.04f;

        [Tooltip("Maximum surface rise per cell of distance from water; caps the grading ramp.")]
        [Min(0.01f)] public float RisePerCellMeters = 0.2f;

        [Tooltip("Land cells whose surface exceeds water + lift by more than this stay untouched (cliff shoreline).")]
        [Min(0.05f)] public float MaxDropToWaterMeters = 0.9f;

        [Tooltip("Fraction of band cells converted to the shore tile; the rest stay their terrain but still grade toward the waterline. Below 1 the band reads as an irregular partial-tile strip instead of a uniform full-cell ring.")]
        [Range(0f, 1f)] public float BandCoverage = 1f;

        [Tooltip("Seed salt so the coverage gate does not correlate with relief or layer masks.")]
        public int SeedSalt = 947;

        [Tooltip("Extra winner tile ids counted as water for shore detection (e.g. 'swamp' tiles that render water but are not in shared WaterLikeTileIds). Shore-only: these ids are not skipped by relief/carve passes.")]
        public string[] WaterTileIds;
    }

    /// <summary>
    /// Recipe-authored deterministic hydrology: priority-flood the relief field,
    /// accumulate D8 drainage, then mark lake depressions and river cells.
    /// Rivers drain to the configured sink layer mask (open water) or the map
    /// border. Requires <see cref="TerrainReliefConfig"/> to be enabled.
    /// </summary>
    [System.Serializable]
    public sealed class RecipeHydrologyConfig
    {
        public bool Enabled;

        [Tooltip("Layer id whose final mask marks open water that rivers drain into. Evaluated before hydrology layers.")]
        public string SinkLayerId;

        [Tooltip("Terrain at or below this height (meters) also counts as a sink. Covers the open-water sheet when no sink layer is set.")]
        public float SinkMaxMeters = 0.1f;

        [Tooltip("Upstream drainage (in cells) a land cell needs to become a river.")]
        [Min(2)] public int RiverAccumulationThreshold = 12;

        [Tooltip("Minimum terrain height in meters for a river source cell.")]
        public float RiverMinSourceMeters = 0.25f;

        [Tooltip("Maximum river cells as a fraction of the map area.")]
        [Range(0f, 0.2f)] public float RiverMaxFraction = 0.035f;

        [Tooltip("Minimum flooded depth in meters for a depression to become a lake.")]
        [Min(0.01f)] public float LakeMinDepthMeters = 0.25f;

        [Tooltip("Maximum lake cells as a fraction of the map area.")]
        [Range(0f, 0.2f)] public float LakeMaxFraction = 0.03f;

        [Tooltip("Water surface offset in meters relative to the flooded level. Negative recesses water below banks.")]
        [Range(-0.5f, 0.5f)] public float WaterSurfaceOffsetMeters = -0.03f;

        [Tooltip("Drop between a water cell and its downstream cell that marks a waterfall.")]
        [Min(0.01f)] public float WaterfallMinDropMeters = 0.5f;

        [Tooltip("Rendered channel depth in meters: river bed sits this far below the water surface.")]
        [Min(0.05f)] public float ChannelDepthMeters = 0.35f;

        [Tooltip("Continuous sloping seabed under water bodies; replaces per-cell bed columns.")]
        public RecipeSeabedConfig Seabed = new();

        [Tooltip("Stylized Water 3 waterfall curtains at water-to-water drops; replaces stretched water strips.")]
        public RecipeWaterfallConfig Waterfalls = new();

        [Tooltip("Seed salt so hydrology does not correlate with relief or layer masks.")]
        public int SeedSalt = 7331;
    }

    /// <summary>
    /// Visual seabed profile: one continuous low-poly surface under every
    /// water body that drops from the waterline at the shore to a per-kind
    /// maximum depth. Depth grows with world-space distance to the nearest
    /// land cell, computed inside each water body so it cannot jump across
    /// land into a different body.
    /// </summary>
    [System.Serializable]
    public sealed class RecipeSeabedConfig
    {
        public bool Enabled = true;

        [Tooltip("Shallow shelf width in meters from the shoreline where the bed stays near the surface.")]
        [Min(0f)] public float ShallowShelfMeters = 1f;

        [Tooltip("Meters of additional shore distance over which the bed falls to its maximum depth.")]
        [Min(0.1f)] public float FalloffMeters = 3f;

        [Tooltip("Depth ramp exponent: >1 keeps more shallow water, <1 deepens sooner.")]
        [Range(0.25f, 4f)] public float DepthCurveExponent = 1.6f;

        [Tooltip("Maximum seabed depth below the local water surface for open water/sea cells.")]
        [Min(0.05f)] public float MaxDepthSeaMeters = 2f;

        [Tooltip("Maximum seabed depth below the local water surface for lake cells.")]
        [Min(0.05f)] public float MaxDepthLakeMeters = 1.2f;

        [Tooltip("Maximum seabed depth below the local water surface for river cells.")]
        [Min(0.05f)] public float MaxDepthRiverMeters = 0.5f;

        [Tooltip("Additional falloff scale for rivers so narrow channels reach depth sooner.")]
        [Range(0.1f, 1f)] public float RiverFalloffScale = 0.4f;

        [Tooltip("Small recess below the waterline at shore vertices to avoid z-fighting the water sheet edge.")]
        [Range(0f, 0.2f)] public float ShoreRecessMeters = 0.02f;

        [Tooltip("Vertical drop of the skirt at water cells on the map border so the bed edge never opens to void.")]
        [Min(0f)] public float BorderSkirtMeters = 0.4f;
    }

    /// <summary>
    /// Waterfall fronts: curtain meshes on the Stylized Water 3 waterfall
    /// material (world-space UV scroll runs downward on any geometry) plus
    /// the package's edge/splash/mist VFX prefabs. The drop threshold is
    /// expressed in terrain-height-step units so it follows the level grid
    /// instead of an absolute meter constant.
    /// </summary>
    [System.Serializable]
    public sealed class RecipeWaterfallConfig
    {
        public bool Enabled = true;

        [Tooltip("Minimum surface drop between an upper water cell and its lower water neighbour, in terrain-height-step units. Two levels is the recommended starting threshold.")]
        [Min(1)] public int MinDropLevels = 2;

        [Tooltip("Front curtain material. The SW3 waterfall material scrolls foam downward in world space; falls back to the water preset material when unset.")]
        public Material CurtainMaterial;

        [Tooltip("Foam strip prefab placed along the fall lip (Stylized Water 3 'Waterfall Edge').")]
        public GameObject EdgeFoamPrefab;

        [Tooltip("Impact splash prefab placed at the fall base (Stylized Water 3 'Waterfall Impact Splashes').")]
        public GameObject ImpactSplashPrefab;

        [Tooltip("Mist prefab placed at the base of tall falls (Stylized Water 3 'WaterfallMist').")]
        public GameObject MistPrefab;

        [Tooltip("Drop height, in terrain-height-step units, from which a mist plume is added at the base.")]
        [Min(1)] public int MistMinDropLevels = 3;

        [Tooltip("Hard cap on spawned waterfall VFX objects per map (mobile particle budget). The largest fronts win.")]
        [Min(0)] public int MaxVfxPerMap = 18;

        [Tooltip("Uniform scale applied to VFX prefabs; widths additionally scale the edge-foam emitter.")]
        [Range(0.25f, 2f)] public float VfxScale = 0.7f;

        [Tooltip("Cap on per-instance particle counts so SW3 prefabs stay inside the mobile budget.")]
        [Min(1)] public int MaxParticlesPerVfx = 60;
    }
}
