using System;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    /// <summary>
    /// Контракт головної multiplayer-панелі з вибором create/join.
    /// Залежності: використовується MultiplayerPanelService.
    /// </summary>
    public interface IMultiplayerViewController
    {
        /// <summary>Кнопка відкриття flow створення кімнати.</summary>
        Button ButtonCreateRoom { get; set; }

        /// <summary>Кнопка відкриття flow входу до кімнати.</summary>
        Button ButtonJoinToRoom { get; set; }

        /// <summary>Подія натискання Create Room.</summary>
        event Action OnCreateRoomClicked;

        /// <summary>Подія натискання Join Room.</summary>
        event Action OnJoinRoomClicked;
    }
}