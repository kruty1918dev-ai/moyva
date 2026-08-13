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
        private readonly ITurnLocalOwnerResolver _localOwnerResolver;
        private readonly List<TurnFaction> _factions = new();
        private readonly HashSet<string> _eliminatedOwners = new(StringComparer.Ordinal);
        private int _activeFactionIndex;
        private bool _worldReady;
        private WorldSpawnPositionsSource _lastSpawnSource = WorldSpawnPositionsSource.Unknown;
        private PendingTurnRestore _pendingRestore;
        private bool _hasPendingRestore;

        public TurnService(
            SignalBus signalBus,
            IWorldGenerationSignalState worldState,
            ICalendarService calendar,
            [InjectOptional] List<ITurnParticipant> participants = null,
            [InjectOptional] List<ITurnBlocker> blockers = null,
            [InjectOptional] ITurnLocalOwnerResolver localOwnerResolver = null)
        {
            _signalBus = signalBus;
            _worldState = worldState;
            _calendar = calendar;
            _participants = participants ?? new List<ITurnParticipant>();
            _blockers = blockers ?? new List<ITurnBlocker>();
            _localOwnerResolver = localOwnerResolver;
            _participants.Sort((left, right) => left.TurnOrder.CompareTo(right.TurnOrder));
        }

        public event Action StateChanged;
        public TurnPhase Phase { get; private set; } = TurnPhase.Initializing;
        public int Round { get; private set; } = 1;
        public long GlobalTurn { get; private set; } = 1;
        public int ActionsThisTurn { get; private set; }
        public string ActiveOwnerId => _factions.Count == 0 ? string.Empty : _factions[_activeFactionIndex].OwnerId;
        public string LocalOwnerId { get; private set; } = string.Empty;
        public bool IsActiveFactionBot => _factions.Count > 0 && _factions[_activeFactionIndex].IsBot;
        public IReadOnlyList<TurnFaction> Factions => _factions;

        public void Initialize()
        {
            _signalBus.Subscribe<WorldSpawnPositionsSignal>(OnSpawnPositions);
            _signalBus.Subscribe<WorldBuiltSignal>(OnWorldBuilt);
            _signalBus.Subscribe<FactionEliminatedSignal>(OnFactionEliminated);
            if (_worldState.TryGetWorldSpawnPositions(out WorldSpawnPositionsSignal cached))
                ConfigureFactions(cached.Assignments, cached.Source);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldSpawnPositionsSignal>(OnSpawnPositions);
            _signalBus.TryUnsubscribe<WorldBuiltSignal>(OnWorldBuilt);
            _signalBus.TryUnsubscribe<FactionEliminatedSignal>(OnFactionEliminated);
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

            if (_eliminatedOwners.Contains(ActiveOwnerId))
            {
                reason = $"Фракція '{ActiveOwnerId}' вибула з гри.";
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
            _pendingRestore = new PendingTurnRestore(
                round,
                globalTurn,
                activeOwnerId?.Trim() ?? string.Empty,
                actions);
            _hasPendingRestore = true;

            // Loading may happen before spawn assignments/world readiness, or while an existing
            // session is already AwaitingInput. In both cases stop the live turn and let TryStart
            // resume the persisted snapshot only after its owner can be resolved.
            if (Phase != TurnPhase.Initializing)
            {
                Phase = TurnPhase.Initializing;
                StateChanged?.Invoke();
            }

            Debug.Log(
                $"[Turns] restore queued round={_pendingRestore.Round} global={_pendingRestore.GlobalTurn} " +
                $"owner='{_pendingRestore.ActiveOwnerId}' actions={_pendingRestore.Actions}.");
            TryStart();
        }

        private void OnSpawnPositions(WorldSpawnPositionsSignal signal)
        {
            ConfigureFactions(signal.Assignments, signal.Source);
            TryStart();
        }

        private void OnWorldBuilt(WorldBuiltSignal _)
        {
            _worldReady = true;
            if (_factions.Count == 0 && _worldState.TryGetWorldSpawnPositions(out WorldSpawnPositionsSignal cached))
                ConfigureFactions(cached.Assignments, cached.Source);
            if (_factions.Count == 0 && _lastSpawnSource == WorldSpawnPositionsSource.DirectGameplayTest)
                ConfigureDirectGameplayFallback();
            TryStart();
        }

        private void OnFactionEliminated(FactionEliminatedSignal signal)
        {
            string ownerId = signal.FactionId?.Trim();
            if (string.IsNullOrWhiteSpace(ownerId) || !_eliminatedOwners.Add(ownerId))
                return;

            bool configured = _factions.FindIndex(
                f => string.Equals(f.OwnerId, ownerId, StringComparison.Ordinal)) >= 0;
            if (!configured)
            {
                Debug.Log($"[Turns] faction elimination queued before registry configuration owner='{ownerId}'.");
                return;
            }

            Debug.Log($"[Turns] faction eliminated owner='{ownerId}'.");
            StateChanged?.Invoke();

            if (_worldReady && Phase == TurnPhase.AwaitingInput && IsOwnerActive(ownerId))
            {
                Debug.Log($"[Turns] active faction '{ownerId}' eliminated; advancing without player input.");
                EndCurrentTurn();
            }
        }

        private void ConfigureFactions(SpawnPositionAssignment[] assignments, WorldSpawnPositionsSource source)
        {
            _lastSpawnSource = source;
            if (assignments == null || assignments.Length == 0)
            {
                if (_worldReady && source == WorldSpawnPositionsSource.DirectGameplayTest && _factions.Count == 0)
                    ConfigureDirectGameplayFallback();
                return;
            }

            string previousActiveOwner = ActiveOwnerId;
            var ordered = (SpawnPositionAssignment[])assignments.Clone();
            Array.Sort(ordered, CompareAssignments);

            _factions.Clear();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < ordered.Length; index++)
            {
                SpawnPositionAssignment assignment = ordered[index];
                string ownerId = NormalizeOwner(assignment.ParticipantId, assignment.IsBot, assignment.SlotIndex);
                if (seen.Add(ownerId))
                    _factions.Add(new TurnFaction(ownerId, assignment.IsBot, assignment.Position));
            }

            _eliminatedOwners.RemoveWhere(ownerId => !seen.Contains(ownerId));
            LocalOwnerId = ResolveLocalOwnerId();

            int preservedIndex = string.IsNullOrWhiteSpace(previousActiveOwner)
                ? -1
                : _factions.FindIndex(f => string.Equals(f.OwnerId, previousActiveOwner, StringComparison.Ordinal));
            if (preservedIndex >= 0)
                _activeFactionIndex = preservedIndex;
            else if (!TrySelectFirstEligibleFaction())
                _activeFactionIndex = 0;

            StateChanged?.Invoke();
        }

        private void ConfigureDirectGameplayFallback()
        {
            if (_factions.Count != 0)
                return;

            _factions.Add(new TurnFaction("player_0", false, Vector2Int.zero));
            LocalOwnerId = ResolveLocalOwnerId();
            _activeFactionIndex = 0;
            Debug.LogWarning("[Turns] DirectGameplayTest has no spawn assignments; using explicit solo player_0 fallback.");
            StateChanged?.Invoke();
        }

        private string ResolveLocalOwnerId()
        {
            if (_factions.Count == 0)
                return string.Empty;

            if (_localOwnerResolver != null)
            {
                string resolved = _localOwnerResolver.ResolveLocalOwnerId(_factions)?.Trim();
                if (string.IsNullOrWhiteSpace(resolved))
                    return string.Empty;

                int match = _factions.FindIndex(f => string.Equals(f.OwnerId, resolved, StringComparison.Ordinal));
                return match >= 0 ? _factions[match].OwnerId : string.Empty;
            }

            for (int index = 0; index < _factions.Count; index++)
            {
                if (!_factions[index].IsBot)
                    return _factions[index].OwnerId;
            }

            return _factions[0].OwnerId;
        }

        private void TryStart()
        {
            if (!_worldReady || _factions.Count == 0 || Phase != TurnPhase.Initializing)
                return;

            if (_hasPendingRestore)
            {
                if (!TryApplyPendingRestore())
                    return;

                ResumeRestoredTurn();
                return;
            }

            if (_eliminatedOwners.Contains(ActiveOwnerId) && !TrySelectFirstEligibleFaction())
            {
                Debug.LogWarning("[Turns] Cannot start: every configured faction is eliminated.");
                return;
            }

            StartCurrentTurn();
        }

        private bool TryApplyPendingRestore()
        {
            if (_pendingRestore.Round < 1)
            {
                Debug.LogError($"[Turns] Cannot resume saved turn: round {_pendingRestore.Round} is invalid.");
                return false;
            }

            if (_pendingRestore.GlobalTurn < 1)
            {
                Debug.LogError($"[Turns] Cannot resume saved turn: global turn {_pendingRestore.GlobalTurn} is invalid.");
                return false;
            }

            if (_pendingRestore.Actions < 0)
            {
                Debug.LogError($"[Turns] Cannot resume saved turn: action count {_pendingRestore.Actions} is invalid.");
                return false;
            }

            string ownerId = _pendingRestore.ActiveOwnerId;
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                Debug.LogError("[Turns] Cannot resume saved turn: active owner id is empty.");
                return false;
            }

            int restoredIndex = _factions.FindIndex(
                faction => string.Equals(faction.OwnerId, ownerId, StringComparison.Ordinal));
            if (restoredIndex < 0)
            {
                Debug.LogWarning(
                    $"[Turns] Saved owner '{ownerId}' is not in the current faction registry; restore remains pending.");
                return false;
            }

            if (_eliminatedOwners.Contains(ownerId))
            {
                Debug.LogError(
                    $"[Turns] Cannot resume saved turn: owner '{ownerId}' is already eliminated.");
                return false;
            }

            Round = _pendingRestore.Round;
            GlobalTurn = _pendingRestore.GlobalTurn;
            ActionsThisTurn = _pendingRestore.Actions;
            _activeFactionIndex = restoredIndex;
            _hasPendingRestore = false;
            return true;
        }

        private void ResumeRestoredTurn()
        {
            // Deliberately do not call ITurnParticipant.OnTurnStarted here. Unit stamina,
            // construction/recruitment counters and other participant state are restored by
            // their own save modules and must not receive a second turn-start side effect.
            Phase = TurnPhase.AwaitingInput;
            Debug.Log(
                $"[Turns] resumed round={Round} global={GlobalTurn} owner='{ActiveOwnerId}' " +
                $"bot={IsActiveFactionBot} actions={ActionsThisTurn}.");
            StateChanged?.Invoke();
        }

        private void StartCurrentTurn()
        {
            Phase = TurnPhase.Starting;
            ActionsThisTurn = 0;
            StateChanged?.Invoke();
            TurnContext context = CurrentContext();
            for (int index = 0; index < _participants.Count; index++)
                _participants[index]?.OnTurnStarted(context);

            if (_eliminatedOwners.Contains(ActiveOwnerId))
            {
                Debug.Log($"[Turns] faction '{ActiveOwnerId}' was eliminated while its turn was starting; advancing.");
                EndCurrentTurn();
                return;
            }

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

            if (!TryFindNextEligibleFaction(out int nextIndex, out bool completedRound))
            {
                Phase = TurnPhase.Resolving;
                Debug.LogWarning("[Turns] No eligible faction remains; turn loop is awaiting game-over resolution.");
                StateChanged?.Invoke();
                return;
            }

            if (completedRound)
            {
                int completedRoundNumber = Round;
                Phase = TurnPhase.Resolving;
                _calendar.AdvanceTurn();
                for (int index = 0; index < _participants.Count; index++)
                    _participants[index]?.OnRoundCompleted(completedRoundNumber);
                Round++;
            }

            _activeFactionIndex = nextIndex;
            GlobalTurn++;
            StartCurrentTurn();
        }

        private bool TryFindNextEligibleFaction(out int nextIndex, out bool completedRound)
        {
            completedRound = false;
            nextIndex = -1;
            if (_factions.Count == 0)
                return false;

            for (int step = 1; step <= _factions.Count; step++)
            {
                int rawIndex = _activeFactionIndex + step;
                if (rawIndex >= _factions.Count)
                    completedRound = true;

                int candidate = rawIndex % _factions.Count;
                if (_eliminatedOwners.Contains(_factions[candidate].OwnerId))
                    continue;

                nextIndex = candidate;
                return true;
            }

            completedRound = false;
            return false;
        }

        private bool TrySelectFirstEligibleFaction()
        {
            for (int index = 0; index < _factions.Count; index++)
            {
                if (_eliminatedOwners.Contains(_factions[index].OwnerId))
                    continue;

                _activeFactionIndex = index;
                return true;
            }

            return false;
        }

        private TurnContext CurrentContext()
            => new(Round, GlobalTurn, _activeFactionIndex, _factions[_activeFactionIndex]);

        private readonly struct PendingTurnRestore
        {
            public PendingTurnRestore(int round, long globalTurn, string activeOwnerId, int actions)
            {
                Round = round;
                GlobalTurn = globalTurn;
                ActiveOwnerId = activeOwnerId ?? string.Empty;
                Actions = actions;
            }

            public int Round { get; }
            public long GlobalTurn { get; }
            public string ActiveOwnerId { get; }
            public int Actions { get; }
        }

        private static int CompareAssignments(SpawnPositionAssignment left, SpawnPositionAssignment right)
        {
            int comparison = left.SlotIndex.CompareTo(right.SlotIndex);
            if (comparison != 0)
                return comparison;

            string leftId = left.ParticipantId?.Trim() ?? string.Empty;
            string rightId = right.ParticipantId?.Trim() ?? string.Empty;
            comparison = string.CompareOrdinal(leftId, rightId);
            if (comparison != 0)
                return comparison;

            comparison = left.IsBot.CompareTo(right.IsBot);
            if (comparison != 0)
                return comparison;

            comparison = left.Position.x.CompareTo(right.Position.x);
            return comparison != 0 ? comparison : left.Position.y.CompareTo(right.Position.y);
        }

        private static string NormalizeOwner(string raw, bool isBot, int slotIndex)
            => string.IsNullOrWhiteSpace(raw) ? (isBot ? $"bot_{slotIndex}" : $"player_{slotIndex}") : raw.Trim();
    }
}
