namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Контракт сервісу панелі керування виключенням гравців з лобі.
    /// Залежності: реалізується KickPlayerPanelService і працює з lobby/player UI.
    /// </summary>
    public interface IKickPlayerPanelService
    {
        /// <summary>Оновити список гравців та стан панелі.</summary>
        void Refresh();
    }
}