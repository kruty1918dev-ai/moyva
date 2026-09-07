using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Combat.API;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    internal sealed partial class MultiplayerAuthorityService
    {
        private readonly Dictionary<string, CombatCommandPayload> _confirmedCombatRequests =
            new(StringComparer.Ordinal);
        private readonly HashSet<string> _appliedCombatConfirmations =
            new(StringComparer.Ordinal);

        public event Action<CombatRemoteCommandResult> AttackRejected;

        public bool TryRequestAttack(
            string requesterOwnerId,
            string attackerEntityId,
            string targetEntityId,
            out string reason)
        {
            reason = null;
            if (IsOfflineOrHost())
            {
                reason = "Local host executes combat directly.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(requesterOwnerId)
                || string.IsNullOrWhiteSpace(attackerEntityId)
                || string.IsNullOrWhiteSpace(targetEntityId))
            {
                reason = "Combat request is missing owner, attacker, or target.";
                return false;
            }

            var payload = new CombatCommandPayload(
                GameActionMessageKind.Request,
                requesterOwnerId,
                attackerEntityId,
                targetEntityId,
                0,
                false,
                Guid.NewGuid().ToString("N"));
            SendRequestToHost(
                GameCommandType.CombatCommand,
                payload.ToBytes());
            return true;
        }

        private async void OnNetworkCombatCommand(string senderId, byte[] body)
        {
            CombatCommandPayload data;
            try { data = CombatCommandPayload.FromBytes(body); }
            catch (Exception exception)
            {
                LogCombatAuthorityWarning($"Invalid combat payload from '{senderId}': {exception.Message}");
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

                string confirmationKey = $"{data.RequesterOwnerId}:{data.AttackerEntityId}:{data.TargetEntityId}:{data.RequestId}";
                if (_appliedCombatConfirmations.Contains(confirmationKey))
                    return;

                if (ApplyConfirmedCombat(data))
                    _appliedCombatConfirmations.Add(confirmationKey);
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

                AttackRejected?.Invoke(new CombatRemoteCommandResult(
                    data.RequesterOwnerId,
                    data.AttackerEntityId,
                    data.TargetEntityId,
                    data.RequestId,
                    string.IsNullOrWhiteSpace(data.RejectionReason)
                        ? "Attack was rejected by host."
                        : data.RejectionReason));
                return;
            }

            if (data.Kind != GameActionMessageKind.Request || !IsOfflineOrHost())
                return;
            if (string.IsNullOrWhiteSpace(data.RequestId))
            {
                RejectCombat(senderId, data, "Request id is missing.");
                return;
            }

            string cacheKey = $"{senderId?.Trim()}:{data.RequestId.Trim()}";
            if (_confirmedCombatRequests.TryGetValue(cacheKey, out var cached))
            {
                _syncService.SendCommandToPeer(
                    senderId,
                    GameCommandType.CombatCommand,
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
                RejectCombat(senderId, data, authorizationReason);
                return;
            }

            if (_combatCommandService == null)
            {
                RejectCombat(senderId, data, "Combat command service is not available.");
                return;
            }

            bool hasReplicationScope =
                TryResolveCombatReplicationScope(
                    data,
                    out string attackerOwnerId,
                    out Vector2Int attackerPosition,
                    out string targetOwnerId,
                    out Vector2Int targetPosition);

            CombatCommandResult result;
            try
            {
                result = await _combatCommandService.ExecuteAsync(
                    authorizedOwnerId, data.AttackerEntityId, data.TargetEntityId, _lifetime.Token);
            }
            catch (OperationCanceledException) when (_disposed) { return; }
            if (_disposed) return;
            if (!result.Succeeded)
            {
                RejectCombat(senderId, data, result.Reason);
                return;
            }

            var confirmed = new CombatCommandPayload(
                GameActionMessageKind.Confirmed,
                authorizedOwnerId,
                data.AttackerEntityId,
                data.TargetEntityId,
                result.DamageApplied,
                result.TargetDied,
                data.RequestId);
            _confirmedCombatRequests[cacheKey] = confirmed;
            TrimConfirmedCombatCache();
            if (hasReplicationScope)
            {
                SendCombatConfirmationToObservers(
                    confirmed,
                    attackerOwnerId,
                    attackerPosition,
                    targetOwnerId,
                    targetPosition);
            }
            else
            {
                SendCombatConfirmationToObservers(confirmed);
            }
        }

        private void RejectCombat(
            string senderId,
            CombatCommandPayload request,
            string reason)
        {
            string normalizedReason = string.IsNullOrWhiteSpace(reason)
                ? "Attack was rejected."
                : reason.Trim();
            LogCombatAuthorityWarning(
                $"Rejected combat request from '{senderId}': {normalizedReason}");

            if (string.IsNullOrWhiteSpace(senderId))
                return;

            var rejected = new CombatCommandPayload(
                GameActionMessageKind.Rejected,
                request.RequesterOwnerId,
                request.AttackerEntityId,
                request.TargetEntityId,
                0,
                false,
                request.RequestId,
                normalizedReason);
            _syncService.SendCommandToPeer(
                senderId,
                GameCommandType.CombatCommand,
                rejected.ToBytes());
        }

        private bool ApplyConfirmedCombat(CombatCommandPayload data)
        {
            if (_healthRegistry == null
                || string.IsNullOrWhiteSpace(data.TargetEntityId)
                || data.DamageApplied <= 0)
            {
                return false;
            }

            _applyingNetworkEvent = true;
            try
            {
                if (_healthRegistry.TryGet(data.TargetEntityId, out IHealth health)
                    && health != null
                    && !health.IsDestroyed)
                {
                    health.TakeDamage(data.DamageApplied);
                    return true;
                }

                if (data.TargetDied)
                    return true;
            }
            finally
            {
                _applyingNetworkEvent = false;
            }

            if (data.TargetDied)
            {
                // Building/unit destruction can arrive through its own confirmed
                // replication before the combat result. In that case the target
                // missing from the health registry already represents the host's
                // final state for this command.
                return true;
            }

            LogCombatAuthorityWarning($"Confirmed combat target '{data.TargetEntityId}' is not available locally.");
            return false;
        }

        private void TrimConfirmedCombatCache()
        {
            const int MaximumEntries = 512;
            if (_confirmedCombatRequests.Count <= MaximumEntries)
                return;
            foreach (string key in new List<string>(_confirmedCombatRequests.Keys))
            {
                _confirmedCombatRequests.Remove(key);
                if (_confirmedCombatRequests.Count <= MaximumEntries)
                    return;
            }
        }

        private void SendCombatConfirmationToObservers(
            CombatCommandPayload payload)
        {
            if (!TryResolveCombatReplicationScope(
                    payload,
                    out string attackerOwnerId,
                    out Vector2Int attackerPosition,
                    out string targetOwnerId,
                    out Vector2Int targetPosition))
            {
                _syncService.SendCommand(
                    GameCommandType.CombatCommand,
                    payload.ToBytes());
                return;
            }

            SendCombatConfirmationToObservers(
                payload,
                attackerOwnerId,
                attackerPosition,
                targetOwnerId,
                targetPosition);
        }

        private void SendCombatConfirmationToObservers(
            CombatCommandPayload payload,
            string attackerOwnerId,
            Vector2Int attackerPosition,
            string targetOwnerId,
            Vector2Int targetPosition)
        {
            attackerOwnerId = NormalizeOwnerId(attackerOwnerId);
            targetOwnerId = NormalizeOwnerId(targetOwnerId);

            byte[] combatBytes =
                payload.ToBytes();
            var participants =
                _sessionManager?.Participants;
            if (participants == null || participants.Count == 0)
            {
                _syncService.SendCommand(
                    GameCommandType.CombatCommand,
                    combatBytes);
                return;
            }

            string localPlayerId =
                NormalizeOwnerId(_sessionManager.LocalPlayerId);
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
                    string.Equals(peerId, attackerOwnerId, StringComparison.Ordinal)
                    || string.Equals(peerId, targetOwnerId, StringComparison.Ordinal);
                bool canObserve =
                    isParticipantOwner
                    || CanPeerObserveWorldEvent(peerId, attackerOwnerId, attackerPosition)
                    || CanPeerObserveWorldEvent(peerId, targetOwnerId, targetPosition);
                if (!canObserve)
                    continue;

                RevealCombatUnitToPeerIfNeeded(
                    peerId,
                    payload.AttackerEntityId,
                    attackerOwnerId,
                    attackerPosition);
                RevealCombatUnitToPeerIfNeeded(
                    peerId,
                    payload.TargetEntityId,
                    targetOwnerId,
                    targetPosition);

                _syncService.SendCommandToPeer(
                    peerId,
                    GameCommandType.CombatCommand,
                    combatBytes);
            }
        }

        private bool TryResolveCombatReplicationScope(
            CombatCommandPayload payload,
            out string attackerOwnerId,
            out Vector2Int attackerPosition,
            out string targetOwnerId,
            out Vector2Int targetPosition)
        {
            attackerOwnerId =
                NormalizeOwnerId(payload.RequesterOwnerId);
            targetOwnerId = string.Empty;
            attackerPosition = default;
            targetPosition = default;

            if (_unitService == null
                || string.IsNullOrWhiteSpace(payload.AttackerEntityId)
                || !_unitService.TryGetUnitPosition(
                    payload.AttackerEntityId,
                    out attackerPosition))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(attackerOwnerId))
            {
                attackerOwnerId =
                    NormalizeOwnerId(
                        _unitOwnershipQuery?.GetUnitOwnerId(
                            payload.AttackerEntityId));
            }

            if (_unitService.TryGetUnitPosition(
                    payload.TargetEntityId,
                    out targetPosition))
            {
                targetOwnerId =
                    NormalizeOwnerId(
                        _unitOwnershipQuery?.GetUnitOwnerId(
                            payload.TargetEntityId));
                return true;
            }

            if (_buildingTargetQuery != null
                && _buildingTargetQuery.TryGetCombatTarget(
                    payload.TargetEntityId,
                    out ConstructionBuildingCombatTarget target))
            {
                targetOwnerId = NormalizeOwnerId(target.OwnerId);
                targetPosition = target.Position;
                return true;
            }

            return false;
        }

        private void RevealCombatUnitToPeerIfNeeded(
            string peerId,
            string unitId,
            string unitOwnerId,
            Vector2Int position)
        {
            if (_unitService == null
                || string.IsNullOrWhiteSpace(unitId)
                || IsUnitKnownByPeer(peerId, unitId)
                || !_unitService.TryGetUnitPosition(unitId, out _))
            {
                return;
            }

            if (!CanPeerObserveWorldEvent(peerId, unitOwnerId, position)
                && !string.Equals(peerId, unitOwnerId, StringComparison.Ordinal))
            {
                return;
            }

            if (!TryCreateUnitSpawnPayload(
                    unitId,
                    unitOwnerId,
                    position,
                    out UnitSpawnPayload spawnPayload))
            {
                return;
            }

            MarkUnitKnownByPeer(peerId, unitId);
            _syncService.SendCommandToPeer(
                peerId,
                GameCommandType.UnitSpawn,
                spawnPayload.ToBytes());
        }

        private static void LogCombatAuthorityWarning(string message)
        {
            if (!Application.isEditor && !Debug.isDebugBuild)
                return;
            Debug.LogWarning($"[MultiplayerAuthority] {message}");
        }
    }
}
