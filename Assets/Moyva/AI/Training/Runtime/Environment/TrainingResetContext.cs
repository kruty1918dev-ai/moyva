namespace Kruty1918.Moyva.AI.Training
{
    public readonly struct TrainingResetContext
    {
        public readonly int EnvironmentId;
        public readonly long EpisodeId;
        public readonly int Seed;
        public readonly int WorldSize;
        public readonly TrainingCurriculumStage CurriculumStage;
        public readonly string ScenarioId;
        public readonly bool LearnInitialCastle;

        public TrainingResetContext(int environmentId, long episodeId, int seed, TrainingCurriculumStage stage,
            int worldSize = 0, string scenarioId = null, bool learnInitialCastle = false)
        {
            EnvironmentId = environmentId;
            EpisodeId = episodeId;
            Seed = seed;
            WorldSize = worldSize;
            CurriculumStage = stage;
            ScenarioId = scenarioId;
            LearnInitialCastle = learnInitialCastle;
        }

        public static int DeriveSeed(int baseSeed, int environmentId, long episodeId)
        {
            unchecked
            {
                uint hash = 2166136261;
                hash = (hash ^ (uint)baseSeed) * 16777619;
                hash = (hash ^ (uint)environmentId) * 16777619;
                hash = (hash ^ (uint)episodeId) * 16777619;
                hash = (hash ^ (uint)(episodeId >> 32)) * 16777619;
                return (int)hash;
            }
        }
    }
}
