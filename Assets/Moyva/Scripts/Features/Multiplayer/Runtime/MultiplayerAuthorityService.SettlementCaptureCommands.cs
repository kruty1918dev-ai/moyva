using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Economy.Runtime;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    internal sealed partial class MultiplayerAuthorityService
    {
        private readonly Dictionary<string, SettlementCaptureCommandPayload> _confirmedSettlementCaptures =
            new(StringComparer.Ordinal);
        private readonly HashSet<string> _appliedSettlementCaptureConfirmations =
            new(StringComparer.Ordinal);

        public event Action<SettlementCaptureRemoteResult> CaptureRejected;

        public bool TryRequestCapture(
            string requesterOwnerId,
            string unitId,
            string targetEntityId,
            Vector2Int targetPosition,
            out string reason)
        {
            reason = null;
            if (IsOfflineOrHost())
            {
                reason = "Local host executes settlement capture directly.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(requesterOwnerId)
                || string.IsNullOrWhiteSpace(unitId)
                || string.IsNullOrWhiteSpace(targetEntityId))
            {
                reason = "Settlement capture request is missing owner, unit, or target.";
                return false;
            }

            var payload = new SettlementCaptureCommandPayload(
                GameActionMessageKind.Request,
                requesterOwnerId,
                unitId,
                targetEntityId,
                targetPosition,
                string.Empty,
                string.Empty,
                Guid.NewGuid().ToString("N"));
            SendRequestToHost(
                GameCommandType.SettlementCaptureCommand,
                payload.ToBytes());
            return true;
        }

        private void OnNetworkSettlementCaptureCommand(string senderId, byte[] body)
        {
            SettlementCaptureCommandPayload data;
            try { data = SettlementCaptureCommandPayload.FromBytes(body); }
            catch (Exception exception)
            {
                LogSettlementCaptureAuthorityWarning($"Invalid capture payload from '{senderId}': {exception.Message}");
                return;
            }

            if (data.Kind == GameActionMessageKind.Confirmed)
            {
                if (IsOfflineOrHost())
                    return;
                if (!IsAuthorizedHostSender(senderId)
                    || string.IsNullOrWhiteSpace(data.RequestId))
                {
                    return;
                }

                string confirmationKey = $"{data.RequesterOwnerId}:{data.TargetPosition.x},{data.TargetPosition.y}:{data.RequestId}";
                if (_appliedSettlementCaptureConfirmations.Contains(confirmationKey))
                    return;

                if (ApplyConfirmedSettlementCapture(data))
                    _appliedSettlementCaptureConfirmations.Add(confirmationKey);
                return;
            }

            if (data.Kind == GameActionMessageKind.Rejected)
            {
                if (IsOfflineOrHost() || !IsAuthorizedHostSender(senderId))
                    return;

                string localOwnerId = _roleResolver?.Resolve().PlayerId;
                if (!string.IsNullOrWhiteSpace(localOwnerId)
                    && !string.Equals(localOwnerId, data.RequesterOwnerId, StringComparison.Ordinal))
                {
                    return;
                }

                CaptureRejected?.Invoke(new SettlementCaptureRemoteResult(
                    data.RequesterOwnerId,
                    data.UnitId,
                    data.TargetEntityId,
                    data.TargetPosition,
                    data.RequestId,
                    string.IsNullOrWhiteSpace(data.RejectionReason)
                        ? "Settlement capture was rejected by host."
                        : data.RejectionReason));
                return;
            }

            if (data.Kind != GameActionMessageKind.Request || !IsOfflineOrHost())
                return;
            if (string.IsNullOrWhiteSpace(data.RequestId))
            {
                RejectSettlementCapture(senderId, data, "Request id is missing.");
                return;
            }

            string cacheKey = $"{senderId?.Trim()}:{data.RequestId.Trim()}";
            if (_confirmedSettlementCaptures.TryGetValue(cacheKey, out var cached))
            {
                _syncService.SendCommandToPeer(
                    senderId,
                    GameCommandType.SettlementCaptureCommand,
                    cached.ToBytes());
                return;
            }

            if (!TryResolveAuthorizedRequestOwner(
                    senderId,
                    data.RequesterOwnerId,
                    data.RequesterOwnerId,
                    out string authorizedOwnerId,
                    out string authorizationReason))
            {
                RejectSettlementCapture(senderId, data, authorizationReason);
                return;
            }

            if (!TryValidateSettlementCapture(
                    authorizedOwnerId,
                    data.UnitId,
                    data.TargetEntityId,
                    data.TargetPosition,
                    out ConstructionBuildingCombatTarget target,
                    out string validationReason))
            {
                RejectSettlementCapture(senderId, data, validationReason);
                return;
            }

            if (_settlementCaptureService == null)
            {
                RejectSettlementCapture(senderId, data, "Settlement capture service is not available.");
                return;
            }

            SettlementCaptureResult result = _settlementCaptureService.CaptureSettlementAtPosition(
                target.Position,
                target.OwnerId,
                authorizedOwnerId,
                "network-captured-by-unit");
            if (!result.Succeeded)
            {
                RejectSettlementCapture(senderId, data, result.Reason);
                return;
            }

            var confirmed = new SettlementCaptureCommandPayload(
                GameActionMessageKind.Confirmed,
                authorizedOwnerId,
                data.UnitId,
                data.TargetEntityId,
                target.Position,
                result.PreviousOwnerId,
                result.NewOwnerId,
                data.RequestId);
            _confirmedSettlementCaptures[cacheKey] = confirmed;
            TrimSettlementCaptureCache();
            SendSettlementCaptureConfirmationToObservers(confirmed);
        }

        private void RejectSettlementCapture(
            string senderId,
            SettlementCaptureCommandPayload request,
            string reason)
        {
            string normalizedReason = string.IsNullOrWhiteSpace(reason)
                ? "Settlement capture was rejected."
                : reason.Trim();
            LogSettlementCaptureAuthorityWarning(
                $"Rejected capture request from '{senderId}': {normalizedReason}");

            if (string.IsNullOrWhiteSpace(senderId))
                return;

            var rejected = new SettlementCaptureCommandPayload(
                GameActionMessageKind.Rejected,
                request.RequesterOwnerId,
                request.UnitId,
                request.TargetEntityId,
                request.TargetPosition,
                string.Empty,
                string.Empty,
                request.RequestId,
                normalizedReason);
            _syncService.SendCommandToPeer(
                senderId,
                GameCommandType.SettlementCaptureCommand,
                rejected.ToBytes());
        }

        private bool ApplyConfirmedSettlementCapture(SettlementCaptureCommandPayload data)
        {
            if (_settlementCaptureService == null)
                return false;

            _applyingNetworkEvent = true;
            try
            {
                SettlementCaptureResult result = _settlementCaptureService.CaptureSettlementAtPosition(
                    data.TargetPosition,
                    data.PreviousOwnerId,
                    data.NewOwnerId,
                    "network-confirmed");
                if (result.Succeeded)
                    return true;

                LogSettlementCaptureAuthorityWarning(
                    $"Confirmed capture could not be applied at {data.TargetPosition.x},{data.TargetPosition.y}: {result.Reason}");
                return false;
            }
            finally
            {
                _applyingNetworkEvent = false;
            }
        }

        private bool TryValidateSettlementCapture(
            string ownerId,
            string unitId,
            string targetEntityId,
            Vector2Int targetPosition,
            out ConstructionBuildingCombatTarget target,
            out string reason)
        {
            target = default;
            reason = string.Empty;

            if (_unitOwnershipQuery == null
                || !string.Equals(_unitOwnershipQuery.GetUnitOwnerId(unitId), ownerId, StringComparison.Ordinal))
            {
                reason = "Only the requester's own unit can capture a settlement.";
                return false;
            }

            if (_unitService == null
                || !_unitService.TryGetUnitPosition(unitId, out Vector2Int unitPosition))
            {
                reason = "Unit position is unavailable.";
                return false;
            }

            if (_buildingTargetQuery == null
                || !_buildingTargetQuery.TryGetCombatTarget(targetEntityId, out target)
                || target.Position != targetPosition)
            {
                reason = "Target building is not available for capture.";
                return false;
            }

            if (string.Equals(target.OwnerId, ownerId, StringComparison.Ordinal))
            {
                reason = "Requester already controls this settlement.";
                return false;
            }

            BuildingDefinition definition = _buildingRegistry?.GetById(target.BuildingId);
            if (!BuildingDefinitionCapabilities.IsCastle(definition)
                && !BuildingDefinitionCapabilities.IsTownHall(definition))
            {
                reason = "Only castles and town halls can be captured.";
                return false;
            }

            int dx = Math.Abs(unitPosition.x - target.Position.x);
            int dy = Math.Abs(unitPosition.y - target.Position.y);
            if (Math.Max(dx, dy) > 1)
            {
                reason = "Unit must be adjacent to the settlement center.";
                return false;
            }

            if (_healthRegistry == null || !_healthRegistry.TryGet(target.EntityId, out IHealth health))
            {
                reason = "Target defenses are unavailable.";
                return false;
            }

            int captureThreshold = Math.Max(1, Mathf.CeilToInt(health.MaxHp * 0.25f));
            if (health.CurrentHp > captureThreshold)
            {
                reason = $"Reduce defenses to {captureThreshold} HP or less before capture.";
                return false;
            }

            return true;
        }

        private void TrimSettlementCaptureCache()
        {
            const int MaximumEntries = 512;
            if (_confirmedSettlementCaptures.Count <= MaximumEntries)
                return;
            foreach (string key in new List<string>(_confirmedSettlementCaptures.Keys))
            {
                _confirmedSettlementCaptures.Remove(key);
                if (_confirmedSettlementCaptures.Count <= MaximumEntries)
                    return;
            }
        }

        private void SendSettlementCaptureConfirmationToObservers(
            SettlementCaptureCommandPayload payload)
        {
            byte[] bytes = payload.ToBytes();
            var participants = _sessionManager?.Participants;
            if (participants == null || participants.Count == 0)
            {
                _syncService.SendCommand(
                    GameCommandType.SettlementCaptureCommand,
                    bytes);
                return;
            }

            string localPlayerId =
                NormalizeOwnerId(_sessionManager.LocalPlayerId);
            string requesterOwnerId =
                NormalizeOwnerId(payload.RequesterOwnerId);
            string previousOwnerId =
                NormalizeOwnerId(payload.PreviousOwnerId);
            string newOwnerId =
                NormalizeOwnerId(payload.NewOwnerId);

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

                bool isParticipantOwner =
                    string.Equals(peerId, requesterOwnerId, StringComparison.Ordinal)
                    || string.Equals(peerId, previousOwnerId, StringComparison.Ordinal)
                    || string.Equals(peerId, newOwnerId, StringComparison.Ordinal);
                bool canObserve =
                    isParticipantOwner
                    || CanPeerObserveWorldEvent(peerId, previousOwnerId, payload.TargetPosition)
                    || CanPeerObserveWorldEvent(peerId, newOwnerId, payload.TargetPosition);
                if (!canObserve)
                    continue;

                RevealCombatUnitToPeerIfNeeded(
                    peerId,
                    payload.UnitId,
                    requesterOwnerId,
                    ResolveUnitPositionOrFallback(
                        payload.UnitId,
                        payload.TargetPosition));

                _syncService.SendCommandToPeer(
                    peerId,
                    GameCommandType.SettlementCaptureCommand,
                    bytes);
            }
        }

        private Vector2Int ResolveUnitPositionOrFallback(
            string unitId,
            Vector2Int fallback)
        {
            return _unitService != null
                   && _unitService.TryGetUnitPosition(
                       unitId,
                       out Vector2Int position)
                ? position
                : fallback;
        }

        private static void LogSettlementCaptureAuthorityWarning(string message)
        {
            if (!Application.isEditor && !Debug.isDebugBuild)
                return;
            Debug.LogWarning($"[MultiplayerAuthority] {message}");
        }
    }
}
