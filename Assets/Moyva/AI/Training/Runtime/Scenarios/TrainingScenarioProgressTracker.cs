using System;
using Kruty1918.Moyva.AI.Bot;

namespace Kruty1918.Moyva.AI.Training
{
    public readonly struct TrainingScenarioFacts
    {
        public readonly int Settlements;
        public readonly int OwnedUnits;
        public readonly double Resources;
        public TrainingScenarioFacts(int settlements, int ownedUnits, double resources)
        { Settlements = settlements; OwnedUnits = ownedUnits; Resources = resources; }
    }

    public sealed class TrainingScenarioProgressTracker
    {
        private readonly TrainingScenarioDefinition _scenario;
        private long _episodeId;
        private int _stepIndex, _count, _completedTurns;
        private bool _sawAgentBuilding;
        private TrainingScenarioFacts _baseline;
        public TrainingScenarioDefinition Scenario => _scenario;
        public int StepIndex => _stepIndex;
        public int StepCount => _scenario?.steps?.Length ?? 0;
        public float Progress => StepCount == 0 ? 0 : Math.Min(1f, (_stepIndex + CurrentFraction()) / StepCount);
        public bool IsComplete => _scenario != null && _stepIndex >= StepCount;
        public TrainingScenarioStepDefinition CurrentStep => IsComplete || _scenario == null ? null : _scenario.steps[_stepIndex];

        public TrainingScenarioProgressTracker(TrainingScenarioDefinition scenario)
        { _scenario = scenario ?? throw new ArgumentNullException(nameof(scenario)); scenario.Validate(); }

        public void Begin(long episodeId, TrainingScenarioFacts baseline)
        {
            _episodeId = episodeId; _baseline = baseline; _stepIndex = _count = _completedTurns = 0; _sawAgentBuilding = false;
        }

        public void ObserveReward(TrainingRewardEvent e, TrainingScenarioFacts facts)
        {
            if (e.EpisodeId != _episodeId || !e.Validated || IsComplete) return;
            if (e.Type == TrainingRewardEventType.BuildingCreated) _sawAgentBuilding = true;
            var step = CurrentStep;
            switch (step.goal)
            {
                case TrainingScenarioGoalKind.CastleOperational:
                    if (_sawAgentBuilding && facts.Settlements > _baseline.Settlements) Advance();
                    break;
                case TrainingScenarioGoalKind.ProductionEstablished:
                    if (e.Type == TrainingRewardEventType.BuildingCreated) Count();
                    break;
                case TrainingScenarioGoalKind.UnitRecruited:
                    if (e.Type == TrainingRewardEventType.UnitCreated) Count();
                    break;
                case TrainingScenarioGoalKind.CombatSuccess:
                    if (e.Type == TrainingRewardEventType.EnemyUnitDestroyed) Count();
                    break;
                case TrainingScenarioGoalKind.ObjectiveCaptured:
                    if (e.Type == TrainingRewardEventType.ObjectiveCaptured) Count();
                    break;
            }
        }

        public void ObserveAction(BotIntentType intent, BotExecutionStatus result, TrainingScenarioFacts facts)
        {
            if (IsComplete || result != BotExecutionStatus.Completed) return;
            if (intent == BotIntentType.EndTurn) _completedTurns++;
            var step = CurrentStep;
            if (step == null) return;
            if (step.goal == TrainingScenarioGoalKind.MovementOrExploration
                && (intent == BotIntentType.Move || intent == BotIntentType.Explore || intent == BotIntentType.Reposition)) Count();
            else if (step.goal == TrainingScenarioGoalKind.StableEconomy
                && _completedTurns >= step.requiredCount && facts.Settlements > 0
                && facts.Resources >= _baseline.Resources + step.threshold) Advance();
            else if (step.goal == TrainingScenarioGoalKind.CastleOperational
                && _sawAgentBuilding && facts.Settlements > _baseline.Settlements) Advance();
        }

        public void ObserveMatch(TrainingEpisodeResult result)
        {
            if (!IsComplete && CurrentStep?.goal == TrainingScenarioGoalKind.MatchWon && result == TrainingEpisodeResult.Victory) Advance();
        }

        private void Count()
        {
            if (++_count >= CurrentStep.requiredCount) Advance();
        }
        private void Advance() { _stepIndex++; _count = 0; _completedTurns = 0; }
        private float CurrentFraction()
        {
            var step = CurrentStep;
            if (step == null) return 0;
            return Math.Min(1f, _count / (float)Math.Max(1, step.requiredCount));
        }
    }
}
