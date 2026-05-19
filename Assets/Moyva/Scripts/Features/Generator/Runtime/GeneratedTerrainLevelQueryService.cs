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

        public bool TryGetTerrainLevel(Vector2Int position, out int level)
        {
            if (_terrainLevelService == null)
            {
                level = 0;
                return false;
            }

            return _terrainLevelService.TryGetLevel(position, out level);
        }
    }
}
