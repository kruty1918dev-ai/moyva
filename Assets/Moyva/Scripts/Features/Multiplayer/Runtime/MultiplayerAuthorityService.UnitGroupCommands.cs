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
    /// Group/stack command path. Host executes the canonical
    /// <see cref="IUnitGroupService"/>; clients send Request payloads and apply
    /// the authoritative full-table <c>UnitGroupSync</c> broadcast. Membership
    /// changes replicate via the table sync; group move orders replicate through
    /// the existing per-unit UnitMove replication.
    /// </summary>
    internal sealed partial class MultiplayerAuthorityService
    {
        public bool TryRequestCreateGroup(
            string ownerId,
            IReadOnlyList<string> unitIds,
            out string reason)
        {
            reason = null;
            if (IsOfflineOrHost())
            {
                reason = "Local host executes group commands directly.";
                return false;
            }

            if (unitIds == null || unitIds.Count == 0)
            {
                reason = "Group request is missing member units.";
                return false;
            }

            var payload = new UnitGroupCommandPayload(
                GameActionMessageKind.Request,
                UnitGroupCommandAction.Create,
                ownerId,
                string.Empty,
                default,
                CopyUnitIds(unitIds));
            SendRequestToHost(GameCommandType.UnitGroupCommand, payload.ToBytes());
            return true;
        }

        public bool TryRequestDisbandGroup(
            string ownerId,
            string groupId,
            out string reason)
            => SendGroupRequest(
                ownerId, groupId, UnitGroupCommandAction.Disband,
                null, default, out reason);

        public bool TryRequestAddUnit(
            string ownerId,
            string groupId,
            string unitId,
            out string reason)
            => SendGroupRequest(
                ownerId, groupId, UnitGroupCommandAction.AddUnit,
                new[] { unitId }, default, out reason);

        public bool TryRequestRemoveUnit(
            string ownerId,
            string groupId,
            string unitId,
            out string reason)
            => SendGroupRequest(
                ownerId, groupId, UnitGroupCommandAction.RemoveUnit,
                new[] { unitId }, default, out reason);

        public bool TryRequestMoveGroup(
            string ownerId,
            string groupId,
            Vector2Int targetPosition,
            out string reason)
            => SendGroupRequest(
                ownerId, groupId, UnitGroupCommandAction.Move,
                null, targetPosition, out reason);

        private bool SendGroupRequest(
            string ownerId,
            string groupId,
            UnitGroupCommandAction action,
            string[] unitIds,
            Vector2Int targetPosition,
            out string reason)
        {
            reason = null;
            if (IsOfflineOrHost())
            {
                reason = "Local host executes group commands directly.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(groupId))
            {
                reason = "Group request is missing the group id.";
                return false;
            }

            var payload = new UnitGroupCommandPayload(
                GameActionMessageKind.Request,
                action,
                ownerId,
                groupId,
                targetPosition,
                unitIds);
            SendRequestToHost(GameCommandType.UnitGroupCommand, payload.ToBytes());
            return true;
        }

        // Local group-move orders (all roles): host executes, client requests.
        private void OnLocalMoveGroupRequest(MoveGroupRequestSignal signal)
        {
            if (_unitGroupService == null)
                return;

            string ownerId = NormalizeOwnerId(signal.RequesterOwnerId);
            if (!_unitGroupService.TryGetGroup(signal.GroupId, out UnitGroupSnapshot group)
                || !IsUnitCommandAuthorized(group.OwnerId, ownerId))
            {
                FireGroupCommandRejected("Group move was rejected: group is not yours.");
                return;
            }

            if (IsOfflineOrHost())
            {
                if (!_unitGroupService.TryMoveGroup(
                        ownerId, signal.GroupId, signal.TargetPosition, out string reason))
                {
                    FireGroupCommandRejected(
                        string.IsNullOrWhiteSpace(reason)
                            ? "Group move was rejected."
                            : reason);
                }
                return;
            }

            TryRequestMoveGroup(ownerId, signal.GroupId, signal.TargetPosition, out _);
        }

        // Host: authoritative group table broadcast on every membership change.
        private void OnUnitGroupChangedBroadcast(UnitGroupChangedSignal signal)
        {
            if (!IsOfflineOrHost() || _applyingNetworkEvent || _unitGroupStateStore == null)
                return;

            IReadOnlyList<UnitGroupSnapshot> groups = _unitGroupStateStore.CaptureState();
            _syncService.SendCommand(
                GameCommandType.UnitGroupSync,
                BuildGroupSyncPayload(groups).ToBytes());
        }

        private void OnNetworkUnitGroupCommand(string senderId, byte[] body)
        {
            UnitGroupCommandPayload data;
            try { data = UnitGroupCommandPayload.FromBytes(body); }
            catch (Exception exception)
            {
                LogCombatAuthorityWarning(
                    $"Invalid unit group payload from '{senderId}': {exception.Message}");
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

                FireGroupCommandRejected(
                    string.IsNullOrWhiteSpace(data.RejectionReason)
                        ? "Group command was rejected by host."
                        : data.RejectionReason);
                return;
            }

            if (data.Kind != GameActionMessageKind.Request || !IsOfflineOrHost())
                return;
            if (_unitGroupService == null)
            {
                RejectGroupCommand(senderId, data, "Unit group service is not available.");
                return;
            }

            if (!TryResolveAuthorizedRequestOwner(
                    senderId,
                    data.RequesterOwnerId,
                    data.RequesterOwnerId,
                    out string authorizedOwnerId,
                    out string authorizationReason))
            {
                RejectGroupCommand(senderId, data, authorizationReason);
                return;
            }

            bool executed = ExecuteHostGroupCommand(authorizedOwnerId, data, out string reason);
            if (!executed)
                RejectGroupCommand(senderId, data, reason);
            // On success UnitGroupChangedSignal fires → OnUnitGroupChangedBroadcast
            // replicates the full table to every peer.
        }

        private bool ExecuteHostGroupCommand(
            string ownerId,
            UnitGroupCommandPayload data,
            out string reason)
        {
            reason = null;
            switch (data.Action)
            {
                case UnitGroupCommandAction.Create:
                    return _unitGroupService.TryCreateGroup(
                        ownerId, data.UnitIds, out _, out reason);

                case UnitGroupCommandAction.Disband:
                    return IsAuthorizedGroupOwner(ownerId, data.GroupId, out reason)
                        && _unitGroupService.TryDisbandGroup(ownerId, data.GroupId, out reason);

                case UnitGroupCommandAction.AddUnit:
                    return IsAuthorizedGroupOwner(ownerId, data.GroupId, out reason)
                        && data.UnitIds != null && data.UnitIds.Length == 1
                        && _unitGroupService.TryAddUnit(ownerId, data.GroupId, data.UnitIds[0], out reason);

                case UnitGroupCommandAction.RemoveUnit:
                    return IsAuthorizedGroupOwner(ownerId, data.GroupId, out reason)
                        && data.UnitIds != null && data.UnitIds.Length == 1
                        && _unitGroupService.TryRemoveUnit(ownerId, data.GroupId, data.UnitIds[0], out reason);

                case UnitGroupCommandAction.Move:
                    return IsAuthorizedGroupOwner(ownerId, data.GroupId, out reason)
                        && _unitGroupService.TryMoveGroup(ownerId, data.GroupId, data.TargetPosition, out reason);

                default:
                    reason = "Unknown group command.";
                    return false;
            }
        }

        private bool IsAuthorizedGroupOwner(
            string ownerId,
            string groupId,
            out string reason)
        {
            reason = null;
            if (_unitGroupService == null
                || !_unitGroupService.TryGetGroup(groupId, out UnitGroupSnapshot group))
            {
                reason = "Group was not found.";
                return false;
            }

            if (!IsUnitCommandAuthorized(group.OwnerId, ownerId))
            {
                reason = "Group belongs to another player.";
                return false;
            }

            return true;
        }

        private void RejectGroupCommand(
            string senderId,
            UnitGroupCommandPayload request,
            string reason)
        {
            string normalizedReason = string.IsNullOrWhiteSpace(reason)
                ? "Group command was rejected."
                : reason.Trim();
            LogCombatAuthorityWarning(
                $"Rejected group command from '{senderId}': {normalizedReason}");

            if (string.IsNullOrWhiteSpace(senderId))
                return;

            var rejected = new UnitGroupCommandPayload(
                GameActionMessageKind.Rejected,
                request.Action,
                request.RequesterOwnerId,
                request.GroupId,
                request.TargetPosition,
                request.UnitIds,
                normalizedReason);
            _syncService.SendCommandToPeer(
                senderId,
                GameCommandType.UnitGroupCommand,
                rejected.ToBytes());
        }

        private void OnNetworkUnitGroupSync(string senderId, byte[] body)
        {
            if (IsOfflineOrHost() || !IsAuthorizedHostSender(senderId))
                return;
            if (_unitGroupStateStore == null)
                return;

            UnitGroupSyncPayload data;
            try { data = UnitGroupSyncPayload.FromBytes(body); }
            catch (Exception exception)
            {
                LogCombatAuthorityWarning(
                    $"Invalid unit group sync payload from '{senderId}': {exception.Message}");
                return;
            }

            var groups = new List<UnitGroupSnapshot>(data.GroupIds.Length);
            for (int index = 0; index < data.GroupIds.Length; index++)
            {
                if (string.IsNullOrWhiteSpace(data.GroupIds[index]))
                    continue;
                groups.Add(new UnitGroupSnapshot(
                    data.GroupIds[index],
                    data.OwnerIds[index],
                    data.MemberUnitIds[index] ?? Array.Empty<string>()));
            }

            _applyingNetworkEvent = true;
            try { _unitGroupStateStore.RestoreReplicated(groups); }
            finally { _applyingNetworkEvent = false; }
        }

        private void FireGroupCommandRejected(string reason)
        {
            _signalBus?.Fire(new UnitGroupCommandRejectedSignal { Reason = reason ?? string.Empty });
        }

        private static string[] CopyUnitIds(IReadOnlyList<string> unitIds)
        {
            var copy = new string[unitIds.Count];
            for (int index = 0; index < unitIds.Count; index++)
                copy[index] = unitIds[index] ?? string.Empty;
            return copy;
        }

        private static UnitGroupSyncPayload BuildGroupSyncPayload(
            IReadOnlyList<UnitGroupSnapshot> groups)
        {
            int count = groups?.Count ?? 0;
            var groupIds = new string[count];
            var ownerIds = new string[count];
            var memberIds = new string[count][];
            for (int index = 0; index < count; index++)
            {
                UnitGroupSnapshot snapshot = groups[index];
                groupIds[index] = snapshot.GroupId;
                ownerIds[index] = snapshot.OwnerId;
                var members = new string[snapshot.Count];
                for (int memberIndex = 0; memberIndex < snapshot.Count; memberIndex++)
                    members[memberIndex] = snapshot.UnitIds[memberIndex] ?? string.Empty;
                memberIds[index] = members;
            }
            return new UnitGroupSyncPayload(groupIds, ownerIds, memberIds);
        }
    }
}
