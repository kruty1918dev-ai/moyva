using System;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Construction.API;
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
        private readonly IBuildingRegistry _buildings;
        private long _attackSequence;
        private string _actorOwner, _targetOwner;
        public event Action<TrainingRewardEvent> Reward;
        public bool IsConnected { get; private set; }
        public string Limitation => IsConnected ? null : "TERMINAL_OUTCOME_BLOCKED: match-result subscription is inactive.";
        public event Action<TrainingEpisodeResult> Completed;
        public TrainingGameplayEventBridge(SignalBus signals, ITurnHistoryQuery history, string player,
            IUnitCombatService combat = null, IUnitOwnershipQuery owners = null, long episodeId = 0,
            IBuildingRegistry buildings = null)
        {
            _signals = signals ?? throw new ArgumentNullException(nameof(signals));
            _history = history ?? throw new ArgumentNullException(nameof(history));
            _player = player;
            _combat = combat; _owners = owners; _episodeId = episodeId;
            _buildings = buildings;
            if (_combat != null && _owners != null)
            {
                _combat.AttackStarted += OnAttackStarted;
                _combat.AttackResolved += OnAttackResolved;
            }
            _signals.Subscribe<GameEndedSignal>(OnGameEnded);
            _signals.Subscribe<UnitRecruitmentDeployedSignal>(OnRecruitDeployed);
            _signals.Subscribe<BuildingOperationalSignal>(OnBuildingOperational);
            _signals.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signals.Subscribe<SettlementCapturedSignal>(OnSettlementCaptured);
            IsConnected = true;
        }
        private void OnAttackStarted(string actor, string target)
        {
            _actorOwner = _owners.GetUnitOwnerId(actor);
            _targetOwner = _owners.GetUnitOwnerId(target);
        }
        private void OnRecruitDeployed(UnitRecruitmentDeployedSignal signal)
        {
            if (signal.OwnerId != _player || string.IsNullOrEmpty(signal.UnitId)) return;
            if (signal.QueueId <= 0 || string.IsNullOrWhiteSpace(signal.UnitTypeId)) return;
            // One milestone per unit type per episode; deleting and recruiting again cannot farm reward.
            Reward?.Invoke(new TrainingRewardEvent(_episodeId, "recruit:" + signal.UnitTypeId,
                TrainingRewardEventType.UnitCreated, "unit-type:" + signal.UnitTypeId, signal.OwnerId,
                validated: true, meaningful: true));
        }
        private void OnBuildingPlaced(BuildingPlacedSignal signal)
        {
            var definition = _buildings?.GetById(signal.BuildingId);
            if (definition != null && definition.BuildTurns <= 0) RewardBuilding(signal.BuildingId, signal.OwnerId);
        }
        private void OnBuildingOperational(BuildingOperationalSignal signal) => RewardBuilding(signal.BuildingId, signal.OwnerId);
        private void RewardBuilding(string building, string owner)
        {
            if (owner != _player || string.IsNullOrEmpty(building)) return;
            // Type identity survives demolition/rebuild and also deduplicates immediate operational signals.
            Reward?.Invoke(new TrainingRewardEvent(_episodeId, "build:" + building,
                TrainingRewardEventType.BuildingCreated, "building-type:" + building, owner, validated: true, meaningful: true));
        }
        private void OnSettlementCaptured(SettlementCapturedSignal signal)
        {
            if (signal.NewOwnerId == signal.PreviousOwnerId || string.IsNullOrEmpty(signal.SettlementId)) return;
            if (signal.NewOwnerId != _player && signal.PreviousOwnerId != _player) return;
            var type = signal.NewOwnerId == _player ? TrainingRewardEventType.ObjectiveCaptured : TrainingRewardEventType.ObjectiveLost;
            Reward?.Invoke(new TrainingRewardEvent(_episodeId, type + ":" + signal.SettlementId, type,
                signal.SettlementId, signal.NewOwnerId, signal.PreviousOwnerId, true, true));
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
            _signals.TryUnsubscribe<UnitRecruitmentDeployedSignal>(OnRecruitDeployed);
            _signals.TryUnsubscribe<BuildingOperationalSignal>(OnBuildingOperational);
            _signals.TryUnsubscribe<BuildingPlacedSignal>(OnBuildingPlaced);
            _signals.TryUnsubscribe<SettlementCapturedSignal>(OnSettlementCaptured);
            if (_combat != null)
            {
                _combat.AttackStarted -= OnAttackStarted;
                _combat.AttackResolved -= OnAttackResolved;
            }
            IsConnected = false;
        }
    }
}
