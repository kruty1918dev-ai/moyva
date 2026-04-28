using System;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    public interface IMultiplayerViewController
    {
        Button ButtonCreateRoom { get; set; }
        Button ButtonJoinToRoom { get; set; }
        event Action OnCreateRoomClicked;
        event Action OnJoinRoomClicked;
    }
}