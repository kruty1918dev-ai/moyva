using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Zenject;
using HomeMenuGameMode = Kruty1918.Moyva.HomeMenu.API.GameMode;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Сервіс панелі одиночної гри.
    /// Залежності:
    /// - слухає <see cref="ISoloViewController"/>;
    /// - записує режим у <see cref="ISelectedGameModeService"/>;
    /// - налаштовує <see cref="IWorldCreationPanelService"/>;
    /// - відкриває наступну панель через <see cref="INavigation"/>.
    /// </summary>
    internal sealed class SoloPanelService : ISoloPanelService, IInitializable, IDisposable
    {
        /// <summary>UI-контролер панелі Solo.</summary>
        [Inject] private ISoloViewController _viewController;

        /// <summary>Сервіс збереження вибраного режиму гри.</summary>
        [Inject] private ISelectedGameModeService _selectedGameMode;

        /// <summary>Сервіс налаштування панелі створення світу.</summary>
        [Inject] private IWorldCreationPanelService _worldCreationPanel;

        /// <summary>Навігація меню.</summary>
        [Inject] private INavigation _navigation;

        /// <summary>Назва панелі налаштування світу.</summary>
        [Inject(Id = "WorldSetupPanelName")] private string _worldSetupPanelName;

        /// <summary>
        /// Підписати сервіс на кнопку переходу далі в Solo-панелі.
        /// </summary>
        public void Initialize()
        {
            // 1: Якщо UI-контролер не під'єднаний через DI, сервіс нічого не може обслуговувати.
            if (_viewController == null)
                return;

            // 2: Знімаємо попередню підписку, щоб уникнути дублювання викликів після повторної ініціалізації.
            _viewController.OnButtonNextClicked -= OnNextClicked;

            // 3: Підписуємо актуальний обробник переходу до налаштування світу.
            _viewController.OnButtonNextClicked += OnNextClicked;
        }

        /// <summary>
        /// Відписати обробники від UI під час знищення сервісу.
        /// </summary>
        public void Dispose()
        {
            // 1: Перевіряємо контролер перед відпискою, щоб не падати при частковій ініціалізації.
            if (_viewController != null)

                // 2: Прибираємо підписку, щоб уникнути висячих колбеків у UI після disposal.
                _viewController.OnButtonNextClicked -= OnNextClicked;
        }

        /// <summary>
        /// Обробити натискання кнопки Next у Solo-панелі.
        /// </summary>
        private void OnNextClicked()
        {
            // 1: Фіксуємо, що користувач обрав саме соло-режим.
            _selectedGameMode.SetSelectedGameMode(HomeMenuGameMode.Solo);

            // 2: Переводимо панель налаштування світу в соло-сценарій.
            _worldCreationPanel.SetupMode(WolrdCreationMode.Solo);

            // 3: Відкриваємо екран конфігурації світу як наступний крок flow.
            _navigation.Open(_worldSetupPanelName);
        }
    }
}
