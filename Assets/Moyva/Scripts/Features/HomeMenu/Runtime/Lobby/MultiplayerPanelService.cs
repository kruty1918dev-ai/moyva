using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Shared.Common;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal sealed class MultiplayerPanelService : IMultiplayerPanelService, IInitializable, IDisposable
    {
        [InjectOptional] private List<IMultiplayerViewController> _viewControllers = new List<IMultiplayerViewController>();
        [Inject] private INavigation _navigation;
        [InjectOptional] private IJoinRoomPanelService _joinRoomPanelService;
        [InjectOptional] private Runtime.Services.MultiplayerMenuModeService _multiplayerMenuModeService;
        [InjectOptional] private IMultiplayerModeSelector _modeSelector;
        [InjectOptional] private ILobbyFlowContext _lobbyFlowContext;
        [InjectOptional] private ICreateRoomViewController _createRoomViewController;
        [Inject(Id = "CreateRoomPanelName")] private string _createRoomPanelName;
        [Inject(Id = "JoinRoomPanelName")] private string _joinRoomPanelName;

        private bool _isOpeningCreate;
        private bool _isOpeningJoin;

        public void Initialize()
        {
            if (_viewControllers == null)
                return;

            foreach (var viewController in _viewControllers)
            {
                if (viewController == null)
                    continue;

                viewController.OnCreateRoomClicked -= OnCreateRoomClicked;
                viewController.OnCreateRoomClicked += OnCreateRoomClicked;
                viewController.OnJoinRoomClicked -= OnJoinRoomClicked;
                viewController.OnJoinRoomClicked += OnJoinRoomClicked;
            }
        }

        public void Dispose()
        {
            if (_viewControllers == null)
                return;

            foreach (var viewController in _viewControllers)
            {
                if (viewController == null)
                    continue;

                viewController.OnCreateRoomClicked -= OnCreateRoomClicked;
                viewController.OnJoinRoomClicked -= OnJoinRoomClicked;
            }
        }

        private void OnCreateRoomClicked(NetworkProviderType provider)
        {
            if (_isOpeningCreate)
                return;

            _isOpeningCreate = true;
            try
            {
                _lobbyFlowContext?.Set(provider, LobbyFlowKind.Create);
                _createRoomViewController?.ApplyPresentation(BuildCreateRoomPresentation(provider));
                _navigation.Open(_createRoomPanelName);
                ApplyModeInBackground(provider);
            }
            finally
            {
                _isOpeningCreate = false;
            }
        }

        private async void OnJoinRoomClicked(NetworkProviderType provider)
        {
            if (_isOpeningJoin)
                return;

            _isOpeningJoin = true;
            try
            {
                _lobbyFlowContext?.Set(provider, LobbyFlowKind.Join);

                if (_modeSelector != null)
                    await _modeSelector.SetModeAsync(provider);
                else if (_multiplayerMenuModeService != null)
                    await _multiplayerMenuModeService.ApplyModeForNavigationAsync(_joinRoomPanelName, _navigation.CurrentMenu);

                if (_joinRoomPanelService != null && !await _joinRoomPanelService.PrepareForOpenAsync())
                    return;

                MainThreadDispatcher.Enqueue(() => _navigation.Open(_joinRoomPanelName));
            }
            finally
            {
                _isOpeningJoin = false;
            }
        }

        private async void ApplyModeInBackground(NetworkProviderType provider)
        {
            try
            {
                if (_modeSelector != null)
                    await _modeSelector.SetModeAsync(provider);
            }
            catch (Exception e)
            {
                Debug.LogError($"[MultiplayerPanelService] Failed to switch multiplayer mode to {provider}: {e.Message}");
                Debug.LogException(e);
            }
        }

        private static CreateRoomPanelPresentation BuildCreateRoomPresentation(NetworkProviderType provider)
        {
            return provider == NetworkProviderType.Lan
                ? new CreateRoomPanelPresentation("Create LAN Lobby", "LAN Room Settings", "Create LAN Lobby")
                : new CreateRoomPanelPresentation("Create Global Lobby", "Global Room Settings", "Create Global Lobby");
        }
    }
}
