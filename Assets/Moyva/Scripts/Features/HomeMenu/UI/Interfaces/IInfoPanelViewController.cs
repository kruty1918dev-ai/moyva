using System;
using Kruty1918.Moyva.HomeMenu.API;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// Контракт для View інформаційної панелі (заголовок, текст, кнопка OK).
    /// </summary>
    public interface IInfoPanelViewController
    {
        /// <summary>Подія натискання кнопки OK (підтвердження прочитання).</summary>
        event Action OnAcknowledged;

        /// <summary>True, якщо панель зараз показана.</summary>
        bool IsVisible { get; }

        /// <summary>Показати повідомлення та зробити панель видимою.</summary>
        void Show(InfoMessage message);

        /// <summary>Сховати панель.</summary>
        void Hide();
    }
}
