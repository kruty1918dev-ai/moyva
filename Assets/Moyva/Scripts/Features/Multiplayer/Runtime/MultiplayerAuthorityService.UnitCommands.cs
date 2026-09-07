using System;
using System.Collections.Generic;
using System.Threading;
using Kruty1918.Moyva.Construction.API;
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
            bool hadPreviousPosition =
                _replicatedUnitPositions.TryGetValue(
                    signal.UnitId,
                    out Vector2Int previousPosition);

            var payload = new UnitMovePayload(
                GameActionMessageKind.Confirmed,
                signal.UnitId,
                signal.NewPosition);

            SendUnitMoveOrRevealToVisiblePeers(
                payload,
                unitOwnerId,
                hadPreviousPosition
                    ? previousPosition
                    : signal.NewPosition,
                signal.NewPosition);

            _replicatedUnitPositions[signal.UnitId] =
                signal.NewPosition;
        }

        private void OnUnitCreatedLocally(UnitCreatedSignal signal)
        {
            if (_applyingNetworkEvent || !IsOfflineOrHost()) return;

            _replicatedUnitPositions[signal.UnitId] =
                signal.Position;

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

            _replicatedUnitPositions.Remove(signal.UnitId);
            foreach (var pair in _knownUnitsByPeer)
                pair.Value?.Remove(signal.UnitId);
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
                },
                payload.ToBytes(),
                GameCommandType.UnitSpawn);
        }

        private void SendUnitMoveOrRevealToVisiblePeers(
            UnitMovePayload movePayload,
            string unitOwnerId,
            Vector2Int previousPosition,
            Vector2Int newPosition)
        {
            byte[] moveBytes =
                movePayload.ToBytes();
            SendUnitCommandToPeers(
                peerId =>
                {
                    bool known =
                        IsUnitKnownByPeer(
                            peerId,
                            movePayload.UnitId);
                    bool canSeeNew =
                        CanPeerObserveWorldEvent(
                            peerId,
                            unitOwnerId,
                            newPosition);
                    bool canSeePrevious =
                        CanPeerObserveWorldEvent(
                            peerId,
                            unitOwnerId,
                            previousPosition);

                    if (!known && canSeeNew)
                    {
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
                        }

                        return;
                    }

                    if (!known
                        || (!canSeePrevious && !canSeeNew))
                    {
                        return;
                    }

                    _syncService.SendCommandToPeer(
                        peerId,
                        GameCommandType.UnitMove,
                        moveBytes);
                },
                moveBytes,
                GameCommandType.UnitMove);
        }

        private void SendUnitCommandToPeers(
            Action<string> sendToPeer,
            byte[] fallbackPayload,
            GameCommandType fallbackType)
        {
            var participants =
                _sessionManager?.Participants;
            if (participants == null
                || participants.Count == 0)
            {
                _syncService.SendCommand(
                    fallbackType,
                    fallbackPayload);
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
