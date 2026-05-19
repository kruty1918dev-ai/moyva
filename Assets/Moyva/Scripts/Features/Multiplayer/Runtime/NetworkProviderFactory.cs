using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    /// <summary>
    /// Creates an <see cref="INetworkProvider"/> from a <see cref="MultiplayerConfig"/>.
    /// When the configured provider differs from the fallback, the result is automatically
    /// wrapped in a <see cref="FallbackNetworkProvider"/> so the system degrades gracefully.
    /// </summary>
    public static class NetworkProviderFactory
    {
        /// <summary>
        /// Builds a provider (or a fallback chain) matching the given config.
        /// </summary>
        /// <param name="config">Loaded multiplayer configuration.</param>
        /// <param name="logger">Logger forwarded to providers.</param>
        /// <returns>
        /// If <c>ProviderType == FallbackProviderType</c> or <c>ProviderType == Offline</c>,
        /// returns a single provider.  Otherwise returns a
        /// <see cref="FallbackNetworkProvider"/> that tries the primary first and
        /// promotes the fallback on failure.
        /// </returns>
        public static INetworkProvider Create(MultiplayerConfig config, IMultiplayerLogger logger, IMultiplayerQosMonitorService qosMonitor = null)
        {
            var primary = CreateSingle(config.ProviderType, config, logger, qosMonitor);

            // Relay lobbies publish Relay join codes into UGS. Falling back to a non-Relay
            // transport would create a visible UGS room that clients cannot actually join.
            if (config.ProviderType == NetworkProviderType.Offline ||
                config.ProviderType == NetworkProviderType.Relay ||
                config.ProviderType == config.FallbackProviderType)
            {
                return primary;
            }

            var fallback = CreateSingle(config.FallbackProviderType, config, logger, qosMonitor);
            return new FallbackNetworkProvider(primary, fallback, logger);
        }

        /// <summary>
        /// Create a provider (or a provider+fallback) for the specified <paramref name="type"/>.
        /// This is a runtime helper for switchable wrappers.
        /// </summary>
        public static INetworkProvider CreateByType(NetworkProviderType type, MultiplayerConfig config, IMultiplayerLogger logger, IMultiplayerQosMonitorService qosMonitor = null)
        {
            var primary = CreateSingle(type, config, logger, qosMonitor);

            if (type == NetworkProviderType.Offline ||
                type == NetworkProviderType.Relay ||
                type == config.FallbackProviderType)
                return primary;

            var fallback = CreateSingle(config.FallbackProviderType, config, logger, qosMonitor);
            return new FallbackNetworkProvider(primary, fallback, logger);
        }

        // ── Internal ───────────────────────────────────────────────────────────────

        private static INetworkProvider CreateSingle(
            NetworkProviderType type,
            MultiplayerConfig config,
            IMultiplayerLogger logger,
            IMultiplayerQosMonitorService qosMonitor)
        {
            return type switch
            {
                NetworkProviderType.Relay     => CreateRelayOrFallback(config, logger),
                NetworkProviderType.WebSocket => new WebSocketNetworkProvider(config.WebSocketSettings, logger, qosMonitor),
                NetworkProviderType.Lan      => new LanNetworkProvider(config, logger),
                _                             => new OfflineNetworkProvider()
            };
        }

        private static INetworkProvider CreateRelayOrFallback(MultiplayerConfig config, IMultiplayerLogger logger)
        {
            if (!config.EnableRelayProvider)
            {
                logger?.Warn("[NetworkProviderFactory] Relay provider is disabled by feature toggle. Falling back to Offline provider.");
                return new OfflineNetworkProvider();
            }

            if (!RelayNetworkProvider.IsRuntimeAvailable)
            {
                logger?.Warn("[NetworkProviderFactory] Relay runtime is unavailable. Falling back to Offline provider.");
                return new OfflineNetworkProvider();
            }

            if (RelayNetworkProvider.TryValidateReflectionBindings(out var error))
                return new RelayNetworkProvider(config.RelaySettings, logger);

            logger?.Warn($"[NetworkProviderFactory] Relay reflection bindings are invalid: {error}. Falling back to Offline provider.");
            return new OfflineNetworkProvider();
        }
    }
}
