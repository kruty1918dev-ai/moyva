namespace Kruty1918.Moyva.Multiplayer.Networking
{
    /// <summary>
    /// Enumerates the supported networking backend providers.
    /// Int values are persisted in <c>BinaryConfigStore</c>; do not reorder.
    /// </summary>
    public enum NetworkProviderType
    {
        /// <summary>Unity Gaming Services Relay — cloud NAT traversal, no open ports required.</summary>
        Relay = 0,

        /// <summary>WebSocket — connects to a custom signalling/relay server via ws:// or wss://.</summary>
        WebSocket = 1,

        /// <summary>Offline / local — no real networking; used for solo play and testing.</summary>
        Offline = 2
    }
}
