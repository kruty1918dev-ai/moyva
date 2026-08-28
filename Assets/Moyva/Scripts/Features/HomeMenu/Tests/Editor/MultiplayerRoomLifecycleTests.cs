using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.Runtime.Services;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.HomeMenu
{
    public sealed class MultiplayerRoomLifecycleTests
    {
        [Test]
        public void ProjectGameplayPlayers_AddsLocalHost_WhenLanLobbyUsesHostAlias()
        {
            var lobby = new LobbyRoom(
                lobbyId: "lobby-id",
                lobbyCode: "code",
                name: "Room",
                maxPlayers: 4,
                isPrivate: false,
                hostPlayerId: "lan-host-alias",
                relayJoinCode: "lan:127.0.0.1:7777",
                players: new List<LobbyPlayer>
                {
                    new LobbyPlayer(
                        "lan-host-alias",
                        "Host",
                        isHost: true),
                });

            var players = MultiplayerRoomLifecycle.ProjectGameplayPlayers(
                lobby,
                "session-host",
                localPlayerIsHost: true);

            Assert.That(players, Has.Count.EqualTo(2));
            Assert.That(players[1].PlayerId, Is.EqualTo("session-host"));
            Assert.That(players[1].IsLocal, Is.True);
            Assert.That(players[1].IsHost, Is.True);
        }
    }
}
