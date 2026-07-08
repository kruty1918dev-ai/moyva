using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.MapChunks.API;
using Kruty1918.Moyva.ObjectsMap.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionBuildGridTileFilter : IConstructionBuildGridTileFilter
    {
        private readonly IGridService _gridService;
        private readonly IObjectsMapService _objectsMapService;
        private readonly IConstructionPlacementRulesProvider _placementRulesProvider;
        private readonly IGeneratedTerrainLevelQuery _generatedTerrainLevelQuery;
        private readonly ITileSettingsService _tileSettings;
        private readonly IMapChunkLayoutService _chunkLayout;
        private readonly IMapVisualChunkRegistry _chunkRegistry;

        [Inject]
        public ConstructionBuildGridTileFilter(
            IGridService gridService,
            IObjectsMapService objectsMapService,
            [InjectOptional] IConstructionPlacementRulesProvider placementRulesProvider = null,
            [InjectOptional] IGeneratedTerrainLevelQuery generatedTerrainLevelQuery = null,
            [InjectOptional] ITileSettingsService tileSettings = null,
            [InjectOptional] IMapChunkLayoutService chunkLayout = null,
            [InjectOptional] IMapVisualChunkRegistry chunkRegistry = null)
        {
            _gridService = gridService;
            _objectsMapService = objectsMapService;
            _placementRulesProvider = placementRulesProvider;
            _generatedTerrainLevelQuery = generatedTerrainLevelQuery;
            _tileSettings = tileSettings;
            _chunkLayout = chunkLayout;
            _chunkRegistry = chunkRegistry;
        }

        public bool ShouldRender(Vector2Int position)
        {
            if (_gridService == null || !_gridService.TryGetTileData(position, out _))
                return false;

            if (_objectsMapService != null && _objectsMapService.IsOccupied(position))
                return false;

            if (ConstructionTerrainBuildabilityUtility.IsTerrainBlocked(
                    position,
                    _gridService,
                    _generatedTerrainLevelQuery,
                    _tileSettings,
                    _placementRulesProvider,
                    null,
                    out _))
            {
                return false;
            }

            return IsInActiveChunk(position);
        }

        private bool IsInActiveChunk(Vector2Int position)
        {
            if (_chunkLayout == null || !_chunkLayout.IsConfigured || _chunkRegistry == null)
                return true;

            return _chunkLayout.TryGetChunkCoord(position, out MapChunkCoord coord)
                && _chunkRegistry.IsCameraVisible(coord);
        }
    }
}
