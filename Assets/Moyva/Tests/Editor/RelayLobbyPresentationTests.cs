using System.Reflection;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Startup
{
    public sealed class RelayLobbyPresentationTests
    {
        [TestCase("ABC123", "ABC123")]
        [TestCase("", "")]
        public void RelayInviteUsesLobbyCodeAndNeverInternalId(string lobbyCode, string expected)
        {
            var room = new LobbyRoom("0f5689d0-989a-4d10-a9da-91dbfb558a64", lobbyCode,
                "My Kingdom", 4, false, "host", "BCDFGH", null);
            var resolver = typeof(LobbyInviteCodePresentation).Assembly.GetType(
                "Kruty1918.Moyva.HomeMenu.Runtime.LobbyInviteCodeResolver");
            var result = (LobbyInviteCodePresentation)resolver.GetMethod("Resolve",
                BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { room, NetworkProviderType.Relay });
            Assert.That(result.Code, Is.EqualTo(expected));
            Assert.That(result.RoomName, Is.EqualTo("My Kingdom"));
            Assert.That(result.DisplayText, Does.Not.Contain(room.LobbyId));
            Assert.That(result.DisplayText, Does.Not.Contain(room.RelayJoinCode));
        }
    }
}
