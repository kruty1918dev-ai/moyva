using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Persistence;
using Kruty1918.Moyva.Multiplayer.Runtime;
using Kruty1918.Moyva.Signals;
using NUnit.Framework;
using Zenject;

namespace Kruty1918.Moyva.Tests.Multiplayer
{
    [TestFixture]
    public class MultiplayerWrapperIntegrationTests
    {
        private sealed class FakeLogger : IMultiplayerLogger
        {
            public readonly List<string> Infos = new List<string>();
            public readonly List<string> Warnings = new List<string>();
            public readonly List<string> Errors = new List<string>();

            public void Info(string msg) => Infos.Add(msg);
            public void Warn(string msg) => Warnings.Add(msg);
            public void Error(string msg) => Errors.Add(msg);
            public void Trace(string msg) { }
        }

        private sealed class MessageBus : IObservable<NetworkMessage>
        {
            private readonly List<IObserver<NetworkMessage>> _observers = new List<IObserver<NetworkMessage>>();

            public IDisposable Subscribe(IObserver<NetworkMessage> observer)
            {
                _observers.Add(observer);
                return new Unsubscriber(_observers, observer);
            }

            public void Publish(NetworkMessage message)
            {
                var snapshot = _observers.ToArray();
                foreach (var observer in snapshot)
                    observer.OnNext(message);
            }

            private sealed class Unsubscriber : IDisposable
            {
                private readonly List<IObserver<NetworkMessage>> _observers;
                private readonly IObserver<NetworkMessage> _observer;

                public Unsubscriber(List<IObserver<NetworkMessage>> observers, IObserver<NetworkMessage> observer)
                {
                    _observers = observers;
                    _observer = observer;
                }

                public void Dispose()
                {
                    _observers.Remove(_observer);
                }
            }
        }

        private sealed class FakeNetworkProvider : INetworkProvider
        {
            private readonly MessageBus _messages = new MessageBus();

            public int HostCalls { get; private set; }
            public int JoinCalls { get; private set; }
            public int SendCalls { get; private set; }

            public Func<string, SessionResult> HostResultFactory { get; set; } = sid => SessionResult.Ok(sid);
            public Func<string, SessionResult> JoinResultFactory { get; set; } = sid => SessionResult.Ok(sid);

            public string LastTargetPeerId { get; private set; }
            public byte[] LastPayload { get; private set; }

            public IObservable<NetworkMessage> Messages => _messages;
            public event Action<string> PeerConnected;
            public event Action<string> PeerDisconnected;

            public Task<SessionResult> HostSessionAsync(string sessionId, CancellationToken ct = default)
            {
                HostCalls++;
                return Task.FromResult(HostResultFactory(sessionId));
            }

            public Task<SessionResult> JoinSessionAsync(string sessionId, CancellationToken ct = default)
            {
                JoinCalls++;
                return Task.FromResult(JoinResultFactory(sessionId));
            }

            public Task LeaveSessionAsync(CancellationToken ct = default)
            {
                return Task.CompletedTask;
            }

            public Task SendMessageAsync(string targetPeerId, byte[] payload, CancellationToken ct = default)
            {
                SendCalls++;
                LastTargetPeerId = targetPeerId;
                LastPayload = payload;
                return Task.CompletedTask;
            }

            public void PushIncoming(string senderId, byte[] payload)
            {
                _messages.Publish(new NetworkMessage(senderId, payload));
            }

            public void SimulatePeerConnected(string peerId) => PeerConnected?.Invoke(peerId);
            public void SimulatePeerDisconnected(string peerId) => PeerDisconnected?.Invoke(peerId);
        }

        private sealed class FakeGameCommandSyncService : IGameCommandSyncService
        {
            public GameCommandType? LastSentType { get; private set; }
            public byte[] LastSentPayload { get; private set; }
            public int SendCount { get; private set; }
            public Action<string, byte[]> StartingPositionsHandler { get; private set; }

            public void SendCommand(GameCommandType type, byte[] payload)
            {
                LastSentType = type;
                LastSentPayload = payload;
                SendCount++;
            }

            public void RegisterHandler(GameCommandType type, Action<string, byte[]> handler)
            {
                if (type == GameCommandType.StartingPositions)
                    StartingPositionsHandler = handler;
            }

            public void PushIncoming(string senderId, byte[] payload)
            {
                StartingPositionsHandler?.Invoke(senderId, payload);
            }
        }

