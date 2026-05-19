using Kruty1918.Moyva.HomeMenu.API;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Runtime DTO зі значеннями ботів за замовчуванням.
    /// Залежності: використовується BotPanelService для первинного заповнення UI.
    /// </summary>
    internal struct BotDefaultSettings
    {
        /// <summary>Складність бота за замовчуванням.</summary>
        public BotDifficulty Difficulty;

        /// <summary>Стратегія бота за замовчуванням.</summary>
        public BotStrategy Strategy;

        /// <summary>Кількість ботів за замовчуванням.</summary>
        public int BotCount;

        /// <summary>Чи дозволено ботам шахраювати за замовчуванням.</summary>
        public bool AllowBotCheating;
    }
}