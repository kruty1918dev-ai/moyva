using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Networking;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Zenject;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal class HomeMenuInitializer : IInitializable
    {
        private readonly IMultiplayerState _multiplayerState;
        private readonly IOverlayLoader _overlayLoader;

        internal HomeMenuInitializer(
            IMultiplayerState multiplayerState,
            [Zenject.InjectOptional] IOverlayLoader overlayLoader = null)
        {
            _multiplayerState = multiplayerState;
            _overlayLoader = overlayLoader;
        }

        public void Initialize()
        {
            if (_overlayLoader == null)
                return;

            var state = _multiplayerState.ConnectionState;
            string prefix = "[HomeMenuInitializer]";

            if (state.IsConnected)
            {
                string userId = GetAuthenticatedUserId();
                Debug.Log($"{prefix} Initialized. Authenticated user id: {userId}.");
                return;
            }

            float progressValue = state.ConnectionProgress * 100f;
            Debug.Log($"{prefix} Multiplayer services are not ready yet. Showing overlay and waiting for Unity services + authentication. Progress: {progressValue:0.##}%.");
            var overlayResult = _overlayLoader.LoadOverlay(progressValue, 100f, "%");
            WaitForMultiplayerReadyAsync(prefix, overlayResult);
        }

        private async void WaitForMultiplayerReadyAsync(string prefix, OverlayLoaderResult overlayResult)
        {
            try
            {
                var progressReporter = new Progress<float>(connectionProgress =>
                {
                    overlayResult?.SetLoading(true, connectionProgress * 100f);
                });

                await _multiplayerState.WaitUntilReadyAsync(progressReporter);
                    var last = overlayResult?.Progress ?? 0f;
                    overlayResult?.SetLoading(false, last);
                    _overlayLoader.StopOverlay(true);

                string userId = GetAuthenticatedUserId();
                Debug.Log($"{prefix} Multiplayer ready. Authenticated user id: {userId}.");
            }
            catch (TimeoutException exception)
            {
                overlayResult?.SetLoading(false, overlayResult?.Progress ?? 0f);
                _overlayLoader.StopOverlay();
                Debug.LogError($"{prefix} Multiplayer initialization timed out: {exception.Message}");
            }
            catch (Exception exception)
            {
                overlayResult?.SetLoading(false, overlayResult?.Progress ?? 0f);
                _overlayLoader.StopOverlay();
                Debug.LogError($"{prefix} Multiplayer initialization failed: {exception.Message}");
            }
        }

        private static string GetAuthenticatedUserId()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized &&
                AuthenticationService.Instance.IsSignedIn)
            {
                return AuthenticationService.Instance.PlayerId ?? "unknown";
            }
            return "unknown";
        }
    }
}