using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using UnityEngine;
#if MOYVA_UGS_RELAY || MOYVA_UGS_LOBBY
using Unity.Services.Authentication;
using Unity.Services.Core;
#endif

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
        private readonly IMultiplayerLogger _logger;

        public MultiplayerIdentityService(IMultiplayerLogger logger)
        {
            _logger = logger;
        }

        public async Task<ParticipantIdentity> ResolveAsync(string preferredNickname, CancellationToken ct = default)
        {
            string nickname = string.IsNullOrWhiteSpace(preferredNickname)
                ? (string.IsNullOrWhiteSpace(Environment.UserName) ? "Player" : Environment.UserName)
                : preferredNickname.Trim();

#if MOYVA_UGS_RELAY || MOYVA_UGS_LOBBY
            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                    await UnityServices.InitializeAsync();

                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                var ugsId = AuthenticationService.Instance.PlayerId;
                if (!string.IsNullOrWhiteSpace(ugsId))
                    return new ParticipantIdentity(ugsId, nickname);
            }
            catch (Exception e)
            {
                _logger?.Warn($"[Identity] UGS sign-in failed: {e.Message}. Falling back to device id.");
            }
#else
            await Task.CompletedTask;
#endif

            string fallback = SystemInfo.deviceUniqueIdentifier;
            if (string.IsNullOrWhiteSpace(fallback) || fallback == SystemInfo.unsupportedIdentifier)
                fallback = $"local-{Guid.NewGuid():N}";

            return new ParticipantIdentity(fallback, nickname);
        }
    }
}
