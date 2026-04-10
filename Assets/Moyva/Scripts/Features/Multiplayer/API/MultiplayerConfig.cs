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
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion { get; }
        public NetworkProviderType ProviderType { get; }
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
            bool matchmakingEnabled)
        {
            SchemaVersion = schemaVersion;
            ProviderType = providerType;
            DefaultSessionRules = defaultSessionRules;
            StrictParticipantLock = strictParticipantLock;
            EnforceConfigConsistency = enforceConfigConsistency;
            MatchmakingEnabled = matchmakingEnabled;
        }

        public static MultiplayerConfig Default() =>
            new MultiplayerConfig(
                schemaVersion: CurrentSchemaVersion,
                providerType: NetworkProviderType.Offline,
                defaultSessionRules: SessionRules.Default(),
                strictParticipantLock: false,
                enforceConfigConsistency: true,
                matchmakingEnabled: false);
    }
}
