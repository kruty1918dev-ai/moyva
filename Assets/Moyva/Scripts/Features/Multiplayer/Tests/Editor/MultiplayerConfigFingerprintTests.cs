using System.Threading.Tasks;
using Kruty1918.Moyva.Jsonization;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Multiplayer
{
    public sealed class MultiplayerConfigFingerprintTests
    {
        [Test]
        public async Task CreatedRoomPublishesLocalJsonFingerprint()
        {
            using var service = new SwitchableLobbyService(CreateConfig(enforce: true));

            LobbyRoom room = await service.CreateRoomAsync(
                new CreateRoomOptions("Test", 2, false, "Host"));

            Assert.That(room.ConfigFingerprint, Is.EqualTo(MoyvaJsonRuntime.ConfigFingerprint));
            Assert.That(room.ConfigFingerprint, Is.Not.Empty);
        }

        [Test]
        public void JoinWithoutFingerprintIsRejectedAndCleanedUp()
        {
            using var service = new SwitchableLobbyService(CreateConfig(enforce: true));

            RoomConfigMismatchException exception = Assert.ThrowsAsync<RoomConfigMismatchException>(
                async () => await service.JoinByCodeAsync("LEGACY", "Client"));

            Assert.That(exception.RemoteFingerprint, Is.Empty);
            Assert.That(service.Current, Is.Null);
            Assert.That(service.State, Is.EqualTo(LobbyState.Closed));
        }

        [Test]
        public async Task ConsistencyFlagCanExplicitlyAllowLegacyRoom()
        {
            using var service = new SwitchableLobbyService(CreateConfig(enforce: false));

            LobbyRoom room = await service.JoinByCodeAsync("LEGACY", "Client");

            Assert.That(room, Is.Not.Null);
            Assert.That(room.ConfigFingerprint, Is.Empty);
        }

        private static MultiplayerConfig CreateConfig(bool enforce)
            => new(
                MultiplayerConfig.CurrentSchemaVersion,
                NetworkProviderType.Offline,
                SessionRules.Default(),
                strictParticipantLock: false,
                enforceConfigConsistency: enforce,
                matchmakingEnabled: false,
                fallbackProviderType: NetworkProviderType.Offline);
    }
}
