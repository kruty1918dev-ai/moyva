using System;
using Kruty1918.Moyva.AI.Bot;
using Kruty1918.Moyva.Turns.API;

namespace Kruty1918.Moyva.AI.Training
{
    // Stable training handle, fresh owned gameplay composition for every episode.
    public sealed class TrainingGameplayScope : IGameplayTrainingReset, ITrainingEpisodeOutcomeSource, IDisposable
    {
        private readonly TrainingConfig _config;
        private GameplayTrainingEpisode _episode;
        public const string LearnerId = "training_player";
        public const string OpponentId = "training_opponent";
        public string PlayerId => LearnerId;
        internal GameplayTrainingEpisode Episode => _episode;
        public ITurnService Turns => _episode?.Turns;
        public IBotTurnGateway TurnGateway => _episode?.Gateway;
        public BotCapabilityRegistry Capabilities => _episode?.Capabilities;
        public IBotPerceptionSource Perception => _episode?.Perception;
        public IGameplayTrainingReset Reset => this;
        public ITrainingEpisodeOutcomeSource Outcomes => this;
        public bool IsConnected => _episode?.Outcomes.IsConnected == true;
        public string Limitation => IsConnected ? null : "Episode has not been initialized.";
        public event Action<TrainingEpisodeResult> Completed;
        public event Action<TrainingRewardEvent> Reward;
        public TrainingGameplayScope(TrainingConfig config) { _config = config; }
        bool IGameplayTrainingReset.Reset(TrainingResetContext context)
        {
            DisposeEpisode();
            try
            {
                _episode = new GameplayTrainingEpisode(_config, context);
                _episode.Outcomes.Completed += ForwardOutcome;
                _episode.Outcomes.Reward += ForwardReward;
                return Turns.CanOwnerAct(PlayerId, out _);
            }
            catch { DisposeEpisode(); throw; }
        }
        public void Tick(float seconds) => _episode?.Tick(seconds);
        private void ForwardOutcome(TrainingEpisodeResult result) => Completed?.Invoke(result);
        private void ForwardReward(TrainingRewardEvent reward) => Reward?.Invoke(reward);
        private void DisposeEpisode()
        {
            if (_episode == null) return;
            _episode.Outcomes.Completed -= ForwardOutcome;
            _episode.Outcomes.Reward -= ForwardReward;
            _episode.Dispose();
            _episode = null;
        }
        public void Dispose() => DisposeEpisode();
    }
}
