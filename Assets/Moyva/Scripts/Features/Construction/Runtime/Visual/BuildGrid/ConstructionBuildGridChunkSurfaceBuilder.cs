
using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.MapChunks.API;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

namespace Kruty1918.Moyva.Construction.Runtime
{
    internal sealed class ConstructionBuildGridChunkSurfaceBuilder : IConstructionBuildGridChunkSurfaceBuilder
    {
        private const float MinSurfaceOffsetY = 0.001f;
        private const float MaxSurfaceOffsetY = 0.5f;

        private readonly IGridService _gridService;
        private readonly IConstructionTerrainAlignmentService _terrainAlignment;
        private readonly IConstructionGridGeometryService _gridGeometry;
        private readonly IConstructionBuildGridTileFilter _tileFilter;
        private readonly IConstructionVisualSettingsProvider _settingsProvider;

        [Inject]
        public ConstructionBuildGridChunkSurfaceBuilder(
            IGridService gridService,
            IConstructionTerrainAlignmentService terrainAlignment,
            [InjectOptional] IConstructionGridGeometryService gridGeometry = null,
            [InjectOptional] IConstructionBuildGridTileFilter tileFilter = null,
            [InjectOptional] IConstructionVisualSettingsProvider settingsProvider = null)
        {
            _gridService = gridService;
            _terrainAlignment = terrainAlignment;
            _gridGeometry = gridGeometry;
            _tileFilter = tileFilter;
            _settingsProvider = settingsProvider;
        }

        public bool TryBuild(MapChunkDescriptor descriptor, out Mesh mesh)
        {
            mesh = null;

            if (_gridService == null || _terrainAlignment == null)
                return false;

            RectInt rect = descriptor.TileRect;
            if (rect.width <= 0 || rect.height <= 0)
                return false;

            Vector2 cellSize = ResolveCellSize(descriptor);
            float inset = ResolveTileInsetNormalized();
            float tileScale = Mathf.Max(0.01f, 1f - inset * 2f);
            float halfX = Mathf.Max(0.01f, cellSize.x * tileScale * 0.5f);
            float halfZ = Mathf.Max(0.01f, cellSize.y * tileScale * 0.5f);

            var vertices = new List<Vector3>(rect.width * rect.height * 4);
            var uvs = new List<Vector2>(rect.width * rect.height * 4);
            var triangles = new List<int>(rect.width * rect.height * 6);

            for (int x = rect.xMin; x < rect.xMax; x++)
            {
                for (int y = rect.yMin; y < rect.yMax; y++)
                {
                    Vector2Int tile = new(x, y);
                    if (!_gridService.TryGetTileData(tile, out _))
                        continue;

                    if (UseBuildableFilter() && _tileFilter != null && !_tileFilter.ShouldRender(tile))
                        continue;

                    AddTileQuad(tile, halfX, halfZ, vertices, uvs, triangles);
                }
            }

            if (vertices.Count == 0)
                return false;

            mesh = new Mesh
            {
                name = $"ConstructionBuildGridChunkSurface_{descriptor.Coord.X}_{descriptor.Coord.Y}"
            };

            if (vertices.Count > 65535)
                mesh.indexFormat = IndexFormat.UInt32;

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return true;
        }

        private void AddTileQuad(
            Vector2Int tile,
            float halfX,
            float halfZ,
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles)
        {
            Vector3 center = _terrainAlignment.ResolveWorldPosition(tile, ResolveSurfaceOffsetY());
            int start = vertices.Count;

            vertices.Add(center + new Vector3(-halfX, 0f, -halfZ));
            vertices.Add(center + new Vector3( halfX, 0f, -halfZ));
            vertices.Add(center + new Vector3( halfX, 0f,  halfZ));
            vertices.Add(center + new Vector3(-halfX, 0f,  halfZ));

            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(1f, 0f));
            uvs.Add(new Vector2(1f, 1f));
            uvs.Add(new Vector2(0f, 1f));

            // Up-facing winding for XZ plane.
            triangles.Add(start + 0);
            triangles.Add(start + 2);
            triangles.Add(start + 1);
            triangles.Add(start + 0);
            triangles.Add(start + 3);
            triangles.Add(start + 2);
        }

        private Vector2 ResolveCellSize(MapChunkDescriptor descriptor)
        {
            if (_gridGeometry != null && _gridGeometry.TryGetCellSize(out Vector2 size))
                return size;

            RectInt rect = descriptor.TileRect;
            Bounds bounds = descriptor.WorldBounds;
            if (rect.width > 0 && rect.height > 0 && bounds.size.x > 0f && bounds.size.z > 0f)
                return new Vector2(bounds.size.x / rect.width, bounds.size.z / rect.height);

            return Vector2.one;
        }

        private bool UseBuildableFilter()
            => _settingsProvider?.BuildGridSurfacePlaneUseBuildableFilter ?? false;

        private float ResolveSurfaceOffsetY()
            => Mathf.Clamp(_settingsProvider?.BuildGridSurfaceOffsetY ?? 0.06f, MinSurfaceOffsetY, MaxSurfaceOffsetY);

        private float ResolveTileInsetNormalized()
            => Mathf.Clamp(_settingsProvider?.BuildGridTileInsetNormalized ?? 0.08f, 0f, 0.45f);
    }
}
