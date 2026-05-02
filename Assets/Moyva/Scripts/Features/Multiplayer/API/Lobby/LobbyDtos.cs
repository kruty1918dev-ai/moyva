using System.Collections.Generic;

namespace Kruty1918.Moyva.Multiplayer.Lobbies
{
    /// <summary>Життєвий стан lobby: відкрита, гра вже стартувала, або lobby закрита.</summary>
    public enum LobbyState
    {
        Open = 0,
        Started = 1,
        Closed = 2
    }

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
        public LobbyState State { get; }

        /// <summary>Хеш пароля кімнати (SHA-256 hex). Порожній = кімната без пароля.</summary>
        public string PasswordHash { get; }

        /// <summary>True, якщо для приєднання потрібен пароль.</summary>
        public bool HasPassword => !string.IsNullOrEmpty(PasswordHash);

        public LobbyRoom(
            string lobbyId,
            string lobbyCode,
            string name,
            int maxPlayers,
            bool isPrivate,
            string hostPlayerId,
            string relayJoinCode,
            IReadOnlyList<LobbyPlayer> players,
            string passwordHash = null,
            LobbyState state = LobbyState.Open)
        {
            LobbyId = lobbyId ?? string.Empty;
            LobbyCode = lobbyCode ?? string.Empty;
            Name = name ?? string.Empty;
            MaxPlayers = maxPlayers;
            IsPrivate = isPrivate;
            HostPlayerId = hostPlayerId ?? string.Empty;
            RelayJoinCode = relayJoinCode ?? string.Empty;
            Players = players ?? new List<LobbyPlayer>();
            PasswordHash = passwordHash ?? string.Empty;
            State = state;
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

        /// <summary>Сирий пароль, введений хостом. Зберігається як SHA-256 хеш, ніколи не передається по мережі в відкритому вигляді.</summary>
        public string Password { get; }

        public CreateRoomOptions(string name, int maxPlayers, bool isPrivate, string displayName, string password = null)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Room" : name.Trim();
            MaxPlayers = maxPlayers > 0 ? maxPlayers : 4;
            IsPrivate = isPrivate;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Player" : displayName.Trim();
            Password = password;
        }

        /// <summary>True, якщо пароль був введений.</summary>
        public bool HasPassword => !string.IsNullOrEmpty(Password);
    }
}
