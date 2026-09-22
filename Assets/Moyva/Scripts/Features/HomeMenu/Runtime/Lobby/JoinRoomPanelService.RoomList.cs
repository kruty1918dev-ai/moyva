using System;
using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.HomeMenu.Runtime.Services;
using Kruty1918.Moyva.HomeMenu.UI;
using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Lobbies;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.SaveSystem;
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
            var myCts = _roomsCts;
            var ct = myCts.Token;

            var statusView = _viewController as IRoomListStatusView;
            await MainThreadDispatcher.EnqueueAsync(() =>
            {
                _viewController.ClearRoomList();
                statusView?.SetRoomListStatus(RoomListStatus.Loading, "Fetching rooms...");
            });

            try
            {
                if (_lobbyService == null)
                {
                    Debug.LogError("[JoinRoomPanelService] ILobbyService not available; clearing room list.");
                    await MainThreadDispatcher.EnqueueAsync(() =>
                        statusView?.SetRoomListStatus(RoomListStatus.Error, "Lobby service is unavailable."));
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
                        _viewController.AddRoomToList(roomInfo);

                    statusView?.SetRoomListStatus(
                        roomInfos.Count == 0 ? RoomListStatus.Empty : RoomListStatus.Ready,
                        roomInfos.Count == 0 ? "No public rooms found. Try Refresh or join by invite code." : string.Empty);
                    populated = true;
                });

                return populated;
            }
            catch (OperationCanceledException)
            {
                // Superseded by a newer refresh — that request owns the status now.
                if (!ReferenceEquals(_roomsCts, myCts))
                    return false;

                // The 5s timeout fired while this request was still current.
                await MainThreadDispatcher.EnqueueAsync(() =>
                    statusView?.SetRoomListStatus(RoomListStatus.Error, "Room search timed out. Check the connection and try Refresh."));
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[JoinRoomPanelService] RefreshRoomListAsync failed: {e}");
                await MainThreadDispatcher.EnqueueAsync(() =>
                    statusView?.SetRoomListStatus(RoomListStatus.Error, "Could not load rooms. Check the connection and try Refresh."));
                return false;
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

            if (_isJoining)
            {
                // Joining may first leave an old lobby or retry with a password.
                // Those Closed events do not describe the target room. Started
                // is handled by the join/reconnect pipeline once it has the room.
                if (_joinState != JoinPipelineState.ConnectingTransport || state != LobbyState.Closed)
                    return;

                _joinedRoomClosed = true;
                _joinCts?.Cancel();
                return;
            }

            RefreshRoomList();
        }

    }
}
