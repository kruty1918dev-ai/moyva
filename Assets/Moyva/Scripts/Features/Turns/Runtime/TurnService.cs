using System;
using System.Collections.Generic;
using Kruty1918.Calendar.Core;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Turns.Runtime
{
    internal sealed partial class TurnService : ITurnService, ITurnEndQuery, ITurnHistoryQuery, ITurnStateRestorer,
        ITurnHistoryRestorer, IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IWorldGenerationSignalState _worldState;
        private readonly ICalendarService _calendar;
        private readonly ICalendarStateRestorer _calendarRestorer;
        private readonly RoundResolutionService _roundResolution;
        // Participant and blocker graphs may depend back on ITurnService through
        // recruitment/construction. Keep those graphs lazy until TurnService itself exists.
        private readonly LazyInject<List<ITurnParticipant>> _lazyParticipants;
        private readonly LazyInject<List<ITurnBlocker>> _lazyBlockers;
        private readonly List<ITurnParticipant> _explicitParticipants;
        private readonly List<ITurnBlocker> _explicitBlockers;
        private List<ITurnParticipant> _resolvedParticipants;
        private List<ITurnBlocker> _resolvedBlockers;
        // Owner resolution is an application-level extension graph. It may depend on
        // construction, which depends back on ITurnService, so keep it deferred too.
        private readonly LazyInject<ITurnLocalOwnerResolver> _lazyLocalOwnerResolver;
        private readonly LazyInject<List<ITurnTimerHold>> _lazyTimerHolds;
        private readonly ITurnLocalOwnerResolver _explicitLocalOwnerResolver;
        private readonly List<ITurnTimerHold> _explicitTimerHolds;
        private ITurnLocalOwnerResolver _resolvedLocalOwnerResolver;
        private List<ITurnTimerHold> _resolvedTimerHolds;
        private bool _localOwnerResolverResolutionAttempted;
        private readonly List<TurnFaction> _factions = new();
        private readonly HashSet<string> _eliminatedOwners = new(StringComparer.Ordinal);
        private int _activeFactionIndex;
        private TurnRules _rules;
        private bool _worldReady;
        private WorldSpawnPositionsSource _lastSpawnSource = WorldSpawnPositionsSource.Unknown;
        private PendingTurnRestore _pendingRestore;
        private bool _hasPendingRestore;
        private bool _endTurnEvaluationInProgress;
        private bool _matchEnded;
        // Set once the first replicated state lands: the first authoritative
        // apply is a silent baseline, later applies replay the host's delta.
        private bool _hasAppliedAuthoritativeBaseline;
        private readonly ITurnAuthorityPolicy _authority;

        private const string EndTurnEvaluationInProgressReason = "Turn end is already being evaluated.";
        private const string DefaultBlockedReason = "Turn is blocked.";
        private const string StateChangedDuringEvaluationReason = "Turn state changed while evaluating blockers.";
        private const string StateChangedDuringTransitionReason = "Turn state changed during lifecycle transition.";

        [Inject]
        public TurnService(
            SignalBus signalBus,
            IWorldGenerationSignalState worldState,
            ICalendarService calendar,
            LazyInject<List<ITurnParticipant>> participants,
            LazyInject<List<ITurnBlocker>> blockers,
            [InjectOptional] LazyInject<ITurnLocalOwnerResolver> localOwnerResolver = null,
            [InjectOptional] LazyInject<List<ITurnTimerHold>> timerHolds = null,
            [InjectOptional] ITurnAuthorityPolicy authority = null,
            [InjectOptional] ICalendarStateRestorer calendarRestorer = null,
            [InjectOptional] TurnRules rules = default)
        {
            _signalBus = signalBus;
            _worldState = worldState;
            _calendar = calendar;
            _calendarRestorer = calendarRestorer;
            _roundResolution = new RoundResolutionService(calendar);
            _lazyParticipants = participants;
            _lazyBlockers = blockers;
            _lazyLocalOwnerResolver = localOwnerResolver;
            _lazyTimerHolds = timerHolds;
            _authority = authority;
            _rules = rules;
        }

        // Keep direct-construction tests deterministic without making Zenject resolve
        // the runtime participant graph eagerly.
        internal TurnService(
            SignalBus signalBus,
            IWorldGenerationSignalState worldState,
            ICalendarService calendar,
            List<ITurnParticipant> participants,
            List<ITurnBlocker> blockers,
            ITurnLocalOwnerResolver localOwnerResolver = null,
            TurnRules rules = default,
            List<ITurnTimerHold> timerHolds = null,
            ICalendarStateRestorer calendarRestorer = null)
        {
            _signalBus = signalBus;
            _worldState = worldState;
            _calendar = calendar;
            _calendarRestorer = calendarRestorer;
            _roundResolution = new RoundResolutionService(calendar);
            _explicitParticipants = participants ?? new List<ITurnParticipant>();
            _explicitBlockers = blockers ?? new List<ITurnBlocker>();
            _explicitLocalOwnerResolver = localOwnerResolver;
            _explicitTimerHolds = timerHolds;
            _rules = rules;
        }

        public event Action StateChanged;
        public TurnPhase Phase { get; private set; } = TurnPhase.Initializing;
        public int Round { get; private set; } = 1;
        public long GlobalTurn { get; private set; } = 1;
        public int ActionsThisTurn { get; private set; }
        public string ActiveOwnerId => _factions.Count == 0 ? string.Empty : _factions[_activeFactionIndex].OwnerId;
        public string LocalOwnerId { get; private set; } = string.Empty;
        public IReadOnlyList<TurnFaction> Factions => _factions;

        public void Initialize()
        {
            _signalBus.Subscribe<WorldSpawnPositionsSignal>(OnSpawnPositions);
            _signalBus.Subscribe<WorldBuiltSignal>(OnWorldBuilt);
            _signalBus.Subscribe<FactionEliminatedSignal>(OnFactionEliminated);
            _signalBus.Subscribe<GameEndedSignal>(OnGameEnded);
            _signalBus.Subscribe<GameStartedSignal>(OnGameStarted);
            if (_worldState.TryGetWorldSpawnPositions(out WorldSpawnPositionsSignal cached))
                ConfigureFactions(cached.Assignments, cached.Source);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldSpawnPositionsSignal>(OnSpawnPositions);
            _signalBus.TryUnsubscribe<WorldBuiltSignal>(OnWorldBuilt);
            _signalBus.TryUnsubscribe<FactionEliminatedSignal>(OnFactionEliminated);
            _signalBus.TryUnsubscribe<GameEndedSignal>(OnGameEnded);
            _signalBus.TryUnsubscribe<GameStartedSignal>(OnGameStarted);
        }

        private List<ITurnParticipant> ResolveParticipants()
        {
            if (_resolvedParticipants != null)
                return _resolvedParticipants;

            List<ITurnParticipant> source =
                _explicitParticipants ?? _lazyParticipants?.Value;
            _resolvedParticipants = source != null
                ? new List<ITurnParticipant>(source)
                : new List<ITurnParticipant>();
            _resolvedParticipants.Sort(
                (left, right) => left.TurnOrder.CompareTo(right.TurnOrder));
            return _resolvedParticipants;
        }

        private List<ITurnBlocker> ResolveBlockers()
        {
            if (_resolvedBlockers != null)
                return _resolvedBlockers;

            List<ITurnBlocker> source =
                _explicitBlockers ?? _lazyBlockers?.Value;
            _resolvedBlockers = source != null
                ? new List<ITurnBlocker>(source)
                : new List<ITurnBlocker>();
            return _resolvedBlockers;
        }

        private List<ITurnTimerHold> ResolveTimerHolds()
        {
            if (_resolvedTimerHolds != null)
                return _resolvedTimerHolds;

            List<ITurnTimerHold> source =
                _explicitTimerHolds ?? _lazyTimerHolds?.Value;
            _resolvedTimerHolds = source != null
                ? new List<ITurnTimerHold>(source)
                : new List<ITurnTimerHold>();
            return _resolvedTimerHolds;
        }

        private ITurnLocalOwnerResolver ResolveLocalOwnerResolver()
        {
            if (_localOwnerResolverResolutionAttempted)
                return _resolvedLocalOwnerResolver;

            ITurnLocalOwnerResolver resolved =
                _explicitLocalOwnerResolver ?? _lazyLocalOwnerResolver?.Value;
            _resolvedLocalOwnerResolver = resolved;
            _localOwnerResolverResolutionAttempted = true;
            return resolved;
        }

        public bool IsOwnerActive(string ownerId)
        {
            if (FlowMode == TurnFlowMode.Simultaneous)
            {
                string normalized = ownerId?.Trim();
                return !string.IsNullOrWhiteSpace(normalized) && IsPendingOwner(normalized);
            }

            return !string.IsNullOrWhiteSpace(ownerId)
                   && string.Equals(ownerId.Trim(), ActiveOwnerId, StringComparison.Ordinal);
        }

        public bool CanOwnerAct(string ownerId, out string reason)
        {
            if (_matchEnded)
            {
                reason = "The match has ended.";
                return false;
            }

            if (!_worldReady || Phase != TurnPhase.AwaitingInput)
            {
                reason = $"Хід недоступний у фазі {Phase}.";
                return false;
            }

            if (FlowMode == TurnFlowMode.Simultaneous)
            {
                string normalized = ownerId?.Trim();
                if (string.IsNullOrWhiteSpace(normalized))
                {
                    reason = "Owner id is empty.";
                    return false;
                }

                if (_factions.FindIndex(
                        faction => string.Equals(faction.OwnerId, normalized, StringComparison.Ordinal)) < 0)
                {
                    reason = $"Фракція '{normalized}' не бере участі в матчі.";
                    return false;
                }

                if (_eliminatedOwners.Contains(normalized))
                {
                    reason = $"Фракція '{normalized}' вибула з гри.";
                    return false;
                }

                if (_endedTurnOwners.Contains(normalized))
                {
                    reason = $"Фракція '{normalized}' вже завершила хід у цьому раунді.";
                    return false;
                }

                reason = null;
                return true;
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
            if (_authority != null && !_authority.IsAuthoritative)
                return false;
            if (!CanOwnerAct(ownerId, out _))
                return false;

            ActionsThisTurn++;
            StateChanged?.Invoke();
            return true;
        }

        public bool TryEndTurn(string requesterOwnerId, out string reason)
        {
            if (!CanEndTurn(requesterOwnerId, out reason))
                return false;
            if (FlowMode == TurnFlowMode.Simultaneous)
                return EndOwnerTurnSimultaneous(requesterOwnerId);
            if (!EndCurrentTurn())
            {
                reason = StateChangedDuringTransitionReason;
                return false;
            }
            return true;
        }

        public bool CanEndTurn(string requesterOwnerId, out string reason)
        {
            if (_authority != null && !_authority.IsAuthoritative)
            {
                reason = "Only the host can advance the turn.";
                return false;
            }
            if (!CanOwnerAct(requesterOwnerId, out reason))
                return false;

            if (_endTurnEvaluationInProgress)
            {
                reason = EndTurnEvaluationInProgressReason;
                return false;
            }

            ITurnBlocker[] blockers = ResolveBlockers().ToArray();
            TurnStateSnapshot snapshot = CaptureTurnState();
            _endTurnEvaluationInProgress = true;
            try
            {
                for (int index = 0; index < blockers.Length; index++)
                {
                    if (!MatchesTurnState(snapshot, TurnPhase.AwaitingInput))
                    {
                        reason = StateChangedDuringEvaluationReason;
                        return false;
                    }

                    ITurnBlocker blocker = blockers[index];
                    if (blocker == null)
                        continue;

                    // Owner-aware blockers see the requesting faction so local-only
                    // concerns (in-flight animations, deployment sessions) do not
                    // stall a remote owner's round in simultaneous mode.
                    bool blocked = blocker is ITurnOwnerBlocker ownerBlocker
                        ? ownerBlocker.IsTurnBlocked(requesterOwnerId, out string blockerReason)
                        : blocker.IsTurnBlocked(out blockerReason);
                    if (!MatchesTurnState(snapshot, TurnPhase.AwaitingInput))
                    {
                        reason = StateChangedDuringEvaluationReason;
                        return false;
                    }

                    if (blocked)
                    {
                        reason = string.IsNullOrWhiteSpace(blockerReason) ? DefaultBlockedReason : blockerReason;
                        return false;
                    }
                }
            }
            finally
            {
                _endTurnEvaluationInProgress = false;
            }

            if (!MatchesTurnState(snapshot, TurnPhase.AwaitingInput))
            {
                reason = StateChangedDuringEvaluationReason;
                return false;
            }

            reason = null;
            return true;
        }

        public void Restore(int round, long globalTurn, string activeOwnerId, int actions)
            => Restore(
                round,
                globalTurn,
                activeOwnerId,
                actions,
                _rules.Mode,
                _rules.RoundTimerSeconds,
                null,
                0f);

        public void Restore(
            int round,
            long globalTurn,
            string activeOwnerId,
            int actions,
            TurnFlowMode mode,
            int roundTimerSeconds,
            IReadOnlyCollection<string> endedOwners,
            float roundSecondsRemaining)
            => QueueRestore(round, globalTurn, activeOwnerId, actions, mode,
                roundTimerSeconds, endedOwners, roundSecondsRemaining,
                isAuthoritative: false, calendarTotalHours: -1);

        public void RestoreAuthoritative(
            int round,
            long globalTurn,
            string activeOwnerId,
            int actions,
            TurnFlowMode mode,
            int roundTimerSeconds,
            IReadOnlyCollection<string> endedOwners,
            float roundSecondsRemaining,
            long calendarTotalHours)
            => QueueRestore(round, globalTurn, activeOwnerId, actions, mode,
                roundTimerSeconds, endedOwners, roundSecondsRemaining,
                isAuthoritative: true, calendarTotalHours: calendarTotalHours);

        private void QueueRestore(
            int round,
            long globalTurn,
            string activeOwnerId,
            int actions,
            TurnFlowMode mode,
            int roundTimerSeconds,
            IReadOnlyCollection<string> endedOwners,
            float roundSecondsRemaining,
            bool isAuthoritative,
            long calendarTotalHours)
        {
            if (!_historyPrepared)
                _deriveLegacyHistory = true;
            _historyPrepared = false;
            if (!isAuthoritative)
                _hasAppliedAuthoritativeBaseline = false;
            _pendingRestore = new PendingTurnRestore(
                round,
                globalTurn,
                activeOwnerId?.Trim() ?? string.Empty,
                actions,
                mode,
                roundTimerSeconds,
                endedOwners,
                roundSecondsRemaining,
                isAuthoritative,
                calendarTotalHours);
            _hasPendingRestore = true;

            // Loading may happen before spawn assignments/world readiness, or while an existing
            // session is already AwaitingInput. In both cases stop the live turn and let TryStart
            // resume the persisted snapshot only after its owner can be resolved.
            bool notifyInitializing = Phase != TurnPhase.Initializing;
            Phase = TurnPhase.Initializing;
            TryStart();
            // Notify once with the resolved state. A transient Initializing event would
            // cancel movement leases even when only the host's action counter changed.
            if (notifyInitializing && Phase == TurnPhase.Initializing)
                StateChanged?.Invoke();
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
                return;
            }
            _historyCache = null;
            StateChanged?.Invoke();

            if ((_authority?.IsAuthoritative ?? true)
                && _worldReady && Phase == TurnPhase.AwaitingInput)
            {
                if (FlowMode == TurnFlowMode.Simultaneous)
                {
                    _endedTurnOwners.Remove(ownerId);
                    RefreshSimultaneousActiveCursor();
                    if (AllAliveOwnersEnded())
                        CompleteSimultaneousRound();
                }
                else if (IsOwnerActive(ownerId))
                {
                    EndCurrentTurn();
                }
            }
        }

        private void OnGameEnded(GameEndedSignal _)
        {
            _matchEnded = true;
            Phase = TurnPhase.Completed;
            StateChanged?.Invoke();
        }

        private void OnGameStarted(GameStartedSignal _)
        {
            _matchEnded = false;
            if (Phase == TurnPhase.Completed)
                Phase = TurnPhase.Initializing;
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

            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < ordered.Length; index++)
                seen.Add(NormalizeOwner(ordered[index].ParticipantId, ordered[index].SlotIndex));

            // A changed owner set means a different match projection: the next
            // authoritative apply is a fresh baseline, not a delta to replay.
            bool ownersChanged = seen.Count != _factions.Count;
            if (!ownersChanged)
            {
                for (int index = 0; index < _factions.Count; index++)
                {
                    if (!seen.Contains(_factions[index].OwnerId))
                    {
                        ownersChanged = true;
                        break;
                    }
                }
            }
            if (ownersChanged)
                _hasAppliedAuthoritativeBaseline = false;

            _factions.Clear();
            var addedOwners = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < ordered.Length; index++)
            {
                SpawnPositionAssignment assignment = ordered[index];
                string ownerId = NormalizeOwner(assignment.ParticipantId, assignment.SlotIndex);
                if (addedOwners.Add(ownerId))
                    _factions.Add(new TurnFaction(ownerId, assignment.Position));
            }

            _eliminatedOwners.RemoveWhere(ownerId => !seen.Contains(ownerId));
            _endedTurnOwners.RemoveWhere(ownerId => !seen.Contains(ownerId));
            _historyCache = null;
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
        }

        private string ResolveLocalOwnerId()
        {
            if (_factions.Count == 0)
                return string.Empty;

            ITurnLocalOwnerResolver localOwnerResolver =
                ResolveLocalOwnerResolver();
            if (localOwnerResolver != null)
            {
                string resolved = localOwnerResolver.ResolveLocalOwnerId(_factions)?.Trim();
                if (string.IsNullOrWhiteSpace(resolved))
                    return string.Empty;

                int match = _factions.FindIndex(f => string.Equals(f.OwnerId, resolved, StringComparison.Ordinal));
                return match >= 0 ? _factions[match].OwnerId : string.Empty;
            }

            return _factions[0].OwnerId;
        }

        private void TryStart()
        {
            if (!_worldReady || _factions.Count == 0 || Phase != TurnPhase.Initializing)
                return;

            if (_hasPendingRestore)
            {
                if (!TryApplyPendingRestore(out TurnTransitionDelta delta))
                    return;

                ResumeRestoredTurn(in delta);
                return;
            }

            if (_matchEnded)
            {
                Phase = TurnPhase.Completed;
                return;
            }

            if (_authority != null && !_authority.IsAuthoritative)
                return;

            if (_eliminatedOwners.Contains(ActiveOwnerId) && !TrySelectFirstEligibleFaction())
            {
                return;
            }

            if (FlowMode == TurnFlowMode.Simultaneous)
            {
                StartSimultaneousRound();
                return;
            }

            StartCurrentTurn();
        }

        private bool TryApplyPendingRestore(out TurnTransitionDelta delta)
        {
            // The covered delta between the last applied state and this one —
            // captured before the overwrite so ResumeRestoredTurn can replay
            // exactly the transitions the authoritative peer already ran.
            delta = new TurnTransitionDelta(
                Round,
                GlobalTurn,
                _activeFactionIndex,
                _pendingRestore.IsAuthoritative,
                _pendingRestore.CalendarTotalHours);

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
                return false;
            }

            if (!_matchEnded && _eliminatedOwners.Contains(ownerId))
            {
                Debug.LogError(
                    $"[Turns] Cannot resume saved turn: owner '{ownerId}' is already eliminated.");
                return false;
            }

            // The synchronized payload is authoritative for the flow rules on
            // clients; the hold flag stays a local JSON/resolver concern.
            _rules = new TurnRules(
                _pendingRestore.Mode,
                _pendingRestore.RoundTimerSeconds,
                _rules.HoldTimerUntilInitialCastlesPlaced);

            Round = _pendingRestore.Round;
            GlobalTurn = _pendingRestore.GlobalTurn;
            ActionsThisTurn = _pendingRestore.Actions;
            _activeFactionIndex = restoredIndex;

            _endedTurnOwners.Clear();
            if (_pendingRestore.EndedOwners != null)
            {
                foreach (string endedOwner in _pendingRestore.EndedOwners)
                {
                    if (string.IsNullOrWhiteSpace(endedOwner))
                        continue;

                    string normalized = endedOwner.Trim();
                    if (_factions.FindIndex(
                            faction => string.Equals(faction.OwnerId, normalized, StringComparison.Ordinal)) >= 0
                        && !_eliminatedOwners.Contains(normalized))
                    {
                        _endedTurnOwners.Add(normalized);
                    }
                }
            }

            ArmRoundTimer();
            if (RoundTimerSeconds > 0 && _pendingRestore.RoundSecondsRemaining > 0f)
                _roundEndsAtUnscaled = Time.unscaledTime + _pendingRestore.RoundSecondsRemaining;

            if (_rules.Mode == TurnFlowMode.Simultaneous)
                RefreshSimultaneousActiveCursor();

            if (_deriveLegacyHistory)
                RestoreLegacyHistory();
            _hasPendingRestore = false;
            return true;
        }

        private void ResumeRestoredTurn(in TurnTransitionDelta delta)
        {
            // Save restores stay silent: participant state is rebuilt by its own
            // save modules and must not receive a second turn-start side effect.
            // Replicated applies instead replay the host's covered delta — the
            // authoritative peer already committed the state, so the callbacks
            // only reproduce deterministic local effects (progress, stamina,
            // economy ticks) with no new authority granted to the client.
            if (delta.IsAuthoritative)
            {
                if (_hasAppliedAuthoritativeBaseline && !_matchEnded)
                    ReplayAuthoritativeDelta(in delta);
                _hasAppliedAuthoritativeBaseline = true;
                if (delta.CalendarTotalHours >= 0)
                    _calendarRestorer?.RestoreByTotalHours(delta.CalendarTotalHours);
            }

            Phase = _matchEnded ? TurnPhase.Completed : TurnPhase.AwaitingInput;
            StateChanged?.Invoke();
        }

        private bool StartCurrentTurn()
        {
            List<ITurnParticipant> participants = ResolveParticipants();
            TurnStateSnapshot transition = CaptureTurnState();
            Phase = TurnPhase.Starting;
            ActionsThisTurn = 0;
            transition = transition.WithPhaseAndActions(TurnPhase.Starting, 0);
            StateChanged?.Invoke();
            if (!MatchesLifecycleState(transition, TurnPhase.Starting))
                return false;

            TurnContext context = CurrentContext();
            for (int index = 0; index < participants.Count; index++)
            {
                participants[index]?.OnTurnStarted(context);
                if (!MatchesLifecycleState(transition, TurnPhase.Starting))
                    return false;
            }

            if (_eliminatedOwners.Contains(ActiveOwnerId))
            {
                return EndCurrentTurn();
            }

            Phase = TurnPhase.AwaitingInput;
            StateChanged?.Invoke();
            return true;
        }

        private bool EndCurrentTurn()
        {
            List<ITurnParticipant> participants = ResolveParticipants();
            TurnStateSnapshot transition = CaptureTurnState();
            Phase = TurnPhase.Ending;
            transition = transition.WithPhaseAndActions(TurnPhase.Ending, ActionsThisTurn);
            StateChanged?.Invoke();
            if (!MatchesLifecycleState(transition, TurnPhase.Ending))
                return false;

            TurnContext context = CurrentContext();
            for (int index = participants.Count - 1; index >= 0; index--)
            {
                participants[index]?.OnTurnEnding(context);
                if (!MatchesLifecycleState(transition, TurnPhase.Ending))
                    return false;
            }

            if (!TryFindNextEligibleFaction(out int nextIndex, out bool completedRound))
            {
                Phase = TurnPhase.Resolving;
                StateChanged?.Invoke();
                return true;
            }

            if (completedRound)
            {
                int completedRoundNumber = Round;
                Phase = TurnPhase.Resolving;
                TurnStateSnapshot resolving = transition.WithPhaseAndActions(TurnPhase.Resolving, ActionsThisTurn);
                StateChanged?.Invoke();
                if (!MatchesLifecycleState(resolving, TurnPhase.Resolving))
                    return false;

                if (!_roundResolution.TryResolve(
                        completedRoundNumber,
                        participants,
                        () => MatchesLifecycleState(resolving, TurnPhase.Resolving),
                        out string resolutionReason))
                {
                    Debug.LogError(
                        $"[Turns] Round {completedRoundNumber} resolution failed; " +
                        $"next round will not start. Reason: {resolutionReason}");
                    return false;
                }

                Round++;
            }

            _activeFactionIndex = nextIndex;
            RecordCompletedTurn(context.Faction.OwnerId);
            GlobalTurn++;
            return StartCurrentTurn();
        }

        private bool TryFindNextEligibleFaction(out int nextIndex, out bool completedRound)
            => TryFindNextEligibleFrom(_activeFactionIndex, out nextIndex, out completedRound);

        private bool TryFindNextEligibleFrom(int fromIndex, out int nextIndex, out bool completedRound)
        {
            completedRound = false;
            nextIndex = -1;
            if (_factions.Count == 0)
                return false;

            for (int step = 1; step <= _factions.Count; step++)
            {
                int rawIndex = fromIndex + step;
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
            => new(
                Round,
                GlobalTurn,
                _activeFactionIndex,
                _factions[_activeFactionIndex],
                FlowMode,
                IsLocalOwnerId(_factions[_activeFactionIndex].OwnerId));

        private TurnStateSnapshot CaptureTurnState()
            => new(Phase, Round, GlobalTurn, ActionsThisTurn, _activeFactionIndex, ActiveOwnerId, _factions.Count, _eliminatedOwners.Count);

        private bool MatchesTurnState(TurnStateSnapshot snapshot, TurnPhase expectedPhase)
            => Phase == expectedPhase
               && Round == snapshot.Round
               && GlobalTurn == snapshot.GlobalTurn
               && ActionsThisTurn == snapshot.Actions
               && _activeFactionIndex == snapshot.ActiveFactionIndex
               && _factions.Count == snapshot.FactionCount
               && _eliminatedOwners.Count == snapshot.EliminatedOwnerCount
               && string.Equals(ActiveOwnerId, snapshot.ActiveOwnerId, StringComparison.Ordinal);

        private bool MatchesLifecycleState(TurnStateSnapshot snapshot, TurnPhase expectedPhase)
            => Phase == expectedPhase
               && Round == snapshot.Round
               && GlobalTurn == snapshot.GlobalTurn
               && ActionsThisTurn == snapshot.Actions
               && _activeFactionIndex == snapshot.ActiveFactionIndex
               && _factions.Count == snapshot.FactionCount
               && string.Equals(ActiveOwnerId, snapshot.ActiveOwnerId, StringComparison.Ordinal);

        private readonly struct TurnStateSnapshot
        {
            public TurnStateSnapshot(
                TurnPhase phase, int round, long globalTurn, int actions,
                int activeFactionIndex, string activeOwnerId, int factionCount, int eliminatedOwnerCount)
            {
                Phase = phase;
                Round = round;
                GlobalTurn = globalTurn;
                Actions = actions;
                ActiveFactionIndex = activeFactionIndex;
                ActiveOwnerId = activeOwnerId ?? string.Empty;
                FactionCount = factionCount;
                EliminatedOwnerCount = eliminatedOwnerCount;
            }

            public TurnPhase Phase { get; }
            public int Round { get; }
            public long GlobalTurn { get; }
            public int Actions { get; }
            public int ActiveFactionIndex { get; }
            public string ActiveOwnerId { get; }
            public int FactionCount { get; }
            public int EliminatedOwnerCount { get; }

            public TurnStateSnapshot WithPhaseAndActions(TurnPhase phase, int actions)
                => new(phase, Round, GlobalTurn, actions, ActiveFactionIndex, ActiveOwnerId, FactionCount, EliminatedOwnerCount);
        }

        private readonly struct PendingTurnRestore
        {
            public PendingTurnRestore(
                int round,
                long globalTurn,
                string activeOwnerId,
                int actions,
                TurnFlowMode mode = TurnFlowMode.Sequential,
                int roundTimerSeconds = 0,
                IReadOnlyCollection<string> endedOwners = null,
                float roundSecondsRemaining = 0f,
                bool isAuthoritative = false,
                long calendarTotalHours = -1)
            {
                Round = round;
                GlobalTurn = globalTurn;
                ActiveOwnerId = activeOwnerId ?? string.Empty;
                Actions = actions;
                Mode = mode;
                RoundTimerSeconds = roundTimerSeconds < 0 ? 0 : roundTimerSeconds;
                EndedOwners = endedOwners;
                RoundSecondsRemaining = roundSecondsRemaining < 0f ? 0f : roundSecondsRemaining;
                IsAuthoritative = isAuthoritative;
                CalendarTotalHours = calendarTotalHours;
            }

            public int Round { get; }
            public long GlobalTurn { get; }
            public string ActiveOwnerId { get; }
            public int Actions { get; }
            public TurnFlowMode Mode { get; }
            public int RoundTimerSeconds { get; }
            public IReadOnlyCollection<string> EndedOwners { get; }
            public float RoundSecondsRemaining { get; }
            /// <summary>True when the snapshot came from the authoritative peer.</summary>
            public bool IsAuthoritative { get; }
            /// <summary>Authoritative calendar total hours; -1 = not carried.</summary>
            public long CalendarTotalHours { get; }
        }

        /// <summary>The delta between the previously applied state and the one
        /// just restored — the span the authoritative replay must cover.</summary>
        private readonly struct TurnTransitionDelta
        {
            public TurnTransitionDelta(
                int previousRound,
                long previousGlobalTurn,
                int previousActiveFactionIndex,
                bool isAuthoritative,
                long calendarTotalHours)
            {
                PreviousRound = previousRound;
                PreviousGlobalTurn = previousGlobalTurn;
                PreviousActiveFactionIndex = previousActiveFactionIndex;
                IsAuthoritative = isAuthoritative;
                CalendarTotalHours = calendarTotalHours;
            }

            public int PreviousRound { get; }
            public long PreviousGlobalTurn { get; }
            public int PreviousActiveFactionIndex { get; }
            public bool IsAuthoritative { get; }
            public long CalendarTotalHours { get; }
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

            comparison = left.Position.x.CompareTo(right.Position.x);
            return comparison != 0 ? comparison : left.Position.y.CompareTo(right.Position.y);
        }

        private static string NormalizeOwner(string raw, int slotIndex)
            => string.IsNullOrWhiteSpace(raw) ? $"player_{slotIndex}" : raw.Trim();
    }
}
