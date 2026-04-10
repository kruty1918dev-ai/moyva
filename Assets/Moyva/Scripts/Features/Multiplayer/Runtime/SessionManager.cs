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
            _logger.Info($"CreateOrJoinSession: room={options.RoomId}, create={options.CreateIfNotExists}");

            // Config consistency check
            if (_config.EnforceConfigConsistency)
            {
                uint localChecksum = ComputeConfigChecksum(_config);
                if (options.ConfigChecksum != 0 && options.ConfigChecksum != localChecksum)
                {
                    _logger.Warn($"Config checksum mismatch: local={localChecksum:X8}, remote={options.ConfigChecksum:X8}");
                    _failurePolicy.HandleRecoverable(FailureCategory.ConfigMismatch, "Config checksums differ.");
                    return false;
                }
            }

            // Participant policy check (host joining their own session)
            WorldSnapshot snapshot = _snapshotStore.Exists(options.RoomId)
                ? _snapshotStore.Load(options.RoomId)
                : null;

            if (!_participantPolicy.CanJoin(options.LocalIdentity, _participants, options.Rules, snapshot))
            {
                _failurePolicy.HandleRecoverable(FailureCategory.ParticipantRejected, $"Participant {options.LocalIdentity.PlayerId} rejected.");
                return false;
            }

            // Network operation
            SessionResult result;
            if (options.CreateIfNotExists)
            {
                result = await _network.HostSessionAsync(options.RoomId, ct);
            }
            else
            {
                result = await _network.JoinSessionAsync(options.RoomId, ct);
            }

            if (!result.Success)
            {
                _logger.Error($"Network operation failed: {result.ErrorMessage}");
                _failurePolicy.HandleNonRecoverable(FailureCategory.NetworkDisconnect, result.ErrorMessage);
                return false;
            }

            _currentSessionId = result.SessionId;
            var localParticipant = new Participant(options.LocalIdentity, isBot: false, isHost: options.CreateIfNotExists);
            _participants.Add(localParticipant);

            _logger.Info($"Session established: {_currentSessionId}, host={options.CreateIfNotExists}");
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
            // Simple deterministic checksum over the key config fields.
            unchecked
            {
                uint crc = 2166136261u;
                crc = (crc ^ (uint)config.SchemaVersion) * 16777619u;
                crc = (crc ^ (uint)config.ProviderType) * 16777619u;
                crc = (crc ^ (config.StrictParticipantLock ? 1u : 0u)) * 16777619u;
                crc = (crc ^ (config.EnforceConfigConsistency ? 1u : 0u)) * 16777619u;
                crc = (crc ^ (uint)config.DefaultSessionRules.Mode) * 16777619u;
                crc = (crc ^ (uint)config.DefaultSessionRules.MaxParticipants) * 16777619u;
                return crc;
            }
        }
    }
}
