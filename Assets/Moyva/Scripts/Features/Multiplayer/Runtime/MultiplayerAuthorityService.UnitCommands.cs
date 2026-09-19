using System;
using System.Collections.Generic;
using System.Threading;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    internal sealed partial class MultiplayerAuthorityService
    {
private void OnLocalMoveUnitRequest(MoveUnitRequestSignal signal)
{
    if (_unitMovementService == null || _unitOwnershipQuery == null)
    {
        Debug.LogError("[MultiplayerAuthority] Unit movement or ownership service is not bound.");
        return;
    }

    string requesterOwnerId =
        string.IsNullOrWhiteSpace(signal.RequesterOwnerId)
            ? _sessionManager?.LocalPlayerId
            : signal.RequesterOwnerId;

    string unitOwnerId =
        _unitOwnershipQuery.GetUnitOwnerId(signal.UnitId);

    bool authorized =
        IsUnitCommandAuthorized(
            unitOwnerId,
            requesterOwnerId);

    if (!authorized)
    {
        return;
    }

    bool offlineOrHost = IsOfflineOrHost();

    if (offlineOrHost)
    {
        _ = _unitMovementService.MoveUnitAsync(
            signal.UnitId,
            signal.TargetPosition,
            _lifetime.Token);
        return;
    }

    var payload = new UnitMovePayload(
        GameActionMessageKind.Request,
        signal.UnitId,
        signal.TargetPosition);

    SendRequestToHost(
        GameCommandType.UnitMove,
        payload.ToBytes());
}

        private void OnUnitMovedLocally(UnitMovedSignal signal)
        {
            if (_applyingNetworkEvent || !IsOfflineOrHost()) return;

            string unitOwnerId =
                NormalizeOwnerId(
                    string.IsNullOrWhiteSpace(signal.SourceFactionId)
                        ? _unitOwnershipQuery?.GetUnitOwnerId(
                            signal.UnitId)
                        : signal.SourceFactionId);

            var payload = new UnitMovePayload(
                GameActionMessageKind.Confirmed,
                signal.UnitId,
                signal.NewPosition);

            SendUnitMoveOrRevealToVisiblePeers(
                payload,
                unitOwnerId,
                signal.NewPosition);
        }

        private void OnUnitCreatedLocally(UnitCreatedSignal signal)
        {
            if (_applyingNetworkEvent || !IsOfflineOrHost()) return;

            var payload = new UnitSpawnPayload(
                GameActionMessageKind.Confirmed,
                signal.UnitId,
                signal.UnitTypeId,
                signal.Position,
                signal.OwnerId);
            SendUnitSpawnToVisiblePeers(payload);
        }

        private void OnUnitDestroyedLocally(UnitDestroyedSignal signal)
        {
            if (string.IsNullOrWhiteSpace(signal.UnitId))
                return;

            // The unit stays marked as peer-known: peers that did not observe
            // the death keep last-known intel, and the visibility-feed
            // reconcile sends UnitVanish once they re-scout the cell.
        }

        private void OnNetworkUnitMove(string senderId, byte[] body)
        {
            var data = UnitMovePayload.FromBytes(body);

            if (_unitMovementService == null)
            {
                return;
            }

            if (data.Kind == GameActionMessageKind.Request)
            {
                // Лише хост обробляє запити на рух.
                if (!IsOfflineOrHost()) return;
                if (_unitOwnershipQuery == null)
                    return;

                string unitOwnerId = _unitOwnershipQuery.GetUnitOwnerId(data.UnitId);
                if (!TryResolveAuthorizedRequestOwner(
                        senderId,
                        unitOwnerId,
                        unitOwnerId,
                        out _,
                        out string authorizationReason))
                {
                    return;
                }
                // Хост виконує рух; UnitMovedSignal транслює кожен крок через OnUnitMovedLocally.
                _ = _unitMovementService.MoveUnitAsync(data.UnitId, data.TargetPosition, _lifetime.Token);
            }
            else // Confirmed
            {
                // Клієнт запускає власний рух до тієї ж позиції (детерміноване pathfinding).
                if (IsOfflineOrHost()) return;
                if (!IsAuthorizedHostSender(senderId))
                {
                    return;
                }

                _ = _unitMovementService.MoveUnitAsync(data.UnitId, data.TargetPosition, _lifetime.Token);
            }
        }

        private void OnNetworkUnitSpawn(string senderId, byte[] body)
        {
            var data = UnitSpawnPayload.FromBytes(body);

            if (_unitFactory == null)
            {
                return;
            }

            if (data.Kind == GameActionMessageKind.Request)
            {
                LogUnitAuthorityWarning(
                    $"Rejected UnitSpawn request from '{senderId}'. Units must be created by canonical gameplay services.");
                return;
            }
            else // Confirmed
            {
                // Клієнт створює юніта з тим самим ID, що і на хості.
                if (IsOfflineOrHost()) return;
                if (!IsAuthorizedHostSender(senderId))
                {
                    LogUnitAuthorityWarning(
                        $"Ignored confirmed unit spawn from unauthorized sender '{senderId}' for '{data.AssignedUnitId}'.");
                    return;
                }

                _applyingNetworkEvent = true;
                try
                {
                    if (_unitService != null
                        && _unitService.TryGetUnitPosition(
                            data.AssignedUnitId,
                            out Vector2Int currentPosition))
                    {
                        if (currentPosition != data.Position
                            && _unitMovementService != null)
                        {
                            _ = _unitMovementService.MoveUnitAsync(
                                data.AssignedUnitId,
                                data.Position,
                                _lifetime.Token);
                        }

                        return;
                    }

                    _unitFactory.CreateUnitWithId(
                        data.AssignedUnitId, data.UnitTypeId, data.Position, data.OwnerId);
                }
                finally
                {
                    _applyingNetworkEvent = false;
                }
            }
        }

        private void SendUnitSpawnToVisiblePeers(
            UnitSpawnPayload payload)
        {
            SendUnitCommandToPeers(
                peerId =>
                {
                    if (!CanPeerObserveWorldEvent(
                            peerId,
                            payload.OwnerId,
                            payload.Position))
                    {
                        return;
                    }

                    MarkUnitKnownByPeer(
                        peerId,
                        payload.AssignedUnitId);
                    _syncService.SendCommandToPeer(
                        peerId,
                        GameCommandType.UnitSpawn,
                        payload.ToBytes());
                });
        }

        private void SendUnitMoveOrRevealToVisiblePeers(
            UnitMovePayload movePayload,
            string unitOwnerId,
            Vector2Int newPosition)
        {
            byte[] moveBytes =
                movePayload.ToBytes();
            SendUnitCommandToPeers(
                peerId =>
                {
                    // Fog-of-war rule: a hidden destination never leaves the
                    // host. Peers that can see the new cell get a spawn payload
                    // (discovers unknown units and re-syncs remembered ones);
                    // everyone else keeps their last-known memory until they
                    // legitimately re-observe the unit.
                    if (!CanPeerObserveWorldEvent(
                            peerId,
                            unitOwnerId,
                            newPosition))
                    {
                        return;
                    }

                    bool known =
                        IsUnitKnownByPeer(
                            peerId,
                            movePayload.UnitId);
                    if (TryCreateUnitSpawnPayload(
                            movePayload.UnitId,
                            unitOwnerId,
                            newPosition,
                            out UnitSpawnPayload spawnPayload))
                    {
                        MarkUnitKnownByPeer(
                            peerId,
                            movePayload.UnitId);
                        _syncService.SendCommandToPeer(
                            peerId,
                            GameCommandType.UnitSpawn,
                            spawnPayload.ToBytes());
                        return;
                    }

                    if (known)
                    {
                        _syncService.SendCommandToPeer(
                            peerId,
                            GameCommandType.UnitMove,
                            moveBytes);
                    }
                });
        }

        private void SendUnitCommandToPeers(
            Action<string> sendToPeer)
        {
            var participants =
                _sessionManager?.Participants;
            if (participants == null
                || participants.Count == 0)
            {
                return;
            }

            string localPlayerId =
                NormalizeOwnerId(
                    _sessionManager.LocalPlayerId);
            for (int index = 0;
                 index < participants.Count;
                 index++)
            {
                string peerId =
                    NormalizeOwnerId(
                        participants[index]?.Identity?.PlayerId);
                if (string.IsNullOrWhiteSpace(peerId)
                    || string.Equals(
                        peerId,
                        localPlayerId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                sendToPeer(peerId);
            }
        }

        private void OnNetworkUnitVanish(string senderId, byte[] body)
        {
            var data = UnitMovePayload.FromBytes(body);
            if (data.Kind != GameActionMessageKind.Confirmed
                || IsOfflineOrHost()
                || !IsAuthorizedHostSender(senderId)
                || string.IsNullOrWhiteSpace(data.UnitId))
            {
                return;
            }

            _applyingNetworkEvent = true;
            try
            {
                string localOwnerId =
                    NormalizeOwnerId(
                        _roleResolver?.Resolve().PlayerId
                        ?? _sessionManager?.LocalPlayerId);
                _intelSink?.RemoveReplicatedUnit(localOwnerId, data.UnitId.Trim());

                // The remembered entity is gone: tear down any stale local
                // replica so it stops occupying the cell.
                if (_unitService != null
                    && _unitService.TryGetUnitPosition(data.UnitId, out _))
                {
                    _signalBus.Fire(new UnitDestroyedSignal
                    {
                        UnitId = data.UnitId,
                    });
                }
            }
            finally
            {
                _applyingNetworkEvent = false;
            }
        }

        private void OnPeerCellsBecameVisible(
            string ownerId,
            IReadOnlyCollection<Vector2Int> cells)
        {
            if (!IsOfflineOrHost()
                || _applyingNetworkEvent
                || cells == null
                || cells.Count == 0)
            {
                return;
            }

            string peerId = NormalizeOwnerId(ownerId);
            if (string.IsNullOrWhiteSpace(peerId)
                || string.Equals(
                    peerId,
                    NormalizeOwnerId(_sessionManager?.LocalPlayerId),
                    StringComparison.Ordinal)
                || !IsRemoteParticipant(peerId))
            {
                return;
            }

            var cellSet =
                cells as HashSet<Vector2Int>
                ?? new HashSet<Vector2Int>(cells);

            ReconcilePeerUnits(peerId, cellSet);
            ReconcilePeerBuildings(peerId, cellSet);
        }

        private bool IsRemoteParticipant(string peerId)
        {
            var participants = _sessionManager?.Participants;
            if (participants == null)
                return false;

            for (int index = 0; index < participants.Count; index++)
            {
                if (string.Equals(
                        NormalizeOwnerId(participants[index]?.Identity?.PlayerId),
                        peerId,
                        StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private void ReconcilePeerUnits(
            string peerId,
            HashSet<Vector2Int> revealedCells)
        {
            // Units standing on freshly revealed cells: spawn (or re-sync) any
            // unit whose last-known state no longer matches reality.
            IReadOnlyCollection<string> unitIds =
                _unitService?.GetAllUnitIds();
            if (unitIds != null)
            {
                foreach (string unitId in unitIds)
                {
                    if (string.IsNullOrWhiteSpace(unitId)
                        || !_unitService.TryGetUnitPosition(
                            unitId,
                            out Vector2Int position)
                        || !revealedCells.Contains(position))
                    {
                        continue;
                    }

                    // Garrisoned units have no world entity at the cell — the
                    // intel store already handled observers of the garrison.
                    if (_unitService is IConstructionUnitGarrisonRuntime garrison
                        && garrison.IsGarrisoned(unitId))
                    {
                        continue;
                    }

                    string unitOwnerId =
                        NormalizeOwnerId(
                            _unitOwnershipQuery?.GetUnitOwnerId(unitId));
                    if (string.Equals(
                            unitOwnerId,
                            peerId,
                            StringComparison.Ordinal))
                    {
                        continue;
                    }

                    bool known =
                        IsUnitKnownByPeer(peerId, unitId);
                    bool memoryIsStale =
                        _intelReader != null
                        && _intelReader.TryGetRememberedUnit(
                            peerId,
                            unitId,
                            out FogIntelUnitRecord memory)
                        && memory.LastKnownPosition != position;
                    if (known && !memoryIsStale)
                        continue;

                    if (TryCreateUnitSpawnPayload(
                            unitId,
                            unitOwnerId,
                            position,
                            out UnitSpawnPayload spawnPayload))
                    {
                        MarkUnitKnownByPeer(peerId, unitId);
                        _syncService.SendCommandToPeer(
                            peerId,
                            GameCommandType.UnitSpawn,
                            spawnPayload.ToBytes());
                    }
                }
            }

            // Known units that no longer exist: forget them only once the peer
            // re-observes the cell where it last saw them.
            if (!_knownUnitsByPeer.TryGetValue(
                    peerId,
                    out HashSet<string> knownUnits)
                || knownUnits.Count == 0)
            {
                return;
            }

            foreach (string unitId in new List<string>(knownUnits))
            {
                if (_unitService == null
                    || _unitService.TryGetUnitPosition(unitId, out _))
                {
                    continue;
                }

                // No intel record → the peer either watched the death (its
                // replica already died) or never built a memory — either way a
                // vanish is safe cleanup. With a record, vanish only once the
                // peer re-observes the last-known cell.
                bool rememberedCellRevealed =
                    _intelReader == null
                    || !_intelReader.TryGetRememberedUnit(
                            peerId,
                            unitId,
                            out FogIntelUnitRecord memory)
                    || revealedCells.Contains(memory.LastKnownPosition);
                if (!rememberedCellRevealed)
                    continue;

                var vanish = new UnitMovePayload(
                    GameActionMessageKind.Confirmed,
                    unitId,
                    default);
                _syncService.SendCommandToPeer(
                    peerId,
                    GameCommandType.UnitVanish,
                    vanish.ToBytes());
                knownUnits.Remove(unitId);
            }
        }

        private void ReconcilePeerBuildings(
            string peerId,
            HashSet<Vector2Int> revealedCells)
        {
            IReadOnlyList<ConstructionSavedPlacement> placements =
                _placementSnapshots?.GetSavedPlacements();
            var authoritative = new Dictionary<Vector2Int, ConstructionSavedPlacement>();
            if (placements != null)
            {
                for (int index = 0; index < placements.Count; index++)
                {
                    ConstructionSavedPlacement placement = placements[index];
                    if (revealedCells.Contains(placement.Position))
                        authoritative[placement.Position] = placement;
                }
            }

            _knownBuildingsByPeer.TryGetValue(
                peerId,
                out HashSet<Vector2Int> knownCells);

            foreach (Vector2Int cell in revealedCells)
            {
                bool hasAuthoritative = authoritative.TryGetValue(
                    cell,
                    out ConstructionSavedPlacement placement);
                bool known = knownCells != null && knownCells.Contains(cell);

                if (hasAuthoritative && !known)
                {
                    var payload = new BuildingPlacePayload(
                        GameActionMessageKind.Confirmed,
                        placement.BuildingId,
                        placement.Position,
                        placement.OwnerId,
                        placement.OwnerId,
                        rotation: placement.Rotation);
                    _syncService.SendCommandToPeer(
                        peerId,
                        GameCommandType.BuildingPlace,
                        payload.ToBytes());
                    if (knownCells == null)
                    {
                        knownCells = new HashSet<Vector2Int>();
                        _knownBuildingsByPeer[peerId] = knownCells;
                    }
                    knownCells.Add(cell);
                }
                else if (!hasAuthoritative && known)
                {
                    var payload = new BuildingDemolishPayload(
                        GameActionMessageKind.Confirmed,
                        cell,
                        string.Empty);
                    _syncService.SendCommandToPeer(
                        peerId,
                        GameCommandType.BuildingDemolish,
                        payload.ToBytes());
                    knownCells.Remove(cell);
                }
            }
        }

        private bool TryCreateUnitSpawnPayload(
            string unitId,
            string ownerId,
            Vector2Int position,
            out UnitSpawnPayload payload)
        {
            payload = default;
            if (_unitService == null
                || string.IsNullOrWhiteSpace(unitId))
            {
                return false;
            }

            string typeId =
                _unitService.GetUnitTypeId(unitId);
            if (string.IsNullOrWhiteSpace(typeId))
                return false;

            payload = new UnitSpawnPayload(
                GameActionMessageKind.Confirmed,
                unitId,
                typeId,
                position,
                ownerId);
            return true;
        }

        private bool IsUnitKnownByPeer(
            string peerId,
            string unitId)
        {
            return _knownUnitsByPeer.TryGetValue(
                       NormalizeOwnerId(peerId),
                       out HashSet<string> units)
                   && units.Contains(unitId);
        }

        private void MarkUnitKnownByPeer(
            string peerId,
            string unitId)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                return;

            string normalizedPeerId =
                NormalizeOwnerId(peerId);
            if (string.IsNullOrWhiteSpace(normalizedPeerId))
                return;

            if (!_knownUnitsByPeer.TryGetValue(
                    normalizedPeerId,
                    out HashSet<string> units))
            {
                units = new HashSet<string>(
                    StringComparer.Ordinal);
                _knownUnitsByPeer[normalizedPeerId] = units;
            }

            units.Add(unitId);
        }

        private void LogUnitAuthorityWarning(
            string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                Debug.LogWarning($"[MultiplayerAuthority][Units] {message}");
        }

    }
}
