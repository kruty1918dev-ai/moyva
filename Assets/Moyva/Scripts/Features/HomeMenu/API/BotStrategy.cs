namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Стратегія поведінки бота, яку обирає користувач у HomeMenu.
    /// Залежності: споживається BotPanelService, UI ботів і runtime налаштуваннями бота.
    /// </summary>
    public enum BotStrategy
    {
        /// <summary>Бот діє без вираженої тактики, випадково.</summary>
        Random,

        /// <summary>Бот віддає перевагу захисним рішенням.</summary>
        Defensive,

        /// <summary>Бот віддає перевагу агресивним рішенням.</summary>
        Aggressive
    }
}
