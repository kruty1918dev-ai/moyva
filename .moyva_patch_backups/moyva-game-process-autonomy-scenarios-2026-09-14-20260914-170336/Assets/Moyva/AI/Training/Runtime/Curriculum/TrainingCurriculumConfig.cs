using System;

namespace Kruty1918.Moyva.AI.Training
{
    public enum TrainingCurriculumStage
    {
        BasicLifecycle, Movement, Combat, Recruitment, Economy, Building, Objectives, FogOfWar, FullGame
    }

    [Serializable]
    public sealed class TrainingCurriculumConfig
    {
        public TrainingCurriculumStage stage = TrainingCurriculumStage.BasicLifecycle;
    }
}
