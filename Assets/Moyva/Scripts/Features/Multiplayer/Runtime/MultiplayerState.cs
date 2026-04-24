using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Networking;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    internal class MultiplayerState : IMultiplayerState
    {
        public MultiplayerConnectionState ConnectionState => BuildCurrentState();

        public Task WaitUntilReadyAsync(CancellationToken ct = default)
        {
            return WaitUntilReadyAsync(null, ct);
        }

        public async Task WaitUntilReadyAsync(IProgress<float> progress, CancellationToken ct = default)
        {
            var timeout = TimeSpan.FromSeconds(10);
            var deadline = DateTime.UtcNow + timeout;

            while (!ConnectionState.IsConnected)
            {
                ct.ThrowIfCancellationRequested();

                await EnsureAuthenticationAsync(ct);

                progress?.Report(ConnectionState.ConnectionProgress);

                if (DateTime.UtcNow > deadline)
                {
                    var message = $"[MultiplayerState] Initialization timeout after {timeout.TotalSeconds} seconds.";
                    Debug.LogError(message);
                    throw new TimeoutException(message);
                }

                await Task.Delay(100, ct);
            }

            progress?.Report(1f);
        }

        private static async Task EnsureAuthenticationAsync(CancellationToken ct = default)
        {
            try
            {
                if (UnityServices.State != ServicesInitializationState.Initialized)
                    await UnityServices.InitializeAsync();

                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[MultiplayerState] Authentication initialization failed: {exception.Message}");
            }
        }

        private MultiplayerConnectionState BuildCurrentState()
        {
            var unityInitialized = UnityServices.State == ServicesInitializationState.Initialized;
            var unityInitializing = UnityServices.State == ServicesInitializationState.Initializing;
            var authenticated = unityInitialized && AuthenticationService.Instance.IsSignedIn;

            var isConnecting = unityInitializing || (unityInitialized && !authenticated);
            var isConnected = unityInitialized && authenticated;
            var progress = unityInitializing
                ? 0.25f
                : unityInitialized
                    ? (authenticated ? 1f : 0.75f)
                    : 0f;

            return new MultiplayerConnectionState(
                unityInitialized,
                authenticated,
                isConnecting,
                isConnected,
                progress);
        }
    }
}
