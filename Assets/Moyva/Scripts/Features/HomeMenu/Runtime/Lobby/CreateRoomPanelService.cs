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
        [InjectOptional] private ILobbyFlowContext _lobbyFlowContext;
        [Inject(Optional = true)] private ILobbyPanelViewController _lobbyPanelViewController;
        [InjectOptional] private ILocalGameSettingsService _localGameSettings;
        [InjectOptional] private INetworkProvider _networkProvider;
        [InjectOptional] private IInfoPanelService _infoPanelService;
        [InjectOptional] private IOverlayLoader _overlayLoader;
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
            Refresh();
            if (_viewController.NextButton != null && !_viewController.NextButton.interactable)
                return;

            if (_lobbyService == null)
            {
                UnityEngine.Debug.LogError("CreateRoomPanelService: ILobbyService not available, cannot create room.");
                return;
            }

            _isCreating = true;
            if (_viewController.NextButton != null)
                _viewController.NextButton.interactable = false;

            try { _overlayLoader?.LoadOverlay(0f, 100f, "%"); } catch { }

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
                        try { _lobbyPanelViewController?.SetInviteCode(LobbyInviteCodeResolver.Resolve(room, GetCurrentProviderType())); } catch { }
                        try { _navigation.Open(_worldSetupPanelName); } catch { }
                    });
                }
                else
                {
                    await FailRoomCreationAsync("Could not create lobby: service returned an empty result.", leaveLobby: false, stopTransport: true);
                }
            }
            catch (Exception ex)
            {
                if (transportHostStarted)
                    await FailRoomCreationAsync($"Could not create lobby: {ex.Message}", leaveLobby: false, stopTransport: true);
                UnityEngine.Debug.LogError($"CreateRoomPanelService: failed to create room: {ex.Message}");
                UnityEngine.Debug.LogException(ex);
            }
            finally
            {
                _isCreating = false;
                // Приховуємо loading-overlay (якщо був показаний)
                try { _overlayLoader?.StopOverlay(true); } catch { }
                // Refresh UI state (re-enable button state if needed) on main thread
                MainThreadDispatcher.Enqueue(() => Refresh());
            }
        }

        private Task ApplySelectedProviderAsync()
        {
            if (_modeSelector == null)
                return Task.CompletedTask;

            return _modeSelector.SetModeAsync(GetCurrentProviderType());
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
                return string.Empty;
            }

            var effectiveNetworkType = GetEffectiveNetworkProviderType();
            if (providerType == NetworkProviderType.Relay && effectiveNetworkType != NetworkProviderType.Relay)
            {
                var error = $"Global Relay transport is unavailable: the active network provider is {effectiveNetworkType}.";
                await FailRoomCreationAsync(error, leaveLobby: false, stopTransport: false);
                return null;
            }

            var transportSessionId = BuildTransportHostSessionId(providerType, roomName);
            var result = await _networkProvider.HostSessionAsync(transportSessionId);
            if (result == null || !result.Success)
            {
                var error = result?.ErrorMessage ?? "Could not start the network session.";
                await FailRoomCreationAsync(error, leaveLobby: false, stopTransport: false);
                return null;
            }

            var transportJoinCode = result.SessionId?.Trim() ?? string.Empty;
            if (providerType == NetworkProviderType.Relay && !RelayJoinCodeUtility.IsValid(transportJoinCode))
            {
                var error = $"Relay returned an invalid join code '{transportJoinCode}'. Expected a short Relay join code; LobbyId is not passed through the transport host flow.";
                await FailRoomCreationAsync(error, leaveLobby: false, stopTransport: true);
                return null;
            }

            if (providerType == NetworkProviderType.Lan && !IsLanJoinCode(transportJoinCode))
            {
                var error = $"LAN transport returned an invalid join code '{transportJoinCode}'. Expected lan:<ip>:<port>.";
                await FailRoomCreationAsync(error, leaveLobby: false, stopTransport: true);
                return null;
            }

            return transportJoinCode;
        }

        private static bool IsLanJoinCode(string joinCode)
        {
            if (string.IsNullOrWhiteSpace(joinCode))
                return false;

            var parts = joinCode.Trim().Split(':');
            return parts.Length >= 3 &&
                   string.Equals(parts[0], "lan", StringComparison.OrdinalIgnoreCase) &&
                   !string.IsNullOrWhiteSpace(parts[1]) &&
                   int.TryParse(parts[2], out var port) &&
                   port > 0 &&
                   port <= 65535;
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
                    await FailRoomCreationAsync($"Could not publish the room network code: {e.Message}", leaveLobby: true, stopTransport: true);
                    return false;
                }
            }

            return true;
        }

        private NetworkProviderType GetCurrentProviderType()
        {
            if (_lobbyFlowContext != null && _lobbyFlowContext.FlowKind == LobbyFlowKind.Create)
                return _lobbyFlowContext.Provider;

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
                catch (Exception) { }
            }

            if (stopTransport)
            {
                try { if (_networkProvider != null) await _networkProvider.LeaveSessionAsync(); }
                catch (Exception) { }
            }

            _infoPanelService?.Show(new InfoMessage("Room Error", error));
        }

        private string GetPlayerName()
        {
            return string.IsNullOrWhiteSpace(_localGameSettings?.PlayerName)
                ? "Player"
                : _localGameSettings.PlayerName;
        }
    }
}