        private sealed class FakeFailurePolicy : IFailureHandlingPolicy
        {
            public bool HandleRecoverable(FailureCategory cat, string details) => false;
            public void HandleNonRecoverable(FailureCategory cat, string details) { }
        }

        [Test]
        public async Task FallbackProvider_HostFailure_SwitchesToFallbackAndRetries()
        {
            var logger = new FakeLogger();
            var primary = new FakeNetworkProvider { HostResultFactory = _ => SessionResult.Fail("primary down") };
            var fallback = new FakeNetworkProvider { HostResultFactory = sid => SessionResult.Ok($"fb-{sid}") };
            var provider = new FallbackNetworkProvider(primary, fallback, logger);

            var result = await provider.HostSessionAsync("room-1");

            Assert.IsTrue(result.Success);
            Assert.AreEqual("fb-room-1", result.SessionId);
            Assert.AreEqual(1, primary.HostCalls);
            Assert.AreEqual(1, fallback.HostCalls);
        }

        [Test]
        public async Task FallbackProvider_Reset_ReturnsTrafficToPrimary()
        {
            var logger = new FakeLogger();
            var primary = new FakeNetworkProvider { HostResultFactory = _ => SessionResult.Fail("primary down") };
            var fallback = new FakeNetworkProvider { HostResultFactory = sid => SessionResult.Ok($"fb-{sid}") };
            var provider = new FallbackNetworkProvider(primary, fallback, logger);

            await provider.HostSessionAsync("room-a");
            provider.Reset();

            primary.HostResultFactory = sid => SessionResult.Ok($"p-{sid}");
            var result = await provider.HostSessionAsync("room-b");

            Assert.IsTrue(result.Success);
            Assert.AreEqual("p-room-b", result.SessionId);
            Assert.AreEqual(2, primary.HostCalls);
            Assert.AreEqual(1, fallback.HostCalls);
        }

        [Test]
        public async Task FallbackProvider_JoinFailure_DoesNotSwitchToFallback()
        {
            var logger = new FakeLogger();
            var primary = new FakeNetworkProvider { JoinResultFactory = _ => SessionResult.Fail("primary join failed") };
            var fallback = new FakeNetworkProvider { JoinResultFactory = sid => SessionResult.Ok($"fb-{sid}") };
            var provider = new FallbackNetworkProvider(primary, fallback, logger);

            var result = await provider.JoinSessionAsync("room-join");

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, primary.JoinCalls);
            Assert.AreEqual(0, fallback.JoinCalls);
        }

