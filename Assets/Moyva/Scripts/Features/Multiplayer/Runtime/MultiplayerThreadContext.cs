using System;
using System.Threading;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Marshals work onto Unity's main thread. The synchronization context is
    /// captured before the first scene loads, so transport pumps and discovery
    /// loops can raise gameplay events from worker threads without touching
    /// Unity APIs off-thread. When no context is captured (edit-mode tests),
    /// actions execute inline so tests stay deterministic.
    /// </summary>
    internal static class MultiplayerThreadContext
    {
        private static SynchronizationContext _context;
        private static int _mainThreadId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Capture()
        {
            _context = SynchronizationContext.Current;
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public static bool IsMainThread
            => _context == null
               || Thread.CurrentThread.ManagedThreadId == _mainThreadId;

        public static void Post(Action action)
        {
            if (action == null)
                return;

            var context = _context;
            if (context == null || Thread.CurrentThread.ManagedThreadId == _mainThreadId)
            {
                InvokeSafe(action);
                return;
            }

            context.Post(_ => InvokeSafe(action), null);
        }

        private static void InvokeSafe(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
