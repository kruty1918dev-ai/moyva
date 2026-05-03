using System;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Networking;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Zenject;
using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal class HomeMenuInitializer : IInitializable
    {
        private const string Prefix = "[HomeMenuInitializer]";
        private readonly IMultiplayerState _multiplayerState;
        private readonly IOverlayLoader _overlayLoader;
        private readonly IConfirmationService _confirmationService;
        private readonly Kruty1918.Moyva.Shared.Connectivity.IConnectivityService _connectivityService;

        internal HomeMenuInitializer(
            [InjectOptional] IMultiplayerState multiplayerState = null,
            [InjectOptional] IConfirmationService confirmation = null,
            [InjectOptional] IOverlayLoader overlayLoader = null,
            [InjectOptional] Kruty1918.Moyva.Shared.Connectivity.IConnectivityService connectivityService = null)
        {
            _multiplayerState = multiplayerState;
            _overlayLoader = overlayLoader;
            _confirmationService = confirmation;
            _connectivityService = connectivityService;
        }

        public void Initialize()
        {
            Debug.Log($"{Prefix} Initialize start. overlayNull={_overlayLoader==null}, confirmationNull={_confirmationService==null}, multiplayerStateNull={_multiplayerState==null}, connectivityServiceNull={_connectivityService==null}");

            if (_overlayLoader == null || _confirmationService == null)
            {
                LogErrorWithPrefix("OverlayLoader or ConfirmationService is not available. Proceeding with degraded UX.");
            }

            // Start async initialization without blocking the main thread.
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            Debug.Log($"{Prefix} InitializeAsync started.");

            bool hasInternet = false;
            try
            {
                if (_connectivityService != null)
                {
                    Debug.Log($"{Prefix} Using IConnectivityService.WaitForOnlineAsync (timeout=8s)");
                    hasInternet = await _connectivityService.WaitForOnlineAsync(TimeSpan.FromSeconds(8));
                }
                else
                {
                    Debug.Log($"{Prefix} No IConnectivityService available — falling back to InternetChecker probe");
                    hasInternet = await Kruty1918.Moyva.Multiplayer.Core.InternetChecker.HasInternetAsync(5, 3);
                }
            }
            catch (Exception ex)
            {
                LogWithPrefix($"Internet check failed: {ex.Message}");
            }

            Debug.Log($"{Prefix} Connectivity probe result: {hasInternet}");

            // If multiplayer state is present, try initializing multiplayer regardless of the quick connectivity probe.
            // The multiplayer state will perform its own initialization/auth and report timeout if needed.
            if (_multiplayerState != null)
            {
                Debug.Log($"{Prefix} Multiplayer state present — starting multiplayer initialization.");
                StartMultiplayerInitialization();
                return;
            }

            // No multiplayer state available — fall back to offline flow only when we truly do not have connectivity.
            if (!hasInternet)
            {
                Debug.Log($"{Prefix} No connectivity detected and no multiplayer state — showing offline dialog.");
                ShowOfflineDialogPanel();
            }
            else
            {
                Debug.Log($"{Prefix} Connectivity detected but no multiplayer state — continuing without multiplayer.");
            }
        }

        private void ShowOfflineDialogPanel()
        {
            if (_confirmationService != null)
            {
                _confirmationService.Show(new ConfirmationRequest
                {
                    LabelText = "Офлайн режим",
                    MessageText = "Мультиплеєрні сервіси недоступні. Ви будете грати в офлайн режимі без можливості взаємодії з іншими гравцями. Бажаєте продовжити?",
                    OnConfirm = () => LogWithPrefix("Player acknowledged offline mode."),
                    OnCancel = () =>
                    {
                        LogWithPrefix("Player cancelled the offline mode dialog.");
                        Application.Quit();
                    }
                });
                LogWithPrefix("Multiplayer services are not available. Showing offline mode dialog.");
                return;
            }

            // Fallback: if confirmation service missing, try to show overlay as a blocking indicator
            LogWithPrefix("Confirmation service missing — falling back to overlay or continuing offline.");
            if (_overlayLoader != null)
            {
                var fallback = _overlayLoader.LoadOverlay(0f, 100f, "%");
                // keep overlay visible for a short period as a fallback UX
                Task.Run(async () =>
                {
                    await Task.Delay(2000).ConfigureAwait(false);
                    MainThreadDispatcher.Enqueue(() => _overlayLoader?.StopOverlay(true));
                });
                return;
            }

            LogWithPrefix("No confirmation or overlay service available; cannot show offline dialog. Continuing without UI.");
        }

        private async void StartMultiplayerInitialization()
        {
            var state = _multiplayerState.ConnectionState;
            var prefix = Prefix;

            if (state.IsConnected)
            {
                string userId = GetAuthenticatedUserId();
                Debug.Log($"{prefix} Initialized. Authenticated user id: {userId}.");
                return;
            }

            // If we are neither connected nor actively connecting, likely an offline stub.
            if (!state.IsConnecting)
            {
                bool canProceed = false;
                try
                {
                    if (_connectivityService != null)
                        canProceed = await _connectivityService.WaitForOnlineAsync(TimeSpan.FromSeconds(6));
                    else
                        canProceed = await Kruty1918.Moyva.Multiplayer.Core.InternetChecker.HasInternetAsync(3, 3);
                }
                catch (Exception ex)
                {
                    LogWithPrefix($"Connectivity quick probe failed: {ex.Message}");
                }

                if (!canProceed)
                {
                    ShowOfflineDialogPanel();
                    return;
                }
            }

            // Re-evaluate state (it might have started initializing)
            state = _multiplayerState.ConnectionState;
            float progressValue = state.ConnectionProgress * 100f;
            Debug.Log($"{prefix} Multiplayer services are not ready yet. Showing overlay and waiting for Unity services + authentication. Progress: {progressValue:0.##}%.");
            var overlayResult = _overlayLoader?.LoadOverlay(progressValue, 100f, "%");
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
                _overlayLoader?.StopOverlay(true);

                string userId = GetAuthenticatedUserId();
                Debug.Log($"{prefix} Multiplayer ready. Authenticated user id: {userId}.");
            }
            catch (TimeoutException exception)
            {
                overlayResult?.SetLoading(false, overlayResult?.Progress ?? 0f);
                _overlayLoader?.StopOverlay();
                Debug.LogError($"{prefix} Multiplayer initialization timed out: {exception.Message}");
            }
            catch (Exception exception)
            {
                overlayResult?.SetLoading(false, overlayResult?.Progress ?? 0f);
                _overlayLoader?.StopOverlay();
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

        private void LogWithPrefix(string msg)
        {
            Debug.Log($"[HomeMenuInitializer] {msg}");
        }

        private void LogErrorWithPrefix(string msg)
        {
            Debug.LogError($"[HomeMenuInitializer] {msg}");
        }
    }
}