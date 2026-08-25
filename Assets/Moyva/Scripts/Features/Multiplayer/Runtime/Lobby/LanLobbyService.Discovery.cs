using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Runtime;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    public sealed partial class LanLobbyService
    {
        private void StartBroadcastLoop()
        {
            _cts?.Cancel();
            try { _listenUdp?.Close(); _listenUdp?.Dispose(); } catch { }
            _cts = new CancellationTokenSource();
            _listenUdp = CreateListeningClient(DiscoveryPort);
            var ct = _cts.Token;
            _ = Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var payload = BuildPayload();
                        var bytes = Encoding.UTF8.GetBytes(payload);
                        await SendDiscoveryPayloadAsync(bytes).ConfigureAwait(false);
                    }
                    catch (Exception) { }
                    try { await Task.Delay(BroadcastIntervalMs, ct); } catch (OperationCanceledException) { break; }
                }
            }, ct);

            _ = Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var result = await ReceiveResultWithTimeoutAsync(_listenUdp, TimeSpan.FromMilliseconds(1000), ct).ConfigureAwait(false);
                        if (!result.HasValue)
                            continue;

                        var json = Encoding.UTF8.GetString(result.Value.Buffer);
                        if (IsDiscoveryQuery(json))
                        {
                            await SendDiscoveryResponseAsync(result.Value.RemoteEndPoint).ConfigureAwait(false);
                            continue;
                        }

                        if (!TryParsePayload(json, out var incomingRoom, out _))
                            continue;

                        RememberDiscoveredRoom(incomingRoom);
                        if (MergeCurrentRoom(incomingRoom))
                            LobbyUpdated?.Invoke(_current);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (ObjectDisposedException) { break; }
                    catch (Exception) { }
                }
            }, ct);
        }

        private void StopBroadcastLoop()
        {
            try { _cts?.Cancel(); } catch { }
            try { _listenUdp?.Close(); _listenUdp?.Dispose(); } catch { }
            _listenUdp = null;
            _cts?.Dispose();
            _cts = null;
        }

        private string BuildPayload()
        {
            // Format: protocol|roomId|name|maxPlayers|ip|port|hostId|hostName|players|isPrivate|passwordHash
            var roomId = _current?.LobbyId ?? Guid.NewGuid().ToString("N");
            var name = _current?.Name ?? "Room";
            var max = _current?.MaxPlayers ?? 4;
            var ip = GetLocalIPAddress() ?? "127.0.0.1";
            var port = DefaultPort.ToString();
            var hostId = _current?.HostPlayerId ?? BuildLocalHostId();
            var hostName = ResolveHostDisplayName(_current);
            var players = SerializePlayers(_current);
            var isPrivate = (_current?.IsPrivate ?? false) ? "1" : "0";
            var passwordHash = _current?.PasswordHash ?? string.Empty;
            var state = ((int)(_current?.State ?? LobbyState.Open)).ToString();
            var worldSettings = EncodeBytes(_current?.StartedWorldSettingsBytes);
            return string.Join('|', PayloadProtocol, roomId, name, max.ToString(), ip, port, hostId, hostName, players, isPrivate, passwordHash, state, worldSettings);
        }

        private async Task SendDiscoveryPayloadAsync(byte[] bytes)
        {
            try
            {
                await _udp.SendAsync(bytes, bytes.Length, _loopbackEndPoint).ConfigureAwait(false);
            }
            catch (Exception) { }

            try
            {
                await _udp.SendAsync(bytes, bytes.Length, _broadcastEndPoint).ConfigureAwait(false);
            }
            catch (Exception) { }
        }

        private async Task SendDiscoveryQueryAsync(UdpClient client)
        {
            var bytes = Encoding.UTF8.GetBytes(string.Join('|', PayloadProtocol, DiscoveryQuery));

            try
            {
                await client.SendAsync(bytes, bytes.Length, _loopbackEndPoint).ConfigureAwait(false);
            }
            catch (Exception) { }

            try
            {
                await client.SendAsync(bytes, bytes.Length, _broadcastEndPoint).ConfigureAwait(false);
            }
            catch (Exception) { }
        }

        private Task SendDiscoveryResponseAsync(IPEndPoint remoteEndPoint)
        {
            if (remoteEndPoint == null || _current == null)
                return Task.CompletedTask;

            var payload = BuildPayload();
            var bytes = Encoding.UTF8.GetBytes(payload);
            return _udp.SendAsync(bytes, bytes.Length, remoteEndPoint);
        }

    }
}
