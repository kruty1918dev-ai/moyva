using Kruty1918.Moyva.Signals;

namespace Kruty1918.Moyva.GameMode.API
{
    /// <summary>
    /// Контракт для UI-панелей, прив'язаних до конкретного ігрового режиму.
    ///
    /// Реалізуйте цей інтерфейс у своїй панелі та зареєструйте її через Zenject:
    /// <code>
    /// Container.BindInterfacesTo&lt;MyPanel&gt;().AsSingle();
    /// </code>
    /// <see cref="Runtime.GameModePanelController"/> автоматично отримає панель і
    /// керуватиме її видимістю — жодних змін у центральному коді не потрібно.
    /// </summary>
    public interface IGameModePanel
    {
        /// <summary>Режим гри, при якому ця панель має бути видима.</summary>
        GameModeType TargetMode { get; }

        /// <summary>Показати панель.</summary>
        void Show();

        /// <summary>Приховати панель.</summary>
        void Hide();
    }
}
