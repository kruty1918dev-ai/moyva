using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Runtime;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal sealed class KickPlayerPanelService : IKickPlayerPanelService, IInitializable, IDisposable
    {
        [InjectOptional] private IKickPlayerPanelViewController _viewController;
        [Inject] private ILobbyService _lobbyService;
        [Inject] private INavigation _navigation;
        [Inject(Id = "LobbyPanelName")] private string _lobbyPanelName;
        [InjectOptional] private ISessionManager _sessionManager;
        [InjectOptional] private IMultiplayerIdentityService _identityService;
        [InjectOptional] private ILocalGameSettingsService _localGameSettings;
        [InjectOptional] private IConfirmationService _confirmationService;

        private LobbyRoom _currentLobby;
        private string _localPlayerId = string.Empty;
        private bool _isResolvingLocalIdentity;
        private bool _isKicking;

        public void Initialize()
        {
            if (_viewController == null)
                return;

            _viewController.OnCloseRequested -= OnCloseRequested;
            _viewController.OnCloseRequested += OnCloseRequested;
            _viewController.OnRefreshRequested -= Refresh;
            _viewController.OnRefreshRequested += Refresh;
            _viewController.OnKickRequested -= OnKickRequested;
            _viewController.OnKickRequested += OnKickRequested;

            _lobbyService.LobbyUpdated -= OnLobbyUpdated;
            _lobbyService.LobbyUpdated += OnLobbyUpdated;
            _lobbyService.KickedFromLobby -= OnKickedFromLobby;
            _lobbyService.KickedFromLobby += OnKickedFromLobby;

            RequestLocalIdentityResolve();
            Refresh();
        }

        public void Dispose()
        {
            if (_viewController != null)
            {
                _viewController.OnCloseRequested -= OnCloseRequested;
                _viewController.OnRefreshRequested -= Refresh;
                _viewController.OnKickRequested -= OnKickRequested;
            }

            if (_lobbyService != null)
            {
                _lobbyService.LobbyUpdated -= OnLobbyUpdated;
                _lobbyService.KickedFromLobby -= OnKickedFromLobby;
            }
        }

        public void Refresh()
        {
            _currentLobby = _lobbyService.Current;
            UpdateView(_currentLobby);
        }

        private void OnLobbyUpdated(LobbyRoom lobby)
        {
            _currentLobby = lobby;
            if (string.IsNullOrWhiteSpace(_localPlayerId) || !ContainsPlayerId(lobby, _localPlayerId))
                RequestLocalIdentityResolve();

            UpdateView(lobby);
        }

        private void OnKickedFromLobby(string reason)
        {
            _currentLobby = null;
            _viewController?.ClearPlayers();
            _viewController?.SetStatus("Тебе видалили з лобі.");
        }

        private void OnCloseRequested()
        {
            if (string.IsNullOrWhiteSpace(_lobbyPanelName))
                return;

            _navigation.Open(_lobbyPanelName);
        }

        private void OnKickRequested(KickPlayerInfo playerInfo)
        {
            if (_isKicking)
                return;

            if (!playerInfo.CanKick)
            {
                _viewController?.SetStatus("Цього гравця не можна кікнути.");
                return;
            }

            if (_confirmationService == null)
            {
                _ = KickPlayerAsync(playerInfo);
                return;
            }

            _confirmationService.Show(new ConfirmationRequest
            {
                LabelText = "Кікнути гравця",
                MessageText = $"Видалити {playerInfo.DisplayName} з кімнати?",
                OnConfirm = () => _ = KickPlayerAsync(playerInfo),
                OnCancel = () => { }
            });
        }

        private async Task KickPlayerAsync(KickPlayerInfo playerInfo)
        {
            if (_isKicking)
                return;

            _isKicking = true;
            _viewController?.SetInteractable(false);
            _viewController?.SetStatus("Видаляю гравця...");

            try
            {
                await _lobbyService.KickAsync(playerInfo.PlayerId);
                Refresh();
            }
            catch (Exception exception)
            {
                _viewController?.SetStatus($"Не вдалося кікнути: {exception.Message}");
            }
            finally
            {
                _isKicking = false;
                _viewController?.SetInteractable(true);
            }
        }

        private void UpdateView(LobbyRoom lobby)
        {
            if (_viewController == null)
                return;

            var canManageLobby = IsLocalPlayerHost(lobby);
            var players = BuildPlayerInfos(lobby, canManageLobby);

            _viewController.SetPlayers(players);
            _viewController.SetInteractable(!_isKicking);
            _viewController.SetStatus(BuildStatusText(lobby, canManageLobby, players));
        }

        private List<KickPlayerInfo> BuildPlayerInfos(LobbyRoom lobby, bool canManageLobby)
        {
            var result = new List<KickPlayerInfo>();
            if (lobby?.Players == null)
                return result;

            var localPlayerId = ResolveLocalPlayerId(lobby);
            foreach (var player in lobby.Players)
            {
                if (player == null)
                    continue;

                var isLocalPlayer = !string.IsNullOrWhiteSpace(localPlayerId) &&
                                    string.Equals(player.PlayerId, localPlayerId, StringComparison.Ordinal);
                var isHost = player.IsHost ||
                             string.Equals(player.PlayerId, lobby.HostPlayerId, StringComparison.Ordinal);
                var canKick = canManageLobby && !isHost && !isLocalPlayer && !string.IsNullOrWhiteSpace(player.PlayerId);

                result.Add(new KickPlayerInfo
                {
                    PlayerId = player.PlayerId,
                    DisplayName = string.IsNullOrWhiteSpace(player.DisplayName) ? "Гравець" : player.DisplayName,
                    IsHost = isHost,
                    IsLocalPlayer = isLocalPlayer,
                    CanKick = canKick,
                    StatusLabel = BuildPlayerStatus(isHost, isLocalPlayer, canKick)
                });
            }

            return result;
        }

        private bool IsLocalPlayerHost(LobbyRoom lobby)
        {
            if (lobby == null)
                return false;

            if (_sessionManager != null && _sessionManager.IsLocalPlayerHost)
                return true;

            var localPlayerId = ResolveLocalPlayerId(lobby);
            return !string.IsNullOrWhiteSpace(localPlayerId) &&
                   string.Equals(lobby.HostPlayerId, localPlayerId, StringComparison.Ordinal);
        }

        private string ResolveLocalPlayerId(LobbyRoom lobby)
        {
            foreach (var candidate in GetLocalPlayerIdCandidates(lobby))
            {
                if (!string.IsNullOrWhiteSpace(candidate) && ContainsPlayerId(lobby, candidate))
                    return candidate;
            }

            foreach (var candidate in GetLocalPlayerIdCandidates(lobby))
            {
                if (!string.IsNullOrWhiteSpace(candidate))
                    return candidate;
            }

            if (_sessionManager != null && _sessionManager.IsLocalPlayerHost)
                return lobby?.HostPlayerId ?? string.Empty;

            return string.Empty;
        }

        private void RequestLocalIdentityResolve()
        {
            if (_identityService == null || _isResolvingLocalIdentity)
                return;

            MainThreadDispatcher.Enqueue(() => _ = ResolveLocalIdentityAsync());
        }

        private async Task ResolveLocalIdentityAsync()
        {
            if (_identityService == null || _isResolvingLocalIdentity)
                return;

            _isResolvingLocalIdentity = true;
            try
            {
                var identity = await _identityService.ResolveAsync(GetPlayerName());
                if (identity == null || string.IsNullOrWhiteSpace(identity.PlayerId))
                    return;

                _localPlayerId = identity.PlayerId;
                MainThreadDispatcher.Enqueue(() => UpdateView(_lobbyService?.Current));
            }
            catch (Exception)
            {
                // The synchronous AuthenticationService fallback below still covers the UGS lobby path.
            }
            finally
            {
                _isResolvingLocalIdentity = false;
            }
        }

        private IEnumerable<string> GetLocalPlayerIdCandidates(LobbyRoom lobby)
        {
            if (!string.IsNullOrWhiteSpace(_localPlayerId))
                yield return _localPlayerId;

            var authenticatedPlayerId = TryGetAuthenticatedPlayerId();
            if (!string.IsNullOrWhiteSpace(authenticatedPlayerId))
                yield return authenticatedPlayerId;

            if (!string.IsNullOrWhiteSpace(_sessionManager?.LocalPlayerId))
                yield return _sessionManager.LocalPlayerId;

            if (_sessionManager != null && _sessionManager.IsLocalPlayerHost && !string.IsNullOrWhiteSpace(lobby?.HostPlayerId))
                yield return lobby.HostPlayerId;
        }

        private static bool ContainsPlayerId(LobbyRoom lobby, string playerId)
        {
            if (lobby?.Players == null || string.IsNullOrWhiteSpace(playerId))
                return false;

            foreach (var player in lobby.Players)
            {
                if (player != null && string.Equals(player.PlayerId, playerId, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private static string TryGetAuthenticatedPlayerId()
        {
            try
            {
                if (UnityServices.State == ServicesInitializationState.Initialized &&
                    AuthenticationService.Instance != null &&
                    AuthenticationService.Instance.IsSignedIn)
                {
                    return AuthenticationService.Instance.PlayerId ?? string.Empty;
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }

            return string.Empty;
        }

        private string GetPlayerName()
        {
            return string.IsNullOrWhiteSpace(_localGameSettings?.PlayerName)
                ? "Player"
                : _localGameSettings.PlayerName;
        }

        private static string BuildStatusText(LobbyRoom lobby, bool canManageLobby, List<KickPlayerInfo> players)
        {
            if (lobby == null)
                return "Лобі ще не створено.";

            if (!canManageLobby)
                return "Тільки хост може кікати гравців.";

            foreach (var player in players)
            {
                if (player.CanKick)
                    return "Натисни на гравця, щоб кікнути його з кімнати.";
            }

            return "Немає гравців, яких можна кікнути.";
        }

        private static string BuildPlayerStatus(bool isHost, bool isLocalPlayer, bool canKick)
        {
            if (isHost)
                return "Хост";

            if (isLocalPlayer)
                return "Це ти";

            return canKick ? "Можна кікнути" : "Недоступно";
        }
    }
}