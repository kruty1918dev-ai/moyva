using System;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.Runtime;

namespace Kruty1918.Moyva.HomeMenu.API
{
    /// <summary>
    /// Результат/стан асинхронного завантаження оверлею в HomeMenu.
    /// Залежності: публікується через статичний стан Current і синхронізується з <see cref="MainThreadDispatcher"/>.
    /// </summary>
    public class OverlayLoaderResult
    {
        /// <summary>
        /// Поточний активний результат оверлей-завантаження.
        /// </summary>
        public static OverlayLoaderResult Current { get; private set; }

        /// <summary>
        /// Подія зміни поточного результату завантаження.
        /// </summary>
        public static event Action<OverlayLoaderResult> CurrentChanged;

        /// <summary>True, якщо завантаження триває.</summary>
        public bool IsLoading { get; private set; }

        /// <summary>Поточний прогрес завантаження у діапазоні 0..1.</summary>
        public float Progress { get; private set; }

        /// <summary>Колбек успішного завершення операції.</summary>
        public Action OnSuccess { get; set; }

        /// <summary>Колбек обробки помилки операції.</summary>
        public Action<Exception> OnError { get; set; }

        /// <summary>Внутрішнє посилання на task, що представляє виконання операції.</summary>
        private Task _task;

        /// <summary>
        /// Приватний конструктор, щоб створення відбувалося лише через <see cref="Start"/>.
        /// </summary>
        private OverlayLoaderResult()
        {
        }

        /// <summary>
        /// Start an async overlay task. The returned <see cref="OverlayLoaderResult"/> does not block threads;
        /// it tracks the provided async action and will invoke OnSuccess/OnError when it completes.
        /// </summary>
        public static OverlayLoaderResult Start(Func<Task> asyncAction, Action onSuccess = null, Action<Exception> onError = null)
        {
            // 1: Створюємо об'єкт результату і прив'язуємо зовнішні колбеки.
            var result = new OverlayLoaderResult
            {
                OnSuccess = onSuccess,
                OnError = onError
            };

            // 2: Запускаємо передану асинхронну дію на thread-pool і зберігаємо Task для спостереження.
            result._task = Task.Run(async () =>
            {
                await asyncAction().ConfigureAwait(false);
            });

            // 3: Підписуємо continuation для перенесення колбеків успіху/помилки у main thread.
            result._task.ContinueWith(t =>
            {
                // 3.1: Faulted Task -> віддаємо помилку в UI через MainThreadDispatcher.
                if (t.IsFaulted)
                    MainThreadDispatcher.Enqueue(() => result.OnError?.Invoke(t.Exception.InnerException ?? t.Exception));

                // 3.2: Успіх -> викликаємо OnSuccess також у main thread.
                else
                    MainThreadDispatcher.Enqueue(() => result.OnSuccess?.Invoke());
            }, TaskScheduler.Default);

            // 4: Повертаємо об'єкт-обгортку, з яким працює UI/сервіси меню.
            return result;
        }

        /// <summary>
        /// Оновлює стан завантаження й нотифікує підписників про зміну Current.
        /// </summary>
        /// <param name="isLoading">Поточний стан завантаження.</param>
        /// <param name="progress">Поточний прогрес.</param>
        internal void SetLoading(bool isLoading, float progress)
        {
            // 1: Фіксуємо нові значення стану.
            IsLoading = isLoading;
            Progress = progress;

            // 2: Публікуємо цей екземпляр як поточний глобальний стан.
            Current = this;

            // 3: Сповіщаємо всіх слухачів про зміну прогресу/стану.
            CurrentChanged?.Invoke(this);
        }
    }
}