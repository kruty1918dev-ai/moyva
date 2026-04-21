using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.Multiplayer.Config
{
    /// <summary>
    /// Authoritative multiplayer configuration.
    /// Stored as a binary file locally; future-proof for remote sharing.
    /// </summary>
    public sealed class MultiplayerConfig
    {
        public const int CurrentSchemaVersion = 2;

        public int SchemaVersion { get; }
        public NetworkProviderType ProviderType { get; }

        /// <summary>
        /// Provider to fall back to when <see cref="ProviderType"/> fails.
        /// Defaults to <see cref="NetworkProviderType.Offline"/>.
        /// </summary>
        public NetworkProviderType FallbackProviderType { get; }

        /// <summary>Settings used when <see cref="ProviderType"/> or <see cref="FallbackProviderType"/> is <see cref="NetworkProviderType.Relay"/>.</summary>
        public RelayProviderSettings RelaySettings { get; }

        /// <summary>Settings used when <see cref="ProviderType"/> or <see cref="FallbackProviderType"/> is <see cref="NetworkProviderType.WebSocket"/>.</summary>
        public WebSocketProviderSettings WebSocketSettings { get; }

        public SessionRules DefaultSessionRules { get; }
        public bool StrictParticipantLock { get; }
        public bool EnforceConfigConsistency { get; }
        public bool MatchmakingEnabled { get; }

        public MultiplayerConfig(
            int schemaVersion,
            NetworkProviderType providerType,
            SessionRules defaultSessionRules,
            bool strictParticipantLock,
            bool enforceConfigConsistency,
            bool matchmakingEnabled,
            RelayProviderSettings relaySettings = null,
            WebSocketProviderSettings webSocketSettings = null,
            NetworkProviderType fallbackProviderType = NetworkProviderType.Offline)
        {
            SchemaVersion = schemaVersion;
            ProviderType = providerType;
            FallbackProviderType = fallbackProviderType;
            RelaySettings = relaySettings ?? RelayProviderSettings.Default();
            WebSocketSettings = webSocketSettings ?? WebSocketProviderSettings.Default();
            DefaultSessionRules = defaultSessionRules;
            StrictParticipantLock = strictParticipantLock;
            EnforceConfigConsistency = enforceConfigConsistency;
            MatchmakingEnabled = matchmakingEnabled;
        }

        public static MultiplayerConfig Default() =>
            new MultiplayerConfig(
                schemaVersion: CurrentSchemaVersion,
                providerType: NetworkProviderType.Relay,
                defaultSessionRules: SessionRules.Default(),
                strictParticipantLock: false,
                enforceConfigConsistency: true,
                matchmakingEnabled: false,
                relaySettings: RelayProviderSettings.Default(),
                webSocketSettings: WebSocketProviderSettings.Default(),
                fallbackProviderType: NetworkProviderType.Offline);
    }
}
