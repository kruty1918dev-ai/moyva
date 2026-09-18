using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using Kruty1918.Moyva.Units.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Units.Runtime
{
    internal sealed class UnitWorldPositionResolver : IUnitWorldPositionResolver
    {
        private readonly IGridProjection _gridProjection;
        private readonly IGridService _gridService;
        private readonly IGeneratedTerrainLevelQuery _terrainLevelQuery;
        private readonly TileRegistrySO _tileRegistry;
        private readonly ITileSettingsService _tileSettings;
        private readonly Dictionary<string, float> _tileSurfaceOffsetYById = new();

        public UnitWorldPositionResolver(
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] IGridService gridService = null,
            [InjectOptional] IGeneratedTerrainLevelQuery terrainLevelQuery = null,
            [InjectOptional] TileRegistrySO tileRegistry = null,
            [InjectOptional] ITileSettingsService tileSettings = null)
        {
            _gridProjection = gridProjection;
            _gridService = gridService;
            _terrainLevelQuery = terrainLevelQuery;
            _tileRegistry = tileRegistry;
            _tileSettings = tileSettings;
        }

        public Vector3 ResolveWorldPosition(
            Vector2Int gridPosition,
            float surfacePivotOffsetY = 0.05f)
        {
            if (_gridProjection == null)
                return new Vector3(gridPosition.x, gridPosition.y, 0f);

            float elevation = _terrainLevelQuery != null
                              && _terrainLevelQuery.TryGetTerrainLevel(gridPosition, out int level)
                ? level
                : 0f;
            Vector3 basePosition = _gridProjection.GridToWorld(gridPosition, elevation, 0.05f);
            if (!GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection))
                return basePosition;

            basePosition.y = ResolveTerrainSurfaceY(gridPosition, elevation)
                             + Mathf.Max(0f, surfacePivotOffsetY);
            return basePosition;
        }

        public float ResolveSurfacePivotOffsetY(
            GameObject unitObject,
            Vector2Int gridPosition,
            float fallbackOffsetY = 0.05f)
        {
            if (!GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection)
                || unitObject == null)
            {
                return Mathf.Max(0f, fallbackOffsetY);
            }

            if (!TryGetTerrainSurfaceY(gridPosition, out float surfaceY))
                return Mathf.Max(0f, fallbackOffsetY);

            return Mathf.Max(
                GridSurfacePlacementUtility.DefaultSurfaceClearance,
                unitObject.transform.position.y - surfaceY);
        }

        public void AlignBottomToSurface(
            GameObject unitObject,
            Vector2Int gridPosition,
            float clearance = GridSurfacePlacementUtility.DefaultSurfaceClearance)
        {
            if (!GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection)
                || unitObject == null)
            {
                return;
            }

            GridSurfacePlacementUtility.AlignBottomToSurface(
                unitObject,
                ResolveTerrainSurfaceY(gridPosition),
                clearance);
        }

        public bool TryGetTerrainSurfaceY(Vector2Int gridPosition, out float surfaceY)
        {
            surfaceY = 0f;
            if (_gridProjection == null)
                return false;

            float elevation = _terrainLevelQuery != null
                              && _terrainLevelQuery.TryGetTerrainLevel(gridPosition, out int level)
                ? level
                : 0f;

            surfaceY = ResolveTerrainSurfaceY(gridPosition, elevation);
            return true;
        }

        private float ResolveTerrainSurfaceY(Vector2Int gridPosition, float elevation = 0f)
        {
            if (_gridProjection == null)
                return 0f;

            if (_terrainLevelQuery != null
                && _terrainLevelQuery.TryGetTerrainSurfaceY(gridPosition, out float generatedSurfaceY)
                && IsFinite(generatedSurfaceY))
            {
                return generatedSurfaceY;
            }

            float baseY = _gridProjection.GridToWorld(gridPosition, elevation, 0f).y;
            if (_gridService != null
                && _gridService.TryGetTileData(gridPosition, out string tileId)
                && TryResolveTileSurfaceOffsetY(tileId, out float offsetY))
            {
                return baseY + offsetY;
            }

            return baseY;
        }

        private bool TryResolveTileSurfaceOffsetY(string tileId, out float offsetY)
        {
            offsetY = 0f;
            if (string.IsNullOrWhiteSpace(tileId))
                return false;

            if (_tileSurfaceOffsetYById.TryGetValue(tileId, out offsetY))
                return true;

            if (_tileSettings != null)
            {
                offsetY = _tileSettings.GetSurfaceOffset(tileId);
                if (Mathf.Abs(offsetY) > 0.0001f)
                {
                    _tileSurfaceOffsetYById[tileId] = offsetY;
                    return true;
                }
            }

            if (_tileRegistry?.Definitions == null)
                return false;

            for (int index = 0; index < _tileRegistry.Definitions.Length; index++)
            {
                TileTypeDefinition definition = _tileRegistry.Definitions[index];
                GameObject surfacePrefab = definition?.SurfaceReferencePrefab;
                if (definition == null
                    || definition.Id != tileId
                    || surfacePrefab == null)
                {
                    continue;
                }

                if (!GridSurfacePlacementUtility.TryResolveTopOffsetY(surfacePrefab, out offsetY))
                    offsetY = 0f;

                _tileSurfaceOffsetYById[tileId] = offsetY;
                return true;
            }

            return false;
        }

        private static bool IsFinite(float value)
            => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
