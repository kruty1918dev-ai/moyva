using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    public interface IMultiplayerState
    {
        MultiplayerConnectionState ConnectionState { get; }
        Task WaitUntilReadyAsync(CancellationToken ct = default);
        Task WaitUntilReadyAsync(IProgress<float> progress, CancellationToken ct = default);
    }


}
public struct MultiplayerConnectionState
{
    public bool IsUnityServicesInitialized { get; }
    public bool IsAuthenticated { get; }
    public bool IsConnecting { get; }
    public bool IsConnected { get; }
    public float ConnectionProgress { get; }

    public bool IsInitialized => IsUnityServicesInitialized;
    public bool IsAuthenticationReady => IsUnityServicesInitialized;

    public MultiplayerConnectionState(
        bool isUnityServicesInitialized,
        bool isAuthenticated,
        bool isConnecting,
        bool isConnected,
        float connectionProgress)
    {
        IsUnityServicesInitialized = isUnityServicesInitialized;
        IsAuthenticated = isAuthenticated;
        IsConnecting = isConnecting;
        IsConnected = isConnected;
        ConnectionProgress = connectionProgress < 0f ? 0f : (connectionProgress > 1f ? 1f : connectionProgress);
    }
}