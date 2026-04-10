namespace Kruty1918.Moyva.Multiplayer.Networking
{
    /// <summary>
    /// Settings specific to the Unity Relay backend.
    /// Requires the Unity Gaming Services Relay package (com.unity.services.relay).
    /// Enable the <c>MOYVA_UGS_RELAY</c> scripting define after installing the package.
    /// </summary>
    public sealed class RelayProviderSettings
    {
        /// <summary>Unity project ID (from Unity Dashboard → Settings).</summary>
        public string ProjectId { get; }

        /// <summary>Service environment: "production" or "development".</summary>
        public string Environment { get; }

        /// <summary>Preferred allocation region, e.g. "eu-west-1". Empty = auto.</summary>
        public string Region { get; }

        /// <summary>Maximum number of connections supported by the Relay allocation.</summary>
        public int MaxConnections { get; }

        public RelayProviderSettings(
            string projectId,
            string environment,
            string region,
            int maxConnections)
        {
            ProjectId = projectId ?? string.Empty;
            Environment = string.IsNullOrEmpty(environment) ? "production" : environment;
            Region = region ?? string.Empty;
            MaxConnections = maxConnections > 0 ? maxConnections : 4;
        }

        public static RelayProviderSettings Default() =>
            new RelayProviderSettings(string.Empty, "production", string.Empty, 4);
    }

    /// <summary>
    /// Settings specific to the WebSocket backend.
    /// The server must implement the simple framing protocol used by
    /// <c>WebSocketNetworkProvider</c>: 8-byte sender-ID prefix followed by raw payload.
    /// </summary>
    public sealed class WebSocketProviderSettings
    {
        /// <summary>WebSocket server URL, e.g. "ws://localhost" or "wss://example.com".</summary>
        public string ServerUrl { get; }

        /// <summary>Port to connect to. Combined with <see cref="ServerUrl"/> at runtime.</summary>
        public int Port { get; }

        /// <summary>Optional Bearer token sent in the Authorization header on connect.</summary>
        public string AuthToken { get; }

        /// <summary>How many times to attempt reconnection before giving up.</summary>
        public int ReconnectAttempts { get; }

        /// <summary>Seconds to wait between reconnection attempts.</summary>
        public float ReconnectDelaySeconds { get; }

        public WebSocketProviderSettings(
            string serverUrl,
            int port,
            string authToken,
            int reconnectAttempts,
            float reconnectDelaySeconds)
        {
            ServerUrl = string.IsNullOrEmpty(serverUrl) ? "ws://localhost" : serverUrl;
            Port = port > 0 ? port : 9999;
            AuthToken = authToken ?? string.Empty;
            ReconnectAttempts = reconnectAttempts >= 0 ? reconnectAttempts : 3;
            ReconnectDelaySeconds = reconnectDelaySeconds >= 0 ? reconnectDelaySeconds : 2f;
        }

        public static WebSocketProviderSettings Default() =>
            new WebSocketProviderSettings("ws://localhost", 9999, string.Empty, 3, 2f);
    }
}
