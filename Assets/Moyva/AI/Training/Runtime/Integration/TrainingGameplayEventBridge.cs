using System;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using Zenject;

namespace Kruty1918.Moyva.AI.Training
{
    // Subscribe only to the owned scope's bus. No scene polling or inferred kills.
    public sealed class TrainingGameplayEventBridge : ITrainingEpisodeOutcomeSource, IDisposable
    {
        private readonly SignalBus _signals;
        private readonly ITurnHistoryQuery _history;
        private readonly string _player;
        private readonly IUnitCombatService _combat;
        private readonly IUnitOwnershipQuery _owners;
        private readonly long _episodeId;
        private long _attackSequence;
        private string _actorOwner, _targetOwner;
        public event Action<TrainingRewardEvent> Reward;
        public bool IsConnected { get; private set; }
        public string Limitation => IsConnected ? null : "TERMINAL_OUTCOME_BLOCKED: match-result subscription is inactive.";
        public event Action<TrainingEpisodeResult> Completed;
        public TrainingGameplayEventBridge(SignalBus signals, ITurnHistoryQuery history, string player,
            IUnitCombatService combat = null, IUnitOwnershipQuery owners = null, long episodeId = 0)
        {
            _signals = signals ?? throw new ArgumentNullException(nameof(signals));
            _history = history ?? throw new ArgumentNullException(nameof(history));
            _player = player;
            _combat = combat; _owners = owners; _episodeId = episodeId;
            if (_combat != null && _owners != null)
            {
                _combat.AttackStarted += OnAttackStarted;
                _combat.AttackResolved += OnAttackResolved;
            }
            _signals.Subscribe<GameEndedSignal>(OnGameEnded);
            IsConnected = true;
        }
        private void OnAttackStarted(string actor, string target)
        {
            _actorOwner = _owners.GetUnitOwnerId(actor);
            _targetOwner = _owners.GetUnitOwnerId(target);
        }
        private void OnAttackResolved(UnitAttackResult result)
        {
            if (!result.Succeeded || !result.TargetDied || string.IsNullOrWhiteSpace(_targetOwner)) return;
            var type = _targetOwner == _player ? TrainingRewardEventType.OwnUnitLost : TrainingRewardEventType.EnemyUnitDestroyed;
            if (type == TrainingRewardEventType.EnemyUnitDestroyed && _actorOwner != _player) return;
            Reward?.Invoke(new TrainingRewardEvent(_episodeId, "combat:" + ++_attackSequence, type,
                result.TargetUnitId, _actorOwner, _targetOwner, true, true));
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
            if (_combat != null)
            {
                _combat.AttackStarted -= OnAttackStarted;
                _combat.AttackResolved -= OnAttackResolved;
            }
            IsConnected = false;
        }
    }
}
