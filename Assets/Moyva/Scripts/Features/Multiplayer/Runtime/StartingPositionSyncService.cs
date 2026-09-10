using System;
using System.Collections.Generic;
using System.IO;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Signals;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Broadcasts world spawn assignments from the authoritative host and
    /// re-fires the same signal on clients when the data arrives from network.
    /// </summary>
    public sealed class StartingPositionSyncService : IInitializable, IDisposable
    {
        private const int PayloadVersion = 1;

        private readonly SignalBus _signalBus;
        private readonly INetworkProvider _networkProvider;
        private readonly IGameCommandSyncService _commandSyncService;
        private readonly IWorldGenerationSignalState _worldGenerationSignalState;

    #pragma warning disable CS0649
        [InjectOptional] private ISessionManager _sessionManager;
        [InjectOptional] private IMultiplayerStartupBarrier _startupBarrier;
    #pragma warning restore CS0649

        private bool _suppressNextBroadcast;

        public StartingPositionSyncService(
            SignalBus signalBus,
            INetworkProvider networkProvider,
            IGameCommandSyncService commandSyncService,
            [InjectOptional] IWorldGenerationSignalState worldGenerationSignalState = null)
        {
            _signalBus = signalBus ?? throw new ArgumentNullException(nameof(signalBus));
            _networkProvider = networkProvider ?? throw new ArgumentNullException(nameof(networkProvider));
            _commandSyncService = commandSyncService ?? throw new ArgumentNullException(nameof(commandSyncService));
            _worldGenerationSignalState = worldGenerationSignalState;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<WorldSpawnPositionsSignal>(OnWorldSpawnPositions);
            _networkProvider.PeerConnected += OnPeerConnected;
            _commandSyncService.RegisterHandler(GameCommandType.StartingPositions, OnStartingPositionsCommand);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<WorldSpawnPositionsSignal>(OnWorldSpawnPositions);
            _networkProvider.PeerConnected -= OnPeerConnected;
            _commandSyncService.RegisterHandler(GameCommandType.StartingPositions, null);
        }

        private void OnWorldSpawnPositions(WorldSpawnPositionsSignal signal)
        {
            if (_suppressNextBroadcast)
            {
                _suppressNextBroadcast = false;
                return;
            }

            if (signal.Assignments == null || signal.Assignments.Length == 0)
                return;

            CacheAssignments(signal.Assignments);

            if (!ShouldBroadcastFromThisPeer())
                return;

            _commandSyncService.SendCommand(GameCommandType.StartingPositions, SerializeAssignments(signal.Assignments));
        }

        private void OnPeerConnected(string peerId)
        {
            if (_startupBarrier != null && !_startupBarrier.IsHostReady)
                return;
            if (string.IsNullOrEmpty(peerId) || !ShouldBroadcastFromThisPeer() || _cachedAssignments == null || _cachedAssignments.Length == 0)
                return;
            _commandSyncService.SendCommand(GameCommandType.StartingPositions, SerializeAssignments(_cachedAssignments));
        }

        private void OnStartingPositionsCommand(string senderId, byte[] payload)
        {
            if (_sessionManager?.IsLocalPlayerHost == true && payload != null && payload.Length == 0)
            {
                // Catch up a client whose world cycle started after the broadcast.
                // Never replay the previous world's cached positions during a new load.
                if (_startupBarrier?.IsHostReady != true || _worldGenerationSignalState == null)
                    return;
                foreach (var participant in _sessionManager.Participants)
                {
                    if (participant?.Identity?.PlayerId != senderId || participant.IsHost)
                        continue;
                    if (_worldGenerationSignalState.TryGetWorldSpawnPositions(out var current) && current.Assignments?.Length > 0)
                        _commandSyncService.SendCommandToPeer(senderId, GameCommandType.StartingPositions,
                            SerializeAssignments(current.Assignments));
                    return;
                }
                return;
            }

            if (_sessionManager == null
                || _sessionManager.IsLocalPlayerHost
                || !MultiplayerAuthorityService.IsAuthorizedHostSender(
                    _sessionManager.Participants, senderId))
            {
                return;
            }

            if (payload == null || payload.Length == 0)
                return;

            SpawnPositionAssignment[] assignments = DeserializeAssignments(payload);
            if (assignments == null || assignments.Length == 0)
                return;

            CacheAssignments(assignments);
            long startupSequence = 0;
            string startupSessionId = null;
            if (_worldGenerationSignalState != null)
                _worldGenerationSignalState.TryGetCurrentWorldIdentity(out startupSequence, out startupSessionId);
            var spawnPositionsSignal = new WorldSpawnPositionsSignal
            {
                StartupSequence = startupSequence,
                StartupSessionId = startupSessionId,
                Source = WorldSpawnPositionsSource.MultiplayerSync,
                PublishedFrame = Time.frameCount,
                Assignments = assignments,
            };
            if (_worldGenerationSignalState == null || _worldGenerationSignalState.TryStoreWorldSpawnPositions(spawnPositionsSignal, out spawnPositionsSignal))
            {
                _suppressNextBroadcast = true;
                try
                {
                    _signalBus.Fire(spawnPositionsSignal);
                }
                finally
                {
                    _suppressNextBroadcast = false;
                }
            }
        }

        private bool ShouldBroadcastFromThisPeer()
        {
            if (_sessionManager == null)
                return true;

            if (string.IsNullOrEmpty(_sessionManager.LocalPlayerId))
                return false;

            return _sessionManager.IsLocalPlayerHost;
        }

        private SpawnPositionAssignment[] _cachedAssignments;

        private void CacheAssignments(IReadOnlyList<SpawnPositionAssignment> assignments)
        {
            if (assignments == null || assignments.Count == 0)
            {
                _cachedAssignments = null;
                return;
            }

            var copy = new SpawnPositionAssignment[assignments.Count];
            for (int index = 0; index < assignments.Count; index++)
                copy[index] = assignments[index];

            _cachedAssignments = copy;
        }

        private static byte[] SerializeAssignments(IReadOnlyList<SpawnPositionAssignment> assignments)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            writer.Write(PayloadVersion);
            writer.Write(assignments.Count);
            for (int index = 0; index < assignments.Count; index++)
            {
                SpawnPositionAssignment assignment = assignments[index];
                writer.Write(assignment.SlotIndex);
                writer.Write(assignment.ParticipantId ?? string.Empty);
                writer.Write(false);
                writer.Write(assignment.Position.x);
                writer.Write(assignment.Position.y);
            }

            writer.Flush();
            return stream.ToArray();
        }

        private SpawnPositionAssignment[] DeserializeAssignments(byte[] payload)
        {
            try
            {
                using var stream = new MemoryStream(payload, writable: false);
                using var reader = new BinaryReader(stream);

                int version = reader.ReadInt32();
                if (version != PayloadVersion)
                {
                    return null;
                }

                int count = reader.ReadInt32();
                if (count <= 0)
                    return Array.Empty<SpawnPositionAssignment>();

                var assignments = new SpawnPositionAssignment[count];
                for (int index = 0; index < count; index++)
                {
                    int slotIndex = reader.ReadInt32();
                    string participantId = reader.ReadString();
                    reader.ReadBoolean();
                    assignments[index] = new SpawnPositionAssignment
                    {
                        SlotIndex = slotIndex,
                        ParticipantId = participantId,
                        Position = new Vector2Int(reader.ReadInt32(), reader.ReadInt32()),
                    };
                }

                return assignments;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
