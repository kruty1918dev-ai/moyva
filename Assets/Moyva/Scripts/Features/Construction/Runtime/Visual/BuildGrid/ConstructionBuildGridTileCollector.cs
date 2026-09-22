using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionBuildGridTileCollector {
        private const float MinSurfaceOffsetY = 0.001f;
        private const float MaxSurfaceOffsetY = 0.5f;

        private readonly IGridService _gridService;
        private readonly ConstructionTerrainAlignmentService _terrainAlignment;
        private readonly IConstructionGridGeometryService _gridGeometry;
        private readonly IConstructionVisualSettingsProvider _settingsProvider;
        private readonly Mesh _quadMesh = ConstructionQuadMeshFactory.Create("ConstructionBuildGridQuad");

        [Inject]
        public ConstructionBuildGridTileCollector(
            IGridService gridService,
            ConstructionTerrainAlignmentService terrainAlignment,
            [InjectOptional] IConstructionGridGeometryService gridGeometry = null,
            [InjectOptional] IConstructionVisualSettingsProvider settingsProvider = null)
        {
            _gridService = gridService;
            _terrainAlignment = terrainAlignment;
            _gridGeometry = gridGeometry;
            _settingsProvider = settingsProvider;
        }

        public void Collect(
            List<ConstructionBuildGridOverlayEntry> results,
            System.Func<Vector2Int, ConstructionBuildGridTileVisualState> resolveVisualState)
        {
            results.Clear();

            if (_gridService == null)
                return;

            Vector3 cellScale = ResolveCellScale();
            for (int x = 0; x < _gridService.GridWidth; x++)
            {
                for (int y = 0; y < _gridService.GridHeight; y++)
                {
                    Vector2Int position = new(x, y);
                    if (!_gridService.TryGetTileData(position, out _))
                        continue;

                    ConstructionBuildGridTileVisualState visualState = resolveVisualState != null
                        ? resolveVisualState(position)
                        : ConstructionBuildGridTileVisualState.General;
                    if (visualState == ConstructionBuildGridTileVisualState.Missing)
                        continue;

                    if (!TryCreateEntry(position, visualState, cellScale, out ConstructionBuildGridOverlayEntry entry))
                        continue;

                    results.Add(entry);
                }
            }
        }

        private bool TryCreateEntry(
            Vector2Int position,
            ConstructionBuildGridTileVisualState visualState,
            Vector3 cellScale,
            out ConstructionBuildGridOverlayEntry entry)
        {
            entry = default;
            if (_terrainAlignment == null)
                return false;

            float offsetY = ResolveSurfaceOffsetY();
            Vector3 center = _terrainAlignment.ResolveWorldPosition(position, offsetY);
            Matrix4x4 matrix;
            if (_terrainAlignment.TryResolveTileSlope(
                    position,
                    out Vector2Int climb,
                    out float lowEdgeY,
                    out float highEdgeY)
                && highEdgeY > lowEdgeY)
            {
                matrix = CreateSlopedMatrix(position, climb, lowEdgeY, highEdgeY, offsetY);
            }
            else
            {
                matrix = Matrix4x4.TRS(center, Quaternion.Euler(90f, 0f, 0f), cellScale);
            }
            entry = new ConstructionBuildGridOverlayEntry(
                position,
                visualState,
                _quadMesh,
                matrix,
                0,
                ResolveEdgeMask(position),
                null);
            return true;
        }

        /// <summary>
        /// Tilted quad matrix for a stair cell: the quad's local +Y axis (which
        /// the flat path maps to world +Z) is aligned with the slope so the
        /// quad rises from the low edge to the module top while keeping the
        /// same horizontal footprint as a flat tile.
        /// </summary>
        private Matrix4x4 CreateSlopedMatrix(
            Vector2Int position,
            Vector2Int climb,
            float lowEdgeY,
            float highEdgeY,
            float offsetY)
        {
            Vector2 cellSize =
                _gridGeometry != null && _gridGeometry.TryGetCellSize(out Vector2 size)
                    ? size
                    : Vector2.one;
            float tileScale = 1f - ResolveTileInsetNormalized() * 2f;
            Vector3 climb3 = new(climb.x, 0f, climb.y);
            bool climbsAlongZ = climb.y != 0;
            float run = Mathf.Max(0.01f, (climbsAlongZ ? cellSize.y : cellSize.x) * tileScale);
            float perpExtent = Mathf.Max(0.01f, (climbsAlongZ ? cellSize.x : cellSize.y) * tileScale);
            float rise = highEdgeY - lowEdgeY;

            Vector3 col0 = Vector3.Cross(Vector3.up, climb3) * perpExtent;
            Vector3 col1 = climb3 * run + Vector3.up * rise;
            Vector3 col2 = Vector3.Cross(col0.normalized, col1.normalized);

            Vector3 center = _terrainAlignment.ResolveWorldPosition(position, offsetY);
            center.y = (lowEdgeY + highEdgeY) * 0.5f + offsetY;

            var matrix = new Matrix4x4();
            matrix.SetColumn(0, new Vector4(col0.x, col0.y, col0.z, 0f));
            matrix.SetColumn(1, new Vector4(col1.x, col1.y, col1.z, 0f));
            matrix.SetColumn(2, new Vector4(col2.x, col2.y, col2.z, 0f));
            matrix.SetColumn(3, new Vector4(center.x, center.y, center.z, 1f));
            return matrix;
        }

        private Vector3 ResolveCellScale()
        {
            if (_gridGeometry == null || !_gridGeometry.TryGetCellSize(out Vector2 cellSize))
                return Vector3.one;

            float tileScale = 1f - ResolveTileInsetNormalized() * 2f;
            return new Vector3(
                Mathf.Max(0.01f, cellSize.x * tileScale),
                Mathf.Max(0.01f, cellSize.y * tileScale),
                1f);
        }

        private Vector4 ResolveEdgeMask(Vector2Int position)
        {
            if (ResolveTileInsetNormalized() > 0.0001f)
                return Vector4.one;

            return new Vector4(
                1f,
                1f,
                ShouldDrawBoundary(position + Vector2Int.right) ? 1f : 0f,
                ShouldDrawBoundary(position + Vector2Int.up) ? 1f : 0f);
        }

        private bool ShouldDrawBoundary(Vector2Int position)
        {
            return _gridService == null || !_gridService.TryGetTileData(position, out _);
        }

        private float ResolveSurfaceOffsetY()
            => Mathf.Clamp(_settingsProvider?.BuildGridSurfaceOffsetY ?? 0.06f, MinSurfaceOffsetY, MaxSurfaceOffsetY);

        private float ResolveTileInsetNormalized()
            => Mathf.Clamp(_settingsProvider?.BuildGridTileInsetNormalized ?? 0.08f, 0f, 0.45f);
    }
}
