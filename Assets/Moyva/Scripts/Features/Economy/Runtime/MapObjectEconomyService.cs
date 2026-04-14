using System.Collections.Generic;
using Kruty1918.Moyva.Economy.API;

namespace Kruty1918.Moyva.Economy.Runtime
{
    internal sealed class MapObjectEconomyService : IMapObjectEconomyService
    {
        private readonly Dictionary<string, MapObjectEconomyEntry> _lookup =
            new Dictionary<string, MapObjectEconomyEntry>();

        public MapObjectEconomyService(EconomyDatabaseSO database)
        {
            if (database == null) return;
            foreach (var entry in database.MapObjectEconomyEntries)
            {
                if (entry == null || string.IsNullOrEmpty(entry.MapObjectId)) continue;
                _lookup[entry.MapObjectId] = entry;
            }
        }

        public bool TryGetEntry(string mapObjectId, out MapObjectEconomyEntry entry)
        {
            if (string.IsNullOrEmpty(mapObjectId))
            {
                entry = null;
                return false;
            }
            return _lookup.TryGetValue(mapObjectId, out entry);
        }

        public bool IsInteractable(string mapObjectId)
        {
            return TryGetEntry(mapObjectId, out var e) && e.IsInteractable;
        }
    }
}
