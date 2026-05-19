using System;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Контракт стекової навігації меню.
    /// Залежності: реалізується HomeMenuNavigation і використовується майже всіма HomeMenu service/UI шарами.
    /// </summary>
    public interface INavigation
    {
        /// <summary>Закрити вказану панель меню зі звичайною логікою навігації.</summary>
        void Close(string menuName);

        /// <summary>Примусово закрити вказану панель без додаткових перевірок.</summary>
        void CloseForce(string menuName);

        /// <summary>Закрити панель тільки якщо асинхронна умова повернула true.</summary>
        Task CloseIf(string menuName, Func<Task<bool>> condition);

        /// <summary>Відкрити панель меню.</summary>
        void Open(string menuName);

        /// <summary>Примусово відкрити панель меню.</summary>
        void OpenForce(string menuName);

        /// <summary>Відкрити попередню панель зі стеку.</summary>
        void OpenLast();

        /// <summary>Примусово відкрити попередню панель зі стеку.</summary>
        void OpenLastForce();

        /// <summary>Відкрити панель лише після успішної асинхронної перевірки.</summary>
        Task OpenIfAsync(string menuName, Func<Task<bool>> condition);

        /// <summary>Закрити поточну верхню панель у стеку.</summary>
        void CloseLast();

        /// <summary>Примусово закрити поточну верхню панель у стеку.</summary>
        void CloseLastForce();

        /// <summary>Назва поточної відкритої панелі.</summary>
        string CurrentMenu { get; }

        /// <summary>Подія зміни активної панелі навігації.</summary>
        event Action<NavigationChangeEventArgs> OnMenuChanged;
    }
}