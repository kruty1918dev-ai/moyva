using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Runtime;
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
        private const int DiscoveryPort = 54544;
        private const string PayloadProtocol = "MOYVA_LAN_LOBBY_V1";
        private const string DiscoveryQuery = "QUERY";
        private const int BroadcastIntervalMs = 1000;
        private const int QueryTimeoutMs = 2500;

        private readonly IMultiplayerLogger _logger;
        private readonly UdpClient _udp;
        private readonly IPEndPoint _broadcastEndPoint;
        private readonly IPEndPoint _loopbackEndPoint;
        private readonly object _stateLock = new object();
        private readonly Dictionary<string, LobbyRoom> _discoveredRooms = new Dictionary<string, LobbyRoom>(StringComparer.OrdinalIgnoreCase);

        private CancellationTokenSource _cts;
        private UdpClient _listenUdp;
        private LobbyRoom _current;
        private string _currentPasswordHash = string.Empty;
        private LobbyState _state = LobbyState.Closed;
        private bool _broadcastWarningLogged;
        private bool _loopbackWarningLogged;

        public event Action<LobbyRoom> LobbyUpdated;
        public event Action<LobbyState> StateChanged;
#pragma warning disable CS0067
        public event Action<string> KickedFromLobby;
#pragma warning restore CS0067

        public LobbyRoom Current => _current;
        public LobbyState State => _state;

        public LanLobbyService(IMultiplayerLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _udp = new UdpClient();
            _udp.EnableBroadcast = true;
            _broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, DiscoveryPort);
            _loopbackEndPoint = new IPEndPoint(IPAddress.Loopback, DiscoveryPort);
        }

        public Task<LobbyRoom> CreateRoomAsync(CreateRoomOptions options, CancellationToken ct = default)
        {
            // For LAN we simply return a LobbyRoom and start broadcasting
            var roomId = Guid.NewGuid().ToString("N");
            var ip = GetLocalIPAddress() ?? "127.0.0.1";
            var lobbyCode = roomId.Substring(0, 8);
            var joinCode = $"lan:{ip}:{DefaultPort}";
            var hostPlayerId = BuildLocalHostId();
            var players = new List<LobbyPlayer>
            {
                new LobbyPlayer(hostPlayerId, options.DisplayName, isHost: true)
            };

            _currentPasswordHash = LobbyPasswordHasher.Hash(options.Password);
            _current = new LobbyRoom(roomId, lobbyCode, options.Name, options.MaxPlayers, options.IsPrivate,
                hostPlayerId: hostPlayerId, relayJoinCode: joinCode, players: players, passwordHash: _currentPasswordHash, state: LobbyState.Open);

            StartBroadcastLoop();
            LobbyUpdated?.Invoke(_current);
            PublishState(LobbyState.Open);
            return Task.FromResult(_current);
        }

        public Task<LobbyRoom> JoinByCodeAsync(string lobbyCode, string displayName, CancellationToken ct = default)
        {
            // For LAN, attempt to discover rooms and match by lobby code.
            // This allows SessionManager's join-by-code flow to work when a LAN room is discovered.
            return Task.Run(async () =>
            {
                var value = lobbyCode?.Trim();
                if (string.IsNullOrWhiteSpace(value))
                {
                    _logger.Warn("[LanLobby] JoinByCodeAsync: порожній код приєднання.");
                    return null;
                }

                _logger.Info($"[LanLobby] JoinByCodeAsync: code='{value}', cacheSize={_discoveredRooms.Count}.");

                try
                {
                    // Спершу пробуємо кеш виявлених кімнат (заповнюється під час QueryRoomsAsync та StartBroadcastLoop).
                    var cachedRoom = FindDiscoveredRoom(value);
                    if (cachedRoom != null)
                    {
                        _logger.Info($"[LanLobby] JoinByCodeAsync: знайдено в кеші lobbyId='{cachedRoom.LobbyId}', relay='{cachedRoom.RelayJoinCode}'.");
                        _current = AddLocalPlayer(cachedRoom, displayName);
                        StartBroadcastLoop();
                        LobbyUpdated?.Invoke(_current);
                        return _current;
                    }

                    _logger.Info("[LanLobby] JoinByCodeAsync: немає в кеші, запускаю активний QueryRoomsAsync.");
                    var rooms = await QueryRoomsAsync(ct).ConfigureAwait(false);
                    _logger.Info($"[LanLobby] JoinByCodeAsync: QueryRoomsAsync повернув {rooms.Count} кімнат(и).");
                    foreach (var r in rooms)
                    {
                        if (MatchesJoinInput(r, value))
                        {
                            _logger.Info($"[LanLobby] JoinByCodeAsync: збіг через активний пошук lobbyId='{r.LobbyId}'.");
                            _current = AddLocalPlayer(r, displayName);
                            StartBroadcastLoop();
                            LobbyUpdated?.Invoke(_current);
                            return _current;
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Warn($"[LanLobby] JoinByCodeAsync: помилка пошуку: {e.Message}");
                }

                if (IsLanJoinCode(value))
                {
                    _logger.Info($"[LanLobby] JoinByCodeAsync: пряме приєднання за lan-кодом '{value}'.");
                    _current = CreateDirectJoinRoom(value, displayName);
                    LobbyUpdated?.Invoke(_current);
                    return _current;
                }

                _logger.Warn($"[LanLobby] JoinByCodeAsync: кімнату не знайдено за кодом '{value}'.");
                return null;
            }, ct);
        }

        public Task<LobbyRoom> JoinByIdAsync(string lobbyId, string displayName, CancellationToken ct = default)
        {
            return JoinByCodeAsync(lobbyId, displayName, ct);
        }

        public async Task<LobbyRoom> JoinByCodeWithPasswordAsync(string lobbyCode, string displayName, string password, CancellationToken ct = default)
        {
            // Спочатку знаходимо кімнату (без додавання локального гравця), звіряємо хеш пароля.
            var value = lobbyCode?.Trim();
            if (string.IsNullOrWhiteSpace(value))
                return null;

            LobbyRoom matched = FindDiscoveredRoom(value);
            if (matched == null)
            {
                var rooms = await QueryRoomsAsync(ct).ConfigureAwait(false);
                foreach (var r in rooms)
                {
                    if (MatchesJoinInput(r, value)) { matched = r; break; }
                }
            }

            if (matched == null && IsLanJoinCode(value))
            {
                // Пряме приєднання без broadcast'у — пароль невідомий, пропускаємо.
                _current = CreateDirectJoinRoom(value, displayName);
                LobbyUpdated?.Invoke(_current);
                return _current;
            }

            if (matched == null)
            {
                _logger.Warn($"[LanLobby] JoinByCodeWithPasswordAsync: кімнату не знайдено за '{value}'.");
                return null;
            }

            if (matched.HasPassword && !LobbyPasswordHasher.Verify(password, matched.PasswordHash))
            {
                _logger.Warn($"[LanLobby] JoinByCodeWithPasswordAsync: невірний пароль для '{value}'.");
                throw new WrongPasswordException();
            }

            _current = AddLocalPlayer(matched, displayName);
            StartBroadcastLoop();
            LobbyUpdated?.Invoke(_current);
            return _current;
        }

        public async Task<IReadOnlyList<LobbyRoom>> QueryRoomsAsync(CancellationToken ct = default)
        {
            var roomsByKey = new Dictionary<string, LobbyRoom>(StringComparer.Ordinal);

            using (var client = CreateQueryClient())
            {
                var deadline = DateTime.UtcNow.AddMilliseconds(QueryTimeoutMs);
                try
                {
                    await SendDiscoveryQueryAsync(client).ConfigureAwait(false);

                    while (DateTime.UtcNow < deadline)
                    {
                        if (ct.IsCancellationRequested) break;
                        try
                        {
                            var remaining = deadline - DateTime.UtcNow;
                            if (remaining <= TimeSpan.Zero) break;

                            var result = await ReceiveResultWithTimeoutAsync(client, remaining, ct).ConfigureAwait(false);
                            if (!result.HasValue) break;

                            var json = Encoding.UTF8.GetString(result.Value.Buffer);
                            if (IsDiscoveryQuery(json))
                                continue;

                            if (TryParsePayload(json, out var room, out var joinCode))
                            {
                                var key = $"{room.LobbyId}:{joinCode}";
                                roomsByKey[key] = roomsByKey.TryGetValue(key, out var existing)
                                    ? MergeRooms(existing, room)
                                    : room;
                                RememberDiscoveredRoom(roomsByKey[key]);
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

            return new List<LobbyRoom>(roomsByKey.Values);
        }

        public Task LeaveAsync(CancellationToken ct = default)
        {
            StopBroadcastLoop();
            _current = null;
            PublishState(LobbyState.Closed);
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

        public Task LockAsync(bool locked, byte[] startedWorldSettingsBytes = null, CancellationToken ct = default)
        {
            var state = locked ? LobbyState.Started : LobbyState.Open;
            if (_current != null)
            {
                _current = new LobbyRoom(_current.LobbyId, _current.LobbyCode, _current.Name, _current.MaxPlayers,
                    _current.IsPrivate, _current.HostPlayerId, _current.RelayJoinCode, _current.Players,
                    _current.PasswordHash, state, _current.ReconnectRecords, locked ? startedWorldSettingsBytes : null);
                LobbyUpdated?.Invoke(_current);
            }
            PublishState(state);
            return Task.CompletedTask;
        }

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
                    catch (Exception e) { _logger.Warn($"[LanLobby] Broadcast error: {e.Message}"); }
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
                    catch (Exception e) { _logger.Warn($"[LanLobby] Receive error: {e.Message}"); }
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
            catch (Exception e)
            {
                if (!_loopbackWarningLogged)
                {
                    _loopbackWarningLogged = true;
                    _logger.Warn($"[LanLobby] Loopback discovery send failed: {e.Message}");
                }
            }

            try
            {
                await _udp.SendAsync(bytes, bytes.Length, _broadcastEndPoint).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (!_broadcastWarningLogged)
                {
                    _broadcastWarningLogged = true;
                    _logger.Warn($"[LanLobby] Broadcast discovery send failed: {e.Message}");
                }
            }
        }

        private async Task SendDiscoveryQueryAsync(UdpClient client)
        {
            var bytes = Encoding.UTF8.GetBytes(string.Join('|', PayloadProtocol, DiscoveryQuery));

            try
            {
                await client.SendAsync(bytes, bytes.Length, _loopbackEndPoint).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (!_loopbackWarningLogged)
                {
                    _loopbackWarningLogged = true;
                    _logger.Warn($"[LanLobby] Loopback discovery query failed: {e.Message}");
                }
            }

            try
            {
                await client.SendAsync(bytes, bytes.Length, _broadcastEndPoint).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (!_broadcastWarningLogged)
                {
                    _broadcastWarningLogged = true;
                    _logger.Warn($"[LanLobby] Broadcast discovery query failed: {e.Message}");
                }
            }
        }

        private Task SendDiscoveryResponseAsync(IPEndPoint remoteEndPoint)
        {
            if (remoteEndPoint == null || _current == null)
                return Task.CompletedTask;

            var payload = BuildPayload();
            var bytes = Encoding.UTF8.GetBytes(payload);
            return _udp.SendAsync(bytes, bytes.Length, remoteEndPoint);
        }

        private bool MergeCurrentRoom(LobbyRoom incomingRoom)
        {
            if (incomingRoom == null)
                return false;

            lock (_stateLock)
            {
                if (_current == null || !string.Equals(_current.LobbyId, incomingRoom.LobbyId, StringComparison.Ordinal))
                    return false;

                var merged = MergeRooms(_current, incomingRoom);
                if (HaveSamePlayers(_current, merged))
                    return false;

                _current = merged;
                return true;
            }
        }

        private static LobbyRoom AddLocalPlayer(LobbyRoom room, string displayName)
        {
            var localId = BuildLocalHostId();
            var players = new List<LobbyPlayer>(room.Players ?? Array.Empty<LobbyPlayer>());
            var exists = false;
            foreach (var player in players)
            {
                if (string.Equals(player.PlayerId, localId, StringComparison.Ordinal))
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
                players.Add(new LobbyPlayer(localId, string.IsNullOrWhiteSpace(displayName) ? "Player" : displayName.Trim(), isHost: false));

            return new LobbyRoom(room.LobbyId, room.LobbyCode, room.Name, room.MaxPlayers, room.IsPrivate,
                room.HostPlayerId, room.RelayJoinCode, players, room.PasswordHash, room.State,
                room.ReconnectRecords, room.StartedWorldSettingsBytes);
        }

        private static bool MatchesJoinInput(LobbyRoom room, string value)
        {
            if (room == null || string.IsNullOrWhiteSpace(value))
                return false;

            return string.Equals(room.LobbyCode, value, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(room.LobbyId, value, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(room.RelayJoinCode, value, StringComparison.OrdinalIgnoreCase);
        }

        private LobbyRoom FindDiscoveredRoom(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            lock (_stateLock)
            {
                return _discoveredRooms.TryGetValue(value.Trim(), out var room) ? room : null;
            }
        }

        private void RememberDiscoveredRoom(LobbyRoom room)
        {
            if (room == null)
                return;

            lock (_stateLock)
            {
                AddDiscoveredKey(room.LobbyId, room);
                AddDiscoveredKey(room.LobbyCode, room);
                AddDiscoveredKey(room.RelayJoinCode, room);
            }
        }

        private void AddDiscoveredKey(string key, LobbyRoom room)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            _discoveredRooms[key.Trim()] = room;
        }

        private static bool IsLanJoinCode(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Trim().StartsWith("lan:", StringComparison.OrdinalIgnoreCase);
        }

        private static LobbyRoom CreateDirectJoinRoom(string joinCode, string displayName)
        {
            var localId = BuildLocalHostId();
            var players = new List<LobbyPlayer>
            {
                new LobbyPlayer(localId, string.IsNullOrWhiteSpace(displayName) ? "Player" : displayName.Trim(), isHost: false)
            };

            return new LobbyRoom(joinCode.Trim(), joinCode.Trim(), "LAN Room", 4, false,
                string.Empty, joinCode.Trim(), players, state: LobbyState.Open);
        }

        private static LobbyRoom MergeRooms(LobbyRoom first, LobbyRoom second)
        {
            var playersById = new Dictionary<string, LobbyPlayer>(StringComparer.Ordinal);
            AddPlayers(playersById, first?.Players);
            AddPlayers(playersById, second?.Players);

            var lobbyId = !string.IsNullOrWhiteSpace(first?.LobbyId) ? first.LobbyId : second?.LobbyId;
            var lobbyCode = !string.IsNullOrWhiteSpace(first?.LobbyCode) ? first.LobbyCode : second?.LobbyCode;
            var name = !string.IsNullOrWhiteSpace(first?.Name) ? first.Name : second?.Name;
            var maxPlayers = first?.MaxPlayers > 0 ? first.MaxPlayers : (second?.MaxPlayers ?? 4);
            var hostPlayerId = !string.IsNullOrWhiteSpace(first?.HostPlayerId) ? first.HostPlayerId : second?.HostPlayerId;
            var relayJoinCode = !string.IsNullOrWhiteSpace(first?.RelayJoinCode) ? first.RelayJoinCode : second?.RelayJoinCode;
            var passwordHash = !string.IsNullOrEmpty(first?.PasswordHash) ? first.PasswordHash : second?.PasswordHash;
            var state = first?.State ?? second?.State ?? LobbyState.Open;
            var reconnectRecords = (first?.ReconnectRecords?.Count ?? 0) > 0 ? first.ReconnectRecords : second?.ReconnectRecords;
            var worldSettings = (first?.StartedWorldSettingsBytes?.Length ?? 0) > 0 ? first.StartedWorldSettingsBytes : second?.StartedWorldSettingsBytes;

            return new LobbyRoom(lobbyId, lobbyCode, name, maxPlayers, first?.IsPrivate ?? second?.IsPrivate ?? false,
                hostPlayerId, relayJoinCode, new List<LobbyPlayer>(playersById.Values), passwordHash, state,
                reconnectRecords, worldSettings);
        }

        private static void AddPlayers(Dictionary<string, LobbyPlayer> playersById, IReadOnlyList<LobbyPlayer> players)
        {
            if (players == null)
                return;

            foreach (var player in players)
            {
                if (player == null || string.IsNullOrWhiteSpace(player.PlayerId))
                    continue;

                playersById[player.PlayerId] = player;
            }
        }

        private static bool HaveSamePlayers(LobbyRoom first, LobbyRoom second)
        {
            if (first?.Players == null || second?.Players == null)
                return first?.Players == second?.Players;

            if (first.Players.Count != second.Players.Count)
                return false;

            var firstPlayers = new HashSet<string>(StringComparer.Ordinal);
            foreach (var player in first.Players)
                firstPlayers.Add(player.PlayerId);

            foreach (var player in second.Players)
            {
                if (!firstPlayers.Contains(player.PlayerId))
                    return false;
            }

            return true;
        }

        private static bool TryParsePayload(string payload, out LobbyRoom room, out string joinCode)
        {
            room = null;
            joinCode = string.Empty;

            var parts = payload?.Split('|');
            if (parts == null || parts.Length < 6)
                return false;

            if (!string.Equals(parts[0], PayloadProtocol, StringComparison.Ordinal))
                return false;

            var roomId = parts[1];
            if (string.IsNullOrWhiteSpace(roomId))
                return false;

            var name = parts[2];
            var max = int.TryParse(parts[3], out var parsedMax) ? parsedMax : 4;
            var ip = parts[4];
            var port = parts[5];
            var hostId = parts.Length >= 7 && !string.IsNullOrWhiteSpace(parts[6]) ? parts[6] : roomId;
            var hostName = parts.Length >= 8 ? parts[7] : string.Empty;
            var players = parts.Length >= 9 ? DeserializePlayers(parts[8]) : new List<LobbyPlayer>();
            if (players.Count == 0 && !string.IsNullOrWhiteSpace(hostName))
                players.Add(new LobbyPlayer(hostId, hostName.Trim(), isHost: true));

            var isPrivate = parts.Length >= 10 && parts[9] == "1";
            var passwordHash = parts.Length >= 11 ? parts[10] : string.Empty;
            var state = LobbyState.Open;
            if (parts.Length >= 12 && int.TryParse(parts[11], out var rawState) && Enum.IsDefined(typeof(LobbyState), rawState))
                state = (LobbyState)rawState;
            var worldSettings = parts.Length >= 13 ? DecodeBytes(parts[12]) : Array.Empty<byte>();

            joinCode = $"lan:{ip}:{port}";
            var lobbyCode = roomId.Length >= 8 ? roomId.Substring(0, 8) : roomId;
            room = new LobbyRoom(roomId, lobbyCode, name, max, isPrivate, hostId, joinCode, players, passwordHash, state,
                startedWorldSettingsBytes: worldSettings);
            return true;
        }

        private void PublishState(LobbyState state)
        {
            if (_state == state) return;
            _state = state;
            StateChanged?.Invoke(state);
        }

        private static bool IsDiscoveryQuery(string payload)
        {
            var parts = payload?.Split('|');
            return parts != null && parts.Length >= 2 &&
                   string.Equals(parts[0], PayloadProtocol, StringComparison.Ordinal) &&
                   string.Equals(parts[1], DiscoveryQuery, StringComparison.Ordinal);
        }

        private static string SerializePlayers(LobbyRoom room)
        {
            if (room?.Players == null || room.Players.Count == 0)
                return string.Empty;

            var parts = new List<string>(room.Players.Count);
            foreach (var player in room.Players)
            {
                if (player == null || string.IsNullOrWhiteSpace(player.PlayerId))
                    continue;

                parts.Add($"{EncodeToken(player.PlayerId)}~{EncodeToken(player.DisplayName)}~{(player.IsHost ? "1" : "0")}");
            }

            return string.Join(",", parts);
        }

        private static List<LobbyPlayer> DeserializePlayers(string value)
        {
            var players = new List<LobbyPlayer>();
            if (string.IsNullOrWhiteSpace(value))
                return players;

            var items = value.Split(',');
            foreach (var item in items)
            {
                var parts = item.Split('~');
                if (parts.Length < 3)
                    continue;

                var playerId = DecodeToken(parts[0]);
                if (string.IsNullOrWhiteSpace(playerId))
                    continue;

                players.Add(new LobbyPlayer(playerId, DecodeToken(parts[1]), parts[2] == "1"));
            }

            return players;
        }

        private static string EncodeToken(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value ?? string.Empty));
        }

        private static string DecodeToken(string value)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(value ?? string.Empty));
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string EncodeBytes(byte[] bytes)
        {
            return bytes == null || bytes.Length == 0 ? string.Empty : Convert.ToBase64String(bytes);
        }

        private static byte[] DecodeBytes(string encoded)
        {
            if (string.IsNullOrWhiteSpace(encoded))
                return Array.Empty<byte>();

            try { return Convert.FromBase64String(encoded); }
            catch { return Array.Empty<byte>(); }
        }

        private static UdpClient CreateListeningClient(int port)
        {
            var client = new UdpClient(AddressFamily.InterNetwork);
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            return client;
        }

        private static UdpClient CreateQueryClient()
        {
            var client = new UdpClient(AddressFamily.InterNetwork);
            client.EnableBroadcast = true;
            client.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
            return client;
        }

        private static async Task<byte[]> ReceiveWithTimeoutAsync(UdpClient client, TimeSpan timeout, CancellationToken ct)
        {
            var result = await ReceiveResultWithTimeoutAsync(client, timeout, ct).ConfigureAwait(false);
            return result.HasValue ? result.Value.Buffer : null;
        }

        private static async Task<UdpReceiveResult?> ReceiveResultWithTimeoutAsync(UdpClient client, TimeSpan timeout, CancellationToken ct)
        {
            var receiveTask = client.ReceiveAsync();
            var delayTask = Task.Delay(timeout, ct);
            var completed = await Task.WhenAny(receiveTask, delayTask).ConfigureAwait(false);
            if (completed != receiveTask)
                return null;

            return receiveTask.Result;
        }

        private static string ResolveHostDisplayName(LobbyRoom room)
        {
            if (room?.Players != null)
            {
                foreach (var player in room.Players)
                {
                    if (player != null && player.IsHost && !string.IsNullOrWhiteSpace(player.DisplayName))
                        return player.DisplayName.Trim();
                }
            }

            return Environment.MachineName ?? "Player";
        }

        private static string BuildLocalHostId()
        {
            var machineName = string.IsNullOrWhiteSpace(Environment.MachineName) ? "local" : Environment.MachineName;
            if (!MultiplayerClientScope.IsDefault)
                return $"{machineName}-{MultiplayerClientScope.ScopeId}";

            return $"{machineName}-{System.Diagnostics.Process.GetCurrentProcess().Id}";
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
