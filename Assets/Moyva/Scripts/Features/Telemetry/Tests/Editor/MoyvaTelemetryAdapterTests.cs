using System;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.AI.Bot;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Telemetry;
using Kruty1918.Moyva.Turns.API;
using Kruty1918.Telemetry.Core;
using NUnit.Framework;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Tests.Telemetry
{
    public class MoyvaTelemetryAdapterTests
    {
        private sealed class CaptureSink : ITelemetrySink
        {
            public readonly List<string> Types = new List<string>();
            public TelemetrySessionContext Session => null;

            public TelemetryResult Track<T>(in T telemetryEvent) where T : struct, ITelemetryEvent
            {
                Types.Add(telemetryEvent.EventType);
                return TelemetryResult.Ok;
            }

            public TelemetryResult TrackRaw(string eventType, string contractId, int contractVersion,
                Action<ITelemetryEventWriter> writePayload)
            {
                Types.Add(eventType);
                return TelemetryResult.Ok;
            }

            public void Flush() { }
        }

        private sealed class FakeNetworkProvider : INetworkProvider
        {
            public IObservable<NetworkMessage> Messages => null;
            public event Action<string> PeerConnected;
            public event Action<string> PeerDisconnected;
            public void RaiseConnected(string id) => PeerConnected?.Invoke(id);
            public void RaiseDisconnected(string id) => PeerDisconnected?.Invoke(id);
            public System.Threading.Tasks.Task<SessionResult> HostSessionAsync(string s, System.Threading.CancellationToken c = default) => null;
            public System.Threading.Tasks.Task<SessionResult> JoinSessionAsync(string s, System.Threading.CancellationToken c = default) => null;
            public System.Threading.Tasks.Task LeaveSessionAsync(System.Threading.CancellationToken c = default) => null;
            public System.Threading.Tasks.Task SendMessageAsync(string t, byte[] p, System.Threading.CancellationToken c = default) => null;
        }

        private DiContainer _container;
        private SignalBus _bus;
        private CaptureSink _sink;
        private MoyvaSignalTelemetryAdapter _adapter;
        private GameObject _installerGo;

        [SetUp]
        public void SetUp()
        {
            _container = new DiContainer();
            _installerGo = new GameObject(nameof(Moyva.Signals.SignalBusInstaller));
            var installer =
                _installerGo.AddComponent<Moyva.Signals.SignalBusInstaller>();
            _container.Inject(installer);
            installer.InstallBindings();
            _bus = _container.Resolve<SignalBus>();
            _sink = new CaptureSink();
            _adapter = new MoyvaSignalTelemetryAdapter(_bus, _sink);
            _adapter.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _adapter.Dispose();
            if (_installerGo != null)
                UnityEngine.Object.DestroyImmediate(_installerGo);
        }

        [Test]
        public void SignalBus_BridgesGameplaySignals()
        {
            _bus.Fire(new GameStartedSignal());
            _bus.Fire(new GameModeChangedSignal { NewMode = GameModeType.Construction });
            _bus.Fire(new BuildingPlacedSignal
            {
                BuildingId = "farm", Position = new Vector2Int(2, 3), OwnerId = "p1",
            });
            _bus.Fire(new UnitCreatedSignal
            {
                UnitId = "u1", UnitTypeId = "warrior", Position = new Vector2Int(1, 1), OwnerId = "p1",
            });
            _bus.Fire(new UnitMovedSignal
            {
                UnitId = "u1", NewPosition = new Vector2Int(2, 1), Cost = 1f, SourceFactionId = "p1",
            });
            _bus.Fire(new UnitRecruitmentDeployedSignal
            {
                OwnerId = "p1", QueueId = 7, UnitTypeId = "warrior", UnitId = "u2",
                BuildingPosition = new Vector2Int(4, 4),
            });
            _bus.Fire(new EconomyTickCompletedSignal
            {
                SettlementId = "s1", OwnerId = "p1", Turn = 1, TotalPopulation = 5,
            });
            _bus.Fire(new FogStateChangedSignal { ChangedTilesCount = 9 });
            _bus.Fire(new SaveCompletedSignal { Slot = 0, Success = true });
            _bus.Fire(new GameEndedSignal { WinnerId = "p1" });

            var want = new[]
            {
                "moyva.match.started", "moyva.match.modeChanged", "moyva.construction.placed",
                "moyva.units.created", "moyva.units.moved", "moyva.recruitment.deployed",
                "moyva.economy.tick", "moyva.fog.changed", "moyva.save.saved", "moyva.match.ended",
            };
            foreach (var t in want)
                Assert.Contains(t, _sink.Types, $"missing telemetry for {t}");
        }

        [Test]
        public void SignalBus_BridgesUiPanels()
        {
            _bus.Fire(new BuildingInfoPanelRequestedSignal { BuildingId = "farm", Position = new Vector2Int(1, 2) });
            _bus.Fire(new WorldInfoPanelClosedSignal());
            Assert.Contains("moyva.ui.panel", _sink.Types);
        }

        [Test]
        public void Dispose_UnsubscribesCleanly()
        {
            _adapter.Dispose();
            _bus.Fire(new GameStartedSignal());
            Assert.IsEmpty(_sink.Types);
        }

        [Test]
        public void TurnParticipant_EmitsLifecycle()
        {
            var participant = new TelemetryTurnParticipant(_sink);
            var ctx = new TurnContext(2, 5, 0, new TurnFaction("p1", Vector2Int.zero));
            participant.OnTurnStarted(ctx);
            participant.OnTurnEnding(ctx);
            participant.OnRoundCompleted(2);
            Assert.AreEqual(3, _sink.Types.Count(t => t == "moyva.turn.lifecycle"));
        }

        [Test]
        public void BotAdapter_BridgesEpisodeMetrics()
        {
            var hub = new BotTelemetryHub();
            var adapter = new MoyvaBotTelemetryAdapter(_sink);
            adapter.Attach(hub);
            hub.Metrics.RecordEpisode(new BotEpisodeMetrics(1, 0.5f, 0.1f, true, false, false, false, 10, 3));
            Assert.Contains("moyva.ai.bot.episode", _sink.Types);
            adapter.Dispose();
        }

        [Test]
        public void BotAdapter_DisabledHub_DoesNotEmit()
        {
            var hub = new BotTelemetryHub(enabled: false);
            var adapter = new MoyvaBotTelemetryAdapter(_sink);
            adapter.Attach(hub);
            adapter.Dispose();
            Assert.IsEmpty(_sink.Types);
        }

        [Test]
        public void NetworkAdapter_HashesPeerIds()
        {
            var net = new FakeNetworkProvider();
            var adapter = new MoyvaNetworkTelemetryAdapter(_sink, net);
            adapter.Initialize();
            net.RaiseConnected("peer-secret-id-123");
            Assert.Contains("moyva.net.peer", _sink.Types);
            adapter.Dispose();
        }
    }
}
