using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Multiplayer.Networking
{
    /// <summary>
    /// Створює мережевий provider та, за потреби, резервний transport-ланцюжок.
    /// </summary>
    public static class NetworkProviderFactory
    {
        /// <summary>
        /// Створює provider відповідно до конфігурації.
        /// </summary>
        /// <param name="config">Loaded multiplayer configuration.</param>
        /// <returns>
        /// If <c>ProviderType == FallbackProviderType</c> or <c>ProviderType == Offline</c>,
        /// returns a single provider.  Otherwise returns a
        /// <see cref="FallbackNetworkProvider"/> that tries the primary first and
        /// promotes the fallback on failure.
        /// </returns>
        public static INetworkProvider Create(MultiplayerConfig config, IMultiplayerQosMonitorService qosMonitor = null)
        {
            var primary = CreateSingle(config.ProviderType, config, qosMonitor);

            // Relay lobbies publish Relay join codes into UGS. Falling back to a non-Relay
            // transport would create a visible UGS room that clients cannot actually join.
            if (config.ProviderType == NetworkProviderType.Offline ||
                config.ProviderType == NetworkProviderType.Relay ||
                config.ProviderType == config.FallbackProviderType)
            {
                return primary;
            }

            var fallback = CreateSingle(config.FallbackProviderType, config, qosMonitor);
            return new FallbackNetworkProvider(primary, fallback);
        }

        /// <summary>
        /// Створює provider для вказаного типу з резервним transport за правилами конфігурації.
        /// </summary>
        public static INetworkProvider CreateByType(NetworkProviderType type, MultiplayerConfig config, IMultiplayerQosMonitorService qosMonitor = null)
        {
            var primary = CreateSingle(type, config, qosMonitor);

            if (type == NetworkProviderType.Offline ||
                type == NetworkProviderType.Relay ||
                type == config.FallbackProviderType)
                return primary;

            var fallback = CreateSingle(config.FallbackProviderType, config, qosMonitor);
            return new FallbackNetworkProvider(primary, fallback);
        }

        // ── Internal ───────────────────────────────────────────────────────────────

        private static INetworkProvider CreateSingle(
            NetworkProviderType type,
            MultiplayerConfig config,
            IMultiplayerQosMonitorService qosMonitor)
        {
            return type switch
            {
                NetworkProviderType.Relay     => CreateRelayOrFallback(config),
                NetworkProviderType.WebSocket => new WebSocketNetworkProvider(config.WebSocketSettings, qosMonitor),
                NetworkProviderType.Lan      => new LanNetworkProvider(config),
                _                             => new OfflineNetworkProvider()
            };
        }

        private static INetworkProvider CreateRelayOrFallback(MultiplayerConfig config)
        {
            if (!config.EnableRelayProvider)
            {
                return new OfflineNetworkProvider();
            }

            if (!RelayNetworkProvider.IsRuntimeAvailable)
            {
                return new OfflineNetworkProvider();
            }

            if (RelayNetworkProvider.TryValidateReflectionBindings(out _))
                return new RelayNetworkProvider(config.RelaySettings);
            return new OfflineNetworkProvider();
        }
    }
}
