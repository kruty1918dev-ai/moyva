using System;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Інформаційне повідомлення для модальної панелі з єдиною кнопкою OK.
    /// Іммутабельна структура — безпечно передавати між потоками.
    /// </summary>
    public readonly struct InfoMessage
    {
        /// <summary>Заголовок повідомлення.</summary>
        public string Title { get; }
        /// <summary>Текст повідомлення.</summary>
        public string Message { get; }
        /// <summary>Дія, що виконається після натискання OK (опціонально).</summary>
        public Action OnAcknowledged { get; }

        public InfoMessage(string title, string message, Action onAcknowledged = null)
        {
            Title = title ?? string.Empty;
            Message = message ?? string.Empty;
            OnAcknowledged = onAcknowledged;
        }
    }

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
