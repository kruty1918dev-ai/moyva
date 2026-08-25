using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.Faction.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.ObjectsMap.API;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.BotAI.Runtime
{
    /// <summary>
    /// Single turn-scoped executor for bot gameplay mutations.
    ///
    /// It deliberately issues commands through the same authoritative construction,
    /// recruitment and movement APIs used by normal gameplay. The executor never ends
    /// the turn itself: TurnBotDriver retries ITurnService.TryEndTurn on later ticks,
    /// allowing ITurnBlocker implementations (notably movement) to settle first.
    /// </summary>
    public sealed class BotTurnExecutor : IBotTurnExecutor, ITurnBlocker, IDisposable
    {
        internal const int MaxMutatingActionsPerTurn = 6;





        private readonly ITurnService _turns;
        private readonly IFactionRegistry _factions;
        private readonly IUnitRecruitmentService _recruitment;
        private readonly IUnitService _units;
        private readonly IUnitOwnershipQuery _ownership;
        private readonly IUnitMovementService _movement;
        private readonly IGridService _grid;
        private readonly IObjectsMapService _objectsMap;
        private readonly IFogOfWarServiceRegistry _fogRegistry;
        private readonly IBotWorldSnapshotBuilder _snapshotBuilder;
        private readonly IBotMemoryStore _memory;
        private readonly BotPlanningProfile _profile;
        private readonly IBotStrategicPlanner _strategicPlanner;
        private readonly IBotTurnPlanner _turnPlanner;
        private readonly IBotActionExecutor _actionExecutor;
        private readonly IBotReasoningTrace _reasoning;
        private readonly IBotStallTracker _stall;
        private readonly BotVerticalSliceDiagnostics _verticalSlice;
        private readonly HashSet<BotTurnEpoch> _startedEpochs = new();
        private BotTurnSession _activeSession;

        [Inject]
        public BotTurnExecutor(
            [InjectOptional] ITurnService turns = null,
            [InjectOptional] IFactionRegistry factions = null,
            [InjectOptional] IUnitRecruitmentService recruitment = null,
            [InjectOptional] IUnitService units = null,
            [InjectOptional] IUnitOwnershipQuery ownership = null,
            [InjectOptional] IUnitMovementService movement = null,
            [InjectOptional] IGridService grid = null,
            [InjectOptional] IObjectsMapService objectsMap = null,
            [InjectOptional] IFogOfWarServiceRegistry fogRegistry = null,
            [InjectOptional] IBotWorldSnapshotBuilder snapshotBuilder = null,
            [InjectOptional] IBotMemoryStore memory = null,
            [InjectOptional] BotPlanningProfile profile = null,
            [InjectOptional] IBotStrategicPlanner strategicPlanner = null,
            [InjectOptional] IBotTurnPlanner turnPlanner = null,
            [InjectOptional] IBotActionExecutor actionExecutor = null,
            [InjectOptional] IBotReasoningTrace reasoning = null,
            [InjectOptional] IBotStallTracker stall = null,
            [InjectOptional] BotVerticalSliceDiagnostics verticalSlice = null)
        {
            _turns = turns;
            _factions = factions;
            _recruitment = recruitment;
            _units = units;
            _ownership = ownership;
            _movement = movement;
            _grid = grid;
            _objectsMap = objectsMap;
            _fogRegistry = fogRegistry;
            _snapshotBuilder = snapshotBuilder;
            _memory = memory;
            _profile = profile ?? BotPlanningProfile.Normal();
            _strategicPlanner = strategicPlanner;
            _turnPlanner = turnPlanner;
            _actionExecutor = actionExecutor;
            _reasoning = reasoning;
            _stall = stall;
            _verticalSlice = verticalSlice;
        }

        internal int StartedEpochCount => _startedEpochs.Count;
        internal bool IsSessionActive => _activeSession != null && !_activeSession.Task.IsCompleted;

        public bool TryBeginTurn(string ownerId, long globalTurn, out string reason)
        {
            reason = null;
            string owner = NormalizeId(ownerId);
            if (owner == null)
            {
                reason = "Bot turn owner is empty.";
                return false;
            }
            if (globalTurn < 1)
            {
                reason = "Bot global turn must be positive.";
                return false;
            }
            if (_turns == null)
            {
                reason = "Turn authority is unavailable for bot execution.";
                return false;
            }
            if (_turns.GlobalTurn != globalTurn)
            {
                reason = $"Bot turn epoch mismatch: requested {globalTurn}, authoritative {_turns.GlobalTurn}.";
                return false;
            }
            if (_turns.Phase != TurnPhase.AwaitingInput)
            {
                reason = $"Bot execution requires AwaitingInput, current phase is {_turns.Phase}.";
                return false;
            }
            if (!_turns.IsActiveFactionBot)
            {
                reason = "The active faction is not a bot.";
                return false;
            }
            if (!string.Equals(NormalizeId(_turns.ActiveOwnerId), owner, StringComparison.Ordinal))
            {
                reason = $"Bot owner '{owner}' is not the active owner '{_turns.ActiveOwnerId}'.";
                return false;
            }
            if (!_turns.CanOwnerAct(owner, out reason))
                return false;

            var epoch = new BotTurnEpoch(owner, globalTurn);
            if (_startedEpochs.Contains(epoch))
            {
                reason = null;
                return true;
            }

            CancelActiveSessionIfDifferent(epoch);

            // Claim the epoch before the first mutation. If a synchronous gameplay
            // signal re-enters the executor, the same turn cannot issue actions twice.
            _startedEpochs.Add(epoch);
            PruneOldEpochs(globalTurn);
            _stall?.BeginTurn(owner, globalTurn);

            _reasoning?.Record(
                owner,
                globalTurn,
                BotReasoningStage.Session,
                "Починаю хід бота",
                $"Активний owner='{owner}', globalTurn={globalTurn}. Спочатку знімаю власний snapshot світу, потім оцінюю strategy, goals і всі доступні candidates.");

            var cancellation = new CancellationTokenSource();
            Task task = ExecuteTurnSessionAsync(owner, globalTurn, cancellation.Token);
            _activeSession = new BotTurnSession(epoch, cancellation, task);
            reason = null;
            return true;
        }

        public bool IsTurnBlocked(out string reason)
        {
            BotTurnSession session = _activeSession;
            if (session == null)
            {
                reason = null;
                return false;
            }

            if (!session.Task.IsCompleted)
            {
                reason = $"Bot turn execution is still running for owner '{session.Epoch.OwnerId}'.";
                return true;
            }

            if (session.Task.IsFaulted)
                Debug.LogError($"[BotTurnExecutor] Bot session faulted: {session.Task.Exception}");

            session.Dispose();
            if (ReferenceEquals(_activeSession, session))
                _activeSession = null;

            reason = null;
            return false;
        }

        public void Dispose()
        {
            BotTurnSession session = _activeSession;
            if (session == null)
                return;

            session.Cancel();
            session.Dispose();
            _activeSession = null;
        }

        private async Task ExecuteTurnSessionAsync(string ownerId, long globalTurn, CancellationToken token)
        {
            bool cancelled = false;

            try
            {
                await ExecuteTurnAsync(ownerId, globalTurn, token);
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
            }
            catch (Exception exception)
            {
                // Keep the epoch claimed after a partial failure. Retrying the same
                // turn could duplicate a successful command that preceded the fault.
                Debug.LogError($"[BotTurnExecutor] owner={ownerId} globalTurn={globalTurn}: {exception}");
            }
            finally
            {
                if (!cancelled && _stall != null)
                {
                    BotStallStatus status =
                        _stall.CompleteTurn(ownerId, globalTurn);

                    if (status.IsStalled)
                    {
                        _reasoning?.Record(
                            ownerId,
                            globalTurn,
                            BotReasoningStage.Warning,
                            "Bot Stall Reason",
                            status.StallReason,
                            subjectId: status.LastCandidateId);
                    }
                    else if (status.ConsecutiveZeroMutationTurns > 0)
                    {
                        _reasoning?.Record(
                            ownerId,
                            globalTurn,
                            BotReasoningStage.TurnComplete,
                            "Turn completed without mutation",
                            $"zeroMutationStreak={status.ConsecutiveZeroMutationTurns}/" +
                            $"{status.Threshold}; lastCandidate='{status.LastCandidateId}'; " +
                            $"lastFailure='{status.LastFailureReason}'.");
                    }
                }
            }
        }

        private async Task ExecuteTurnAsync(string ownerId, long globalTurn, CancellationToken token)
        {
            var budget = new BotTurnBudget(MaxMutatingActionsPerTurn);

            if (_snapshotBuilder == null ||
                _strategicPlanner == null ||
                _turnPlanner == null ||
                _actionExecutor == null)
            {
                _reasoning?.Record(
                    ownerId,
                    globalTurn,
                    BotReasoningStage.Warning,
                    "Modern BotAI pipeline unavailable",
                    "BotRuntimeBindings must provide snapshot, strategic planner, turn planner and action executor. " +
                    "The legacy barrack/warrior fallback path has been removed to keep one authoritative AI graph.");

                return;
            }

            await ExecutePlannedActionsAsync(
                ownerId,
                globalTurn,
                budget,
                token);
        }

        private async Task ExecutePlannedActionsAsync(
            string ownerId,
            long globalTurn,
            BotTurnBudget budget,
            CancellationToken token)
        {
            if (_snapshotBuilder == null
                || _strategicPlanner == null
                || _turnPlanner == null
                || _actionExecutor == null)
            {
                return;
            }

            int successfulMutations = 0;
            int failedMutations = 0;
            var rejectedCandidateIds = new HashSet<string>(StringComparer.Ordinal);
            int maxIterations = Math.Max(1, Math.Min(_profile.MaxDecisionIterations, budget.Remaining));
            for (int iteration = 0;
                 iteration < maxIterations
                 && budget.HasRemaining
                 && successfulMutations < _profile.MaxSuccessfulMutations
                 && failedMutations < _profile.MaxFailedMutations;
                 iteration++)
            {
                token.ThrowIfCancellationRequested();
                if (!IsSameTurnEpoch(ownerId, globalTurn))
                    return;

                BotWorldSnapshot snapshot = _snapshotBuilder.Build(ownerId, globalTurn);
                _memory?.UpdateFromObservation(snapshot, _profile);
                snapshot = _snapshotBuilder.Build(ownerId, globalTurn);
                _verticalSlice?.Observe(snapshot);
                BotStrategicContext strategy = _strategicPlanner.Plan(snapshot);

                _reasoning?.Record(
                    ownerId,
                    globalTurn,
                    BotReasoningStage.Strategy,
                    $"Стратегія: {strategy.Posture}",
                    BotReasoningNarrator.DescribeStrategy(strategy),
                    strategy.PostureScore);

                IReadOnlyList<BotActionCandidate> candidates = _turnPlanner.GenerateCandidates(snapshot, strategy);
                if (candidates == null || candidates.Count == 0)
                {
                    _reasoning?.Record(
                        ownerId,
                        globalTurn,
                        BotReasoningStage.Warning,
                        "Немає допустимих дій",
                        "Планувальники не повернули жодного candidate; бот завершить decision loop без вигаданої дії.");
                    return;
                }

                int diagnosticCandidates = Math.Min(8, candidates.Count);
                for (int diagnosticIndex = 0; diagnosticIndex < diagnosticCandidates; diagnosticIndex++)
                {
                    BotActionCandidate diagnostic = candidates[diagnosticIndex];
                    _reasoning?.Record(
                        ownerId,
                        globalTurn,
                        BotReasoningStage.Candidate,
                        $"Кандидат #{diagnosticIndex + 1}: {diagnostic.Kind}",
                        BotReasoningNarrator.DescribeCandidate(
                            diagnostic,
                            diagnosticIndex + 1,
                            candidates.Count),
                        diagnostic.Score.Total,
                        diagnostic.TargetCell,
                        diagnostic.CandidateId);
                }

                BotActionCandidate selected = default;
                bool found = false;
                for (int index = 0; index < candidates.Count; index++)
                {
                    if (rejectedCandidateIds.Contains(candidates[index].CandidateId))
                        continue;
                    if (candidates[index].Score.Total < _profile.MinUtilityToAct)
                        continue;

                    selected = candidates[index];
                    found = true;
                    break;
                }

                if (!found)
                    return;

                _reasoning?.Record(
                    ownerId,
                    globalTurn,
                    BotReasoningStage.Selection,
                    $"Обираю {selected.Kind}",
                    BotReasoningNarrator.DescribeCandidate(selected, 1, candidates.Count),
                    selected.Score.Total,
                    selected.TargetCell,
                    selected.CandidateId);

                _reasoning?.Record(
                    ownerId,
                    globalTurn,
                    BotReasoningStage.ActionAttempt,
                    $"Пробую виконати {selected.Kind}",
                    $"Canonical executor отримує candidate '{selected.CandidateId}'.",
                    selected.Score.Total,
                    selected.TargetCell,
                    selected.CandidateId);

                BotActionExecutionResult result = await _actionExecutor.ExecuteAsync(ownerId, selected, token);

                _stall?.RecordActionResult(
                    ownerId,
                    globalTurn,
                    selected.CandidateId,
                    result.Succeeded,
                    result.Mutated,
                    result.Reason);

                _reasoning?.Record(
                    ownerId,
                    globalTurn,
                    BotReasoningStage.ActionResult,
                    result.Succeeded
                        ? $"Результат {selected.Kind}: success"
                        : $"Результат {selected.Kind}: rejected",
                    BotReasoningNarrator.DescribeActionResult(selected, result),
                    selected.Score.Total,
                    selected.TargetCell,
                    selected.CandidateId);

                if (result.Succeeded && result.Mutated)
                {
                    budget.TrySpend();
                    successfulMutations++;
                    continue;
                }

                failedMutations++;
                if (!result.Succeeded)
                {
                    rejectedCandidateIds.Add(selected.CandidateId);
                    continue;
                }
            }
        }

        private bool IsSameTurnEpoch(string ownerId, long globalTurn)
            => _turns != null
               && _turns.GlobalTurn == globalTurn
               && _turns.Phase == TurnPhase.AwaitingInput
               && string.Equals(NormalizeId(_turns.ActiveOwnerId), ownerId, StringComparison.Ordinal);





        private void CancelActiveSessionIfDifferent(BotTurnEpoch epoch)
        {
            BotTurnSession session = _activeSession;
            if (session == null)
                return;

            if (session.Epoch.Equals(epoch))
                return;

            session.Cancel();
            if (session.Task.IsCompleted)
                session.Dispose();
            _activeSession = null;
        }




        internal static Vector2Int[] BuildPreferredMoveCandidates(
            Vector2Int current,
            Vector2Int? enemy,
            long globalTurn,
            int unitOrdinal)
        {
            Vector2Int[] offsets =
            {
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.left,
                Vector2Int.down,
            };

            int rotation = PositiveModulo(globalTurn + unitOrdinal, offsets.Length);
            var candidates = new Vector2Int[offsets.Length];
            for (int index = 0; index < offsets.Length; index++)
                candidates[index] = current + offsets[(rotation + index) % offsets.Length];

            if (!enemy.HasValue)
                return candidates;

            // Stable insertion sort by distance to the target. The rotated order
            // remains the tie-breaker, so identical inputs are deterministic.
            for (int index = 1; index < candidates.Length; index++)
            {
                Vector2Int value = candidates[index];
                int valueDistance = ManhattanDistance(value, enemy.Value);
                int cursor = index - 1;
                while (cursor >= 0
                       && ManhattanDistance(candidates[cursor], enemy.Value) > valueDistance)
                {
                    candidates[cursor + 1] = candidates[cursor];
                    cursor--;
                }
                candidates[cursor + 1] = value;
            }

            return candidates;
        }


        private static int ManhattanDistance(Vector2Int left, Vector2Int right)
            => Mathf.Abs(left.x - right.x) + Mathf.Abs(left.y - right.y);

        private static int PositiveModulo(long value, int divisor)
        {
            if (divisor <= 0)
                return 0;
            long result = value % divisor;
            return (int)(result < 0 ? result + divisor : result);
        }


        private static void AddStableHash(ref uint hash, string value)
        {
            value ??= string.Empty;
            for (int index = 0; index < value.Length; index++)
            {
                hash ^= value[index];
                hash *= 16777619u;
            }
        }

        private void PruneOldEpochs(long currentGlobalTurn)
        {
            long minimum = Math.Max(1L, currentGlobalTurn - 64L);
            _startedEpochs.RemoveWhere(epoch => epoch.GlobalTurn < minimum);
        }

        private static string NormalizeId(string value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        internal readonly struct BotTurnEpoch : IEquatable<BotTurnEpoch>
        {
            public BotTurnEpoch(string ownerId, long globalTurn)
            {
                OwnerId = ownerId ?? string.Empty;
                GlobalTurn = globalTurn;
            }

            public string OwnerId { get; }
            public long GlobalTurn { get; }

            public bool Equals(BotTurnEpoch other)
                => GlobalTurn == other.GlobalTurn
                   && string.Equals(OwnerId, other.OwnerId, StringComparison.Ordinal);

            public override bool Equals(object obj)
                => obj is BotTurnEpoch other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((OwnerId != null ? StringComparer.Ordinal.GetHashCode(OwnerId) : 0) * 397)
                           ^ GlobalTurn.GetHashCode();
                }
            }
        }

        private sealed class BotTurnSession : IDisposable
        {
            public BotTurnSession(BotTurnEpoch epoch, CancellationTokenSource cancellation, Task task)
            {
                Epoch = epoch;
                Cancellation = cancellation;
                Task = task ?? Task.CompletedTask;
            }

            public BotTurnEpoch Epoch { get; }
            public CancellationTokenSource Cancellation { get; }
            public Task Task { get; }

            public void Cancel()
            {
                if (!Cancellation.IsCancellationRequested)
                    Cancellation.Cancel();
            }

            public void Dispose()
                => Cancellation.Dispose();
        }

        internal sealed class BotTurnBudget
        {
            public BotTurnBudget(int limit)
            {
                Remaining = Math.Max(0, limit);
            }

            public int Remaining { get; private set; }
            public bool HasRemaining => Remaining > 0;

            public bool TrySpend()
            {
                if (Remaining <= 0)
                    return false;
                Remaining--;
                return true;
            }
        }
    }
}
