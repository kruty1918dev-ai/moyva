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
        // Legacy fixed stage remains available for manual/single-scenario runs.
        public TrainingCurriculumStage stage = TrainingCurriculumStage.BasicLifecycle;
        public AutonomousTrainingConfig autonomous = new AutonomousTrainingConfig();
    }
}
