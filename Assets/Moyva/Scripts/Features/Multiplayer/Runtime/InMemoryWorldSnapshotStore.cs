using System.Collections.Generic;
using Kruty1918.Moyva.Multiplayer.Core;
using Kruty1918.Moyva.Multiplayer.Persistence;

namespace Kruty1918.Moyva.Multiplayer.Runtime
{
    /// <summary>
    /// Зберігає знімки світу в пам'яті (для тестування та короткострокового кешування).
    /// </summary>
    internal sealed class InMemoryWorldSnapshotStore : IWorldSnapshotStore
    {
        private readonly Dictionary<string, WorldSnapshot> _store = new();

        public bool Exists(string worldId) => _store.ContainsKey(worldId);

        public WorldSnapshot Load(string worldId) =>
            _store.TryGetValue(worldId, out var snapshot) ? snapshot : null;

        public void Save(WorldSnapshot snapshot) =>
            _store[snapshot.WorldId] = snapshot;
    }
}
