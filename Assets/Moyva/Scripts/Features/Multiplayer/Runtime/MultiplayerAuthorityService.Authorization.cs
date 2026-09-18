using System;
using System.Threading;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.GameMode.API;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Signals;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    internal sealed partial class MultiplayerAuthorityService
    {
        // ─── Допоміжне ───────────────────────────────────────────────────────────

        private bool IsOfflineOrHost()
        {
            return _roleResolver == null
                || _roleResolver.Resolve().IsAuthoritative;
        }

        private bool TryResolveAuthorizedRequestOwner(
            string senderId,
            string requestedOwnerId,
            string requestedSourceOwnerId,
            out string authorizedOwnerId,
            out string reason)
            => TryResolveAuthorizedRequestOwner(
                _sessionManager?.Participants,
                senderId,
                requestedOwnerId,
                requestedSourceOwnerId,
                out authorizedOwnerId,
                out reason);

        internal static bool TryResolveAuthorizedRequestOwner(
            System.Collections.Generic.IReadOnlyList<Participant>
                participants,
            string senderId,
            string requestedOwnerId,
            string requestedSourceOwnerId,
            out string authorizedOwnerId,
            out string reason)
        {
            authorizedOwnerId = null;
            reason = null;
            string normalizedSender = senderId?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedSender))
            {
                reason = "Transport sender identity is empty.";
                return false;
            }

            Participant authorizedParticipant = null;
            if (participants != null)
            {
                for (int index = 0;
                     index < participants.Count;
                     index++)
                {
                    Participant candidate = participants[index];
                    if (candidate?.Identity == null
                        || !string.Equals(
                            candidate.Identity.PlayerId,
                            normalizedSender,
                            System.StringComparison.Ordinal))
                    {
                        continue;
                    }

                    authorizedParticipant = candidate;
                    break;
                }
            }

            if (authorizedParticipant == null)
            {
                reason =
                    $"Sender '{normalizedSender}' is not an active participant.";
                return false;
            }

            if (!MatchesRequestedOwner(
                    requestedOwnerId,
                    normalizedSender)
                || !MatchesRequestedOwner(
                    requestedSourceOwnerId,
                    normalizedSender))
            {
                reason =
                    $"Requested owner does not match sender '{normalizedSender}'.";
                return false;
            }

            authorizedOwnerId = normalizedSender;
            return true;
        }

        private bool IsAuthorizedHostSender(string senderId)
            => IsAuthorizedHostSender(
                _sessionManager?.Participants,
                senderId);

        internal static bool IsAuthorizedHostSender(
            System.Collections.Generic.IReadOnlyList<Participant>
                participants,
            string senderId)
        {
            string normalizedSender = senderId?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedSender))
                return false;

            if (participants == null)
                return false;

            for (int index = 0; index < participants.Count; index++)
            {
                Participant participant = participants[index];
                if (participant?.Identity != null
                    && participant.IsHost
                    && string.Equals(
                        participant.Identity.PlayerId,
                        normalizedSender,
                        System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool MatchesRequestedOwner(
            string requestedOwnerId,
            string senderId)
            => string.IsNullOrWhiteSpace(requestedOwnerId)
                || string.Equals(
                    requestedOwnerId.Trim(),
                    senderId,
                    System.StringComparison.Ordinal);

        internal static bool IsUnitCommandAuthorized(
            string unitOwnerId,
            string requesterOwnerId)
            => !string.IsNullOrWhiteSpace(unitOwnerId)
               && !string.IsNullOrWhiteSpace(requesterOwnerId)
               && string.Equals(
                   unitOwnerId.Trim(),
                   requesterOwnerId.Trim(),
                   StringComparison.Ordinal);
    }
}
