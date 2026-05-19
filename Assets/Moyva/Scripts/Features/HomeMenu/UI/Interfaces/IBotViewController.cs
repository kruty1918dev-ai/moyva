using System;
using Kruty1918.Moyva.HomeMenu.API;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    /// <summary>
    /// Контракт UI-контролера панелі ботів.
    /// Залежності: використовується BotPanelService для синхронізації налаштувань ботів.
    /// </summary>
    public interface IBotViewController
    {
        /// <summary>Обрана складність ботів.</summary>
        BotDifficulty Difficulty { get; set; }

        /// <summary>Обрана стратегія поведінки ботів.</summary>
        BotStrategy Strategy { get; set; }

        /// <summary>Кількість ботів у сесії.</summary>
        int BotCount { get; set; }

        /// <summary>Прапорець дозволу читерської поведінки ботів.</summary>
        bool AllowBotCheating { get; set; }

        /// <summary>Подія натискання кнопки переходу далі.</summary>
        event Action OnButtonNextClicked;

        /// <summary>Оновити UI за поточним станом.</summary>
        void Refresh();
    }
}