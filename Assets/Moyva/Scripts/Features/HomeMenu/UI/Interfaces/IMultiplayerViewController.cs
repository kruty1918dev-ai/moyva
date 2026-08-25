using System;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
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
