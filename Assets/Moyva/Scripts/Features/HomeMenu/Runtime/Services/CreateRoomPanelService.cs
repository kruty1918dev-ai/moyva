using System;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal class CreateRoomPanelService : ICreateRoomPanelService, IInitializable, IDisposable
    {
        [Inject] private ICreateRoomViewController _viewController;
        [Inject(Optional = true)] private ILobbyService _lobbyService;
        [InjectOptional] private IMultiplayerModeSelector _modeSelector;
        [Inject(Optional = true)] private ILobbyPanelViewController _lobbyPanelViewController;
        [Inject] private IWorldCreationPanelService _worldCreationPanelService;
        [InjectOptional] private ILocalGameSettingsService _localGameSettings;
        [InjectOptional] private INetworkProvider _networkProvider;
        [InjectOptional] private IInfoPanelService _infoPanelService;
        [Inject] private INavigation _navigation;
        [Inject(Id = "WorldSetupPanelName")] private string _worldSetupPanelName;

        private bool _isCreating;

        public void Dispose()
        {
            if (_viewController != null)
                _viewController.OnButtonNextClicked -= OnBtnNextClicked;
        }

        public void Initialize()
        {
            if (_viewController != null)
            {
                _viewController.OnButtonNextClicked -= OnBtnNextClicked;
                _viewController.OnButtonNextClicked += OnBtnNextClicked;
            }

            // Ensure initial state
            Refresh();
        }

        public void Refresh()
        {
            // If lobby service is not available (not bound yet), disallow creating multiplayer rooms.
            if (_lobbyService == null)
            {
                if (_viewController.NextButton != null)
                    _viewController.NextButton.interactable = false;
                return;
            }

            bool canProceed = !_isCreating && !string.IsNullOrEmpty(_viewController.RoomName) && (_viewController.IsPublic || !string.IsNullOrEmpty(_viewController.Password));
            if (_viewController.NextButton != null)
                _viewController.NextButton.interactable = canProceed;
        }

        private async void OnBtnNextClicked()
        {
            if (_viewController == null) return;
            if (_isCreating) return;

            if (_lobbyService == null)
            {
                UnityEngine.Debug.LogError("CreateRoomPanelService: ILobbyService not available, cannot create room.");
                return;
            }

            _isCreating = true;
            if (_viewController.NextButton != null)
                _viewController.NextButton.interactable = false;

            try
            {
                await ApplySelectedProviderAsync();

                var opts = new Kruty1918.Moyva.Multiplayer.Lobbies.CreateRoomOptions(
                    _viewController.RoomName,
                    _viewController.MaxPlayers,
                    isPrivate: !_viewController.IsPublic,
                    displayName: GetPlayerName(),
                    password: _viewController.Password);

                var room = await _lobbyService.CreateRoomAsync(opts);

                // If created successfully, update UI and navigate on main thread
                if (room != null)
                {
                    var transportReady = await StartNetworkHostAsync(room);
                    if (!transportReady)
                        return;

                    MainThreadDispatcher.Enqueue(() =>
                    {
                        var inviteCode = !string.IsNullOrWhiteSpace(room.LobbyCode) ? room.LobbyCode : room.LobbyId;
                        try { _lobbyPanelViewController?.SetLobbyInvateCode(inviteCode); } catch { }
                        try { _worldCreationPanelService?.SetupMode(WolrdCreationMode.Multiplayer); } catch { }
                        try { _navigation.Open(_worldSetupPanelName); } catch { }
                    });
                }
                else
                {
                    UnityEngine.Debug.LogError("CreateRoomPanelService: failed to create room, no exception but result was null.");
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"CreateRoomPanelService: failed to create room: {ex.Message}");
                UnityEngine.Debug.LogException(ex);
            }
            finally
            {
                _isCreating = false;
                // Refresh UI state (re-enable button state if needed) on main thread
                MainThreadDispatcher.Enqueue(() => Refresh());
            }
        }

        private Task ApplySelectedProviderAsync()
        {
            if (_modeSelector == null)
                return Task.CompletedTask;

            return _modeSelector.SetModeAsync(_modeSelector.CurrentMode);
        }

        /// <summary>
        /// Після створення lobby запускає transport-хост (LAN/Relay/etc.) і публікує join-code назад у lobby.
        /// Без цього кімната може бути видима у списку, але клієнт не матиме реального transport endpoint.
        /// </summary>
        private async Task<bool> StartNetworkHostAsync(LobbyRoom room)
        {
            if (_networkProvider == null)
            {
                UnityEngine.Debug.LogWarning("[CreateRoomPanelService] INetworkProvider not available; lobby created without transport host.");
                return true;
            }

            var sessionId = !string.IsNullOrWhiteSpace(room.LobbyId) ? room.LobbyId : room.LobbyCode;
            var providerType = GetCurrentProviderType();
            var effectiveNetworkType = GetEffectiveNetworkProviderType();
            if (providerType == NetworkProviderType.Relay && effectiveNetworkType != NetworkProviderType.Relay)
            {
                var error = $"Глобальний Relay транспорт недоступний: активний мережевий провайдер зараз {effectiveNetworkType}.";
                await FailCreatedLobbyAsync(error);
                return false;
            }

            var result = await _networkProvider.HostSessionAsync(sessionId);
            if (result == null || !result.Success)
            {
                var error = result?.ErrorMessage ?? "Не вдалося запустити мережеву сесію.";
                await FailCreatedLobbyAsync(error);
                return false;
            }

            var transportJoinCode = result.SessionId?.Trim() ?? string.Empty;
            if (providerType == NetworkProviderType.Relay && !RelayJoinCodeUtility.IsValid(transportJoinCode))
            {
                var error = $"Relay повернув невалідний код підключення '{transportJoinCode}'. Очікувався короткий Relay join code, а не lobby id '{room?.LobbyId}'.";
                await FailCreatedLobbyAsync(error);
                return false;
            }

            if (!string.IsNullOrWhiteSpace(transportJoinCode) && _lobbyService != null)
            {
                try
                {
                    await _lobbyService.SetRelayJoinCodeAsync(transportJoinCode);
                }
                catch (Exception e)
                {
                    await FailCreatedLobbyAsync($"Не вдалося опублікувати мережевий код кімнати: {e.Message}");
                    return false;
                }
            }

            return true;
        }

        private NetworkProviderType GetCurrentProviderType()
        {
            return _modeSelector?.CurrentMode ?? NetworkProviderType.Relay;
        }

        private NetworkProviderType GetEffectiveNetworkProviderType()
        {
            return _networkProvider is SwitchableNetworkProvider switchableNetworkProvider
                ? switchableNetworkProvider.CurrentType
                : GetCurrentProviderType();
        }

        private async Task FailCreatedLobbyAsync(string error)
        {
            UnityEngine.Debug.LogError($"[CreateRoomPanelService] HostSessionAsync failed: {error}");
            try { if (_lobbyService != null) await _lobbyService.LeaveAsync(); }
            catch (Exception leaveError) { UnityEngine.Debug.LogWarning($"[CreateRoomPanelService] Leave after failed host transport failed: {leaveError.Message}"); }
            _infoPanelService?.Show(new InfoMessage("Помилка кімнати", error));
        }

        private string GetPlayerName()
        {
            return string.IsNullOrWhiteSpace(_localGameSettings?.PlayerName)
                ? "Player"
                : _localGameSettings.PlayerName;
        }
    }
}