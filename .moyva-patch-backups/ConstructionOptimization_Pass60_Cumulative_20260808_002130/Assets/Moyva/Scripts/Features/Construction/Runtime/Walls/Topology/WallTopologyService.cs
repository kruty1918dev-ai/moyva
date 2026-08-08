using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.ObjectsMap.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class WallTopologyService : IWallTopologyService
    {
        private readonly LazyInject<IConstructionService> _constructionService;
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IObjectsMapService _objectsMapService;

        [Inject]
        public WallTopologyService(
            LazyInject<IConstructionService> constructionService,
            IBuildingRegistry buildingRegistry,
            IObjectsMapService objectsMapService)
        {
            _constructionService = constructionService;
            _buildingRegistry = buildingRegistry;
            _objectsMapService = objectsMapService;
        }

        public bool IsWallOrGate(string buildingId)
        {
            return _buildingRegistry.GetWallCollectionByBuildingId(buildingId) != null;
        }

        public bool IsWall(string buildingId)
        {
            var collection = _buildingRegistry.GetWallCollectionByBuildingId(buildingId);
            return collection != null && collection.IsWall(buildingId);
        }

        public bool IsGate(string buildingId)
        {
            var collection = _buildingRegistry.GetWallCollectionByBuildingId(buildingId);
            return collection != null && collection.IsGate(buildingId);
        }

        public bool TryBuildPlacedMask(Vector2Int position, string buildingId, out WallCollectionDefinition collection, out TopologyNeighborMask mask)
        {
            if (!TryGetCollection(buildingId, out collection))
            {
                mask = default;
                return false;
            }

            mask = BuildMask(position, collection, includePendingNeighbors: false);
            return true;
        }

        public bool TryBuildPreviewMask(Vector2Int position, string buildingId, out WallCollectionDefinition collection, out TopologyNeighborMask mask)
        {
            if (!TryGetCollection(buildingId, out collection))
            {
                mask = default;
                return false;
            }

            mask = BuildMask(position, collection, includePendingNeighbors: true);
            return true;
        }

        private bool TryGetCollection(string buildingId, out WallCollectionDefinition collection)
        {
            collection = _buildingRegistry.GetWallCollectionByBuildingId(buildingId);
            return collection != null;
        }

        private TopologyNeighborMask BuildMask(Vector2Int position, WallCollectionDefinition collection, bool includePendingNeighbors)
        {
            bool n = IsConnected(position + Vector2Int.up, collection, includePendingNeighbors);
            bool e = IsConnected(position + Vector2Int.right, collection, includePendingNeighbors);
            bool s = IsConnected(position + Vector2Int.down, collection, includePendingNeighbors);
            bool w = IsConnected(position + Vector2Int.left, collection, includePendingNeighbors);

            return new TopologyNeighborMask(
                north: n,
                northEast: false,
                east: e,
                southEast: false,
                south: s,
                southWest: false,
                west: w,
                northWest: false);
        }

        public bool IsHorizontalWallSegment(Vector2Int position, WallCollectionDefinition collection, bool includePendingNeighbors)
        {
            bool n = IsConnected(position + Vector2Int.up, collection, includePendingNeighbors);
            bool e = IsConnected(position + Vector2Int.right, collection, includePendingNeighbors);
            bool s = IsConnected(position + Vector2Int.down, collection, includePendingNeighbors);
            bool w = IsConnected(position + Vector2Int.left, collection, includePendingNeighbors);

            bool hasHorizontalConnection = e || w;
            bool hasVerticalConnection = n || s;
            return hasHorizontalConnection && !hasVerticalConnection;
        }

        private bool IsConnected(Vector2Int position, WallCollectionDefinition collection, bool includePendingNeighbors)
        {
            if (_objectsMapService.TryGetOccupant(position, out var neighborId)
                && collection.ContainsBuilding(neighborId))
            {
                return true;
            }

            if (includePendingNeighbors
                && _constructionService.Value.TryGetPendingBuildingIdAt(position, out var pendingBuildingId)
                && collection.ContainsBuilding(pendingBuildingId))
            {
                return true;
            }

            return false;
        }
    }
}
