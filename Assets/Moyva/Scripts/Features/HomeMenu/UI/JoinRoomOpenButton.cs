using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime.Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public sealed class JoinRoomOpenButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private string _menuToOpen = "JoinRoomPanel";
        [SerializeField] private string _menuToClose;

        private INavigation _navigation;
        private IJoinRoomPanelService _joinRoomPanelService;
        private MultiplayerMenuModeService _multiplayerMenuModeService;
        private bool _isOpening;

        [Inject]
        public void Construct(
            INavigation navigation,
            IJoinRoomPanelService joinRoomPanelService,
            [InjectOptional] MultiplayerMenuModeService multiplayerMenuModeService = null)
        {
            _navigation = navigation;
            _joinRoomPanelService = joinRoomPanelService;
            _multiplayerMenuModeService = multiplayerMenuModeService;
        }

        private void Awake()
        {
            if (_button == null)
                _button = GetComponent<Button>();

            if (_button != null)
            {
                _button.onClick.RemoveListener(OnClicked);
                _button.onClick.AddListener(OnClicked);
            }
            else
                Debug.LogError($"[JoinRoomOpenButton] No Button component found on '{gameObject.name}'.");
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnClicked);
        }

        private async void OnClicked()
        {
            if (_isOpening || _joinRoomPanelService == null)
                return;

            _isOpening = true;
            var previousInteractable = _button == null || _button.interactable;
            if (_button != null)
                _button.interactable = false;

            try
            {
                if (_multiplayerMenuModeService != null)
                    await _multiplayerMenuModeService.ApplyModeForNavigationAsync(_menuToOpen, _navigation?.CurrentMenu);

                var prepared = await _joinRoomPanelService.PrepareForOpenAsync();
                if (!prepared)
                {
                    Debug.LogWarning($"[JoinRoomOpenButton] Join room panel '{_menuToOpen}' was not opened because room refresh failed or timed out.");
                    return;
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
    }
}