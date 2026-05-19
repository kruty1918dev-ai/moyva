using System;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    /// <summary>
    /// Контракт UI-контролера панелі одиночної гри.
    /// Залежності: використовується SoloPanelService.
    /// </summary>
    public interface ISoloViewController
    {
        /// <summary>Подія натискання кнопки переходу далі.</summary>
        event Action OnButtonNextClicked;
    }
}