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

        [Tooltip("Noise scale in cells — larger values produce wider plateaus.")]
        [Min(0.0001f)] public float NoiseScale = 26f;

        [Range(1, 8)] public int Octaves = 4;
        [Range(0.01f, 1f)] public float Persistence = 0.5f;
        [Min(1f)] public float Lacunarity = 2f;
        public Vector2 Offset = Vector2.zero;

        [Tooltip("Majority-smoothing passes over quantized levels; removes single-cell noise spikes.")]
        [Range(0, 8)] public int SmoothingIterations = 2;

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
        [Min(0f)] public float OverlaySurfaceOffsetMeters = 0.003f;

        [Tooltip("Extra path cost per meter of height change; higher values keep roads on flat ground.")]
        [Min(0f)] public float HeightPenaltyPerMeter = 4f;

        [Tooltip("Fraction of routes rendered as roads; the rest become footpaths.")]
        [Range(0f, 1f)] public float RoadFraction = 0.6f;

        [Tooltip("Seed salt so routes do not correlate with relief or layer masks.")]
        public int SeedSalt = 4177;
    }
}
