using System;
using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Контракт UI-контролера панелі створення кімнати.
    /// Залежності: використовується CreateRoomPanelService.
    /// </summary>
    public interface ICreateRoomViewController
    {
        /// <summary>Назва кімнати.</summary>
        string RoomName { get; set; }

        /// <summary>Пароль кімнати (може бути порожнім).</summary>
        string Password { get; set; }

        /// <summary>True, якщо кімната публічна.</summary>
        bool IsPublic { get; set; }

        /// <summary>Максимальна кількість гравців.</summary>
        int MaxPlayers { get; set; }

        /// <summary>Подія натискання кнопки Next/Create.</summary>
        event Action OnButtonNextClicked;

        /// <summary>Кнопка продовження, доступ до interactable/state.</summary>
        Button NextButton { get; }

        void ApplyPresentation(CreateRoomPanelPresentation presentation);
    }
}
