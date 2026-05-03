using Kruty1918.Moyva.HomeMenu.API;
using UnityEngine.UI;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    public interface ILobbyPanelViewController
    {
        Button StartGameButton { get; }

        void SetLobbyInvateCode(string code);
        void ClearLobbyInvateCode();

        void AddNewUser(LobbyUserInfo userInfo);
        void RemoveUser(int userId);
        void ClearUsers();
        void RefreshUserList();
    }
}