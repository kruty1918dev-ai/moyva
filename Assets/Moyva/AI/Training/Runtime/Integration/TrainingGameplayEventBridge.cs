using System;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Zenject;

namespace Kruty1918.Moyva.AI.Training
{
    // Subscribe only to the owned scope's bus. No scene polling or inferred kills.
    public sealed class TrainingGameplayEventBridge : ITrainingEpisodeOutcomeSource, IDisposable
    {
        private readonly SignalBus _signals;
        private readonly ITurnHistoryQuery _history;
        private readonly string _player;
        public bool IsConnected { get; private set; }
        public string Limitation => IsConnected ? null : "TERMINAL_OUTCOME_BLOCKED: match-result subscription is inactive.";
        public event Action<TrainingEpisodeResult> Completed;
        public TrainingGameplayEventBridge(SignalBus signals, ITurnHistoryQuery history, string player)
        {
            _signals = signals ?? throw new ArgumentNullException(nameof(signals));
            _history = history ?? throw new ArgumentNullException(nameof(history));
            _player = player;
            _signals.Subscribe<GameEndedSignal>(OnGameEnded);
            IsConnected = true;
        }
        private void OnGameEnded(GameEndedSignal signal)
        {
            if (!string.IsNullOrWhiteSpace(signal.WinnerId))
            {
                Completed?.Invoke(signal.WinnerId == _player ? TrainingEpisodeResult.Victory : TrainingEpisodeResult.Defeat);
                return;
            }
            // Empty winner also means cancellation. Only zero surviving participants
            // matches MatchEndConditionService's authoritative draw condition.
            var participants = _history.GetParticipantHistory();
            bool draw = participants.Count > 1;
            foreach (var participant in participants)
                if (!participant.IsEliminated && !string.IsNullOrWhiteSpace(participant.OwnerId)) draw = false;
            Completed?.Invoke(draw ? TrainingEpisodeResult.Draw : TrainingEpisodeResult.InvalidState);
        }
        public void Dispose()
        {
            if (!IsConnected) return;
            _signals.TryUnsubscribe<GameEndedSignal>(OnGameEnded);
            IsConnected = false;
        }
    }
}
