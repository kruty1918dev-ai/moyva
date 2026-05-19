using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime.Services;
using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Універсальна кнопка навігації між панелями HomeMenu.
    /// Залежності: <see cref="INavigation"/>, опційно <see cref="IJoinRoomPanelService"/> та <see cref="MultiplayerMenuModeService"/>.
    /// </summary>
    public class NavigationButton : MonoBehaviour
    {
        /// <summary>Назва панелі, яку потрібно відкрити.</summary>
        [SerializeField] private string _menuToOpen;

        /// <summary>Назва панелі, яку потрібно закрити.</summary>
        [SerializeField] private string _menuToClose;

        /// <summary>True, якщо натискання має відкрити останню панель зі стеку.</summary>
        [SerializeField] private bool _openLast;

        /// <summary>Сервіс навігації меню.</summary>
        private INavigation _navigation;

        /// <summary>Сервіс підготовки join-room flow.</summary>
        private IJoinRoomPanelService _joinRoomPanelService;

        /// <summary>Сервіс синхронізації multiplayer mode перед переходом.</summary>
        private MultiplayerMenuModeService _multiplayerMenuModeService;

        /// <summary>Канонічна назва панелі join room.</summary>
        private string _joinRoomPanelName;

        /// <summary>Кеш компонента Button.</summary>
        private Button _button;

        /// <summary>Гейт проти повторного паралельного відкриття панелей.</summary>
        private bool _isOpening;

        [Inject]
        public void Construct(
            [InjectOptional] INavigation navigation = null,
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
            // 1: Блокуємо повторне натискання та сценарій без навігації.
            if (_isOpening || _navigation == null)
                return;

            // 2: Для кнопки Back/OpenLast виконуємо окремий короткий сценарій.
            if (_openLast)
            {
                _navigation.OpenLast();
                return;
            }

            // 3: Фіксуємо in-flight стан і тимчасово вимикаємо кнопку.
            _isOpening = true;
            var previousInteractable = _button == null || _button.interactable;
            if (_button != null)
                _button.interactable = false;

            try
            {
                // 4: Синхронізуємо multiplayer mode з цільовою панеллю (якщо сервіс доступний).
                if (_multiplayerMenuModeService != null)
                    await _multiplayerMenuModeService.ApplyModeForNavigationAsync(_menuToOpen, _navigation?.CurrentMenu);

                // 5: Для join-room сценарію запускаємо preflight-підготовку.
                if (ShouldPrepareJoinRoom(_menuToOpen))
                {
                    var prepared = await _joinRoomPanelService.PrepareForOpenAsync();
                    if (!prepared)
                    {
                        Debug.LogWarning($"[NavigationButton] Join room panel '{_menuToOpen}' was not opened because room refresh failed or timed out.");
                        return;
                    }
                }

                // 6: Закриваємо попередню панель, якщо вона задана.
                if (!string.IsNullOrWhiteSpace(_menuToClose))
                    _navigation.Close(_menuToClose);

                // 7: Відкриваємо цільову панель.
                if (!string.IsNullOrWhiteSpace(_menuToOpen))
                    _navigation.Open(_menuToOpen);
            }
            finally
            {
                // 8: Відновлюємо кнопку і скидаємо in-flight стан.
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