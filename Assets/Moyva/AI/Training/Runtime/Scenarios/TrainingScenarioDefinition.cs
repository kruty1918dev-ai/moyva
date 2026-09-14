using System;

namespace Kruty1918.Moyva.AI.Training
{
    public enum TrainingScenarioGoalKind
    {
        None = 0,
        CastleOperational = 1,
        ProductionEstablished = 2,
        StableEconomy = 3,
        UnitRecruited = 4,
        MovementOrExploration = 5,
        CombatSuccess = 6,
        ObjectiveCaptured = 7,
        MatchWon = 8
    }

    [Serializable]
    public sealed class TrainingScenarioStepDefinition
    {
        public string id;
        public TrainingScenarioGoalKind goal;
        public int requiredCount = 1;
        public float threshold;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Scenario step id is required.");
            if (goal == TrainingScenarioGoalKind.None) throw new ArgumentException("Scenario step goal is required: " + id);
            if (requiredCount < 1) throw new ArgumentException("Scenario step requiredCount must be positive: " + id);
            if (float.IsNaN(threshold) || float.IsInfinity(threshold) || threshold < 0)
                throw new ArgumentException("Scenario step threshold is invalid: " + id);
        }
    }

    [Serializable]
    public sealed class TrainingScenarioDefinition
    {
        public string id;
        public string title;
        public TrainingCurriculumStage legacyStage = TrainingCurriculumStage.FullGame;
        public bool learnerBuildsInitialCastle;
        public bool fullGame;
        public string[] prerequisites = Array.Empty<string>();
        public TrainingScenarioStepDefinition[] steps = Array.Empty<TrainingScenarioStepDefinition>();

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Scenario id is required.");
            if (!Enum.IsDefined(typeof(TrainingCurriculumStage), legacyStage))
                throw new ArgumentException("Scenario has invalid legacy stage: " + id);
            if (steps == null || steps.Length == 0) throw new ArgumentException("Scenario must have at least one step: " + id);
            var seen = new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
            foreach (var step in steps)
            {
                if (step == null) throw new ArgumentException("Scenario contains a null step: " + id);
                step.Validate();
                if (!seen.Add(step.id)) throw new ArgumentException("Duplicate scenario step id: " + step.id);
            }
        }

        public float GoalCode => Stable01(id);

        private static float Stable01(string value)
        {
            unchecked
            {
                uint hash = 2166136261;
                if (!string.IsNullOrEmpty(value)) foreach (char c in value) hash = (hash ^ c) * 16777619;
                return (hash & 0xffff) / 65535f;
            }
        }
    }
}
