namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Рівень складності ботів у меню створення або налаштування сесії.
    /// Залежності: використовується UI контролерами HomeMenu та сервісами конфігурації ботів.
    /// </summary>
    public enum BotDifficulty
    {
        /// <summary>Найпростіший рівень поведінки бота.</summary>
        Easy,

        /// <summary>Середній рівень поведінки бота.</summary>
        Medium,

        /// <summary>Найскладніший рівень поведінки бота.</summary>
        Hard
    }
}
