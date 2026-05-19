using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    /// <summary>
    /// In-memory, single-process stub used when the UGS Lobby SDK is not available
    /// (e.g. the UGS Lobbies package is not installed), or for tests.
    /// All operations succeed locally so that <see cref="Core.SessionManager"/> can run.
    /// </summary>
    public sealed class OfflineLobbyService : ILobbyService
    {
        private readonly IMultiplayerLogger _logger;
        private LobbyRoom _current;
        private LobbyState _state = LobbyState.Closed;

        public LobbyRoom Current => _current;
        public LobbyState State => _state;

        public event Action<LobbyRoom> LobbyUpdated;
#pragma warning disable CS0067
        public event Action<string> KickedFromLobby;
#pragma warning restore CS0067
        public event Action<LobbyState> StateChanged;

        public OfflineLobbyService(IMultiplayerLogger logger = null)
        {
            _logger = logger;
        }

        public Task<LobbyRoom> CreateRoomAsync(CreateRoomOptions options, CancellationToken ct = default)
        {
            var lobbyId = $"offline-{Guid.NewGuid():N}".Substring(0, 16);
            var code = lobbyId.Substring(8).ToUpperInvariant();
            var hostId = $"offline-host-{Guid.NewGuid():N}".Substring(0, 16);

            var players = new List<LobbyPlayer>
            {
                new LobbyPlayer(hostId, options.DisplayName, isHost: true)
            };

            _current = new LobbyRoom(lobbyId, code, options.Name, options.MaxPlayers,
                options.IsPrivate, hostId, relayJoinCode: options.RelayJoinCode, players: players);

            _logger?.Info($"[OfflineLobby] Created room '{options.Name}' code={code}");
            LobbyUpdated?.Invoke(_current);
            PublishState(LobbyState.Open);
            return Task.FromResult(_current);
        }

        public Task<LobbyRoom> JoinByCodeAsync(string lobbyCode, string displayName, CancellationToken ct = default)
        {
            var code = string.IsNullOrWhiteSpace(lobbyCode)
                ? $"OFF{Guid.NewGuid():N}".Substring(0, 6).ToUpperInvariant()
                : lobbyCode.Trim().ToUpperInvariant();

            var lobbyId = $"offline-join-{code}";
            var hostId = $"offline-host-{code}";

            // Keep players empty in the offline join stub: SessionManager adds local participant.
            _current = new LobbyRoom(
                lobbyId,
                code,
                code,
                maxPlayers: 4,
                isPrivate: false,
                hostPlayerId: hostId,
                relayJoinCode: code,
                players: new List<LobbyPlayer>());

            _logger?.Info($"[OfflineLobby] Joined synthetic room code={code}");
            LobbyUpdated?.Invoke(_current);
            PublishState(LobbyState.Open);
            return Task.FromResult(_current);
        }

        public Task<LobbyRoom> JoinByIdAsync(string lobbyId, string displayName, CancellationToken ct = default)
        {
            var id = string.IsNullOrWhiteSpace(lobbyId)
                ? $"offline-join-{Guid.NewGuid():N}".Substring(0, 16)
                : lobbyId.Trim();
            var code = id.Length >= 6 ? id.Substring(0, 6).ToUpperInvariant() : id.ToUpperInvariant();
            return JoinByCodeAsync(code, displayName, ct);
        }

        public Task<LobbyRoom> JoinByCodeWithPasswordAsync(string lobbyCode, string displayName, string password, CancellationToken ct = default)
        {
            // Offline-режим не підтримує приєднання до віддалених кімнат.
            return Task.FromResult<LobbyRoom>(null);
        }

        public Task<IReadOnlyList<LobbyRoom>> QueryRoomsAsync(CancellationToken ct = default)
        {
            IReadOnlyList<LobbyRoom> empty = Array.Empty<LobbyRoom>();
            return Task.FromResult(empty);
        }

        public Task LeaveAsync(CancellationToken ct = default)
        {
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
            if (_current == null)
                return Task.CompletedTask;

            _current = new LobbyRoom(
                _current.LobbyId, _current.LobbyCode, _current.Name, _current.MaxPlayers,
                _current.IsPrivate, _current.HostPlayerId, relayJoinCode, _current.Players, _current.PasswordHash, _current.State);
            LobbyUpdated?.Invoke(_current);
            return Task.CompletedTask;
        }

        public Task LockAsync(bool locked, byte[] startedWorldSettingsBytes = null, CancellationToken ct = default)
        {
            if (_current != null)
            {
                _current = new LobbyRoom(
                    _current.LobbyId, _current.LobbyCode, _current.Name, _current.MaxPlayers,
                    _current.IsPrivate, _current.HostPlayerId, _current.RelayJoinCode, _current.Players,
                    _current.PasswordHash, locked ? LobbyState.Started : LobbyState.Open,
                    _current.ReconnectRecords, locked ? startedWorldSettingsBytes : null);
                LobbyUpdated?.Invoke(_current);
            }

            PublishState(locked ? LobbyState.Started : LobbyState.Open);
            return Task.CompletedTask;
        }

        private void PublishState(LobbyState state)
        {
            if (_state == state) return;
            _state = state;
            StateChanged?.Invoke(state);
        }
    }
}
