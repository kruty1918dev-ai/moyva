using System.Collections.Generic;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    /// <summary>
    /// Snapshot of a remote UGS Lobby, projected into a DTO that UI and SessionManager
    /// can consume without depending on UGS types directly.
    /// </summary>
    public sealed class LobbyRoom
    {
        public string LobbyId { get; }
        public string LobbyCode { get; }
        public string Name { get; }
        public int MaxPlayers { get; }
        public bool IsPrivate { get; }
        public string HostPlayerId { get; }
        public string RelayJoinCode { get; }
        public IReadOnlyList<LobbyPlayer> Players { get; }

        public LobbyRoom(
            string lobbyId,
            string lobbyCode,
            string name,
            int maxPlayers,
            bool isPrivate,
            string hostPlayerId,
            string relayJoinCode,
            IReadOnlyList<LobbyPlayer> players)
        {
            LobbyId = lobbyId ?? string.Empty;
            LobbyCode = lobbyCode ?? string.Empty;
            Name = name ?? string.Empty;
            MaxPlayers = maxPlayers;
            IsPrivate = isPrivate;
            HostPlayerId = hostPlayerId ?? string.Empty;
            RelayJoinCode = relayJoinCode ?? string.Empty;
            Players = players ?? new List<LobbyPlayer>();
        }
    }

    /// <summary>
    /// Player entry inside a <see cref="LobbyRoom"/>.
    /// </summary>
    public sealed class LobbyPlayer
    {
        public string PlayerId { get; }
        public string DisplayName { get; }
        public bool IsHost { get; }

        public LobbyPlayer(string playerId, string displayName, bool isHost)
        {
            PlayerId = playerId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            IsHost = isHost;
        }
    }

    /// <summary>
    /// Options passed to <see cref="ILobbyService.CreateRoomAsync"/>.
    /// </summary>
    public sealed class CreateRoomOptions
    {
        public string Name { get; }
        public int MaxPlayers { get; }
        public bool IsPrivate { get; }
        public string DisplayName { get; }

        public CreateRoomOptions(string name, int maxPlayers, bool isPrivate, string displayName)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Room" : name.Trim();
            MaxPlayers = maxPlayers > 0 ? maxPlayers : 4;
            IsPrivate = isPrivate;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Player" : displayName.Trim();
        }
    }
}
