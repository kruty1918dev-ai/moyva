using Kruty1918.Moyva.HomeMenu.API;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.HomeMenu
{
    [TestFixture]
    public sealed class JoinRoomResolverTests
    {
        [Test]
        public void FromRoom_UsesJoinCodeFirst()
        {
            var room = new RoomInfo
            {
                JoinCode = " code-1 ",
                LobbyId = "lobby-1"
            };

            var target = JoinRoomResolver.FromRoom(room);

            Assert.AreEqual(JoinRoomTargetKind.JoinCode, target.Kind);
            Assert.AreEqual("code-1", target.Value);
            Assert.IsTrue(target.IsValid);
        }

        [Test]
        public void FromRoom_FallsBackToLobbyId_WhenJoinCodeMissing()
        {
            var room = new RoomInfo
            {
                LobbyId = " lobby-1 "
            };

            var target = JoinRoomResolver.FromRoom(room);

            Assert.AreEqual(JoinRoomTargetKind.LobbyId, target.Kind);
            Assert.AreEqual("lobby-1", target.Value);
            Assert.IsTrue(target.IsValid);
        }

        [Test]
        public void FromRoom_ReturnsNone_WhenNoIdentifierAvailable()
        {
            var target = JoinRoomResolver.FromRoom(new RoomInfo());

            Assert.AreEqual(JoinRoomTargetKind.None, target.Kind);
            Assert.IsFalse(target.IsValid);
        }

        [Test]
        public void FromManualInput_AlwaysTreatsInputAsJoinCode()
        {
            var target = JoinRoomResolver.FromManualInput(" ABC123 ");

            Assert.AreEqual(JoinRoomTargetKind.JoinCode, target.Kind);
            Assert.AreEqual("ABC123", target.Value);
            Assert.IsTrue(target.IsValid);
        }
    }
}