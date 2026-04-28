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
        private const string Prefix = "[MultiplayerState]";
        public MultiplayerConnectionState ConnectionState => BuildCurrentState();

        public Task WaitUntilReadyAsync(CancellationToken ct = default)
        {
            return WaitUntilReadyAsync(null, ct);
        }

        public async Task WaitUntilReadyAsync(IProgress<float> progress, CancellationToken ct = default)
        {
            var timeout = TimeSpan.FromSeconds(10);
            var deadline = DateTime.UtcNow + timeout;

            Debug.Log($"{Prefix} WaitUntilReadyAsync start. deadline in {timeout.TotalSeconds}s.");

            while (!ConnectionState.IsConnected)
            {
                ct.ThrowIfCancellationRequested();

                var state = ConnectionState;
                Debug.Log($"{Prefix} CurrentState: IsInitialized={state.IsUnityServicesInitialized}, IsAuthenticated={state.IsAuthenticated}, IsConnecting={state.IsConnecting}, Progress={state.ConnectionProgress:0.##}");

                Debug.Log($"{Prefix} Ensuring authentication/initialization...");
                await EnsureAuthenticationAsync(ct);
                Debug.Log($"{Prefix} EnsureAuthenticationAsync returned.");

                progress?.Report(ConnectionState.ConnectionProgress);

                if (DateTime.UtcNow > deadline)
                {
                    var message = $"[MultiplayerState] Initialization timeout after {timeout.TotalSeconds} seconds.";
                    Debug.LogError(message);
                    throw new TimeoutException(message);
                }

                await Task.Delay(100, ct);
            }

            Debug.Log($"{Prefix} WaitUntilReadyAsync completed — connected.");
            progress?.Report(1f);
        }

        private static async Task EnsureAuthenticationAsync(CancellationToken ct = default)
        {
            try
            {
                Debug.Log($"{Prefix} EnsureAuthenticationAsync start.");
                if (UnityServices.State != ServicesInitializationState.Initialized)
                {
                    Debug.Log($"{Prefix} UnityServices.State={UnityServices.State} — calling InitializeAsync()");
                    await UnityServices.InitializeAsync();
                    Debug.Log($"{Prefix} UnityServices.InitializeAsync completed. State={UnityServices.State}");
                }

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    Debug.Log($"{Prefix} Not signed in — calling SignInAnonymouslyAsync()");
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log($"{Prefix} SignInAnonymouslyAsync completed. IsSignedIn={AuthenticationService.Instance.IsSignedIn}");
                }
                else
                {
                    Debug.Log($"{Prefix} Already signed in.");
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"{Prefix} Authentication initialization failed: {exception.Message}");
            }
        }

        private MultiplayerConnectionState BuildCurrentState()
        {
            Debug.Log($"{Prefix} BuildCurrentState start. UnityServices.State={UnityServices.State}, IsSignedIn={AuthenticationService.Instance.IsSignedIn}");
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
            
            // Note: returning state — caller will log details as needed
        }
    }
}
