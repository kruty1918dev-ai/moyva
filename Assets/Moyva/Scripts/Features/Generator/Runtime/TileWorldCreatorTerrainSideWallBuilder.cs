using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Generator.Runtime
{
    [DisallowMultipleComponent]
    public sealed class TileWorldCreatorTerrainSideWallBuilder : MonoBehaviour
    {
        private readonly TileWorldCreatorTerrainSideWallState _state = new TileWorldCreatorTerrainSideWallState();
        private ITileWorldCreatorTerrainSideWallService _service;

        [Inject]
        private void Construct([InjectOptional] ITileWorldCreatorTerrainSideWallService service = null)
        {
            _service = service;
        }

        public void Configure(
            Transform targetRoot,
            int[,] terrainLevelMap,
            float cellSize,
            int heightStep,
            float baseY,
            Material materialOverride,
            Color wallColor,
            bool includeMapBoundaryWalls)
        {
            var config = new TileWorldCreatorTerrainSideWallConfig(
                targetRoot,
                terrainLevelMap,
                cellSize,
                heightStep,
                baseY,
                materialOverride,
                wallColor,
                includeMapBoundaryWalls);

            ResolveService().Configure(_state, this, config);
        }

        public void RebuildFromLastConfiguration(string reason)
        {
            ResolveService().RebuildFromLastConfiguration(_state, this, reason);
        }

        public void ClearWalls(string reason)
        {
            ResolveService().ClearWalls(_state, reason);
        }

        private void OnDestroy()
        {
            ResolveService().Dispose(_state);
        }

        private ITileWorldCreatorTerrainSideWallService ResolveService()
        {
            return _service ??= TileWorldCreatorTerrainSideWallComposition.Create();
        }
    }
}
