using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Economy.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Signals;
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

        public bool TryRequestSupplyDispatch(ConstructionSupplyDispatchRequest request,
            IReadOnlyDictionary<string, float> requiredCosts, out string reason)
            => SendCaravanRequest(new CaravanCommandPayload(
                GameActionMessageKind.Request,
                CaravanCommandAction.SupplyDispatch,
                request.OwnerId,
                request.UnitId,
                0,
                request.SourceSettlementId,
                request.SourceWarehouseKey,
                string.Empty,
                FormatPosition(request.Position),
                false,
                request.BuildingId,
                requiredCosts), out reason);

        public bool TryRequestCancelSupply(string ownerId, Vector2Int position, out string reason)
            => SendCaravanRequest(new CaravanCommandPayload(
                GameActionMessageKind.Request,
                CaravanCommandAction.CancelSupply,
                ownerId,
                string.Empty,
                0,
                string.Empty,
                string.Empty,
                string.Empty,
                FormatPosition(position),
                false,
                string.Empty,
                null), out reason);

        // Host → owner peers: an order closed (cancel/confirm); stop route mirrors.
        private void OnConstructionSupplyOrderClosed(ConstructionSupplyOrderClosedSignal signal)
        {
            if (_applyingNetworkEvent || !IsOfflineOrHost()) return;
            var wagons = signal.WagonIds;
            if (wagons == null || wagons.Count == 0) return;
            foreach (var wagonId in wagons)
            {
                if (string.IsNullOrWhiteSpace(wagonId)) continue;
                var payload = WithRequestId(new CaravanCommandPayload(
                    GameActionMessageKind.Confirmed,
                    CaravanCommandAction.CancelSupply,
                    signal.OwnerId,
                    wagonId,
                    0,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    FormatPosition(signal.Position),
                    false,
                    string.Empty,
                    null), $"supply-close:{signal.Position.x}:{signal.Position.y}:{wagonId}:{++_routeConfirmationSequence}");
                SendConfirmedCommandToOwnerPeers(
                    GameCommandType.CaravanCommand,
                    payload.ToBytes(),
                    signal.OwnerId);
            }
        }

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
            if (string.IsNullOrWhiteSpace(payload.OwnerId)
                || (string.IsNullOrWhiteSpace(payload.UnitId)
                    && payload.Action != CaravanCommandAction.CancelSupply))
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

            if (data.Action == CaravanCommandAction.SupplyDispatch)
                BroadcastConfirmedSupplyRoute(authorizedOwnerId, data.UnitId);

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

        // The supply dispatch created the route host-side; peers mirror it via a
        // regular confirmed StartRoute carrying the actual shipment.
        private void BroadcastConfirmedSupplyRoute(string ownerId, string unitId)
        {
            if (_caravanService == null
                || !_caravanService.TryGetRoute(ownerId, unitId, out var route))
                return;
            var routeRequest = route.Request;
            var payload = WithRequestId(new CaravanCommandPayload(
                GameActionMessageKind.Confirmed,
                CaravanCommandAction.StartRoute,
                routeRequest.OwnerId,
                routeRequest.UnitId,
                0,
                routeRequest.SourceSettlementId,
                routeRequest.SourceWarehouseKey,
                routeRequest.TargetSettlementId,
                routeRequest.TargetWarehouseKey,
                routeRequest.Repeat,
                string.Empty,
                routeRequest.Resources), $"supply-route:{++_routeConfirmationSequence}");
            SendConfirmedCommandToOwnerPeers(
                GameCommandType.CaravanCommand,
                payload.ToBytes(),
                routeRequest.OwnerId);
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
                CaravanCommandAction.SupplyDispatch => _constructionSupply == null
                    ? CaravanTransferResult.Rejected("Construction supply is unavailable.")
                    : TryParsePosition(data.TargetWarehouseKey, out var supplyPosition)
                        ? _constructionSupply.DispatchSupply(new ConstructionSupplyDispatchRequest(
                            ownerId,
                            data.BuildingId,
                            supplyPosition,
                            data.SettlementId,
                            data.WarehouseKey,
                            data.UnitId), data.Resources)
                        : CaravanTransferResult.Rejected("Supply dispatch position is invalid."),
                CaravanCommandAction.CancelSupply => _constructionSupply == null
                    ? CaravanTransferResult.Rejected("Construction supply is unavailable.")
                    : TryParsePosition(data.TargetWarehouseKey, out var cancelPosition)
                        ? CancelSupplyAt(ownerId, cancelPosition)
                        : CaravanTransferResult.Rejected("Supply cancel position is invalid."),
                _ => CaravanTransferResult.Rejected("Unknown caravan command."),
            };
        }

        private CaravanTransferResult CancelSupplyAt(string ownerId, Vector2Int position)
        {
            if (_constructionSupply.TryGetOrderAt(position, out var order)
                && !string.Equals(order.OwnerId, ownerId, StringComparison.Ordinal))
                return CaravanTransferResult.Rejected("The supply order belongs to another kingdom.");
            _constructionSupply.CancelOrderAt(position);
            return CaravanTransferResult.Success();
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
                CaravanCommandAction.SupplyDispatch => _constructionSupply == null
                    ? CaravanTransferResult.Rejected("Construction supply is unavailable.")
                    : TryParsePosition(data.TargetWarehouseKey, out var confirmedSupplyPosition)
                        ? _constructionSupply.ApplyConfirmedDispatch(new ConstructionSupplyDispatchRequest(
                            data.OwnerId,
                            data.BuildingId,
                            confirmedSupplyPosition,
                            data.SettlementId,
                            data.WarehouseKey,
                            data.UnitId), data.Resources)
                        : CaravanTransferResult.Rejected("Confirmed supply position is invalid."),
                CaravanCommandAction.CancelSupply => _constructionSupply == null
                    ? CaravanTransferResult.Rejected("Construction supply is unavailable.")
                    : TryParsePosition(data.TargetWarehouseKey, out var confirmedCancelPosition)
                        ? ApplyConfirmedCancelSupply(data.OwnerId, data.UnitId, confirmedCancelPosition)
                        : CaravanTransferResult.Rejected("Confirmed supply position is invalid."),
                _ => CaravanTransferResult.Rejected("Unknown confirmed caravan command."),
            };
        }

        private CaravanTransferResult ApplyConfirmedCancelSupply(string ownerId, string unitId,
            Vector2Int position)
        {
            _constructionSupply.ApplyConfirmedCancelOrder(ownerId, unitId, position);
            return CaravanTransferResult.Success();
        }

        private static string FormatPosition(Vector2Int position)
            => $"{position.x}:{position.y}";

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
                CaravanCommandAction.SupplyDispatch => CaravanCommandKind.SupplyDispatch,
                CaravanCommandAction.CancelSupply => CaravanCommandKind.CancelSupply,
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
