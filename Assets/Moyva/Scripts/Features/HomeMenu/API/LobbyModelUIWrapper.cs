using System.Diagnostics;

namespace Kruty1918.Moyva.HomeMenu.API
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public struct LobbyModelUIWrapper
    {
        public string LobbyDisplayName;
        public string LobbyId { get; }
        public string HostNickname { get; }
        public PlayerUIWrapper[] CurrentPlayers { get; }
        public int MaxPlayers { get; }
        public string HashPassword;
        public bool HasPassword => !string.IsNullOrWhiteSpace(HashPassword);
        public bool IsFull => CurrentPlayers.Length >= MaxPlayers;

        public LobbyModelUIWrapper(
            string lobbyId,
            string hostNickname,
            PlayerUIWrapper[] currentPlayers,
            int maxPlayers,
            string hashPassword,
            string lobbyDisplayName)
        {
            LobbyId = lobbyId;
            HostNickname = hostNickname;
            CurrentPlayers = currentPlayers;
            MaxPlayers = maxPlayers;
            HashPassword = hashPassword;
            LobbyDisplayName = lobbyDisplayName ?? $"{hostNickname}'s Lobby";
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}