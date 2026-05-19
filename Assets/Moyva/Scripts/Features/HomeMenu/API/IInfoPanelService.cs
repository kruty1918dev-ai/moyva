namespace Kruty1918.Moyva.HomeMenu.API
{

    /// <summary>
    /// Сервіс показу єдиної модальної інформаційної панелі (OK-only).
    /// Використовуйте для не-блокуючих повідомлень: помилки мережі, кік, "гра вже почалась" тощо.
    /// </summary>
    public interface IInfoPanelService
    {
        /// <summary>Показати повідомлення. Якщо панель уже відкрита — повідомлення стає в чергу.</summary>
        void Show(InfoMessage message);

        /// <summary>Примусово сховати панель і скинути чергу.</summary>
        void ForceHide();

        /// <summary>True, якщо панель зараз відкрита.</summary>
        bool IsShown { get; }
    }
}
