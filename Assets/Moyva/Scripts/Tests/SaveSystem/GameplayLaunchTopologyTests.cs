using Kruty1918.Moyva.Bootstrap.Runtime;
using Kruty1918.Moyva.SaveSystem;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.SaveSystem
{
    public sealed class GameplayLaunchTopologyTests
    {
        [Test]
        public void DirectGameplay_AlwaysRequestsHumanPlusBot()
        {
            int count = GameplayLaunchTopology.ResolveStartPositionCount(
                GameLaunchMode.DirectGameplayTest,
                1,
                false,
                0,
                false,
                8);

            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public void DirectGameplay_LocalOwnerIsDeterministic()
        {
            Assert.That(
                GameplayLaunchTopology.ResolveLocalPlayerId(
                    GameLaunchMode.DirectGameplayTest,
                    string.Empty),
                Is.EqualTo("player_0"));
        }

        [Test]
        public void DirectGameplay_SecondSlotIsBot()
        {
            GameplayParticipantSpec human =
                GameplayLaunchTopology.ResolveParticipant(
                    0,
                    true,
                    null,
                    string.Empty,
                    2);

            GameplayParticipantSpec bot =
                GameplayLaunchTopology.ResolveParticipant(
                    1,
                    true,
                    null,
                    string.Empty,
                    2);

            Assert.That(human.ParticipantId, Is.EqualTo("player_0"));
            Assert.That(human.IsBot, Is.False);
            Assert.That(bot.ParticipantId, Is.EqualTo("bot-01"));
            Assert.That(bot.IsBot, Is.True);
        }

        [Test]
        public void MenuSinglePlayer_RemainsSinglePlayer()
        {
            int count = GameplayLaunchTopology.ResolveStartPositionCount(
                GameLaunchMode.MenuNewGame,
                1,
                true,
                1,
                false,
                8);

            Assert.That(count, Is.EqualTo(1));
        }
    }
}
