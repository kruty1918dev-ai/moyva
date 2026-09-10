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
                _infoPanelService?.Show(new InfoMessage("Please Wait", "You are clicking too quickly. Try again in a moment."));
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
                _infoPanelService?.Show(new InfoMessage("Request In Progress", "Another Join request with the same code is already running."));
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
                LabelText = "Confirmation",
                MessageText = $"Join room {room.HostOrRoomDisplayName}?",
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
            _joinedRoomClosed = false;
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

                // LAN discovery and transport do not depend on Unity cloud authentication.
                if (GetCurrentProviderType() == NetworkProviderType.Relay && _multiplayerState != null)
                    await _multiplayerState.WaitUntilReadyAsync(ct);

                _joinState = JoinPipelineState.JoiningLobby;
                var joinResult = await TryJoinWithPasswordLoopResultAsync(target, ct);
                if (joinResult.IsFailure)
                {
                    if (joinResult.Error.Code == DomainErrorCode.Cancelled)
                        return;

                    shouldRefreshRoomListAfterFailure = true;
                    var joinError = joinProviderType == NetworkProviderType.Lan && joinResult.Error.Code == DomainErrorCode.NotFound
                        ? new MultiplayerUserFacingError(
                            "MP-LAN-404",
                            "No LAN host responded with this invite code.",
                            "Check the code, keep the host lobby open, and allow the game through the firewall on both devices on the same local network.",
                            traceId)
                        : MultiplayerUserFacingError.FromDomainError(joinResult.Error, traceId);
                    await ReturnToLobbyChooserWithMessageAsync(
                        joinPanelName,
                        "Join Failed",
                        joinError.BuildDisplayMessage(),
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
                        await ReturnToLobbyChooserWithMessageAsync(joinPanelName, "Room Unavailable", blockReason + "\n\nAction: Refresh the room list.", ct);
                        return;
                    }

                    if (_roomAccessPolicy != null)
                    {
                        var localPlayerId = (_lobbyService as ILobbyLocalIdentity)?.LocalPlayerId
                            ?? JoinRoomDomainLogic.ResolveLocalPlayerId(room, GetPlayerName());
                        if (!_roomAccessPolicy.CanJoin(room, localPlayerId, out var policyReason))
                        {
                            shouldRefreshRoomListAfterFailure = true;
                            await ReturnToLobbyChooserWithMessageAsync(joinPanelName, "Access Denied", policyReason + "\n\nAction: Choose another room.", ct);
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
                            "Join Failed",
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
                                "Reconnect Failed",
                                "The room is already in game, but it does not contain valid world settings for reconnect.",
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
                shouldRefreshRoomListAfterFailure = true;
                await ReturnToLobbyChooserWithMessageAsync(
                    joinPanelName,
                    _joinedRoomClosed ? "Room Unavailable" : "Join Timeout",
                    _joinedRoomClosed ? "The room is closed. Refresh the room list and try again." : new MultiplayerUserFacingError(
                        "MP-NET-408",
                        "Joining the room timed out.",
                        "Make sure the host is still in the lobby and both devices are on the same network.",
                        traceId).BuildDisplayMessage(),
                    CancellationToken.None);
                _joinState = JoinPipelineState.Failed;
            }
            catch (RoomFullException ex)
            {
                shouldRefreshRoomListAfterFailure = true;
                await ReturnToLobbyChooserWithMessageAsync(joinPanelName, "Room Full",
                    new MultiplayerUserFacingError(ex.ErrorCode, ex.Message, "Choose another room.", traceId).BuildDisplayMessage(), ct);
                _joinState = JoinPipelineState.Failed;
            }
            catch (RoomAccessDeniedException ex)
            {
                shouldRefreshRoomListAfterFailure = true;
                await ReturnToLobbyChooserWithMessageAsync(joinPanelName, "Access Denied",
                    new MultiplayerUserFacingError(ex.ErrorCode, ex.Message, "Contact the room host.", traceId).BuildDisplayMessage(), ct);
                _joinState = JoinPipelineState.Failed;
            }
            catch (SessionExpiredException ex)
            {
                shouldRefreshRoomListAfterFailure = true;
                await ReturnToLobbyChooserWithMessageAsync(joinPanelName, "Session Expired",
                    new MultiplayerUserFacingError(ex.ErrorCode, ex.Message, "Refresh the room list.", traceId).BuildDisplayMessage(), ct);
                _joinState = JoinPipelineState.Failed;
            }
            catch (MultiplayerDomainException ex)
            {
                Debug.LogError($"[JoinRoomPanelService] [{traceId}] Domain error [{ex.ErrorCode}]: {ex.Message}");
                shouldRefreshRoomListAfterFailure = true;
                await ReturnToLobbyChooserWithMessageAsync(joinPanelName, "Join Failed",
                    new MultiplayerUserFacingError(ex.ErrorCode, ex.Message, "Check your network and try again.", traceId).BuildDisplayMessage(), ct);
                _joinState = JoinPipelineState.Failed;
            }
            catch (Exception e)
            {
                Debug.LogError($"[JoinRoomPanelService] [{traceId}] JoinRoomAsync failed: {e}");
                shouldRefreshRoomListAfterFailure = true;
                await ReturnToLobbyChooserWithMessageAsync(
                    joinPanelName,
                    "Join Failed",
                    new MultiplayerUserFacingError("MP-JOIN-500", BuildJoinFailureMessage(e), "Check your network and try again.", traceId).BuildDisplayMessage(),
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
            await Task.CompletedTask;
            return false;
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

            var localId = (_lobbyService as ILobbyLocalIdentity)?.LocalPlayerId
                            ?? JoinRoomDomainLogic.ResolveLocalPlayerId(room, GetPlayerName());
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
                worldSettings.Height,
                isLocalPlayerHost: false,
                localPlayerId: localId);

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
            catch (Exception)
            {
                return 120f;
            }
        }

    }
}
