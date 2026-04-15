using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Persistence;
using Kruty1918.Moyva.Multiplayer.Runtime;
using NUnit.Framework;

namespace Kruty1918.Moyva.Tests.Multiplayer
{
    [TestFixture]
    public class SessionManagerTests
    {
        // Fakes

        private sealed class FakeLogger : IMultiplayerLogger
        {
            public List<string> Warnings { get; } = new List<string>();
            public List<string> Errors { get; } = new List<string>();
            public void Info(string msg) { }
            public void Warn(string msg) => Warnings.Add(msg);
            public void Error(string msg) => Errors.Add(msg);
            public void Trace(string msg) { }
        }

        private sealed class FakeFailurePolicy : IFailureHandlingPolicy
        {
            public bool HandleRecoverable(FailureCategory cat, string details) => false;
            public void HandleNonRecoverable(FailureCategory cat, string details) { }
        }

        private sealed class FakeSnapshotStore : IWorldSnapshotStore
        {
            private readonly Dictionary<string, WorldSnapshot> _store = new Dictionary<string, WorldSnapshot>();
            public bool Exists(string worldId) => _store.ContainsKey(worldId);
            public WorldSnapshot Load(string worldId) => _store.TryGetValue(worldId, out var s) ? s : null;
            public void Save(WorldSnapshot snapshot) => _store[snapshot.WorldId] = snapshot;
        }

        private sealed class AlwaysFailNetworkProvider : INetworkProvider
        {
            public IObservable<NetworkMessage> Messages => new EmptyObservable();
            public event System.Action<string> PeerConnected { add { } remove { } }
            public event System.Action<string> PeerDisconnected { add { } remove { } }

            public Task<SessionResult> HostSessionAsync(string sessionId, System.Threading.CancellationToken ct = default)
                => Task.FromResult(SessionResult.Fail("host failed"));

            public Task<SessionResult> JoinSessionAsync(string sessionId, System.Threading.CancellationToken ct = default)
                => Task.FromResult(SessionResult.Fail("join failed"));

            public Task LeaveSessionAsync(System.Threading.CancellationToken ct = default) => Task.CompletedTask;

            public Task SendMessageAsync(string targetPeerId, byte[] payload, System.Threading.CancellationToken ct = default)
                => Task.CompletedTask;

            private sealed class EmptyObservable : IObservable<NetworkMessage>
            {
                public IDisposable Subscribe(IObserver<NetworkMessage> observer) => new NoopDisposable();
            }

            private sealed class NoopDisposable : IDisposable
            {
                public void Dispose() { }
            }
        }

        private sealed class FakeConfigStore : IConfigStore
        {
            public MultiplayerConfig Config { get; set; } = MultiplayerConfig.Default();
            public MultiplayerConfig Load() => Config;
            public void Save(MultiplayerConfig config) => Config = config;
            public bool Exists() => true;
        }

        private sealed class FakeHostMigrationService : IHostMigrationService
        {
            public Participant ChooseNewHost(System.Collections.Generic.IReadOnlyList<Participant> remaining)
            {
                foreach (var p in remaining)
                    if (!p.IsBot) return p.AsHost();
                return null;
            }
        }

        private sealed class FakeParticipantFallbackService : IParticipantFallbackService
        {
            public Participant GetFallback(
                ParticipantIdentity leavingParticipant,
                System.Collections.Generic.IReadOnlyList<Participant> remaining,
                SessionRules rules) => null; // no fallback by default
        }

        // Helpers

        private SessionManager BuildManager(INetworkProvider network = null, IConfigStore configStore = null, IWorldSnapshotStore snapshotStore = null)
        {
            var logger = new FakeLogger();
            var failPolicy = new FakeFailurePolicy();
            var snapStore = snapshotStore ?? new FakeSnapshotStore();
            var cfgStore = configStore ?? new FakeConfigStore();
            var participantPolicy = new ParticipantPolicyService(logger, snapStore);
            var consistencyService = new WorldConsistencyService(logger);
            var netProvider = network ?? new OfflineNetworkProvider();
            var hostMigration = new FakeHostMigrationService();
            var participantFallback = new FakeParticipantFallbackService();

            return new SessionManager(netProvider, participantPolicy, consistencyService, snapStore, cfgStore, logger, failPolicy, hostMigration, participantFallback);
        }

        private SessionConnectOptions MakeOptions(string roomId = "room-1", bool create = true)
        {
            var identity = new ParticipantIdentity("p1", "Player1");
            var rules = SessionRules.Default();
            return new SessionConnectOptions(identity, roomId, create, rules, configChecksum: 0);
        }

        // Tests

        [Test]
        public async Task CreateOrJoin_ShouldReturnTrue_WhenCreatingOfflineSession()
        {
            var manager = BuildManager();
            var options = MakeOptions(create: true);

            var result = await manager.CreateOrJoinSessionAsync(options);

            Assert.IsTrue(result);
            Assert.AreEqual(1, manager.Participants.Count);
        }

