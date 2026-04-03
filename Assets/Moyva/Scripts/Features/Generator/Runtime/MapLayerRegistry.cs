using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Grid.API;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MapLayerRegistry : IMapLayerRegistry
    {
        private readonly Dictionary<string, TileTypeDefinition> _tileCache = new();
        private readonly Dictionary<string, MapObjectDefinition> _objectCache = new();

        public MapLayerRegistry(TileRegistrySO tileRegistry, MapObjectRegistrySO objectRegistry)
        {
            if (tileRegistry?.Definitions != null)
            {
                foreach (var def in tileRegistry.Definitions)
                    if (!string.IsNullOrEmpty(def.Id))
                        _tileCache[def.Id] = def;
            }

            if (objectRegistry?.Definitions != null)
            {
                foreach (var def in objectRegistry.Definitions)
                    if (!string.IsNullOrEmpty(def.Id))
                        _objectCache[def.Id] = def;
            }
        }

        public bool TryGetTileDefinition(string id, out TileTypeDefinition definition)
            => _tileCache.TryGetValue(id, out definition);

        public bool TryGetObjectDefinition(string id, out MapObjectDefinition definition)
            => _objectCache.TryGetValue(id, out definition);

        public bool IsKnownId(string id)
            => _tileCache.ContainsKey(id) || _objectCache.ContainsKey(id);
    }
}
