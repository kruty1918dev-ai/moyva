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

        /// <summary>Створити інформаційне повідомлення для модальної панелі.</summary>
        public InfoMessage(string title, string message, Action onAcknowledged = null)
        {
            Title = title ?? string.Empty;
            Message = message ?? string.Empty;
            OnAcknowledged = onAcknowledged;
        }
    }
}
