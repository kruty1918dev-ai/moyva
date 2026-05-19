using System.Collections.Generic;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Контракт однієї навігаційної панелі HomeMenu.
    /// Залежності: керується через INavigation/HomeMenuNavigation.
    /// </summary>
    public interface INavigationPanel
    {
        /// <summary>Унікальна назва панелі для маршрутизації.</summary>
        string MenuName { get; }

        /// <summary>Відкрити панель.</summary>
        void Open();

        /// <summary>Закрити панель.</summary>
        void Close();
    }
}