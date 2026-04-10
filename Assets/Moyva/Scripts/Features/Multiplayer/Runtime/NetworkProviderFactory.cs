using Kruty1918.Moyva.Multiplayer.Config;

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
        public static INetworkProvider Create(MultiplayerConfig config, IMultiplayerLogger logger)
        {
            var primary = CreateSingle(config.ProviderType, config, logger);

            // If no real fallback is configured, or primary is already offline, skip wrapping
            if (config.ProviderType == NetworkProviderType.Offline ||
                config.ProviderType == config.FallbackProviderType)
            {
                return primary;
            }

            var fallback = CreateSingle(config.FallbackProviderType, config, logger);
            return new FallbackNetworkProvider(primary, fallback, logger);
        }

        // ── Internal ───────────────────────────────────────────────────────────────

        private static INetworkProvider CreateSingle(
            NetworkProviderType type,
            MultiplayerConfig config,
            IMultiplayerLogger logger)
        {
            return type switch
            {
                NetworkProviderType.Relay     => new RelayNetworkProvider(config.RelaySettings, logger),
                NetworkProviderType.WebSocket => new WebSocketNetworkProvider(config.WebSocketSettings, logger),
                _                             => new OfflineNetworkProvider()
            };
        }
    }
}
