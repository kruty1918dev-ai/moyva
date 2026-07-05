using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class WallDragPreviewService : IWallDragPreviewService
    {
        private readonly LazyInject<IConstructionService> _constructionService;
        private readonly IWallPathfinder _wallPathfinder;
        private readonly IScreenToGridConverter _screenToGridConverter;
        private readonly IWallHandleController _wallHandleController;

        [Inject]
        public WallDragPreviewService(
            LazyInject<IConstructionService> constructionService,
            IWallPathfinder wallPathfinder,
            IScreenToGridConverter screenToGridConverter,
            IWallHandleController wallHandleController)
        {
            _constructionService = constructionService;
            _wallPathfinder = wallPathfinder;
            _screenToGridConverter = screenToGridConverter;
            _wallHandleController = wallHandleController;
        }

        public void PreviewDrag(Vector2Int startPosition, Vector2 touchWorldPosition)
        {
            _wallHandleController.TrackDragStart(startPosition);
            Vector2Int endGrid = _screenToGridConverter.WorldToGrid(touchWorldPosition);

            var path = _wallPathfinder.BuildPath(startPosition, endGrid);
            for (int i = 0; i < path.Count; i++)
                _constructionService.Value.TryPreviewAt(path[i]);
        }
    }
}
