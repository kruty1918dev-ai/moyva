namespace Kruty1918.Moyva.HomeMenu.Runtime.Services
{
    /// <summary>
    /// Адаптер доступу до UI-навігації join/lobby flow.
    /// Залежності: приховує конкретні UI-контролери та навігацію від доменної join-логіки.
    /// </summary>
    internal interface IJoinRoomUiGateway
    {
        /// <summary>Назва поточного активного меню.</summary>
        string CurrentMenu { get; }

        /// <summary>Визначити, з якої панелі був ініційований join-flow.</summary>
        string ResolveJoinOriginPanelName(string lastJoinPanelName);

        /// <summary>Відкрити панель lobby після успішного входу.</summary>
        void OpenLobbyPanel(string inviteCode);

        /// <summary>Примусово відкрити join-панель.</summary>
        void OpenJoinPanelForce(string panelName);
    }
}
