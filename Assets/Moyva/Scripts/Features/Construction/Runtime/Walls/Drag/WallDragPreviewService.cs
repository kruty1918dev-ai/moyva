using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class WallDragPreviewService {
        private readonly LazyInject<IConstructionSessionCommands> _constructionService;
        private readonly IWallPathfinder _wallPathfinder;
        private readonly IScreenToGridConverter _screenToGridConverter;
        private readonly WallHandleController _wallHandleController;
        private readonly ConstructionBuildGridTileFilter _buildGridTileFilter;

        [Inject]
        public WallDragPreviewService(
            LazyInject<IConstructionSessionCommands> constructionService,
            IWallPathfinder wallPathfinder,
            IScreenToGridConverter screenToGridConverter,
            WallHandleController wallHandleController,
            [InjectOptional] ConstructionBuildGridTileFilter buildGridTileFilter = null)
        {
            _constructionService = constructionService;
            _wallPathfinder = wallPathfinder;
            _screenToGridConverter = screenToGridConverter;
            _wallHandleController = wallHandleController;
            _buildGridTileFilter = buildGridTileFilter;
        }

        public void PreviewDrag(Vector2Int startPosition, Vector2 touchWorldPosition)
        {
            _wallHandleController.TrackDragStart(startPosition);
            Vector2Int endGrid = _screenToGridConverter.WorldToGrid(touchWorldPosition);

            var path = _wallPathfinder.BuildPath(startPosition, endGrid);
            string buildingId = _constructionService.Value.GetSelectedBuildingId();
            for (int i = 0; i < path.Count; i++)
            {
                if (_buildGridTileFilter != null
                    && !_buildGridTileFilter.ShouldRenderForPlacement(path[i], buildingId))
                {
                    break;
                }

                _constructionService.Value.TryPreviewAt(path[i]);
            }
        }
    }
}
