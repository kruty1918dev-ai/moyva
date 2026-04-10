using Kruty1918.Moyva.Multiplayer.Config;

namespace Kruty1918.Moyva.Multiplayer.Config
{
    /// <summary>
    /// Runtime-friendly abstraction for loading and saving MultiplayerConfig.
    /// Does NOT depend on UnityEditor.
    /// </summary>
    public interface IConfigStore
    {
        MultiplayerConfig Load();
        void Save(MultiplayerConfig config);
        bool Exists();
    }
}
