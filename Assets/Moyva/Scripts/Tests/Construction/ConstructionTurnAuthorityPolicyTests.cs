using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Turns.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Construction
{
    [TestFixture]
    public sealed class ConstructionTurnAuthorityPolicyTests
    {
        [Test]
        public void MissingTurnAuthority_FailsClosed()
        {
            var snapshot = new ConstructionTurnAuthoritySnapshot(
                false,
                "player_a",
                "player_a",
                TurnPhase.AwaitingInput);

            Assert.IsFalse(Authorize("player_a", snapshot, true, out _, out string reason));
            StringAssert.Contains("unavailable", reason);
        }

        [Test]
        public void EmptyRequestedOwner_FailsClosed()
        {
            var snapshot = Ready("player_a", "player_a");

            Assert.IsFalse(Authorize("  ", snapshot, true, out _, out string reason));
            StringAssert.Contains("empty", reason);
        }

        [Test]
        public void EmptyActiveOwner_FailsClosed()
        {
            var snapshot = new ConstructionTurnAuthoritySnapshot(
                true,
                string.Empty,
                "player_a",
                TurnPhase.AwaitingInput);

            Assert.IsFalse(Authorize("player_a", snapshot, true, out _, out string reason));
            StringAssert.Contains("Active turn owner", reason);
        }

        [Test]
        public void StartingPhase_FailsClosed()
        {
            var snapshot = new ConstructionTurnAuthoritySnapshot(
                true,
                "player_a",
                "player_a",
                TurnPhase.Starting);

            Assert.IsFalse(Authorize("player_a", snapshot, true, out _, out string reason));
            StringAssert.Contains("AwaitingInput", reason);
        }

        [Test]
        public void EndingPhase_FailsClosed()
        {
            var snapshot = new ConstructionTurnAuthoritySnapshot(
                true,
                "player_a",
                "player_a",
                TurnPhase.Ending);

            Assert.IsFalse(Authorize("player_a", snapshot, true, out _, out string reason));
            StringAssert.Contains("AwaitingInput", reason);
        }

        [Test]
        public void WrongActiveOwner_FailsClosed()
        {
            var snapshot = Ready("player_a", "player_a");

            Assert.IsFalse(Authorize("player_b", snapshot, false, out _, out string reason));
            StringAssert.Contains("not the active turn owner", reason);
        }

        [Test]
        public void InteractiveCommandDuringRemoteOrBotTurn_FailsClosed()
        {
            var snapshot = Ready("bot_1", "player_a");

            Assert.IsFalse(Authorize("bot_1", snapshot, true, out _, out string reason));
            StringAssert.Contains("not the local turn owner", reason);
        }

        [Test]
        public void HostAuthoritativeCommandForRemoteActiveOwner_IsAllowedByLocalOwnerLayer()
        {
            var snapshot = Ready("remote_2", "host_0");

            Assert.IsTrue(Authorize("remote_2", snapshot, false, out string normalized, out string reason));
            Assert.AreEqual("remote_2", normalized);
            Assert.IsNull(reason);
        }

        [Test]
        public void InteractiveCommandWithEmptyLocalOwner_FailsClosed()
        {
            var snapshot = Ready("player_a", string.Empty);

            Assert.IsFalse(Authorize("player_a", snapshot, true, out _, out string reason));
            StringAssert.Contains("Local turn owner", reason);
        }

        [Test]
        public void ValidLocalOwner_NormalizesWhitespaceAndAuthorizes()
        {
            var snapshot = Ready("player_a", "player_a");

            Assert.IsTrue(Authorize("  player_a  ", snapshot, true, out string normalized, out string reason));
            Assert.AreEqual("player_a", normalized);
            Assert.IsNull(reason);
        }

        [Test]
        public void OwnerComparison_IsOrdinalAndCaseSensitive()
        {
            var snapshot = Ready("Player_A", "Player_A");

            Assert.IsFalse(Authorize("player_a", snapshot, true, out _, out _));
        }

        private static bool Authorize(
            string ownerId,
            ConstructionTurnAuthoritySnapshot snapshot,
            bool requireLocalOwner,
            out string normalized,
            out string reason)
            => ConstructionTurnAuthorityPolicy.TryAuthorize(
                ownerId,
                snapshot,
                requireLocalOwner,
                out normalized,
                out reason);

        private static ConstructionTurnAuthoritySnapshot Ready(
            string activeOwnerId,
            string localOwnerId)
            => new ConstructionTurnAuthoritySnapshot(
                true,
                activeOwnerId,
                localOwnerId,
                TurnPhase.AwaitingInput);
    }
}
