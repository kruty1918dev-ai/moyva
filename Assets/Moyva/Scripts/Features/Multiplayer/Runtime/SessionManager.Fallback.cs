using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Diagnostics.API;
using Kruty1918.Moyva.Diagnostics.Runtime.Flows;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Persistence;
using UnityEngine.SceneManagement;

namespace Kruty1918.Moyva.Multiplayer.Core
{
    public sealed partial class SessionManager
    {
        private bool StartLocalSession(SessionConnectOptions opts)
        {
            var roomId = $"{LocalSessionPrefix}-{Guid.NewGuid():N}";
            _participants.Clear();
            _currentSessionId = roomId;
            _currentRules = opts.Rules ?? LocalSessionRules;
            _isHost = true;
            _participants.Add(new Participant(opts.LocalIdentity, isHost: true));
            return true;
        }

        internal static uint ComputeConfigChecksum(MultiplayerConfig config)
        {
            const uint FnvOffsetBasis = 2166136261u;
            const uint FnvPrime = 16777619u;
            unchecked
            {
                uint crc = FnvOffsetBasis;
                crc = (crc ^ (uint)config.SchemaVersion) * FnvPrime;
                crc = (crc ^ (uint)config.ProviderType) * FnvPrime;
                crc = (crc ^ (config.StrictParticipantLock ? 1u : 0u)) * FnvPrime;
                crc = (crc ^ (config.EnforceConfigConsistency ? 1u : 0u)) * FnvPrime;
                crc = (crc ^ (uint)config.DefaultSessionRules.Mode) * FnvPrime;
                crc = (crc ^ (uint)config.DefaultSessionRules.MaxParticipants) * FnvPrime;
                return crc;
            }
        }

        private SessionConnectOptions NormalizeOptions(SessionConnectOptions options)
        {
            var localIdentity = options.LocalIdentity ?? new ParticipantIdentity("local-player", "Local Player");
            var rules = options.Rules ?? _config.DefaultSessionRules;
            var roomId = string.IsNullOrWhiteSpace(options.RoomId)
                ? $"room-{Guid.NewGuid():N}".Substring(0, 12)
                : options.RoomId.Trim();

            return new SessionConnectOptions(
                localIdentity,
                roomId,
                options.CreateIfNotExists,
                rules,
                options.ConfigChecksum);
        }

        private async Task<bool> FailSessionAsync(CancellationToken ct)
        {
            await SafeCleanupAsync(ct);
            return false;
        }

        private void CleanupHostAliasParticipants()
        {
            if (!_isHost || string.IsNullOrEmpty(_localPlayerId))
                return;

            for (int i = _participants.Count - 1; i >= 0; i--)
            {
                var participant = _participants[i];
                if (participant.IsHost && !string.Equals(participant.Identity.PlayerId, _localPlayerId, StringComparison.Ordinal))
                    _participants.RemoveAt(i);
            }

            int localIndex = _participants.FindIndex(p => string.Equals(p.Identity.PlayerId, _localPlayerId, StringComparison.Ordinal));
            if (localIndex >= 0 && !_participants[localIndex].IsHost)
                _participants[localIndex] = _participants[localIndex].AsHost();
        }
    }
}
