using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Runtime
{
    internal sealed class MapVisualTileIdResolver : IMapVisualTileIdResolver
    {
        private readonly Dictionary<string, TileTypeDefinition> _definitions = new();
        private readonly HashSet<string> _loggedFallbacks = new();

        public MapVisualTileIdResolver(TileRegistrySO tileRegistry)
        {
            if (tileRegistry?.Definitions == null)
                return;
            foreach (var definition in tileRegistry.Definitions)
            {
                if (definition != null && !string.IsNullOrEmpty(definition.Id))
                    _definitions[definition.Id] = definition;
            }
        }

        public string ResolveGridTileId(string tileId)
        {
            return TryResolve(tileId, out _, out string resolvedTileId) ? resolvedTileId : tileId;
        }

        public bool TryResolve(string tileId, out TileTypeDefinition tileType, out string resolvedTileId)
        {
            resolvedTileId = tileId;
            if (TryGet(tileId, out tileType))
                return true;

            foreach (string fallback in MapVisualTileFallbacks.Resolve(tileId))
            {
                if (!TryGet(fallback, out tileType))
                    continue;
                resolvedTileId = fallback;
                LogFallback(tileId, fallback);
                return true;
            }

            tileType = null;
            return false;
        }

        private bool TryGet(string tileId, out TileTypeDefinition tileType)
        {
            if (!string.IsNullOrEmpty(tileId) && _definitions.TryGetValue(tileId, out tileType) && tileType != null)
                return true;
            tileType = null;
            return false;
        }

        private void LogFallback(string sourceTileId, string fallbackTileId)
        {
            string key = $"{sourceTileId}->{fallbackTileId}";
            if (_loggedFallbacks.Add(key))
                Debug.LogWarning($"[MapInstantiator] Tile ID '{sourceTileId}' відсутній у поточному реєстрі. Використано сумісний fallback '{fallbackTileId}'.");
        }
    }
}
