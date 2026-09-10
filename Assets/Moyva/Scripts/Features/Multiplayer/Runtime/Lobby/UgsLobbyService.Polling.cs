using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;
using Kruty1918.Moyva.Multiplayer.Runtime;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    public sealed partial class UgsLobbyService
    {
        private void StartLoops()
        {
            StopLoops();
            _loopCts = new CancellationTokenSource();
            _ = PollLoop(_loopCts.Token);
            if (_isHost)
                _ = HeartbeatLoop(_loopCts.Token);
        }

        private void StopLoops()
        {
            if (_loopCts != null)
            {
                try { _loopCts.Cancel(); } catch { /* ignore */ }
                _loopCts.Dispose();
                _loopCts = null;
            }
        }

        private async Task HeartbeatLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _lobby != null && _isHost)
            {
                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(_lobby.Id);
                }
                catch (LobbyServiceException e) when (IsUnauthorized(e))
                {
                    CloseLobbyState("unauthorized");
                    return;
                }
                catch (Exception)
                {
                }

                try { await Task.Delay(TimeSpan.FromSeconds(HeartbeatSeconds), ct); }
                catch (OperationCanceledException) { return; }
            }
        }

        private async Task PollLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _lobby != null)
            {
                try
                {
                    var lobby = _lobby;
                    var lobbyService = LobbyService.Instance;
                    if (lobby == null || lobbyService == null)
                        return;

                    await UpdateLocalPlayerTimeAsync();
                    if (ct.IsCancellationRequested || _lobby == null)
                        return;

                    var refreshed = await lobbyService.GetLobbyAsync(lobby.Id);
                    if (refreshed != null)
                    {
                        var previous = _current;
                        _lobby = refreshed;
                        _current = Project(_lobby);
                        bool wasHost = _isHost;
                        _isHost = string.Equals(_current.HostPlayerId, AuthenticationService.Instance.PlayerId, StringComparison.Ordinal);
                        if (!wasHost && _isHost)
                            StartLoops();
                        if (_isHost)
                            await PublishReconnectRecordsForRemovedPlayersAsync(previous, _current, ct);

                        LobbyUpdated?.Invoke(_current);
                        PublishState(_current.State);

                        bool stillIn = false;
                        string myId = AuthenticationService.Instance.PlayerId;
                        foreach (var p in _current.Players)
                        {
                            if (p.PlayerId == myId) { stillIn = true; break; }
                        }
                        if (!stillIn)
                        {
                            KickedFromLobby?.Invoke("removed");
                            StopLoops();
                            _lobby = null;
                            _current = null;
                            PublishState(LobbyState.Closed);
                            return;
                        }
                    }
                }
                catch (LobbyServiceException e) when ((int)e.Reason == (int)LobbyExceptionReason.LobbyNotFound)
                {
                    KickedFromLobby?.Invoke("lobby_closed");
                    StopLoops();
                    _lobby = null;
                    _current = null;
                    PublishState(LobbyState.Closed);
                    return;
                }
                catch (LobbyServiceException e) when (IsUnauthorized(e))
                {
                    CloseLobbyState("unauthorized");
                    return;
                }
                catch (Exception e)
                {
                    var delaySeconds = e.Message != null && e.Message.Contains("Too Many Requests") ? PollBackoffSeconds : PollSeconds;
                    try { await Task.Delay(TimeSpan.FromSeconds(delaySeconds), ct); }
                    catch (OperationCanceledException) { return; }
                    continue;
                }

                try { await Task.Delay(TimeSpan.FromSeconds(PollSeconds), ct); }
                catch (OperationCanceledException) { return; }
            }
        }

    }
}