        [Test]
        public async Task FallbackProvider_JoinException_DoesNotSwitchToFallback()
        {
            var logger = new FakeLogger();
            var primary = new FakeNetworkProvider { JoinResultFactory = _ => throw new InvalidOperationException("join exception") };
            var fallback = new FakeNetworkProvider { JoinResultFactory = sid => SessionResult.Ok($"fb-{sid}") };
            var provider = new FallbackNetworkProvider(primary, fallback, logger);

            var result = await provider.JoinSessionAsync("room-join");

            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, primary.JoinCalls);
            Assert.AreEqual(0, fallback.JoinCalls);
            Assert.IsTrue(result.ErrorMessage.Contains("join exception"));
        }

        [Test]
        public void SwitchableNetworkProvider_RelayDisabled_ReportsOfflineEffectiveType()
        {
            var logger = new FakeLogger();
            var config = new MultiplayerConfig(
                MultiplayerConfig.CurrentSchemaVersion,
                NetworkProviderType.Relay,
                SessionRules.Default(),
                strictParticipantLock: false,
                enforceConfigConsistency: false,
                matchmakingEnabled: false,
                enableRelayProvider: false);

            using var provider = new SwitchableNetworkProvider(config, logger);

            Assert.AreEqual(NetworkProviderType.Relay, provider.RequestedType);
            Assert.AreEqual(NetworkProviderType.Offline, provider.CurrentType);
        }

        [Test]
        public void GameCommandSync_SendCommand_BuildsPacketAndBroadcasts()
        {
            var logger = new FakeLogger();
            var network = new FakeNetworkProvider();
            using var sync = new GameCommandSyncService(network, logger);

            sync.SendCommand(GameCommandType.UnitMove, new byte[] { 10, 20, 30 });

            Assert.AreEqual(1, network.SendCalls);
            Assert.AreEqual("*", network.LastTargetPeerId);
            Assert.AreEqual((byte)GameCommandType.UnitMove, network.LastPayload[0]);
            Assert.AreEqual(4, network.LastPayload.Length);
            Assert.AreEqual(10, network.LastPayload[1]);
            Assert.AreEqual(20, network.LastPayload[2]);
            Assert.AreEqual(30, network.LastPayload[3]);
        }

        [Test]
        public void GameCommandSync_IncomingMessage_DispatchesToRegisteredHandler()
        {
            var logger = new FakeLogger();
            var network = new FakeNetworkProvider();
            using var sync = new GameCommandSyncService(network, logger);

            string sender = null;
            byte[] body = null;
            sync.RegisterHandler(GameCommandType.BuildingPlace, (s, p) =>
            {
                sender = s;
                body = p;
            });

            network.PushIncoming("peer-2", new byte[] { (byte)GameCommandType.BuildingPlace, 1, 2, 3 });

            Assert.AreEqual("peer-2", sender);
            Assert.IsNotNull(body);
            Assert.AreEqual(3, body.Length);
            Assert.AreEqual(1, body[0]);
            Assert.AreEqual(2, body[1]);
            Assert.AreEqual(3, body[2]);
        }

        [Test]
        public void StartingPositionSync_BroadcastsAndRestoresAssignments()
        {
            var container = new DiContainer();
            Zenject.SignalBusInstaller.Install(container);
            container.DeclareSignal<WorldSpawnPositionsSignal>().OptionalSubscriber();

            var signalBus = container.Resolve<SignalBus>();
            var logger = new FakeLogger();
            var network = new FakeNetworkProvider();
            var commandSync = new FakeGameCommandSyncService();
            var service = new StartingPositionSyncService(signalBus, network, commandSync, logger);

            service.Initialize();

            WorldSpawnPositionsSignal received = default;
            bool receivedFlag = false;
            signalBus.Subscribe<WorldSpawnPositionsSignal>(signal =>
            {
                received = signal;
                receivedFlag = true;
            });

            signalBus.Fire(new WorldSpawnPositionsSignal
            {
                Assignments = new[]
                {
                    new SpawnPositionAssignment
                    {
                        SlotIndex = 0,
                        ParticipantId = "player-1",
                        IsBot = false,
                        Position = new UnityEngine.Vector2Int(7, 9),
                    },
                },
            });

            Assert.AreEqual(1, commandSync.SendCount);
            Assert.AreEqual(GameCommandType.StartingPositions, commandSync.LastSentType);

            network.SimulatePeerConnected("peer-2");
            Assert.AreEqual(2, commandSync.SendCount);

            commandSync.PushIncoming("peer-2", commandSync.LastSentPayload);

            Assert.IsTrue(receivedFlag);
            Assert.AreEqual(1, received.Assignments.Length);
            Assert.AreEqual("player-1", received.Assignments[0].ParticipantId);
            Assert.AreEqual(7, received.Assignments[0].Position.x);
            Assert.AreEqual(9, received.Assignments[0].Position.y);
        }

        [Test]
        public void ConfigSync_ReceivesHostConfig_StoresLoadedConfig()
        {
            var logger = new FakeLogger();
            var service = new ConfigSyncService(logger);
            var hostConfig = new MultiplayerConfig(
                schemaVersion: 2,
                providerType: NetworkProviderType.Relay,
                defaultSessionRules: SessionRules.Default(),
                strictParticipantLock: true,
                enforceConfigConsistency: true,
                matchmakingEnabled: false);

            service.SyncFromHost(hostConfig);

            Assert.AreSame(hostConfig, service.LoadedConfig);
            Assert.AreEqual(NetworkProviderType.Relay, service.LoadedConfig.ProviderType);
        }

        [Test]
        public void SnapshotStore_SaveAndLoad_RoundTrip()
        {
            IWorldSnapshotStore store = new InMemoryWorldSnapshotStore();
            var snapshot = new WorldSnapshot("world-01", version: 7, checksum: 0xAABBCCDDu);

            store.Save(snapshot);

            Assert.IsTrue(store.Exists("world-01"));
            var loaded = store.Load("world-01");
            Assert.IsNotNull(loaded);
            Assert.AreEqual("world-01", loaded.WorldId);
            Assert.AreEqual(7, loaded.Version);
            Assert.AreEqual(0xAABBCCDDu, loaded.Checksum);
        }

        [Test]
        public void BinaryConfigStore_SaveLoad_RoundTrip()
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"moyva_mp_cfg_{Guid.NewGuid():N}.dat");

            try
            {
                var store = new BinaryConfigStore(filePath);
                var source = new MultiplayerConfig(
                    schemaVersion: 2,
                    providerType: NetworkProviderType.Relay,
                    defaultSessionRules: new SessionRules(SessionMode.MultiplayerHumans, 6, 6, 0, false, true, true),
                    strictParticipantLock: true,
                    enforceConfigConsistency: true,
                    matchmakingEnabled: true,
                    relaySettings: new RelayProviderSettings("project-x", "production", "europe-west", 8),
                    webSocketSettings: new WebSocketProviderSettings("ws://localhost", 7777, "", 4, 1.5f),
                    fallbackProviderType: NetworkProviderType.Offline);

                store.Save(source);
                var loaded = store.Load();

                Assert.AreEqual(MultiplayerConfig.CurrentSchemaVersion, loaded.SchemaVersion);
                Assert.AreEqual(NetworkProviderType.Relay, loaded.ProviderType);
                Assert.AreEqual(NetworkProviderType.Offline, loaded.FallbackProviderType);
                Assert.AreEqual(6, loaded.DefaultSessionRules.MaxParticipants);
                Assert.AreEqual("project-x", loaded.RelaySettings.ProjectId);
                Assert.AreEqual(8, loaded.RelaySettings.MaxConnections);
                Assert.AreEqual(7777, loaded.WebSocketSettings.Port);
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        [Test]
        public void BinaryConfigStore_Load_MigratesV2ToLatest_WithToggleDefaultsEnabled()
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"moyva_mp_cfg_migrate_{Guid.NewGuid():N}.dat");

            try
            {
                var store = new BinaryConfigStore(filePath);
                var v2 = new MultiplayerConfig(
                    schemaVersion: 2,
                    providerType: NetworkProviderType.WebSocket,
                    defaultSessionRules: SessionRules.Default(),
                    strictParticipantLock: false,
                    enforceConfigConsistency: true,
                    matchmakingEnabled: false,
                    relaySettings: RelayProviderSettings.Default(),
                    webSocketSettings: new WebSocketProviderSettings("ws://localhost", 9000, string.Empty, 3, 1f),
                    fallbackProviderType: NetworkProviderType.Offline,
                    reconnectLocalTimeToleranceSeconds: 120f,
                    enableRelayProvider: true,
                    enableHostMigration: true);

                store.Save(v2);
                var loaded = store.Load();

                Assert.AreEqual(MultiplayerConfig.CurrentSchemaVersion, loaded.SchemaVersion);
                Assert.IsTrue(loaded.EnableRelayProvider);
                Assert.IsTrue(loaded.EnableHostMigration);
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        [Test]
        public async Task SessionManager_JoinExisting_SetsLocalParticipantAsNonHost()
        {
            var logger = new FakeLogger();
            var network = new FakeNetworkProvider { JoinResultFactory = sid => SessionResult.Ok(sid) };
            var snapshotStore = new InMemoryWorldSnapshotStore();
            var participantPolicy = new ParticipantPolicyService(logger, snapshotStore);
            var manager = new SessionManager(
                network,
                new OfflineLobbyService(logger),
                participantPolicy,
                new WorldConsistencyService(logger),
                snapshotStore,
                new BinaryConfigStore(Path.Combine(Path.GetTempPath(), $"moyva_join_cfg_{Guid.NewGuid():N}.dat")),
                logger,
                new FakeFailurePolicy(),
                new HostMigrationService(logger),
                new ParticipantFallbackService());

            var ok = await manager.CreateOrJoinSessionAsync(new SessionConnectOptions(
                new ParticipantIdentity("p2", "Client"),
                "room-77",
                createIfNotExists: false,
                rules: SessionRules.Default(),
                configChecksum: 0));

            Assert.IsTrue(ok);
            Assert.AreEqual(1, manager.Participants.Count);
            Assert.AreEqual("p2", manager.Participants[0].Identity.PlayerId);
            Assert.IsFalse(manager.Participants[0].IsHost);
        }
    }
}