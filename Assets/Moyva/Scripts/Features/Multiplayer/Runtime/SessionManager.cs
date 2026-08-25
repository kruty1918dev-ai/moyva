using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Persistence;
using UnityEngine.SceneManagement;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    /// <summary>
    /// Orchestrates session lifecycle. In online mode it composes a UGS Lobby
    /// (room discovery / player list) with an <see cref="INetworkProvider"/>
    /// (Relay transport) by storing the Relay join code in lobby data.
    /// Participants are synced from the lobby's player list so identities
    /// are always the authoritative <c>PlayerId</c>.
    /// </summary>
    public sealed partial class SessionManager : ISessionManager, IDisposable
    {
        private const string LocalSessionPrefix = "local-session";
        private static readonly SessionRules LocalSessionRules = new SessionRules(
            SessionMode.Local,
            maxParticipants: 1,
            allowMatchSaveForAnalysis: false,
            strictParticipantLock: false);

        private readonly INetworkProvider _network;
        private readonly ILobbyService _lobby;
        private readonly IParticipantPolicyService _participantPolicy;
        private readonly IWorldConsistencyService _consistency;
        private readonly IWorldSnapshotStore _snapshotStore;
        private readonly IConfigStore _configStore;
        private readonly IMultiplayerLogger _logger;
        private readonly IMultiplayerSessionDiagnostics _diagnostics;
        private readonly IFailureHandlingPolicy _failurePolicy;
        private readonly IHostMigrationService _hostMigration;
        private readonly IHostMigrationCheckpointService _hostMigrationCheckpoint;

        private readonly List<Participant> _participants = new List<Participant>();
        private MultiplayerConfig _config;
        private string _currentSessionId;
        private string _currentLobbyId;
        private string _currentLobbyCode;
        private SessionRules _currentRules;
        private bool _isHost;
        private string _localPlayerId;
        private readonly Dictionary<string, CancellationTokenSource> _pendingDisconnects = new Dictionary<string, CancellationTokenSource>(StringComparer.Ordinal);

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
            [Zenject.InjectOptional] IMultiplayerSessionDiagnostics diagnostics,
            IFailureHandlingPolicy failurePolicy,
            IHostMigrationService hostMigration,
            IHostMigrationCheckpointService hostMigrationCheckpoint = null)
        {
            _network             = network             ?? throw new ArgumentNullException(nameof(network));
            _lobby               = lobby               ?? throw new ArgumentNullException(nameof(lobby));
            _participantPolicy   = participantPolicy   ?? throw new ArgumentNullException(nameof(participantPolicy));
            _consistency         = consistency         ?? throw new ArgumentNullException(nameof(consistency));
            _snapshotStore       = snapshotStore       ?? throw new ArgumentNullException(nameof(snapshotStore));
            _configStore         = configStore         ?? throw new ArgumentNullException(nameof(configStore));
            _logger              = logger              ?? throw new ArgumentNullException(nameof(logger));
            _diagnostics         = diagnostics;
            _failurePolicy       = failurePolicy       ?? throw new ArgumentNullException(nameof(failurePolicy));
            _hostMigration       = hostMigration       ?? throw new ArgumentNullException(nameof(hostMigration));
            _hostMigrationCheckpoint = hostMigrationCheckpoint;

            _network.PeerConnected    += OnPeerConnected;
            _network.PeerDisconnected += OnPeerDisconnected;
            _lobby.LobbyUpdated       += OnLobbyUpdated;
            _lobby.KickedFromLobby    += OnKickedFromLobby;
        }

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
            IHostMigrationCheckpointService hostMigrationCheckpoint = null)
            : this(
                network,
                lobby,
                participantPolicy,
                consistency,
                snapshotStore,
                configStore,
                logger,
                diagnostics: null,
                failurePolicy,
                hostMigration,
                hostMigrationCheckpoint)
        {
        }

        public void Dispose()
        {
            _network.PeerConnected    -= OnPeerConnected;
            _network.PeerDisconnected -= OnPeerDisconnected;
            _lobby.LobbyUpdated       -= OnLobbyUpdated;
            _lobby.KickedFromLobby    -= OnKickedFromLobby;
            CancelAllPendingDisconnects();
        }

    }
}
