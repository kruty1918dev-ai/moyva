using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal class CreateRoomPanelService : ICreateRoomPanelService, IInitializable, IDisposable
    {
        [Inject] private ICreateRoomViewController _viewController;
        [Inject(Optional = true)] private ILobbyService _lobbyService;
        [InjectOptional] private IMultiplayerModeSelector _modeSelector;
        [InjectOptional] private ILobbyFlowContext _lobbyFlowContext;
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

            bool canProceed = !_isCreating
                && !string.IsNullOrWhiteSpace(_viewController.RoomName)
                && Mathf.Clamp(_viewController.MaxPlayers, 2, 8) >= 2
                && (_viewController.IsPublic || !string.IsNullOrWhiteSpace(_viewController.Password));
            if (_viewController.NextButton != null)
                _viewController.NextButton.interactable = canProceed;
        }

        private void OnBtnNextClicked()
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

            try
            {
                _isCreating = true;
                _viewController.MaxPlayers = Mathf.Clamp(_viewController.MaxPlayers, 2, 8);
                _lobbyFlowContext?.SetRoomDraft(
                    _viewController.RoomName,
                    _viewController.MaxPlayers,
                    _viewController.IsPublic,
                    _viewController.Password);
                ApplySelectedProviderInBackground();
                _navigation.Open(_worldSetupPanelName);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"CreateRoomPanelService: failed to continue to world setup: {ex.Message}");
                UnityEngine.Debug.LogException(ex);
                _infoPanelService?.Show(new InfoMessage("Room Error", ex.Message));
            }
            finally
            {
                _isCreating = false;
                Refresh();
            }
        }

        private async void ApplySelectedProviderInBackground()
        {
            if (_modeSelector == null)
                return;

            try
            {
                await _modeSelector.SetModeAsync(GetCurrentProviderType());
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"CreateRoomPanelService: failed to switch multiplayer mode: {e.Message}");
                UnityEngine.Debug.LogException(e);
            }
        }

        private NetworkProviderType GetCurrentProviderType()
        {
            if (_lobbyFlowContext != null && _lobbyFlowContext.FlowKind == LobbyFlowKind.Create)
                return _lobbyFlowContext.Provider;

            return _modeSelector?.CurrentMode ?? NetworkProviderType.Relay;
        }
    }
}
