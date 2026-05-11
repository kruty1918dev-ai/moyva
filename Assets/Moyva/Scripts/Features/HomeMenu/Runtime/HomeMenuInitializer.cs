using System;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.SaveSystem;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Zenject;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Shared.Common;
using Kruty1918.Moyva.Shared.Graphics;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal class HomeMenuInitializer : IInitializable, IDisposable
    {
        private const string Prefix = "[HomeMenuInitializer]";
        private readonly IMultiplayerState _multiplayerState;
        private readonly IGameplaySession _gameplaySession;
        private readonly IOverlayLoader _overlayLoader;
        private readonly IConfirmationService _confirmationService;
        private readonly Kruty1918.Moyva.Shared.Connectivity.IConnectivityService _connectivityService;
        private readonly IGraphicsSettingsService _graphicsSettingsService;
        private readonly ServiceModeProfile _menuProfile;
        private CancellationTokenSource _lifecycleCts;

        internal HomeMenuInitializer(
            [InjectOptional] IMultiplayerState multiplayerState = null,
            [InjectOptional] IGameplaySession gameplaySession = null,
            [InjectOptional] IConfirmationService confirmation = null,
            [InjectOptional] IOverlayLoader overlayLoader = null,
            [InjectOptional] Kruty1918.Moyva.Shared.Connectivity.IConnectivityService connectivityService = null,
            [InjectOptional] IGraphicsSettingsService graphicsSettingsService = null,
            [InjectOptional] IServiceModeProfileProvider serviceModeProfileProvider = null)
        {
            _multiplayerState = multiplayerState;
            _gameplaySession = gameplaySession;
            _overlayLoader = overlayLoader;
            _confirmationService = confirmation;
            _connectivityService = connectivityService;
            _graphicsSettingsService = graphicsSettingsService;
            _menuProfile = serviceModeProfileProvider?.Get(ServiceRuntimeMode.Menu) ?? ServiceModeProfileDefaults.Menu;
        }

        public void Initialize()
        {
            // Home menu is a fresh entry point; clear stale cross-scene launch/session state.
            GameLaunchContext.Reset();
            _gameplaySession?.Clear();

            _lifecycleCts?.Cancel();
            _lifecycleCts?.Dispose();
            _lifecycleCts = new CancellationTokenSource();

            ApplyMenuGraphicsPolicy();
            LogVerbose($"Initialize start. overlayNull={_overlayLoader==null}, confirmationNull={_confirmationService==null}, multiplayerStateNull={_multiplayerState==null}, connectivityServiceNull={_connectivityService==null}");

            if (_overlayLoader == null || _confirmationService == null)
            {
                LogErrorWithPrefix("OverlayLoader or ConfirmationService is not available. Proceeding with degraded UX.");
            }

            // Start async initialization without blocking the main thread.
            _ = InitializeAsync(_lifecycleCts.Token);
        }

        public void Dispose()
        {
            _lifecycleCts?.Cancel();
            _lifecycleCts?.Dispose();
            _lifecycleCts = null;
        }

        private async Task InitializeAsync(CancellationToken ct)
        {
            LogVerbose("InitializeAsync started.");

            bool hasInternet = false;
            try
            {
                if (_connectivityService != null)
                {
                    LogVerbose($"Using IConnectivityService.WaitForOnlineAsync (timeout={_menuProfile.ConnectivityWaitTimeout.TotalSeconds:0.#}s)");
                    hasInternet = await _connectivityService.WaitForOnlineAsync(_menuProfile.ConnectivityWaitTimeout);
                    ct.ThrowIfCancellationRequested();
                }
                else
                {
                    LogVerbose("No IConnectivityService available — falling back to InternetChecker probe");
                    hasInternet = await Kruty1918.Moyva.Multiplayer.Core.InternetChecker.HasInternetAsync(5, 3);
                    ct.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException)
            {
                LogWithPrefix("InitializeAsync cancelled.");
                return;
            }
            catch (Exception ex)
            {
                LogWithPrefix($"Internet check failed: {ex.Message}");
            }

            LogVerbose($"Connectivity probe result: {hasInternet}");

            // If multiplayer state is present, try initializing multiplayer regardless of the quick connectivity probe.
            // The multiplayer state will perform its own initialization/auth and report timeout if needed.
            if (_multiplayerState != null)
            {
                LogVerbose("Multiplayer state present — starting multiplayer initialization.");
                StartMultiplayerInitialization(ct);
                return;
            }

            // No multiplayer state available — fall back to offline flow only when we truly do not have connectivity.
            if (!hasInternet)
            {
                LogVerbose("No connectivity detected and no multiplayer state — showing offline dialog.");
                ShowOfflineDialogPanel(ct);
            }
            else
            {
                LogVerbose("Connectivity detected but no multiplayer state — continuing without multiplayer.");
            }
        }

        private void ShowOfflineDialogPanel(CancellationToken ct)
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
                _ = StopOverlayWithDelayAsync(2000, ct);
                return;
            }

            LogWithPrefix("No confirmation or overlay service available; cannot show offline dialog. Continuing without UI.");
        }

        private async void StartMultiplayerInitialization(CancellationToken ct)
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
                        canProceed = await _connectivityService.WaitForOnlineAsync(_menuProfile.ConnectivityQuickProbeTimeout);
                    else
                        canProceed = await Kruty1918.Moyva.Multiplayer.Core.InternetChecker.HasInternetAsync(3, 3);

                    ct.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                    LogWithPrefix("StartMultiplayerInitialization cancelled.");
                    return;
                }
                catch (Exception ex)
                {
                    LogWithPrefix($"Connectivity quick probe failed: {ex.Message}");
                }

                if (!canProceed)
                {
                    ShowOfflineDialogPanel(ct);
                    return;
                }
            }

            // Re-evaluate state (it might have started initializing)
            state = _multiplayerState.ConnectionState;
            float progressValue = state.ConnectionProgress * 100f;
            LogVerbose($"Multiplayer services are not ready yet. Showing overlay and waiting for Unity services + authentication. Progress: {progressValue:0.##}%.");
            var overlayResult = _overlayLoader?.LoadOverlay(progressValue, 100f, "%");
            WaitForMultiplayerReadyAsync(prefix, overlayResult, ct);
        }

        private async void WaitForMultiplayerReadyAsync(string prefix, OverlayLoaderResult overlayResult, CancellationToken ct)
        {
            try
            {
                var progressReporter = new Progress<float>(connectionProgress =>
                {
                    overlayResult?.SetLoading(true, connectionProgress * 100f);
                });

                await _multiplayerState.WaitUntilReadyAsync(progressReporter, ct);
                var last = overlayResult?.Progress ?? 0f;
                overlayResult?.SetLoading(false, last);
                _overlayLoader?.StopOverlay(true);

                string userId = GetAuthenticatedUserId();
                Debug.Log($"{prefix} Multiplayer ready. Authenticated user id: {userId}.");
            }
            catch (OperationCanceledException)
            {
                overlayResult?.SetLoading(false, overlayResult?.Progress ?? 0f);
                _overlayLoader?.StopOverlay();
                Debug.Log($"{prefix} Multiplayer initialization cancelled.");
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

        private async Task StopOverlayWithDelayAsync(int delayMs, CancellationToken ct)
        {
            try
            {
                await Task.Delay(delayMs, ct).ConfigureAwait(false);
                MainThreadDispatcher.Enqueue(() => _overlayLoader?.StopOverlay(true));
            }
            catch (OperationCanceledException)
            {
                // lifecycle cancelled, skip delayed UI work
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
            Debug.Log($"{Prefix} {msg}");
        }

        private void LogErrorWithPrefix(string msg)
        {
            Debug.LogError($"{Prefix} {msg}");
        }

        private void LogVerbose(string msg)
        {
            if (_menuProfile.IsVerboseLogging)
                Debug.Log($"{Prefix} {msg}");
        }

        private void ApplyMenuGraphicsPolicy()
        {
            if (_graphicsSettingsService == null || !_menuProfile.ApplyGraphicsProfile)
                return;

            var current = _graphicsSettingsService.Settings.Profile;
            if (_menuProfile.RespectCustomGraphicsProfile && current == GraphicsQualityProfile.Custom)
                return;

            if (current != _menuProfile.GraphicsProfile)
                _graphicsSettingsService.SetProfile(_menuProfile.GraphicsProfile);
        }
    }
}