using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime.Services;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public class NavigationButton : MonoBehaviour
    {
        [SerializeField] private string _menuToOpen;
        [SerializeField] private string _menuToClose;
        [SerializeField] private bool _openLast;

        private INavigation _navigation;
        private IJoinRoomPanelService _joinRoomPanelService;
        private MultiplayerMenuModeService _multiplayerMenuModeService;
        private string _joinRoomPanelName;
        private Button _button;
        private bool _isOpening;

        [Inject]
        public void Construct(
            INavigation navigation,
            [InjectOptional] IJoinRoomPanelService joinRoomPanelService = null,
            [InjectOptional] MultiplayerMenuModeService multiplayerMenuModeService = null,
            [Inject(Id = "JoinRoomPanelName", Optional = true)] string joinRoomPanelName = null)
        {
            _navigation = navigation;
            _joinRoomPanelService = joinRoomPanelService;
            _multiplayerMenuModeService = multiplayerMenuModeService;
            _joinRoomPanelName = joinRoomPanelName;
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
                _button.onClick.AddListener(OnButtonClicked);
            }
            else
            {
                Debug.LogError($"[NavigationButton] No Button component found on '{gameObject.name}'. NavigationButton will not work.");
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnButtonClicked);
        }

        private async void OnButtonClicked()
        {
            if (_isOpening)
                return;

            if (_openLast)
            {
                _navigation.OpenLast();
                return;
            }

            _isOpening = true;
            var previousInteractable = _button == null || _button.interactable;
            if (_button != null)
                _button.interactable = false;

            try
            {
                if (_multiplayerMenuModeService != null)
                    await _multiplayerMenuModeService.ApplyModeForNavigationAsync(_menuToOpen, _navigation?.CurrentMenu);

                if (ShouldPrepareJoinRoom(_menuToOpen))
                {
                    var prepared = await _joinRoomPanelService.PrepareForOpenAsync();
                    if (!prepared)
                    {
                        Debug.LogWarning($"[NavigationButton] Join room panel '{_menuToOpen}' was not opened because room refresh failed or timed out.");
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(_menuToClose))
                    _navigation.Close(_menuToClose);

                if (!string.IsNullOrWhiteSpace(_menuToOpen))
                    _navigation.Open(_menuToOpen);
            }
            finally
            {
                if (_button != null)
                    _button.interactable = previousInteractable;
                _isOpening = false;
            }
        }

        private bool ShouldPrepareJoinRoom(string menuName)
        {
            if (_joinRoomPanelService == null || string.IsNullOrWhiteSpace(menuName))
                return false;

            if (!string.IsNullOrWhiteSpace(_joinRoomPanelName) && string.Equals(menuName, _joinRoomPanelName, StringComparison.Ordinal))
                return true;

            return menuName.IndexOf("JoinRoom", StringComparison.OrdinalIgnoreCase) >= 0
                || menuName.IndexOf("Join Room", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}