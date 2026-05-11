using System;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Networking;

namespace Kruty1918.Moyva.Multiplayer.Config
{
    /// <summary>
    /// Unified runtime lifecycle for multiplayer config:
    /// Load -> Validate -> Freeze.
    /// </summary>
    public static class MultiplayerConfigLifecycle
    {
        public static MultiplayerConfig LoadValidateFreeze(IConfigStore store, IMultiplayerLogger logger = null)
        {
            if (store == null)
                throw new ArgumentNullException(nameof(store));

            MultiplayerConfig loaded = store.Exists() ? store.Load() : MultiplayerConfig.Default();
            return ValidateAndFreeze(loaded, logger);
        }

        public static MultiplayerConfig ValidateAndFreeze(MultiplayerConfig config, IMultiplayerLogger logger = null)
        {
            bool corrected = false;

            if (config == null)
            {
                corrected = true;
                config = MultiplayerConfig.Default();
            }

            int schemaVersion = MultiplayerConfig.CurrentSchemaVersion;
            if (schemaVersion != config.SchemaVersion)
                corrected = true;

            NetworkProviderType providerType = IsDefinedNetworkProvider(config.ProviderType)
                ? config.ProviderType
                : NetworkProviderType.Offline;
            if (providerType != config.ProviderType)
                corrected = true;

            NetworkProviderType fallbackProviderType = IsDefinedNetworkProvider(config.FallbackProviderType)
                ? config.FallbackProviderType
                : NetworkProviderType.Offline;
            if (fallbackProviderType != config.FallbackProviderType)
                corrected = true;

            SessionRules rules = config.DefaultSessionRules ?? SessionRules.Default();
            if (config.DefaultSessionRules == null)
                corrected = true;

            SessionMode mode = Enum.IsDefined(typeof(SessionMode), rules.Mode)
                ? rules.Mode
                : SessionMode.MultiplayerHumans;
            if (mode != rules.Mode)
                corrected = true;

            int maxParticipants = rules.MaxParticipants >= 1 ? rules.MaxParticipants : 1;
            if (maxParticipants != rules.MaxParticipants)
                corrected = true;

            int maxHumans = Clamp(rules.MaxHumans, 0, maxParticipants);
            if (maxHumans != rules.MaxHumans)
                corrected = true;

            int maxBots = Clamp(rules.MaxBots, 0, Math.Max(0, maxParticipants - maxHumans));
            if (maxBots != rules.MaxBots)
                corrected = true;

            if (maxHumans == 0 && maxBots == 0)
            {
                maxHumans = 1;
                corrected = true;
            }

            float reconnectTolerance = config.ReconnectLocalTimeToleranceSeconds >= 0f
                ? config.ReconnectLocalTimeToleranceSeconds
                : 0f;
            if (Math.Abs(reconnectTolerance - config.ReconnectLocalTimeToleranceSeconds) > 0.0001f)
                corrected = true;

            float gracefulReconnectWindow = config.GracefulReconnectWindowSeconds >= 1f
                ? config.GracefulReconnectWindowSeconds
                : 1f;
            if (Math.Abs(gracefulReconnectWindow - config.GracefulReconnectWindowSeconds) > 0.0001f)
                corrected = true;

            RelayProviderSettings relay = FreezeRelay(config.RelaySettings);
            if (config.RelaySettings == null)
                corrected = true;

            WebSocketProviderSettings webSocket = FreezeWebSocket(config.WebSocketSettings);
            if (config.WebSocketSettings == null)
                corrected = true;

            SessionRules frozenRules = new SessionRules(
                mode,
                maxParticipants,
                maxHumans,
                maxBots,
                rules.AllowBotsFallbackOnLeave,
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

            if (corrected)
                logger?.Warn("Multiplayer config normalized during runtime lifecycle (Load/Validate/Freeze).");

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

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }
    }
}