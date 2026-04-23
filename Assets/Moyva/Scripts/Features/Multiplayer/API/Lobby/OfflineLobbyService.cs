using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    /// <summary>
    /// In-memory, single-process stub used when the UGS Lobby SDK is not available
    /// (e.g. the <c>MOYVA_UGS_LOBBY</c> scripting define is not set), or for tests.
    /// All operations succeed locally so that <see cref="Core.SessionManager"/> can run.
    /// </summary>
    public sealed class OfflineLobbyService : ILobbyService
    {
        private readonly IMultiplayerLogger _logger;
        private LobbyRoom _current;

        public LobbyRoom Current => _current;

        public event Action<LobbyRoom> LobbyUpdated;
        public event Action<string> KickedFromLobby;

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
                options.IsPrivate, hostId, relayJoinCode: string.Empty, players: players);

            _logger?.Info($"[OfflineLobby] Created room '{options.Name}' code={code}");
            LobbyUpdated?.Invoke(_current);
            return Task.FromResult(_current);
        }

        public Task<LobbyRoom> JoinByCodeAsync(string lobbyCode, string displayName, CancellationToken ct = default)
        {
            return Task.FromResult<LobbyRoom>(null);
        }

        public Task<LobbyRoom> JoinByIdAsync(string lobbyId, string displayName, CancellationToken ct = default)
        {
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
                _current.IsPrivate, _current.HostPlayerId, relayJoinCode, _current.Players);
            LobbyUpdated?.Invoke(_current);
            return Task.CompletedTask;
        }

        public Task LockAsync(bool locked, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }
}
