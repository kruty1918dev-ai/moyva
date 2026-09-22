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

#if UNITY_EDITOR
        // Edit mode never runs RuntimeInitializeOnLoadMethod, so EditMode
        // tests and editor tooling would fall back to a worker thread —
        // which Unity Transport 6.x forbids (Allocator.Temp is main-thread
        // or job-worker only). Capture the editor's synchronization context
        // on domain reload so the pump matches player behavior.
        [UnityEditor.InitializeOnLoadMethod]
        private static void CaptureEditor()
        {
            if (_context != null)
                return;

            _context = SynchronizationContext.Current;
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }
#endif

        public static bool IsMainThread
            => _context == null
               || Thread.CurrentThread.ManagedThreadId == _mainThreadId;

        /// <summary>True when a Unity synchronization context was captured and
        /// <see cref="Post"/> can defer work to the main thread.</summary>
        public static bool CanPost => _context != null;

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

        /// <summary>
        /// Always defers to the next context pump, even when already on the
        /// main thread. Use for self-reposting loops where inline execution
        /// would recurse unboundedly.
        /// </summary>
        public static void PostDeferred(Action action)
        {
            if (action == null)
                return;

            var context = _context;
            if (context == null)
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
