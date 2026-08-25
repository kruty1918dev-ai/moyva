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
    internal partial class JoinRoomPanelService
    {
        private async void OnJoinClicked()
        {
            if (_viewController == null) return;
            if (_isJoining) return;
            if (!_actionRateLimiter.Allow("join-click", TimeSpan.FromMilliseconds(400)))
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
                RefreshRoomList();
                return;
            }

            var target = JoinRoomResolver.FromRoom(room);
            if (!target.IsValid)
            {
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
                _joinState = JoinPipelineState.ResolvingTarget;
                await ApplySelectedProviderAsync(ct);

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
                    var blockReason = JoinRoomDomainLogic.GetPostJoinBlockReason(room, GetPlayerName(), ResolveReconnectToleranceSeconds());
                    if (!string.IsNullOrEmpty(blockReason))
                    {
                        shouldRefreshRoomListAfterFailure = true;
                        await ReturnToLobbyChooserWithMessageAsync(joinPanelName, "Кімната недоступна", blockReason + "\n\nДія: Оновіть список кімнат.", ct);
                        return;
                    }

                    if (_roomAccessPolicy != null)
                    {
                        var localPlayerId = JoinRoomDomainLogic.ResolveLocalPlayerId(room, GetPlayerName());
                        if (!_roomAccessPolicy.CanJoin(room, localPlayerId, out var policyReason))
                        {
                            shouldRefreshRoomListAfterFailure = true;
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
                _joinState = JoinPipelineState.Failed;
            }
            catch (RoomFullException ex)
            {
                shouldRefreshRoomListAfterFailure = true;
                await ReturnToLobbyChooserWithMessageAsync(joinPanelName, "Кімната переповнена",
                    new MultiplayerUserFacingError(ex.ErrorCode, ex.Message, "Оберіть іншу кімнату.", traceId).BuildDisplayMessage(), ct);
                _joinState = JoinPipelineState.Failed;
            }
            catch (RoomAccessDeniedException ex)
            {
                shouldRefreshRoomListAfterFailure = true;
                await ReturnToLobbyChooserWithMessageAsync(joinPanelName, "Доступ заборонено",
                    new MultiplayerUserFacingError(ex.ErrorCode, ex.Message, "Зверніться до організатора кімнати.", traceId).BuildDisplayMessage(), ct);
                _joinState = JoinPipelineState.Failed;
            }
            catch (SessionExpiredException ex)
            {
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
                return 120f;
            }
        }

    }
}
