using Kruty1918.Moyva.AI.Bot;
using Kruty1918.Moyva.Turns.API;

namespace Kruty1918.Moyva.AI.Training
{
    public sealed class GameplayTrainingSimulation : ITrainingSimulation, ITrainingBotRuntimeSource
    {
        private readonly TrainingGameplayScope _scope;
        private bool _disposed;
        public GameplayTrainingSimulation(TrainingGameplayScope scope) { _scope = scope; }
        public string PlayerId => _scope.PlayerId;
        public ITurnService Turns => _scope.Turns;
        public IBotTurnGateway TurnGateway => _scope.TurnGateway;
        public BotCapabilityRegistry Capabilities => _scope.Capabilities;
        public IBotPerceptionSource Perception => _scope.Perception;
        public ITrainingEpisodeOutcomeSource Outcomes => _scope.Outcomes;
        public event System.Action<TrainingRewardEvent> GameplayReward
        {
            add => _scope.Reward += value;
            remove => _scope.Reward -= value;
        }
        internal GameplayTrainingEpisode Episode => _scope.Episode;
        public bool IsReady { get; private set; }
        public string Limitation => Outcomes == null || !Outcomes.IsConnected
            ? "TERMINAL_OUTCOME_BLOCKED: connect authoritative match results in the owned gameplay scope." : null;
        public bool Reset(TrainingResetContext context)
        {
            IsReady = false;
            if (_disposed || context.EnvironmentId != 0) return false;
            IsReady = _scope.Reset.Reset(context);
            return IsReady;
        }
        public bool CanEndTurn() => IsReady && TurnGateway.CanEndTurn(PlayerId, out _);
        public void Tick(float seconds) { if (IsReady) _scope.Tick(seconds); }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            IsReady = false;
            _scope.Dispose();
        }
    }
}
