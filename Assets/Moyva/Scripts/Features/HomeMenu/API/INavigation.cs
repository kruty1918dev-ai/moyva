using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.HomeMenu.API
{
    public interface INavigation
    {
        void Open(string menuName);
    }

    public interface IOverlayLoader
    {
        OverlayLoaderResult LoadOverlay(float value, float maxValue = 100, string sufix = "%");
    }



    public class OverlayLoaderResult : Task
    {
        public Action OnSuccess { get; set; }
        public Action<Exception> OnError { get; set; }

        public OverlayLoaderResult(Action action) : base(action)
        {
        }

        public OverlayLoaderResult(Action action, CancellationToken cancellationToken) : base(action, cancellationToken)
        {
        }

        public OverlayLoaderResult(Action action, TaskCreationOptions creationOptions) : base(action, creationOptions)
        {
        }

        public OverlayLoaderResult(Action<object> action, object state) : base(action, state)
        {
        }

        public OverlayLoaderResult(Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : base(action, cancellationToken, creationOptions)
        {
        }

        public OverlayLoaderResult(Action<object> action, object state, CancellationToken cancellationToken) : base(action, state, cancellationToken)
        {
        }

        public OverlayLoaderResult(Action<object> action, object state, TaskCreationOptions creationOptions) : base(action, state, creationOptions)
        {
        }

        public OverlayLoaderResult(Action<object> action, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : base(action, state, cancellationToken, creationOptions)
        {
        }
    }
}