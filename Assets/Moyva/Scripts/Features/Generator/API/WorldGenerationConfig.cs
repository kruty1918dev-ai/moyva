using System;
using Kruty1918.JsonConfig;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.API
{
    /// <summary>
    /// JSON-authored parameters for the deterministic world-geography engine.
    /// Loaded via <c>JsonConfigRuntime.Get&lt;WorldGenerationConfig&gt;</c>.
    /// </summary>
    [Serializable]
    public sealed class WorldGenerationConfig : JsonConfigObject
    {
        /// <summary>Master switch. When false the graph-driven generator stays canonical.</summary>
        public bool Enabled = true;

        /// <summary>Maximum deterministic generation attempts before accepting the best candidate.</summary>
        [Min(1)]
        public int MaxGenerationAttempts = 6;

        /// <summary>Integer terrain levels mirrored from the TWC build options.</summary>
        public TerrainLevelSettings TerrainLevels = new();

        /// <summary>Per-archetype macro-geography parameters.</summary>
        public ArchetypeParameters[] Archetypes = DefaultArchetypes();

        /// <summary>Hydrology thresholds shared by all archetypes.</summary>
        public HydrologySettings Hydrology = new();

        /// <summary>Coastline/beach shaping.</summary>
        public CoastSettings Coast = new();

        /// <summary>Biome selection weights.</summary>
        public BiomeSettings Biomes = new();

        /// <summary>Strategic object placement.</summary>
        public ObjectPlacementSettings Objects = new();

        /// <summary>Spawn fairness gates and parity metrics.</summary>
        public FairnessSettings Fairness = new();

        [Serializable]
        public sealed class TerrainLevelSettings
        {
            [Min(0)] public int WaterLevel = 0;
            [Min(0)] public int ShoreLevel = 1;
            [Min(0)] public int LandLevel = 1;
            [Min(0)] public int HillLevel = 3;
            [Min(0)] public int MaxLevel = 5;
            /// <summary>World-space height of one integer level (TWC terrainHeightStep).</summary>
            [Min(0.05f)] public float HeightStep = 1f;
            /// <summary>Water surface offset below the containing level's surface.</summary>
            [Range(-0.9f, -0.05f)] public float WaterSurfaceOffset = -0.35f;
        }

        [Serializable]
        public sealed class ArchetypeParameters
        {
            /// <summary>Stable archetype id: continents|pangaea|islands|highlands|desert|balanced.</summary>
            public string Id = "balanced";
            /// <summary>Target fraction of land cells (0..1).</summary>
            [Range(0.05f, 0.95f)] public float LandRatio = 0.45f;
            /// <summary>Number of macro continent/island centres.</summary>
            [Min(1)] public int CentreCount = 2;
            /// <summary>Continent radius relative to map half-extent.</summary>
            [Range(0.1f, 1.5f)] public float CentreRadius = 0.75f;
            /// <summary>Coastline roughness (domain-warp amplitude in cells).</summary>
            [Min(0f)] public float CoastWarp = 8f;
            /// <summary>Mountain coverage strength 0..1.</summary>
            [Range(0f, 1f)] public float MountainDensity = 0.35f;
            /// <summary>Number of seeded mountain ranges.</summary>
            [Min(0)] public int MountainRangeCount = 3;
            /// <summary>Hill/roughness strength 0..1.</summary>
            [Range(0f, 1f)] public float HillDensity = 0.45f;
            /// <summary>River target density multiplier.</summary>
            [Range(0f, 3f)] public float RiverDensity = 1f;
            /// <summary>Lake probability boost 0..1.</summary>
            [Range(0f, 1f)] public float LakeDensity = 0.5f;
            /// <summary>Aridity 0..1 — pushes land biomes toward sand.</summary>
            [Range(0f, 1f)] public float Aridity = 0.25f;
            /// <summary>Forest coverage 0..1.</summary>
            [Range(0f, 1f)] public float ForestDensity = 0.5f;
            /// <summary>Snow cap presence 0..1.</summary>
            [Range(0f, 1f)] public float SnowDensity = 0.35f;
        }

        [Serializable]
        public sealed class HydrologySettings
        {
            /// <summary>Flow accumulation (cells) required for a river.</summary>
            [Min(2)] public int RiverAccumulationThreshold = 12;
            /// <summary>Minimum source elevation level for rivers.</summary>
            [Min(1)] public int RiverMinSourceLevel = 2;
            /// <summary>Min distance between river sources.</summary>
            [Min(1)] public int RiverSourceSpacing = 6;
            /// <summary>Max rivers as a fraction of map cells / 1000.</summary>
            [Range(0.01f, 10f)] public float RiversPerThousandCells = 0.35f;
            /// <summary>Ford cadence — every Nth river cell becomes passable sand.</summary>
            [Min(2)] public int FordEveryNCells = 9;
            /// <summary>Minimum depression depth (levels) to become a lake.</summary>
            [Min(0.01f)] public float LakeMinDepth = 0.15f;
            /// <summary>Max lake cells as fraction of map.</summary>
            [Range(0f, 0.2f)] public float LakeMaxFraction = 0.03f;
        }

        [Serializable]
        public sealed class CoastSettings
        {
            /// <summary>Beach half-width in cells (randomised ±1 by noise).</summary>
            [Min(0)] public int BeachWidth = 1;
            /// <summary>Chance for a coastal cell to be a cliff instead of beach when elevated.</summary>
            [Range(0f, 1f)] public float ElevatedCoastCliffBias = 0.8f;
            /// <summary>Shallow-water band width in cells.</summary>
            [Min(0)] public int ShallowBandWidth = 2;
        }

        [Serializable]
        public sealed class BiomeSettings
        {
            /// <summary>Moisture noise scale (cells per feature).</summary>
            [Min(4)] public float MoistureScale = 28f;
            /// <summary>Forest cluster noise scale.</summary>
            [Min(4)] public float ForestScale = 18f;
            /// <summary>Moisture threshold above which forests appear.</summary>
            [Range(0f, 1f)] public float ForestMoistureThreshold = 0.55f;
            /// <summary>Moisture threshold below which land turns to desert sand.</summary>
            [Range(0f, 1f)] public float DesertMoistureThreshold = 0.2f;
            /// <summary>Level at which cold/snow replaces other land.</summary>
            [Min(1)] public int SnowMinLevel = 5;
        }

        [Serializable]
        public sealed class ObjectPlacementSettings
        {
            /// <summary>Object id written into ObjectMap for lumber POIs.</summary>
            public string LumberObjectId = "resource-lumber";
            /// <summary>Object id written into ObjectMap for stone POIs.</summary>
            public string StoneObjectId = "resource-stone";
            /// <summary>POIs per 1000 land cells.</summary>
            [Range(0f, 20f)] public float PoiPerThousandLand = 2.5f;
            /// <summary>Minimum cell spacing between POIs.</summary>
            [Min(1)] public int PoiMinSpacing = 5;
        }

        [Serializable]
        public sealed class FairnessSettings
        {
            /// <summary>Radius (cells) evaluated for per-start opportunity metrics.</summary>
            [Min(2)] public int OpportunityRadius = 10;
            /// <summary>Minimum usable-land fraction inside the opportunity radius.</summary>
            [Range(0f, 1f)] public float MinUsableLandFraction = 0.35f;
            /// <summary>Max allowed parity gap (worst/best usable land ratio - 1).</summary>
            [Range(0f, 2f)] public float MaxOpportunityGap = 0.5f;
            /// <summary>Minimum separation between starts (fraction of map diagonal).</summary>
            [Range(0f, 0.75f)] public float MinSeparationFraction = 0.28f;
            /// <summary>All starts must be mutually reachable over passable terrain.</summary>
            public bool RequireMutualConnectivity = true;
            /// <summary>Minimum overall land fraction to accept a candidate world.</summary>
            [Range(0.05f, 0.9f)] public float MinLandFraction = 0.18f;
            /// <summary>Maximum overall land fraction to accept a candidate world.</summary>
            [Range(0.1f, 0.95f)] public float MaxLandFraction = 0.85f;
        }

        public ArchetypeParameters FindArchetype(string id)
        {
            if (Archetypes != null)
            {
                for (int i = 0; i < Archetypes.Length; i++)
                {
                    var candidate = Archetypes[i];
                    if (candidate != null
                        && string.Equals(candidate.Id, id, StringComparison.OrdinalIgnoreCase))
                        return candidate;
                }
            }
            return DefaultArchetypes()[0];
        }

        public static ArchetypeParameters[] DefaultArchetypes() => new[]
        {
            new ArchetypeParameters
            {
                Id = "balanced", LandRatio = 0.45f, CentreCount = 2, CentreRadius = 0.75f,
                CoastWarp = 8f, MountainDensity = 0.35f, MountainRangeCount = 3,
                HillDensity = 0.45f, RiverDensity = 1f, LakeDensity = 0.5f,
                Aridity = 0.25f, ForestDensity = 0.5f, SnowDensity = 0.35f
            },
            new ArchetypeParameters
            {
                Id = "continents", LandRatio = 0.5f, CentreCount = 3, CentreRadius = 0.6f,
                CoastWarp = 9f, MountainDensity = 0.4f, MountainRangeCount = 4,
                HillDensity = 0.5f, RiverDensity = 1.1f, LakeDensity = 0.55f,
                Aridity = 0.25f, ForestDensity = 0.5f, SnowDensity = 0.35f
            },
            new ArchetypeParameters
            {
                Id = "pangaea", LandRatio = 0.68f, CentreCount = 1, CentreRadius = 0.95f,
                CoastWarp = 10f, MountainDensity = 0.45f, MountainRangeCount = 4,
                HillDensity = 0.55f, RiverDensity = 0.9f, LakeDensity = 0.6f,
                Aridity = 0.3f, ForestDensity = 0.45f, SnowDensity = 0.3f
            },
            new ArchetypeParameters
            {
                Id = "islands", LandRatio = 0.28f, CentreCount = 10, CentreRadius = 0.28f,
                CoastWarp = 6f, MountainDensity = 0.3f, MountainRangeCount = 2,
                HillDensity = 0.4f, RiverDensity = 0.5f, LakeDensity = 0.3f,
                Aridity = 0.2f, ForestDensity = 0.55f, SnowDensity = 0.3f
            },
            new ArchetypeParameters
            {
                Id = "highlands", LandRatio = 0.6f, CentreCount = 2, CentreRadius = 0.8f,
                CoastWarp = 8f, MountainDensity = 0.65f, MountainRangeCount = 5,
                HillDensity = 0.75f, RiverDensity = 1.2f, LakeDensity = 0.6f,
                Aridity = 0.2f, ForestDensity = 0.4f, SnowDensity = 0.55f
            },
            new ArchetypeParameters
            {
                Id = "desert", LandRatio = 0.62f, CentreCount = 1, CentreRadius = 0.9f,
                CoastWarp = 7f, MountainDensity = 0.3f, MountainRangeCount = 2,
                HillDensity = 0.45f, RiverDensity = 0.35f, LakeDensity = 0.15f,
                Aridity = 0.85f, ForestDensity = 0.15f, SnowDensity = 0.05f
            },
        };
    }
}
