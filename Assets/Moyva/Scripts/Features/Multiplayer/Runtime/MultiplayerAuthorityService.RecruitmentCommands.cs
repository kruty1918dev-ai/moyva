using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Recruitment command path. Host executes the canonical
    /// <see cref="IUnitRecruitmentService"/>; clients send Request payloads and
    /// apply the authoritative full-queue <c>UnitRecruitmentSync</c> broadcast.
    /// Deploy also produces a unit which replicates through the existing
    /// UnitSpawn replication.
    /// </summary>
    internal sealed partial class MultiplayerAuthorityService
    {
        public bool TryRequestEnqueue(
            string ownerId,
            Vector2Int recruitingBuildingPosition,
            string unitTypeId,
            out string reason)
        {
            reason = null;
            if (IsOfflineOrHost())
            {
                reason = "Local host executes recruitment directly.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(unitTypeId))
            {
                reason = "Recruitment request is missing the unit type.";
                return false;
            }

            SendRecruitmentRequest(
                new UnitRecruitmentCommandPayload(
                    GameActionMessageKind.Request,
                    UnitRecruitmentCommandAction.Enqueue,
                    ownerId,
                    recruitingBuildingPosition,
                    0,
                    unitTypeId,
                    default));
            return true;
        }

        public bool TryRequestCancel(
            string ownerId,
            Vector2Int recruitingBuildingPosition,
            long queueId,
            out string reason)
        {
            reason = null;
            if (IsOfflineOrHost())
            {
                reason = "Local host executes recruitment directly.";
                return false;
            }

            if (queueId <= 0)
            {
                reason = "Recruitment cancel request is missing the queue id.";
                return false;
            }

            SendRecruitmentRequest(
                new UnitRecruitmentCommandPayload(
                    GameActionMessageKind.Request,
                    UnitRecruitmentCommandAction.Cancel,
                    ownerId,
                    recruitingBuildingPosition,
                    queueId,
                    string.Empty,
                    default));
            return true;
        }

        public bool TryRequestDeploy(
            string ownerId,
            Vector2Int recruitingBuildingPosition,
            long queueId,
            Vector2Int targetPosition,
            out string reason)
        {
            reason = null;
            if (IsOfflineOrHost())
            {
                reason = "Local host executes recruitment directly.";
                return false;
            }

            if (queueId <= 0)
            {
                reason = "Recruitment deploy request is missing the queue id.";
                return false;
            }

            SendRecruitmentRequest(
                new UnitRecruitmentCommandPayload(
                    GameActionMessageKind.Request,
                    UnitRecruitmentCommandAction.Deploy,
                    ownerId,
                    recruitingBuildingPosition,
                    queueId,
                    string.Empty,
                    targetPosition));
            return true;
        }

        private void SendRecruitmentRequest(UnitRecruitmentCommandPayload payload)
            => SendRequestToHost(GameCommandType.UnitRecruitmentCommand, payload.ToBytes());

        // Host: authoritative queue broadcast on every queue change
        // (enqueue, cancel, deploy, turn progress, building loss).
        private void OnRecruitmentQueueChangedBroadcast(
            UnitRecruitmentQueueChangedSignal signal)
        {
            if (!IsOfflineOrHost() || _applyingNetworkEvent
                || _recruitmentStateStore == null)
            {
                return;
            }

            _syncService.SendCommand(
                GameCommandType.UnitRecruitmentSync,
                BuildRecruitmentSyncPayload(
                    _recruitmentStateStore.CaptureState()).ToBytes());
        }

        private void OnNetworkUnitRecruitmentCommand(string senderId, byte[] body)
        {
            UnitRecruitmentCommandPayload data;
            try { data = UnitRecruitmentCommandPayload.FromBytes(body); }
            catch (Exception exception)
            {
                LogCombatAuthorityWarning(
                    $"Invalid recruitment payload from '{senderId}': {exception.Message}");
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

                FireRecruitmentCommandRejected(
                    string.IsNullOrWhiteSpace(data.RejectionReason)
                        ? "Recruitment command was rejected by host."
                        : data.RejectionReason);
                return;
            }

            if (data.Kind != GameActionMessageKind.Request || !IsOfflineOrHost())
                return;
            if (_recruitmentService == null)
            {
                RejectRecruitmentCommand(senderId, data, "Recruitment service is not available.");
                return;
            }

            if (!TryResolveAuthorizedRequestOwner(
                    senderId,
                    data.RequesterOwnerId,
                    data.RequesterOwnerId,
                    out string authorizedOwnerId,
                    out string authorizationReason))
            {
                RejectRecruitmentCommand(senderId, data, authorizationReason);
                return;
            }

            bool executed = ExecuteHostRecruitmentCommand(authorizedOwnerId, data, out string reason);
            if (!executed)
                RejectRecruitmentCommand(senderId, data, reason);
            // On success UnitRecruitmentQueueChangedSignal fires →
            // OnRecruitmentQueueChangedBroadcast replicates the full table.
        }

        private bool ExecuteHostRecruitmentCommand(
            string ownerId,
            UnitRecruitmentCommandPayload data,
            out string reason)
        {
            reason = null;
            switch (data.Action)
            {
                case UnitRecruitmentCommandAction.Enqueue:
                    return _recruitmentService.TryEnqueue(
                        ownerId, data.BuildingPosition, data.UnitTypeId, out reason);

                case UnitRecruitmentCommandAction.Cancel:
                    return _recruitmentService.TryCancel(
                        ownerId, data.BuildingPosition, data.QueueId, out reason);

                case UnitRecruitmentCommandAction.Deploy:
                    return _recruitmentService.TryDeployReady(
                        ownerId, data.BuildingPosition, data.QueueId,
                        data.TargetPosition, out _, out reason);

                default:
                    reason = "Unknown recruitment command.";
                    return false;
            }
        }

        private void RejectRecruitmentCommand(
            string senderId,
            UnitRecruitmentCommandPayload request,
            string reason)
        {
            string normalizedReason = string.IsNullOrWhiteSpace(reason)
                ? "Recruitment command was rejected."
                : reason.Trim();
            LogCombatAuthorityWarning(
                $"Rejected recruitment command from '{senderId}': {normalizedReason}");

            if (string.IsNullOrWhiteSpace(senderId))
                return;

            var rejected = new UnitRecruitmentCommandPayload(
                GameActionMessageKind.Rejected,
                request.Action,
                request.RequesterOwnerId,
                request.BuildingPosition,
                request.QueueId,
                request.UnitTypeId,
                request.TargetPosition,
                normalizedReason);
            _syncService.SendCommandToPeer(
                senderId,
                GameCommandType.UnitRecruitmentCommand,
                rejected.ToBytes());
        }

        private void OnNetworkUnitRecruitmentSync(string senderId, byte[] body)
        {
            if (IsOfflineOrHost() || !IsAuthorizedHostSender(senderId))
                return;
            if (_recruitmentStateStore == null)
                return;

            UnitRecruitmentSyncPayload data;
            try { data = UnitRecruitmentSyncPayload.FromBytes(body); }
            catch (Exception exception)
            {
                LogCombatAuthorityWarning(
                    $"Invalid recruitment sync payload from '{senderId}': {exception.Message}");
                return;
            }

            var items = new List<UnitRecruitmentQueueItemSnapshot>(data.Count);
            for (int index = 0; index < data.Count; index++)
            {
                items.Add(new UnitRecruitmentQueueItemSnapshot(
                    data.QueueIds[index],
                    data.OwnerIds[index],
                    data.BuildingPositions[index],
                    data.BuildingIds[index],
                    data.UnitTypeIds[index],
                    data.CompletedTurns[index],
                    data.TrainingTurns[index],
                    data.EnqueuedGlobalTurns[index],
                    data.LastProgressGlobalTurns[index],
                    (UnitRecruitmentQueueStatus)data.Statuses[index],
                    paidCosts: null,
                    fundingSettlementId: data.FundingSettlementIds[index],
                    trainingSeconds: data.TrainingSeconds[index],
                    completedSeconds: data.CompletedSeconds[index]));
            }

            _applyingNetworkEvent = true;
            try { _recruitmentStateStore.RestoreState(items); }
            finally { _applyingNetworkEvent = false; }
        }

        private void FireRecruitmentCommandRejected(string reason)
        {
            _signalBus?.Fire(new UnitRecruitmentCommandRejectedSignal
            {
                Reason = reason ?? string.Empty,
            });
        }

        private static UnitRecruitmentSyncPayload BuildRecruitmentSyncPayload(
            IReadOnlyList<UnitRecruitmentQueueItemSnapshot> items)
        {
            int count = items?.Count ?? 0;
            var payload = new UnitRecruitmentSyncPayload(
                new long[count],
                new string[count],
                new Vector2Int[count],
                new string[count],
                new string[count],
                new int[count],
                new int[count],
                new long[count],
                new long[count],
                new byte[count],
                new string[count],
                new float[count],
                new float[count]);
            for (int index = 0; index < count; index++)
            {
                UnitRecruitmentQueueItemSnapshot item = items[index];
                payload.QueueIds[index] = item.QueueId;
                payload.OwnerIds[index] = item.OwnerId;
                payload.BuildingPositions[index] = item.RecruitingBuildingPosition;
                payload.BuildingIds[index] = item.RecruitingBuildingId;
                payload.UnitTypeIds[index] = item.UnitTypeId;
                payload.CompletedTurns[index] = item.CompletedTurns;
                payload.TrainingTurns[index] = item.TrainingTurns;
                payload.EnqueuedGlobalTurns[index] = item.EnqueuedGlobalTurn;
                payload.LastProgressGlobalTurns[index] = item.LastProgressGlobalTurn;
                payload.Statuses[index] = (byte)item.Status;
                payload.FundingSettlementIds[index] = item.FundingSettlementId;
                payload.TrainingSeconds[index] = item.TrainingSeconds;
                payload.CompletedSeconds[index] = item.CompletedSeconds;
            }
            return payload;
        }
    }
}
