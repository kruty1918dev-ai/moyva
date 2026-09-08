using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.Runtime;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.HomeMenu
{
    public sealed class LobbyInviteCodeResolverTests
    {
        [Test]
        public void Resolve_ForRelay_UsesLobbyCode()
        {
            var lobby = CreateLobby(lobbyCode: "UGS123", relayJoinCode: "LAN456");

            var presentation = LobbyInviteCodeResolver.Resolve(lobby, NetworkProviderType.Relay);

            Assert.That(presentation.Label, Is.EqualTo("Invite Code"));
            Assert.That(presentation.Code, Is.EqualTo("UGS123"));
        }

        [Test]
        public void Resolve_ForLan_UsesShortLobbyCode()
        {
            var lobby = CreateLobby(lobbyCode: "UGS123", relayJoinCode: "LAN456");

            var presentation = LobbyInviteCodeResolver.Resolve(lobby, NetworkProviderType.Lan);

            Assert.That(presentation.Label, Is.EqualTo("LAN Join Code"));
            Assert.That(presentation.Code, Is.EqualTo("UGS123"));
        }

        private static LobbyRoom CreateLobby(string lobbyCode, string relayJoinCode)
        {
            return new LobbyRoom(
                lobbyId: "lobby-id",
                lobbyCode: lobbyCode,
                name: "Room",
                maxPlayers: 4,
                isPrivate: false,
                hostPlayerId: "host",
                relayJoinCode: relayJoinCode,
                players: new List<LobbyPlayer>());
        }
    }
}
