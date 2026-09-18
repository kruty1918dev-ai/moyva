using System;
using Kruty1918.Moyva.AI.Bot;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class ScenarioAwarePerceptionSource : IBotPerceptionSource
    {
        private readonly IBotPerceptionSource _inner;
        private readonly Func<TrainingScenarioProgressTracker> _progress;
        private readonly Func<bool> _suppressHints;

        public ScenarioAwarePerceptionSource(IBotPerceptionSource inner,
            Func<TrainingScenarioProgressTracker> progress, Func<bool> suppressHints = null)
        {
            _inner = inner ?? new EmptyBotPerceptionSource();
            _progress = progress;
            _suppressHints = suppressHints;
        }

        public BotPerceptionSnapshot Capture(string player)
        {
            var snapshot = _inner.Capture(player) ?? new BotPerceptionSnapshot();
            var progress = _progress?.Invoke();
            var scenario = progress?.Scenario;
            if (scenario == null) return snapshot;

            // FullGame parity: production inference has no scenario channel, and
            // dropout episodes must see the same blank channel the live game sends.
            if (scenario.fullGame || (_suppressHints?.Invoke() ?? false)) return snapshot;

            snapshot.Global[BotObservationSchema.ScenarioGoal] = scenario.GoalCode;
            snapshot.Global[BotObservationSchema.ScenarioStep] = progress.StepCount <= 1
                ? 0f : progress.StepIndex / (float)(progress.StepCount - 1);
            // Setup/scaffolding is never exposed as learner progress.
            snapshot.Global[BotObservationSchema.ScenarioProgress] =
                progress.IsScoringActive ? progress.Progress : 0f;
            WriteGoalVector(snapshot, scenario);
            return snapshot;
        }

        private static void WriteGoalVector(BotPerceptionSnapshot snapshot, TrainingScenarioDefinition scenario)
        {
            var steps = scenario.steps;
            if (steps == null) return;
            for (int i = 0; i < steps.Length; i++)
            {
                int slot = GoalSlot(steps[i]?.EffectiveCriterion ?? TrainingScenarioCriterionKind.None);
                if (slot >= 0) snapshot.Global[slot] = 1f;
            }
        }

        private static int GoalSlot(TrainingScenarioCriterionKind criterion)
        {
            switch (criterion)
            {
                case TrainingScenarioCriterionKind.OperationalCastle: return BotObservationSchema.GoalCastle;
                case TrainingScenarioCriterionKind.ResourceProduction: return BotObservationSchema.GoalProduction;
                case TrainingScenarioCriterionKind.StableResources: return BotObservationSchema.GoalEconomy;
                case TrainingScenarioCriterionKind.DeployedUnit: return BotObservationSchema.GoalRecruit;
                case TrainingScenarioCriterionKind.Movement: return BotObservationSchema.GoalMove;
                case TrainingScenarioCriterionKind.Scouting: return BotObservationSchema.GoalScout;
                case TrainingScenarioCriterionKind.EnemyDestroyed: return BotObservationSchema.GoalCombat;
                case TrainingScenarioCriterionKind.ObjectiveOwned: return BotObservationSchema.GoalCapture;
                case TrainingScenarioCriterionKind.MatchVictory: return BotObservationSchema.GoalWin;
                default: return -1;
            }
        }
    }
}
