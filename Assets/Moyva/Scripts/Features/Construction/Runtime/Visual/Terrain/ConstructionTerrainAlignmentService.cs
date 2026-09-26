using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.Grid.Runtime;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionTerrainAlignmentService {
        private const string FoundationSkirtName = "FoundationSkirt";
        private const float FoundationOverlapY = 0.04f;
        private const float FoundationDepthMarginY = 0.15f;
        private const float BuildingSurfaceOffsetY = 0.5f;
        private const float PreviewSurfaceOffsetY = 0.7f;
        /// <summary>
        /// Share of the cell footprint a building visual may occupy. Oversized
        /// prefabs are shrunk uniformly so every building stays inside its one
        /// logical tile; smaller prefabs keep their authored size.
        /// </summary>
        private const float BuildingFootprintCellFraction = 0.92f;

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
        private Material _foundationMaterial;

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

        public Vector3 ResolveAlignedInstancePosition(GameObject instance, Vector2Int tile, bool isPreviewVisual, float visualOffsetY = 0f, IReadOnlyList<Vector2Int> footprintCells = null)
        {
            Vector3 fallback = ResolveWorldPosition(tile, 0.1f);
            if (instance == null || !GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection))
                return fallback;

            return ResolveCachedAlignedPosition(instance, tile, isPreviewVisual, visualOffsetY, footprintCells);
        }

        /// <summary>
        /// Shrinks an instance whose renderer bounds exceed one tile's
        /// footprint so placed and preview visuals never spill onto
        /// neighbouring cells. Must run before the first
        /// <see cref="AlignInstanceToTerrainSurface"/> call: the metrics cache
        /// captures the final scale.
        /// </summary>
        public void NormalizeInstanceFootprintToTile(GameObject instance)
        {
            if (instance == null
                || _gridGeometry == null
                || !_gridGeometry.TryGetCellSize(out Vector2 cellSize))
            {
                return;
            }
            if (!TryResolveBuildingBounds(instance, out Bounds bounds))
                return;

            float footprint = Mathf.Max(bounds.size.x, bounds.size.z);
            float limit = Mathf.Min(cellSize.x, cellSize.y) * BuildingFootprintCellFraction;
            if (footprint <= limit || footprint <= 0.0001f)
                return;

            instance.transform.localScale *= limit / footprint;
        }

        public void AlignInstanceToTerrainSurface(GameObject instance, Vector2Int tile, bool isPreviewVisual, float visualOffsetY = 0f, IReadOnlyList<Vector2Int> footprintCells = null)
        {
            if (!GridSurfacePlacementUtility.Uses3DWorldPlane(_gridProjection) || instance == null)
                return;

            instance.transform.position = ResolveCachedAlignedPosition(instance, tile, isPreviewVisual, visualOffsetY, footprintCells);
        }

        private Vector3 ResolveCachedAlignedPosition(
            GameObject instance,
            Vector2Int tile,
            bool isPreviewVisual,
            float visualOffsetY,
            IReadOnlyList<Vector2Int> footprintCells)
        {
            CachedVisualMetrics metrics = _visualMetrics.GetValue(instance, CreateVisualMetrics);
            Vector3 gridCenter = ResolveGridCenter(tile);
            float targetSurfaceY = ResolveFootprintSurfaceY(tile, footprintCells)
                + ResolveVisualSurfaceOffsetY(isPreviewVisual)
                + visualOffsetY;

            UpdateFootprintFoundation(instance, footprintCells, targetSurfaceY);

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

        /// <summary>Renderer bounds of the building alone — the foundation
        /// skirt is excluded so it never feeds footprint metrics.</summary>
        private static bool TryResolveBuildingBounds(
            GameObject instance,
            out Bounds bounds)
        {
            bounds = default;
            if (instance == null)
                return false;

            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            bool hasBounds = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null
                    || !renderer.enabled
                    || renderer.name == FoundationSkirtName)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                    continue;
                }

                bounds.Encapsulate(renderer.bounds);
            }

            return hasBounds;
        }

        private static CachedVisualMetrics CreateVisualMetrics(GameObject instance)
        {
            // The foundation skirt hangs below the building; it must not feed
            // the bottom-offset, or reused previews would float by the skirt
            // depth. Compute bounds excluding skirt renderers.
            if (!TryResolveBuildingBounds(instance, out Bounds bounds))
                return CachedVisualMetrics.Empty;

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

        /// <summary>
        /// Building base level: the highest terrain surface under the
        /// footprint, so multi-cell buildings on sloped dry ground never
        /// sink into an uphill cell. Uneven drops are covered by the
        /// foundation skirt instead of rejecting placement.
        /// </summary>
        private float ResolveFootprintSurfaceY(
            Vector2Int tile,
            IReadOnlyList<Vector2Int> footprintCells)
        {
            float surfaceY = ResolveTerrainSurfaceY(tile);
            if (footprintCells == null)
                return surfaceY;

            for (int index = 0; index < footprintCells.Count; index++)
            {
                float cellY = ResolveTerrainSurfaceY(footprintCells[index]);
                if (cellY > surfaceY)
                    surfaceY = cellY;
            }

            return surfaceY;
        }

        /// <summary>
        /// Keeps a solid plinth under the building base when the footprint
        /// spans a terrain drop: the base sits on the highest cell and the
        /// skirt reaches down into the lowest one, hiding the gap. Flat
        /// footprints remove any stale skirt.
        /// </summary>
        private void UpdateFootprintFoundation(
            GameObject instance,
            IReadOnlyList<Vector2Int> footprintCells,
            float targetSurfaceY)
        {
            if (instance == null || !instance)
                return;

            Transform existing = instance.transform.Find(FoundationSkirtName);
            if (footprintCells == null || footprintCells.Count < 2)
            {
                if (existing != null)
                    DestroySkirt(existing);
                return;
            }

            float minSurfaceY = targetSurfaceY;
            bool hasSurface = false;
            Vector3 minCorner = new Vector3(float.MaxValue, 0f, float.MaxValue);
            Vector3 maxCorner = new Vector3(float.MinValue, 0f, float.MinValue);
            float halfCellX = 0.5f;
            float halfCellZ = 0.5f;
            if (_gridGeometry != null
                && _gridGeometry.TryGetCellSize(out Vector2 cellSize))
            {
                halfCellX = cellSize.x * 0.5f;
                halfCellZ = cellSize.y * 0.5f;
            }

            for (int index = 0; index < footprintCells.Count; index++)
            {
                Vector2Int cell = footprintCells[index];
                float surfaceY = ResolveTerrainSurfaceY(cell);
                if (!hasSurface || surfaceY < minSurfaceY)
                {
                    minSurfaceY = surfaceY;
                    hasSurface = true;
                }

                Vector3 center = ResolveGridCenter(cell);
                minCorner.x = Mathf.Min(minCorner.x, center.x - halfCellX);
                minCorner.z = Mathf.Min(minCorner.z, center.z - halfCellZ);
                maxCorner.x = Mathf.Max(maxCorner.x, center.x + halfCellX);
                maxCorner.z = Mathf.Max(maxCorner.z, center.z + halfCellZ);
            }

            float drop = targetSurfaceY - minSurfaceY;
            if (!hasSurface || drop <= 0.01f)
            {
                if (existing != null)
                    DestroySkirt(existing);
                return;
            }

            Transform skirt = existing;
            if (skirt == null)
            {
                var skirtObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                skirtObject.name = FoundationSkirtName;
                var collider = skirtObject.GetComponent<Collider>();
                if (collider != null)
                {
                    if (Application.isPlaying)
                        Object.Destroy(collider);
                    else
                        Object.DestroyImmediate(collider);
                }

                var renderer = skirtObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = ResolveFoundationMaterial();
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    renderer.receiveShadows = true;
                }

                skirt = skirtObject.transform;
                skirt.SetParent(instance.transform, worldPositionStays: false);
            }

            float topY = targetSurfaceY + FoundationOverlapY;
            float bottomY = minSurfaceY - FoundationDepthMarginY;
            float height = Mathf.Max(0.01f, topY - bottomY);
            Vector3 lossyScale = instance.transform.lossyScale;
            float parentX = Mathf.Max(0.0001f, Mathf.Abs(lossyScale.x));
            float parentY = Mathf.Max(0.0001f, Mathf.Abs(lossyScale.y));
            float parentZ = Mathf.Max(0.0001f, Mathf.Abs(lossyScale.z));

            // The footprint cells are already the rotated set, so the skirt
            // is axis-aligned in world space; the parent-frame size and the
            // world-space position convert through the instance transform.
            skirt.localRotation = Quaternion.identity;
            skirt.position = new Vector3(
                (minCorner.x + maxCorner.x) * 0.5f,
                (topY + bottomY) * 0.5f,
                (minCorner.z + maxCorner.z) * 0.5f);
            Vector3 rotatedSize = new Vector3(
                maxCorner.x - minCorner.x,
                height,
                maxCorner.z - minCorner.z);
            Vector3 inv = Quaternion.Inverse(instance.transform.rotation)
                * new Vector3(rotatedSize.x, 0f, rotatedSize.z);
            skirt.localScale = new Vector3(
                Mathf.Abs(inv.x) / parentX,
                rotatedSize.y / parentY,
                Mathf.Abs(inv.z) / parentZ);
        }

        private static void DestroySkirt(Transform skirt)
        {
            if (skirt == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(skirt.gameObject);
            else
                Object.DestroyImmediate(skirt.gameObject);
        }

        private Material ResolveFoundationMaterial()
        {
            if (_foundationMaterial != null)
                return _foundationMaterial;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            _foundationMaterial = new Material(shader != null ? shader : Shader.Find("Diffuse"))
            {
                name = "Moyva_FoundationSkirt",
            };
            if (_foundationMaterial.HasProperty("_BaseColor"))
                _foundationMaterial.SetColor("_BaseColor", new Color(0.36f, 0.27f, 0.18f, 1f));
            else if (_foundationMaterial.HasProperty("_Color"))
                _foundationMaterial.SetColor("_Color", new Color(0.36f, 0.27f, 0.18f, 1f));
            return _foundationMaterial;
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
