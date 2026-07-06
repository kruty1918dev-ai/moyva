using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.ObjectsMap.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionWallVisualRefreshService : IConstructionWallVisualRefreshService
    {
        private readonly IObjectsMapService _objectsMapService;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly LazyInject<IConstructionService> _constructionService;
        private readonly IWallVisualResolver _wallVisualResolver;
        private readonly IConstructionPlacedVisualService _placedVisuals;
        private readonly IConstructionPreviewVisualService _previewVisuals;

        [Inject]
        public ConstructionWallVisualRefreshService(
            IObjectsMapService objectsMapService,
            IBuildingRegistry buildingRegistry,
            LazyInject<IConstructionService> constructionService,
            IWallVisualResolver wallVisualResolver,
            IConstructionPlacedVisualService placedVisuals,
            IConstructionPreviewVisualService previewVisuals)
        {
            _objectsMapService = objectsMapService;
            _buildingRegistry = buildingRegistry;
            _constructionService = constructionService;
            _wallVisualResolver = wallVisualResolver;
            _placedVisuals = placedVisuals;
            _previewVisuals = previewVisuals;
        }

        public void RefreshPlacedNeighborhood(Vector2Int center)
        {
            RefreshPlacedAt(center);
            RefreshPlacedAt(center + Vector2Int.up);
            RefreshPlacedAt(center + Vector2Int.right);
            RefreshPlacedAt(center + Vector2Int.down);
            RefreshPlacedAt(center + Vector2Int.left);
        }

        public void RefreshPreviewNeighborhood(Vector2Int center, string buildingId)
        {
            RefreshPreviewAt(center + Vector2Int.up, buildingId);
            RefreshPreviewAt(center + Vector2Int.right, buildingId);
            RefreshPreviewAt(center + Vector2Int.down, buildingId);
            RefreshPreviewAt(center + Vector2Int.left, buildingId);
        }

        private void RefreshPlacedAt(Vector2Int position)
        {
            if (!_objectsMapService.TryGetOccupant(position, out string occupantId))
            {
                _placedVisuals.Remove(position);
                return;
            }

            if (!_wallVisualResolver.TryResolvePlacedVisual(position, occupantId, out GameObject prefab, out Quaternion rotation))
                return;

            _placedVisuals.Replace(position, occupantId, prefab, rotation, ResolveVisualYOffset(occupantId));
        }

        private void RefreshPreviewAt(Vector2Int position, string fallbackBuildingId)
        {
            if (!_previewVisuals.Has(position))
                return;

            string buildingId = ResolvePreviewBuildingId(position, fallbackBuildingId);
            if (_wallVisualResolver.TryResolvePreviewVisual(position, buildingId, out GameObject prefab))
                _previewVisuals.ReplaceWallPreview(position, buildingId, prefab, ResolveVisualYOffset(buildingId));
        }

        private string ResolvePreviewBuildingId(Vector2Int position, string fallbackBuildingId)
        {
            if (_constructionService.Value.TryGetPendingBuildingIdAt(position, out string pendingId)
                && !string.IsNullOrWhiteSpace(pendingId))
            {
                return pendingId;
            }

            return fallbackBuildingId;
        }

        private float ResolveVisualYOffset(string buildingId)
            => _buildingRegistry.GetById(buildingId)?.VisualYOffset ?? 0f;
    }
}
