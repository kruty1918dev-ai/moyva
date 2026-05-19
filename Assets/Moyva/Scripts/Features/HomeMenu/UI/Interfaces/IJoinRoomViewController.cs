using System;
using Kruty1918.Moyva.HomeMenu.API;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    // TODO: Implement view controllers for each menu panel (Solo, Multiplayer, CreateRoom, JoinRoom, Continue, WorldSetup) following the pattern of IBotViewController and BotViewController.
    /// <summary>
    /// Контракт UI-контролера панелі входу до кімнати.
    /// Залежності: використовується JoinRoomPanelService.
    /// </summary>
    public interface IJoinRoomViewController
    {
        /// <summary>Ручний join code/lobby id, який ввів користувач.</summary>
        string JoinCode { get; set; }

        /// <summary>Додати кімнату в список відображення.</summary>
        void AddRoomToList(RoomInfo room);

        /// <summary>Очистити список кімнат.</summary>
        void ClearRoomList();

        /// <summary>Оновити візуальне відображення списку кімнат.</summary>
        void RefreshRoomList();

        /// <summary>Увімкнути/вимкнути взаємодію з кнопкою join.</summary>
        void SetJoinInteractable(bool interactable);

        /// <summary>Подія запиту на join за поточним вводом.</summary>
        event Action OnJoinRequested;

        /// <summary>Подія зміни тексту join code/lobby id.</summary>
        event Action OnJoinCodeChanged;

        /// <summary>Подія запиту оновлення списку кімнат.</summary>
        event Action OnListRoomsRefresh;

        /// <summary>Подія вибору кімнати зі списку.</summary>
        event Action<RoomInfo> OnRoomSelected;
    }
}