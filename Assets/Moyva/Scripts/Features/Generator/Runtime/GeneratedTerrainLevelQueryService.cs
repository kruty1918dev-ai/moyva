using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Generator.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    public sealed class GeneratedTerrainLevelQueryService : IGeneratedTerrainLevelQuery
    {
        private readonly IGeneratorTerrainLevelService _terrainLevelService;

        [Inject]
        public GeneratedTerrainLevelQueryService(IGeneratorTerrainLevelService terrainLevelService)
        {
            _terrainLevelService = terrainLevelService;
        }

        public bool HasExplicitTerrainSurfaceMap => _terrainLevelService?.HasExplicitSurfaceHeightMap ?? false;

        public bool TryGetTerrainLevel(Vector2Int position, out int level)
        {
            if (_terrainLevelService == null)
            {
                level = 0;
                return false;
            }

            return _terrainLevelService.TryGetLevel(position, out level);
        }

        public bool TryGetTerrainSurfaceY(Vector2Int position, out float surfaceY)
        {
            if (_terrainLevelService == null)
            {
                surfaceY = 0f;
                return false;
            }

            return _terrainLevelService.TryGetSurfaceHeight(position, out surfaceY);
        }
    }
}
