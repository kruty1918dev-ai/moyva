using System;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.Multiplayer.Config
{
    /// <summary>
    /// Завантажує, нормалізує та заморожує конфігурацію мультиплеєра.
    /// </summary>
    public static class MultiplayerConfigLifecycle
    {
        /// <summary>Завантажує конфігурацію зі сховища та повертає валідний runtime-знімок.</summary>
        public static MultiplayerConfig LoadValidateFreeze(IConfigStore store)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));

            MultiplayerConfig loaded = store.Exists() ? store.Load() : MultiplayerConfig.Default();
            return ValidateAndFreeze(loaded);
        }

        /// <summary>Нормалізує конфігурацію та повертає незалежний незмінний знімок.</summary>
        public static MultiplayerConfig ValidateAndFreeze(MultiplayerConfig config)
        {
            if (config == null)
                config = MultiplayerConfig.Default();

            int schemaVersion = MultiplayerConfig.CurrentSchemaVersion;

            NetworkProviderType providerType = IsDefinedNetworkProvider(config.ProviderType)
                ? config.ProviderType
                : NetworkProviderType.Offline;

            NetworkProviderType fallbackProviderType = IsDefinedNetworkProvider(config.FallbackProviderType)
                ? config.FallbackProviderType
                : NetworkProviderType.Offline;

            SessionRules rules = config.DefaultSessionRules ?? SessionRules.Default();

            SessionMode mode = Enum.IsDefined(typeof(SessionMode), rules.Mode)
                ? rules.Mode
                : SessionMode.Multiplayer;

            int maxParticipants = rules.MaxParticipants >= 1 ? rules.MaxParticipants : 1;

            float reconnectTolerance = config.ReconnectLocalTimeToleranceSeconds >= 0f
                ? config.ReconnectLocalTimeToleranceSeconds
                : 0f;

            float gracefulReconnectWindow = config.GracefulReconnectWindowSeconds >= 1f
                ? config.GracefulReconnectWindowSeconds
                : 1f;

            RelayProviderSettings relay = FreezeRelay(config.RelaySettings);

            WebSocketProviderSettings webSocket = FreezeWebSocket(config.WebSocketSettings);

            SessionRules frozenRules = new SessionRules(
                mode,
                maxParticipants,
                rules.AllowMatchSaveForAnalysis,
                rules.StrictParticipantLock);

            MultiplayerConfig frozen = new MultiplayerConfig(
                schemaVersion,
                providerType,
                frozenRules,
                config.StrictParticipantLock,
                config.EnforceConfigConsistency,
                config.MatchmakingEnabled,
                relay,
                webSocket,
                fallbackProviderType,
                reconnectTolerance,
                gracefulReconnectWindow,
                config.EnableRelayProvider,
                config.EnableHostMigration);

            return frozen;
        }

        private static bool IsDefinedNetworkProvider(NetworkProviderType value)
        {
            return Enum.IsDefined(typeof(NetworkProviderType), value);
        }

        private static RelayProviderSettings FreezeRelay(RelayProviderSettings settings)
        {
            RelayProviderSettings source = settings ?? RelayProviderSettings.Default();
            return new RelayProviderSettings(
                source.ProjectId,
                source.Environment,
                source.Region,
                source.MaxConnections);
        }

        private static WebSocketProviderSettings FreezeWebSocket(WebSocketProviderSettings settings)
        {
            WebSocketProviderSettings source = settings ?? WebSocketProviderSettings.Default();
            return new WebSocketProviderSettings(
                source.ServerUrl,
                source.Port,
                source.AuthToken,
                source.ReconnectAttempts,
                source.ReconnectDelaySeconds);
        }

    }
}
