using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.BotAI.API;
using Kruty1918.Moyva.BotAI.Runtime;
using Kruty1918.Moyva.Turns.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.BotAI
{
    public sealed class BotTurnExecutorTests
    {
        [Test]
        public void BeginTurn_RejectsWrongEpochAndHumanTurn()
        {
            var turns = FakeTurns.Bot("bot-a", 7);
            var executor = new BotTurnExecutor(turns);

            Assert.IsFalse(executor.TryBeginTurn("bot-a", 6, out string epochReason));
            StringAssert.Contains("epoch mismatch", epochReason);

            turns.IsBot = false;
            Assert.IsFalse(executor.TryBeginTurn("bot-a", 7, out string humanReason));
            StringAssert.Contains("not a bot", humanReason);
            Assert.AreEqual(0, executor.StartedEpochCount);
        }

        [Test]
        public void BeginTurn_IsIdempotentForSameOwnerAndGlobalTurn()
        {
            var turns = FakeTurns.Bot("bot-a", 9);
            var executor = new BotTurnExecutor(turns);

            Assert.IsTrue(executor.TryBeginTurn("bot-a", 9, out string firstReason), firstReason);
            Assert.AreEqual(1, executor.StartedEpochCount);

            Assert.IsTrue(executor.TryBeginTurn("bot-a", 9, out string secondReason), secondReason);
            Assert.AreEqual(1, executor.StartedEpochCount);
        }

        [Test]
        public void Executor_BlocksTurnWhileAsyncActionIsRunning()
        {
            var turns = FakeTurns.Bot("bot-a", 11);
            var action = new DelayedActionExecutor();
            var executor = new BotTurnExecutor(
                turns: turns,
                snapshotBuilder: new StaticSnapshotBuilder(MakeSnapshot(11)),
                memory: null,
                profile: SingleActionProfile(),
                strategicPlanner: new StaticStrategicPlanner(),
                turnPlanner: new StaticTurnPlanner(MakeAction()),
                actionExecutor: action);

            Assert.IsTrue(executor.TryBeginTurn("bot-a", 11, out string startReason), startReason);
            Assert.IsTrue(executor.IsTurnBlocked(out string blockReason));
            StringAssert.Contains("still running", blockReason);

            action.Complete(BotActionExecutionResult.Success("done"));

            Assert.IsFalse(executor.IsTurnBlocked(out string finishedReason), finishedReason);
        }

        [Test]
        public void Executor_DisposeCancelsActiveSession()
        {
            var turns = FakeTurns.Bot("bot-a", 12);
            var action = new DelayedActionExecutor();
            var executor = new BotTurnExecutor(
                turns: turns,
                snapshotBuilder: new StaticSnapshotBuilder(MakeSnapshot(12)),
                memory: null,
                profile: SingleActionProfile(),
                strategicPlanner: new StaticStrategicPlanner(),
                turnPlanner: new StaticTurnPlanner(MakeAction()),
                actionExecutor: action);

            Assert.IsTrue(executor.TryBeginTurn("bot-a", 12, out string reason), reason);

            executor.Dispose();

            Assert.IsTrue(action.LastToken.IsCancellationRequested);
            Assert.IsFalse(executor.IsTurnBlocked(out _));
        }

        [Test]
        public void ActionBudget_NeverSpendsPastConfiguredLimit()
        {
            var budget = new BotTurnExecutor.BotTurnBudget(2);
            Assert.IsTrue(budget.TrySpend());
            Assert.IsTrue(budget.TrySpend());
            Assert.IsFalse(budget.TrySpend());
            Assert.AreEqual(0, budget.Remaining);
            Assert.IsFalse(budget.HasRemaining);
        }

        [Test]
        public void RingCandidates_AreDeterministicUniqueAndExcludeCenter()
        {
            var center = new Vector2Int(3, 4);
            List<Vector2Int> first = BotDeterministicGeometry.BuildRingCandidates(center, 2);
            List<Vector2Int> second = BotDeterministicGeometry.BuildRingCandidates(center, 2);

            CollectionAssert.AreEqual(first, second);
            Assert.AreEqual(24, first.Count);
            Assert.IsFalse(first.Contains(center));
            Assert.AreEqual(first.Count, new HashSet<Vector2Int>(first).Count);
        }

        [Test]
        public void MovePreference_IsStableAndPrioritizesDistanceReduction()
        {
            var current = new Vector2Int(0, 0);
            var enemy = new Vector2Int(4, 1);

            Vector2Int[] first = BotTurnExecutor.BuildPreferredMoveCandidates(current, enemy, 11, 0);
            Vector2Int[] second = BotTurnExecutor.BuildPreferredMoveCandidates(current, enemy, 11, 0);

            CollectionAssert.AreEqual(first, second);
            Assert.AreEqual(Vector2Int.right, first[0]);
        }

        private sealed class FakeTurns : ITurnService
        {
            private readonly List<TurnFaction> _factions = new();

            public static FakeTurns Bot(string ownerId, long globalTurn)
            {
                var result = new FakeTurns
                {
                    Phase = TurnPhase.AwaitingInput,
                    GlobalTurn = globalTurn,
                    Round = 1,
                    ActiveOwnerId = ownerId,
                    LocalOwnerId = "human",
                    IsBot = true,
                };
                result._factions.Add(new TurnFaction(ownerId, true, new Vector2Int(2, 2)));
                return result;
            }

            public event Action StateChanged { add { } remove { } }
            public TurnPhase Phase { get; set; }
            public int Round { get; set; }
            public long GlobalTurn { get; set; }
            public int ActionsThisTurn { get; set; }
            public string ActiveOwnerId { get; set; }
            public string LocalOwnerId { get; set; }
            public bool IsBot { get; set; }
            public bool IsActiveFactionBot => IsBot;
            public IReadOnlyList<TurnFaction> Factions => _factions;

            public bool IsOwnerActive(string ownerId)
                => string.Equals(ownerId, ActiveOwnerId, StringComparison.Ordinal);

            public bool CanOwnerAct(string ownerId, out string reason)
            {
                if (Phase != TurnPhase.AwaitingInput || !IsOwnerActive(ownerId))
                {
                    reason = "not active";
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
                return true;
            }

            public bool TryEndTurn(string requesterOwnerId, out string reason)
            {
                reason = null;
                return IsOwnerActive(requesterOwnerId);
            }
        }

        private static BotPlanningProfile SingleActionProfile()
            => new(
                "test",
                DifficultyLevel.Normal,
                maxDecisionIterations: 1,
                maxSuccessfulMutations: 1,
                maxFailedMutations: 1,
                deterministicNoiseMagnitude: 0,
                minUtilityToAct: 1);

        private static BotWorldSnapshot MakeSnapshot(long globalTurn)
            => new(
                "bot-a",
                round: 1,
                globalTurn,
                TurnPhase.AwaitingInput,
                actionsThisTurn: 0,
                Vector2Int.zero,
                Array.Empty<BotUnitSnapshot>(),
                Array.Empty<BotUnitSnapshot>(),
                Array.Empty<BotBuildingSnapshot>(),
                Array.Empty<Kruty1918.Moyva.Units.API.UnitRecruitmentQueueItemSnapshot>(),
                Array.Empty<BotKnownEntityMemory>());

        private static BotActionCandidate MakeAction()
            => new(
                "test:hold",
                BotActionKind.Hold,
                BotStrategicPosture.Search,
                new BotActionScore(10, "test"));

        private sealed class StaticSnapshotBuilder : IBotWorldSnapshotBuilder
        {
            private readonly BotWorldSnapshot _snapshot;

            public StaticSnapshotBuilder(BotWorldSnapshot snapshot)
            {
                _snapshot = snapshot;
            }

            public BotWorldSnapshot Build(string ownerId, long globalTurn) => _snapshot;
        }

        private sealed class StaticStrategicPlanner : IBotStrategicPlanner
        {
            public BotStrategicContext Plan(BotWorldSnapshot snapshot)
                => new("bot-a", snapshot.GlobalTurn, BotStrategicPosture.Search, 500, "test");
        }

        private sealed class StaticTurnPlanner : IBotTurnPlanner
        {
            private readonly IReadOnlyList<BotActionCandidate> _actions;

            public StaticTurnPlanner(params BotActionCandidate[] actions)
            {
                _actions = actions;
            }

            public IReadOnlyList<BotActionCandidate> GenerateCandidates(
                BotWorldSnapshot snapshot,
                BotStrategicContext strategy)
                => _actions;
        }

        private sealed class DelayedActionExecutor : IBotActionExecutor
        {
            private readonly TaskCompletionSource<BotActionExecutionResult> _completion = new();

            public CancellationToken LastToken { get; private set; }

            public Task<BotActionExecutionResult> ExecuteAsync(
                string ownerId,
                BotActionCandidate action,
                CancellationToken token)
            {
                LastToken = token;
                return _completion.Task;
            }

            public void Complete(BotActionExecutionResult result)
                => _completion.TrySetResult(result);
        }
    }
}
