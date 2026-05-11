using System;
using System.Linq;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Multiplayer
{
    [TestFixture]
    [Category("SmokeMultiplayer")]
    public sealed class MultiplayerSmokeScenariosTests
    {
        [Test]
        public async Task Smoke_HostScenario_OfflineHostCreatesSession()
        {
            var provider = new OfflineNetworkProvider();
            var result = await provider.HostSessionAsync("smoke-host-room");

            Assert.IsTrue(result.Success);
            Assert.AreEqual("smoke-host-room", result.SessionId);
        }

        [Test]
        public async Task Smoke_JoinScenario_OfflineJoinCreatesSession()
        {
            var provider = new OfflineNetworkProvider();
            var result = await provider.JoinSessionAsync("smoke-join-room");

            Assert.IsTrue(result.Success);
            Assert.AreEqual("smoke-join-room", result.SessionId);
        }

        [Test]
        public void Smoke_StartScenario_CommandSchemaContainsStartSync()
        {
            bool hasStartSync = Enum.GetValues(typeof(GameCommandType))
                .Cast<GameCommandType>()
                .Any(v => v == GameCommandType.MatchStartSync);

            Assert.IsTrue(hasStartSync);
        }

        [Test]
        public void Smoke_ReconnectScenario_QosMonitorTracksReconnects()
        {
            IMultiplayerQosMonitorService monitor = new MultiplayerQosMonitorService();

            monitor.RecordReconnect("websocket", 1);
            monitor.RecordReconnect("websocket", 2);

            var snapshot = monitor.Snapshot;
            Assert.AreEqual(2, snapshot.ReconnectCount);
        }
    }
}
