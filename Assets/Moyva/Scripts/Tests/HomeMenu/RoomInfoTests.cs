using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Networking;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.HomeMenu
{
    [TestFixture]
    public sealed class RoomInfoTests
    {
        [Test]
        public void DisplayKey_UsesJoinCode_WhenAvailable()
        {
            var room = new RoomInfo
            {
                JoinCode = " ABC123 ",
                LobbyId = "lobby-1"
            };

            Assert.AreEqual("ABC123", room.DisplayKey);
            Assert.AreEqual("ABC123", room.DisplayIdentifier);
            Assert.IsTrue(room.HasJoinCode);
            Assert.IsTrue(room.IsJoinable);
        }

        [Test]
        public void DisplayKey_FallsBackToLobbyId_WhenJoinCodeMissing()
        {
            var room = new RoomInfo
            {
                LobbyId = " lobby-1 "
            };

            Assert.AreEqual("lobby-1", room.DisplayKey);
            Assert.AreEqual("LobbyId: lobby-1", room.DisplayIdentifier);
            Assert.IsFalse(room.HasJoinCode);
            Assert.IsTrue(room.HasLobbyId);
            Assert.IsTrue(room.IsJoinable);
        }

        [Test]
        public void IsJoinable_ReturnsFalse_WhenNoIdentifiers()
        {
            var room = new RoomInfo();

            Assert.AreEqual(string.Empty, room.DisplayKey);
            Assert.AreEqual("Код недоступний", room.DisplayIdentifier);
            Assert.IsFalse(room.IsJoinable);
        }

        [Test]
        public void ProviderLabel_ReturnsReadableLabel()
        {
            var room = new RoomInfo { ProviderType = NetworkProviderType.Lan };

            Assert.AreEqual("Local", room.ProviderLabel);
        }
    }
}