using System;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.Runtime;

namespace Kruty1918.Moyva.HomeMenu.API
{
    public class OverlayLoaderResult
    {
        public static OverlayLoaderResult Current { get; private set; }
        public static event Action<OverlayLoaderResult> CurrentChanged;

        public bool IsLoading { get; private set; }
        public float Progress { get; private set; }

        public Action OnSuccess { get; set; }
        public Action<Exception> OnError { get; set; }

        private Task _task;

        private OverlayLoaderResult()
        {
        }

        /// <summary>
        /// Start an async overlay task. The returned <see cref="OverlayLoaderResult"/> does not block threads;
        /// it tracks the provided async action and will invoke OnSuccess/OnError when it completes.
        /// </summary>
        public static OverlayLoaderResult Start(Func<Task> asyncAction, Action onSuccess = null, Action<Exception> onError = null)
        {
            var result = new OverlayLoaderResult
            {
                OnSuccess = onSuccess,
                OnError = onError
            };

            // Run the provided async action on the thread-pool and observe completion.
            result._task = Task.Run(async () =>
            {
                await asyncAction().ConfigureAwait(false);
            });

            result._task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    MainThreadDispatcher.Enqueue(() => result.OnError?.Invoke(t.Exception.InnerException ?? t.Exception));
                else
                    MainThreadDispatcher.Enqueue(() => result.OnSuccess?.Invoke());
            }, TaskScheduler.Default);

            return result;
        }

        internal void SetLoading(bool isLoading, float progress)
        {
            IsLoading = isLoading;
            Progress = progress;
            Current = this;
            CurrentChanged?.Invoke(this);
        }
    }
}