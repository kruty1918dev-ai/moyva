using Kruty1918.Moyva.Construction.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class WallVisualResolver : IWallVisualResolver
    {
        private readonly IBuildingRegistry _buildingRegistry;
        private readonly IObjectTypePicker _objectTypePicker;
        private readonly IWallTopologyService _wallTopologyService;
        private readonly IWallPrefabResolver _wallPrefabResolver;

        [Inject]
        public WallVisualResolver(
            IBuildingRegistry buildingRegistry,
            IObjectTypePicker objectTypePicker,
            IWallTopologyService wallTopologyService,
            IWallPrefabResolver wallPrefabResolver)
        {
            _buildingRegistry = buildingRegistry;
            _objectTypePicker = objectTypePicker;
            _wallTopologyService = wallTopologyService;
            _wallPrefabResolver = wallPrefabResolver;
        }

        public bool TryResolvePlacedVisual(Vector2Int position, string occupantId, out GameObject prefab, out Quaternion rotation)
        {
            rotation = Quaternion.identity;
            return TryResolveVisual(position, occupantId, preview: false, out prefab);
        }

        public bool TryResolvePreviewVisual(Vector2Int position, string buildingId, out GameObject prefab)
        {
            return TryResolveVisual(position, buildingId, preview: true, out prefab);
        }

        private bool TryResolveVisual(Vector2Int position, string buildingId, bool preview, out GameObject prefab)
        {
            prefab = null;

            var collection = _buildingRegistry.GetWallCollectionByBuildingId(buildingId);
            if (collection == null)
                return false;

            if (collection.IsGate(buildingId))
            {
                prefab = collection.GatePrefab;
                if (prefab == null)
                    prefab = _buildingRegistry.GetById(buildingId)?.Prefab;
                return prefab != null;
            }

            if (!collection.IsWall(buildingId))
                return false;

            if (!TryResolveMask(position, buildingId, preview, out var mask))
                return false;

            if (_objectTypePicker.TryPickId(buildingId, mask, out var resolvedId))
                prefab = _buildingRegistry.GetById(resolvedId)?.Prefab;

            if (prefab == null)
                prefab = _wallPrefabResolver.ResolvePrefab(collection, buildingId, mask);

            if (prefab == null)
                prefab = _buildingRegistry.GetById(buildingId)?.Prefab;

            return prefab != null;
        }

        private bool TryResolveMask(Vector2Int position, string buildingId, bool preview, out TopologyNeighborMask mask)
        {
            if (preview)
                return _wallTopologyService.TryBuildPreviewMask(position, buildingId, out _, out mask);

            return _wallTopologyService.TryBuildPlacedMask(position, buildingId, out _, out mask);
        }
    }
}
