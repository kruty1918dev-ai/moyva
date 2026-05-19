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

            var transportHostStarted = false;
            try
            {
                await ApplySelectedProviderAsync();

                var transportJoinCode = await StartNetworkHostAsync(_viewController.RoomName);
                if (transportJoinCode == null)
                    return;
                transportHostStarted = _networkProvider != null;

                var opts = new Kruty1918.Moyva.Multiplayer.Lobbies.CreateRoomOptions(
                    _viewController.RoomName,
                    _viewController.MaxPlayers,
                    isPrivate: !_viewController.IsPublic,
                    displayName: GetPlayerName(),
                    password: _viewController.Password,
                    relayJoinCode: transportJoinCode);

                var room = await _lobbyService.CreateRoomAsync(opts);

                // If created successfully, update UI and navigate on main thread
                if (room != null)
                {
                    if (!await PublishTransportJoinCodeAsync(transportJoinCode))
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
                    await FailRoomCreationAsync("Не вдалося створити lobby: сервіс повернув порожній результат.", leaveLobby: false, stopTransport: true);
                }
            }
            catch (Exception ex)
            {
                if (transportHostStarted)
                    await FailRoomCreationAsync($"Не вдалося створити lobby: {ex.Message}", leaveLobby: false, stopTransport: true);
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
        /// Запускає transport-хост (LAN/Relay/etc.) до створення lobby, щоб lobby одразу містила реальний transport join-code.
        /// </summary>
        private async Task<string> StartNetworkHostAsync(string roomName)
        {
            var providerType = GetCurrentProviderType();
            if (_networkProvider == null)
            {
                var warning = "INetworkProvider not available; lobby created without transport host.";
                if (providerType == NetworkProviderType.Relay)
                {
                    await FailRoomCreationAsync(warning, leaveLobby: false, stopTransport: false);
                    return null;
                }

                UnityEngine.Debug.LogWarning($"[CreateRoomPanelService] {warning}");
                return string.Empty;
            }

            var effectiveNetworkType = GetEffectiveNetworkProviderType();
            if (providerType == NetworkProviderType.Relay && effectiveNetworkType != NetworkProviderType.Relay)
            {
                var error = $"Глобальний Relay транспорт недоступний: активний мережевий провайдер зараз {effectiveNetworkType}.";
                await FailRoomCreationAsync(error, leaveLobby: false, stopTransport: false);
                return null;
            }

            var transportSessionId = BuildTransportHostSessionId(providerType, roomName);
            var result = await _networkProvider.HostSessionAsync(transportSessionId);
            if (result == null || !result.Success)
            {
                var error = result?.ErrorMessage ?? "Не вдалося запустити мережеву сесію.";
                await FailRoomCreationAsync(error, leaveLobby: false, stopTransport: false);
                return null;
            }

            var transportJoinCode = result.SessionId?.Trim() ?? string.Empty;
            if (providerType == NetworkProviderType.Relay && !RelayJoinCodeUtility.IsValid(transportJoinCode))
            {
                var error = $"Relay повернув невалідний код підключення '{transportJoinCode}'. Очікувався короткий Relay join code; LobbyId не передається у transport host-flow.";
                await FailRoomCreationAsync(error, leaveLobby: false, stopTransport: true);
                return null;
            }

            return transportJoinCode;
        }

        private async Task<bool> PublishTransportJoinCodeAsync(string transportJoinCode)
        {
            if (!string.IsNullOrWhiteSpace(transportJoinCode) && _lobbyService != null)
            {
                try
                {
                    await _lobbyService.SetRelayJoinCodeAsync(transportJoinCode);
                }
                catch (Exception e)
                {
                    await FailRoomCreationAsync($"Не вдалося опублікувати мережевий код кімнати: {e.Message}", leaveLobby: true, stopTransport: true);
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
                : _networkProvider is RelayNetworkProvider ? NetworkProviderType.Relay
                : _networkProvider is LanNetworkProvider ? NetworkProviderType.Lan
                : _networkProvider is WebSocketNetworkProvider ? NetworkProviderType.WebSocket
                : _networkProvider is OfflineNetworkProvider ? NetworkProviderType.Offline
                : GetCurrentProviderType();
        }

        private static string BuildTransportHostSessionId(NetworkProviderType providerType, string roomName)
        {
            if (providerType == NetworkProviderType.Relay)
                return string.Empty;

            return string.IsNullOrWhiteSpace(roomName)
                ? Guid.NewGuid().ToString("N")
                : roomName.Trim();
        }

        private async Task FailRoomCreationAsync(string error, bool leaveLobby, bool stopTransport)
        {
            UnityEngine.Debug.LogError($"[CreateRoomPanelService] Room creation failed: {error}");
            if (leaveLobby)
            {
                try { if (_lobbyService != null) await _lobbyService.LeaveAsync(); }
                catch (Exception leaveError) { UnityEngine.Debug.LogWarning($"[CreateRoomPanelService] Leave after failed room creation failed: {leaveError.Message}"); }
            }

            if (stopTransport)
            {
                try { if (_networkProvider != null) await _networkProvider.LeaveSessionAsync(); }
                catch (Exception leaveError) { UnityEngine.Debug.LogWarning($"[CreateRoomPanelService] Transport cleanup after failed room creation failed: {leaveError.Message}"); }
            }

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