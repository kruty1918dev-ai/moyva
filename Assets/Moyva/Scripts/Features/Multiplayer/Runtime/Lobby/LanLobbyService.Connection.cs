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
            var lobbyCode = roomId.Substring(0, 8);
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
                hostPlayerId: hostPlayerId, relayJoinCode: joinCode, players: players, passwordHash: _currentPasswordHash, state: LobbyState.Open);

            if (ip.StartsWith("127.", StringComparison.Ordinal))
                _logger.Warn($"[LanLobby] CreateRoomAsync detected loopback address '{ip}'. Other devices will not be able to connect.");

            _logger.Info($"[LanLobby] CreateRoomAsync created room='{options.Name}', lobbyId='{roomId}', joinCode='{joinCode}', hostId='{hostPlayerId}', maxPlayers={options.MaxPlayers}, private={options.IsPrivate}.");

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
            var responses = 0;
            var parsed = 0;
            var invalid = 0;

            using (var client = CreateQueryClient())
            {
                var deadline = DateTime.UtcNow.AddMilliseconds(QueryTimeoutMs);
                try
                {
                    _logger.Trace($"[LanLobby] QueryRoomsAsync started. timeoutMs={QueryTimeoutMs}, discoveryPort={DiscoveryPort}, broadcast='{_broadcastEndPoint.Address}:{_broadcastEndPoint.Port}'.");
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
                            responses++;

                            var json = Encoding.UTF8.GetString(result.Value.Buffer);
                            if (IsDiscoveryQuery(json))
                                continue;

                            if (TryParsePayload(json, out var room, out var joinCode))
                            {
                                parsed++;
                                var key = $"{room.LobbyId}:{joinCode}";
                                roomsByKey[key] = roomsByKey.TryGetValue(key, out var existing)
                                    ? MergeRooms(existing, room)
                                    : room;
                                RememberDiscoveredRoom(roomsByKey[key]);
                            }
                            else
                            {
                                invalid++;
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

            if (roomsByKey.Count == 0)
            {
                _logger.Warn($"[LanLobby] QueryRoomsAsync discovered no rooms (responses={responses}, parsed={parsed}, invalid={invalid}). Check same subnet, firewall/UDP broadcast, and host advertised IP.");
            }
            else
            {
                _logger.Info($"[LanLobby] QueryRoomsAsync discovered {roomsByKey.Count} room(s) (responses={responses}, parsed={parsed}, invalid={invalid}).");
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

    }
}
