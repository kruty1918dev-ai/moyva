using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Lightweight offline implementation of IMultiplayerState used when network is unavailable.
    /// Reports disconnected state and completes any waiters immediately.
    /// </summary>
    internal sealed class OfflineMultiplayerState : IMultiplayerState
    {
        private static readonly MultiplayerConnectionState _offlineState =
            new MultiplayerConnectionState(
                isUnityServicesInitialized: false,
                isAuthenticated: false,
                isConnecting: false,
                isConnected: false,
                connectionProgress: 0f);

        public MultiplayerConnectionState ConnectionState => _offlineState;

        public Task WaitUntilReadyAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public Task WaitUntilReadyAsync(IProgress<float> progress, CancellationToken ct = default)
        {
            progress?.Report(0f);
            ct.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }
}
