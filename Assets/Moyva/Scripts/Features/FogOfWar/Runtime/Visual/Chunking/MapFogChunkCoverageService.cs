using Kruty1918.Moyva.FogOfWar.API;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.FogOfWar.Runtime
{
    internal sealed class MapFogChunkCoverageService : IMapFogChunkCoverageService
    {
        private readonly FogOfWarService _fogService;

        public MapFogChunkCoverageService([InjectOptional] FogOfWarService fogService = null)
        {
            _fogService = fogService;
        }

        public bool IsChunkFullyHidden(MapChunkDescriptor descriptor)
        {
            if (_fogService == null || !_fogService.IsReady || descriptor.TileCount <= 0)
                return false;

            RectInt rect = descriptor.TileRect;
            for (int x = rect.xMin; x < rect.xMax; x++)
            for (int y = rect.yMin; y < rect.yMax; y++)
            {
                if (_fogService.GetFogState(new Vector2Int(x, y)) != FogStateType.Unexplored)
                    return false;
            }

            return true;
        }
    }
}
