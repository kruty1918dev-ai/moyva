using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Persistence;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Orchestrates session lifecycle. In online mode it composes a UGS Lobby
    /// (room discovery / player list) with an <see cref="INetworkProvider"/>
    /// (Relay transport) by storing the Relay join code in lobby data.
    /// Participants are synced from the lobby's player list so identities
    /// are always the authoritative <c>PlayerId</c>.
    /// </summary>
    public sealed class SessionManager : ISessionManager, IDisposable
    {
        private const string SoloSessionPrefix = "solo-local";
        private static readonly SessionRules SoloFallbackRules = new SessionRules(
            SessionMode.PeacefulSolo,
            maxParticipants: 1,
            maxHumans: 1,
            maxBots: 0,
            allowBotsFallbackOnLeave: false,
            allowMatchSaveForAnalysis: false,
            strictParticipantLock: false);

        private readonly INetworkProvider _network;
        private readonly ILobbyService _lobby;
        private readonly IParticipantPolicyService _participantPolicy;
        private readonly IWorldConsistencyService _consistency;
        private readonly IWorldSnapshotStore _snapshotStore;
        private readonly IConfigStore _configStore;
        private readonly IMultiplayerLogger _logger;
        private readonly IFailureHandlingPolicy _failurePolicy;
        private readonly IHostMigrationService _hostMigration;
        private readonly IParticipantFallbackService _participantFallback;

        private readonly List<Participant> _participants = new List<Participant>();
        private MultiplayerConfig _config;
        private string _currentSessionId;
        private string _currentLobbyId;
        private string _currentLobbyCode;
        private SessionRules _currentRules;
        private bool _isHost;
        private string _localPlayerId;

        public IReadOnlyList<Participant> Participants => _participants;

            /// <summary>PlayerId of the local participant (or empty when unknown).</summary>
            public string LocalPlayerId => _localPlayerId;

            /// <summary>True when the local participant is the host of the current session.</summary>
            public bool IsLocalPlayerHost => _isHost;

        /// <summary>Current lobby join code (visible to UI / shareable).</summary>
        public string CurrentLobbyCode => _currentLobbyCode;

        public SessionManager(
            INetworkProvider network,
            ILobbyService lobby,
            IParticipantPolicyService participantPolicy,
            IWorldConsistencyService consistency,
            IWorldSnapshotStore snapshotStore,
            IConfigStore configStore,
            IMultiplayerLogger logger,
            IFailureHandlingPolicy failurePolicy,
            IHostMigrationService hostMigration,
            IParticipantFallbackService participantFallback)
        {
            _network             = network             ?? throw new ArgumentNullException(nameof(network));
            _lobby               = lobby               ?? throw new ArgumentNullException(nameof(lobby));
            _participantPolicy   = participantPolicy   ?? throw new ArgumentNullException(nameof(participantPolicy));
            _consistency         = consistency         ?? throw new ArgumentNullException(nameof(consistency));
            _snapshotStore       = snapshotStore       ?? throw new ArgumentNullException(nameof(snapshotStore));
            _configStore         = configStore         ?? throw new ArgumentNullException(nameof(configStore));
            _logger              = logger              ?? throw new ArgumentNullException(nameof(logger));
            _failurePolicy       = failurePolicy       ?? throw new ArgumentNullException(nameof(failurePolicy));
            _hostMigration       = hostMigration       ?? throw new ArgumentNullException(nameof(hostMigration));
            _participantFallback = participantFallback ?? throw new ArgumentNullException(nameof(participantFallback));

            _network.PeerConnected    += OnPeerConnected;
            _network.PeerDisconnected += OnPeerDisconnected;
            _lobby.LobbyUpdated       += OnLobbyUpdated;
            _lobby.KickedFromLobby    += OnKickedFromLobby;
        }

        public void Dispose()
        {
            _network.PeerConnected    -= OnPeerConnected;
            _network.PeerDisconnected -= OnPeerDisconnected;
            _lobby.LobbyUpdated       -= OnLobbyUpdated;
            _lobby.KickedFromLobby    -= OnKickedFromLobby;
        }

        // ── Session API ───────────────────────────────────────────────────────────

        public async Task<bool> CreateOrJoinSessionAsync(SessionConnectOptions options, CancellationToken ct = default)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _config = _configStore.Load();
            var opts = NormalizeOptions(options);
            _localPlayerId = opts.LocalIdentity.PlayerId;

            // Missing join target: fall back to local solo instead of attempting online join.
            if (!options.CreateIfNotExists && string.IsNullOrWhiteSpace(options.RoomId))
            {
                _logger.Warn("Join requested without room id. Falling back to local single-player session.");
                return StartOfflineSolo(opts);
            }

            // Config consistency should be validated even for offline provider.
            if (_config.EnforceConfigConsistency)
            {
                uint localChecksum = ComputeConfigChecksum(_config);
                if (opts.ConfigChecksum != 0 && opts.ConfigChecksum != localChecksum)
                {
                    _logger.Warn($"Config checksum mismatch: local={localChecksum:X8}, remote={opts.ConfigChecksum:X8}");
                    _failurePolicy.HandleRecoverable(FailureCategory.ConfigMismatch, "Config checksums differ.");
                    return false;
                }
            }

            // Pure-offline / solo path
            if (_config.ProviderType == NetworkProviderType.Offline)
            {
                return StartOfflineSolo(opts);
            }

            // Local participant policy
            WorldSnapshot snapshot = _snapshotStore.Exists(opts.RoomId) ? _snapshotStore.Load(opts.RoomId) : null;
            if (!_participantPolicy.CanJoin(opts.LocalIdentity, _participants, opts.Rules, snapshot))
            {
                _failurePolicy.HandleRecoverable(FailureCategory.ParticipantRejected, $"Participant {opts.LocalIdentity.PlayerId} rejected.");
                return false;
            }

            try
            {
                if (opts.CreateIfNotExists)
                    return await HostFlowAsync(opts, ct);

                return await JoinFlowAsync(opts, ct);
            }
            catch (Exception e)
            {
                _logger.Error($"CreateOrJoinSession failed: {e.Message}");
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, e.Message);
                return await FallbackToOfflineSoloAsync(opts, "Unhandled exception in session flow.", ct);
            }
        }

        public async Task LeaveSessionAsync(CancellationToken ct = default)
        {
            _logger.Info($"Leaving session: {_currentSessionId}");
            await SafeCleanupAsync(ct);
        }

        // ── Host / Join flows ─────────────────────────────────────────────────────

        private async Task<bool> HostFlowAsync(SessionConnectOptions opts, CancellationToken ct)
        {
            _logger.Info($"Host flow: room='{opts.RoomId}' max={opts.Rules.MaxParticipants}");

            var createOpts = new CreateRoomOptions(opts.RoomId, opts.Rules.MaxParticipants, isPrivate: false,
                displayName: opts.LocalIdentity.Nickname);

            var lobby = await _lobby.CreateRoomAsync(createOpts, ct);
            if (lobby == null)
            {
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, "Failed to create lobby.");
                return await FallbackToOfflineSoloAsync(opts, "Failed to create lobby.", ct);
            }

            _currentLobbyId = lobby.LobbyId;
            _currentLobbyCode = lobby.LobbyCode;
            _isHost = true;

            var hostResult = await _network.HostSessionAsync(lobby.LobbyId, ct);
            if (!hostResult.Success)
            {
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, hostResult.ErrorMessage);
                return await FallbackToOfflineSoloAsync(opts, hostResult.ErrorMessage, ct);
            }

            // Relay join code → lobby data so clients can discover it.
            await _lobby.SetRelayJoinCodeAsync(hostResult.SessionId, ct);

            _currentSessionId = hostResult.SessionId;
            _currentRules = opts.Rules;

            UpsertLocalParticipant(opts.LocalIdentity, isHost: true);
            CleanupHostAliasParticipants();
            _logger.Info($"Session hosted. LobbyCode={_currentLobbyCode} RelayCode={_currentSessionId}");
            return true;
        }

        private async Task<bool> JoinFlowAsync(SessionConnectOptions opts, CancellationToken ct)
        {
            _logger.Info($"Join flow: lobbyCode='{opts.RoomId}'");

            var lobby = await _lobby.JoinByCodeAsync(opts.RoomId, opts.LocalIdentity.Nickname, ct);
            if (lobby == null)
            {
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, "Failed to join lobby.");
                return await FallbackToOfflineSoloAsync(opts, "Failed to join lobby.", ct);
            }

            _currentLobbyId = lobby.LobbyId;
            _currentLobbyCode = lobby.LobbyCode;
            _isHost = false;

            // Wait for relay code to become available (host publishes it asynchronously).
            string relayCode = await WaitForRelayCodeAsync(TimeSpan.FromSeconds(15), ct);
            if (string.IsNullOrEmpty(relayCode))
            {
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, "Relay code not published by host.");
                return await FallbackToOfflineSoloAsync(opts, "Relay code not published by host.", ct);
            }

            var joinResult = await _network.JoinSessionAsync(relayCode, ct);
            if (!joinResult.Success)
            {
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, joinResult.ErrorMessage);
                return await FallbackToOfflineSoloAsync(opts, joinResult.ErrorMessage, ct);
            }

            _currentSessionId = joinResult.SessionId;
            _currentRules = opts.Rules;

            UpsertLocalParticipant(opts.LocalIdentity, isHost: false);
            _logger.Info($"Session joined. Lobby={_currentLobbyCode}");
            return true;
        }

        private async Task<string> WaitForRelayCodeAsync(TimeSpan timeout, CancellationToken ct)
        {
            var deadline = DateTime.UtcNow.Add(timeout);
            while (DateTime.UtcNow < deadline)
            {
                if (ct.IsCancellationRequested) return null;

                var snap = _lobby.Current;
                if (snap != null && !string.IsNullOrEmpty(snap.RelayJoinCode))
                    return snap.RelayJoinCode;

                try { await Task.Delay(500, ct); }
                catch (OperationCanceledException) { return null; }
            }
            return null;
        }

        // ── Participant sync ──────────────────────────────────────────────────────

        private void UpsertLocalParticipant(ParticipantIdentity identity, bool isHost)
        {
            var existing = _participants.Find(p => p.Identity.PlayerId == identity.PlayerId);
            if (existing == null)
                _participants.Add(new Participant(identity, isBot: false, isHost));
        }

        private void OnLobbyUpdated(LobbyRoom snapshot)
        {
            if (snapshot == null) return;

            // Add missing remote participants from the lobby's player list.
            foreach (var p in snapshot.Players)
            {
                if (string.IsNullOrEmpty(p.PlayerId)) continue;
                if (_participants.Exists(x => x.Identity.PlayerId == p.PlayerId)) continue;

                // In tests/stubs the lobby host id may not match the local identity.
                // When we're the authoritative host, ignore that foreign host alias.
                if (_isHost && p.IsHost && !string.IsNullOrEmpty(_localPlayerId) &&
                    !string.Equals(p.PlayerId, _localPlayerId, StringComparison.Ordinal))
                    continue;

                var identity = new ParticipantIdentity(p.PlayerId, p.DisplayName);
                _participants.Add(new Participant(identity, isBot: false, isHost: p.IsHost));
                _logger.Info($"[Lobby] Participant added: {p.PlayerId} ({p.DisplayName})");
            }

            // Remove participants no longer in the lobby.
            for (int i = _participants.Count - 1; i >= 0; i--)
            {
                var p = _participants[i];
                bool stillInLobby = false;
                for (int j = 0; j < snapshot.Players.Count; j++)
                {
                    if (snapshot.Players[j].PlayerId == p.Identity.PlayerId) { stillInLobby = true; break; }
                }
                if (!stillInLobby)
                {
                    _participants.RemoveAt(i);
                    _logger.Info($"[Lobby] Participant removed: {p.Identity.PlayerId}");
                }
            }
        }

        private void OnKickedFromLobby(string reason)
        {
            _logger.Warn($"Kicked from lobby: {reason}");
            _ = SafeCleanupAsync();
        }

        private void OnPeerConnected(string peerId)
        {
            if (string.IsNullOrEmpty(peerId) || peerId == _localPlayerId) return;
            _logger.Info($"[Net] Peer connected: {peerId}");
            // Lobby update will fill identity; nothing to add here unless missing.
        }

        private void OnPeerDisconnected(string peerId)
        {
            if (string.IsNullOrEmpty(peerId)) return;

            var leaving = _participants.Find(p => p.Identity.PlayerId == peerId);
            if (leaving == null)
            {
                _logger.Warn($"OnPeerDisconnected: unknown peer '{peerId}' - ignoring.");
                return;
            }

            _participants.Remove(leaving);
            _logger.Info($"Participant '{peerId}' left. Remaining: {_participants.Count}");

            if (leaving.IsHost && _participants.Count > 0)
            {
                var migrated = _hostMigration.ChooseNewHost(_participants);
                if (migrated == null)
                {
                    _logger.Warn("Host disconnected and migration failed. Ending session.");
                    _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, "Host disconnected.");
                    _ = SafeCleanupAsync();
                    return;
                }

                int index = _participants.FindIndex(p => p.Identity.PlayerId == migrated.Identity.PlayerId);
                if (index >= 0)
                    _participants[index] = migrated;

                _isHost = string.Equals(migrated.Identity.PlayerId, _localPlayerId, StringComparison.Ordinal);
                _logger.Warn($"Host disconnected. Migrated host to '{migrated.Identity.PlayerId}'.");
                return;
            }

            // Bot fallback (non-host)
            var rules = _currentRules ?? SoloFallbackRules;
            var fallback = _participantFallback.GetFallback(leaving.Identity, _participants, rules);
            if (fallback != null)
            {
                _participants.Add(fallback);
                _logger.Info($"Bot fallback added: '{fallback.Identity.PlayerId}' replaces '{peerId}'.");
            }
        }

        // ── Cleanup & helpers ─────────────────────────────────────────────────────

        private async Task SafeCleanupAsync(CancellationToken ct = default)
        {
            try { await _network.LeaveSessionAsync(ct); } catch (Exception e) { _logger.Warn($"Net leave: {e.Message}"); }
            try { await _lobby.LeaveAsync(ct); } catch (Exception e) { _logger.Warn($"Lobby leave: {e.Message}"); }

            _participants.Clear();
            _currentSessionId = null;
            _currentLobbyId = null;
            _currentLobbyCode = null;
            _currentRules = null;
            _isHost = false;
        }

        private bool StartOfflineSolo(SessionConnectOptions opts)
        {
            var roomId = $"{SoloSessionPrefix}-{Guid.NewGuid():N}";
            _participants.Clear();
            _currentSessionId = roomId;
            _currentRules = opts.Rules ?? SoloFallbackRules;
            _participants.Add(new Participant(opts.LocalIdentity, isBot: false, isHost: true));
            _logger.Info($"Offline solo session '{roomId}' started.");
            return true;
        }

        internal static uint ComputeConfigChecksum(MultiplayerConfig config)
        {
            const uint FnvOffsetBasis = 2166136261u;
            const uint FnvPrime = 16777619u;
            unchecked
            {
                uint crc = FnvOffsetBasis;
                crc = (crc ^ (uint)config.SchemaVersion) * FnvPrime;
                crc = (crc ^ (uint)config.ProviderType) * FnvPrime;
                crc = (crc ^ (config.StrictParticipantLock ? 1u : 0u)) * FnvPrime;
                crc = (crc ^ (config.EnforceConfigConsistency ? 1u : 0u)) * FnvPrime;
                crc = (crc ^ (uint)config.DefaultSessionRules.Mode) * FnvPrime;
                crc = (crc ^ (uint)config.DefaultSessionRules.MaxParticipants) * FnvPrime;
                return crc;
            }
        }

        private SessionConnectOptions NormalizeOptions(SessionConnectOptions options)
        {
            var localIdentity = options.LocalIdentity ?? new ParticipantIdentity("local-player", "Local Player");
            var rules = options.Rules ?? _config.DefaultSessionRules;
            var roomId = string.IsNullOrWhiteSpace(options.RoomId)
                ? $"room-{Guid.NewGuid():N}".Substring(0, 12)
                : options.RoomId.Trim();

            return new SessionConnectOptions(
                localIdentity,
                roomId,
                options.CreateIfNotExists,
                rules,
                options.ConfigChecksum);
        }

        private async Task<bool> FallbackToOfflineSoloAsync(SessionConnectOptions opts, string reason, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(reason))
                _logger.Warn($"Online session failed ({reason}). Falling back to local single-player.");

            await SafeCleanupAsync(ct);
            return StartOfflineSolo(opts);
        }

        private void CleanupHostAliasParticipants()
        {
            if (!_isHost || string.IsNullOrEmpty(_localPlayerId))
                return;

            for (int i = _participants.Count - 1; i >= 0; i--)
            {
                var participant = _participants[i];
                if (participant.IsHost && !string.Equals(participant.Identity.PlayerId, _localPlayerId, StringComparison.Ordinal))
                    _participants.RemoveAt(i);
            }

            int localIndex = _participants.FindIndex(p => string.Equals(p.Identity.PlayerId, _localPlayerId, StringComparison.Ordinal));
            if (localIndex >= 0 && !_participants[localIndex].IsHost)
                _participants[localIndex] = _participants[localIndex].AsHost();
        }
    }
}
