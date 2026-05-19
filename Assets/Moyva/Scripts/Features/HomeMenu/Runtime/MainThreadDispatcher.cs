using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Простий singleton dispatcher для виконання дій у головному Unity-потоці.
    /// Створюється автоматично перед завантаженням сцен.
    /// </summary>
    public class MainThreadDispatcher : MonoBehaviour
    {
        /// <summary>Черга дій, які мають бути виконані в головному Unity-потоці.</summary>
        private static readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();

        /// <summary>Поточний singleton-екземпляр dispatcher'а.</summary>
        public static MainThreadDispatcher Instance { get; private set; }

        /// <summary>Автоматично створити dispatcher до завантаження першої сцени.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeDispatcher()
        {
            // 1: Створюємо технічний GameObject для singleton-диспетчера.
            var go = new GameObject("MainThreadDispatcher");

            // 2: Робимо його персистентним між сценами.
            DontDestroyOnLoad(go);

            // 3: Додаємо сам MonoBehaviour-компонент, який буде спорожнювати чергу в Update.
            go.AddComponent<MainThreadDispatcher>();
        }

        /// <summary>Ініціалізувати singleton або прибрати дубльований екземпляр.</summary>
        private void Awake()
        {
            // 1: Перший екземпляр стає глобальним singleton'ом.
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            // 2: Усі зайві дублікати видаляємо, щоб у черги був один обробник.
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>Виконати всі відкладені дії в головному Unity-потоці.</summary>
        private void Update()
        {
            // 1: Обробляємо чергу доти, доки в ній є дії на поточний кадр.
            while (_actions.TryDequeue(out var action))
            {
                try
                {
                    // 2: Викликаємо дію, якщо вона не null.
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    // 3: Не даємо одній помилці зупинити виконання всієї черги.
                    Debug.LogException(ex);
                }
            }
        }

        /// <summary>Поставити дію в чергу для виконання в головному потоці.</summary>
        public static void Enqueue(Action action)
        {
            // 1: Null-дії ігноруємо, щоб не засмічувати чергу порожніми елементами.
            if (action == null) return;

            // 2: Додаємо дію в thread-safe чергу на виконання в Update.
            _actions.Enqueue(action);
        }

        /// <summary>Поставити дію в чергу й отримати Task, який завершиться після її виконання.</summary>
        public static Task EnqueueAsync(Action action)
        {
            // 1: Для null-дії одразу повертаємо завершений Task.
            if (action == null)
                return Task.CompletedTask;

            // 2: Створюємо completion source, щоб викликаючий код міг дочекатися виконання дії.
            var completion = new TaskCompletionSource<bool>();
            Enqueue(() =>
            {
                try
                {
                    // 3: Виконуємо дію вже в головному потоці.
                    action();

                    // 4: Позначаємо Task як успішно завершений.
                    completion.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    // 5: Пробрасываем exception у Task для коду, який очікує результат.
                    completion.TrySetException(ex);
                    throw;
                }
            });

            // 6: Повертаємо Task очікування завершення queued action.
            return completion.Task;
        }
    }
}
