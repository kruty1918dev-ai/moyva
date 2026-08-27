using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.HomeMenu.API
{
    public interface ILobbyFlowContext
    {
        NetworkProviderType Provider { get; }
        LobbyFlowKind FlowKind { get; }

        void Set(NetworkProviderType provider, LobbyFlowKind flowKind);
    }
}
