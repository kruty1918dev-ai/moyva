using System;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Асинхронне завантаження сцен із прогресом.
    /// Реалізація базується на <c>SceneManager.LoadSceneAsync</c>.
    /// </summary>
    public interface ISceneLoadService
    {
        /// <summary>Повертає <c>true</c>, коли у цей момент виконується завантаження.</summary>
        bool IsLoading { get; }

        /// <summary>
        /// Починає завантаження сцени за назвою (має бути доданою у Build Settings).
        /// Публікує прогрес у <paramref name="progress"/> у [0..1]. Виклик <paramref name="onCompleted"/>
        /// відбувається після того, як сцена активована.
        /// </summary>
        /// <param name="sceneName">Назва сцени у Build Settings.</param>
        /// <param name="progress">Опціональний колбек прогресу (0..1).</param>
        /// <param name="onCompleted">Опціональний колбек завершення.</param>
        void LoadSceneAsync(string sceneName, Action<float> progress = null, Action onCompleted = null);

        /// <summary>Завершує додаток. В редакторі викликає зупинку Play Mode, у білді — Application.Quit.</summary>
        void QuitApplication();
    }
}
