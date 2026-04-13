using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Синхронізує локальну конфігурацію з авторитетною конфігурацією хоста.
    /// </summary>
    internal sealed class ConfigSyncService : IConfigSyncService
    {
        private readonly IMultiplayerLogger _logger;

        /// <summary>Остання завантажена конфігурація від хоста.</summary>
        public MultiplayerConfig LoadedConfig { get; private set; }

        public ConfigSyncService(IMultiplayerLogger logger)
        {
            _logger = logger;
        }

        public void SyncFromHost(MultiplayerConfig hostConfig)
        {
            LoadedConfig = hostConfig;
            _logger.Info($"ConfigSyncService: конфігурацію синхронізовано (schema v{hostConfig.SchemaVersion}, provider: {hostConfig.ProviderType})");
        }
    }
}
