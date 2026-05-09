using Kruty1918.Moyva.HomeMenu.API;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>Агрегатор сервісів меню для геймплейного шару.</summary>
    internal sealed class MenuApi : IMenuApi
    {
        public IGameplaySession GameplaySession { get; }
        public ILocalGameSettingsService LocalGameSettings { get; }
        public IInfoPanelService InfoPanel { get; }

        public MenuApi(IGameplaySession session, ILocalGameSettingsService settings, IInfoPanelService info)
        {
            GameplaySession = session;
            LocalGameSettings = settings;
            InfoPanel = info;
        }
    }
}
