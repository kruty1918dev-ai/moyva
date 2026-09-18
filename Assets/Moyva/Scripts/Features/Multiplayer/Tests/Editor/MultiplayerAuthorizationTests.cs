using System.Collections.Generic;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Multiplayer
{
    /// <summary>
    /// Host-side authorization rules for remote commands: the transport sender
    /// must be an active participant, and requested owner ids must match the
    /// sender so one client cannot drive another player's units.
    /// </summary>
    [TestFixture]
    public sealed class MultiplayerAuthorizationTests
    {
        private static readonly Participant Host =
            new Participant(new ParticipantIdentity("host-p", "Host"), isHost: true);
        private static readonly Participant Client =
            new Participant(new ParticipantIdentity("client-p", "Client"), isHost: false);

        private static IReadOnlyList<Participant> Participants
            => new[] { Host, Client };

        [Test]
        public void RequestFromActiveParticipant_WithMatchingOwner_IsAuthorized()
        {
            bool ok = MultiplayerAuthorityService.TryResolveAuthorizedRequestOwner(
                Participants,
                "client-p",
                "client-p",
                "client-p",
                out string authorizedOwnerId,
                out _);

            Assert.IsTrue(ok);
            Assert.AreEqual("client-p", authorizedOwnerId);
        }

        [Test]
        public void RequestFromUnknownSender_IsRejected()
        {
            bool ok = MultiplayerAuthorityService.TryResolveAuthorizedRequestOwner(
                Participants,
                "stranger",
                "stranger",
                "stranger",
                out _,
                out string reason);

            Assert.IsFalse(ok);
            StringAssert.Contains("not an active participant", reason);
        }

        [Test]
        public void RequestClaimingAnotherPlayersOwner_IsRejected()
        {
            // A client claiming host ownership must not execute.
            bool ok = MultiplayerAuthorityService.TryResolveAuthorizedRequestOwner(
                Participants,
                "client-p",
                "host-p",
                "host-p",
                out _,
                out string reason);

            Assert.IsFalse(ok);
            StringAssert.Contains("does not match sender", reason);
        }

        [Test]
        public void RequestWithEmptySender_IsRejected()
        {
            Assert.IsFalse(MultiplayerAuthorityService.TryResolveAuthorizedRequestOwner(
                Participants, " ", "x", "x", out _, out _));
        }

        [Test]
        public void RequestWithEmptyRequestedOwner_IsAuthorized()
        {
            // Empty requested owner means "infer sender" — still bound to the
            // transport sender's identity.
            bool ok = MultiplayerAuthorityService.TryResolveAuthorizedRequestOwner(
                Participants,
                "client-p",
                null,
                null,
                out string authorizedOwnerId,
                out _);

            Assert.IsTrue(ok);
            Assert.AreEqual("client-p", authorizedOwnerId);
        }

        [Test]
        public void HostSender_AcceptedAsAuthorizedHost()
        {
            Assert.IsTrue(MultiplayerAuthorityService.IsAuthorizedHostSender(
                Participants, "host-p"));
        }

        [Test]
        public void ClientSender_IsNotAuthorizedHost()
        {
            Assert.IsFalse(MultiplayerAuthorityService.IsAuthorizedHostSender(
                Participants, "client-p"));
        }

        [Test]
        public void UnknownSender_IsNotAuthorizedHost()
        {
            Assert.IsFalse(MultiplayerAuthorityService.IsAuthorizedHostSender(
                Participants, "ghost"));
        }

        [Test]
        public void NullParticipantList_NoHostAuthorized()
        {
            Assert.IsFalse(MultiplayerAuthorityService.IsAuthorizedHostSender(
                null, "host-p"));
        }

        [Test]
        public void UnitCommandAuthorization_RequiresSameOwner()
        {
            Assert.IsTrue(
                MultiplayerAuthorityService.IsUnitCommandAuthorized("p1", "p1"));
            Assert.IsTrue(
                MultiplayerAuthorityService.IsUnitCommandAuthorized(" p1 ", "p1"));
            Assert.IsFalse(
                MultiplayerAuthorityService.IsUnitCommandAuthorized("p1", "p2"));
            Assert.IsFalse(
                MultiplayerAuthorityService.IsUnitCommandAuthorized("", "p1"));
            Assert.IsFalse(
                MultiplayerAuthorityService.IsUnitCommandAuthorized("p1", null));
        }
    }
}
