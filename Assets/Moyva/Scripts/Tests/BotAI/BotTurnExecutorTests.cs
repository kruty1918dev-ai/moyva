using System;
using System.Collections.Generic;
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
            List<Vector2Int> first = BotTurnExecutor.BuildRingCandidates(center, 2);
            List<Vector2Int> second = BotTurnExecutor.BuildRingCandidates(center, 2);

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
    }
}