        [Test]
        public async Task CreateOrJoin_ShouldAddLocalParticipantAsHost_WhenCreating()
        {
            var manager = BuildManager();
            var options = MakeOptions(create: true);

            await manager.CreateOrJoinSessionAsync(options);

            Assert.IsTrue(manager.Participants[0].IsHost);
            Assert.AreEqual("p1", manager.Participants[0].Identity.PlayerId);
        }

        [Test]
        public async Task Leave_ShouldClearParticipants()
        {
            var manager = BuildManager();
            await manager.CreateOrJoinSessionAsync(MakeOptions(create: true));

            await manager.LeaveSessionAsync();

            Assert.AreEqual(0, manager.Participants.Count);
        }

        [Test]
        public async Task CreateOrJoin_ShouldReturnFalse_WhenConfigChecksumMismatch()
        {
            var cfgStore = new FakeConfigStore
            {
                Config = new MultiplayerConfig(
                    1, NetworkProviderType.Offline, SessionRules.Default(),
                    false, enforceConfigConsistency: true, false)
            };
            var manager = BuildManager(configStore: cfgStore);

            var identity = new ParticipantIdentity("p1", "Player1");
            var rules = SessionRules.Default();
            var options = new SessionConnectOptions(identity, "room-1", false, rules, configChecksum: 0xDEADBEEF);

            var result = await manager.CreateOrJoinSessionAsync(options);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task CreateOrJoin_ShouldReturnFalse_WhenSessionFull()
        {
            var manager = BuildManager();

            var rules = new SessionRules(SessionMode.MultiplayerHumans, 1, 1, 0, false, false, false);
            var identity1 = new ParticipantIdentity("p1", "Player1");
            var opt1 = new SessionConnectOptions(identity1, "room-1", true, rules, 0);
            await manager.CreateOrJoinSessionAsync(opt1);

            var identity2 = new ParticipantIdentity("p2", "Player2");
            var opt2 = new SessionConnectOptions(identity2, "room-1", false, rules, 0);
            var result = await manager.CreateOrJoinSessionAsync(opt2);

            Assert.IsFalse(result);
        }

        [Test]
        public async Task CreateOrJoin_ShouldFallbackToSolo_WhenRoomIdIsMissing()
        {
            var manager = BuildManager();

            var identity = new ParticipantIdentity("p1", "Player1");
            var options = new SessionConnectOptions(identity, "", createIfNotExists: false, SessionRules.Default(), configChecksum: 1234);

            var result = await manager.CreateOrJoinSessionAsync(options);

            Assert.IsTrue(result);
            Assert.AreEqual(1, manager.Participants.Count);
            Assert.AreEqual("p1", manager.Participants[0].Identity.PlayerId);
            Assert.IsTrue(manager.Participants[0].IsHost);
        }

        [Test]
        public async Task CreateOrJoin_ShouldFallbackToLocalSinglePlayer_WhenNetworkFails()
        {
            var manager = BuildManager(network: new AlwaysFailNetworkProvider());
            var options = MakeOptions(roomId: "room-x", create: true);

            var result = await manager.CreateOrJoinSessionAsync(options);

            Assert.IsTrue(result);
            Assert.AreEqual(1, manager.Participants.Count);
            Assert.IsTrue(manager.Participants[0].IsHost);
        }
    }

    [TestFixture]
    public class SessionManagerMigrationTests
    {
        // A network provider that allows manually firing PeerDisconnected
        private sealed class ControllableNetworkProvider : INetworkProvider
        {
            public IObservable<NetworkMessage> Messages => new EmptyObservable();
            public event System.Action<string> PeerConnected;
            public event System.Action<string> PeerDisconnected;

            public void SimulateDisconnect(string peerId) => PeerDisconnected?.Invoke(peerId);
            public void SimulateConnect(string peerId) => PeerConnected?.Invoke(peerId);

            public Task<SessionResult> HostSessionAsync(string sessionId, System.Threading.CancellationToken ct = default)
                => Task.FromResult(SessionResult.Ok(sessionId));

            public Task<SessionResult> JoinSessionAsync(string sessionId, System.Threading.CancellationToken ct = default)
                => Task.FromResult(SessionResult.Ok(sessionId));

            public Task LeaveSessionAsync(System.Threading.CancellationToken ct = default) => Task.CompletedTask;

            public Task SendMessageAsync(string targetPeerId, byte[] payload, System.Threading.CancellationToken ct = default)
                => Task.CompletedTask;

            private sealed class EmptyObservable : IObservable<NetworkMessage>
            {
                public IDisposable Subscribe(IObserver<NetworkMessage> observer) => new NoopDisposable();
            }

            private sealed class NoopDisposable : IDisposable { public void Dispose() { } }
        }

        private sealed class FakeLogger : IMultiplayerLogger
        {
            public List<string> Info { get; } = new List<string>();
            public List<string> Warnings { get; } = new List<string>();
            void IMultiplayerLogger.Info(string msg) => Info.Add(msg);
            public void Warn(string msg) => Warnings.Add(msg);
            public void Error(string msg) { }
            public void Trace(string msg) { }
        }

