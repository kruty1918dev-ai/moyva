using System.Collections.Generic;
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
        private readonly IGeneratedTerrainLevelQuery _generatedTerrainLevelQuery;
        private readonly TileRegistrySO _tileRegistry;
        private readonly Dictionary<string, float> _tileSurfaceOffsetYById = new();
        private readonly float _buildingSurfaceOffsetY;
        private readonly float _previewSurfaceOffsetY;

        [Inject]
        public ConstructionTerrainAlignmentService(
            IGridService gridService,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] IGeneratedTerrainLevelQuery generatedTerrainLevelQuery = null,
            [InjectOptional] IConstructionVisualSettingsProvider visualSettingsProvider = null,
            [InjectOptional] TileRegistrySO tileRegistry = null)
        {
            _gridService = gridService;
            _gridProjection = gridProjection;
            _generatedTerrainLevelQuery = generatedTerrainLevelQuery;
            _tileRegistry = tileRegistry;
            _buildingSurfaceOffsetY = visualSettingsProvider?.BuildingSurfaceOffsetY ?? BuildingSurfaceOffsetY;
            _previewSurfaceOffsetY = visualSettingsProvider?.PreviewSurfaceOffsetY ?? PreviewSurfaceOffsetY;
        }

        public Vector3 ResolveWorldPosition(Vector2Int tile, float layerOffset)
        {
            if (_gridProjection == null)
                return new Vector3(tile.x, tile.y, layerOffset);

            if (GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection)
                && TryGetGeneratedTerrainSurfaceY(tile, out float surfaceY))
            {
                Vector3 position = _gridProjection.GridToWorld(tile, 0f, 0f);
                position.y = surfaceY + layerOffset;
                return position;
            }

            float elevation = _generatedTerrainLevelQuery != null && _generatedTerrainLevelQuery.TryGetTerrainLevel(tile, out int level)
                ? level
                : 0f;
            return _gridProjection.GridToWorld(tile, elevation, layerOffset);
        }

        public void AlignInstanceToTerrainSurface(GameObject instance, Vector2Int tile, bool isPreviewVisual)
        {
            if (!GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection) || instance == null)
                return;

            float surfaceOffset = isPreviewVisual ? _previewSurfaceOffsetY : _buildingSurfaceOffsetY;
            GridSurfacePlacementUtility.AlignBottomToSurface(instance, ResolveTerrainSurfaceY(tile) + surfaceOffset);
        }

        private float ResolveTerrainSurfaceY(Vector2Int tile)
        {
            if (!GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection))
                return ResolveWorldPosition(tile, 0f).y;

            float baseY = TryGetGeneratedTerrainSurfaceY(tile, out float surfaceY)
                ? surfaceY
                : ResolveProjectedTerrainBaseY(tile);

            if (_gridService.TryGetTileData(tile, out string tileId) && TryResolveTileSurfaceOffsetY(tileId, out float offsetY))
                return baseY + offsetY;

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

        private bool TryResolveTileSurfaceOffsetY(string tileId, out float offsetY)
        {
            offsetY = 0f;
            if (string.IsNullOrWhiteSpace(tileId) || _tileRegistry?.Definitions == null)
                return false;

            if (_tileSurfaceOffsetYById.TryGetValue(tileId, out offsetY))
                return true;

            for (int i = 0; i < _tileRegistry.Definitions.Length; i++)
            {
                var definition = _tileRegistry.Definitions[i];
                var surfacePrefab = definition?.SurfaceReferencePrefab;
                if (definition == null || definition.Id != tileId || surfacePrefab == null)
                    continue;

                if (!GridSurfacePlacementUtility.TryResolveTopOffsetY(surfacePrefab, out offsetY))
                    offsetY = 0f;

                _tileSurfaceOffsetYById[tileId] = offsetY;
                return true;
            }

            return false;
        }
    }
}
