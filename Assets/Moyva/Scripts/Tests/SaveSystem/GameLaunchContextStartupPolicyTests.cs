using Kruty1918.Moyva.SaveSystem;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.SaveSystem
{
    public sealed class GameLaunchContextStartupPolicyTests
    {
        [SetUp]
        public void SetUp()
        {
            GameLaunchContext.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            GameLaunchContext.Reset();
        }

        [Test]
        public void DirectGameplayTest_HasExplicitSourceAndSoloCapacity()
        {
            GameLaunchContext.ConfigureDirectGameplayTest();

            Assert.That(GameLaunchContext.Mode, Is.EqualTo(GameLaunchMode.DirectGameplayTest));
            Assert.That(GameLaunchContext.Source, Is.EqualTo(GameLaunchSource.DirectGameplayTest));
            Assert.That(GameLaunchContext.MaxPlayers, Is.EqualTo(1));
            Assert.That(GameLaunchContext.HasWorldSettings, Is.False);
            Assert.That(GameLaunchContext.IsAutoLoadEnabled(), Is.False);
            Assert.That(GameLaunchContext.IsAutoSaveEnabled(), Is.False);
        }

        [Test]
        public void MenuNewGame_IsAttributedToHomeMenu()
        {
            GameLaunchContext.ConfigureMenuNewGame(2);

            Assert.That(GameLaunchContext.Mode, Is.EqualTo(GameLaunchMode.MenuNewGame));
            Assert.That(GameLaunchContext.Source, Is.EqualTo(GameLaunchSource.HomeMenu));
        }

        [Test]
        public void MenuLoadGame_IsAttributedToSaveLoad()
        {
            GameLaunchContext.ConfigureMenuLoadGame(3);

            Assert.That(GameLaunchContext.Mode, Is.EqualTo(GameLaunchMode.MenuLoadGame));
            Assert.That(GameLaunchContext.Source, Is.EqualTo(GameLaunchSource.SaveLoad));
        }

        [Test]
        public void MultiplayerAndJoin_AreAttributedToHomeMenu()
        {
            GameLaunchContext.ConfigureMenuMultiplayerGame(
                "test",
                123,
                0,
                0,
                0,
                4,
                false);

            Assert.That(GameLaunchContext.Source, Is.EqualTo(GameLaunchSource.HomeMenu));

            GameLaunchContext.ConfigureMenuJoinGame();
            Assert.That(GameLaunchContext.Source, Is.EqualTo(GameLaunchSource.HomeMenu));
        }

        [Test]
        public void Reset_ClearsLaunchSource()
        {
            GameLaunchContext.ConfigureDirectGameplayTest();
            GameLaunchContext.Reset();

            Assert.That(GameLaunchContext.Mode, Is.EqualTo(GameLaunchMode.Unknown));
            Assert.That(GameLaunchContext.Source, Is.EqualTo(GameLaunchSource.Unknown));
            Assert.That(GameLaunchContext.MaxPlayers, Is.EqualTo(0));
        }
    }
}
