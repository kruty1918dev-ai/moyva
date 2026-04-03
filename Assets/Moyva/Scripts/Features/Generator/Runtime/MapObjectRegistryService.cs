using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MapObjectRegistryService : IMapObjectRegistryService
    {
        private readonly Dictionary<string, MapObjectDefinition> _cache = new();

        public MapObjectRegistryService(MapObjectRegistrySO registry)
        {
            if (registry?.Definitions == null) return;
            foreach (var def in registry.Definitions)
            {
                if (!string.IsNullOrEmpty(def.Id))
                    _cache[def.Id] = def;
            }
        }

        public bool TryGetDefinition(string id, out MapObjectDefinition definition)
            => _cache.TryGetValue(id, out definition);
    }
}
