using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    /// <summary>
    /// High-level abstraction over Unity Gaming Services Lobby.
    /// Responsible for room lifecycle (create, join, list, leave) and for
    /// storing/sharing the Relay join code between host and clients.
    /// </summary>
    public interface ILobbyService
    {
        /// <summary>Currently active lobby snapshot, or null when not in a lobby.</summary>
        LobbyRoom Current { get; }

        /// <summary>Fires whenever the lobby snapshot is refreshed (heartbeat, poll, updates).</summary>
        event Action<LobbyRoom> LobbyUpdated;

        /// <summary>Fires when the local player was kicked or the lobby was removed.</summary>
        event Action<string> KickedFromLobby;

        Task<LobbyRoom> CreateRoomAsync(CreateRoomOptions options, CancellationToken ct = default);
        Task<LobbyRoom> JoinByCodeAsync(string lobbyCode, string displayName, CancellationToken ct = default);
        Task<LobbyRoom> JoinByIdAsync(string lobbyId, string displayName, CancellationToken ct = default);
        Task<IReadOnlyList<LobbyRoom>> QueryRoomsAsync(CancellationToken ct = default);
        Task LeaveAsync(CancellationToken ct = default);
        Task KickAsync(string playerId, CancellationToken ct = default);

        /// <summary>
        /// Writes the Relay join code into the current lobby's shared data.
        /// Host must call this right after the Relay allocation is created.
        /// </summary>
        Task SetRelayJoinCodeAsync(string relayJoinCode, CancellationToken ct = default);

        /// <summary>
        /// Marks the lobby as locked (no new players). Typically called after game start.
        /// </summary>
        Task LockAsync(bool locked, CancellationToken ct = default);
    }
}
