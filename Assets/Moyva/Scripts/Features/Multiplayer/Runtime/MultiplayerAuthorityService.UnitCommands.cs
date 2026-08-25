using System;
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
    long trace = UnitMovementDiagnostics.TraceForUnit(signal.UnitId);
    double started = UnitMovementDiagnostics.NowMs();

    UnitMovementDiagnostics.Log(
        trace,
        "AUTHORITY_REQUEST_RECEIVED",
        $"unit={UnitMovementDiagnostics.Safe(signal.UnitId)}; " +
        $"target={signal.TargetPosition}; " +
        $"requesterInSignal={UnitMovementDiagnostics.Safe(signal.RequesterOwnerId)}; " +
        $"movementServiceBound={_unitMovementService != null}; " +
        $"ownershipBound={_unitOwnershipQuery != null}");

    if (_unitMovementService == null || _unitOwnershipQuery == null)
    {
        UnitMovementDiagnostics.Error(
            trace,
            "AUTHORITY_REJECT_SERVICES",
            "unit movement or ownership service is not bound");
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
        UnitMovementDiagnostics.Warn(
            trace,
            "AUTHORITY_REJECT_OWNERSHIP",
            $"unit={signal.UnitId}; " +
            $"requester={UnitMovementDiagnostics.Safe(requesterOwnerId)}; " +
            $"unitOwner={UnitMovementDiagnostics.Safe(unitOwnerId)}");
        return;
    }

    bool offlineOrHost = IsOfflineOrHost();

    if (offlineOrHost)
    {
        UnitMovementDiagnostics.Log(
            trace,
            "AUTHORITY_ROUTE_LOCAL",
            $"unit={signal.UnitId}; target={signal.TargetPosition}; " +
            $"requester={UnitMovementDiagnostics.Safe(requesterOwnerId)}; " +
            $"unitOwner={UnitMovementDiagnostics.Safe(unitOwnerId)}; " +
            $"dispatchMs={UnitMovementDiagnostics.Ms(UnitMovementDiagnostics.NowMs() - started)}");

        _ = _unitMovementService.MoveUnitAsync(
            signal.UnitId,
            signal.TargetPosition,
            CancellationToken.None);
        return;
    }

    UnitMovementDiagnostics.Log(
        trace,
        "AUTHORITY_ROUTE_NETWORK",
        $"unit={signal.UnitId}; target={signal.TargetPosition}; " +
        $"requester={UnitMovementDiagnostics.Safe(requesterOwnerId)}; " +
        $"unitOwner={UnitMovementDiagnostics.Safe(unitOwnerId)}");

    var payload = new UnitMovePayload(
        GameActionMessageKind.Request,
        signal.UnitId,
        signal.TargetPosition);

    _syncService.SendCommand(
        GameCommandType.UnitMove,
        payload.ToBytes());

    UnitMovementDiagnostics.Log(
        trace,
        "AUTHORITY_NETWORK_SENT",
        $"unit={signal.UnitId}; target={signal.TargetPosition}; " +
        $"elapsedMs={UnitMovementDiagnostics.Ms(UnitMovementDiagnostics.NowMs() - started)}");
}

        private void OnUnitMovedLocally(UnitMovedSignal signal)
        {
            if (_applyingNetworkEvent || !IsOfflineOrHost()) return;

            // Транслюємо кожен крок руху; клієнти синхронно запускають власний MoveUnitAsync.
            var payload = new UnitMovePayload(
                GameActionMessageKind.Confirmed,
                signal.UnitId,
                signal.NewPosition);
            _syncService.SendCommand(GameCommandType.UnitMove, payload.ToBytes());
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
            _syncService.SendCommand(GameCommandType.UnitSpawn, payload.ToBytes());
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
                _ = _unitMovementService.MoveUnitAsync(data.UnitId, data.TargetPosition, CancellationToken.None);
            }
            else // Confirmed
            {
                // Клієнт запускає власний рух до тієї ж позиції (детерміноване pathfinding).
                if (IsOfflineOrHost()) return;
                if (!IsAuthorizedHostSender(senderId))
                {
                    return;
                }

                _ = _unitMovementService.MoveUnitAsync(data.UnitId, data.TargetPosition, CancellationToken.None);
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
                // Резерв для майбутнього: клієнт запитує спавн юніта.
                if (!IsOfflineOrHost()) return;

                // Хост створює юніта; UnitCreatedSignal надішле Confirmed із призначеним ID.
                _unitFactory.CreateUnit(data.UnitTypeId, data.Position, data.OwnerId);
            }
            else // Confirmed
            {
                // Клієнт створює юніта з тим самим ID, що і на хості.
                if (IsOfflineOrHost()) return;

                _applyingNetworkEvent = true;
                try
                {
                    _unitFactory.CreateUnitWithId(
                        data.AssignedUnitId, data.UnitTypeId, data.Position, data.OwnerId);
                }
                finally
                {
                    _applyingNetworkEvent = false;
                }
            }
        }

    }
}
