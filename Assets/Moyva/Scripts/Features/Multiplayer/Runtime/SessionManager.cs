using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Persistence;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Orchestrates session lifecycle using injected abstractions.
    /// Carcass: no actual netcode, only domain flow.
    /// </summary>
    public sealed class SessionManager : ISessionManager
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
        private readonly IParticipantPolicyService _participantPolicy;
        private readonly IWorldConsistencyService _consistency;
        private readonly IWorldSnapshotStore _snapshotStore;
        private readonly IConfigStore _configStore;
        private readonly IMultiplayerLogger _logger;
        private readonly IFailureHandlingPolicy _failurePolicy;

        private readonly List<Participant> _participants = new List<Participant>();
        private MultiplayerConfig _config;
        private string _currentSessionId;

        public IReadOnlyList<Participant> Participants => _participants;

        public SessionManager(
            INetworkProvider network,
            IParticipantPolicyService participantPolicy,
            IWorldConsistencyService consistency,
            IWorldSnapshotStore snapshotStore,
            IConfigStore configStore,
            IMultiplayerLogger logger,
            IFailureHandlingPolicy failurePolicy)
        {
            _network = network ?? throw new ArgumentNullException(nameof(network));
            _participantPolicy = participantPolicy ?? throw new ArgumentNullException(nameof(participantPolicy));
            _consistency = consistency ?? throw new ArgumentNullException(nameof(consistency));
            _snapshotStore = snapshotStore ?? throw new ArgumentNullException(nameof(snapshotStore));
            _configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _failurePolicy = failurePolicy ?? throw new ArgumentNullException(nameof(failurePolicy));
        }

        public async Task<bool> CreateOrJoinSessionAsync(SessionConnectOptions options, CancellationToken ct = default)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            _config = _configStore.Load();
            var normalizedOptions = NormalizeOptions(options);
            _logger.Info($"CreateOrJoinSession: room={normalizedOptions.RoomId}, create={normalizedOptions.CreateIfNotExists}");

            // Config consistency check
            if (_config.EnforceConfigConsistency)
            {
                uint localChecksum = ComputeConfigChecksum(_config);
                if (normalizedOptions.ConfigChecksum != 0 && normalizedOptions.ConfigChecksum != localChecksum)
                {
                    _logger.Warn($"Config checksum mismatch: local={localChecksum:X8}, remote={normalizedOptions.ConfigChecksum:X8}");
                    _failurePolicy.HandleRecoverable(FailureCategory.ConfigMismatch, "Config checksums differ.");
                    return false;
                }
            }

            // Participant policy check (host joining their own session)
            WorldSnapshot snapshot = _snapshotStore.Exists(normalizedOptions.RoomId)
                ? _snapshotStore.Load(normalizedOptions.RoomId)
                : null;

            if (!_participantPolicy.CanJoin(normalizedOptions.LocalIdentity, _participants, normalizedOptions.Rules, snapshot))
            {
                _failurePolicy.HandleRecoverable(FailureCategory.ParticipantRejected, $"Participant {normalizedOptions.LocalIdentity.PlayerId} rejected.");
                return false;
            }

            // Network operation
            SessionResult result;
            if (normalizedOptions.CreateIfNotExists)
            {
                result = await _network.HostSessionAsync(normalizedOptions.RoomId, ct);
            }
            else
            {
                result = await _network.JoinSessionAsync(normalizedOptions.RoomId, ct);
            }

            if (!result.Success)
            {
                _logger.Warn($"Network operation failed: {result.ErrorMessage}. Trying offline solo fallback.");
                if (!TryStartOfflineSolo(normalizedOptions))
                {
                    _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, result.ErrorMessage);
                    return false;
                }

                return true;
            }

            _currentSessionId = result.SessionId;
            var localParticipant = new Participant(normalizedOptions.LocalIdentity, isBot: false, isHost: normalizedOptions.CreateIfNotExists);
            _participants.Add(localParticipant);

            _logger.Info($"Session established: {_currentSessionId}, host={normalizedOptions.CreateIfNotExists}");
            return true;
        }

        public async Task LeaveSessionAsync(CancellationToken ct = default)
        {
            _logger.Info($"Leaving session: {_currentSessionId}");
            await _network.LeaveSessionAsync(ct);
            _participants.Clear();
            _currentSessionId = null;
        }

        internal static uint ComputeConfigChecksum(MultiplayerConfig config)
        {
            // FNV-1a 32-bit hash over key config fields.
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
            var hasRoomId = !string.IsNullOrWhiteSpace(options.RoomId);
            var localIdentity = options.LocalIdentity ?? new ParticipantIdentity("local-player", "Local Player");
            var rules = options.Rules ?? _config.DefaultSessionRules;

            if (hasRoomId)
            {
                return new SessionConnectOptions(
                    localIdentity,
                    options.RoomId,
                    options.CreateIfNotExists,
                    rules,
                    options.ConfigChecksum);
            }

            var soloRoomId = BuildSoloRoomId();
            _logger.Warn($"RoomId is empty. Starting offline-compatible solo fallback session '{soloRoomId}'.");

            return new SessionConnectOptions(
                localIdentity,
                soloRoomId,
                createIfNotExists: true,
                rules: SoloFallbackRules,
                configChecksum: 0);
        }

        private bool TryStartOfflineSolo(SessionConnectOptions normalizedOptions)
        {
            try
            {
                var roomId = BuildSoloRoomId();
                _participants.Clear();
                _currentSessionId = roomId;
                _participants.Add(new Participant(normalizedOptions.LocalIdentity, isBot: false, isHost: true));
                _logger.Warn($"Session fallback activated: local single-player session '{roomId}'.");
                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to start local fallback session: {e.Message}");
                return false;
            }
        }

        private static string BuildSoloRoomId()
        {
            return $"{SoloSessionPrefix}-{Guid.NewGuid():N}";
        }
    }
}
