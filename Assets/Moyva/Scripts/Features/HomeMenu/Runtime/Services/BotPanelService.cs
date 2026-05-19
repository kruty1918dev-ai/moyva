using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Сервіс панелі ботів, який синхронізує default settings з UI та фіксує вибір режиму Bot.
    /// Залежності:
    /// - працює з <see cref="IBotViewController"/>;
    /// - читає <see cref="BotDefaultSettings"/>;
    /// - записує режим у <see cref="ISelectedGameModeService"/>.
    /// </summary>
    internal sealed class BotPanelService : IBotPanelService, IInitializable, IDisposable
    {
        /// <summary>UI-контролер панелі ботів.</summary>
        [Inject] private IBotViewController _botViewController;

        /// <summary>Початкові значення налаштувань ботів.</summary>
        [InjectOptional] private BotDefaultSettings _defaultSettings;

        /// <summary>Сервіс збереження вибраного режиму гри.</summary>
        [InjectOptional] private Kruty1918.Moyva.HomeMenu.API.ISelectedGameModeService _selectedGameModeService;

        /// <summary>Ініціалізувати UI панелі ботів початковими значеннями та підписками.</summary>
        public void Initialize()
        {
            // 1: Копіюємо default settings локально, щоб безпечно нормалізувати їх перед передачею в UI.
            var ds = _defaultSettings;

            // 2: Гарантуємо мінімально валідну кількість ботів навіть при порожній конфігурації.
            if (ds.BotCount <= 0) ds.BotCount = 1; // fallback

            // 3: Послідовно переносимо всі значення конфігурації в UI-контролер.
            _botViewController.AllowBotCheating = ds.AllowBotCheating;
            _botViewController.BotCount = ds.BotCount;
            _botViewController.Difficulty = ds.Difficulty;
            _botViewController.Strategy = ds.Strategy;

            // 4: Спершу знімаємо стару підписку, щоб уникнути дублювання обробника.
            _botViewController.OnButtonNextClicked -= OnNext;

            // 5: Підписуємо поточний обробник підтвердження вибору бот-режиму.
            _botViewController.OnButtonNextClicked += OnNext;

            // 6: Просимо контролер оновити відображення після встановлення всіх значень.
            _botViewController.Refresh();
        }

        /// <summary>Відписати сервіс від UI-подій під час завершення життєвого циклу.</summary>
        public void Dispose()
        {
            // 1: Перевіряємо наявність контролера перед відпискою.
            if (_botViewController != null)

                // 2: Прибираємо обробник кнопки, щоб не лишати висячі посилання.
                _botViewController.OnButtonNextClicked -= OnNext;
        }

        /// <summary>Обробити підтвердження бот-режиму користувачем.</summary>
        private void OnNext()
        {
            // 1: Фіксуємо в глобальному HomeMenu state, що користувач обрав режим гри з ботами.
            _selectedGameModeService.SetSelectedGameMode(Kruty1918.Moyva.HomeMenu.API.GameMode.Bot);
        }
    }

}