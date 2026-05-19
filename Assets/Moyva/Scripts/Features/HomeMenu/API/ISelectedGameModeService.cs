using System;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Контракт стану вибраного режиму гри в меню.
    /// Залежності: використовується панелями Solo/Bot/Multiplayer і сервісами переходів HomeMenu.
    /// </summary>
    public interface ISelectedGameModeService
    {
        /// <summary>Поточний обраний режим гри.</summary>
        GameMode SelectedGameMode { get; }

        /// <summary>Змінити поточний режим гри.</summary>
        void SetSelectedGameMode(GameMode mode);

        /// <summary>Подія зміни обраного режиму гри.</summary>
        event Action<GameMode> OnSelectedGameModeChanged;
    }
}