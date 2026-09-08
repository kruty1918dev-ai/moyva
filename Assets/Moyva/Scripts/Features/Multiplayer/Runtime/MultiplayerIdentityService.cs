using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Returns a stable <see cref="ParticipantIdentity"/> for the local player.
    /// When UGS Authentication is available, signs in anonymously and uses
    /// <c>AuthenticationService.Instance.PlayerId</c> so the identity matches
    /// the one visible to Relay and Lobby services.
    /// </summary>
    public interface IMultiplayerIdentityService
    {
        Task<ParticipantIdentity> ResolveAsync(string preferredNickname, CancellationToken ct = default);
    }

    /// <inheritdoc cref="IMultiplayerIdentityService"/>
    public sealed class MultiplayerIdentityService : IMultiplayerIdentityService
    {
        public async Task<ParticipantIdentity> ResolveAsync(string preferredNickname, CancellationToken ct = default)
        {
            string nickname = string.IsNullOrWhiteSpace(preferredNickname)
                ? (string.IsNullOrWhiteSpace(Environment.UserName) ? "Player" : Environment.UserName)
                : preferredNickname.Trim();

            try
            {
                await MultiplayerAuthenticationGate.EnsureReadyAsync(ct);

                var ugsId = AuthenticationService.Instance.PlayerId;
                if (!string.IsNullOrWhiteSpace(ugsId))
                    return new ParticipantIdentity(ugsId, nickname);
            }
            catch (Exception)
            {
            }

            string fallback = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrWhiteSpace(fallback) || fallback == SystemInfo.unsupportedIdentifier)
                fallback = $"local-{Guid.NewGuid():N}";

            if (!MultiplayerClientScope.IsDefault)
                fallback = $"{fallback}-{MultiplayerClientScope.ScopeId}";

            return new ParticipantIdentity(fallback, nickname);
        }
    }
}
