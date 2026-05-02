using System;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    public interface ICreateRoomViewController
    {
        string RoomName { get; set; }
        string Password { get; set; }

        bool IsPublic { get; set; }
        int MaxPlayers { get; set; }
        event Action OnButtonNextClicked;
        Button NextButton { get; }
    }
}