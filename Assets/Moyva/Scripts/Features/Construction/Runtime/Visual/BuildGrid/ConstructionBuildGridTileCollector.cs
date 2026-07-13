using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using UnityEngine;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionBuildGridTileCollector : IConstructionBuildGridTileCollector
    {
        private const float MinSurfaceOffsetY = 0.001f;
        private const float MaxSurfaceOffsetY = 0.5f;

        private readonly IGridService _gridService;
        private readonly IConstructionTerrainAlignmentService _terrainAlignment;
        private readonly IConstructionGridGeometryService _gridGeometry;
        private readonly IConstructionVisualSettingsProvider _settingsProvider;
        private readonly Mesh _quadMesh = ConstructionQuadMeshFactory.Create("ConstructionBuildGridQuad");

        [Inject]
        public ConstructionBuildGridTileCollector(
            IGridService gridService,
            IConstructionTerrainAlignmentService terrainAlignment,
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
            System.Func<Vector2Int, ConstructionBuildGridTileVisualState> resolveVisualState,
            out ConstructionBuildGridCollectionStats stats)
        {
            results.Clear();
            int positionsScanned = 0;
            int positionsWithTileData = 0;
            int missingSurfaceData = 0;
            int filteredOut = 0;
            int skippedEntries = 0;

            if (_gridService == null)
            {
                stats = new ConstructionBuildGridCollectionStats(0, 0, 0, 0, 0, 0);
                return;
            }

            Vector3 cellScale = ResolveCellScale();
            for (int x = 0; x < _gridService.GridWidth; x++)
            {
                for (int y = 0; y < _gridService.GridHeight; y++)
                {
                    positionsScanned++;
                    Vector2Int position = new(x, y);
                    if (!_gridService.TryGetTileData(position, out _))
                        continue;

                    positionsWithTileData++;
                    ConstructionBuildGridTileVisualState visualState = resolveVisualState != null
                        ? resolveVisualState(position)
                        : ConstructionBuildGridTileVisualState.General;
                    if (visualState == ConstructionBuildGridTileVisualState.Missing)
                    {
                        filteredOut++;
                        continue;
                    }

                    if (!TryCreateEntry(position, visualState, cellScale, out ConstructionBuildGridOverlayEntry entry))
                    {
                        skippedEntries++;
                        continue;
                    }

                    results.Add(entry);
                }
            }

            stats = new ConstructionBuildGridCollectionStats(
                positionsScanned,
                positionsWithTileData,
                missingSurfaceData,
                filteredOut,
                skippedEntries,
                results.Count);
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

            Vector3 center = _terrainAlignment.ResolveWorldPosition(position, ResolveSurfaceOffsetY());
            Matrix4x4 matrix = Matrix4x4.TRS(center, Quaternion.Euler(90f, 0f, 0f), cellScale);
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
