using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.Turns.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Turns
{
    public sealed class GameplayTurnHudAuthorityPolicyTests
    {
        [Test]
        public void LocalAwaitingInputWithoutBlockers_CanIssueCommandsAndEndTurn()
        {
            var turns = FakeTurnService.Local("player_0");
            GameplayTurnHudAuthoritySnapshot result =
                GameplayTurnHudAuthorityPolicy.Evaluate(turns, Array.Empty<ITurnBlocker>());

            Assert.IsTrue(result.IsLocalOwnerTurn);
            Assert.IsTrue(result.CanIssueLocalCommands);
            Assert.IsTrue(result.CanEndTurn);
            Assert.AreEqual("Ваш хід", result.StatusText);
        }

        [Test]
        public void ForeignHumanTurn_IsReadOnlyAndCannotEndTurn()
        {
            var turns = FakeTurnService.Local("player_0");
            turns.ActiveOwnerIdValue = "player_1";

            GameplayTurnHudAuthoritySnapshot result =
                GameplayTurnHudAuthorityPolicy.Evaluate(turns, Array.Empty<ITurnBlocker>());

            Assert.IsFalse(result.IsLocalOwnerTurn);
            Assert.IsFalse(result.CanIssueLocalCommands);
            Assert.IsFalse(result.CanEndTurn);
            StringAssert.Contains("player_1", result.StatusText);
        }

        [Test]
        public void BotTurn_IsReadOnlyEvenIfOwnerIdsCoincide()
        {
            var turns = FakeTurnService.Local("bot_0");
            turns.IsActiveFactionBotValue = true;

            GameplayTurnHudAuthoritySnapshot result =
                GameplayTurnHudAuthorityPolicy.Evaluate(turns, Array.Empty<ITurnBlocker>());

            Assert.IsFalse(result.IsLocalOwnerTurn);
            Assert.IsFalse(result.CanEndTurn);
            StringAssert.Contains("Хід бота", result.StatusText);
        }

        [Test]
        public void NonInputPhase_IsFailClosed()
        {
            var turns = FakeTurnService.Local("player_0");
            turns.PhaseValue = TurnPhase.Ending;

            GameplayTurnHudAuthoritySnapshot result =
                GameplayTurnHudAuthorityPolicy.Evaluate(turns, Array.Empty<ITurnBlocker>());

            Assert.IsFalse(result.CanIssueLocalCommands);
            Assert.IsFalse(result.CanEndTurn);
            StringAssert.Contains("Завершення", result.StatusText);
        }

        [Test]
        public void BlockingReasons_AreDeduplicatedSortedAndDisableEndTurn()
        {
            var turns = FakeTurnService.Local("player_0");
            var blockers = new ITurnBlocker[]
            {
                new FixedBlocker(true, "zeta"),
                new FixedBlocker(true, "alpha"),
                new FixedBlocker(true, "alpha"),
            };

            GameplayTurnHudAuthoritySnapshot result =
                GameplayTurnHudAuthorityPolicy.Evaluate(turns, blockers);

            Assert.IsFalse(result.CanEndTurn);
            CollectionAssert.AreEqual(new[] { "alpha", "zeta" }, result.BlockingReasons);
            StringAssert.Contains("alpha", result.StatusText);
            StringAssert.Contains("zeta", result.StatusText);
        }

        [Test]
        public void ThrowingBlocker_FailsClosedInsteadOfBreakingHud()
        {
            var turns = FakeTurnService.Local("player_0");
            var blockers = new ITurnBlocker[] { new ThrowingBlocker() };

            GameplayTurnHudAuthoritySnapshot result =
                GameplayTurnHudAuthorityPolicy.Evaluate(turns, blockers);

            Assert.IsFalse(result.CanEndTurn);
            Assert.AreEqual(1, result.BlockingReasons.Count);
            StringAssert.Contains(nameof(ThrowingBlocker), result.BlockingReasons[0]);
        }

        [Test]
        public void MissingLocalOwner_FailsClosed()
        {
            var turns = FakeTurnService.Local(string.Empty);
            turns.ActiveOwnerIdValue = "player_0";

            GameplayTurnHudAuthoritySnapshot result =
                GameplayTurnHudAuthorityPolicy.Evaluate(turns, Array.Empty<ITurnBlocker>());

            Assert.IsFalse(result.CanIssueLocalCommands);
            Assert.IsFalse(result.CanEndTurn);
            StringAssert.Contains("не визначено", result.StatusText);
        }

        private sealed class FixedBlocker : ITurnBlocker
        {
            private readonly bool _blocked;
            private readonly string _reason;

            public FixedBlocker(bool blocked, string reason)
            {
                _blocked = blocked;
                _reason = reason;
            }

            public bool IsTurnBlocked(out string reason)
            {
                reason = _reason;
                return _blocked;
            }
        }

        private sealed class ThrowingBlocker : ITurnBlocker
        {
            public bool IsTurnBlocked(out string reason)
            {
                reason = null;
                throw new InvalidOperationException("synthetic blocker failure");
            }
        }

        private sealed class FakeTurnService : ITurnService
        {
            public static FakeTurnService Local(string ownerId)
            {
                return new FakeTurnService
                {
                    PhaseValue = TurnPhase.AwaitingInput,
                    ActiveOwnerIdValue = ownerId,
                    LocalOwnerIdValue = ownerId,
                };
            }

            public event Action StateChanged { add { } remove { } }
            public TurnPhase Phase => PhaseValue;
            public int Round => 1;
            public long GlobalTurn => 1;
            public int ActionsThisTurn => 0;
            public string ActiveOwnerId => ActiveOwnerIdValue;
            public string LocalOwnerId => LocalOwnerIdValue;
            public bool IsActiveFactionBot => IsActiveFactionBotValue;
            public IReadOnlyList<TurnFaction> Factions => Array.Empty<TurnFaction>();

            public TurnPhase PhaseValue;
            public string ActiveOwnerIdValue;
            public string LocalOwnerIdValue;
            public bool IsActiveFactionBotValue;

            public bool IsOwnerActive(string ownerId)
                => string.Equals(ownerId, ActiveOwnerIdValue, StringComparison.Ordinal);

            public bool CanOwnerAct(string ownerId, out string reason)
            {
                bool allowed = PhaseValue == TurnPhase.AwaitingInput && IsOwnerActive(ownerId);
                reason = allowed ? null : "not allowed";
                return allowed;
            }

            public bool TryRecordAction(string ownerId, string actionId) => false;
            public bool TryEndTurn(string requesterOwnerId, out string reason)
            {
                reason = "not used by policy tests";
                return false;
            }
        }
    }
}
