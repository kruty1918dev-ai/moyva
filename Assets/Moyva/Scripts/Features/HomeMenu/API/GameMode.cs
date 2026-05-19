namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Режим гри, обраний у HomeMenu.
    /// Залежності: використовується SelectedGameModeService, панелями меню та стартом сесії.
    /// </summary>
    public enum GameMode
    {
        /// <summary>Локальна одиночна гра.</summary>
        Solo,

        /// <summary>Локальна мережа LAN.</summary>
        LAN,

        /// <summary>Онлайн або relay multiplayer.</summary>
        Multiplayer,

        /// <summary>Режим із ботами.</summary>
        Bot
    }
}