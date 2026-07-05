using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.ObjectsMap.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class WallGateReplacementValidator : IWallGateReplacementValidator
    {
        private readonly LazyInject<IConstructionService> _constructionService;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IObjectsMapService _objectsMapService;
        private readonly IWallTopologyService _wallTopologyService;
        private readonly IConstructionWallSettingsProvider _wallSettingsProvider;

        [Inject]
        public WallGateReplacementValidator(
            LazyInject<IConstructionService> constructionService,
            IBuildingRegistry buildingRegistry,
            IObjectsMapService objectsMapService,
            IWallTopologyService wallTopologyService,
            [InjectOptional] IConstructionWallSettingsProvider wallSettingsProvider = null)
        {
            _constructionService = constructionService;
            _buildingRegistry = buildingRegistry;
            _objectsMapService = objectsMapService;
            _wallTopologyService = wallTopologyService;
            _wallSettingsProvider = wallSettingsProvider;
        }

        public bool CanReplaceWallWithGate(Vector2Int position, string gateBuildingId, out string replacedWallId)
        {
            replacedWallId = null;

            if (_wallSettingsProvider != null && !_wallSettingsProvider.AllowGateReplacement)
                return false;

            var collection = _buildingRegistry.GetWallCollectionByBuildingId(gateBuildingId);
            if (collection == null || !collection.IsGate(gateBuildingId))
                return false;

            string wallAtPosition = null;
            if (_objectsMapService.TryGetOccupant(position, out var occupantId) && collection.IsWall(occupantId))
                wallAtPosition = occupantId;

            if (string.IsNullOrWhiteSpace(wallAtPosition)
                && _constructionService.Value.TryGetPendingBuildingIdAt(position, out var pendingId)
                && collection.IsWall(pendingId))
            {
                wallAtPosition = pendingId;
            }

            if (string.IsNullOrWhiteSpace(wallAtPosition))
                return false;

            bool requiresHorizontalWall = _wallSettingsProvider?.GateRequiresHorizontalWall ?? true;
            if (requiresHorizontalWall && !_wallTopologyService.IsHorizontalWallSegment(position, collection, includePendingNeighbors: true))
                return false;

            replacedWallId = wallAtPosition;
            return true;
        }
    }
}
