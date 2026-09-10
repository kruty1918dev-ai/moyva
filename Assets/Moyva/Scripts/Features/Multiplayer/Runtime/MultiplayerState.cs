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

            while (!ConnectionState.IsConnected)
            {
                ct.ThrowIfCancellationRequested();

                var state = ConnectionState;
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
                await MultiplayerAuthenticationGate.EnsureReadyAsync(ct);
            }
            catch (Exception exception)
            {
                Debug.LogError($"{Prefix} Authentication initialization failed: {exception.Message}");
            }
        }

        private MultiplayerConnectionState BuildCurrentState()
        {
            var unityInitialized = UnityServices.State == ServicesInitializationState.Initialized;
            var unityInitializing = UnityServices.State == ServicesInitializationState.Initializing;
            var authenticated = unityInitialized && AuthenticationService.Instance.IsSignedIn
                && AuthenticationService.Instance.IsAuthorized;

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

    internal static class MultiplayerAuthenticationGate
    {
        private static readonly object Gate = new();
        private static Task _initializationTask;

        public static async Task EnsureReadyAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            Task pending;
            lock (Gate)
            {
                bool ready = UnityServices.State == ServicesInitializationState.Initialized
                    && AuthenticationService.Instance.IsSignedIn
                    && AuthenticationService.Instance.IsAuthorized;
                if (ready)
                    return;
                if (_initializationTask == null || _initializationTask.IsCompleted)
                    _initializationTask = InitializeAndSignInAsync();
                pending = _initializationTask;
            }
            // Cancelling one menu must not cancel application-wide initialization.
            var cancelled = new TaskCompletionSource<bool>();
            using (ct.Register(() => cancelled.TrySetCanceled(ct)))
            {
                await await Task.WhenAny(pending, cancelled.Task);
            }
        }

        private static async Task InitializeAndSignInAsync()
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(30);
            while (UnityServices.State == ServicesInitializationState.Initializing)
            {
                if (DateTime.UtcNow >= deadline)
                    throw new TimeoutException("Unity Services initialization did not finish.");
                await Task.Delay(100);
            }
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();

            MultiplayerClientScope.ApplyAuthenticationProfileIfNeeded();

            if (AuthenticationService.Instance == null)
                throw new InvalidOperationException("Authentication service is unavailable.");
            if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
                return;
            if (AuthenticationService.Instance.IsSignedIn)
                AuthenticationService.Instance.SignOut();

            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            catch (AuthenticationException e) when (e.ErrorCode == AuthenticationErrorCodes.ClientInvalidUserState)
            {
                await WaitForConcurrentSignInAsync();
            }
        }

        private static async Task WaitForConcurrentSignInAsync()
        {
            const int delayMs = 100;
            const int timeoutMs = 5000;
            int waitedMs = 0;

            while (waitedMs < timeoutMs)
            {
                if (AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn
                    && AuthenticationService.Instance.IsAuthorized)
                    return;

                await Task.Delay(delayMs);
                waitedMs += delayMs;
            }

            if (AuthenticationService.Instance == null || !AuthenticationService.Instance.IsSignedIn
                || !AuthenticationService.Instance.IsAuthorized)
                throw new InvalidOperationException("Authentication is already signing in and did not complete.");
        }
    }
}
