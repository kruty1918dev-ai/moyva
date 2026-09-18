using System;

namespace Kruty1918.Moyva.Generator.Runtime
{
    /// <summary>
    /// Scopes both Moyva and Unity random state so recipe evaluation is
    /// deterministic without leaking global state to the editor or gameplay.
    /// </summary>
    public sealed class DeterministicRandomScope : IDisposable
    {
        private int _externalGlobalSeed;
        private UnityEngine.Random.State _externalUnityState;
        private int _sessionGlobalSeed;
        private UnityEngine.Random.State _sessionUnityState;
        private bool _isSuspended;
        private bool _disposed;

        public DeterministicRandomScope(int seed)
        {
            int normalized = GlobalSeed.Normalize(seed);
            _externalGlobalSeed = GlobalSeed.Current;
            _externalUnityState = UnityEngine.Random.state;
            GlobalSeed.Set(normalized);
            UnityEngine.Random.InitState(normalized);
            CaptureSessionState();
        }

        /// <summary>
        /// Temporarily restores the caller's random state while an asynchronous
        /// evaluation yields control back to Unity. Resume restores the exact
        /// session state, so editor/game code cannot perturb evaluation.
        /// </summary>
        public void Suspend()
        {
            ThrowIfDisposed();
            if (_isSuspended)
                return;
            CaptureSessionState();
            GlobalSeed.Set(_externalGlobalSeed);
            UnityEngine.Random.state = _externalUnityState;
            _isSuspended = true;
        }

        /// <summary>
        /// Continues a previously suspended random session. Any random state
        /// changes made by the caller while suspended are preserved and restored
        /// again by the next Suspend or Dispose.
        /// </summary>
        public void Resume()
        {
            ThrowIfDisposed();
            if (!_isSuspended)
                return;
            _externalGlobalSeed = GlobalSeed.Current;
            _externalUnityState = UnityEngine.Random.state;
            GlobalSeed.Set(_sessionGlobalSeed);
            UnityEngine.Random.state = _sessionUnityState;
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

        private void CaptureSessionState()
        {
            _sessionGlobalSeed = GlobalSeed.Current;
            _sessionUnityState = UnityEngine.Random.state;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DeterministicRandomScope));
        }
    }
}
