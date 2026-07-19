using System;

namespace Kruty1918.Moyva.GraphSystem.API
{
    /// <summary>
    /// Scopes both Moyva and Unity random state so graph evaluation is deterministic
    /// without leaking global state to the editor or gameplay.
    /// </summary>
    public sealed class GraphRandomScope : IDisposable
    {
        private int _externalGlobalSeed;
        private UnityEngine.Random.State _externalUnityState;
        private int _graphGlobalSeed;
        private UnityEngine.Random.State _graphUnityState;
        private bool _isSuspended;
        private bool _disposed;

        public GraphRandomScope(int seed)
        {
            int normalized = GlobalSeed.Normalize(seed);
            _externalGlobalSeed = GlobalSeed.Current;
            _externalUnityState = UnityEngine.Random.state;
            GlobalSeed.Set(normalized);
            UnityEngine.Random.InitState(normalized);
            CaptureGraphState();
        }

        /// <summary>
        /// Temporarily restores the caller's random state while an asynchronous
        /// graph evaluation yields control back to Unity. Resume restores the
        /// exact graph state, so editor/game code cannot perturb evaluation.
        /// </summary>
        public void Suspend()
        {
            ThrowIfDisposed();
            if (_isSuspended)
                return;

            CaptureGraphState();
            GlobalSeed.Set(_externalGlobalSeed);
            UnityEngine.Random.state = _externalUnityState;
            _isSuspended = true;
        }

        /// <summary>
        /// Continues a previously suspended graph random session. Any random
        /// state changes made by the caller while suspended are preserved and
        /// restored again by the next Suspend or Dispose.
        /// </summary>
        public void Resume()
        {
            ThrowIfDisposed();
            if (!_isSuspended)
                return;

            _externalGlobalSeed = GlobalSeed.Current;
            _externalUnityState = UnityEngine.Random.state;
            GlobalSeed.Set(_graphGlobalSeed);
            UnityEngine.Random.state = _graphUnityState;
            _isSuspended = false;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            if (!_isSuspended)
            {
                GlobalSeed.Set(_externalGlobalSeed);
                UnityEngine.Random.state = _externalUnityState;
            }

            _disposed = true;
        }

        private void CaptureGraphState()
        {
            _graphGlobalSeed = GlobalSeed.Current;
            _graphUnityState = UnityEngine.Random.state;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(GraphRandomScope));
        }
    }
}
