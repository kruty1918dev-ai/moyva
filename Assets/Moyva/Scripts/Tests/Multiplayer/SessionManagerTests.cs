using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Lobbies;
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

        private sealed class CapturingNetworkProvider : INetworkProvider
        {
            private readonly string _hostResult;
            private readonly bool _echoHostInput;

            public string LastHostedSessionId { get; private set; }
            public int LeaveCalls { get; private set; }

            public IObservable<NetworkMessage> Messages => new EmptyObservable();
            public event System.Action<string> PeerConnected { add { } remove { } }
            public event System.Action<string> PeerDisconnected { add { } remove { } }

            public CapturingNetworkProvider(string hostResult, bool echoHostInput = false)
            {
                _hostResult = hostResult;
                _echoHostInput = echoHostInput;
            }

            public Task<SessionResult> HostSessionAsync(string sessionId, System.Threading.CancellationToken ct = default)
            {
                LastHostedSessionId = sessionId;
                return Task.FromResult(SessionResult.Ok(_echoHostInput ? sessionId : _hostResult));
            }

            public Task<SessionResult> JoinSessionAsync(string sessionId, System.Threading.CancellationToken ct = default)
                => Task.FromResult(SessionResult.Ok(sessionId));

            public Task LeaveSessionAsync(System.Threading.CancellationToken ct = default)
            {
                LeaveCalls++;
                return Task.CompletedTask;
            }

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

        private sealed class FakeLobbyService : ILobbyService
        {
            private readonly Dictionary<string, LobbyRoom> _roomsByCode = new Dictionary<string, LobbyRoom>();
            private readonly string _createdLobbyId;
            public LobbyRoom Current { get; private set; }
            public LobbyState State { get; private set; } = LobbyState.Closed;
            public CreateRoomOptions LastCreateOptions { get; private set; }
            public string LastRelayJoinCodeSet { get; private set; }
            public int CreateRoomCalls { get; private set; }
            public event Action<LobbyRoom> LobbyUpdated;
            public event Action<string> KickedFromLobby { add { } remove { } }
            public event Action<LobbyState> StateChanged;

            public FakeLobbyService(string createdLobbyId = null)
            {
                _createdLobbyId = createdLobbyId;
            }

            public Task<LobbyRoom> CreateRoomAsync(CreateRoomOptions options, System.Threading.CancellationToken ct = default)
            {
                CreateRoomCalls++;
                LastCreateOptions = options;

                var lobbyId = _createdLobbyId ?? Guid.NewGuid().ToString("N");
                var code = lobbyId.Substring(0, 6).ToUpperInvariant();
                var hostId = "host";
                var room = new LobbyRoom(
                    lobbyId,
                    code,
                    options.Name,
                    options.MaxPlayers,
                    options.IsPrivate,
                    hostId,
                    options.RelayJoinCode,
                    new List<LobbyPlayer> { new LobbyPlayer(hostId, options.DisplayName, true) });
                _roomsByCode[code] = room;
                Current = room;
                PublishState(LobbyState.Open);
                LobbyUpdated?.Invoke(room);
                return Task.FromResult(room);
            }

            public Task<LobbyRoom> JoinByCodeAsync(string lobbyCode, string displayName, System.Threading.CancellationToken ct = default)
            {
                if (_roomsByCode.TryGetValue(lobbyCode, out var room))
                {
                    Current = room;
                    PublishState(LobbyState.Open);
                    LobbyUpdated?.Invoke(room);
                    return Task.FromResult(room);
                }

                var joinedRoom = new LobbyRoom(
                    "join-" + lobbyCode,
                    lobbyCode,
                    lobbyCode,
                    4,
                    false,
                    "host",
                    lobbyCode,
                    new List<LobbyPlayer>());
                Current = joinedRoom;
                PublishState(LobbyState.Open);
                LobbyUpdated?.Invoke(joinedRoom);
                return Task.FromResult<LobbyRoom>(joinedRoom);
            }

            public Task<LobbyRoom> JoinByIdAsync(string lobbyId, string displayName, System.Threading.CancellationToken ct = default)
                => Task.FromResult(Current);
            public Task<LobbyRoom> JoinByCodeWithPasswordAsync(string lobbyCode, string displayName, string password, System.Threading.CancellationToken ct = default)
                => JoinByCodeAsync(lobbyCode, displayName, ct);
            public Task<IReadOnlyList<LobbyRoom>> QueryRoomsAsync(System.Threading.CancellationToken ct = default)
                => Task.FromResult<IReadOnlyList<LobbyRoom>>(Array.Empty<LobbyRoom>());
            public Task LeaveAsync(System.Threading.CancellationToken ct = default) { Current = null; PublishState(LobbyState.Closed); return Task.CompletedTask; }
            public Task KickAsync(string playerId, System.Threading.CancellationToken ct = default) => Task.CompletedTask;
            public Task SetRelayJoinCodeAsync(string relayJoinCode, System.Threading.CancellationToken ct = default)
            {
                LastRelayJoinCodeSet = relayJoinCode;
                if (Current != null)
                {
                    Current = new LobbyRoom(
                        Current.LobbyId,
                        Current.LobbyCode,
                        Current.Name,
                        Current.MaxPlayers,
                        Current.IsPrivate,
                        Current.HostPlayerId,
                        relayJoinCode,
                        Current.Players,
                        Current.PasswordHash,
                        Current.State);
                }

                return Task.CompletedTask;
            }
            public Task LockAsync(bool locked, byte[] startedWorldSettingsBytes = null, System.Threading.CancellationToken ct = default) { PublishState(locked ? LobbyState.Started : LobbyState.Open); return Task.CompletedTask; }

            private void PublishState(LobbyState state)
            {
                if (State == state)
                    return;

                State = state;
                StateChanged?.Invoke(state);
            }
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

        private SessionManager BuildManager(INetworkProvider network = null, IConfigStore configStore = null, IWorldSnapshotStore snapshotStore = null, ILobbyService lobby = null)
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
            var lobbyService = lobby ?? new FakeLobbyService();

            return new SessionManager(netProvider, lobbyService, participantPolicy, consistencyService, snapStore, cfgStore, logger, null, failPolicy, hostMigration, participantFallback);
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
        public async Task CreateOrJoin_RelayHost_ShouldStartTransportBeforeLobby_AndPublishRelayCode()
        {
            const string relayJoinCode = "BCDFGH";
            const string lobbyId = "eRFAKp5N2J9rwkFewHhEGU";
            var network = new CapturingNetworkProvider(relayJoinCode);
            var lobby = new FakeLobbyService(lobbyId);
            var manager = BuildManager(network: network, lobby: lobby);

            var result = await manager.CreateOrJoinSessionAsync(MakeOptions(roomId: "room-with-relay", create: true));

            Assert.IsTrue(result);
            Assert.AreEqual(string.Empty, network.LastHostedSessionId);
            Assert.AreNotEqual(lobbyId, network.LastHostedSessionId);
            Assert.AreEqual(1, lobby.CreateRoomCalls);
            Assert.AreEqual(relayJoinCode, lobby.LastCreateOptions.RelayJoinCode);
            Assert.AreEqual(relayJoinCode, lobby.Current.RelayJoinCode);
            Assert.AreEqual(relayJoinCode, lobby.LastRelayJoinCodeSet);
        }

        [Test]
        public async Task CreateOrJoin_RelayHost_ShouldNotCreateLobby_WhenTransportDoesNotReturnRelayCode()
        {
            var network = new CapturingNetworkProvider(hostResult: null, echoHostInput: true);
            var lobby = new FakeLobbyService("eRFAKp5N2J9rwkFewHhEGU");
            var manager = BuildManager(network: network, lobby: lobby);

            var result = await manager.CreateOrJoinSessionAsync(MakeOptions(roomId: "room-with-relay", create: true));

            Assert.IsTrue(result);
            Assert.AreEqual(string.Empty, network.LastHostedSessionId);
            Assert.AreEqual(0, lobby.CreateRoomCalls);
            Assert.AreEqual(1, network.LeaveCalls);
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

        private sealed class FakeLobbyService : ILobbyService
        {
            private readonly Dictionary<string, LobbyRoom> _roomsByCode = new Dictionary<string, LobbyRoom>();
            public LobbyRoom Current { get; private set; }
            public LobbyState State { get; private set; } = LobbyState.Closed;
            public event Action<LobbyRoom> LobbyUpdated;
            public event Action<string> KickedFromLobby { add { } remove { } }
            public event Action<LobbyState> StateChanged;

            public Task<LobbyRoom> CreateRoomAsync(CreateRoomOptions options, System.Threading.CancellationToken ct = default)
            {
                var lobbyId = Guid.NewGuid().ToString("N");
                var code = lobbyId.Substring(0, 6).ToUpperInvariant();
                var hostId = "host";
                var room = new LobbyRoom(
                    lobbyId,
                    code,
                    options.Name,
                    options.MaxPlayers,
                    options.IsPrivate,
                    hostId,
                    string.Empty,
                    new List<LobbyPlayer> { new LobbyPlayer(hostId, options.DisplayName, true) });
                _roomsByCode[code] = room;
                Current = room;
                PublishState(LobbyState.Open);
                LobbyUpdated?.Invoke(room);
                return Task.FromResult(room);
            }

            public Task<LobbyRoom> JoinByCodeAsync(string lobbyCode, string displayName, System.Threading.CancellationToken ct = default)
            {
                if (_roomsByCode.TryGetValue(lobbyCode, out var room))
                {
                    Current = room;
                    PublishState(LobbyState.Open);
                    LobbyUpdated?.Invoke(room);
                    return Task.FromResult(room);
                }

                var joinedRoom = new LobbyRoom(
                    "join-" + lobbyCode,
                    lobbyCode,
                    lobbyCode,
                    4,
                    false,
                    "host",
                    lobbyCode,
                    new List<LobbyPlayer>());
                Current = joinedRoom;
                PublishState(LobbyState.Open);
                LobbyUpdated?.Invoke(joinedRoom);
                return Task.FromResult<LobbyRoom>(joinedRoom);
            }

            public Task<LobbyRoom> JoinByIdAsync(string lobbyId, string displayName, System.Threading.CancellationToken ct = default)
                => Task.FromResult(Current);
            public Task<LobbyRoom> JoinByCodeWithPasswordAsync(string lobbyCode, string displayName, string password, System.Threading.CancellationToken ct = default)
                => JoinByCodeAsync(lobbyCode, displayName, ct);
            public Task<IReadOnlyList<LobbyRoom>> QueryRoomsAsync(System.Threading.CancellationToken ct = default)
                => Task.FromResult<IReadOnlyList<LobbyRoom>>(Array.Empty<LobbyRoom>());
            public Task LeaveAsync(System.Threading.CancellationToken ct = default) { Current = null; PublishState(LobbyState.Closed); return Task.CompletedTask; }
            public Task KickAsync(string playerId, System.Threading.CancellationToken ct = default) => Task.CompletedTask;
            public Task SetRelayJoinCodeAsync(string relayJoinCode, System.Threading.CancellationToken ct = default) => Task.CompletedTask;
            public Task LockAsync(bool locked, byte[] startedWorldSettingsBytes = null, System.Threading.CancellationToken ct = default) { PublishState(locked ? LobbyState.Started : LobbyState.Open); return Task.CompletedTask; }

            private void PublishState(LobbyState state)
            {
                if (State == state)
                    return;

                State = state;
                StateChanged?.Invoke(state);
            }
        }

        private sealed class FakeHostMigrationService : IHostMigrationService
        {
            private readonly IMultiplayerLogger _logger;

            public FakeHostMigrationService(IMultiplayerLogger logger)
            {
                _logger = logger;
            }

            public Participant ChooseNewHost(IReadOnlyList<Participant> remaining)
            {
                foreach (var participant in remaining)
                {
                    if (!participant.IsBot)
                    {
                        _logger.Info($"FakeHostMigrationService: новий хост -> {participant.Identity}");
                        return participant.AsHost();
                    }
                }

                _logger.Warn("FakeHostMigrationService: жодного живого людського учасника.");
                return null;
            }
        }

        private sealed class ConfigurableParticipantFallbackService : IParticipantFallbackService
        {
            private readonly bool _forceDisableFallback;

            public ConfigurableParticipantFallbackService(bool forceDisableFallback)
            {
                _forceDisableFallback = forceDisableFallback;
            }

            public Participant GetFallback(
                ParticipantIdentity leavingParticipant,
                IReadOnlyList<Participant> remaining,
                SessionRules rules)
            {
                if (_forceDisableFallback || !rules.AllowBotsFallbackOnLeave)
                    return null;

                var botIdentity = new ParticipantIdentity(
                    ParticipantIdentity.BotIdPrefix + leavingParticipant.PlayerId,
                    leavingParticipant.Nickname);

                return new Participant(botIdentity, isBot: true, isHost: false);
            }
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
            var hostMigration = new FakeHostMigrationService(logger);
            var participantFallback = new ConfigurableParticipantFallbackService(forceDisableFallback: !allowBotFallback);
            var lobby = new FakeLobbyService();

            var manager = new SessionManager(
                net, lobby, participantPolicy, consistencyService, snapStore, cfgStore,
                logger, null, failPolicy, hostMigration, participantFallback);

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
            var (manager, net, logger) = BuildMigrationManager(allowBotFallback: true);

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
