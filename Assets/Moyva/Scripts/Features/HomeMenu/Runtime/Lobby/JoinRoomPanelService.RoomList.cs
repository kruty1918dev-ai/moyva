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
        public async Task<bool> RefreshRoomListAsync(CancellationToken externalCt = default)
        {
            if (_isJoining)
                return false;

            _roomsCts?.Cancel();
            _roomsCts?.Dispose();
            _roomsCts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
            _roomsCts.CancelAfter(TimeSpan.FromSeconds(5));
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
                        }
                        else if (string.IsNullOrEmpty(roomInfo.JoinCode) && string.IsNullOrEmpty(roomInfo.LobbyId))
                        {
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

    }
}
