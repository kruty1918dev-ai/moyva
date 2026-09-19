using Kruty1918.Moyva.Multiplayer.Core;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Multiplayer
{
    /// <summary>
    /// Wire-protocol regression guard: command type numeric values are part of
    /// the network contract and must never drift, or peers on different builds
    /// silently misinterpret each other's commands.
    /// </summary>
    [TestFixture]
    public sealed class GameCommandTypeProtocolTests
    {
        [Test]
        public void CommandTypeValues_MatchProtocolContract()
        {
            Assert.AreEqual(1, (int)GameCommandType.UnitMove);
            Assert.AreEqual(2, (int)GameCommandType.BuildingPlace);
            Assert.AreEqual(3, (int)GameCommandType.BuildingDemolish);
            Assert.AreEqual(4, (int)GameCommandType.UnitSpawn);
            Assert.AreEqual(5, (int)GameCommandType.GameStateChange);
            Assert.AreEqual(6, (int)GameCommandType.StartGame);
            Assert.AreEqual(7, (int)GameCommandType.EndTurn);
            Assert.AreEqual(8, (int)GameCommandType.StartingPositions);
            Assert.AreEqual(9, (int)GameCommandType.QosPing);
            Assert.AreEqual(10, (int)GameCommandType.QosPong);
            Assert.AreEqual(11, (int)GameCommandType.WorldSeedHandshake);
            Assert.AreEqual(12, (int)GameCommandType.MatchStartSync);
            Assert.AreEqual(13, (int)GameCommandType.WorldStateSnapshot);
            Assert.AreEqual(14, (int)GameCommandType.CaravanCommand);
            Assert.AreEqual(15, (int)GameCommandType.CombatCommand);
            Assert.AreEqual(16, (int)GameCommandType.SettlementCaptureCommand);
            Assert.AreEqual(17, (int)GameCommandType.WorldStateSnapshotChunk);
            Assert.AreEqual(18, (int)GameCommandType.UnitVanish);
            Assert.AreEqual(19, (int)GameCommandType.UnitGroupCommand);
            Assert.AreEqual(20, (int)GameCommandType.UnitGroupSync);
            Assert.AreEqual(21, (int)GameCommandType.UnitRecruitmentCommand);
            Assert.AreEqual(22, (int)GameCommandType.UnitRecruitmentSync);
        }
    }
}
