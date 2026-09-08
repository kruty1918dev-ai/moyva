using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal sealed class LobbyFlowContext : ILobbyFlowContext
    {
        public NetworkProviderType Provider { get; private set; } = NetworkProviderType.Relay;
        public LobbyFlowKind FlowKind { get; private set; } = LobbyFlowKind.None;
        public bool HasRoomDraft { get; private set; }
        public string RoomName { get; private set; } = string.Empty;
        public string Password { get; private set; } = string.Empty;
        public bool IsPublic { get; private set; } = true;
        public int MaxPlayers { get; private set; } = 4;

        public void Set(NetworkProviderType provider, LobbyFlowKind flowKind)
        {
            Provider = provider;
            FlowKind = flowKind;
        }

        public void SetRoomDraft(string roomName, int maxPlayers, bool isPublic, string password)
        {
            RoomName = string.IsNullOrWhiteSpace(roomName) ? "Moyva Lobby" : roomName.Trim();
            MaxPlayers = UnityEngine.Mathf.Clamp(maxPlayers, 2, 8);
            IsPublic = isPublic;
            Password = password ?? string.Empty;
            HasRoomDraft = true;
        }

        public void ClearRoomDraft()
        {
            RoomName = string.Empty;
            Password = string.Empty;
            IsPublic = true;
            MaxPlayers = 4;
            HasRoomDraft = false;
        }
    }
}
