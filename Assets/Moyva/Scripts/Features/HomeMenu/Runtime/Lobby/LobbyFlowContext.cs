using Kruty1918.Moyva.HomeMenu.API;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.HomeMenu.Runtime
{
    internal sealed class LobbyFlowContext : ILobbyFlowContext
    {
        public NetworkProviderType Provider { get; private set; } = NetworkProviderType.Relay;
        public LobbyFlowKind FlowKind { get; private set; } = LobbyFlowKind.None;

        public void Set(NetworkProviderType provider, LobbyFlowKind flowKind)
        {
            Provider = provider;
            FlowKind = flowKind;
        }
    }
}
