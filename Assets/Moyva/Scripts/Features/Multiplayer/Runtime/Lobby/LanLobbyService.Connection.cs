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
        public Task<LobbyRoom> CreateRoomAsync(CreateRoomOptions options, CancellationToken ct = default)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            // For LAN we simply return a LobbyRoom and start broadcasting
            var roomId = Guid.NewGuid().ToString("N");
            var ip = GetLocalIPAddress() ?? "127.0.0.1";
            var lobbyCode = BuildShortLanLobbyCode(roomId);
            var joinCode = string.IsNullOrWhiteSpace(options.RelayJoinCode)
                ? $"lan:{ip}:{DefaultPort}"
                : options.RelayJoinCode.Trim();
            var hostPlayerId = BuildLocalHostId();
            var players = new List<LobbyPlayer>
            {
                new LobbyPlayer(hostPlayerId, options.DisplayName, isHost: true)
            };

            _currentPasswordHash = LobbyPasswordHasher.Hash(options.Password);
            _current = new LobbyRoom(roomId, lobbyCode, options.Name, options.MaxPlayers, options.IsPrivate,
                hostPlayerId: hostPlayerId, relayJoinCode: joinCode, players: players,
                passwordHash: _currentPasswordHash, state: LobbyState.Open,
                configFingerprint: options.ConfigFingerprint);

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
                    return null;
                }

                try
                {
                    // Спершу пробуємо кеш виявлених кімнат (заповнюється під час QueryRoomsAsync та StartBroadcastLoop).
                    var cachedRoom = FindDiscoveredRoom(value);
                    if (cachedRoom != null)
                    {
                        _current = AddLocalPlayer(cachedRoom, displayName);
                        StartBroadcastLoop();
                        LobbyUpdated?.Invoke(_current);
                        PublishState(_current.State);
                        return _current;
                    }
                    var rooms = await QueryRoomsAsync(ct).ConfigureAwait(false);
                    foreach (var r in rooms)
                    {
                        if (MatchesJoinInput(r, value))
                        {
                            _current = AddLocalPlayer(r, displayName);
                            StartBroadcastLoop();
                            LobbyUpdated?.Invoke(_current);
                            PublishState(_current.State);
                            return _current;
                        }
                    }
                }
                catch (Exception)
                {
                }

                if (IsLanJoinCode(value))
                {
                    _current = CreateDirectJoinRoom(value, displayName);
                    LobbyUpdated?.Invoke(_current);
                    PublishState(_current.State);
                    return _current;
                }
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
                return null;
            }

            if (matched.HasPassword && !LobbyPasswordHasher.Verify(password, matched.PasswordHash))
            {
                throw new WrongPasswordException();
            }

            _current = AddLocalPlayer(matched, displayName);
            StartBroadcastLoop();
            LobbyUpdated?.Invoke(_current);
            PublishState(_current.State);
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

                            if (TryParsePayload(json, out var room, out var joinCode, result.Value.RemoteEndPoint))
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
                        catch (Exception)
                        {
                        }
                    }
                }
                catch { }
            }

            foreach (var cachedRoom in SnapshotDiscoveredRooms())
            {
                var key = $"{cachedRoom.LobbyId}:{cachedRoom.RelayJoinCode}";
                roomsByKey[key] = roomsByKey.TryGetValue(key, out var existing)
                    ? MergeRooms(existing, cachedRoom)
                    : cachedRoom;
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
            var normalizedJoinCode = relayJoinCode?.Trim() ?? string.Empty;
            if (_current != null && !string.Equals(_current.RelayJoinCode, normalizedJoinCode, StringComparison.Ordinal))
            {
                _current = new LobbyRoom(_current.LobbyId, _current.LobbyCode, _current.Name, _current.MaxPlayers,
                    _current.IsPrivate, _current.HostPlayerId, normalizedJoinCode, _current.Players,
                    _current.PasswordHash, _current.State, _current.ReconnectRecords,
                    _current.StartedWorldSettingsBytes, _current.BannedPlayerIds,
                    _current.CapabilityFlags, _current.ConfigFingerprint);
                RememberDiscoveredRoom(_current);
                LobbyUpdated?.Invoke(_current);
            }

            return Task.CompletedTask;
        }

        public Task LockAsync(bool locked, byte[] startedWorldSettingsBytes = null, CancellationToken ct = default)
        {
            var state = locked ? LobbyState.Started : LobbyState.Open;
            if (_current != null)
            {
                _current = new LobbyRoom(_current.LobbyId, _current.LobbyCode, _current.Name, _current.MaxPlayers,
                    _current.IsPrivate, _current.HostPlayerId, _current.RelayJoinCode, _current.Players,
                    _current.PasswordHash, state, _current.ReconnectRecords,
                    locked ? startedWorldSettingsBytes : null,
                    _current.BannedPlayerIds, _current.CapabilityFlags, _current.ConfigFingerprint);
                LobbyUpdated?.Invoke(_current);
            }
            PublishState(state);
            return Task.CompletedTask;
        }

    }
}
