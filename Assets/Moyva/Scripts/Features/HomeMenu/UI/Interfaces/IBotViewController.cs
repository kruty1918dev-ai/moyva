using System;
using Kruty1918.Moyva.HomeMenu.API;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    public interface IBotViewController
    {
        BotDifficulty Difficulty { get; set; }
        BotStrategy Strategy { get; set; }
        int BotCount { get; set; }
        bool AllowBotCheating { get; set; }

        event Action OnButtonNextClicked;

        void Refresh();
    }
}