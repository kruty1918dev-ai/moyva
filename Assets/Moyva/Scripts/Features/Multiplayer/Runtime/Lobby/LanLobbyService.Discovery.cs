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
            // Browsers must also listen on the discovery port: host advertisements
            // never arrive at the ephemeral port used for query responses.
            lock (_discoveryLock)
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(LanLobbyService));
                if (_listenUdp != null)
                    return;

                _listenUdp = CreateListeningClient(DiscoveryPort);
                _cts = new CancellationTokenSource();
                RunDiscoveryLoops(_listenUdp, _cts.Token);
                Debug.Log($"[LAN Discovery] Listening on UDP {DiscoveryPort}; local IPv4={GetLocalIPAddress() ?? "unavailable"}.");
            }
        }

        private void RunDiscoveryLoops(UdpClient listener, CancellationToken ct)
        {
            _ = Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        if (_current != null)
                        {
                            var payload = BuildPayload();
                            var bytes = Encoding.UTF8.GetBytes(payload);
                            await SendDiscoveryPayloadAsync(bytes).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex) { LogDiscoveryFailure("advertise", ex); }
                    try { await Task.Delay(BroadcastIntervalMs, ct); } catch (OperationCanceledException) { break; }
                }
            }, ct);

            _ = Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var result = await ReceiveResultWithTimeoutAsync(listener, TimeSpan.FromMilliseconds(1000), ct).ConfigureAwait(false);
                        if (!result.HasValue)
                            continue;

                        var json = Encoding.UTF8.GetString(result.Value.Buffer);
                        if (IsDiscoveryQuery(json))
                        {
                            if (IsCurrentLocalHost())
                                await SendDiscoveryResponseAsync(listener, result.Value.RemoteEndPoint).ConfigureAwait(false);
                            continue;
                        }

                        if (!TryParsePayload(json, out var incomingRoom, out _, result.Value.RemoteEndPoint))
                            continue;

                        RememberDiscoveredRoom(incomingRoom);
                        if (MergeCurrentRoom(incomingRoom))
                            LobbyUpdated?.Invoke(_current);
                    }
                    catch (OperationCanceledException) { break; }
                    catch (ObjectDisposedException) { break; }
                    catch (Exception ex)
                    {
                        if (ct.IsCancellationRequested) break;
                        LogDiscoveryFailure("receive", ex);
                        try { await Task.Delay(BroadcastIntervalMs, ct).ConfigureAwait(false); }
                        catch (OperationCanceledException) { break; }
                    }
                }
            }, ct);
        }

        private void StopBroadcastLoop()
        {
            lock (_discoveryLock)
            {
                try { _cts?.Cancel(); } catch { }
                try { _listenUdp?.Close(); _listenUdp?.Dispose(); } catch { }
                _listenUdp = null;
                _cts?.Dispose();
                _cts = null;
            }
        }

        private string BuildPayload()
        {
            // Format: protocol|roomId|name|maxPlayers|ip|port|hostId|hostName|players|isPrivate|passwordHash|state|worldSettings|fingerprint|lobbyCode
            var roomId = _current?.LobbyId ?? Guid.NewGuid().ToString("N");
            var name = _current?.Name ?? "Room";
            var max = _current?.MaxPlayers ?? 4;
            var ip = ResolveAdvertisedTransportIp(_current) ?? GetLocalIPAddress() ?? "127.0.0.1";
            var port = ResolveAdvertisedTransportPort(_current).ToString();
            var hostId = _current?.HostPlayerId ?? BuildLocalHostId();
            var hostName = ResolveHostDisplayName(_current);
            var players = SerializePlayers(_current);
            var isPrivate = (_current?.IsPrivate ?? false) ? "1" : "0";
            var passwordHash = _current?.PasswordHash ?? string.Empty;
            var state = ((int)(_current?.State ?? LobbyState.Open)).ToString();
            var worldSettings = EncodeBytes(_current?.StartedWorldSettingsBytes);
            var configFingerprint = _current?.ConfigFingerprint ?? string.Empty;
            var lobbyCode = _current?.LobbyCode ?? BuildShortLanLobbyCode(roomId);
            return string.Join('|', PayloadProtocol, roomId, name, max.ToString(), ip, port, hostId, hostName, players, isPrivate, passwordHash, state, worldSettings, configFingerprint, lobbyCode);
        }

        private static string ResolveAdvertisedTransportIp(LobbyRoom room)
        {
            var joinCode = room?.RelayJoinCode;
            if (string.IsNullOrWhiteSpace(joinCode))
                return null;

            var parts = joinCode.Trim().Split(':');
            if (parts.Length >= 3 &&
                string.Equals(parts[0], "lan", StringComparison.OrdinalIgnoreCase) &&
                IPAddress.TryParse(parts[1], out var parsedIp) &&
                parsedIp.AddressFamily == AddressFamily.InterNetwork)
            {
                return parsedIp.ToString();
            }

            return null;
        }

        private static int ResolveAdvertisedTransportPort(LobbyRoom room)
        {
            var joinCode = room?.RelayJoinCode;
            if (!string.IsNullOrWhiteSpace(joinCode))
            {
                var parts = joinCode.Trim().Split(':');
                if (parts.Length >= 3 &&
                    string.Equals(parts[0], "lan", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(parts[2], out var parsedPort) &&
                    parsedPort > 0 &&
                    parsedPort <= 65535)
                {
                    return parsedPort;
                }
            }

            return DefaultPort;
        }

        private async Task SendDiscoveryPayloadAsync(byte[] bytes)
        {
            if (IsCurrentLocalHost())
            {
                await SendHostDiscoveryPayloadAsync(bytes).ConfigureAwait(false);
                return;
            }

            await SendClientPresencePayloadAsync(bytes).ConfigureAwait(false);
        }

        private async Task SendHostDiscoveryPayloadAsync(byte[] bytes)
        {
            try
            {
                await _udp.SendAsync(bytes, bytes.Length, _loopbackEndPoint).ConfigureAwait(false);
            }
            catch (Exception ex) { LogDiscoveryFailure("send", ex); }

            try
            {
                await _udp.SendAsync(bytes, bytes.Length, _broadcastEndPoint).ConfigureAwait(false);
            }
            catch (Exception ex) { LogDiscoveryFailure("send", ex); }

            foreach (var endPoint in GetDirectedBroadcastEndPoints(DiscoveryPort))
            {
                try
                {
                    await _udp.SendAsync(bytes, bytes.Length, endPoint).ConfigureAwait(false);
                }
                catch (Exception ex) { LogDiscoveryFailure("send", ex); }
            }
        }

        private async Task SendClientPresencePayloadAsync(byte[] bytes)
        {
            if (!TryResolveHostDiscoveryEndPoint(_current, out var hostDiscoveryEndPoint))
                return;

            try
            {
                await _udp.SendAsync(bytes, bytes.Length, hostDiscoveryEndPoint).ConfigureAwait(false);
            }
            catch (Exception ex) { LogDiscoveryFailure("send", ex); }
        }

        private async Task SendDiscoveryQueryAsync(UdpClient client)
        {
            var bytes = Encoding.UTF8.GetBytes(string.Join('|', PayloadProtocol, DiscoveryQuery));

            try
            {
                await client.SendAsync(bytes, bytes.Length, _loopbackEndPoint).ConfigureAwait(false);
            }
            catch (Exception ex) { LogDiscoveryFailure("send query", ex); }

            try
            {
                await client.SendAsync(bytes, bytes.Length, _broadcastEndPoint).ConfigureAwait(false);
            }
            catch (Exception ex) { LogDiscoveryFailure("send query", ex); }

            foreach (var endPoint in GetDirectedBroadcastEndPoints(DiscoveryPort))
            {
                try
                {
                    await client.SendAsync(bytes, bytes.Length, endPoint).ConfigureAwait(false);
                }
                catch (Exception ex) { LogDiscoveryFailure("send query", ex); }
            }
        }

        private Task SendDiscoveryResponseAsync(UdpClient listener, IPEndPoint remoteEndPoint)
        {
            if (remoteEndPoint == null || _current == null)
                return Task.CompletedTask;

            var payload = BuildPayload();
            var bytes = Encoding.UTF8.GetBytes(payload);
            // Reply from the queried port, so stateful firewalls can associate
            // the response with the client's outgoing discovery request.
            return listener.SendAsync(bytes, bytes.Length, remoteEndPoint);
        }

        private void LogDiscoveryFailure(string operation, Exception exception)
        {
            if (_disposed)
                return;
            var message = $"[LAN Discovery] {operation} failed on UDP {DiscoveryPort}: {exception.GetType().Name}: {exception.Message}";
            if (string.Equals(_lastDiscoveryError, message, StringComparison.Ordinal))
                return;

            _lastDiscoveryError = message;
            Debug.LogWarning(message);
        }

        private static bool TryResolveHostDiscoveryEndPoint(LobbyRoom room, out IPEndPoint endPoint)
        {
            endPoint = null;

            var joinCode = room?.RelayJoinCode;
            if (string.IsNullOrWhiteSpace(joinCode))
                return false;

            var parts = joinCode.Trim().Split(':');
            if (parts.Length < 3 ||
                !string.Equals(parts[0], "lan", StringComparison.OrdinalIgnoreCase) ||
                !IPAddress.TryParse(parts[1], out var ip) ||
                ip.AddressFamily != AddressFamily.InterNetwork)
            {
                return false;
            }

            endPoint = new IPEndPoint(ip, DiscoveryPort);
            return true;
        }

    }
}
