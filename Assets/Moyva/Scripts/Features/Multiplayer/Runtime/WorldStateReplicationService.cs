using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Units.API;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;
using Zenject;
using Kruty1918.EntityHealth;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Late-join catch-up channel that ships the current world state
    /// (placed buildings + per-owner economy pools) from the host to any
    /// peer that connects after the initial bootstrap completed.
    ///
    /// Host behaviour: when a new peer connects, builds a binary snapshot
    /// of every player-placed building and every owner's resource totals,
    /// and unicasts it to that peer using <see cref="GameCommandType.WorldStateSnapshot"/>.
    ///
    /// Client behaviour: when a snapshot arrives, restores buildings via
    /// <see cref="IConstructionSaveRestorer.RestoreFromSave"/> and resources via
    /// <see cref="EconomyManager.RestoreOwnerResourcePools"/>.
    /// </summary>
    internal sealed class WorldStateReplicationService : IInitializable, ITickable, IDisposable
    {
        private const byte SchemaVersion = 5;
        private const int MaxStatePayloadBytes =
            16 * 1024 * 1024;
        private const int MaxUnitSnapshotCount = 100000;

        // Snapshots travel as ordered chunks below the transport frame limit
        // (~60 KiB); the wire layout lives in WorldSnapshotChunkCodec.
        private const int MaxChunksPerTransfer =
            (MaxStatePayloadBytes / WorldSnapshotChunkCodec.DataBytes) + 1;
        private const int ChunksSentPerTick = 2;
        private const float ChunkTransferExpirySeconds = 30f;
        private const int MaxIncomingTransfers = 8;
        private const int MaxOutgoingTransfers = 16;

        private readonly struct OutgoingChunk
        {
            public readonly string TargetPeerId;
            public readonly byte[] Payload;

            public OutgoingChunk(string targetPeerId, byte[] payload)
            {
                TargetPeerId = targetPeerId;
                Payload = payload;
            }
        }

        private sealed class IncomingTransfer
        {
            public ushort Count;
            public int TotalSize;
            public byte[] Buffer;
            public readonly HashSet<ushort> ReceivedIndexes = new();
            public float ExpiresAt;
        }

        private readonly List<IConstructionModuleStatePersistence>
            _stateProviders;
        private readonly Queue<string> _pendingSnapshotPeers = new();
        private readonly HashSet<string> _pendingSnapshotPeerSet =
            new(StringComparer.Ordinal);
        private readonly Queue<OutgoingChunk> _outgoingChunks = new();
        private readonly Dictionary<string, IncomingTransfer> _incomingTransfers =
            new(StringComparer.Ordinal);
        private readonly IGameCommandSyncService _commandSync;
        private readonly INetworkProvider _network;
        private readonly ISessionManager _sessionManager;
        private readonly IConstructionSaveSnapshotSource _placementSnapshots;
        private readonly IConstructionSaveRestorer _placementRestorer;
        private readonly IConstructionSessionCommands _constructionSession;
        private readonly EconomyManager _economyManager;
        private readonly IFogOwnerStateReader _ownerFog;
        private readonly IFogIntelReader _intelReader;
        private readonly IFogIntelReplicationSink _intelSink;
        private readonly IFogOwnerExplorationSnapshotStore _ownerSnapshots;
        private readonly IUnitService _unitService;
        private readonly IUnitFactory _unitFactory;
        private readonly IUnitOwnershipQuery _unitOwnership;
        private readonly IHealthRegistry _healthRegistry;
        private readonly ITurnService _turns;
        private bool _disposed;
        private bool _receivedSnapshot;
        private float _nextRequestAt;
        private byte[] _pendingSnapshot;
        private uint _nextTransferId = 1;

        public WorldStateReplicationService(
            IGameCommandSyncService commandSync,
            INetworkProvider network,
            ISessionManager sessionManager,
            IConstructionSaveSnapshotSource placementSnapshots,
            IConstructionSaveRestorer placementRestorer,
            IConstructionSessionCommands constructionSession,
            [InjectOptional] EconomyManager economyManager = null,
            [InjectOptional] IFogOwnerStateReader ownerFog = null,
            [InjectOptional] IFogIntelReader intelReader = null,
            [InjectOptional] IFogIntelReplicationSink intelSink = null,
            [InjectOptional] IFogOwnerExplorationSnapshotStore ownerSnapshots = null,
            [InjectOptional] IUnitService unitService = null,
            [InjectOptional] IUnitFactory unitFactory = null,
            [InjectOptional] IUnitOwnershipQuery unitOwnership = null,
            [InjectOptional] IHealthRegistry healthRegistry = null,
            [InjectOptional]
            List<IConstructionModuleStatePersistence>
                stateProviders = null,
            [InjectOptional] ITurnService turns = null)
        {
            _commandSync = commandSync;
            _network = network;
            _sessionManager = sessionManager;
            _placementSnapshots = placementSnapshots;
            _placementRestorer = placementRestorer;
            _constructionSession = constructionSession;
            _economyManager = economyManager;
            _ownerFog = ownerFog;
            _intelReader = intelReader;
            _intelSink = intelSink;
            _ownerSnapshots = ownerSnapshots;
            _unitService = unitService;
            _unitFactory = unitFactory;
            _unitOwnership = unitOwnership;
            _healthRegistry = healthRegistry;
            _turns = turns;
            _stateProviders =
                stateProviders
                ?? new List<IConstructionModuleStatePersistence>();
        }

        public void Initialize()
        {
            _commandSync.RegisterHandler(GameCommandType.WorldStateSnapshot, OnSnapshotReceived);
            _commandSync.RegisterHandler(GameCommandType.WorldStateSnapshotChunk, OnSnapshotChunkReceived);
            _network.PeerConnected += OnPeerConnected;
        }

        public void Dispose()
        {
            _disposed = true;
            _network.PeerConnected -= OnPeerConnected;
            _commandSync.RegisterHandler(GameCommandType.WorldStateSnapshot, null);
            _commandSync.RegisterHandler(GameCommandType.WorldStateSnapshotChunk, null);
            _pendingSnapshot = null;
            _pendingSnapshotPeers.Clear();
            _pendingSnapshotPeerSet.Clear();
            _outgoingChunks.Clear();
            _incomingTransfers.Clear();
        }

        private bool WorldReady => _turns != null
            && (_turns.Phase == TurnPhase.AwaitingInput || _turns.Phase == TurnPhase.Completed);

        public void Tick()
        {
            if (_disposed) return;

            ExpireIncomingTransfers();

            if (_sessionManager.IsLocalPlayerHost)
            {
                DrainSnapshotJobs();
                DrainOutgoingChunks();
                return;
            }

            if (!WorldReady) return;
            if (_pendingSnapshot != null)
            {
                byte[] payload = _pendingSnapshot;
                _pendingSnapshot = null;
                TryApplySnapshot(payload);
            }
            if (_receivedSnapshot || Time.unscaledTime < _nextRequestAt) return;
            _nextRequestAt = Time.unscaledTime + 2f;
            foreach (var participant in _sessionManager.Participants)
            {
                if (participant?.IsHost != true || participant.Identity == null) continue;
                _commandSync.SendCommandToPeer(participant.Identity.PlayerId,
                    GameCommandType.WorldStateSnapshot, Array.Empty<byte>());
                break;
            }
        }

        // ── Host side ──────────────────────────────────────────────────────────

        private void OnPeerConnected(string peerId)
        {
            // Only the host ships the snapshot; clients ignore peer-connect events.
            if (_disposed || !_sessionManager.IsLocalPlayerHost || !WorldReady)
                return;
            if (string.IsNullOrEmpty(peerId))
                return;
            if (string.Equals(peerId, _sessionManager.LocalPlayerId, StringComparison.Ordinal))
                return;
            if (!MultiplayerAuthorityService.TryResolveAuthorizedRequestOwner(
                _sessionManager.Participants, peerId, peerId, peerId, out _, out _)) return;

            QueueSnapshotJob(peerId);
        }

        private void QueueSnapshotJob(string peerId)
        {
            if (_pendingSnapshotPeerSet.Add(peerId))
                _pendingSnapshotPeers.Enqueue(peerId);
        }

        // Serializing the world is a main-thread cost, so at most one snapshot
        // is built per tick instead of doing it inside the connect callback.
        private void DrainSnapshotJobs()
        {
            if (_pendingSnapshotPeers.Count == 0 || !WorldReady)
                return;
            if (_outgoingChunks.Count > MaxOutgoingTransfers * 4)
                return;

            var peerId = _pendingSnapshotPeers.Dequeue();
            _pendingSnapshotPeerSet.Remove(peerId);
            try
            {
                string targetOwnerId = ResolvePeerOwnerId(peerId);
                byte[] payload = BuildSnapshotPayload(targetOwnerId);
                if (payload == null || payload.Length == 0 || payload.Length > MaxStatePayloadBytes)
                    return;
                EnqueueSnapshotChunks(peerId, payload);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[WorldReplication] Could not capture snapshot: {exception.Message}");
            }
        }

        private void EnqueueSnapshotChunks(string peerId, byte[] payload)
        {
            var transferId = _nextTransferId++;
            if (transferId == 0)
                transferId = _nextTransferId++;

            var count = WorldSnapshotChunkCodec.ChunkCountFor(payload.Length);
            if (count > MaxChunksPerTransfer)
                return;

            for (var index = 0; index < count; index++)
            {
                var offset = index * WorldSnapshotChunkCodec.DataBytes;
                var length = Math.Min(
                    WorldSnapshotChunkCodec.DataBytes,
                    payload.Length - offset);
                _outgoingChunks.Enqueue(
                    new OutgoingChunk(
                        peerId,
                        WorldSnapshotChunkCodec.Build(
                            transferId,
                            (ushort)index,
                            (ushort)count,
                            payload.Length,
                            payload,
                            offset,
                            length)));
            }
        }

        private void DrainOutgoingChunks()
        {
            var sent = 0;
            while (sent < ChunksSentPerTick && _outgoingChunks.Count > 0)
            {
                var chunk = _outgoingChunks.Dequeue();
                _commandSync.SendCommandToPeer(
                    chunk.TargetPeerId,
                    GameCommandType.WorldStateSnapshotChunk,
                    chunk.Payload);
                sent++;
            }
        }

        private byte[] BuildSnapshotPayload(
            string targetOwnerId)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8);

            writer.Write(SchemaVersion);

            // Buildings with owner identity.
            IReadOnlyList<ConstructionSavedPlacement> buildings =
                _placementSnapshots.GetSavedPlacements();
            var visibleBuildings =
                FilterBuildingsForOwner(buildings, targetOwnerId);

            writer.Write(visibleBuildings.Count);
            for (int index = 0;
                 index < visibleBuildings.Count;
                 index++)
            {
                ConstructionSavedPlacement building =
                    visibleBuildings[index];
                writer.Write(building.Position.x);
                writer.Write(building.Position.y);
                writer.Write(
                    building.BuildingId
                    ?? string.Empty);
                writer.Write(
                    building.OwnerId
                    ?? string.Empty);
                writer.Write((int)building.Rotation);
            }

            var visiblePositions = new HashSet<Vector2Int>();
            foreach (var building in visibleBuildings) visiblePositions.Add(building.Position);
            WriteConstructionModuleStates(writer, targetOwnerId, visiblePositions);
            List<UnitSnapshotRecord> visibleUnits =
                WriteUnitSnapshots(writer, targetOwnerId);

            // Economy
            Dictionary<string, Dictionary<string, float>> pools =
                FilterEconomyPoolsForOwner(
                    _economyManager?.GetOwnerResourceTotalsSnapshot(),
                    targetOwnerId);

            writer.Write(pools.Count);
            foreach (var ownerPair in pools)
            {
                writer.Write(ownerPair.Key ?? string.Empty);
                var resources = ownerPair.Value;
                int count = resources?.Count ?? 0;
                writer.Write(count);
                if (resources == null) continue;
                foreach (var rp in resources)
                {
                    writer.Write(rp.Key ?? string.Empty);
                    writer.Write(rp.Value);
                }
            }

            // Schema v5: last-known intel + explored-bitmap for the receiving
            // owner. Everything below is filtered through the same per-owner
            // legitimacy rules — only remembered state is shipped.
            WriteIntelSection(writer, targetOwnerId, visibleUnits);
            WriteExploredSection(writer, targetOwnerId);

            writer.Flush();
            return stream.ToArray();
        }

        private void WriteIntelSection(
            BinaryWriter writer,
            string targetOwnerId,
            List<UnitSnapshotRecord> visibleUnits)
        {
            IReadOnlyCollection<FogIntelUnitRecord> rememberedUnits =
                _intelReader?.GetRememberedUnits(targetOwnerId);
            IReadOnlyCollection<FogIntelBuildingRecord> rememberedBuildings =
                _intelReader?.GetRememberedBuildings(targetOwnerId);

            var liveUnitIds = new HashSet<string>(StringComparer.Ordinal);
            if (visibleUnits != null)
            {
                for (int index = 0; index < visibleUnits.Count; index++)
                    liveUnitIds.Add(visibleUnits[index].UnitId);
            }

            var intelUnits = new List<FogIntelUnitRecord>();
            if (rememberedUnits != null)
            {
                foreach (FogIntelUnitRecord record in rememberedUnits)
                {
                    // Units shipped as live entities need no separate memory.
                    if (record != null
                        && !string.IsNullOrWhiteSpace(record.UnitId)
                        && !liveUnitIds.Contains(record.UnitId))
                    {
                        intelUnits.Add(record);
                    }
                }
            }

            var intelBuildings = new List<FogIntelBuildingRecord>();
            if (rememberedBuildings != null)
            {
                foreach (FogIntelBuildingRecord record in rememberedBuildings)
                {
                    if (record == null)
                        continue;
                    // A remembered building whose cell is currently visible is
                    // authoritative truth — it already shipped as a placement.
                    if (_ownerFog != null
                        && _ownerFog.IsVisible(targetOwnerId, record.Position))
                    {
                        continue;
                    }
                    intelBuildings.Add(record);
                }
            }

            writer.Write(intelUnits.Count);
            foreach (FogIntelUnitRecord record in intelUnits)
            {
                writer.Write(record.UnitId ?? string.Empty);
                writer.Write(record.TypeId ?? string.Empty);
                writer.Write(record.OwnerId ?? string.Empty);
                writer.Write(record.LastKnownPosition.x);
                writer.Write(record.LastKnownPosition.y);
                writer.Write(record.LastSeenSequence);
            }

            writer.Write(intelBuildings.Count);
            foreach (FogIntelBuildingRecord record in intelBuildings)
            {
                writer.Write(record.BuildingId ?? string.Empty);
                writer.Write(record.OwnerId ?? string.Empty);
                writer.Write(record.Position.x);
                writer.Write(record.Position.y);
                writer.Write(record.RotationQuarterTurns);
                writer.Write(record.LastSeenSequence);
            }
        }

        private void WriteExploredSection(
            BinaryWriter writer,
            string targetOwnerId)
        {
            bool[,] explored =
                _ownerSnapshots?.GetExploredSnapshot(targetOwnerId);
            if (explored == null)
            {
                writer.Write(0);
                writer.Write(0);
                writer.Write(0);
                return;
            }

            int width = explored.GetLength(0);
            int height = explored.GetLength(1);
            int byteCount = (width * height + 7) / 8;
            var packed = new byte[byteCount];
            int bitIndex = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++, bitIndex++)
                {
                    if (explored[x, y])
                        packed[bitIndex >> 3] |= (byte)(1 << (bitIndex & 7));
                }
            }

            writer.Write(width);
            writer.Write(height);
            writer.Write(packed.Length);
            writer.Write(packed);
        }

        private List<UnitSnapshotRecord> WriteUnitSnapshots(
            BinaryWriter writer,
            string targetOwnerId)
        {
            if (_unitService == null)
            {
                writer.Write(0);
                return new List<UnitSnapshotRecord>(0);
            }

            IReadOnlyCollection<string> unitIds =
                _unitService.GetAllUnitIds();
            if (unitIds == null || unitIds.Count == 0)
            {
                writer.Write(0);
                return new List<UnitSnapshotRecord>(0);
            }

            string ownerId = NormalizeOwnerId(targetOwnerId);
            var visibleUnits =
                new List<UnitSnapshotRecord>(unitIds.Count);
            foreach (string unitId in unitIds)
            {
                if (string.IsNullOrWhiteSpace(unitId)
                    || !_unitService.TryGetUnitPosition(
                        unitId,
                        out Vector2Int position))
                {
                    continue;
                }

                string unitOwnerId =
                    NormalizeOwnerId(
                        _unitOwnership?.GetUnitOwnerId(unitId));
                if (!CanIncludeUnitForOwner(
                        ownerId,
                        unitOwnerId,
                        position))
                {
                    continue;
                }

                int currentHp = 0;
                if (_healthRegistry != null
                    && _healthRegistry.TryGet(unitId, out IHealth health)
                    && health != null)
                {
                    currentHp = health.CurrentHp;
                }

                visibleUnits.Add(new UnitSnapshotRecord(
                    unitId.Trim(),
                    _unitService.GetUnitTypeId(unitId) ?? string.Empty,
                    unitOwnerId,
                    position,
                    _unitService.GetStamina(unitId),
                    currentHp));
            }

            writer.Write(visibleUnits.Count);
            for (int index = 0;
                 index < visibleUnits.Count;
                 index++)
            {
                UnitSnapshotRecord unit =
                    visibleUnits[index];
                writer.Write(unit.UnitId);
                writer.Write(unit.TypeId);
                writer.Write(unit.OwnerId);
                writer.Write(unit.Position.x);
                writer.Write(unit.Position.y);
                writer.Write(unit.Stamina);
                writer.Write(unit.CurrentHp);
            }

            return visibleUnits;
        }

        private bool CanIncludeUnitForOwner(
            string targetOwnerId,
            string unitOwnerId,
            Vector2Int position)
        {
            // Fail closed: an unresolved receiving owner means we cannot prove
            // legitimacy — ship no unit state at all.
            if (string.IsNullOrWhiteSpace(targetOwnerId))
                return false;

            if (!string.IsNullOrWhiteSpace(unitOwnerId)
                && string.Equals(
                    targetOwnerId,
                    unitOwnerId,
                    StringComparison.Ordinal))
            {
                return true;
            }

            return _ownerFog != null
                   && _ownerFog.IsVisible(
                       targetOwnerId,
                       position);
        }

        private void ReadIntelSection(
            BinaryReader reader,
            string localOwnerId)
        {
            int unitCount = Math.Max(0, reader.ReadInt32());
            if (unitCount > MaxUnitSnapshotCount)
                throw new InvalidDataException(
                    $"Invalid intel unit count {unitCount}.");

            for (int index = 0; index < unitCount; index++)
            {
                var record = new FogIntelUnitRecord
                {
                    UnitId = reader.ReadString(),
                    TypeId = reader.ReadString(),
                    OwnerId = reader.ReadString(),
                    LastKnownPosition = new Vector2Int(
                        reader.ReadInt32(),
                        reader.ReadInt32()),
                    LastSeenSequence = reader.ReadInt64(),
                };
                if (!string.IsNullOrWhiteSpace(record.UnitId))
                    _intelSink?.ApplyReplicatedUnit(localOwnerId, record);
            }

            int buildingCount = Math.Max(0, reader.ReadInt32());
            if (buildingCount > MaxStatePayloadBytes / 16)
                throw new InvalidDataException(
                    $"Invalid intel building count {buildingCount}.");

            for (int index = 0; index < buildingCount; index++)
            {
                var record = new FogIntelBuildingRecord
                {
                    BuildingId = reader.ReadString(),
                    OwnerId = reader.ReadString(),
                    Position = new Vector2Int(
                        reader.ReadInt32(),
                        reader.ReadInt32()),
                    RotationQuarterTurns = reader.ReadInt32(),
                    LastSeenSequence = reader.ReadInt64(),
                };
                _intelSink?.ApplyReplicatedBuilding(localOwnerId, record);
            }
        }

        private void ReadExploredSection(
            BinaryReader reader,
            string localOwnerId)
        {
            int width = Math.Max(0, reader.ReadInt32());
            int height = Math.Max(0, reader.ReadInt32());
            int byteCount = Math.Max(0, reader.ReadInt32());
            if (width == 0 || height == 0 || byteCount == 0)
                return;
            if (width > 4096 || height > 4096
                || byteCount != (width * height + 7) / 8)
            {
                throw new InvalidDataException(
                    $"Invalid explored bitmap {width}x{height} ({byteCount} bytes).");
            }

            byte[] packed = reader.ReadBytes(byteCount);
            if (packed.Length != byteCount)
                throw new EndOfStreamException("Explored bitmap truncated.");

            var explored = new bool[width, height];
            int bitIndex = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++, bitIndex++)
                {
                    explored[x, y] =
                        (packed[bitIndex >> 3] & (1 << (bitIndex & 7))) != 0;
                }
            }

            _ownerSnapshots?.LoadFromSnapshot(localOwnerId, explored);
        }

        private string ResolvePeerOwnerId(
            string peerId)
        {
            string normalizedPeerId = NormalizeOwnerId(peerId);
            if (string.IsNullOrWhiteSpace(normalizedPeerId))
                return string.Empty;

            IReadOnlyList<Participant> participants =
                _sessionManager?.Participants;
            if (participants == null || participants.Count == 0)
                return normalizedPeerId;

            for (int index = 0;
                 index < participants.Count;
                 index++)
            {
                string participantId =
                    NormalizeOwnerId(
                        participants[index]?.Identity?.PlayerId);
                if (string.Equals(
                        participantId,
                        normalizedPeerId,
                        StringComparison.Ordinal))
                {
                    return participantId;
                }
            }

            return normalizedPeerId;
        }

        private List<ConstructionSavedPlacement> FilterBuildingsForOwner(
            IReadOnlyList<ConstructionSavedPlacement> buildings,
            string targetOwnerId)
        {
            var result = new List<ConstructionSavedPlacement>();
            if (buildings == null || buildings.Count == 0)
                return result;

            string ownerId = NormalizeOwnerId(targetOwnerId);
            bool canFilterByFog =
                _ownerFog != null
                && !string.IsNullOrWhiteSpace(ownerId);

            for (int index = 0;
                 index < buildings.Count;
                 index++)
            {
                ConstructionSavedPlacement building =
                    buildings[index];
                string buildingOwnerId =
                    NormalizeOwnerId(building.OwnerId);

                bool isOwnBuilding =
                    !string.IsNullOrWhiteSpace(ownerId)
                    && string.Equals(
                        buildingOwnerId,
                        ownerId,
                        StringComparison.Ordinal);
                if (isOwnBuilding
                    || (canFilterByFog && _ownerFog.IsVisible(ownerId, building.Position)))
                {
                    result.Add(building);
                }
            }

            return result;
        }

        private static Dictionary<string, Dictionary<string, float>>
            FilterEconomyPoolsForOwner(
                Dictionary<string, Dictionary<string, float>> pools,
                string targetOwnerId)
        {
            var result =
                new Dictionary<string, Dictionary<string, float>>(
                    StringComparer.Ordinal);
            if (pools == null || pools.Count == 0)
                return result;

            string ownerId = NormalizeOwnerId(targetOwnerId);
            if (string.IsNullOrWhiteSpace(ownerId))
            {
                // Fail closed: an unresolved receiving owner gets no economy
                // data — broadcasting every player's pool leaks hidden info.
                return result;
            }

            if (pools.TryGetValue(ownerId, out var exact)
                && exact != null
                && exact.Count > 0)
            {
                result[ownerId] =
                    new Dictionary<string, float>(
                        exact,
                        StringComparer.Ordinal);
                return result;
            }

            foreach (var pair in pools)
            {
                string candidateOwnerId =
                    NormalizeOwnerId(pair.Key);
                if (!string.Equals(
                        candidateOwnerId,
                        ownerId,
                        StringComparison.Ordinal)
                    || pair.Value == null
                    || pair.Value.Count == 0)
                {
                    continue;
                }

                result[ownerId] =
                    new Dictionary<string, float>(
                        pair.Value,
                        StringComparer.Ordinal);
                break;
            }

            return result;
        }

        private static string NormalizeOwnerId(
            string ownerId)
        {
            return string.IsNullOrWhiteSpace(ownerId)
                ? string.Empty
                : ownerId.Trim();
        }

        private void WriteConstructionModuleStates(
            BinaryWriter writer, string ownerId, ISet<Vector2Int> visiblePositions)
        {
            var providers =
                new List<IConstructionModuleStatePersistence>();
            var keys =
                new HashSet<string>(
                    StringComparer.Ordinal);

            for (int index = 0;
                 index < _stateProviders.Count;
                 index++)
            {
                IConstructionModuleStatePersistence provider =
                    _stateProviders[index];
                string key = provider?.StateKey?.Trim();
                if (provider == null
                    || string.IsNullOrWhiteSpace(key)
                    || !keys.Add(key))
                {
                    continue;
                }

                providers.Add(provider);
            }

            providers.Sort(
                (left, right) =>
                    string.CompareOrdinal(
                        left.StateKey,
                        right.StateKey));

            writer.Write(providers.Count);
            for (int index = 0;
                 index < providers.Count;
                 index++)
            {
                IConstructionModuleStatePersistence provider =
                    providers[index];

                if (!(provider is IConstructionObserverStateSource observerSource))
                    throw new InvalidOperationException($"Module '{provider.StateKey}' cannot filter observer state.");
                byte[] payload = observerSource.CaptureObserverState(ownerId, visiblePositions)
                    ?? throw new InvalidOperationException($"Module '{provider.StateKey}' returned no observer state.");

                writer.Write(provider.StateKey);
                writer.Write(payload.Length);
                if (payload.Length > 0)
                    writer.Write(payload);
            }
        }

        private void ReadConstructionModuleStates(
            BinaryReader reader)
        {
            var providers =
                new Dictionary<
                    string,
                    IConstructionModuleStatePersistence>(
                    StringComparer.Ordinal);

            for (int index = 0;
                 index < _stateProviders.Count;
                 index++)
            {
                IConstructionModuleStatePersistence provider =
                    _stateProviders[index];
                string key = provider?.StateKey?.Trim();
                if (provider != null
                    && !string.IsNullOrWhiteSpace(key)
                    && !providers.ContainsKey(key))
                {
                    providers[key] = provider;
                }
            }

            int stateCount =
                Math.Max(0, reader.ReadInt32());
            int restored = 0;

            for (int index = 0;
                 index < stateCount;
                 index++)
            {
                string key = reader.ReadString();
                int length = reader.ReadInt32();

                if (length < 0
                    || length > MaxStatePayloadBytes)
                {
                    throw new InvalidDataException(
                        $"Invalid construction module payload " +
                        $"'{key}' length={length}.");
                }

                byte[] payload =
                    reader.ReadBytes(length);
                if (payload.Length != length)
                {
                    throw new EndOfStreamException(
                        $"Construction module payload '{key}' truncated.");
                }

                if (!providers.TryGetValue(
                        key,
                        out IConstructionModuleStatePersistence provider))
                {
                    continue;
                }

                provider.RestoreState(payload);
                restored++;
            }
        }

        // ── Client side ────────────────────────────────────────────────────────

        private void OnSnapshotReceived(string senderId, byte[] payload)
        {
            if (_disposed) return;
            if (_sessionManager.IsLocalPlayerHost)
            {
                if (payload != null && payload.Length == 0) OnPeerConnected(senderId);
                return;
            }
            if (!MultiplayerAuthorityService.IsAuthorizedHostSender(_sessionManager.Participants, senderId)
                || payload == null || payload.Length < 1 || payload.Length > MaxStatePayloadBytes)
            {
                return;
            }

            if (!WorldReady)
            {
                _pendingSnapshot = payload;
                return;
            }
            TryApplySnapshot(payload);
        }

        private void OnSnapshotChunkReceived(string senderId, byte[] payload)
        {
            if (_disposed || _sessionManager.IsLocalPlayerHost)
                return;
            if (!MultiplayerAuthorityService.IsAuthorizedHostSender(_sessionManager.Participants, senderId))
                return;
            if (!WorldSnapshotChunkCodec.TryParse(
                    payload,
                    WorldSnapshotChunkCodec.DataBytes,
                    MaxStatePayloadBytes,
                    out var transferId,
                    out var index,
                    out var count,
                    out var totalSize,
                    out var dataOffset,
                    out var dataLength)
                || count > MaxChunksPerTransfer)
            {
                return;
            }

            var key = senderId + "#" + transferId;
            if (!_incomingTransfers.TryGetValue(key, out var transfer))
            {
                if (_incomingTransfers.Count >= MaxIncomingTransfers)
                    return;
                transfer = new IncomingTransfer
                {
                    Count = count,
                    TotalSize = totalSize,
                    Buffer = new byte[totalSize]
                };
                _incomingTransfers.Add(key, transfer);
            }
            else if (transfer.Count != count || transfer.TotalSize != totalSize)
            {
                _incomingTransfers.Remove(key);
                return;
            }

            var destOffset = index * WorldSnapshotChunkCodec.DataBytes;
            if (destOffset < 0 || destOffset >= totalSize)
            {
                _incomingTransfers.Remove(key);
                return;
            }

            if (transfer.ReceivedIndexes.Add(index))
            {
                Buffer.BlockCopy(
                    payload,
                    dataOffset,
                    transfer.Buffer,
                    destOffset,
                    Math.Min(dataLength, totalSize - destOffset));
            }
            transfer.ExpiresAt = Time.unscaledTime + ChunkTransferExpirySeconds;

            if (transfer.ReceivedIndexes.Count < transfer.Count)
                return;

            _incomingTransfers.Remove(key);
            var assembled = transfer.Buffer;
            if (!WorldReady)
            {
                _pendingSnapshot = assembled;
                return;
            }
            TryApplySnapshot(assembled);
        }

        private void ExpireIncomingTransfers()
        {
            if (_incomingTransfers.Count == 0)
                return;

            var now = Time.unscaledTime;
            List<string> expired = null;
            foreach (var pair in _incomingTransfers)
            {
                if (pair.Value == null || now < pair.Value.ExpiresAt)
                    continue;
                expired ??= new List<string>();
                expired.Add(pair.Key);
            }

            if (expired == null)
                return;
            foreach (var key in expired)
                _incomingTransfers.Remove(key);
        }

        private void TryApplySnapshot(byte[] payload)
        {
            try
            {
                ApplySnapshotPayload(payload);
                _receivedSnapshot = true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[WorldReplication] Could not restore snapshot: {exception.Message}");
            }
        }

        private void ApplySnapshotPayload(byte[] payload)
        {
            using var stream = new MemoryStream(payload);
            using var reader = new BinaryReader(stream, Encoding.UTF8);

            byte version = reader.ReadByte();
            if (version < 1 || version > SchemaVersion)
            {
                throw new InvalidDataException($"Unsupported world snapshot version {version}.");
            }

            // Buildings
            int buildingCount =
                Math.Max(0, reader.ReadInt32());
            for (int i = 0; i < buildingCount; i++)
            {
                int x = reader.ReadInt32();
                int y = reader.ReadInt32();
                string id = reader.ReadString();
                string ownerId =
                    version >= 2
                        ? reader.ReadString()
                        : _constructionSession.GetActiveOwner();
                var rotation =
                    version >= 3
                        ? (ConstructionRotation)reader.ReadInt32()
                        : ConstructionRotation.Degrees0;

                if (string.IsNullOrWhiteSpace(id))
                    continue;

                var position = new Vector2Int(x, y);
                _placementRestorer.RestoreFromSave(
                    position,
                    id,
                    ownerId,
                    rotation);
            }

            if (version >= 2)
                ReadConstructionModuleStates(reader);
            if (version >= 4)
                ReadUnitSnapshots(reader);

            // Economy
            int ownerCount = reader.ReadInt32();
            var restored = new Dictionary<string, Dictionary<string, float>>(StringComparer.Ordinal);
            for (int o = 0; o < ownerCount; o++)
            {
                string ownerId = reader.ReadString();
                int rc = reader.ReadInt32();
                var pool = new Dictionary<string, float>(StringComparer.Ordinal);
                for (int r = 0; r < rc; r++)
                {
                    string resourceId = reader.ReadString();
                    float amount = reader.ReadSingle();
                    if (string.IsNullOrWhiteSpace(resourceId) || amount <= 0f)
                        continue;
                    pool[resourceId.Trim()] = amount;
                }
                if (!string.IsNullOrWhiteSpace(ownerId) && pool.Count > 0)
                    restored[ownerId.Trim()] = pool;
            }

            _economyManager?.RestoreOwnerResourcePools(restored);

            if (version >= 5 && stream.Position < stream.Length)
            {
                string localOwnerId =
                    string.IsNullOrWhiteSpace(_sessionManager?.LocalPlayerId)
                        ? string.Empty
                        : _sessionManager.LocalPlayerId.Trim();
                if (!string.IsNullOrWhiteSpace(localOwnerId))
                {
                    ReadIntelSection(reader, localOwnerId);
                    ReadExploredSection(reader, localOwnerId);
                }
            }
        }

        private void ReadUnitSnapshots(
            BinaryReader reader)
        {
            int unitCount =
                Math.Max(0, reader.ReadInt32());
            if (unitCount > MaxUnitSnapshotCount)
            {
                throw new InvalidDataException(
                    $"Invalid unit snapshot count {unitCount}.");
            }

            for (int index = 0;
                 index < unitCount;
                 index++)
            {
                string unitId = reader.ReadString();
                string typeId = reader.ReadString();
                string ownerId = reader.ReadString();
                var position =
                    new Vector2Int(
                        reader.ReadInt32(),
                        reader.ReadInt32());
                float stamina = reader.ReadSingle();
                int currentHp = reader.ReadInt32();

                if (_unitFactory == null
                    || _unitService == null
                    || string.IsNullOrWhiteSpace(unitId)
                    || string.IsNullOrWhiteSpace(typeId))
                {
                    continue;
                }

                string normalizedUnitId = unitId.Trim();
                if (_unitService.TryGetUnitPosition(
                        normalizedUnitId,
                        out _))
                {
                    continue;
                }

                string createdUnitId =
                    _unitFactory.CreateUnitWithId(
                        normalizedUnitId,
                        typeId.Trim(),
                        position,
                        ownerId);
                if (string.IsNullOrWhiteSpace(createdUnitId))
                    continue;

                if (!float.IsNaN(stamina)
                    && !float.IsInfinity(stamina)
                    && stamina >= 0f)
                {
                    _unitService.SetStamina(
                        createdUnitId,
                        stamina);
                }

                if (currentHp > 0
                    && _healthRegistry != null
                    && _healthRegistry.TryGet(
                        createdUnitId,
                        out IHealth health)
                    && health != null)
                {
                    int damageToRestore =
                        Math.Max(
                            0,
                            health.CurrentHp - currentHp);
                    if (damageToRestore > 0)
                        health.TakeDamage(damageToRestore);
                }
            }
        }

        private readonly struct UnitSnapshotRecord
        {
            public UnitSnapshotRecord(
                string unitId,
                string typeId,
                string ownerId,
                Vector2Int position,
                float stamina,
                int currentHp)
            {
                UnitId = unitId ?? string.Empty;
                TypeId = typeId ?? string.Empty;
                OwnerId = ownerId ?? string.Empty;
                Position = position;
                Stamina = stamina;
                CurrentHp = currentHp;
            }

            public string UnitId { get; }
            public string TypeId { get; }
            public string OwnerId { get; }
            public Vector2Int Position { get; }
            public float Stamina { get; }
            public int CurrentHp { get; }
        }
    }
}
