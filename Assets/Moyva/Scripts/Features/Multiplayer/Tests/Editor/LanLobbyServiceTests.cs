using System.Net;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Multiplayer
{
    public sealed class LanLobbyServiceTests
    {
        [Test]
        public void BuildShortLanLobbyCode_ReturnsSixDigits()
        {
            var code = LanLobbyService.BuildShortLanLobbyCode("5c4db7e48e5448fdaee1d149ac8047f8");

            Assert.That(code, Has.Length.EqualTo(6));
            Assert.That(LanLobbyService.IsShortLanLobbyCode(code), Is.True);
        }

        [Test]
        public void TryParsePayload_UsesAdvertisedShortLobbyCode()
        {
            var payload = string.Join(
                "|",
                "MOYVA_LAN_LOBBY_V1",
                "5c4db7e48e5448fdaee1d149ac8047f8",
                "Room",
                "4",
                "192.168.1.10",
                "54545",
                "host",
                "Host",
                string.Empty,
                "0",
                string.Empty,
                "0",
                string.Empty,
                "fingerprint",
                "482731");

            var parsed = LanLobbyService.TryParsePayload(payload, out var room, out var joinCode);

            Assert.That(parsed, Is.True);
            Assert.That(room.LobbyCode, Is.EqualTo("482731"));
            Assert.That(joinCode, Is.EqualTo("lan:192.168.1.10:54545"));
        }

        [Test]
        public async System.Threading.Tasks.Task CreateRoomAsync_AssignsShortLobbyCodeAndKeepsTransportJoinCode()
        {
            using var service = new LanLobbyService();

            var room = await service.CreateRoomAsync(
                new CreateRoomOptions(
                    "Room",
                    4,
                    false,
                    "Host",
                    relayJoinCode: "lan:127.0.0.1:54545"));

            Assert.That(LanLobbyService.IsShortLanLobbyCode(room.LobbyCode), Is.True);
            Assert.That(room.RelayJoinCode, Is.EqualTo("lan:127.0.0.1:54545"));
        }
    }
}
