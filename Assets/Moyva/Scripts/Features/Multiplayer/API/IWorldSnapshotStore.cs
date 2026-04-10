using Kruty1918.Moyva.Multiplayer.Persistence;

namespace Kruty1918.Moyva.Multiplayer.Persistence
{
    /// <summary>
    /// Bridge to the existing save system for world snapshot metadata.
    /// </summary>
    public interface IWorldSnapshotStore
    {
        bool Exists(string worldId);
        WorldSnapshot Load(string worldId);
        void Save(WorldSnapshot snapshot);
    }
}
