using Kruty1918.Moyva.Multiplayer.Config;

namespace Kruty1918.Moyva.Multiplayer.Config
{
    /// <summary>
    /// Updates local config when host's config is authoritative.
    /// Carcass only.
    /// </summary>
    public interface IConfigSyncService
    {
        void SyncFromHost(MultiplayerConfig hostConfig);
    }
}
