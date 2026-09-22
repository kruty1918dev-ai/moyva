using System.Runtime.CompilerServices;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionTerrainAlignmentService {
        private const float BuildingSurfaceOffsetY = 0.5f;
        private const float PreviewSurfaceOffsetY = 0.7f;

        private readonly IGridService _gridService;
        private readonly IGridProjection _gridProjection;
        private readonly IConstructionGridGeometryService _gridGeometry;
        private readonly ConstructionTileSurfaceOffsetService _tileSurfaceOffsets;
        private readonly ConstructionVisualBoundsAlignmentService _boundsAlignment;
        private readonly IGeneratedTerrainLevelQuery _generatedTerrainLevelQuery;
        private readonly ITerrainPassageMap _terrainPassages;
        private readonly ConditionalWeakTable<GameObject, CachedVisualMetrics> _visualMetrics = new();
        private readonly float _buildingSurfaceOffsetY;
        private readonly float _previewSurfaceOffsetY;

        [Inject]
        public ConstructionTerrainAlignmentService(
            IGridService gridService,
            [InjectOptional] IGridProjection gridProjection = null,
            [InjectOptional] IConstructionGridGeometryService gridGeometry = null,
            [InjectOptional] ConstructionTileSurfaceOffsetService tileSurfaceOffsets = null,
            [InjectOptional] ConstructionVisualBoundsAlignmentService boundsAlignment = null,
            [InjectOptional] IGeneratedTerrainLevelQuery generatedTerrainLevelQuery = null,
            [InjectOptional] IConstructionVisualSettingsProvider visualSettingsProvider = null,
            [InjectOptional] ITerrainPassageMap terrainPassages = null)
        {
            _gridService = gridService;
            _gridProjection = gridProjection;
            _gridGeometry = gridGeometry;
            _tileSurfaceOffsets = tileSurfaceOffsets;
            _boundsAlignment = boundsAlignment;
            _ = _boundsAlignment; // Constructor compatibility; hot-path alignment now uses cached metrics.
            _generatedTerrainLevelQuery = generatedTerrainLevelQuery;
            _terrainPassages = terrainPassages;
            _buildingSurfaceOffsetY = visualSettingsProvider?.BuildingSurfaceOffsetY ?? BuildingSurfaceOffsetY;
            _previewSurfaceOffsetY = visualSettingsProvider?.PreviewSurfaceOffsetY ?? PreviewSurfaceOffsetY;
        }

        /// <summary>
        /// Sloped surface info for a generated stair module occupying the cell.
        /// Returns false for flat tiles. <paramref name="climbDirection"/> is the
        /// grid-space direction the surface rises toward (one of ±X/±Y, mapping
        /// to world ±X/±Z). Edge heights are raw surface Y (without layer
        /// offset): <paramref name="lowEdgeSurfaceY"/> at the edge opposite the
        /// climb, <paramref name="highEdgeSurfaceY"/> at the climb-side edge.
        /// </summary>
        public bool TryResolveTileSlope(
            Vector2Int tile,
            out Vector2Int climbDirection,
            out float lowEdgeSurfaceY,
            out float highEdgeSurfaceY)
        {
            climbDirection = default;
            lowEdgeSurfaceY = 0f;
            highEdgeSurfaceY = 0f;

            if (!GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection)
                || _terrainPassages == null
                || !_terrainPassages.TryGetModule(tile, out TerrainPassageModule module))
            {
                return false;
            }

            climbDirection = StairDirectionOffset(module.DirectionIndex);
            if (climbDirection == Vector2Int.zero)
                return false;

            highEdgeSurfaceY = module.TopY;
            // The module's bottom edge meets the previous module's top, or the
            // low plateau surface for the first module of a flight.
            Vector2Int prevCell = tile - climbDirection;
            lowEdgeSurfaceY =
                _terrainPassages.TryGetModule(prevCell, out TerrainPassageModule prev)
                    ? prev.TopY
                    : module.LowSurfaceY;
            return true;
        }

        /// <summary>
        /// Per-corner surface heights of one tile in world Y, corners ordered
        /// (-x,-z), (+x,-z), (+x,+z), (-x,+z) around the tile center. Flat tiles
        /// return four equal heights; stair cells slope toward the climb edge.
        /// </summary>
        public TileSurfaceQuad ResolveTileSurfaceQuad(Vector2Int tile, float layerOffset)
        {
            if (TryResolveTileSlope(tile, out Vector2Int climb, out float lowY, out float highY))
            {
                float lo = lowY + layerOffset;
                float hi = highY + layerOffset;
                float y00 = lo, y10 = lo, y11 = lo, y01 = lo;
                if (climb.y > 0) { y11 = hi; y01 = hi; }
                else if (climb.y < 0) { y00 = hi; y10 = hi; }
                else if (climb.x > 0) { y10 = hi; y11 = hi; }
                else { y00 = hi; y01 = hi; }
                return new TileSurfaceQuad(y00, y10, y11, y01);
            }

            float flatY = ResolveWorldPosition(tile, layerOffset).y;
            return new TileSurfaceQuad(flatY, flatY, flatY, flatY);
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

            return ResolveCachedAlignedPosition(instance, tile, isPreviewVisual, visualOffsetY);
        }

        public void AlignInstanceToTerrainSurface(GameObject instance, Vector2Int tile, bool isPreviewVisual, float visualOffsetY = 0f)
        {
            if (!GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection) || instance == null)
                return;

            instance.transform.position = ResolveCachedAlignedPosition(instance, tile, isPreviewVisual, visualOffsetY);
        }

        private Vector3 ResolveCachedAlignedPosition(
            GameObject instance,
            Vector2Int tile,
            bool isPreviewVisual,
            float visualOffsetY)
        {
            CachedVisualMetrics metrics = _visualMetrics.GetValue(instance, CreateVisualMetrics);
            Vector3 gridCenter = ResolveGridCenter(tile);
            float targetSurfaceY = ResolveTerrainSurfaceY(tile)
                + ResolveVisualSurfaceOffsetY(isPreviewVisual)
                + visualOffsetY;

            if (!metrics.HasRendererBounds)
            {
                return new Vector3(
                    gridCenter.x,
                    targetSurfaceY,
                    gridCenter.z);
            }

            return new Vector3(
                gridCenter.x - metrics.CenterOffsetX,
                targetSurfaceY - metrics.BottomOffsetY,
                gridCenter.z - metrics.CenterOffsetZ);
        }

        private static CachedVisualMetrics CreateVisualMetrics(GameObject instance)
        {
            if (instance == null
                || !GridSurfacePlacementUtility.TryResolveRendererBounds(instance, out Bounds bounds))
            {
                return CachedVisualMetrics.Empty;
            }

            Vector3 rootPosition = instance.transform.position;
            return new CachedVisualMetrics(
                hasRendererBounds: true,
                centerOffsetX: bounds.center.x - rootPosition.x,
                centerOffsetZ: bounds.center.z - rootPosition.z,
                bottomOffsetY: bounds.min.y - rootPosition.y);
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

        // Mirrors TerrainPassagePlan.DirectionIndex: 0=+Y(grid/+Z world),
        // 1=+X, 2=-Y(-Z), 3=-X. Kept local — Generator.Runtime is not a
        // dependency of this assembly.
        private static Vector2Int StairDirectionOffset(int directionIndex)
        {
            return directionIndex switch
            {
                0 => new Vector2Int(0, 1),
                1 => new Vector2Int(1, 0),
                2 => new Vector2Int(0, -1),
                3 => new Vector2Int(-1, 0),
                _ => Vector2Int.zero,
            };
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

        private sealed class CachedVisualMetrics
        {
            public static readonly CachedVisualMetrics Empty = new(
                hasRendererBounds: false,
                centerOffsetX: 0f,
                centerOffsetZ: 0f,
                bottomOffsetY: 0f);

            public CachedVisualMetrics(
                bool hasRendererBounds,
                float centerOffsetX,
                float centerOffsetZ,
                float bottomOffsetY)
            {
                HasRendererBounds = hasRendererBounds;
                CenterOffsetX = centerOffsetX;
                CenterOffsetZ = centerOffsetZ;
                BottomOffsetY = bottomOffsetY;
            }

            public bool HasRendererBounds { get; }
            public float CenterOffsetX { get; }
            public float CenterOffsetZ { get; }
            public float BottomOffsetY { get; }
        }
    }

    /// <summary>
    /// World-space surface heights at the four corners of one tile, ordered
    /// (-x,-z), (+x,-z), (+x,+z), (-x,+z) relative to the tile centre. Flat
    /// tiles carry four equal heights; generated stair cells slope toward the
    /// climb edge so overlay geometry hugs the ramp instead of hovering.
    /// </summary>
    internal readonly struct TileSurfaceQuad
    {
        public TileSurfaceQuad(float y00, float y10, float y11, float y01)
        {
            Y00 = y00;
            Y10 = y10;
            Y11 = y11;
            Y01 = y01;
        }

        public float Y00 { get; }
        public float Y10 { get; }
        public float Y11 { get; }
        public float Y01 { get; }

        public float MinY => Mathf.Min(Mathf.Min(Y00, Y10), Mathf.Min(Y11, Y01));
        public float MaxY => Mathf.Max(Mathf.Max(Y00, Y10), Mathf.Max(Y11, Y01));
        public float CenterY => (Y00 + Y10 + Y11 + Y01) * 0.25f;
        public bool IsSloped => MaxY - MinY > 0.0001f;

        /// <summary>Geometric normal of the quad (world space, up-facing).</summary>
        public Vector3 ComputeNormal(Vector3 center, float halfX, float halfZ)
        {
            Vector3 v0 = new(center.x - halfX, Y00, center.z - halfZ);
            Vector3 v1 = new(center.x + halfX, Y10, center.z - halfZ);
            Vector3 v3 = new(center.x - halfX, Y01, center.z + halfZ);
            Vector3 normal = Vector3.Cross(v3 - v0, v1 - v0);
            return normal.sqrMagnitude > 1e-10f ? normal.normalized : Vector3.up;
        }
    }
}
