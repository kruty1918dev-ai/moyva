namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Агрегований API меню — єдина точка входу для геймплейного шару.
    /// Геймплейний шар не повинен мати прямих залежностей до конкретних сервісів меню/мережі;
    /// усе споживається через цей інтерфейс.
    /// </summary>
    public interface IMenuApi
    {
        IGameplaySession GameplaySession { get; }
        ILocalGameSettingsService LocalGameSettings { get; }
        IInfoPanelService InfoPanel { get; }
    }
}
