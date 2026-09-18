using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.HomeMenu.API
{
    public interface ILobbyFlowContext
    {
        NetworkProviderType Provider { get; }
        LobbyFlowKind FlowKind { get; }
        bool HasRoomDraft { get; }
        string RoomName { get; }
        string Password { get; }
        bool IsPublic { get; }
        int MaxPlayers { get; }

        void Set(NetworkProviderType provider, LobbyFlowKind flowKind);
        void SetRoomDraft(string roomName, int maxPlayers, bool isPublic, string password);
        void ClearRoomDraft();
    }
}
