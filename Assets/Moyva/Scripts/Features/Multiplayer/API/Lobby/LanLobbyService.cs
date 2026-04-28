using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using UnityEngine;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    /// <summary>
    /// Simple LAN lobby service using UDP broadcast for discovery.
    /// This is a lightweight implementation intended as a skeleton.
    /// </summary>
    public sealed class LanLobbyService : ILobbyService, IDisposable
    {
        public const int DefaultPort = 54545;
        private const int BroadcastIntervalMs = 1000;

        private readonly IMultiplayerLogger _logger;
        private readonly UdpClient _udp;
        private readonly IPEndPoint _broadcastEndPoint;

        private CancellationTokenSource _cts;
        private LobbyRoom _current;
        private bool _isHost;

        public event Action<LobbyRoom> LobbyUpdated;
        public event Action<string> KickedFromLobby;

        public LobbyRoom Current => _current;

        public LanLobbyService(IMultiplayerLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _udp = new UdpClient();
            _udp.EnableBroadcast = true;
            _broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, DefaultPort);
        }

        public Task<LobbyRoom> CreateRoomAsync(CreateRoomOptions options, CancellationToken ct = default)
        {
            // For LAN we simply return a LobbyRoom and start broadcasting
            var roomId = Guid.NewGuid().ToString("N");
            var ip = GetLocalIPAddress() ?? "127.0.0.1";
            var lobbyCode = roomId.Substring(0, 8);
            var joinCode = $"lan:{ip}:{DefaultPort}";

            _current = new LobbyRoom(roomId, lobbyCode, options.Name, options.MaxPlayers, options.IsPrivate,
                hostPlayerId: Environment.MachineName, relayJoinCode: joinCode, players: new List<LobbyPlayer>());

            _isHost = true;
            StartBroadcastLoop();
            LobbyUpdated?.Invoke(_current);
            return Task.FromResult(_current);
        }

        public Task<LobbyRoom> JoinByCodeAsync(string lobbyCode, string displayName, CancellationToken ct = default)
        {
            // For LAN, attempt to discover rooms and match by lobby code.
            // This allows SessionManager's join-by-code flow to work when a LAN room is discovered.
            return Task.Run(async () =>
            {
                try
                {
                    var rooms = await QueryRoomsAsync(ct).ConfigureAwait(false);
                    foreach (var r in rooms)
                    {
                        if (string.Equals(r.LobbyCode, lobbyCode, StringComparison.OrdinalIgnoreCase))
                        {
                            // Found matching room — return it. Clients do not broadcast.
                            return r;
                        }
                    }
                }
                catch { }
                return null;
            }, ct);
        }

        public Task<LobbyRoom> JoinByIdAsync(string lobbyId, string displayName, CancellationToken ct = default)
        {
            return Task.FromResult<LobbyRoom>(null);
        }

        public async Task<IReadOnlyList<LobbyRoom>> QueryRoomsAsync(CancellationToken ct = default)
        {
            var list = new List<LobbyRoom>();

            using (var client = new UdpClient(DefaultPort))
            {
                client.Client.ReceiveTimeout = 500;
                var deadline = DateTime.UtcNow.AddMilliseconds(500);
                try
                {
                    while (DateTime.UtcNow < deadline)
                    {
                        if (ct.IsCancellationRequested) break;
                        try
                        {
                            var result = await client.ReceiveAsync();
                            var json = Encoding.UTF8.GetString(result.Buffer);
                            // Parse simple format: roomId|name|maxPlayers|ip|port
                            var parts = json.Split('|');
                            if (parts.Length >= 5)
                            {
                                var roomId = parts[0];
                                var name = parts[1];
                                int max = int.TryParse(parts[2], out var m) ? m : 4;
                                string ip = parts[3];
                                string port = parts[4];
                                var joinCode = $"lan:{ip}:{port}";
                                var lr = new LobbyRoom(roomId, roomId.Substring(0,8), name, max, false, roomId, joinCode, new List<LobbyPlayer>());
                                list.Add(lr);
                            }
                        }
                        catch (SocketException) { break; }
                        catch (OperationCanceledException) { break; }
                        catch (Exception e)
                        {
                            _logger.Warn($"[LanLobby] Query parse error: {e.Message}");
                        }
                    }
                }
                catch { }
            }

            return list;
        }

        public Task LeaveAsync(CancellationToken ct = default)
        {
            StopBroadcastLoop();
            _current = null;
            _isHost = false;
            return Task.CompletedTask;
        }

        public Task KickAsync(string playerId, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task SetRelayJoinCodeAsync(string relayJoinCode, CancellationToken ct = default)
        {
            // Not applicable for LAN; keep as no-op.
            return Task.CompletedTask;
        }

        public Task LockAsync(bool locked, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        private void StartBroadcastLoop()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;
            _ = Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var payload = BuildPayload();
                        var bytes = Encoding.UTF8.GetBytes(payload);
                        await _udp.SendAsync(bytes, bytes.Length, _broadcastEndPoint);
                    }
                    catch (Exception e) { _logger.Warn($"[LanLobby] Broadcast error: {e.Message}"); }
                    try { await Task.Delay(BroadcastIntervalMs, ct); } catch (OperationCanceledException) { break; }
                }
            }, ct);
        }

        private void StopBroadcastLoop()
        {
            try { _cts?.Cancel(); } catch { }
            _cts?.Dispose();
            _cts = null;
        }

        private string BuildPayload()
        {
            // Format: roomId|name|maxPlayers|ip|port
            var roomId = _current?.LobbyId ?? Guid.NewGuid().ToString("N");
            var name = _current?.Name ?? "Room";
            var max = _current?.MaxPlayers ?? 4;
            var ip = GetLocalIPAddress() ?? "127.0.0.1";
            var port = DefaultPort.ToString();
            return string.Join('|', roomId, name, max.ToString(), ip, port);
        }

        private static string GetLocalIPAddress()
        {
            try
            {
                foreach (var ni in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (ni.AddressFamily == AddressFamily.InterNetwork)
                        return ni.ToString();
                }
            }
            catch { }
            return null;
        }

        public void Dispose()
        {
            StopBroadcastLoop();
            try { _udp.Close(); _udp.Dispose(); } catch { }
        }
    }
}
