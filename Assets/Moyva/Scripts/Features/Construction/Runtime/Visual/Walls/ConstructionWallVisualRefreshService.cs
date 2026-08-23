using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.ObjectsMap.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionWallVisualRefreshService {
        private readonly IObjectsMapService _objectsMapService;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly LazyInject<IConstructionService> _constructionService;
        private readonly IWallVisualResolver _wallVisualResolver;
        private readonly ConstructionPlacedVisualService _placedVisuals;
        private readonly ConstructionPreviewVisualService _previewVisuals;
        private readonly IConstructionLifecycle _constructionLifecycle;

        [Inject]
        public ConstructionWallVisualRefreshService(
            IObjectsMapService objectsMapService,
            IBuildingRegistry buildingRegistry,
            LazyInject<IConstructionService> constructionService,
            IWallVisualResolver wallVisualResolver,
            ConstructionPlacedVisualService placedVisuals,
            ConstructionPreviewVisualService previewVisuals,
            [InjectOptional] IConstructionLifecycle constructionLifecycle = null)
        {
            _objectsMapService = objectsMapService;
            _buildingRegistry = buildingRegistry;
            _constructionService = constructionService;
            _wallVisualResolver = wallVisualResolver;
            _placedVisuals = placedVisuals;
            _previewVisuals = previewVisuals;
            _constructionLifecycle = constructionLifecycle;
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

            BuildingDefinition def = _buildingRegistry.GetById(occupantId);
            bool isOperational =
                _constructionLifecycle == null
                || _constructionLifecycle.IsOperational(position);
            bool showConstructionVisual =
                def != null
                && def.BuildTurns > 0
                && !isOperational
                && def.ResolveConstructionPrefab() != null;
            if (showConstructionVisual)
            {
                prefab = def.ResolveConstructionPrefab();
                rotation = Quaternion.identity;
            }

            _placedVisuals.Replace(
                position,
                occupantId,
                prefab,
                rotation,
                def?.ResolveVisualYOffset() ?? 0f,
                presentation: def?.Presentation);

            if (showConstructionVisual)
                _placedVisuals.MarkUnderConstruction(position);
        }

        private void RefreshPreviewAt(Vector2Int position, string fallbackBuildingId)
        {
            if (!_previewVisuals.Has(position))
                return;

            string buildingId = ResolvePreviewBuildingId(position, fallbackBuildingId);
            if (_wallVisualResolver.TryResolvePreviewVisual(position, buildingId, out GameObject prefab))
            {
                BuildingDefinition def = _buildingRegistry.GetById(buildingId);
                _previewVisuals.ReplaceWallPreview(
                    position,
                    buildingId,
                    prefab,
                    def?.ResolveVisualYOffset() ?? 0f,
                    def?.Presentation);
            }
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

    }
}
