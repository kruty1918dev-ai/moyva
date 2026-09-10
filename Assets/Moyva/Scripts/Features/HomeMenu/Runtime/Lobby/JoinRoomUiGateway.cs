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
        [InjectOptional] private ILobbyFlowContext _lobbyFlowContext;
        [InjectOptional] private Kruty1918.Moyva.Multiplayer.Lobbies.ILobbyService _lobbyService;

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
                var label = _lobbyFlowContext?.Provider == Kruty1918.Moyva.Multiplayer.Networking.NetworkProviderType.Lan
                    ? "LAN Join Code"
                    : "Invite Code";
                var provider = _lobbyFlowContext?.Provider ?? Kruty1918.Moyva.Multiplayer.Networking.NetworkProviderType.Relay;
                _lobbyPanelViewController?.SetInviteCode(_lobbyService?.Current != null
                    ? LobbyInviteCodeResolver.Resolve(_lobbyService.Current, provider)
                    : new LobbyInviteCodePresentation(label, inviteCode));
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
