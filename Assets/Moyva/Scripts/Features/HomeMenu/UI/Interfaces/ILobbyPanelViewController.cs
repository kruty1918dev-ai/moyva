using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Контракт UI-контролера панелі лобі.
    /// Залежності: використовується LobbyPanelService для синхронізації стану кімнати.
    /// </summary>
    public interface ILobbyPanelViewController
    {
        /// <summary>Кнопка старту гри для host flow.</summary>
        Button StartGameButton { get; }

        /// <summary>Показати код запрошення до лобі.</summary>
        void SetLobbyInvateCode(string code);

        /// <summary>Очистити код запрошення в UI.</summary>
        void ClearLobbyInvateCode();

        /// <summary>Додати користувача до списку лобі.</summary>
        void AddNewUser(LobbyUserInfo userInfo);

        /// <summary>Видалити користувача зі списку за user id.</summary>
        void RemoveUser(int userId);

        /// <summary>Очистити список користувачів.</summary>
        void ClearUsers();

        /// <summary>Оновити візуальне відображення списку користувачів.</summary>
        void RefreshUserList();
    }
}