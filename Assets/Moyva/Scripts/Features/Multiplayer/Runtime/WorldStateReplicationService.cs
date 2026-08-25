using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using UnityEngine;
using Zenject;

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
    internal sealed class WorldStateReplicationService : IInitializable, IDisposable
    {
        private const byte SchemaVersion = 2;
        private const int MaxStatePayloadBytes =
            16 * 1024 * 1024;
        private readonly IGameCommandSyncService _commandSync;
        private readonly INetworkProvider _network;
        private readonly ISessionManager _sessionManager;
        private readonly IConstructionSaveSnapshotSource _placementSnapshots;
        private readonly IConstructionSaveRestorer _placementRestorer;
        private readonly IConstructionSessionCommands _constructionSession;
        private readonly EconomyManager _economyManager;
        private readonly List<IConstructionModuleStatePersistence>
            _stateProviders;

        public WorldStateReplicationService(
            IGameCommandSyncService commandSync,
            INetworkProvider network,
            ISessionManager sessionManager,
            IConstructionSaveSnapshotSource placementSnapshots,
            IConstructionSaveRestorer placementRestorer,
            IConstructionSessionCommands constructionSession,
            [InjectOptional] EconomyManager economyManager = null,
            [InjectOptional]
            List<IConstructionModuleStatePersistence>
                stateProviders = null)
        {
            _commandSync = commandSync;
            _network = network;
            _sessionManager = sessionManager;
            _placementSnapshots = placementSnapshots;
            _placementRestorer = placementRestorer;
            _constructionSession = constructionSession;
            _economyManager = economyManager;
            _stateProviders =
                stateProviders
                ?? new List<IConstructionModuleStatePersistence>();
        }

        public void Initialize()
        {
            _commandSync.RegisterHandler(GameCommandType.WorldStateSnapshot, OnSnapshotReceived);
            _network.PeerConnected += OnPeerConnected;
        }

        public void Dispose()
        {
            _network.PeerConnected -= OnPeerConnected;
        }

        // ── Host side ──────────────────────────────────────────────────────────

        private void OnPeerConnected(string peerId)
        {
            // Only the host ships the snapshot; clients ignore peer-connect events.
            if (!_sessionManager.IsLocalPlayerHost)
                return;
            if (string.IsNullOrEmpty(peerId))
                return;
            if (string.Equals(peerId, _sessionManager.LocalPlayerId, StringComparison.Ordinal))
                return;

            try
            {
                byte[] payload = BuildSnapshotPayload();
                _commandSync.SendCommandToPeer(peerId, GameCommandType.WorldStateSnapshot, payload);
            }
            catch (Exception)
            {
            }
        }

        private byte[] BuildSnapshotPayload()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8);

            writer.Write(SchemaVersion);

            // Buildings with owner identity.
            IReadOnlyList<ConstructionSavedPlacement> buildings =
                _placementSnapshots.GetSavedPlacements();

            writer.Write(buildings.Count);
            for (int index = 0;
                 index < buildings.Count;
                 index++)
            {
                ConstructionSavedPlacement building =
                    buildings[index];
                writer.Write(building.Position.x);
                writer.Write(building.Position.y);
                writer.Write(
                    building.BuildingId
                    ?? string.Empty);
                writer.Write(
                    building.OwnerId
                    ?? string.Empty);
            }

            WriteConstructionModuleStates(writer);

            // Economy
            Dictionary<string, Dictionary<string, float>> pools =
                _economyManager?.GetOwnerResourceTotalsSnapshot()
                ?? new Dictionary<string, Dictionary<string, float>>(StringComparer.Ordinal);

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

            writer.Flush();
            return stream.ToArray();
        }

        private void WriteConstructionModuleStates(
            BinaryWriter writer)
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

                byte[] payload;
                try
                {
                    payload =
                        provider.CaptureState()
                        ?? Array.Empty<byte>();
                }
                catch (Exception)
                {
                    payload = Array.Empty<byte>();
                }

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
            if (_sessionManager.IsLocalPlayerHost)
            {
                // Host should never apply snapshots from itself or peers; ignore.
                return;
            }
            if (payload == null || payload.Length < 1)
            {
                return;
            }

            try
            {
                ApplySnapshotPayload(payload);
            }
            catch (Exception)
            {
            }
        }

        private void ApplySnapshotPayload(byte[] payload)
        {
            using var stream = new MemoryStream(payload);
            using var reader = new BinaryReader(stream, Encoding.UTF8);

            byte version = reader.ReadByte();
            if (version != 1
                && version != SchemaVersion)
            {
                return;
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

                if (string.IsNullOrWhiteSpace(id))
                    continue;

                var position = new Vector2Int(x, y);
                _placementRestorer.RestoreFromSave(
                    position,
                    id,
                    ownerId);
            }

            if (version >= 2)
                ReadConstructionModuleStates(reader);

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
        }
    }
}
