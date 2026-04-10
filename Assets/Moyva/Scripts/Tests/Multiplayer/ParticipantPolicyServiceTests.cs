using System.Collections.Generic;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Persistence;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Multiplayer
{
    [TestFixture]
    public class ParticipantPolicyServiceTests
    {
        private sealed class FakeLogger : IMultiplayerLogger
        {
            public void Info(string msg) { }
            public void Warn(string msg) { }
            public void Error(string msg) { }
            public void Trace(string msg) { }
        }

        private sealed class FakeSnapshotStore : IWorldSnapshotStore
        {
            public bool Exists(string worldId) => false;
            public WorldSnapshot Load(string worldId) => null;
            public void Save(WorldSnapshot snapshot) { }
        }

        private ParticipantPolicyService _service;
        private SessionRules _defaultRules;

        [SetUp]
        public void SetUp()
        {
            _service = new ParticipantPolicyService(new FakeLogger(), new FakeSnapshotStore());
            _defaultRules = SessionRules.Default();
        }

        [Test]
        public void CanJoin_ShouldReturnTrue_WhenSessionIsEmpty()
        {
            var identity = new ParticipantIdentity("p1", "Player1");
            var result = _service.CanJoin(identity, new List<Participant>(), _defaultRules, null);
            Assert.IsTrue(result);
        }

        [Test]
        public void CanJoin_ShouldReturnFalse_WhenMaxParticipantsReached()
        {
            var rules = new SessionRules(SessionMode.MultiplayerHumans, 4, 4, 0, false, false, false);
            var participants = new List<Participant>
            {
                new Participant(new ParticipantIdentity("p1", "P1"), false, true),
                new Participant(new ParticipantIdentity("p2", "P2"), false, false),
                new Participant(new ParticipantIdentity("p3", "P3"), false, false),
                new Participant(new ParticipantIdentity("p4", "P4"), false, false),
            };

            var candidate = new ParticipantIdentity("p5", "P5");
            var result = _service.CanJoin(candidate, participants, rules, null);
            Assert.IsFalse(result);
        }

        [Test]
        public void CanJoin_ShouldReturnFalse_WhenMaxHumansReached()
        {
            var rules = new SessionRules(SessionMode.MixedHumansAndBots, 4, 2, 2, false, false, false);
            var participants = new List<Participant>
            {
                new Participant(new ParticipantIdentity("p1", "P1"), false, true),
                new Participant(new ParticipantIdentity("p2", "P2"), false, false),
            };

            var candidate = new ParticipantIdentity("p3", "P3");
            var result = _service.CanJoin(candidate, participants, rules, null);
            Assert.IsFalse(result);
        }

        [Test]
        public void CanJoin_ShouldReturnFalse_WhenMaxBotsReached()
        {
            var rules = new SessionRules(SessionMode.MixedHumansAndBots, 4, 2, 1, false, false, false);
            var participants = new List<Participant>
            {
                new Participant(new ParticipantIdentity("BOT_1", "Bot1"), true, false),
            };

            var botCandidate = new ParticipantIdentity("BOT_2", "Bot2");
            var result = _service.CanJoin(botCandidate, participants, rules, null);
            Assert.IsFalse(result);
        }

        [Test]
        public void CanJoin_ShouldAllowBot_WhenBotSlotAvailable()
        {
            var rules = new SessionRules(SessionMode.MixedHumansAndBots, 4, 2, 2, false, false, false);
            var participants = new List<Participant>
            {
                new Participant(new ParticipantIdentity("BOT_1", "Bot1"), true, false),
            };

            var botCandidate = new ParticipantIdentity("BOT_2", "Bot2");
            var result = _service.CanJoin(botCandidate, participants, rules, null);
            Assert.IsTrue(result);
        }

        [Test]
        public void CanJoin_HumansAndBots_ShouldNotExceedMaxParticipants()
        {
            var rules = new SessionRules(SessionMode.MixedHumansAndBots, 3, 2, 2, false, false, false);
            var participants = new List<Participant>
            {
                new Participant(new ParticipantIdentity("p1", "P1"), false, true),
                new Participant(new ParticipantIdentity("p2", "P2"), false, false),
                new Participant(new ParticipantIdentity("BOT_1", "Bot1"), true, false),
            };

            var candidate = new ParticipantIdentity("p3", "P3");
            var result = _service.CanJoin(candidate, participants, rules, null);
            Assert.IsFalse(result);
        }
    }
}
