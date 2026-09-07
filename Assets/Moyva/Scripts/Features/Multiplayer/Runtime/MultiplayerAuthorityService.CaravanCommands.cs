using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    internal sealed partial class MultiplayerAuthorityService
    {
        private readonly Dictionary<string, CaravanCommandPayload> _confirmedCaravanRequests = new(StringComparer.Ordinal);
        private readonly HashSet<string> _appliedCaravanConfirmations = new(StringComparer.Ordinal);
        private int _routeConfirmationSequence;

        public event Action<CaravanRemoteCommandResult> CommandRejected;

        public bool TryRequestExecute(CaravanCargoRequest request, out string reason)
            => SendCaravanRequest(ToPayload(CaravanCommandAction.Transfer, request), out reason);

        public bool TryRequestSetRoute(CaravanRouteRequest request, out string reason)
            => SendCaravanRequest(new CaravanCommandPayload(
                GameActionMessageKind.Request,
                CaravanCommandAction.StartRoute,
                request.OwnerId,
                request.UnitId,
                0,
                request.SourceSettlementId,
                request.SourceWarehouseKey,
                request.TargetSettlementId,
                request.TargetWarehouseKey,
                request.Repeat,
                string.Empty,
                request.Resources), out reason);

        public bool TryRequestStopRoute(string ownerId, string unitId, out string reason)
            => SendCaravanRequest(new CaravanCommandPayload(
                GameActionMessageKind.Request,
                CaravanCommandAction.StopRoute,
                ownerId,
                unitId,
                0,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                false,
                string.Empty,
                null), out reason);

        public bool TryRequestFoundSettlement(string ownerId, string unitId, string buildingId, out string reason)
            => SendCaravanRequest(new CaravanCommandPayload(
                GameActionMessageKind.Request,
                CaravanCommandAction.FoundSettlement,
                ownerId,
                unitId,
                0,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                false,
                buildingId,
                null), out reason);

        private void OnRouteTransferCommittedLocally(CaravanRouteTransferCommitted committed)
        {
            if (_applyingNetworkEvent || !IsOfflineOrHost())
                return;

            var confirmed = WithKind(
                WithRoutePhase(
                    WithRequestId(
                        ToPayload(CaravanCommandAction.Transfer, committed.Request),
                        $"route:{++_routeConfirmationSequence}"),
                    committed.NextPhase),
                GameActionMessageKind.Confirmed,
                committed.Request.OwnerId);
            SendConfirmedCommandToOwnerPeers(
                GameCommandType.CaravanCommand,
                confirmed.ToBytes(),
                confirmed.OwnerId);
        }

        private bool SendCaravanRequest(CaravanCommandPayload payload, out string reason)
        {
            reason = null;
            if (IsOfflineOrHost())
            {
                reason = "Local host executes logistics directly.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(payload.OwnerId) || string.IsNullOrWhiteSpace(payload.UnitId))
            {
                reason = "Logistics request is missing owner or wagon.";
                return false;
            }

            SendRequestToHost(
                GameCommandType.CaravanCommand,
                WithRequestId(payload, Guid.NewGuid().ToString("N")).ToBytes());
            return true;
        }

        private void OnNetworkCaravanCommand(string senderId, byte[] body)
        {
            CaravanCommandPayload data;
            try { data = CaravanCommandPayload.FromBytes(body); }
            catch (Exception exception)
            {
                LogCaravanAuthorityWarning($"Invalid caravan payload from '{senderId}': {exception.Message}");
                return;
            }

            if (data.Kind == GameActionMessageKind.Rejected)
            {
                if (IsOfflineOrHost() || !IsAuthorizedHostSender(senderId))
                    return;

                string localOwnerId = _roleResolver?.Resolve().PlayerId;
                if (!string.IsNullOrWhiteSpace(localOwnerId)
                    && !string.Equals(localOwnerId, data.OwnerId, StringComparison.Ordinal))
                {
                    return;
                }

                CommandRejected?.Invoke(new CaravanRemoteCommandResult(
                    ToPublicCaravanCommandKind(data.Action),
                    data.OwnerId,
                    data.UnitId,
                    data.RequestId,
                    string.IsNullOrWhiteSpace(data.RejectionReason)
                        ? "Logistics command was rejected by host."
                        : data.RejectionReason));
                return;
            }

            if (_caravanService == null)
            {
                if (data.Kind == GameActionMessageKind.Request && IsOfflineOrHost())
                    RejectCaravan(senderId, data, "Logistics service is not available.");
                else
                    LogCaravanAuthorityWarning("Caravan command ignored because caravan service is not bound.");
                return;
            }

            if (data.Kind == GameActionMessageKind.Confirmed)
            {
                if (IsOfflineOrHost())
                    return;
                if (!IsAuthorizedHostSender(senderId))
                {
                    LogCaravanAuthorityWarning($"Ignored confirmed caravan command from unauthorized sender '{senderId}'.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(data.RequestId))
                    return;
                string confirmationKey = ConfirmationKey(data);
                if (_appliedCaravanConfirmations.Contains(confirmationKey))
                {
                    return;
                }

                CaravanTransferResult applied;
                _applyingNetworkEvent = true;
                try
                {
                    applied = ApplyConfirmedCaravanCommand(data);
                }
                finally
                {
                    _applyingNetworkEvent = false;
                }
                if (!applied.Succeeded)
                {
                    LogCaravanAuthorityWarning($"Failed to apply confirmed caravan {data.Action}: {applied.Reason}");
                    return;
                }
                _appliedCaravanConfirmations.Add(confirmationKey);
                return;
            }

            if (data.Kind != GameActionMessageKind.Request || !IsOfflineOrHost())
                return;
            if (string.IsNullOrWhiteSpace(data.RequestId))
            {
                RejectCaravan(senderId, data, "Request id is missing.");
                return;
            }
            string cacheKey = RequestCacheKey(senderId, data.RequestId);
            if (_confirmedCaravanRequests.TryGetValue(cacheKey, out var cached))
            {
                _syncService.SendCommandToPeer(
                    senderId,
                    GameCommandType.CaravanCommand,
                    cached.ToBytes());
                return;
            }
            if (!TryResolveAuthorizedRequestOwner(
                    senderId,
                    data.OwnerId,
                    data.OwnerId,
                    out string authorizedOwnerId,
                    out string authorizationReason))
            {
                RejectCaravan(senderId, data, authorizationReason);
                return;
            }

            var confirmed = WithKind(data, GameActionMessageKind.Confirmed, authorizedOwnerId);
            if (data.Action == CaravanCommandAction.FoundSettlement)
            {
                var preflight = _caravanService.CanFoundSettlement(
                    authorizedOwnerId,
                    data.UnitId,
                    data.BuildingId,
                    out var foundingPosition);
                if (!preflight.Succeeded)
                {
                    RejectCaravan(senderId, data, preflight.Reason);
                    return;
                }
                confirmed = WithFoundingPosition(confirmed, foundingPosition);
            }

            var result = ExecuteAuthorizedCaravanCommand(authorizedOwnerId, data);
            if (!result.Succeeded)
            {
                RejectCaravan(senderId, data, result.Reason);
                return;
            }

            _confirmedCaravanRequests[cacheKey] = confirmed;
            TrimConfirmedCaravanCache();
            SendConfirmedCommandToOwnerPeers(
                GameCommandType.CaravanCommand,
                confirmed.ToBytes(),
                confirmed.OwnerId);
        }

        private void RejectCaravan(
            string senderId,
            CaravanCommandPayload request,
            string reason)
        {
            string normalizedReason = string.IsNullOrWhiteSpace(reason)
                ? "Logistics command was rejected."
                : reason.Trim();
            LogCaravanAuthorityWarning(
                $"Rejected caravan {request.Action} from '{senderId}': {normalizedReason}");

            if (string.IsNullOrWhiteSpace(senderId))
                return;

            var rejected = new CaravanCommandPayload(
                GameActionMessageKind.Rejected,
                request.Action,
                request.OwnerId,
                request.UnitId,
                request.CargoOperation,
                request.SettlementId,
                request.WarehouseKey,
                request.TargetSettlementId,
                request.TargetWarehouseKey,
                request.Repeat,
                request.BuildingId,
                request.Resources,
                request.RequestId,
                request.RoutePhase,
                normalizedReason);
            _syncService.SendCommandToPeer(
                senderId,
                GameCommandType.CaravanCommand,
                rejected.ToBytes());
        }

        private CaravanTransferResult ExecuteAuthorizedCaravanCommand(
            string ownerId,
            CaravanCommandPayload data)
        {
            return data.Action switch
            {
                CaravanCommandAction.Transfer => _caravanService.Execute(ToRequest(ownerId, data)),
                CaravanCommandAction.StartRoute => _caravanService.SetRoute(new CaravanRouteRequest(
                    ownerId,
                    data.UnitId,
                    data.SettlementId,
                    data.WarehouseKey,
                    data.TargetSettlementId,
                    data.TargetWarehouseKey,
                    data.Resources,
                    data.Repeat)),
                CaravanCommandAction.StopRoute => _caravanService.StopRoute(ownerId, data.UnitId),
                CaravanCommandAction.FoundSettlement => _caravanService.FoundSettlement(
                    ownerId,
                    data.UnitId,
                    data.BuildingId),
                _ => CaravanTransferResult.Rejected("Unknown caravan command."),
            };
        }

        private CaravanTransferResult ApplyConfirmedCaravanCommand(CaravanCommandPayload data)
        {
            return data.Action switch
            {
                CaravanCommandAction.Transfer => TryGetRoutePhase(data, out var nextPhase)
                    ? _caravanService.ApplyConfirmedRouteTransfer(ToRequest(data.OwnerId, data), nextPhase)
                    : _caravanService.ApplyConfirmedTransfer(ToRequest(data.OwnerId, data)),
                CaravanCommandAction.StartRoute => _caravanService.ApplyConfirmedSetRoute(new CaravanRouteRequest(
                    data.OwnerId,
                    data.UnitId,
                    data.SettlementId,
                    data.WarehouseKey,
                    data.TargetSettlementId,
                    data.TargetWarehouseKey,
                    data.Resources,
                    data.Repeat)),
                CaravanCommandAction.StopRoute => _caravanService.ApplyConfirmedStopRoute(data.OwnerId, data.UnitId),
                CaravanCommandAction.FoundSettlement => TryParsePosition(data.TargetWarehouseKey, out var position)
                    ? _caravanService.ApplyConfirmedFoundSettlement(
                        data.OwnerId,
                        data.UnitId,
                        data.BuildingId,
                        position)
                    : CaravanTransferResult.Rejected("Confirmed founding position is invalid."),
                _ => CaravanTransferResult.Rejected("Unknown confirmed caravan command."),
            };
        }

        private static CaravanCommandPayload ToPayload(
            CaravanCommandAction action,
            CaravanCargoRequest request)
            => new(
                GameActionMessageKind.Request,
                action,
                request.OwnerId,
                request.UnitId,
                (byte)request.Operation,
                request.SettlementId,
                request.WarehouseKey,
                string.Empty,
                string.Empty,
                false,
                string.Empty,
                request.Resources);

        private static CaravanCommandPayload WithRequestId(CaravanCommandPayload payload, string requestId)
            => new(
                payload.Kind,
                payload.Action,
                payload.OwnerId,
                payload.UnitId,
                payload.CargoOperation,
                payload.SettlementId,
                payload.WarehouseKey,
                payload.TargetSettlementId,
                payload.TargetWarehouseKey,
                payload.Repeat,
                payload.BuildingId,
                payload.Resources,
                requestId,
                payload.RoutePhase);

        private static CaravanCommandPayload WithRoutePhase(CaravanCommandPayload payload,
            CaravanRoutePhase phase)
            => new(
                payload.Kind,
                payload.Action,
                payload.OwnerId,
                payload.UnitId,
                payload.CargoOperation,
                payload.SettlementId,
                payload.WarehouseKey,
                payload.TargetSettlementId,
                payload.TargetWarehouseKey,
                payload.Repeat,
                payload.BuildingId,
                payload.Resources,
                payload.RequestId,
                (byte)phase);

        private static CaravanCargoRequest ToRequest(string ownerId, CaravanCommandPayload payload)
            => new(
                ownerId,
                payload.UnitId,
                (CaravanCargoOperation)payload.CargoOperation,
                payload.SettlementId,
                payload.WarehouseKey,
                payload.Resources);

        private static CaravanCommandPayload WithFoundingPosition(
            CaravanCommandPayload payload,
            Vector2Int position)
            => new(
                payload.Kind,
                payload.Action,
                payload.OwnerId,
                payload.UnitId,
                payload.CargoOperation,
                payload.SettlementId,
                payload.WarehouseKey,
                payload.TargetSettlementId,
                $"{position.x}:{position.y}",
                payload.Repeat,
                payload.BuildingId,
                payload.Resources,
                payload.RequestId,
                payload.RoutePhase);

        private static bool TryParsePosition(string key, out Vector2Int position)
        {
            position = default;
            if (string.IsNullOrWhiteSpace(key))
                return false;
            int separator = key.IndexOf(':');
            if (separator <= 0
                || !int.TryParse(key.Substring(0, separator), out int x)
                || !int.TryParse(key.Substring(separator + 1), out int y))
            {
                return false;
            }
            position = new Vector2Int(x, y);
            return true;
        }

        private static CaravanCommandPayload WithKind(
            CaravanCommandPayload payload,
            GameActionMessageKind kind,
            string ownerId)
            => new(
                kind,
                payload.Action,
                ownerId,
                payload.UnitId,
                payload.CargoOperation,
                payload.SettlementId,
                payload.WarehouseKey,
                payload.TargetSettlementId,
                payload.TargetWarehouseKey,
                payload.Repeat,
                payload.BuildingId,
                payload.Resources,
                payload.RequestId,
                payload.RoutePhase);

        private static bool TryGetRoutePhase(CaravanCommandPayload payload,
            out CaravanRoutePhase phase)
        {
            phase = default;
            if (payload.RoutePhase == byte.MaxValue)
                return false;
            phase = (CaravanRoutePhase)payload.RoutePhase;
            return phase >= CaravanRoutePhase.ToSource && phase <= CaravanRoutePhase.Completed;
        }

        private static string RequestCacheKey(string senderId, string requestId)
            => $"{senderId?.Trim()}:{requestId?.Trim()}";

        private static string ConfirmationKey(CaravanCommandPayload payload)
            => $"{payload.OwnerId}:{payload.UnitId}:{payload.RequestId}";

        private static CaravanCommandKind ToPublicCaravanCommandKind(CaravanCommandAction action)
            => action switch
            {
                CaravanCommandAction.StartRoute => CaravanCommandKind.StartRoute,
                CaravanCommandAction.StopRoute => CaravanCommandKind.StopRoute,
                CaravanCommandAction.FoundSettlement => CaravanCommandKind.FoundSettlement,
                _ => CaravanCommandKind.Transfer,
            };

        private void TrimConfirmedCaravanCache()
        {
            const int MaximumEntries = 512;
            if (_confirmedCaravanRequests.Count <= MaximumEntries)
                return;
            foreach (string key in new List<string>(_confirmedCaravanRequests.Keys))
            {
                _confirmedCaravanRequests.Remove(key);
                if (_confirmedCaravanRequests.Count <= MaximumEntries)
                    return;
            }
        }

        private static void LogCaravanAuthorityWarning(string message)
        {
            if (!Application.isEditor && !Debug.isDebugBuild)
                return;
            Debug.LogWarning($"[MultiplayerAuthority] {message}");
        }
    }
}
