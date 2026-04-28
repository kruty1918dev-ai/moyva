using System;
using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    public interface IJoinRoomViewController
    {
        string JoinCode { get; set; }
        Button JoinToRoomButton { get; set; }
        void AddRoomToList(RoomInfo room);
        void ClearRoomList();
        void RefreshRoomList();
        event Action OnJoinCodeChanged; 
        event Action OnListRoomsRefresh;
    }
}