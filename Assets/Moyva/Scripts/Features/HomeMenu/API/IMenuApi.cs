namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Агрегований API меню — єдина точка входу для геймплейного шару.
    /// Геймплейний шар не повинен мати прямих залежностей до конкретних сервісів меню/мережі;
    /// усе споживається через цей інтерфейс.
    /// </summary>
    public interface IMenuApi
    {
        /// <summary>Стан/контекст активної gameplay-сесії, підготовлений у HomeMenu.</summary>
        IGameplaySession GameplaySession { get; }

        /// <summary>Локальні налаштування гравця (ім'я, гучність, mute, тощо).</summary>
        ILocalGameSettingsService LocalGameSettings { get; }

        /// <summary>Сервіс інфопанелі для показу повідомлень користувачу.</summary>
        IInfoPanelService InfoPanel { get; }
    }
}
