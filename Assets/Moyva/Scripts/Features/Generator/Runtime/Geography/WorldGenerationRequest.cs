using Kruty1918.Moyva.Generator.API;

namespace Kruty1918.Moyva.Generator.Runtime.Geography
{
    /// <summary>Immutable inputs for one world-generation attempt.</summary>
    internal sealed class WorldGenerationRequest
    {
        public WorldGenerationRequest(
            int seed,
            int width,
            int height,
            WorldArchetype archetype,
            int playerCount,
            WorldGenerationConfig config,
            int waterLevel,
            int shoreLevel,
            int landLevel,
            int hillLevel,
            int maxLevel,
            float heightStep,
            float waterSurfaceOffset)
        {
            Seed = seed;
            Width = width;
            Height = height;
            Archetype = archetype;
            PlayerCount = playerCount;
            Config = config;
            WaterLevel = waterLevel;
            ShoreLevel = shoreLevel;
            LandLevel = landLevel;
            HillLevel = hillLevel;
            MaxLevel = maxLevel;
            HeightStep = heightStep;
            WaterSurfaceOffset = waterSurfaceOffset;
        }

        public int Seed { get; }
        public int Width { get; }
        public int Height { get; }
        public WorldArchetype Archetype { get; }
        public int PlayerCount { get; }
        public WorldGenerationConfig Config { get; }
        public int WaterLevel { get; }
        public int ShoreLevel { get; }
        public int LandLevel { get; }
        public int HillLevel { get; }
        public int MaxLevel { get; }
        public float HeightStep { get; }
        public float WaterSurfaceOffset { get; }
    }

    /// <summary>Macro geography archetypes produced by the engine.</summary>
    internal enum WorldArchetype
    {
        Balanced = 0,
        Continents = 1,
        Pangaea = 2,
        Islands = 3,
        Highlands = 4,
        Desert = 5,
    }

    internal static class WorldArchetypeResolver
    {
        /// <summary>MapType (Features/WorldCreation) → engine archetype. Random picks by seed.</summary>
        internal static WorldArchetype FromMapType(int mapType, int seed)
        {
            return mapType switch
            {
                0 => WorldArchetype.Continents,
                1 => WorldArchetype.Pangaea,
                2 => WorldArchetype.Islands,
                3 => WorldArchetype.Highlands,
                4 => WorldArchetype.Desert,
                5 => (WorldArchetype)(1 + DeterministicNoise.HashInt(seed, 991, 17, 7) % 5),
                _ => WorldArchetype.Balanced,
            };
        }

        /// <summary>MapTypePreset fallback when no MapType is supplied.</summary>
        internal static WorldArchetype FromPreset(int preset)
        {
            return preset switch
            {
                1 => WorldArchetype.Continents,
                2 => WorldArchetype.Islands,
                3 => WorldArchetype.Highlands,
                4 => WorldArchetype.Balanced,
                _ => WorldArchetype.Balanced,
            };
        }

        internal static string IdOf(WorldArchetype archetype) => archetype switch
        {
            WorldArchetype.Continents => "continents",
            WorldArchetype.Pangaea => "pangaea",
            WorldArchetype.Islands => "islands",
            WorldArchetype.Highlands => "highlands",
            WorldArchetype.Desert => "desert",
            _ => "balanced",
        };
    }
}
