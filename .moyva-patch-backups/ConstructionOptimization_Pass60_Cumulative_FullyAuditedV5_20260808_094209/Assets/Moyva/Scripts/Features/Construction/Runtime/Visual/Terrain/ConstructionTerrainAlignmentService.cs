using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionTerrainAlignmentService : IConstructionTerrainAlignmentService
    {
        private const float BuildingSurfaceOffsetY = 0.5f;
        private const float PreviewSurfaceOffsetY = 0.7f;

        private readonly IGridService _gridService;
        private readonly IGridProjection _gridProjection;
        private readonly IConstructionGridGeometryService _gridGeometry;
        private readonly IConstructionTileSurfaceOffsetService _tileSurfaceOffsets;
        private readonly IConstructionVisualBoundsAlignmentService _boundsAlignment;
        private readonly IGeneratedTerrainLevelQuery _generatedTerrainLevelQuery;
        private readonly float _buildingSurfaceOffsetY;
        private readonly float _previewSurfaceOffsetY;

        [Inject]
        public ConstructionTerrainAlignmentService(
            IGridService gridService,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] IConstructionGridGeometryService gridGeometry = null,
            [InjectOptional] IConstructionTileSurfaceOffsetService tileSurfaceOffsets = null,
            [InjectOptional] IConstructionVisualBoundsAlignmentService boundsAlignment = null,
            [InjectOptional] IGeneratedTerrainLevelQuery generatedTerrainLevelQuery = null,
            [InjectOptional] IConstructionVisualSettingsProvider visualSettingsProvider = null)
        {
            _gridService = gridService;
            _gridProjection = gridProjection;
            _gridGeometry = gridGeometry;
            _tileSurfaceOffsets = tileSurfaceOffsets;
            _boundsAlignment = boundsAlignment;
            _generatedTerrainLevelQuery = generatedTerrainLevelQuery;
            _buildingSurfaceOffsetY = visualSettingsProvider?.BuildingSurfaceOffsetY ?? BuildingSurfaceOffsetY;
            _previewSurfaceOffsetY = visualSettingsProvider?.PreviewSurfaceOffsetY ?? PreviewSurfaceOffsetY;
        }

        public Vector3 ResolveWorldPosition(Vector2Int tile, float layerOffset)
        {
            if (_gridProjection == null)
            {
                Vector3 fallback = ResolveGridCenter(tile);
                fallback.y += layerOffset;
                return fallback;
            }

            if (GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection)
                && TryGetGeneratedTerrainSurfaceY(tile, out _))
            {
                Vector3 position = ResolveGridCenter(tile);
                position.y = ResolveTerrainSurfaceY(tile) + layerOffset;
                return position;
            }

            float elevation = _generatedTerrainLevelQuery != null && _generatedTerrainLevelQuery.TryGetTerrainLevel(tile, out int level)
                ? level
                : 0f;
            Vector3 projected = _gridProjection.GridToWorld(tile, elevation, layerOffset);
            Vector3 center = ResolveGridCenter(tile);
            projected.x = center.x;
            projected.z = center.z;
            return projected;
        }

        public Vector3 ResolveAlignedInstancePosition(GameObject instance, Vector2Int tile, bool isPreviewVisual, float visualOffsetY = 0f)
        {
            Vector3 fallback = ResolveWorldPosition(tile, 0.1f);
            if (instance == null || !GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection))
                return fallback;

            Transform transform = instance.transform;
            Vector3 originalPosition = transform.position;
            AlignInstanceToTerrainSurface(instance, tile, isPreviewVisual, visualOffsetY);
            Vector3 alignedPosition = transform.position;
            transform.position = originalPosition;
            return alignedPosition;
        }

        public void AlignInstanceToTerrainSurface(GameObject instance, Vector2Int tile, bool isPreviewVisual, float visualOffsetY = 0f)
        {
            if (!GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection) || instance == null)
                return;

            Vector3 gridCenter = ResolveGridCenter(tile);
            AlignCenterXZ(instance, gridCenter);

            float surfaceOffset = ResolveVisualSurfaceOffsetY(isPreviewVisual);
            GridSurfacePlacementUtility.AlignBottomToSurface(instance, ResolveTerrainSurfaceY(tile) + surfaceOffset + visualOffsetY, 0f);
            AlignCenterXZ(instance, gridCenter);
        }

        private void AlignCenterXZ(GameObject instance, Vector3 gridCenter)
        {
            if (_boundsAlignment != null)
                _boundsAlignment.AlignCenterXZ(instance, gridCenter);
        }

        private Vector3 ResolveGridCenter(Vector2Int tile)
        {
            if (GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection)
                && _gridGeometry != null
                && _gridGeometry.TryGetCellCenter(tile, out Vector3 center))
            {
                return center;
            }

            return _gridProjection != null
                ? _gridProjection.GridToWorld(tile, 0f, 0f)
                : new Vector3(tile.x, 0f, tile.y);
        }

        private float ResolveVisualSurfaceOffsetY(bool isPreviewVisual)
        {
            if (GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection))
                return 0f;

            return isPreviewVisual ? _previewSurfaceOffsetY : _buildingSurfaceOffsetY;
        }

        private float ResolveTerrainSurfaceY(Vector2Int tile)
        {
            if (!GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection))
                return ResolveWorldPosition(tile, 0f).y;

            bool hasGeneratedSurface = TryGetGeneratedTerrainSurfaceY(tile, out float surfaceY);
            float baseY = hasGeneratedSurface
                ? surfaceY
                : ResolveProjectedTerrainBaseY(tile);

            if (hasGeneratedSurface && _generatedTerrainLevelQuery.HasExplicitTerrainSurfaceMap)
                return baseY;

            if (_gridService.TryGetTileData(tile, out string tileId)
                && _tileSurfaceOffsets != null
                && _tileSurfaceOffsets.TryResolveTileSurfaceOffsetY(tileId, out float offsetY))
            {
                return baseY + offsetY;
            }

            return baseY;
        }

        private bool TryGetGeneratedTerrainSurfaceY(Vector2Int tile, out float surfaceY)
        {
            surfaceY = 0f;
            return _generatedTerrainLevelQuery != null
                && _generatedTerrainLevelQuery.TryGetTerrainSurfaceY(tile, out surfaceY);
        }

        private float ResolveProjectedTerrainBaseY(Vector2Int tile)
        {
            if (_gridProjection == null)
                return 0f;

            float elevation = _generatedTerrainLevelQuery != null && _generatedTerrainLevelQuery.TryGetTerrainLevel(tile, out int level)
                ? level
                : 0f;

            return _gridProjection.GridToWorld(tile, elevation, 0f).y;
        }
    }
}
