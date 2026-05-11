using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.UI;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime.Services
{
    internal sealed class JoinRoomUiGateway : IJoinRoomUiGateway
    {
        [Inject] private INavigation _navigation;
        [Inject] private ILobbyPanelViewController _lobbyPanelViewController;
        [Inject(Id = "LobbyPanelName")] private string _lobbyPanelName;
        [Inject(Id = "JoinRoomPanelName")] private string _joinRoomPanelName;

        public string CurrentMenu => _navigation?.CurrentMenu;

        public string ResolveJoinOriginPanelName(string lastJoinPanelName)
        {
            var currentMenu = _navigation?.CurrentMenu;
            if (!string.IsNullOrWhiteSpace(currentMenu) && !string.Equals(currentMenu, _lobbyPanelName, StringComparison.Ordinal))
                return currentMenu;

            if (!string.IsNullOrWhiteSpace(lastJoinPanelName))
                return lastJoinPanelName;

            return _joinRoomPanelName;
        }

        public void OpenLobbyPanel(string inviteCode)
        {
            try
            {
                _lobbyPanelViewController?.SetLobbyInvateCode(inviteCode);
            }
            catch
            {
                // Keep navigation alive even when invite code UI update fails.
            }

            try
            {
                _navigation?.Open(_lobbyPanelName);
            }
            catch (Exception navEx)
            {
                Debug.LogError($"[JoinRoomUiGateway] Navigation.Open('{_lobbyPanelName}') failed: {navEx.Message}");
            }
        }

        public void OpenJoinPanelForce(string panelName)
        {
            var targetPanel = string.IsNullOrWhiteSpace(panelName) ? _joinRoomPanelName : panelName;
            _navigation?.OpenForce(targetPanel);
        }
    }
}
