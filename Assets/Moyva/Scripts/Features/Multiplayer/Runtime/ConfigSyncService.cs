using Kruty1918.Moyva.Multiplayer.Config;
using Kruty1918.Moyva.Multiplayer.Core;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Синхронізує локальну конфігурацію з авторитетною конфігурацією хоста.
    /// </summary>
    internal sealed class ConfigSyncService : IConfigSyncService
    {
        /// <summary>Остання завантажена конфігурація від хоста.</summary>
        public MultiplayerConfig LoadedConfig { get; private set; }

        public void SyncFromHost(MultiplayerConfig hostConfig)
        {
            LoadedConfig = hostConfig;
        }
    }
}
