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
        private readonly IMultiplayerLogger _logger;

    #pragma warning disable CS0649
        [InjectOptional] private ISessionManager _sessionManager;
    #pragma warning restore CS0649

        private bool _suppressNextBroadcast;

        public StartingPositionSyncService(
            SignalBus signalBus,
            INetworkProvider networkProvider,
            IGameCommandSyncService commandSyncService,
            IMultiplayerLogger logger)
        {
            _signalBus = signalBus ?? throw new ArgumentNullException(nameof(signalBus));
            _networkProvider = networkProvider ?? throw new ArgumentNullException(nameof(networkProvider));
            _commandSyncService = commandSyncService ?? throw new ArgumentNullException(nameof(commandSyncService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            if (string.IsNullOrEmpty(peerId) || !ShouldBroadcastFromThisPeer() || _cachedAssignments == null || _cachedAssignments.Length == 0)
                return;

            _logger.Trace($"StartingPositionSyncService: rebroadcasting spawn positions to peer {peerId}.");
            _commandSyncService.SendCommand(GameCommandType.StartingPositions, SerializeAssignments(_cachedAssignments));
        }

        private void OnStartingPositionsCommand(string senderId, byte[] payload)
        {
            if (payload == null || payload.Length == 0)
                return;

            SpawnPositionAssignment[] assignments = DeserializeAssignments(payload);
            if (assignments == null || assignments.Length == 0)
                return;

            CacheAssignments(assignments);
            _logger.Trace($"StartingPositionSyncService: received {assignments.Length} assignments from {senderId}.");
            _suppressNextBroadcast = true;
            _signalBus.Fire(new WorldSpawnPositionsSignal
            {
                Assignments = assignments,
            });
        }

        private bool ShouldBroadcastFromThisPeer()
        {
            if (_sessionManager == null)
                return true;

            if (_sessionManager.Participants == null || _sessionManager.Participants.Count == 0)
                return string.IsNullOrEmpty(_sessionManager.LocalPlayerId) || _sessionManager.IsLocalPlayerHost;

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
                writer.Write(assignment.IsBot);
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
                    _logger.Warn($"StartingPositionSyncService: unsupported payload version {version}.");
                    return null;
                }

                int count = reader.ReadInt32();
                if (count <= 0)
                    return Array.Empty<SpawnPositionAssignment>();

                var assignments = new SpawnPositionAssignment[count];
                for (int index = 0; index < count; index++)
                {
                    assignments[index] = new SpawnPositionAssignment
                    {
                        SlotIndex = reader.ReadInt32(),
                        ParticipantId = reader.ReadString(),
                        IsBot = reader.ReadBoolean(),
                        Position = new Vector2Int(reader.ReadInt32(), reader.ReadInt32()),
                    };
                }

                return assignments;
            }
            catch (Exception exception)
            {
                _logger.Warn($"StartingPositionSyncService: failed to deserialize assignments: {exception.Message}");
                return null;
            }
        }
    }
}