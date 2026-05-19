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
    /// <see cref="IConstructionService.RestoreFromSave"/> and resources via
    /// <see cref="EconomyManager.RestoreOwnerResourcePools"/>.
    /// </summary>
    internal sealed class WorldStateReplicationService : IInitializable, IDisposable
    {
        private const byte SchemaVersion = 1;

        private readonly IGameCommandSyncService _commandSync;
        private readonly INetworkProvider _network;
        private readonly ISessionManager _sessionManager;
        private readonly IConstructionService _constructionService;
        private readonly IMultiplayerLogger _logger;
        private readonly EconomyManager _economyManager;

        public WorldStateReplicationService(
            IGameCommandSyncService commandSync,
            INetworkProvider network,
            ISessionManager sessionManager,
            IConstructionService constructionService,
            IMultiplayerLogger logger,
            [InjectOptional] EconomyManager economyManager = null)
        {
            _commandSync = commandSync;
            _network = network;
            _sessionManager = sessionManager;
            _constructionService = constructionService;
            _logger = logger;
            _economyManager = economyManager;
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
                _logger.Info($"[WorldStateReplication] Snapshot sent to peer '{peerId}' ({payload.Length} bytes).");
            }
            catch (Exception e)
            {
                _logger.Warn($"[WorldStateReplication] Failed to build/send snapshot for '{peerId}': {e.Message}");
            }
        }

        private byte[] BuildSnapshotPayload()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8);

            writer.Write(SchemaVersion);

            // Buildings
            var buildings = _constructionService.GetPlayerPlacedBuildings();
            writer.Write(buildings.Count);
            foreach (var pair in buildings)
            {
                writer.Write(pair.Key.x);
                writer.Write(pair.Key.y);
                writer.Write(pair.Value ?? string.Empty);
            }

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
                _logger.Warn("[WorldStateReplication] Received empty snapshot payload.");
                return;
            }

            try
            {
                ApplySnapshotPayload(payload);
                _logger.Info($"[WorldStateReplication] Snapshot applied from host '{senderId}' ({payload.Length} bytes).");
            }
            catch (Exception e)
            {
                _logger.Warn($"[WorldStateReplication] Failed to apply snapshot: {e.Message}");
            }
        }

        private void ApplySnapshotPayload(byte[] payload)
        {
            using var stream = new MemoryStream(payload);
            using var reader = new BinaryReader(stream, Encoding.UTF8);

            byte version = reader.ReadByte();
            if (version != SchemaVersion)
            {
                _logger.Warn($"[WorldStateReplication] Unsupported snapshot version {version}; ignored.");
                return;
            }

            // Buildings
            int buildingCount = reader.ReadInt32();
            for (int i = 0; i < buildingCount; i++)
            {
                int x = reader.ReadInt32();
                int y = reader.ReadInt32();
                string id = reader.ReadString();
                if (string.IsNullOrWhiteSpace(id))
                    continue;
                _constructionService.RestoreFromSave(new Vector2Int(x, y), id);
            }

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
