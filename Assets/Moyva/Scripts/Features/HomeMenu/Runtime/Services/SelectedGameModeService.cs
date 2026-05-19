using System;
using Kruty1918.Moyva.HomeMenu.API;
using HomeMenuGameMode = Kruty1918.Moyva.HomeMenu.API.GameMode;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Сервіс зберігає поточний обраний режим гри в HomeMenu.
    /// Залежності:
    /// - реалізує <see cref="ISelectedGameModeService"/> як централізоване сховище стану;
    /// - повідомляє UI та runtime-сервіси через <see cref="OnSelectedGameModeChanged"/>.
    /// </summary>
    internal class SelectedGameModeService : ISelectedGameModeService
    {
        /// <summary>
        /// Поточний вибраний режим гри.
        /// </summary>
        public HomeMenuGameMode SelectedGameMode { get; private set; }

        /// <summary>
        /// Подія, яка викликається після зміни вибраного режиму.
        /// Залежності: на неї підписуються UI-контролери та сервіси переходів між панелями.
        /// </summary>
        public event Action<HomeMenuGameMode> OnSelectedGameModeChanged;

        /// <summary>
        /// Змінити вибраний режим гри й повідомити всіх підписників.
        /// </summary>
        /// <param name="gameMode">Новий режим гри, який треба зберегти.</param>
        public void SetSelectedGameMode(HomeMenuGameMode gameMode)
        {
            // 1: Зберігаємо нове значення, щоб усі наступні читачі сервісу бачили актуальний режим.
            SelectedGameMode = gameMode;

            // 2: Одразу розсилаємо подію, щоб UI і workflow-сервіси синхронізувалися з новим режимом.
            OnSelectedGameModeChanged?.Invoke(gameMode);
        }
    }
}