using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    /// <summary>
    /// Простий singleton dispatcher для виконання дій у головному Unity-потоці.
    /// Створюється автоматично перед завантаженням сцен.
    /// </summary>
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();
        public static MainThreadDispatcher Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeDispatcher()
        {
            var go = new GameObject("MainThreadDispatcher");
            DontDestroyOnLoad(go);
            go.AddComponent<MainThreadDispatcher>();
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            while (_actions.TryDequeue(out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        public static void Enqueue(Action action)
        {
            if (action == null) return;
            _actions.Enqueue(action);
        }
    }
}
