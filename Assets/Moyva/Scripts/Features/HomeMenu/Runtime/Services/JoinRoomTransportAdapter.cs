using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.Runtime.Services;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Shared.Common;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal sealed class JoinRoomTransportAdapter
    {
        private readonly ILobbyService _lobbyService;
        private readonly INetworkProvider _networkProvider;
        private readonly SwitchableNetworkProvider _switchableNetworkProvider;
        private readonly Func<NetworkProviderType> _providerTypeAccessor;
        private readonly ServiceModeProfile _menuProfile;

        public JoinRoomTransportAdapter(
            ILobbyService lobbyService,
            INetworkProvider networkProvider,
            SwitchableNetworkProvider switchableNetworkProvider,
            Func<NetworkProviderType> providerTypeAccessor,
            IServiceModeProfileProvider serviceModeProfileProvider = null)
        {
            _lobbyService = lobbyService;
            _networkProvider = networkProvider;
            _switchableNetworkProvider = switchableNetworkProvider;
            _providerTypeAccessor = Guard.NotNull(providerTypeAccessor, nameof(providerTypeAccessor));
            _menuProfile = serviceModeProfileProvider?.Get(ServiceRuntimeMode.Menu) ?? ServiceModeProfileDefaults.Menu;
        }

        /// <summary>
        /// Connects transport provider (LAN/Relay/etc.) after a successful lobby join.
        /// </summary>
        public async Task<Result> JoinNetworkSessionAsync(LobbyRoom room, string traceId, CancellationToken ct = default)
        {
            if (_networkProvider == null)
            {
                Debug.LogWarning($"[JoinRoomPanelService] [{traceId}] INetworkProvider not available; lobby joined without transport connection.");
                return Result.Success();
            }

            var providerType = _providerTypeAccessor();
            if (!await EnsureNetworkProviderMatchesLobbyAsync(providerType, ct))
            {
                return Result.Fail(DomainErrorCode.Network, $"Не вдалося перемкнути transport provider на {providerType}.");
            }

            var joinCode = await ResolveNetworkJoinCodeAsync(room, traceId, providerType, ct);
            if (string.IsNullOrWhiteSpace(joinCode))
            {
                const string error = "Хост ще не опублікував мережевий код підключення для цієї кімнати.";
                Debug.LogError($"[JoinRoomPanelService] [{traceId}] {error} LobbyId='{room?.LobbyId}' LobbyCode='{room?.LobbyCode}' Provider='{providerType}'.");
                await LeaveLobbyAfterFailedTransportJoinAsync(traceId, ct);
                return Result.Fail(DomainErrorCode.NotFound, error);
            }

            var normalizedJoinCode = joinCode.Trim();
            if (providerType == NetworkProviderType.Relay && !RelayJoinCodeUtility.IsValid(normalizedJoinCode))
            {
                Debug.LogError($"[JoinRoomPanelService] [{traceId}] Resolved transport code '{normalizedJoinCode}' is not a valid Relay join code. LobbyId='{room?.LobbyId}', LobbyCode='{room?.LobbyCode}', RelayJoinCode='{room?.RelayJoinCode}'.");
                await LeaveLobbyAfterFailedTransportJoinAsync(traceId, ct);
                return Result.Fail(DomainErrorCode.Validation, "Отримано невалідний Relay join code.");
            }

            Debug.Log($"[JoinRoomPanelService] [{traceId}] Joining transport session '{normalizedJoinCode}' using provider '{providerType}'.");
            var result = await MultiplayerReliabilityPolicy.RetryWithBackoffAndJitterAsync(
                async token => await _networkProvider.JoinSessionAsync(normalizedJoinCode, token),
                candidate => candidate != null && candidate.Success,
                maxAttempts: 3,
                baseDelay: TimeSpan.FromMilliseconds(250),
                operationName: "JoinSessionAsync",
                ct: ct);
            if (result == null || !result.Success)
            {
                var error = result?.ErrorMessage ?? "Не вдалося підключитися до мережевої сесії.";
                Debug.LogError($"[JoinRoomPanelService] [{traceId}] JoinSessionAsync failed: {error}");
                await LeaveLobbyAfterFailedTransportJoinAsync(traceId, ct);
                return Result.Fail(DomainErrorCode.Network, error);
            }

            return Result.Success();
        }

        private async Task<bool> EnsureNetworkProviderMatchesLobbyAsync(NetworkProviderType providerType, CancellationToken ct)
        {
            if (providerType == NetworkProviderType.Offline)
                return true;

            var switchable = _switchableNetworkProvider ?? _networkProvider as SwitchableNetworkProvider;
            if (switchable == null)
            {
                Debug.LogWarning($"[JoinRoomPanelService] Network provider is not switchable; cannot force transport provider {providerType}.");
                return true;
            }

            if (switchable.CurrentType == providerType)
                return true;

            try
            {
                await switchable.SwitchToAsync(providerType, ct);
                Debug.Log($"[JoinRoomPanelService] Transport provider switched to {switchable.CurrentType} before lobby join transport step.");
                return switchable.CurrentType == providerType;
            }
            catch (Exception e)
            {
                Debug.LogError($"[JoinRoomPanelService] Failed to switch transport provider to {providerType}: {e.Message}");
                return false;
            }
        }

        private async Task<string> ResolveNetworkJoinCodeAsync(LobbyRoom room, string traceId, NetworkProviderType providerType, CancellationToken ct)
        {
            if (TryNormalizeTransportJoinCode(room?.RelayJoinCode, providerType, out var initialJoinCode))
                return initialJoinCode;

            async Task<string> ResolveAttemptAsync()
            {
                var current = _lobbyService?.Current;
                if (TryNormalizeTransportJoinCode(current?.RelayJoinCode, providerType, out var currentJoinCode))
                    return currentJoinCode;

                try
                {
                    var rooms = await _lobbyService.QueryRoomsAsync(ct);
                    if (rooms == null)
                        return null;

                    foreach (var candidate in rooms)
                    {
                        if (candidate == null)
                            continue;

                        var sameLobby = string.Equals(candidate.LobbyId, room?.LobbyId, StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(candidate.LobbyCode, room?.LobbyCode, StringComparison.OrdinalIgnoreCase);
                        if (sameLobby && TryNormalizeTransportJoinCode(candidate.RelayJoinCode, providerType, out var candidateJoinCode))
                        {
                            Debug.Log($"[JoinRoomPanelService] [{traceId}] ResolveNetworkJoinCodeAsync found relay code in query list for lobby '{candidate.LobbyId}'.");
                            return candidateJoinCode;
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[JoinRoomPanelService] [{traceId}] ResolveNetworkJoinCodeAsync query failed: {e.Message}");
                }

                return null;
            }

            var resolved = await MultiplayerReliabilityPolicy.RetryWithBackoffAndJitterAsync(
                async token => await ResolveAttemptAsync(),
                value => !string.IsNullOrWhiteSpace(value),
                maxAttempts: 3,
                baseDelay: _menuProfile.JoinCodePollInterval,
                operationName: "ResolveNetworkJoinCodeAsync",
                ct: ct);

            if (!string.IsNullOrWhiteSpace(resolved))
                return resolved;

            Debug.LogWarning($"[JoinRoomPanelService] [{traceId}] ResolveNetworkJoinCodeAsync timed out after {_menuProfile.JoinCodeResolveTimeout.TotalSeconds:0.#}s for lobbyId='{room?.LobbyId}' lobbyCode='{room?.LobbyCode}' provider='{providerType}'.");

            if (providerType == NetworkProviderType.Relay || providerType == NetworkProviderType.Lan)
                return null;

            return !string.IsNullOrWhiteSpace(room?.RelayJoinCode)
                ? room.RelayJoinCode.Trim()
                : (!string.IsNullOrWhiteSpace(room?.LobbyCode) ? room.LobbyCode : room?.LobbyId);
        }

        private static bool TryNormalizeTransportJoinCode(string value, NetworkProviderType providerType, out string normalizedJoinCode)
        {
            normalizedJoinCode = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
            if (string.IsNullOrEmpty(normalizedJoinCode))
                return false;

            return providerType != NetworkProviderType.Relay || RelayJoinCodeUtility.IsValid(normalizedJoinCode);
        }

        private async Task LeaveLobbyAfterFailedTransportJoinAsync(string traceId, CancellationToken ct)
        {
            try
            {
                if (_lobbyService != null)
                    await _lobbyService.LeaveAsync(ct);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[JoinRoomPanelService] [{traceId}] Leave after failed transport join failed: {e.Message}");
            }
        }
    }
}
