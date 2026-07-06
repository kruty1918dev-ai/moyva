using System.Collections.Generic;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionTileSurfaceOffsetService : IConstructionTileSurfaceOffsetService
    {
        private readonly Dictionary<string, float> _offsetYByTileId = new();
        private readonly TileRegistrySO _tileRegistry;

        [Inject]
        public ConstructionTileSurfaceOffsetService([InjectOptional] TileRegistrySO tileRegistry = null)
        {
            _tileRegistry = tileRegistry;
        }

        public bool TryResolveTileSurfaceOffsetY(string tileId, out float offsetY)
        {
            offsetY = 0f;
            if (string.IsNullOrWhiteSpace(tileId) || _tileRegistry?.Definitions == null)
                return false;

            if (_offsetYByTileId.TryGetValue(tileId, out offsetY))
                return true;

            return TryCacheTileOffset(tileId, out offsetY);
        }

        private bool TryCacheTileOffset(string tileId, out float offsetY)
        {
            offsetY = 0f;
            for (int i = 0; i < _tileRegistry.Definitions.Length; i++)
            {
                var definition = _tileRegistry.Definitions[i];
                GameObject surfacePrefab = definition?.SurfaceReferencePrefab;
                if (definition == null || definition.Id != tileId || surfacePrefab == null)
                    continue;

                if (!GridSurfacePlacementUtility.TryResolveTopOffsetY(surfacePrefab, out offsetY))
                    offsetY = 0f;

                _offsetYByTileId[tileId] = offsetY;
                return true;
            }

            return false;
        }
    }
}
