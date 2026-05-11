using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime.Services;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.SaveSystem;
using Kruty1918.Moyva.Shared.Common;
using Kruty1918.Moyva.WorldCreation.API;
using UnityEngine;
using Zenject;
using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal class JoinRoomPanelService : IJoinRoomPanelService, IInitializable, IDisposable
    {
        [Inject] private IJoinRoomViewController _viewController;
        [InjectOptional] private ILobbyService _lobbyService;
        [InjectOptional] private IMultiplayerModeSelector _modeSelector;
        [InjectOptional] private SwitchableLobbyService _switchableLobbyService;
        [InjectOptional] private IOverlayLoader _loader;
        [Inject] private IJoinRoomUiGateway _uiGateway;
        [InjectOptional] private ILocalGameSettingsService _localGameSettings;
        [InjectOptional] private IConfirmationService _confirmationService;
        [InjectOptional] private IPasswordPanelService _passwordPanelService;
        [InjectOptional] private IInfoPanelService _infoPanelService;
        [InjectOptional] private INetworkProvider _networkProvider;
        [InjectOptional] private SwitchableNetworkProvider _switchableNetworkProvider;
        [InjectOptional] private IConfigStore _configStore;
        [InjectOptional] private IGameplaySession _gameplaySession;
        [InjectOptional] private IHomeMenuGameStarter _gameStarter;
        [InjectOptional] private IServiceModeProfileProvider _serviceModeProfileProvider;
        [InjectOptional] private IMultiplayerState _multiplayerState;
        [InjectOptional] private IRoomAccessPolicyService _roomAccessPolicy;

        private JoinRoomTransportAdapter _transportAdapter;

        private CancellationTokenSource _roomsCts;
        private CancellationTokenSource _joinCts;
        private Action _onJoinRequested;
        private Action _onListRefreshRequested;
        private Action _onJoinCodeChangedCallback;
        private Action<RoomInfo> _onRoomSelectedCallback;
        private Action<NetworkProviderType> _onModeChangedCallback;
        private Action<LobbyState> _onLobbyStateChangedCallback;
        private bool _isJoining;
        private JoinPipelineState _joinState = JoinPipelineState.Idle;
        private readonly MultiplayerActionRateLimiter _actionRateLimiter = new MultiplayerActionRateLimiter();
        private readonly MultiplayerIdempotencyGuard _idempotencyGuard = new MultiplayerIdempotencyGuard();
        private string _activeJoinOperationKey;

        public string LastJoinPanelName { get; private set; }
        public NetworkProviderType LastJoinProviderType { get; private set; } = NetworkProviderType.Relay;

        public void Dispose()
        {
            try { if (_onJoinRequested != null) _viewController.OnJoinRequested -= _onJoinRequested; } catch { }
            try { if (_onListRefreshRequested != null) _viewController.OnListRoomsRefresh -= _onListRefreshRequested; } catch { }
            try { if (_onJoinCodeChangedCallback != null) _viewController.OnJoinCodeChanged -= _onJoinCodeChangedCallback; } catch { }
            try { if (_onRoomSelectedCallback != null) _viewController.OnRoomSelected -= _onRoomSelectedCallback; } catch { }
            try { if (_modeSelector != null && _onModeChangedCallback != null) _modeSelector.OnModeChanged -= _onModeChangedCallback; } catch { }
            try { if (_lobbyService != null && _onLobbyStateChangedCallback != null) _lobbyService.StateChanged -= _onLobbyStateChangedCallback; } catch { }

            _roomsCts?.Cancel();
            _roomsCts?.Dispose();
            _joinCts?.Cancel();
            _joinCts?.Dispose();
        }

        public void Initialize()
        {
            if (_onJoinRequested != null)
                _viewController.OnJoinRequested -= _onJoinRequested;
            _onJoinRequested = OnJoinClicked;
            _viewController.OnJoinRequested += _onJoinRequested;

            if (_onJoinCodeChangedCallback != null)
                _viewController.OnJoinCodeChanged -= _onJoinCodeChangedCallback;
            _onJoinCodeChangedCallback = () => OnJoinCodeChanged(_viewController.JoinCode);
            _viewController.OnJoinCodeChanged += _onJoinCodeChangedCallback;

            if (_onRoomSelectedCallback != null)
                _viewController.OnRoomSelected -= _onRoomSelectedCallback;
            _onRoomSelectedCallback = room => OnRoomSelected(room);
            _viewController.OnRoomSelected += _onRoomSelectedCallback;

            if (_onListRefreshRequested != null)
                _viewController.OnListRoomsRefresh -= _onListRefreshRequested;
            _onListRefreshRequested = () => _ = RefreshRoomListAsync();
            _viewController.OnListRoomsRefresh += _onListRefreshRequested;

            if (_modeSelector != null)
            {
                if (_onModeChangedCallback != null)
                    _modeSelector.OnModeChanged -= _onModeChangedCallback;
                _onModeChangedCallback = _ => OnProviderModeChanged();
                _modeSelector.OnModeChanged += _onModeChangedCallback;
            }

            if (_lobbyService != null)
            {
                if (_onLobbyStateChangedCallback != null)
                    _lobbyService.StateChanged -= _onLobbyStateChangedCallback;
                _onLobbyStateChangedCallback = OnLobbyStateChanged;
                _lobbyService.StateChanged += _onLobbyStateChangedCallback;
            }

            Refresh();
        }

        public void Refresh()
        {
            OnJoinCodeChanged(_viewController.JoinCode);
        }

        public void RefreshRoomList()
        {
            if (_isJoining)
                return;

            _ = RefreshRoomListAsync();
        }

        public Task<bool> PrepareForOpenAsync(CancellationToken ct = default)
        {
            return RefreshRoomListAsync(ct);
        }

        private JoinRoomTransportAdapter TransportAdapter => _transportAdapter ??=
            new JoinRoomTransportAdapter(_lobbyService, _networkProvider, _switchableNetworkProvider, GetCurrentProviderType, _serviceModeProfileProvider);

        public async Task<bool> RefreshRoomListAsync(CancellationToken externalCt = default)
        {
            if (_isJoining)
                return false;

            _roomsCts?.Cancel();
            _roomsCts?.Dispose();
            _roomsCts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
            _roomsCts.CancelAfter(TimeSpan.FromSeconds(8));
            var ct = _roomsCts.Token;

            OverlayLoaderResult overlay = null;
            await MainThreadDispatcher.EnqueueAsync(() =>
            {
                _viewController.ClearRoomList();
                overlay = _loader?.LoadOverlay(0f, 100f, "%");
            });

            try
            {
                if (_lobbyService == null)
                {
                    Debug.LogError("[JoinRoomPanelService] ILobbyService not available; clearing room list.");
                    return false;
                }

                await ApplySelectedProviderAsync(ct);
                var providerType = GetCurrentProviderType();
                var rooms = await _lobbyService.QueryRoomsAsync(ct);
                ct.ThrowIfCancellationRequested();

                if (GetCurrentProviderType() != providerType)
                    return false;

                var roomInfos = JoinRoomDomainLogic.ProjectRoomInfos(rooms, providerType);
                var populated = false;

                await MainThreadDispatcher.EnqueueAsync(() =>
                {
                    if (GetCurrentProviderType() != providerType)
                        return;

                    _viewController.ClearRoomList();
                    foreach (var roomInfo in roomInfos)
                    {
                        if (string.IsNullOrEmpty(roomInfo.JoinCode) && !string.IsNullOrEmpty(roomInfo.LobbyId))
                        {
                            Debug.LogWarning($"[JoinRoomPanelService] Room '{roomInfo.RoomName}' has empty join code, will try join by lobby ID.");
                        }
                        else if (string.IsNullOrEmpty(roomInfo.JoinCode) && string.IsNullOrEmpty(roomInfo.LobbyId))
                        {
                            Debug.LogWarning($"[JoinRoomPanelService] Room '{roomInfo.RoomName}' has no join code and no lobby ID.");
                        }

                        _viewController.AddRoomToList(roomInfo);
                    }

                    populated = true;
                });

                return populated;
            }
            catch (OperationCanceledException)
            {
                // request was canceled/timeout
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[JoinRoomPanelService] RefreshRoomListAsync failed: {e}");
                return false;
            }
            finally
            {
                try
                {
                    await MainThreadDispatcher.EnqueueAsync(() =>
                    {
                        float progress = overlay != null ? overlay.Progress : 100f;
                        overlay?.SetLoading(false, progress);
                        _loader?.StopOverlay(true);
                    });
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[JoinRoomPanelService] Failed to stop overlay: {e.Message}");
                }
            }
        }

        private void OnProviderModeChanged()
        {
            _roomsCts?.Cancel();
            _joinCts?.Cancel();
            MainThreadDispatcher.Enqueue(() =>
            {
                _viewController.ClearRoomList();
                RefreshRoomList();
            });
        }

        private void OnLobbyStateChanged(LobbyState state)
        {
            if (state != LobbyState.Started && state != LobbyState.Closed)
                return;

            _passwordPanelService?.Cancel();
            _joinCts?.Cancel();
            if (_isJoining)
            {
                _infoPanelService?.Show(new InfoMessage(
                    "Кімната недоступна",
                    state == LobbyState.Started
                        ? "Гру вже розпочато. Оберіть іншу кімнату."
                        : "Кімнату закрито. Оберіть іншу кімнату."));
            }

            RefreshRoomList();
        }

        private async void OnJoinClicked()
        {
            if (_viewController == null) return;
            if (_isJoining) return;
            if (!_actionRateLimiter.Allow("join-click", TimeSpan.FromMilliseconds(700)))
            {
                _infoPanelService?.Show(new InfoMessage("Зачекайте", "Натискання виконується надто часто. Спробуйте ще раз за мить."));
                return;
            }
            if (_lobbyService == null)
            {
                Debug.LogError("[JoinRoomPanelService] ILobbyService not available, cannot join room.");
                return;
            }

            var target = JoinRoomResolver.FromManualInput(_viewController.JoinCode);
            if (!target.IsValid)
            {
                Debug.LogWarning("[JoinRoomPanelService] Join code is empty.");
                return;
            }

            if (!_idempotencyGuard.TryEnter($"join:{target.Kind}:{target.Value}"))
            {
                _infoPanelService?.Show(new InfoMessage("Запит вже виконується", "Повторний Join із тим самим кодом ігноровано."));
                return;
            }

            await JoinRoomAsync(target);
        }

        private void OnJoinCodeChanged(string code)
        {
            // Встановлюємо реверс значення для interactable, 
            // щоб кнопка була активною лише коли код не порожній
            bool interactable = !_isJoining && !string.IsNullOrWhiteSpace(code);
            _viewController.SetJoinInteractable(interactable);
        }

        private void OnRoomSelected(RoomInfo room)
        {
            if (_isJoining)
                return;

            if (room.HasJoinCode)
            {
                _viewController.JoinCode = room.JoinCode;
                OnJoinCodeChanged(_viewController.JoinCode);
            }

            if (_confirmationService == null)
            {
                _ = JoinSelectedRoomAsync(room);
                return;
            }

            _confirmationService.Show(new ConfirmationRequest
            {
                LabelText = "Підтвердження",
                MessageText = $"Увійти в кімнату {room.HostOrRoomDisplayName}?",
                OnConfirm = () => _ = JoinSelectedRoomAsync(room),
                OnCancel = () => { }
            });
        }

        private async Task JoinSelectedRoomAsync(RoomInfo room)
        {
            if (_viewController == null || _lobbyService == null) return;
            if (_isJoining) return;

            var providerType = GetCurrentProviderType();
            if (room.ProviderType != providerType)
            {
                Debug.LogWarning($"[JoinRoomPanelService] Ignoring room '{room.RoomName}' from {room.ProviderType}; current provider is {providerType}.");
                RefreshRoomList();
                return;
            }

            var target = JoinRoomResolver.FromRoom(room);
            if (!target.IsValid)
            {
                Debug.LogWarning($"[JoinRoomPanelService] Selected room '{room.RoomName}' has neither join code nor lobby ID.");
                return;
            }

            await JoinRoomAsync(target);
        }

        private async Task JoinRoomAsync(JoinRoomTarget target)
        {
            if (_isJoining)
                return;

            var traceId = MoyvaId.NewTraceId();
            _joinState = JoinPipelineState.Preflight;
            _activeJoinOperationKey = $"join:{target.Kind}:{target.Value}";

            var preflight = MultiplayerPreflightChecks.ValidateForJoin(
                hasLobbyService: _lobbyService != null,
                hasNetworkProvider: _networkProvider != null,
                hasModeSelector: _modeSelector != null);
            if (preflight.IsFailure)
            {
                var preflightError = MultiplayerUserFacingError.FromDomainError(preflight.Error, traceId);
                _infoPanelService?.Show(new InfoMessage("Preflight failed", preflightError.BuildDisplayMessage()));
                _joinState = JoinPipelineState.Failed;
                _idempotencyGuard.Exit(_activeJoinOperationKey);
                _activeJoinOperationKey = null;
                return;
            }

            var joinPanelName = ResolveJoinOriginPanelName();
            var joinProviderType = GetCurrentProviderType();
            var shouldRefreshRoomListAfterFailure = false;
            _isJoining = true;
            _roomsCts?.Cancel();
            _joinCts?.Cancel();
            _joinCts?.Dispose();
            _joinCts = new CancellationTokenSource();
            _joinCts.CancelAfter(MultiplayerReliabilityPolicy.GetJoinTimeout(joinProviderType));
            var ct = _joinCts.Token;
            MainThreadDispatcher.Enqueue(() => OnJoinCodeChanged(_viewController.JoinCode));

            var overlay = _loader?.LoadOverlay(0f, 100f, "%");
            try
            {
                Debug.Log($"[JoinRoomPanelService] [{traceId}] JoinRoomAsync start: kind={target.Kind} value='{target.Value}' provider={joinProviderType} currentMenu='{_uiGateway?.CurrentMenu}'.");
                _joinState = JoinPipelineState.ResolvingTarget;
                await ApplySelectedProviderAsync(ct);
                Debug.Log($"[JoinRoomPanelService] [{traceId}] JoinRoomAsync provider applied; effective={GetCurrentProviderType()} requested={joinProviderType}.");

                if (_multiplayerState != null)
                    await _multiplayerState.WaitUntilReadyAsync(ct);

                _joinState = JoinPipelineState.JoiningLobby;
                var joinResult = await TryJoinWithPasswordLoopResultAsync(target, ct);
                if (joinResult.IsFailure)
                {
                    if (joinResult.Error.Code == DomainErrorCode.Cancelled)
                        return;

                    shouldRefreshRoomListAfterFailure = true;
                    await ReturnToLobbyChooserWithMessageAsync(
                        joinPanelName,
                        "Помилка приєднання",
                        MultiplayerUserFacingError.FromDomainError(joinResult.Error, traceId).BuildDisplayMessage(),
                        ct);
                    return;
                }

                LobbyRoom room = joinResult.Value;

                if (room != null)
                {
                    Debug.Log($"[JoinRoomPanelService] [{traceId}] JoinRoomAsync lobby join ok: lobbyId='{room.LobbyId}' code='{room.LobbyCode}' relay='{room.RelayJoinCode}' players={room.Players?.Count ?? 0} state={room.State}.");
                    var blockReason = JoinRoomDomainLogic.GetPostJoinBlockReason(room, GetPlayerName(), ResolveReconnectToleranceSeconds());
                    if (!string.IsNullOrEmpty(blockReason))
                    {
                        shouldRefreshRoomListAfterFailure = true;
                        Debug.LogWarning($"[JoinRoomPanelService] [{traceId}] Join blocked after lobby join: {blockReason}");
                        await ReturnToLobbyChooserWithMessageAsync(joinPanelName, "Кімната недоступна", blockReason + "\n\nДія: Оновіть список кімнат.", ct);
                        return;
                    }

                    if (_roomAccessPolicy != null)
                    {
                        var localPlayerId = JoinRoomDomainLogic.ResolveLocalPlayerId(room, GetPlayerName());
                        if (!_roomAccessPolicy.CanJoin(room, localPlayerId, out var policyReason))
                        {
                            shouldRefreshRoomListAfterFailure = true;
                            Debug.LogWarning($"[JoinRoomPanelService] [{traceId}] Join blocked by room access policy: {policyReason}");
                            await ReturnToLobbyChooserWithMessageAsync(joinPanelName, "Доступ заборонено", policyReason + "\n\nДія: Оберіть іншу кімнату.", ct);
                        return;
                        }
                    }

                    _joinState = JoinPipelineState.ConnectingTransport;
                    var transportResult = await TransportAdapter.JoinNetworkSessionAsync(room, traceId, ct);
                    if (transportResult.IsFailure)
                    {
                        if (await TryFallbackTransportAsync(traceId, room, ct))
                        {
                            _joinState = JoinPipelineState.Ready;
                            RememberJoinOrigin(joinPanelName, GetCurrentProviderType());
                            MainThreadDispatcher.Enqueue(() =>
                            {
                                var inviteCode = !string.IsNullOrWhiteSpace(room.LobbyCode) ? room.LobbyCode : room.LobbyId;
                                _uiGateway?.OpenLobbyPanel(inviteCode);
                            });
                            return;
                        }

                        shouldRefreshRoomListAfterFailure = true;
                        Debug.LogWarning($"[JoinRoomPanelService] [{traceId}] Join transport phase failed for lobby '{room.LobbyId}'.");
                        await ReturnToLobbyChooserWithMessageAsync(
                            joinPanelName,
                            "Помилка приєднання",
                            MultiplayerUserFacingError.FromDomainError(transportResult.Error, traceId).BuildDisplayMessage(),
                            ct);
                        return;
                    }

                    RememberJoinOrigin(joinPanelName, joinProviderType);

                    if (room.State == LobbyState.Started)
                    {
                        bool reconnected = await StartReconnectedGameAsync(room, ct);
                        if (!reconnected)
                        {
                            shouldRefreshRoomListAfterFailure = true;
                            Debug.LogWarning($"[JoinRoomPanelService] [{traceId}] Reconnect flow failed: started room has no valid world settings.");
                            await ReturnToLobbyChooserWithMessageAsync(
                                joinPanelName,
                                "Не вдалося перепідключитися",
                                "Кімната вже у грі, але не містить валідних налаштувань світу для перепідключення.",
                                ct);
                        }
                        return;
                    }

                    MainThreadDispatcher.Enqueue(() =>
                    {
                        var inviteCode = !string.IsNullOrWhiteSpace(room.LobbyCode) ? room.LobbyCode : room.LobbyId;
                        _uiGateway?.OpenLobbyPanel(inviteCode);
                    });
                    _joinState = JoinPipelineState.Ready;
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log($"[JoinRoomPanelService] [{traceId}] JoinRoomAsync canceled.");
                _joinState = JoinPipelineState.Failed;
            }
            catch (RoomFullException ex)
            {
                Debug.LogWarning($"[JoinRoomPanelService] [{traceId}] Room full: {ex.Message}");
                shouldRefreshRoomListAfterFailure = true;
                await ReturnToLobbyChooserWithMessageAsync(joinPanelName, "Кімната переповнена",
                    new MultiplayerUserFacingError(ex.ErrorCode, ex.Message, "Оберіть іншу кімнату.", traceId).BuildDisplayMessage(), ct);
                _joinState = JoinPipelineState.Failed;
            }
            catch (RoomAccessDeniedException ex)
            {
                Debug.LogWarning($"[JoinRoomPanelService] [{traceId}] Access denied: {ex.Reason}");
                shouldRefreshRoomListAfterFailure = true;
                await ReturnToLobbyChooserWithMessageAsync(joinPanelName, "Доступ заборонено",
                    new MultiplayerUserFacingError(ex.ErrorCode, ex.Message, "Зверніться до організатора кімнати.", traceId).BuildDisplayMessage(), ct);
                _joinState = JoinPipelineState.Failed;
            }
            catch (SessionExpiredException ex)
            {
                Debug.LogWarning($"[JoinRoomPanelService] [{traceId}] Session expired: {ex.Message}");
                shouldRefreshRoomListAfterFailure = true;
                await ReturnToLobbyChooserWithMessageAsync(joinPanelName, "Сесія застаріла",
                    new MultiplayerUserFacingError(ex.ErrorCode, ex.Message, "Оновіть список кімнат.", traceId).BuildDisplayMessage(), ct);
                _joinState = JoinPipelineState.Failed;
            }
            catch (MultiplayerDomainException ex)
            {
                Debug.LogError($"[JoinRoomPanelService] [{traceId}] Domain error [{ex.ErrorCode}]: {ex.Message}");
                shouldRefreshRoomListAfterFailure = true;
                await ReturnToLobbyChooserWithMessageAsync(joinPanelName, "Помилка приєднання",
                    new MultiplayerUserFacingError(ex.ErrorCode, ex.Message, "Перевірте мережу і повторіть спробу.", traceId).BuildDisplayMessage(), ct);
                _joinState = JoinPipelineState.Failed;
            }
            catch (Exception e)
            {
                Debug.LogError($"[JoinRoomPanelService] [{traceId}] JoinRoomAsync failed: {e}");
                shouldRefreshRoomListAfterFailure = true;
                await ReturnToLobbyChooserWithMessageAsync(
                    joinPanelName,
                    "Помилка приєднання",
                    new MultiplayerUserFacingError("MP-JOIN-500", BuildJoinFailureMessage(e), "Перевірте мережу і повторіть спробу.", traceId).BuildDisplayMessage(),
                    ct);
                _joinState = JoinPipelineState.Failed;
            }
            finally
            {
                _isJoining = false;
                MainThreadDispatcher.Enqueue(() =>
                {
                    _loader?.StopOverlay(true);
                    OnJoinCodeChanged(_viewController.JoinCode);
                });

                if (shouldRefreshRoomListAfterFailure)
                    _ = RefreshRoomListAsync();

                _joinCts?.Dispose();
                _joinCts = null;
                _idempotencyGuard.Exit(_activeJoinOperationKey);
                _activeJoinOperationKey = null;
                if (_joinState != JoinPipelineState.Ready)
                    _joinState = JoinPipelineState.Idle;
            }
        }

        private async Task<bool> TryFallbackTransportAsync(string traceId, LobbyRoom room, CancellationToken ct)
        {
            if (_modeSelector == null)
                return false;

            var current = GetCurrentProviderType();
            var fallback = current == NetworkProviderType.Relay
                ? NetworkProviderType.Lan
                : (current == NetworkProviderType.Lan ? NetworkProviderType.Offline : NetworkProviderType.Offline);

            if (fallback == current)
                return false;

            Debug.LogWarning($"[JoinRoomPanelService] [{traceId}] Transport fallback: {current} -> {fallback}");
            await _modeSelector.SetModeAsync(fallback, ct);
            var retry = await TransportAdapter.JoinNetworkSessionAsync(room, traceId, ct);
            return retry.IsSuccess;
        }

        private Task ApplySelectedProviderAsync(CancellationToken ct = default)
        {
            if (_modeSelector == null)
                return Task.CompletedTask;

            return _modeSelector.SetModeAsync(_modeSelector.CurrentMode, ct);
        }

        private async Task<bool> StartReconnectedGameAsync(LobbyRoom room, CancellationToken ct)
        {
            if (room == null || room.StartedWorldSettingsBytes == null || room.StartedWorldSettingsBytes.Length == 0)
                return false;

            if (!WorldSettingsDto.TryFromBytes(room.StartedWorldSettingsBytes, out var worldSettings))
                return false;

            var localId = JoinRoomDomainLogic.ResolveLocalPlayerId(room, GetPlayerName());
            var mode = GetCurrentProviderType();
            _gameplaySession?.Apply(mode, worldSettings, MultiplayerRoomLifecycle.ProjectGameplayPlayers(room, localId), localId);
            GameLaunchContext.ConfigureMenuMultiplayerGame(
                worldSettings.WorldName,
                worldSettings.Seed,
                worldSettings.Size,
                (int)worldSettings.MapType,
                (int)worldSettings.Difficulty,
                worldSettings.MaxPlayers,
                worldSettings.IsPrivate,
                worldSettings.Width,
                worldSettings.Height);

            if (_gameStarter != null)
                await _gameStarter.StartGameAsync(ct);

            return true;
        }

        private float ResolveReconnectToleranceSeconds()
        {
            try
            {
                return _configStore?.Load()?.ReconnectLocalTimeToleranceSeconds ?? 120f;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[JoinRoomPanelService] Failed to load reconnect tolerance: {e.Message}");
                return 120f;
            }
        }

        /// <summary>
        /// Виконує приєднання з підтримкою кімнат із паролем. Якщо кімната захищена паролем —
        /// показує <see cref="IPasswordPanelService"/>; при невірному паролі повторює запит з повідомленням про помилку.
        /// </summary>
        private async Task<Result<LobbyRoom>> TryJoinWithPasswordLoopResultAsync(JoinRoomTarget target, CancellationToken ct)
        {
            // 1) Спочатку — швидка спроба без пароля. Для UGS це поверне room з PasswordHash != "" якщо приватна,
            //    тоді ми залишимо лобі та запитаємо пароль. Для LAN ми вже маємо PasswordHash у кеші discovered rooms.
            var probe = await TryProbeRoomForPasswordAsync(target, ct);
            if (!probe.RequiresPassword)
            {
                var joinedWithoutPassword = await JoinTargetResultAsync(target, null, ct);
                if (joinedWithoutPassword.IsFailure)
                    return joinedWithoutPassword;

                var room = joinedWithoutPassword.Value;

                if (!room.HasPassword)
                    return joinedWithoutPassword;

                try { await _lobbyService.LeaveAsync(ct); }
                catch (Exception e) { Debug.LogWarning($"[JoinRoomPanelService] Leave before password retry failed: {e.Message}"); }

                probe = new ProbeResult(true, room.Name);
            }

            if (_passwordPanelService == null)
            {
                Debug.LogWarning("[JoinRoomPanelService] Кімната потребує пароль, але IPasswordPanelService не підключений.");
                _infoPanelService?.Show(new InfoMessage("Приватна кімната", "Ця кімната потребує пароль, але панель введення недоступна."));
                return Result<LobbyRoom>.Fail(DomainErrorCode.Validation, "Панель введення пароля недоступна.");
            }

            string error = null;
            for (int attempt = 0; attempt < 5; attempt++)
            {
                var prompt = await _passwordPanelService.RequestPasswordAsync(probe.DisplayName, error, ct);
                if (!prompt.Confirmed)
                    return Result<LobbyRoom>.Fail(DomainErrorCode.Cancelled, "Користувач скасував введення пароля.");

                var room = await JoinTargetResultAsync(target, prompt.Password, ct);
                if (room.IsSuccess)
                    return room;

                if (room.Error.Code == DomainErrorCode.WrongPassword)
                {
                    error = "Невірний пароль. Спробуйте ще раз.";
                    continue;
                }

                return room;
            }

            return Result<LobbyRoom>.Fail(DomainErrorCode.WrongPassword, "Невірний пароль кімнати.");
        }

        private async Task<Result<LobbyRoom>> JoinTargetResultAsync(JoinRoomTarget target, string password = null, CancellationToken ct = default)
        {
            if (!target.IsValid)
                return Result<LobbyRoom>.Fail(DomainErrorCode.Validation, "Ціль приєднання невалідна.");

            var room = await JoinExactTargetAsync(target, password, ct);
            if (room.IsSuccess || target.Kind != JoinRoomTargetKind.JoinCode)
                return room;

            var resolved = await ResolveJoinCodeAliasAsync(target.Value, ct);
            if (!resolved.IsValid ||
                (resolved.Kind == target.Kind && string.Equals(resolved.Value, target.Value, StringComparison.OrdinalIgnoreCase)))
            {
                return room;
            }

            Debug.LogWarning($"[JoinRoomPanelService] Join by code '{target.Value}' returned null; retrying as {resolved.Kind}='{resolved.Value}'.");
            return await JoinExactTargetAsync(resolved, password, ct);
        }

        private async Task<Result<LobbyRoom>> JoinExactTargetAsync(JoinRoomTarget target, string password, CancellationToken ct)
        {
            try
            {
                if (target.Kind == JoinRoomTargetKind.LobbyId)
                    return await JoinByIdWithOptionalPasswordAsync(target.Value, password, ct);

                LobbyRoom room;
                if (string.IsNullOrEmpty(password))
                    room = await _lobbyService.JoinByCodeAsync(target.Value, GetPlayerName(), ct);
                else
                    room = await _lobbyService.JoinByCodeWithPasswordAsync(target.Value, GetPlayerName(), password, ct);

                if (room == null)
                {
                    return Result<LobbyRoom>.Fail(
                        DomainErrorCode.NotFound,
                        $"Кімнату '{target.Value}' не знайдено або вона недоступна.");
                }

                return Result<LobbyRoom>.Success(room);
            }
            catch (WrongPasswordException ex)
            {
                return Result<LobbyRoom>.Fail(DomainErrorCode.WrongPassword, ex.Message);
            }
            catch (RoomFullException ex)
            {
                return Result<LobbyRoom>.Fail(DomainErrorCode.RoomFull, ex.Message);
            }
            catch (RoomAccessDeniedException ex)
            {
                return Result<LobbyRoom>.Fail(DomainErrorCode.PermissionDenied, ex.Message);
            }
            catch (SessionExpiredException ex)
            {
                return Result<LobbyRoom>.Fail(DomainErrorCode.SessionExpired, ex.Message);
            }
        }

        private async Task<Result<LobbyRoom>> JoinByIdWithOptionalPasswordAsync(string lobbyId, string password, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(lobbyId))
                return Result<LobbyRoom>.Fail(DomainErrorCode.Validation, "LobbyId порожній.");

            var normalizedLobbyId = lobbyId.Trim();
            Debug.Log($"[JoinRoomPanelService] JoinByIdWithOptionalPasswordAsync: calling JoinByIdAsync('{normalizedLobbyId}')...");
            var room = await _lobbyService.JoinByIdAsync(normalizedLobbyId, GetPlayerName(), ct);
            Debug.Log($"[JoinRoomPanelService] JoinByIdWithOptionalPasswordAsync: JoinByIdAsync returned {(room == null ? "null" : $"room '{room.LobbyId}'")}.");
            if (room == null)
            {
                var fallback = await ResolveJoinCodeAliasAsync(normalizedLobbyId, ct);
                if (fallback.IsValid &&
                    !(fallback.Kind == JoinRoomTargetKind.LobbyId && string.Equals(fallback.Value, normalizedLobbyId, StringComparison.OrdinalIgnoreCase)))
                {
                    Debug.LogWarning($"[JoinRoomPanelService] JoinByIdAsync returned null for lobbyId='{normalizedLobbyId}', retrying via {fallback.Kind}='{fallback.Value}'.");
                    var fallbackResult = await JoinExactTargetAsync(fallback, password, ct);
                    if (fallbackResult.IsFailure)
                        return fallbackResult;

                    room = fallbackResult.Value;
                }
                else
                {
                    Debug.LogWarning($"[JoinRoomPanelService] JoinByIdAsync returned null for lobbyId='{normalizedLobbyId}', fallback={fallback.Kind}/'{fallback.Value}' isValid={fallback.IsValid} — giving up.");
                }

                if (room == null)
                {
                    return Result<LobbyRoom>.Fail(
                        DomainErrorCode.NotFound,
                        $"Кімнату з lobbyId '{normalizedLobbyId}' не знайдено або вона закрита.");
                }
            }

            if (!string.IsNullOrEmpty(password) && room.HasPassword && !LobbyPasswordHasher.Verify(password, room.PasswordHash))
            {
                try { await _lobbyService.LeaveAsync(ct); } catch { }
                return Result<LobbyRoom>.Fail(DomainErrorCode.WrongPassword, "Невірний пароль кімнати.");
            }

            return Result<LobbyRoom>.Success(room);
        }

        private async Task<JoinRoomTarget> ResolveJoinCodeAliasAsync(string value, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new JoinRoomTarget(JoinRoomTargetKind.None, string.Empty);

            try
            {
                var rooms = await _lobbyService.QueryRoomsAsync(ct);
                if (rooms == null)
                    return new JoinRoomTarget(JoinRoomTargetKind.None, string.Empty);

                foreach (var room in rooms)
                {
                    if (room == null)
                        continue;

                    if (string.Equals(room.LobbyId, value, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrWhiteSpace(room.LobbyCode))
                            return new JoinRoomTarget(JoinRoomTargetKind.JoinCode, room.LobbyCode.Trim());

                        if (!string.IsNullOrWhiteSpace(room.RelayJoinCode))
                            return new JoinRoomTarget(JoinRoomTargetKind.JoinCode, room.RelayJoinCode.Trim());

                        if (!string.IsNullOrWhiteSpace(room.LobbyId))
                            return new JoinRoomTarget(JoinRoomTargetKind.LobbyId, room.LobbyId.Trim());
                    }

                    if (string.Equals(room.RelayJoinCode, value, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrWhiteSpace(room.LobbyId))
                            return new JoinRoomTarget(JoinRoomTargetKind.LobbyId, room.LobbyId.Trim());

                        if (!string.IsNullOrWhiteSpace(room.LobbyCode))
                            return new JoinRoomTarget(JoinRoomTargetKind.JoinCode, room.LobbyCode.Trim());
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[JoinRoomPanelService] ResolveJoinCodeAliasAsync failed for '{value}': {e.Message}");
            }

            return new JoinRoomTarget(JoinRoomTargetKind.None, string.Empty);
        }

        private async Task ReturnToLobbyChooserWithMessageAsync(string joinPanelName, string title, string message, CancellationToken ct)
        {
            _passwordPanelService?.Cancel();

            try
            {
                if (_networkProvider != null)
                    await _networkProvider.LeaveSessionAsync(ct);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[JoinRoomPanelService] LeaveSessionAsync after join failure failed: {e.Message}");
            }

            try
            {
                if (_lobbyService != null)
                    await _lobbyService.LeaveAsync(ct);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[JoinRoomPanelService] LeaveAsync after join failure failed: {e.Message}");
            }

            await MainThreadDispatcher.EnqueueAsync(() =>
            {
                _uiGateway?.OpenJoinPanelForce(joinPanelName);
            });

            _infoPanelService?.Show(new InfoMessage(title, string.IsNullOrWhiteSpace(message) ? "Дія: Оновіть список кімнат." : message));
        }

        private static string BuildJoinFailureMessage(Exception exception)
        {
            if (exception == null)
                return "Не вдалося приєднатися до лобі. Спробуйте ще раз.";

            var message = exception.Message;
            if (string.IsNullOrWhiteSpace(message))
                return "Не вдалося приєднатися до лобі. Спробуйте ще раз.";

            if (message.IndexOf("relay", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Не вдалося отримати або використати Relay-код кімнати. Оновіть список лобі й спробуйте ще раз.";

            if (message.IndexOf("lan", StringComparison.OrdinalIgnoreCase) >= 0)
                return "Не вдалося підключитися до LAN-сесії. Переконайтеся, що хост ще в кімнаті, і спробуйте ще раз.";

            return message;
        }

        private static string BuildJoinFailureMessage(DomainError error)
        {
            if (error.IsNone)
                return "Не вдалося приєднатися до лобі. Спробуйте ще раз.";

            if (error.Code == DomainErrorCode.WrongPassword)
                return "Невірний пароль кімнати. Спробуйте ще раз.";

            if (error.Code == DomainErrorCode.NotFound)
                return "Кімнату не знайдено або вона вже недоступна. Оновіть список і спробуйте ще раз.";

            if (error.Code == DomainErrorCode.Validation)
                return string.IsNullOrWhiteSpace(error.Message)
                    ? "Невалідні дані для приєднання. Перевірте параметри й повторіть спробу."
                    : error.Message;

            return string.IsNullOrWhiteSpace(error.Message)
                ? "Не вдалося приєднатися до лобі. Спробуйте ще раз."
                : error.Message;
        }

        private readonly struct ProbeResult
        {
            public bool RequiresPassword { get; }
            public string DisplayName { get; }
            public ProbeResult(bool requiresPassword, string displayName)
            {
                RequiresPassword = requiresPassword;
                DisplayName = displayName ?? string.Empty;
            }
        }

        /// <summary>
        /// Перевіряє за списком кімнат (без приєднання), чи потребує обрана кімната пароль.
        /// Це робиться через дані останнього QueryRoomsAsync — точно для LAN, для UGS — best-effort.
        /// </summary>
        private async Task<ProbeResult> TryProbeRoomForPasswordAsync(JoinRoomTarget target, CancellationToken ct)
        {
            try
            {
            var rooms = await _lobbyService.QueryRoomsAsync(ct);
                if (rooms != null)
                {
                    foreach (var r in rooms)
                    {
                        if (r == null) continue;
                        bool match = string.Equals(r.LobbyCode, target.Value, StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(r.LobbyId, target.Value, StringComparison.OrdinalIgnoreCase) ||
                                     string.Equals(r.RelayJoinCode, target.Value, StringComparison.OrdinalIgnoreCase);
                        if (match)
                            return new ProbeResult(r.HasPassword, r.Name);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[JoinRoomPanelService] TryProbeRoomForPasswordAsync failed: {e.Message}");
            }
            return new ProbeResult(false, target.Value);
        }

        private string ResolveJoinOriginPanelName()
        {
            return _uiGateway?.ResolveJoinOriginPanelName(LastJoinPanelName) ?? LastJoinPanelName;
        }

        private void RememberJoinOrigin(string panelName, NetworkProviderType providerType)
        {
            if (!string.IsNullOrWhiteSpace(panelName))
                LastJoinPanelName = panelName;
            LastJoinProviderType = providerType;
        }

        private NetworkProviderType GetCurrentProviderType()
        {
            if (_switchableLobbyService != null)
                return _switchableLobbyService.CurrentProviderType;

            if (_lobbyService is SwitchableLobbyService switchableLobbyService)
                return switchableLobbyService.CurrentProviderType;

            if (_modeSelector != null)
                return _modeSelector.EffectiveMode;

            return NetworkProviderType.Relay;
        }

        private string GetPlayerName()
        {
            return string.IsNullOrWhiteSpace(_localGameSettings?.PlayerName)
                ? "Player"
                : _localGameSettings.PlayerName;
        }

    }
}