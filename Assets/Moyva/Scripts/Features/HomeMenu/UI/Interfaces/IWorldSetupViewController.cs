using System;
using Kruty1918.Moyva.HomeMenu.API;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    public interface IWorldSetupViewController
    {
        string WorldName { get; set; }
        int Seed { get; set; }
        WorldSize Size { get; set; }

        event Action OnButtonNextClicked;
    }
}