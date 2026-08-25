using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.Multiplayer.Config
{
    /// <summary>
    /// Послідовно оновлює збережену конфігурацію мультиплеєра до поточної схеми.
    /// </summary>
    public static class MultiplayerConfigMigrationPipeline
    {
        /// <summary>Мігрує конфігурацію до поточної версії без зміни вихідного об'єкта.</summary>
        public static MultiplayerConfig MigrateToLatest(MultiplayerConfig source)
        {
            if (source == null)
                return MultiplayerConfig.Default();

            if (source.SchemaVersion <= 0)
            {
                return MultiplayerConfig.Default();
            }

            var migrated = source;
            while (migrated.SchemaVersion < MultiplayerConfig.CurrentSchemaVersion)
            {
                migrated = migrated.SchemaVersion switch
                {
                    1 => MigrateV1ToV2(migrated),
                    2 => MigrateV2ToV3(migrated),
                    3 => MigrateV3ToV4(migrated),
                    4 => MigrateV4ToV5(migrated),
                    _ => MigrateUnknownLegacy(migrated)
                };
            }

            return migrated;
        }

        private static MultiplayerConfig MigrateV1ToV2(MultiplayerConfig source)
        {
            return new MultiplayerConfig(
                schemaVersion: 2,
                providerType: source.ProviderType,
                defaultSessionRules: source.DefaultSessionRules,
                strictParticipantLock: source.StrictParticipantLock,
                enforceConfigConsistency: source.EnforceConfigConsistency,
                matchmakingEnabled: source.MatchmakingEnabled,
                relaySettings: source.RelaySettings,
                webSocketSettings: source.WebSocketSettings,
                fallbackProviderType: source.FallbackProviderType,
                reconnectLocalTimeToleranceSeconds: 120f,
                enableRelayProvider: true,
                enableHostMigration: true);
        }

        private static MultiplayerConfig MigrateV2ToV3(MultiplayerConfig source)
        {
            return new MultiplayerConfig(
                schemaVersion: 3,
                providerType: source.ProviderType,
                defaultSessionRules: source.DefaultSessionRules,
                strictParticipantLock: source.StrictParticipantLock,
                enforceConfigConsistency: source.EnforceConfigConsistency,
                matchmakingEnabled: source.MatchmakingEnabled,
                relaySettings: source.RelaySettings,
                webSocketSettings: source.WebSocketSettings,
                fallbackProviderType: source.FallbackProviderType,
                reconnectLocalTimeToleranceSeconds: source.ReconnectLocalTimeToleranceSeconds,
                enableRelayProvider: true,
                enableHostMigration: true);
        }

        private static MultiplayerConfig MigrateV3ToV4(MultiplayerConfig source)
        {
            return new MultiplayerConfig(
                schemaVersion: 4,
                providerType: source.ProviderType,
                defaultSessionRules: source.DefaultSessionRules,
                strictParticipantLock: source.StrictParticipantLock,
                enforceConfigConsistency: source.EnforceConfigConsistency,
                matchmakingEnabled: source.MatchmakingEnabled,
                relaySettings: source.RelaySettings,
                webSocketSettings: source.WebSocketSettings,
                fallbackProviderType: source.FallbackProviderType,
                reconnectLocalTimeToleranceSeconds: source.ReconnectLocalTimeToleranceSeconds,
                gracefulReconnectWindowSeconds: source.GracefulReconnectWindowSeconds,
                enableRelayProvider: source.EnableRelayProvider,
                enableHostMigration: source.EnableHostMigration);
        }

        private static MultiplayerConfig MigrateV4ToV5(MultiplayerConfig source)
        {
            return new MultiplayerConfig(
                schemaVersion: 5,
                providerType: source.ProviderType,
                defaultSessionRules: source.DefaultSessionRules,
                strictParticipantLock: source.StrictParticipantLock,
                enforceConfigConsistency: source.EnforceConfigConsistency,
                matchmakingEnabled: source.MatchmakingEnabled,
                relaySettings: source.RelaySettings,
                webSocketSettings: source.WebSocketSettings,
                fallbackProviderType: source.FallbackProviderType,
                reconnectLocalTimeToleranceSeconds: source.ReconnectLocalTimeToleranceSeconds,
                gracefulReconnectWindowSeconds: 8f,
                enableRelayProvider: source.EnableRelayProvider,
                enableHostMigration: source.EnableHostMigration);
        }

        private static MultiplayerConfig MigrateUnknownLegacy(MultiplayerConfig source)
        {
            return new MultiplayerConfig(
                schemaVersion: MultiplayerConfig.CurrentSchemaVersion,
                providerType: NormalizeProvider(source.ProviderType),
                defaultSessionRules: source.DefaultSessionRules ?? SessionRules.Default(),
                strictParticipantLock: source.StrictParticipantLock,
                enforceConfigConsistency: source.EnforceConfigConsistency,
                matchmakingEnabled: source.MatchmakingEnabled,
                relaySettings: source.RelaySettings,
                webSocketSettings: source.WebSocketSettings,
                fallbackProviderType: NormalizeFallback(source.FallbackProviderType),
                reconnectLocalTimeToleranceSeconds: source.ReconnectLocalTimeToleranceSeconds,
                gracefulReconnectWindowSeconds: source.GracefulReconnectWindowSeconds,
                enableRelayProvider: source.EnableRelayProvider,
                enableHostMigration: source.EnableHostMigration);
        }

        private static NetworkProviderType NormalizeProvider(NetworkProviderType type)
        {
            return System.Enum.IsDefined(typeof(NetworkProviderType), type)
                ? type
                : NetworkProviderType.Offline;
        }

        private static NetworkProviderType NormalizeFallback(NetworkProviderType type)
        {
            return System.Enum.IsDefined(typeof(NetworkProviderType), type)
                ? type
                : NetworkProviderType.Offline;
        }
    }
}
