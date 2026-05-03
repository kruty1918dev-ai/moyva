using System;

namespace Kruty1918.Moyva.HomeMenu.UI
{
    /// <summary>
    /// View-контракт для панелі введення пароля.
    /// </summary>
    public interface IPasswordPanelViewController
    {
        /// <summary>Подія натискання OK з введеним паролем.</summary>
        event Action<string> OnConfirmed;
        /// <summary>Подія натискання Cancel або закриття панелі.</summary>
        event Action OnCancelled;

        /// <summary>True, якщо панель видима.</summary>
        bool IsVisible { get; }

        /// <summary>Показати панель з заголовком (назва кімнати) і опційним повідомленням про помилку.</summary>
        void Show(string roomDisplayName, string errorText);

        /// <summary>Сховати панель.</summary>
        void Hide();

        /// <summary>Оновити текст помилки без приховування панелі (наприклад, після невірного пароля).</summary>
        void SetErrorText(string errorText);
    }
}
