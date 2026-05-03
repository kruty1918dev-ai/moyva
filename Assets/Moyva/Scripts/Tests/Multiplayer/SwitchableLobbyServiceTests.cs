using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Multiplayer
{
    [TestFixture]
    public sealed class SwitchableLobbyServiceTests
    {
        private sealed class FakeLogger : IMultiplayerLogger
        {
            public List<string> Warnings { get; } = new List<string>();
            public void Info(string msg) { }
            public void Warn(string msg) => Warnings.Add(msg);
            public void Error(string msg) { }
            public void Trace(string msg) { }
        }

        [Test]
        public void Constructor_UsesConfiguredProvider()
        {
            var service = CreateService(NetworkProviderType.Offline, out _);
            try
            {
                Assert.AreEqual(NetworkProviderType.Offline, service.RequestedProviderType);
                Assert.AreEqual(NetworkProviderType.Offline, service.CurrentProviderType);
            }
            finally
            {
                service.Dispose();
            }
        }

        [Test]
        public async Task SwitchToAsync_UpdatesRequestedAndEffectiveProvider()
        {
            var service = CreateService(NetworkProviderType.Offline, out _);
            try
            {
                await service.SwitchToAsync(NetworkProviderType.Lan);

                Assert.AreEqual(NetworkProviderType.Lan, service.RequestedProviderType);
                Assert.AreEqual(NetworkProviderType.Lan, service.CurrentProviderType);
            }
            finally
            {
                service.Dispose();
            }
        }

        [Test]
        public async Task SwitchToAsync_RehooksLobbyUpdatedEvent()
        {
            var service = CreateService(NetworkProviderType.Lan, out _);
            var updates = 0;
            service.LobbyUpdated += _ => updates++;

            try
            {
                await service.SwitchToAsync(NetworkProviderType.Offline);
                await service.CreateRoomAsync(new CreateRoomOptions("Room", 4, false, "Player"));

                Assert.AreEqual(1, updates);
            }
            finally
            {
                service.Dispose();
            }
        }

        [Test]
        public void LanPayloadParser_RejectsLegacyPayloadWithoutProtocolMarker()
        {
            var parsed = TryParseLanPayload("room-id|Room|4|127.0.0.1|54545|host-id|Host|", out _, out _);

            Assert.IsFalse(parsed);
        }

        [Test]
        public void LanPayloadParser_AcceptsMoyvaLanPayload()
        {
            var parsed = TryParseLanPayload("MOYVA_LAN_LOBBY_V1|room-id-12345678|Room|4|127.0.0.1|54545|host-id|Host|", out var room, out var joinCode);

            Assert.IsTrue(parsed);
            Assert.AreEqual("room-id-12345678", room.LobbyId);
            Assert.AreEqual("room-id-", room.LobbyCode);
            Assert.AreEqual("lan:127.0.0.1:54545", joinCode);
            Assert.AreEqual("lan:127.0.0.1:54545", room.RelayJoinCode);
        }

        private static SwitchableLobbyService CreateService(NetworkProviderType providerType, out FakeLogger logger)
        {
            logger = new FakeLogger();
            var config = new MultiplayerConfig(
                MultiplayerConfig.CurrentSchemaVersion,
                providerType,
                SessionRules.Default(),
                strictParticipantLock: false,
                enforceConfigConsistency: true,
                matchmakingEnabled: false,
                fallbackProviderType: NetworkProviderType.Offline);

            return new SwitchableLobbyService(config, logger);
        }

        private static bool TryParseLanPayload(string payload, out LobbyRoom room, out string joinCode)
        {
            var method = typeof(LanLobbyService).GetMethod("TryParsePayload", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);

            var arguments = new object[] { payload, null, null };
            var parsed = (bool)method.Invoke(null, arguments);
            room = (LobbyRoom)arguments[1];
            joinCode = (string)arguments[2];
            return parsed;
        }
    }
}