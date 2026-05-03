using UnityEngine;

namespace Kruty1918.Moyva.WorldCreation.API
{
    [CreateAssetMenu(fileName = "WorldCreationDefaults", menuName = "Moyva/World Creation/Defaults")]
    public sealed class WorldCreationDefaultsSO : ScriptableObject
    {
        public string DefaultWorldName = "Novyi svit";
        public int DefaultSeed = 0;
        public int DefaultSizePreset = 1;
        public int DefaultCustomWidth = 64;
        public int DefaultCustomHeight = 64;
        public MapType DefaultMapType = MapType.Continents;
        public Difficulty DefaultDifficulty = Difficulty.Normal;
        public bool DefaultEnableBots = true;
        public int DefaultHumanPlayerCount = 1;
        public int DefaultBotCount = 1;
        public int DefaultStartingGold = 200;
        public int DefaultStartingFood = 100;
        public float DefaultForestDensity = 0.4f;
        public float DefaultMountainDensity = 0.3f;
        public float DefaultWaterDensity = 0.25f;
        public float DefaultVillageDensity = 0.2f;
        public bool DefaultGenerateRivers = true;
        public bool DefaultGenerateBiomes = true;
        public bool DefaultApplyWFC = true;
    }
}