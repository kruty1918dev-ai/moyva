using System;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Логічні панелі головного меню. Використовується <see cref="IHomeMenuFlow"/>
    /// та UI-контролерами для обміну повідомленнями про поточний стан.
    /// </summary>
    public enum HomeMenuPanel
    {
        /// <summary>Завантажувальний оверлей (показується при старті / завантаженні сцен).</summary>
        Loading       = 0,
        /// <summary>Головна панель з кнопками Start / Settings / Quit.</summary>
        Main          = 1,
        /// <summary>Панель налаштування світу (з WorldCreationUIController).</summary>
        WorldCreation = 2,
        /// <summary>Панель налаштувань гри (аудіо / соц.мережі / видалення даних).</summary>
        Settings      = 3,
        /// <summary>Модальний діалог підтвердження (Quit / видалення даних).</summary>
        Confirm       = 4
    }

    /// <summary>
    /// Централізований оркестратор панелей HomeMenu. Усі переходи між UI-станами
    /// (показ/сховування панелей, відкриття діалогів) проходять через нього.
    /// </summary>
    public interface IHomeMenuFlow
    {
        /// <summary>Поточна активна панель.</summary>
        HomeMenuPanel CurrentPanel { get; }

        /// <summary>Подія зміни активної панелі.</summary>
        event Action<HomeMenuPanel> PanelChanged;

        /// <summary>Показує <see cref="HomeMenuPanel.Main"/>.</summary>
        void ShowMain();

        /// <summary>Показує <see cref="HomeMenuPanel.WorldCreation"/>.</summary>
        void ShowWorldCreation();

        /// <summary>Показує <see cref="HomeMenuPanel.Settings"/>.</summary>
        void ShowSettings();

        /// <summary>
        /// Показує модальний діалог підтвердження.
        /// </summary>
        /// <param name="title">Заголовок.</param>
        /// <param name="message">Повідомлення.</param>
        /// <param name="onConfirm">Колбек при підтвердженні.</param>
        /// <param name="onCancel">Опціональний колбек при скасуванні.</param>
        void ShowConfirm(string title, string message, Action onConfirm, Action onCancel = null);

        /// <summary>Ініціює завершення додатку з підтвердженням (показує діалог і на Confirm — ISceneLoadService.QuitApplication).</summary>
        void RequestQuit();
    }
}
