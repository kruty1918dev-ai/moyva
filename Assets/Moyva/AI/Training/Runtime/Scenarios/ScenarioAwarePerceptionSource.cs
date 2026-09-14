using System;
using Kruty1918.Moyva.AI.Bot;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class ScenarioAwarePerceptionSource : IBotPerceptionSource
    {
        private readonly IBotPerceptionSource _inner;
        private readonly Func<TrainingScenarioProgressTracker> _progress;
        public ScenarioAwarePerceptionSource(IBotPerceptionSource inner, Func<TrainingScenarioProgressTracker> progress)
        { _inner = inner ?? new EmptyBotPerceptionSource(); _progress = progress; }
        public BotPerceptionSnapshot Capture(string player)
        {
            var snapshot = _inner.Capture(player) ?? new BotPerceptionSnapshot();
            var p = _progress?.Invoke();
            if (p?.Scenario != null)
            {
                snapshot.Global[BotObservationSchema.ScenarioGoal] = p.Scenario.GoalCode;
                snapshot.Global[BotObservationSchema.ScenarioStep] = p.StepCount <= 1 ? 0 : p.StepIndex / (float)(p.StepCount - 1);
                snapshot.Global[BotObservationSchema.ScenarioProgress] = p.Progress;
            }
            return snapshot;
        }
    }
}
