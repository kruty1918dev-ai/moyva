using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime.Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Кнопка відкриття панелі Join Room з preflight-підготовкою даних кімнат.
    /// Залежності: <see cref="INavigation"/>, <see cref="IJoinRoomPanelService"/>, опційно <see cref="MultiplayerMenuModeService"/>.
    /// </summary>
    public sealed class JoinRoomOpenButton : MonoBehaviour
    {
        /// <summary>Кнопка-джерело натискання.</summary>
        [SerializeField] private Button _button;

        /// <summary>Назва панелі, яку потрібно відкрити.</summary>
        [SerializeField] private string _menuToOpen = "JoinRoomPanel";

        /// <summary>Назва панелі, яку потрібно закрити перед відкриттям нової.</summary>
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
            // 1: Захищаємося від повторного запуску та некоректної конфігурації сервісу.
            if (_isOpening || _joinRoomPanelService == null)
                return;

            // 2: Позначаємо in-flight стан і блокуємо кнопку на час async-операції.
            _isOpening = true;
            var previousInteractable = _button == null || _button.interactable;
            if (_button != null)
                _button.interactable = false;

            try
            {
                // 3: Актуалізуємо multiplayer mode перед навігацією (за наявності сервісу).
                if (_multiplayerMenuModeService != null)
                    await _multiplayerMenuModeService.ApplyModeForNavigationAsync(_menuToOpen, _navigation?.CurrentMenu);

                // 4: Готуємо join-room панель (оновлення списку кімнат, таймаути, ретраї).
                var prepared = await _joinRoomPanelService.PrepareForOpenAsync();
                if (!prepared)
                {
                    Debug.LogWarning($"[JoinRoomOpenButton] Join room panel '{_menuToOpen}' was not opened because room refresh failed or timed out.");
                    return;
                }

                // 5: Закриваємо стару панель, якщо вона вказана.
                if (!string.IsNullOrWhiteSpace(_menuToClose))
                    _navigation.Close(_menuToClose);

                // 6: Відкриваємо цільову панель join room.
                if (!string.IsNullOrWhiteSpace(_menuToOpen))
                    _navigation.Open(_menuToOpen);
            }
            finally
            {
                // 7: Відновлюємо початковий стан кнопки і скидаємо in-flight прапорець.
                if (_button != null)
                    _button.interactable = previousInteractable;
                _isOpening = false;
            }
        }
    }
}