namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Дані події зміни активної панелі навігації.
    /// Залежності: публікується INavigation і читається сервісами та UI HomeMenu.
    /// </summary>
    public struct NavigationChangeEventArgs
    {
        /// <summary>Назва попередньої панелі.</summary>
        public string PreviousMenu;

        /// <summary>Назва нової поточної панелі.</summary>
        public string CurrentMenu;

        /// <summary>True, якщо поточна панель відкрита після зміни стану.</summary>
        public bool CurrentIsOpen;
    }
}