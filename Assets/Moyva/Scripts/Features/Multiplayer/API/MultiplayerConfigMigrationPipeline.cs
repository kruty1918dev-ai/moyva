using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.Multiplayer.Config
{
    /// <summary>
    /// Step-by-step migration pipeline for multiplayer config schema upgrades.
    /// Ensures older persisted configs are migrated to the current runtime format.
    /// </summary>
    public static class MultiplayerConfigMigrationPipeline
    {
        public static MultiplayerConfig MigrateToLatest(MultiplayerConfig source, IMultiplayerLogger logger = null)
        {
            if (source == null)
                return MultiplayerConfig.Default();

            if (source.SchemaVersion <= 0)
            {
                logger?.Warn("[MultiplayerConfigMigration] Invalid schema version detected. Falling back to defaults.");
                return MultiplayerConfig.Default();
            }

            var migrated = source;
            while (migrated.SchemaVersion < MultiplayerConfig.CurrentSchemaVersion)
            {
                migrated = migrated.SchemaVersion switch
                {
                    1 => MigrateV1ToV2(migrated, logger),
                    2 => MigrateV2ToV3(migrated, logger),
                    3 => MigrateV3ToV4(migrated, logger),
                    4 => MigrateV4ToV5(migrated, logger),
                    _ => MigrateUnknownLegacy(migrated, logger)
                };
            }

            return migrated;
        }

        private static MultiplayerConfig MigrateV1ToV2(MultiplayerConfig source, IMultiplayerLogger logger)
        {
            logger?.Info("[MultiplayerConfigMigration] Migrating config v1 -> v2.");
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

        private static MultiplayerConfig MigrateV2ToV3(MultiplayerConfig source, IMultiplayerLogger logger)
        {
            logger?.Info("[MultiplayerConfigMigration] Migrating config v2 -> v3.");
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

        private static MultiplayerConfig MigrateV3ToV4(MultiplayerConfig source, IMultiplayerLogger logger)
        {
            logger?.Info("[MultiplayerConfigMigration] Migrating config v3 -> v4.");
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

            private static MultiplayerConfig MigrateV4ToV5(MultiplayerConfig source, IMultiplayerLogger logger)
            {
                logger?.Info("[MultiplayerConfigMigration] Migrating config v4 -> v5.");
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

        private static MultiplayerConfig MigrateUnknownLegacy(MultiplayerConfig source, IMultiplayerLogger logger)
        {
            logger?.Warn($"[MultiplayerConfigMigration] Unsupported legacy schema '{source.SchemaVersion}'. Rebuilding to current schema with safe defaults.");
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