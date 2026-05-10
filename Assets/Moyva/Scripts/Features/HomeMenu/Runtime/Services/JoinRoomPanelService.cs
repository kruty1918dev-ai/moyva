using System;
using System.Collections.Generic;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime.Services;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.SaveSystem;
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
        [Inject] private INavigation _navigation;
        [Inject] private ILobbyPanelViewController _lobbyPanelViewController;
        [Inject(Id = "LobbyPanelName")] private string _lobbyPanelName;
        [Inject(Id = "JoinRoomPanelName")] private string _joinRoomPanelName;
        [InjectOptional] private ILocalGameSettingsService _localGameSettings;
        [InjectOptional] private IConfirmationService _confirmationService;
        [InjectOptional] private IPasswordPanelService _passwordPanelService;
        [InjectOptional] private IInfoPanelService _infoPanelService;
        [InjectOptional] private INetworkProvider _networkProvider;
        [InjectOptional] private SwitchableNetworkProvider _switchableNetworkProvider;
        [InjectOptional] private IConfigStore _configStore;
        [InjectOptional] private IGameplaySession _gameplaySession;
        [InjectOptional] private IHomeMenuGameStarter _gameStarter;

        private CancellationTokenSource _roomsCts;
        private Action _onListRefreshRequested;
        private Action _onJoinCodeChangedCallback;
        private Action<RoomInfo> _onRoomSelectedCallback;
        private Action<NetworkProviderType> _onModeChangedCallback;
        private Action<LobbyState> _onLobbyStateChangedCallback;
        private bool _isJoining;

        public string LastJoinPanelName { get; private set; }
        public NetworkProviderType LastJoinProviderType { get; private set; } = NetworkProviderType.Relay;

        public void Dispose()
        {
            try { if (_onListRefreshRequested != null) _viewController.OnListRoomsRefresh -= _onListRefreshRequested; } catch { }
            try { if (_onJoinCodeChangedCallback != null) _viewController.OnJoinCodeChanged -= _onJoinCodeChangedCallback; } catch { }
            try { if (_onRoomSelectedCallback != null) _viewController.OnRoomSelected -= _onRoomSelectedCallback; } catch { }
            try { if (_modeSelector != null && _onModeChangedCallback != null) _modeSelector.OnModeChanged -= _onModeChangedCallback; } catch { }
            try { if (_lobbyService != null && _onLobbyStateChangedCallback != null) _lobbyService.StateChanged -= _onLobbyStateChangedCallback; } catch { }

            try
            {
                if (_viewController.JoinToRoomButton != null)
                    _viewController.JoinToRoomButton.onClick.RemoveListener(OnJoinClicked);
            }
            catch { }

            _roomsCts?.Cancel();
            _roomsCts?.Dispose();
        }

        public void Initialize()
        {
            if (_viewController.JoinToRoomButton != null)
            {
                _viewController.JoinToRoomButton.onClick.RemoveListener(OnJoinClicked);
                _viewController.JoinToRoomButton.onClick.AddListener(OnJoinClicked);
            }

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

                var roomInfos = ProjectRoomInfos(rooms, providerType);
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

            await JoinRoomAsync(target);
        }

        private void OnJoinCodeChanged(string code)
        {
            // Встановлюємо реверс значення для interactable, 
            // щоб кнопка була активною лише коли код не порожній
            bool interactable = !_isJoining && !string.IsNullOrWhiteSpace(code);

            if (_viewController.JoinToRoomButton != null)
                _viewController.JoinToRoomButton.interactable = interactable;
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

            var traceId = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();

            var joinPanelName = ResolveJoinOriginPanelName();
            var joinProviderType = GetCurrentProviderType();
            var shouldRefreshRoomListAfterFailure = false;
            _isJoining = true;
            _roomsCts?.Cancel();
            MainThreadDispatcher.Enqueue(() => OnJoinCodeChanged(_viewController.JoinCode));

            var overlay = _loader?.LoadOverlay(0f, 100f, "%");
            try
            {
                Debug.Log($"[JoinRoomPanelService] [{traceId}] JoinRoomAsync start: kind={target.Kind} value='{target.Value}' provider={joinProviderType} currentMenu='{_navigation?.CurrentMenu}'.");
                await ApplySelectedProviderAsync();
                Debug.Log($"[JoinRoomPanelService] [{traceId}] JoinRoomAsync provider applied; effective={GetCurrentProviderType()} requested={joinProviderType}.");

                LobbyRoom room = await TryJoinWithPasswordLoopAsync(target);

                if (room != null)
                {
                    Debug.Log($"[JoinRoomPanelService] [{traceId}] JoinRoomAsync lobby join ok: lobbyId='{room.LobbyId}' code='{room.LobbyCode}' relay='{room.RelayJoinCode}' players={room.Players?.Count ?? 0} state={room.State}.");
                    var blockReason = GetPostJoinBlockReason(room);
                    if (!string.IsNullOrEmpty(blockReason))
                    {
                        shouldRefreshRoomListAfterFailure = true;
                        Debug.LogWarning($"[JoinRoomPanelService] [{traceId}] Join blocked after lobby join: {blockReason}");
                        await ReturnToLobbyChooserWithMessageAsync(joinPanelName, "Кімната недоступна", blockReason);
                        return;
                    }

                    var transportReady = await JoinNetworkSessionAsync(room, traceId);
                    if (!transportReady)
                    {
                        shouldRefreshRoomListAfterFailure = true;
                        Debug.LogWarning($"[JoinRoomPanelService] [{traceId}] Join transport phase failed for lobby '{room.LobbyId}'.");
                        await ReturnToLobbyChooserWithMessageAsync(
                            joinPanelName,
                            "Помилка приєднання",
                            "Не вдалося підключитися до мережевої сесії. Оберіть іншу кімнату або спробуйте ще раз.");
                        return;
                    }

                    RememberJoinOrigin(joinPanelName, joinProviderType);

                    if (room.State == LobbyState.Started)
                    {
                        bool reconnected = await StartReconnectedGameAsync(room);
                        if (!reconnected)
                        {
                            shouldRefreshRoomListAfterFailure = true;
                            Debug.LogWarning($"[JoinRoomPanelService] [{traceId}] Reconnect flow failed: started room has no valid world settings.");
                            await ReturnToLobbyChooserWithMessageAsync(
                                joinPanelName,
                                "Не вдалося перепідключитися",
                                "Кімната вже у грі, але не містить валідних налаштувань світу для перепідключення.");
                        }
                        return;
                    }

                    MainThreadDispatcher.Enqueue(() =>
                    {
                        var inviteCode = !string.IsNullOrWhiteSpace(room.LobbyCode) ? room.LobbyCode : room.LobbyId;
                        try { _lobbyPanelViewController?.SetLobbyInvateCode(inviteCode); } catch { }
                        try { _navigation?.Open(_lobbyPanelName); } catch (Exception navEx) { Debug.LogError($"[JoinRoomPanelService] Navigation.Open('{_lobbyPanelName}') failed: {navEx.Message}"); }
                    });
                }
                else
                {
                    Debug.LogError($"[JoinRoomPanelService] [{traceId}] JoinRoomAsync failed to join by {target.Kind}='{target.Value}': result was null.");
                    shouldRefreshRoomListAfterFailure = true;
                    await ReturnToLobbyChooserWithMessageAsync(
                        joinPanelName,
                        "Помилка приєднання",
                        "Не вдалося приєднатися до лобі. Оновіть список кімнат і спробуйте ще раз.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[JoinRoomPanelService] [{traceId}] JoinRoomAsync failed: {e}");
                shouldRefreshRoomListAfterFailure = true;
                await ReturnToLobbyChooserWithMessageAsync(
                    joinPanelName,
                    "Помилка приєднання",
                    BuildJoinFailureMessage(e));
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
            }
        }

        private Task ApplySelectedProviderAsync(CancellationToken ct = default)
        {
            if (_modeSelector == null)
                return Task.CompletedTask;

            return _modeSelector.SetModeAsync(_modeSelector.CurrentMode, ct);
        }

        /// <summary>
        /// Після lobby-join підключає transport provider до реальної сесії (LAN/Relay/etc.).
        /// Саме цей крок потрібен, щоб знайдена кімната перетворилась на активне мережеве підключення.
        /// </summary>
        private async Task<bool> JoinNetworkSessionAsync(LobbyRoom room, string traceId)
        {
            if (_networkProvider == null)
            {
                Debug.LogWarning($"[JoinRoomPanelService] [{traceId}] INetworkProvider not available; lobby joined without transport connection.");
                return true;
            }

            var providerType = GetCurrentProviderType();
            if (!await EnsureNetworkProviderMatchesLobbyAsync(providerType))
                return false;

            var joinCode = await ResolveNetworkJoinCodeAsync(room, traceId);
            if (string.IsNullOrWhiteSpace(joinCode))
            {
                const string error = "Хост ще не опублікував мережевий код підключення для цієї кімнати.";
                Debug.LogError($"[JoinRoomPanelService] [{traceId}] {error} LobbyId='{room?.LobbyId}' LobbyCode='{room?.LobbyCode}' Provider='{providerType}'.");
                await LeaveLobbyAfterFailedTransportJoinAsync(traceId);
                return false;
            }

            var normalizedJoinCode = joinCode.Trim();
            if (providerType == NetworkProviderType.Relay)
            {
                if (!RelayJoinCodeUtility.IsValid(normalizedJoinCode))
                {
                    Debug.LogError($"[JoinRoomPanelService] [{traceId}] Resolved transport code '{normalizedJoinCode}' is not a valid Relay join code. LobbyId='{room?.LobbyId}', LobbyCode='{room?.LobbyCode}', RelayJoinCode='{room?.RelayJoinCode}'.");
                    await LeaveLobbyAfterFailedTransportJoinAsync(traceId);
                    return false;
                }
            }

            Debug.Log($"[JoinRoomPanelService] [{traceId}] Joining transport session '{normalizedJoinCode}' using provider '{providerType}'.");
            var result = await _networkProvider.JoinSessionAsync(normalizedJoinCode);
            if (result == null || !result.Success)
            {
                var error = result?.ErrorMessage ?? "Не вдалося підключитися до мережевої сесії.";
                Debug.LogError($"[JoinRoomPanelService] [{traceId}] JoinSessionAsync failed: {error}");
                await LeaveLobbyAfterFailedTransportJoinAsync(traceId);
                return false;
            }

            return true;
        }

        private async Task LeaveLobbyAfterFailedTransportJoinAsync(string traceId)
        {
            try { if (_lobbyService != null) await _lobbyService.LeaveAsync(); }
            catch (Exception e) { Debug.LogWarning($"[JoinRoomPanelService] [{traceId}] Leave after failed transport join failed: {e.Message}"); }
        }

        private async Task<bool> StartReconnectedGameAsync(LobbyRoom room)
        {
            if (room == null || room.StartedWorldSettingsBytes == null || room.StartedWorldSettingsBytes.Length == 0)
                return false;

            if (!WorldSettingsDto.TryFromBytes(room.StartedWorldSettingsBytes, out var worldSettings))
                return false;

            var localId = ResolveLocalPlayerId(room);
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
                await _gameStarter.StartGameAsync();

            return true;
        }

        private string GetPostJoinBlockReason(LobbyRoom room)
        {
            if (room == null)
                return "Кімната недоступна.";

            if (room.State == LobbyState.Closed)
                return "Кімната вже закрита.";

            if (room.State == LobbyState.Started && !MultiplayerRoomLifecycle.IsReconnectAllowed(room, GetPlayerName(), ResolveReconnectToleranceSeconds()))
                return "Гра вже запущена. Приєднання доступне лише для перепідключення з тим самим ніком і коректним локальним часом.";

            return null;
        }

        private string ResolveLocalPlayerId(LobbyRoom room)
        {
            var playerName = GetPlayerName();
            if (room?.Players != null)
            {
                foreach (var player in room.Players)
                {
                    if (!string.IsNullOrWhiteSpace(player.PlayerId) && string.Equals(player.DisplayName, playerName, StringComparison.OrdinalIgnoreCase))
                        return player.PlayerId;
                }

                foreach (var player in room.Players)
                {
                    if (!string.IsNullOrWhiteSpace(player.PlayerId) && !player.IsHost)
                        return player.PlayerId;
                }
            }

            return string.Empty;
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

        private async Task<bool> EnsureNetworkProviderMatchesLobbyAsync(NetworkProviderType providerType)
        {
            if (providerType == NetworkProviderType.Offline)
                return true;

            var switchable = _switchableNetworkProvider ?? _networkProvider as SwitchableNetworkProvider;
            if (switchable == null)
            {
                Debug.LogWarning($"[JoinRoomPanelService] Network provider is not switchable; cannot force transport provider {providerType}.");
                return true;
            }

            if (switchable.CurrentType == providerType)
                return true;

            try
            {
                await switchable.SwitchToAsync(providerType);
                Debug.Log($"[JoinRoomPanelService] Transport provider switched to {switchable.CurrentType} before lobby join transport step.");
                return switchable.CurrentType == providerType;
            }
            catch (Exception e)
            {
                Debug.LogError($"[JoinRoomPanelService] Failed to switch transport provider to {providerType}: {e.Message}");
                return false;
            }
        }

        private async Task<string> ResolveNetworkJoinCodeAsync(LobbyRoom room, string traceId)
        {
            var providerType = GetCurrentProviderType();
            if (TryNormalizeTransportJoinCode(room?.RelayJoinCode, providerType, out var initialJoinCode))
                return initialJoinCode;

            var deadline = DateTime.UtcNow.AddSeconds(15);
            while (DateTime.UtcNow < deadline)
            {
                var current = _lobbyService?.Current;
                if (TryNormalizeTransportJoinCode(current?.RelayJoinCode, providerType, out var currentJoinCode))
                    return currentJoinCode;

                try
                {
                    var rooms = await _lobbyService.QueryRoomsAsync();
                    if (rooms != null)
                    {
                        foreach (var candidate in rooms)
                        {
                            if (candidate == null) continue;
                            var sameLobby = string.Equals(candidate.LobbyId, room.LobbyId, StringComparison.OrdinalIgnoreCase) ||
                                            string.Equals(candidate.LobbyCode, room.LobbyCode, StringComparison.OrdinalIgnoreCase);
                            if (sameLobby && TryNormalizeTransportJoinCode(candidate.RelayJoinCode, providerType, out var candidateJoinCode))
                            {
                                Debug.Log($"[JoinRoomPanelService] [{traceId}] ResolveNetworkJoinCodeAsync found relay code in query list for lobby '{candidate.LobbyId}'.");
                                return candidateJoinCode;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[JoinRoomPanelService] [{traceId}] ResolveNetworkJoinCodeAsync query failed: {e.Message}");
                }

                await Task.Delay(250);
            }

            Debug.LogWarning($"[JoinRoomPanelService] [{traceId}] ResolveNetworkJoinCodeAsync timed out after 15s for lobbyId='{room?.LobbyId}' lobbyCode='{room?.LobbyCode}' provider='{providerType}'.");

            if (providerType == NetworkProviderType.Relay || providerType == NetworkProviderType.Lan)
                return null;

            return !string.IsNullOrWhiteSpace(room?.RelayJoinCode)
                ? room.RelayJoinCode.Trim()
                : (!string.IsNullOrWhiteSpace(room?.LobbyCode) ? room.LobbyCode : room?.LobbyId);
        }

        private static bool TryNormalizeTransportJoinCode(string value, NetworkProviderType providerType, out string normalizedJoinCode)
        {
            normalizedJoinCode = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
            if (string.IsNullOrEmpty(normalizedJoinCode))
                return false;

            return providerType != NetworkProviderType.Relay || RelayJoinCodeUtility.IsValid(normalizedJoinCode);
        }

        /// <summary>
        /// Виконує приєднання з підтримкою кімнат із паролем. Якщо кімната захищена паролем —
        /// показує <see cref="IPasswordPanelService"/>; при невірному паролі повторює запит з повідомленням про помилку.
        /// </summary>
        private async Task<LobbyRoom> TryJoinWithPasswordLoopAsync(JoinRoomTarget target)
        {
            // 1) Спочатку — швидка спроба без пароля. Для UGS це поверне room з PasswordHash != "" якщо приватна,
            //    тоді ми залишимо лобі та запитаємо пароль. Для LAN ми вже маємо PasswordHash у кеші discovered rooms.
            var probe = await TryProbeRoomForPasswordAsync(target);
            if (!probe.RequiresPassword)
            {
                var joinedWithoutPassword = await JoinTargetAsync(target);

                if (joinedWithoutPassword == null || !joinedWithoutPassword.HasPassword)
                    return joinedWithoutPassword;

                try { await _lobbyService.LeaveAsync(); }
                catch (Exception e) { Debug.LogWarning($"[JoinRoomPanelService] Leave before password retry failed: {e.Message}"); }

                probe = new ProbeResult(true, joinedWithoutPassword.Name);
            }

            if (_passwordPanelService == null)
            {
                Debug.LogWarning("[JoinRoomPanelService] Кімната потребує пароль, але IPasswordPanelService не підключений.");
                _infoPanelService?.Show(new InfoMessage("Приватна кімната", "Ця кімната потребує пароль, але панель введення недоступна."));
                return null;
            }

            string error = null;
            for (int attempt = 0; attempt < 5; attempt++)
            {
                var prompt = await _passwordPanelService.RequestPasswordAsync(probe.DisplayName, error);
                if (!prompt.Confirmed)
                    return null;

                try
                {
                    var room = await JoinTargetAsync(target, prompt.Password);
                    return room;
                }
                catch (WrongPasswordException)
                {
                    error = "Невірний пароль. Спробуйте ще раз.";
                }
                catch (Exception e)
                {
                    Debug.LogError($"[JoinRoomPanelService] JoinByCodeWithPasswordAsync error: {e}");
                    return null;
                }
            }
            return null;
        }

        private async Task<LobbyRoom> JoinTargetAsync(JoinRoomTarget target, string password = null)
        {
            if (!target.IsValid)
                return null;

            var room = await JoinExactTargetAsync(target, password);
            if (room != null || target.Kind != JoinRoomTargetKind.JoinCode)
                return room;

            var resolved = await ResolveJoinCodeAliasAsync(target.Value);
            if (!resolved.IsValid ||
                (resolved.Kind == target.Kind && string.Equals(resolved.Value, target.Value, StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            Debug.LogWarning($"[JoinRoomPanelService] Join by code '{target.Value}' returned null; retrying as {resolved.Kind}='{resolved.Value}'.");
            return await JoinExactTargetAsync(resolved, password);
        }

        private async Task<LobbyRoom> JoinExactTargetAsync(JoinRoomTarget target, string password)
        {
            if (target.Kind == JoinRoomTargetKind.LobbyId)
                return await JoinByIdWithOptionalPasswordAsync(target.Value, password);

            if (string.IsNullOrEmpty(password))
                return await _lobbyService.JoinByCodeAsync(target.Value, GetPlayerName());

            return await _lobbyService.JoinByCodeWithPasswordAsync(target.Value, GetPlayerName(), password);
        }

        private async Task<LobbyRoom> JoinByIdWithOptionalPasswordAsync(string lobbyId, string password)
        {
            if (string.IsNullOrWhiteSpace(lobbyId))
                return null;

            var normalizedLobbyId = lobbyId.Trim();
            Debug.Log($"[JoinRoomPanelService] JoinByIdWithOptionalPasswordAsync: calling JoinByIdAsync('{normalizedLobbyId}')...");
            var room = await _lobbyService.JoinByIdAsync(normalizedLobbyId, GetPlayerName());
            Debug.Log($"[JoinRoomPanelService] JoinByIdWithOptionalPasswordAsync: JoinByIdAsync returned {(room == null ? "null" : $"room '{room.LobbyId}'")}.");
            if (room == null)
            {
                var fallback = await ResolveJoinCodeAliasAsync(normalizedLobbyId);
                if (fallback.IsValid &&
                    !(fallback.Kind == JoinRoomTargetKind.LobbyId && string.Equals(fallback.Value, normalizedLobbyId, StringComparison.OrdinalIgnoreCase)))
                {
                    Debug.LogWarning($"[JoinRoomPanelService] JoinByIdAsync returned null for lobbyId='{normalizedLobbyId}', retrying via {fallback.Kind}='{fallback.Value}'.");
                    room = await JoinExactTargetAsync(fallback, password);
                }
                else
                {
                    Debug.LogWarning($"[JoinRoomPanelService] JoinByIdAsync returned null for lobbyId='{normalizedLobbyId}', fallback={fallback.Kind}/'{fallback.Value}' isValid={fallback.IsValid} — giving up.");
                }

                if (room == null)
                    return null;
            }

            if (!string.IsNullOrEmpty(password) && room.HasPassword && !LobbyPasswordHasher.Verify(password, room.PasswordHash))
            {
                try { await _lobbyService.LeaveAsync(); } catch { }
                throw new WrongPasswordException();
            }

            return room;
        }

        private async Task<JoinRoomTarget> ResolveJoinCodeAliasAsync(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new JoinRoomTarget(JoinRoomTargetKind.None, string.Empty);

            try
            {
                var rooms = await _lobbyService.QueryRoomsAsync();
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

        private async Task ReturnToLobbyChooserWithMessageAsync(string joinPanelName, string title, string message)
        {
            _passwordPanelService?.Cancel();

            try
            {
                if (_networkProvider != null)
                    await _networkProvider.LeaveSessionAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[JoinRoomPanelService] LeaveSessionAsync after join failure failed: {e.Message}");
            }

            try
            {
                if (_lobbyService != null)
                    await _lobbyService.LeaveAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[JoinRoomPanelService] LeaveAsync after join failure failed: {e.Message}");
            }

            await MainThreadDispatcher.EnqueueAsync(() =>
            {
                if (!string.IsNullOrWhiteSpace(joinPanelName))
                    _navigation?.OpenForce(joinPanelName);
                else
                    _navigation?.OpenForce(_joinRoomPanelName);
            });

            _infoPanelService?.Show(new InfoMessage(title, message));
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
        private async Task<ProbeResult> TryProbeRoomForPasswordAsync(JoinRoomTarget target)
        {
            try
            {
                var rooms = await _lobbyService.QueryRoomsAsync();
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
            var currentMenu = _navigation?.CurrentMenu;
            if (!string.IsNullOrWhiteSpace(currentMenu) && !string.Equals(currentMenu, _lobbyPanelName, StringComparison.Ordinal))
                return currentMenu;

            if (!string.IsNullOrWhiteSpace(LastJoinPanelName))
                return LastJoinPanelName;

            return _joinRoomPanelName;
        }

        private void RememberJoinOrigin(string panelName, NetworkProviderType providerType)
        {
            LastJoinPanelName = string.IsNullOrWhiteSpace(panelName) ? _joinRoomPanelName : panelName;
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

        private static List<RoomInfo> ProjectRoomInfos(IReadOnlyList<LobbyRoom> rooms, NetworkProviderType providerType)
        {
            var result = new List<RoomInfo>();
            if (rooms == null)
                return result;

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var room in rooms)
            {
                if (room == null)
                    continue;

                // Пункт 16: приватні кімнати приховуємо зі списку — приєднання лише за кодом.
                if (room.IsPrivate)
                    continue;

                // Стартовані/закриті кімнати не мають бути доступні для нового join.
                if (room.State != LobbyState.Open)
                    continue;

                int currentPlayers = room.Players?.Count ?? 0;
                if (room.MaxPlayers > 0 && currentPlayers >= room.MaxPlayers)
                    continue;

                var key = BuildRoomKey(room, providerType);
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                if (!seen.Add(key))
                    continue;

                result.Add(new RoomInfo
                {
                    RoomName = room.Name,
                    JoinCode = room.LobbyCode,
                    LobbyId = room.LobbyId,
                    HostDisplayName = ResolveHostDisplayName(room),
                    ProviderType = providerType,
                    CurrentPlayers = currentPlayers,
                    MaxPlayers = room.MaxPlayers,
                    HasPassword = room.HasPassword,
                    IsPrivate = room.IsPrivate,
                });
            }

            return result;
        }

        private static string BuildRoomKey(LobbyRoom room, NetworkProviderType providerType)
        {
            if (!string.IsNullOrWhiteSpace(room.LobbyId))
                return $"{providerType}:id:{room.LobbyId.Trim()}";

            if (!string.IsNullOrWhiteSpace(room.LobbyCode))
                return $"{providerType}:code:{room.LobbyCode.Trim()}";

            if (!string.IsNullOrWhiteSpace(room.RelayJoinCode))
                return $"{providerType}:relay:{room.RelayJoinCode.Trim()}";

            return string.Empty;
        }

        private static string ResolveHostDisplayName(LobbyRoom room)
        {
            if (room?.Players == null)
                return string.Empty;

            foreach (var player in room.Players)
            {
                if (player != null && player.IsHost && !string.IsNullOrWhiteSpace(player.DisplayName))
                    return player.DisplayName.Trim();
            }

            foreach (var player in room.Players)
            {
                if (player != null && string.Equals(player.PlayerId, room.HostPlayerId, StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(player.DisplayName))
                    return player.DisplayName.Trim();
            }

            foreach (var player in room.Players)
            {
                if (player != null && !string.IsNullOrWhiteSpace(player.DisplayName))
                    return player.DisplayName.Trim();
            }

            return string.Empty;
        }
    }
}