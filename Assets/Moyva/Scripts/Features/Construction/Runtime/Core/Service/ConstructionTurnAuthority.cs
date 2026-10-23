using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Turns.API;
using UnityEngine;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal readonly struct ConstructionTurnAuthoritySnapshot
    {
        public ConstructionTurnAuthoritySnapshot(
            bool hasTurnAuthority,
            string activeOwnerId,
            string localOwnerId,
            TurnPhase phase,
            bool isRealtimeSandbox = false,
            TurnFlowMode flowMode = TurnFlowMode.Sequential)
        {
            HasTurnAuthority = hasTurnAuthority;
            ActiveOwnerId = activeOwnerId ?? string.Empty;
            LocalOwnerId = localOwnerId ?? string.Empty;
            Phase = phase;
            IsRealtimeSandbox = isRealtimeSandbox;
            FlowMode = flowMode;
        }

        public bool HasTurnAuthority { get; }
        public string ActiveOwnerId { get; }
        public string LocalOwnerId { get; }
        public TurnPhase Phase { get; }
        public bool IsRealtimeSandbox { get; }
        public TurnFlowMode FlowMode { get; }
    }

    internal static class ConstructionTurnAuthorityPolicy
    {
        public static bool TryAuthorize(
            string requestedOwnerId,
            in ConstructionTurnAuthoritySnapshot snapshot,
            bool requireLocalOwner,
            out string normalizedOwnerId,
            out string reason)
        {
            normalizedOwnerId = requestedOwnerId?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedOwnerId))
            {
                normalizedOwnerId = string.Empty;
                reason = "Construction command owner is empty.";
                return false;
            }

            if (snapshot.IsRealtimeSandbox)
            {
                if (!requireLocalOwner)
                {
                    reason = null;
                    return true;
                }

                string localOwnerId = snapshot.LocalOwnerId?.Trim();
                if (string.IsNullOrWhiteSpace(localOwnerId))
                {
                    reason = null;
                    return true;
                }

                if (string.Equals(localOwnerId, normalizedOwnerId, StringComparison.Ordinal))
                {
                    reason = null;
                    return true;
                }

                reason =
                    $"Owner '{normalizedOwnerId}' is not the local sandbox owner '{localOwnerId}'.";
                return false;
            }

            if (!snapshot.HasTurnAuthority)
            {
                reason = "Turn authority is unavailable.";
                return false;
            }

            if (snapshot.Phase != TurnPhase.AwaitingInput)
            {
                reason =
                    $"Turn phase is {snapshot.Phase}, expected {TurnPhase.AwaitingInput}.";
                return false;
            }

            // Sequential mode requires the requester to hold the active turn.
            // In simultaneous mode every living faction may act during the
            // shared AwaitingInput window; CanOwnerAct performs the real
            // configured/alive/pending validation for the requesting owner.
            if (snapshot.FlowMode == TurnFlowMode.Sequential)
            {
                string activeOwnerId = snapshot.ActiveOwnerId?.Trim();
                if (string.IsNullOrWhiteSpace(activeOwnerId))
                {
                    reason = "Active turn owner is empty.";
                    return false;
                }

                if (!string.Equals(
                        activeOwnerId,
                        normalizedOwnerId,
                        StringComparison.Ordinal))
                {
                    reason =
                        $"Owner '{normalizedOwnerId}' is not the active turn owner '{activeOwnerId}'.";
                    return false;
                }
            }

            if (requireLocalOwner)
            {
                string localOwnerId = snapshot.LocalOwnerId?.Trim();
                if (string.IsNullOrWhiteSpace(localOwnerId))
                {
                    reason = "Local turn owner is empty.";
                    return false;
                }

                if (!string.Equals(
                        localOwnerId,
                        normalizedOwnerId,
                        StringComparison.Ordinal))
                {
                    reason =
                        $"Owner '{normalizedOwnerId}' is not the local turn owner '{localOwnerId}'.";
                    return false;
                }
            }

            reason = null;
            return true;
        }
    }
}
