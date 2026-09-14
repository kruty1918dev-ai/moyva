using System;
using Kruty1918.Moyva.AI.Bot;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class ScenarioAwarePerceptionSource : IBotPerceptionSource
    {
        private readonly IBotPerceptionSource _inner;
        private readonly Func<TrainingScenarioProgressTracker> _progress;

        public ScenarioAwarePerceptionSource(IBotPerceptionSource inner, Func<TrainingScenarioProgressTracker> progress)
        {
            _inner = inner ?? new EmptyBotPerceptionSource();
            _progress = progress;
        }

        public BotPerceptionSnapshot Capture(string player)
        {
            var snapshot = _inner.Capture(player) ?? new BotPerceptionSnapshot();
            var progress = _progress?.Invoke();
            if (progress?.Scenario == null) return snapshot;

            snapshot.Global[BotObservationSchema.ScenarioGoal] = progress.Scenario.GoalCode;
            snapshot.Global[BotObservationSchema.ScenarioStep] = progress.StepCount <= 1
                ? 0f : progress.StepIndex / (float)(progress.StepCount - 1);
            // Setup/scaffolding is never exposed as learner progress.
            snapshot.Global[BotObservationSchema.ScenarioProgress] =
                progress.IsScoringActive ? progress.Progress : 0f;
            return snapshot;
        }
    }
}
