namespace Kruty1918.Moyva.HomeMenu.Runtime.Services
{
    internal interface IJoinRoomUiGateway
    {
        string CurrentMenu { get; }
        string ResolveJoinOriginPanelName(string lastJoinPanelName);
        void OpenLobbyPanel(string inviteCode);
        void OpenJoinPanelForce(string panelName);
    }
}
