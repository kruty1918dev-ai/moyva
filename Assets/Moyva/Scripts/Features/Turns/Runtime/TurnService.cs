using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Calendar.Core;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Turns.Runtime
{
    internal sealed class TurnService : ITurnService, ITurnStateRestorer, IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IWorldGenerationSignalState _worldState;
        private readonly ICalendarService _calendar;
        private readonly List<ITurnParticipant> _participants;
        private readonly List<ITurnBlocker> _blockers;
        private readonly List<TurnFaction> _factions = new();
        private int _activeFactionIndex;
        private bool _worldReady;

        public TurnService(
            SignalBus signalBus,
            IWorldGenerationSignalState worldState,
            ICalendarService calendar,
            [InjectOptional] List<ITurnParticipant> participants = null,
            [InjectOptional] List<ITurnBlocker> blockers = null)
        {
            _signalBus = signalBus;
            _worldState = worldState;
            _calendar = calendar;
            _participants = participants ?? new List<ITurnParticipant>();
            _blockers = blockers ?? new List<ITurnBlocker>();
            _participants.Sort((left, right) => left.TurnOrder.CompareTo(right.TurnOrder));
        }

        public event Action StateChanged;
        public TurnPhase Phase { get; private set; } = TurnPhase.Initializing;
        public int Round { get; private set; } = 1;
        public long GlobalTurn { get; private set; } = 1;
        public int ActionsThisTurn { get; private set; }
        public string ActiveOwnerId => _factions.Count == 0 ? string.Empty : _factions[_activeFactionIndex].OwnerId;
        public string LocalOwnerId { get; private set; } = "player_0";
        public bool IsActiveFactionBot => _factions.Count > 0 && _factions[_activeFactionIndex].IsBot;
        public IReadOnlyList<TurnFaction> Factions => _factions;

        public void Initialize()
        {
            _signalBus.Subscribe<WorldSpawnPositionsSignal>(OnSpawnPositions);
            _signalBus.Subscribe<WorldBuiltSignal>(OnWorldBuilt);
            if (_worldState.TryGetWorldSpawnPositions(out WorldSpawnPositionsSignal cached))
                ConfigureFactions(cached.Assignments);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldSpawnPositionsSignal>(OnSpawnPositions);
            _signalBus.TryUnsubscribe<WorldBuiltSignal>(OnWorldBuilt);
        }

        public bool IsOwnerActive(string ownerId)
            => !string.IsNullOrWhiteSpace(ownerId)
               && string.Equals(ownerId.Trim(), ActiveOwnerId, StringComparison.Ordinal);

        public bool CanOwnerAct(string ownerId, out string reason)
        {
            if (!_worldReady || Phase != TurnPhase.AwaitingInput)
            {
                reason = $"Хід недоступний у фазі {Phase}.";
                return false;
            }

            if (!IsOwnerActive(ownerId))
            {
                reason = $"Зараз хід фракції '{ActiveOwnerId}'.";
                return false;
            }

            reason = null;
            return true;
        }

        public bool TryRecordAction(string ownerId, string actionId)
        {
            if (!CanOwnerAct(ownerId, out _))
                return false;

            ActionsThisTurn++;
            Debug.Log($"[Turns] action turn={GlobalTurn} owner='{ownerId}' id='{actionId}' count={ActionsThisTurn}.");
            StateChanged?.Invoke();
            return true;
        }

        public bool TryEndTurn(string requesterOwnerId, out string reason)
        {
            if (!CanOwnerAct(requesterOwnerId, out reason))
                return false;

            for (int index = 0; index < _blockers.Count; index++)
            {
                if (_blockers[index] != null && _blockers[index].IsTurnBlocked(out reason))
                    return false;
            }

            EndCurrentTurn();
            reason = null;
            return true;
        }

        public void Restore(int round, long globalTurn, string activeOwnerId, int actions)
        {
            Round = Mathf.Max(1, round);
            GlobalTurn = Math.Max(1, globalTurn);
            ActionsThisTurn = Mathf.Max(0, actions);
            int restoredIndex = _factions.FindIndex(f => string.Equals(f.OwnerId, activeOwnerId, StringComparison.Ordinal));
            if (restoredIndex >= 0)
                _activeFactionIndex = restoredIndex;
            StateChanged?.Invoke();
        }

        private void OnSpawnPositions(WorldSpawnPositionsSignal signal)
        {
            ConfigureFactions(signal.Assignments);
            TryStart();
        }

        private void OnWorldBuilt(WorldBuiltSignal _)
        {
            _worldReady = true;
            if (_factions.Count == 0 && _worldState.TryGetWorldSpawnPositions(out WorldSpawnPositionsSignal cached))
                ConfigureFactions(cached.Assignments);
            if (_factions.Count == 0)
                ConfigureFactions(new[] { new SpawnPositionAssignment { SlotIndex = 0, ParticipantId = "player_0", IsBot = false } });
            TryStart();
        }

        private void ConfigureFactions(SpawnPositionAssignment[] assignments)
        {
            if (assignments == null || assignments.Length == 0)
                return;

            Array.Sort(assignments, (left, right) => left.SlotIndex.CompareTo(right.SlotIndex));
            _factions.Clear();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < assignments.Length; index++)
            {
                string ownerId = NormalizeOwner(assignments[index].ParticipantId, assignments[index].IsBot, index);
                if (seen.Add(ownerId))
                    _factions.Add(new TurnFaction(ownerId, assignments[index].IsBot, assignments[index].Position));
            }

            TurnFaction local = _factions.Find(f => !f.IsBot);
            LocalOwnerId = string.IsNullOrWhiteSpace(local.OwnerId) ? _factions[0].OwnerId : local.OwnerId;
            _activeFactionIndex = Mathf.Clamp(_activeFactionIndex, 0, _factions.Count - 1);
            StateChanged?.Invoke();
        }

        private void TryStart()
        {
            if (!_worldReady || _factions.Count == 0 || Phase != TurnPhase.Initializing)
                return;
            StartCurrentTurn();
        }

        private void StartCurrentTurn()
        {
            Phase = TurnPhase.Starting;
            ActionsThisTurn = 0;
            StateChanged?.Invoke();
            TurnContext context = CurrentContext();
            for (int index = 0; index < _participants.Count; index++)
                _participants[index]?.OnTurnStarted(context);
            Phase = TurnPhase.AwaitingInput;
            Debug.Log($"[Turns] started round={Round} global={GlobalTurn} owner='{ActiveOwnerId}' bot={IsActiveFactionBot}.");
            StateChanged?.Invoke();
        }

        private void EndCurrentTurn()
        {
            Phase = TurnPhase.Ending;
            StateChanged?.Invoke();
            TurnContext context = CurrentContext();
            for (int index = _participants.Count - 1; index >= 0; index--)
                _participants[index]?.OnTurnEnding(context);

            bool completedRound = _activeFactionIndex >= _factions.Count - 1;
            if (completedRound)
            {
                int completedRoundNumber = Round;
                Phase = TurnPhase.Resolving;
                _calendar.AdvanceTurn();
                for (int index = 0; index < _participants.Count; index++)
                    _participants[index]?.OnRoundCompleted(completedRoundNumber);
                Round++;
                _activeFactionIndex = 0;
            }
            else
            {
                _activeFactionIndex++;
            }

            GlobalTurn++;
            StartCurrentTurn();
        }

        private TurnContext CurrentContext()
            => new(Round, GlobalTurn, _activeFactionIndex, _factions[_activeFactionIndex]);

        private static string NormalizeOwner(string raw, bool isBot, int index)
            => string.IsNullOrWhiteSpace(raw) ? (isBot ? $"bot_{index}" : $"player_{index}") : raw.Trim();
    }
}
