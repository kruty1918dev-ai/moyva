using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Multiplayer
{
    [TestFixture]
    public class SessionRulesTests
    {
        [Test]
        public void Default_ShouldHaveMaxParticipants4()
        {
            var rules = SessionRules.Default();
            Assert.AreEqual(4, rules.MaxParticipants);
        }

        [Test]
        public void Default_ShouldNotHaveStrictLock()
        {
            var rules = SessionRules.Default();
            Assert.IsFalse(rules.StrictParticipantLock);
        }

        [Test]
        public void Constructor_SetsAllProperties()
        {
            var rules = new SessionRules(
                SessionMode.MixedHumansAndBots,
                maxParticipants: 4,
                maxHumans: 2,
                maxBots: 2,
                allowBotsFallbackOnLeave: true,
                allowMatchSaveForAnalysis: true,
                strictParticipantLock: true);

            Assert.AreEqual(SessionMode.MixedHumansAndBots, rules.Mode);
            Assert.AreEqual(4, rules.MaxParticipants);
            Assert.AreEqual(2, rules.MaxHumans);
            Assert.AreEqual(2, rules.MaxBots);
            Assert.IsTrue(rules.AllowBotsFallbackOnLeave);
            Assert.IsTrue(rules.AllowMatchSaveForAnalysis);
            Assert.IsTrue(rules.StrictParticipantLock);
        }
    }
}
