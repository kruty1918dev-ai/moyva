using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Zenject;
using HomeMenuGameMode = Kruty1918.Moyva.HomeMenu.API.GameMode;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal class SelectedGameModeService : ISelectedGameModeService
    {
        public HomeMenuGameMode SelectedGameMode { get; private set; }

        public event Action<HomeMenuGameMode> OnSelectedGameModeChanged;

        public void SetSelectedGameMode(HomeMenuGameMode gameMode)
        {
            SelectedGameMode = gameMode;
            OnSelectedGameModeChanged?.Invoke(gameMode);
        }
    }

    internal sealed class SoloPanelService : ISoloPanelService, IInitializable, IDisposable
    {
        [Inject] private ISoloViewController _viewController;
        [Inject] private ISelectedGameModeService _selectedGameMode;
        [Inject] private IWorldCreationPanelService _worldCreationPanel;
        [Inject] private INavigation _navigation;
        [Inject(Id = "WorldSetupPanelName")] private string _worldSetupPanelName;

        public void Initialize()
        {
            if (_viewController == null)
                return;

            _viewController.OnButtonNextClicked -= OnNextClicked;
            _viewController.OnButtonNextClicked += OnNextClicked;
        }

        public void Dispose()
        {
            if (_viewController != null)
                _viewController.OnButtonNextClicked -= OnNextClicked;
        }

        private void OnNextClicked()
        {
            _selectedGameMode.SetSelectedGameMode(HomeMenuGameMode.Solo);
            _worldCreationPanel.SetupMode(WolrdCreationMode.Solo);
            _navigation.Open(_worldSetupPanelName);
        }
    }

    internal sealed class MultiplayerPanelService : IMultiplayerPanelService, IInitializable, IDisposable
    {
        [InjectOptional] private List<IMultiplayerViewController> _viewControllers = new List<IMultiplayerViewController>();
        [Inject] private INavigation _navigation;
        [InjectOptional] private IJoinRoomPanelService _joinRoomPanelService;
        [InjectOptional] private Runtime.Services.MultiplayerMenuModeService _multiplayerMenuModeService;
        [Inject(Id = "CreateRoomPanelName")] private string _createRoomPanelName;
        [Inject(Id = "JoinRoomPanelName")] private string _joinRoomPanelName;

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

        private async void OnCreateRoomClicked()
        {
            if (_multiplayerMenuModeService != null)
                await _multiplayerMenuModeService.ApplyModeForNavigationAsync(_createRoomPanelName, _navigation.CurrentMenu);

            _navigation.Open(_createRoomPanelName);
        }

        private async void OnJoinRoomClicked()
        {
            if (_isOpeningJoin)
                return;

            _isOpeningJoin = true;
            try
            {
                if (_multiplayerMenuModeService != null)
                    await _multiplayerMenuModeService.ApplyModeForNavigationAsync(_joinRoomPanelName, _navigation.CurrentMenu);

                if (_joinRoomPanelService != null && !await _joinRoomPanelService.PrepareForOpenAsync())
                    return;

                _navigation.Open(_joinRoomPanelName);
            }
            finally
            {
                _isOpeningJoin = false;
            }
        }
    }
}