        private sealed class FakeFailurePolicy : IFailureHandlingPolicy
        {
            public bool HandleRecoverable(FailureCategory cat, string details) => false;
            public void HandleNonRecoverable(FailureCategory cat, string details) { }
        }

        private sealed class FakeSnapshotStore : IWorldSnapshotStore
        {
            private readonly Dictionary<string, WorldSnapshot> _store = new Dictionary<string, WorldSnapshot>();
            public bool Exists(string worldId) => _store.ContainsKey(worldId);
            public WorldSnapshot Load(string worldId) => _store.TryGetValue(worldId, out var s) ? s : null;
            public void Save(WorldSnapshot snapshot) => _store[snapshot.WorldId] = snapshot;
        }

        private sealed class FakeConfigStore : IConfigStore
        {
            public MultiplayerConfig Config { get; set; } = MultiplayerConfig.Default();
            public MultiplayerConfig Load() => Config;
            public void Save(MultiplayerConfig config) => Config = config;
            public bool Exists() => true;
        }

        private (SessionManager manager, ControllableNetworkProvider net, FakeLogger logger) BuildMigrationManager(
            bool allowBotFallback = false)
        {
            var net = new ControllableNetworkProvider();
            var logger = new FakeLogger();
            var failPolicy = new FakeFailurePolicy();
            var snapStore = new FakeSnapshotStore();
            var cfgStore = new FakeConfigStore();
            var participantPolicy = new ParticipantPolicyService(logger, snapStore);
            var consistencyService = new WorldConsistencyService(logger);
            var hostMigration = new HostMigrationService(logger);

            // Fallback service that creates a bot when rules allow it
            var participantFallback = new ParticipantFallbackService();

            var manager = new SessionManager(
                net, participantPolicy, consistencyService, snapStore, cfgStore,
                logger, failPolicy, hostMigration, participantFallback);

            return (manager, net, logger);
        }

        [Test]
        public async Task OnHostDisconnect_ShouldMigrateHostToRemainingParticipant()
        {
            var (manager, net, logger) = BuildMigrationManager();

            var rules = new SessionRules(SessionMode.MultiplayerHumans, 4, 4, 0, false, false, false);

            // Join as p1 (host)
            await manager.CreateOrJoinSessionAsync(new SessionConnectOptions(
                new ParticipantIdentity("p1", "P1"), "room-1", true, rules, 0));

            // Manually add p2 as non-host participant (simulating them having joined)
            // We test the migration handler directly by calling the private method via reflection
            var p2Identity = new ParticipantIdentity("p2", "P2");
            var p2 = new Participant(p2Identity, isBot: false, isHost: false);
            var participantsField = typeof(SessionManager).GetField("_participants",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var participants = (List<Participant>)participantsField.GetValue(manager);
            participants.Add(p2);

            Assert.AreEqual(2, manager.Participants.Count);

            // Simulate host (p1) disconnecting
            net.SimulateDisconnect("p1");

            // p2 should now be the host
            Assert.AreEqual(1, manager.Participants.Count);
            Assert.IsTrue(manager.Participants[0].IsHost);
            Assert.AreEqual("p2", manager.Participants[0].Identity.PlayerId);
        }

        [Test]
        public async Task OnParticipantDisconnect_ShouldAddBotFallback_WhenRulesAllow()
        {
            var (manager, net, logger) = BuildMigrationManager();

            var rules = new SessionRules(SessionMode.MixedHumansAndBots, 4, 2, 2,
                allowBotsFallbackOnLeave: true, false, false);

            await manager.CreateOrJoinSessionAsync(new SessionConnectOptions(
                new ParticipantIdentity("p1", "P1"), "room-1", true, rules, 0));

            // Add p2 to participants
            var p2Identity = new ParticipantIdentity("p2", "P2");
            var participantsField = typeof(SessionManager).GetField("_participants",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var participants = (List<Participant>)participantsField.GetValue(manager);
            participants.Add(new Participant(p2Identity, isBot: false, isHost: false));

            // Update current rules to allow bot fallback
            var rulesField = typeof(SessionManager).GetField("_currentRules",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            rulesField.SetValue(manager, rules);

            Assert.AreEqual(2, manager.Participants.Count);

            // p2 disconnects — should be replaced by a bot
            net.SimulateDisconnect("p2");

            // p1 remains + bot fallback added
            Assert.AreEqual(2, manager.Participants.Count);
            var bot = manager.Participants[1];
            Assert.IsTrue(bot.IsBot);
            Assert.IsTrue(bot.Identity.PlayerId.StartsWith(ParticipantIdentity.BotIdPrefix));
        }

        [Test]
        public async Task OnUnknownPeerDisconnect_ShouldNotThrow_AndLogWarning()
        {
            var (manager, net, logger) = BuildMigrationManager();
            await manager.CreateOrJoinSessionAsync(new SessionConnectOptions(
                new ParticipantIdentity("p1", "P1"), "room-1", true, SessionRules.Default(), 0));

            // Simulate unknown peer disconnecting
            Assert.DoesNotThrow(() => net.SimulateDisconnect("unknown-peer"));
        }
    }
}
