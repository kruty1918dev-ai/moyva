using Kruty1918.Moyva.HomeMenu.API;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>Агрегатор сервісів меню для геймплейного шару.</summary>
    internal sealed class MenuApi : IMenuApi
    {
        /// <summary>Сесія gameplay, підготовлена в меню.</summary>
        public IGameplaySession GameplaySession { get; }

        /// <summary>Локальні налаштування гравця.</summary>
        public ILocalGameSettingsService LocalGameSettings { get; }

        /// <summary>Сервіс інформаційних повідомлень меню.</summary>
        public IInfoPanelService InfoPanel { get; }

        /// <summary>Створити агрегатор API меню.</summary>
        public MenuApi(IGameplaySession session, ILocalGameSettingsService settings, IInfoPanelService info)
        {
            GameplaySession = session;
            LocalGameSettings = settings;
            InfoPanel = info;
        }
    }
}
