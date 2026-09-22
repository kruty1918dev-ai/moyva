using System.Collections.Generic;
using Kruty1918.Moyva.Construction.API;
using Kruty1918.Moyva.Construction.Runtime;
using Kruty1918.Moyva.Grid.API;
using Kruty1918.Moyva.MapChunks.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Tests.Construction
{
    /// <summary>
    /// Geometry contract of the build-grid chunk surface: quad size comes from
    /// the grid geometry service (or chunk bounds fallback), tile centers come
    /// from the projection/geometry, and elevated tiles sit on the generated
    /// terrain surface plus the surface offset — not on a flat plane.
    /// </summary>
    public class ConstructionBuildGridSurfaceTests
    {
        private const float DefaultInsetNormalized = 0.08f;
        private const float DefaultSurfaceOffsetY = 0.06f;

        private FakeGridService _grid;
        private FakeGridGeometry _geometry;
        private FakeTerrainQuery _terrain;
        private FakeGridProjection _projection;
        private FakePassageMap _passages;

        [SetUp]
        public void SetUp()
        {
            _grid = new FakeGridService();
            _geometry = new FakeGridGeometry { CellSize = new Vector2(2f, 2f) };
            _terrain = new FakeTerrainQuery();
            _projection = new FakeGridProjection(new Vector2(2f, 2f));
            _passages = new FakePassageMap();
        }

        [Test]
        public void Build_SkipsTilesOutsideGrid()
        {
            // У чанку 2x2 реально існує лише (0,0) — решта поза сіткою.
            _grid.SetTileData(Vector2Int.zero, "grass");
            var builder = CreateBuilder();
            var descriptor = CreateDescriptor(0, 0, 2, 2);

            Assert.IsTrue(builder.TryBuild(descriptor, out Mesh mesh));
            Assert.AreEqual(4, mesh.vertexCount);
            Assert.AreEqual(6, mesh.triangles.Length);
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void Build_ReturnsFalse_WhenNoValidTiles()
        {
            var builder = CreateBuilder();
            Assert.IsFalse(builder.TryBuild(CreateDescriptor(0, 0, 2, 2), out Mesh mesh));
            Assert.IsNull(mesh);
        }

        [Test]
        public void Build_UsesGeometryCellSize_ForQuadExtents()
        {
            _geometry.CellSize = new Vector2(2f, 3f);
            _grid.SetTileData(Vector2Int.zero, "grass");
            var builder = CreateBuilder();

            Assert.IsTrue(builder.TryBuild(CreateDescriptor(0, 0, 1, 1), out Mesh mesh));

            float expectedHalfX = 2f * (1f - DefaultInsetNormalized * 2f) * 0.5f;
            float expectedHalfZ = 3f * (1f - DefaultInsetNormalized * 2f) * 0.5f;
            foreach (var v in mesh.vertices)
            {
                Assert.AreEqual(expectedHalfX, Mathf.Abs(v.x), 0.001f);
                Assert.AreEqual(expectedHalfZ, Mathf.Abs(v.z), 0.001f);
            }
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void Build_FallsBackToChunkBounds_WhenGeometryMissing()
        {
            // Без geometry-сервісу розмір клітини виводиться з WorldBounds/TileRect.
            _grid.SetTileData(Vector2Int.zero, "grass");
            var builder = CreateBuilder(withGeometry: false);
            var bounds = new Bounds(Vector3.zero, new Vector3(8f, 0f, 4f));
            var descriptor = new MapChunkDescriptor(
                new MapChunkCoord(0, 0),
                new RectInt(0, 0, 2, 2),
                bounds);

            Assert.IsTrue(builder.TryBuild(descriptor, out Mesh mesh));

            float expectedHalfX = (8f / 2f) * (1f - DefaultInsetNormalized * 2f) * 0.5f;
            foreach (var v in mesh.vertices)
                Assert.AreEqual(expectedHalfX, Mathf.Abs(v.x), 0.001f);
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void Build_ElevatedTile_UsesGeneratedTerrainSurfaceY()
        {
            var tile = new Vector2Int(1, 0);
            _grid.SetTileData(tile, "grass");
            _terrain.ExplicitSurfaceMap = true;
            _terrain.SurfaceY[tile] = 3.5f;
            var builder = CreateBuilder();

            Assert.IsTrue(builder.TryBuild(CreateDescriptor(0, 0, 2, 1), out Mesh mesh));

            foreach (var v in mesh.vertices)
                Assert.AreEqual(3.5f + DefaultSurfaceOffsetY, v.y, 0.001f);
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void Build_WithoutSurfaceMap_FallsBackToProjectedLevel()
        {
            var tile = new Vector2Int(1, 0);
            _grid.SetTileData(tile, "grass");
            // Немає explicit surface map: висота виводиться з terrain level
            // через проєкцію (level 2 -> y = 2 + offset).
            _terrain.Levels[tile] = 2;
            var builder = CreateBuilder();

            Assert.IsTrue(builder.TryBuild(CreateDescriptor(0, 0, 2, 1), out Mesh mesh));

            foreach (var v in mesh.vertices)
                Assert.AreEqual(2f + DefaultSurfaceOffsetY, v.y, 0.001f);
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void Build_TileCenters_ComeFromGeometryService()
        {
            var tile = new Vector2Int(1, 0);
            _grid.SetTileData(tile, "grass");
            _terrain.ExplicitSurfaceMap = true;
            _terrain.SurfaceY[tile] = 1f;
            // Геометрія каже, що центр клітини (1,0) — у (10, *, 20):
            _geometry.CellCenters[tile] = new Vector3(10f, 0f, 20f);
            var builder = CreateBuilder();

            Assert.IsTrue(builder.TryBuild(CreateDescriptor(0, 0, 2, 1), out Mesh mesh));

            float minX = float.MaxValue, maxX = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;
            foreach (var v in mesh.vertices)
            {
                minX = Mathf.Min(minX, v.x);
                maxX = Mathf.Max(maxX, v.x);
                minZ = Mathf.Min(minZ, v.z);
                maxZ = Mathf.Max(maxZ, v.z);
            }
            Assert.AreEqual(10f, (minX + maxX) * 0.5f, 0.001f);
            Assert.AreEqual(20f, (minZ + maxZ) * 0.5f, 0.001f);
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void Build_QuadsFaceUp_WithUpNormals()
        {
            _grid.SetTileData(Vector2Int.zero, "grass");
            var builder = CreateBuilder();

            Assert.IsTrue(builder.TryBuild(CreateDescriptor(0, 0, 1, 1), out Mesh mesh));

            foreach (var n in mesh.normals)
                Assert.AreEqual(Vector3.up, n);

            Vector3[] v = mesh.vertices;
            int[] t = mesh.triangles;
            for (int i = 0; i < t.Length; i += 3)
            {
                Vector3 normal = Vector3.Cross(
                    v[t[i + 1]] - v[t[i]],
                    v[t[i + 2]] - v[t[i]]);
                Assert.Greater(normal.y, 0f, "triangle must face up on XZ");
            }
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void Build_StairCell_QuadSlopesTowardClimbEdge()
        {
            // Модуль сходів у (1,0): сходить до +Z, верх = 2.25, низ = 2.0.
            // Плаский quad на висоті центру висів би над нижнім краєм рампи.
            var tile = new Vector2Int(1, 0);
            _grid.SetTileData(tile, "grass");
            _terrain.ExplicitSurfaceMap = true;
            _terrain.SurfaceY[tile] = 2.25f;
            _passages.Modules[tile] = new TerrainPassageModule
            {
                Cell = tile,
                DirectionIndex = 0,
                TopY = 2.25f,
                LowSurfaceY = 2.0f,
                HighSurfaceY = 3.0f,
            };

            var builder = CreateBuilder();
            Assert.IsTrue(builder.TryBuild(CreateDescriptor(0, 0, 2, 1), out Mesh mesh));

            float halfZ = 2f * (1f - DefaultInsetNormalized * 2f) * 0.5f;
            float centerZ = tile.y * 2f;
            foreach (Vector3 v in mesh.vertices)
            {
                bool highEdge = v.z > centerZ;
                float expected = (highEdge ? 2.25f : 2.0f) + DefaultSurfaceOffsetY;
                Assert.AreEqual(expected, v.y, 0.001f,
                    $"corner z={v.z} must sit on the ramp edge, halfZ={halfZ}");
            }
            foreach (Vector3 n in mesh.normals)
            {
                Assert.Greater(n.y, 0.5f, "stair quad still faces up");
                Assert.Less(n.y, 1f, "stair quad normal leans, not flat-up");
                Assert.Less(n.z, 0f, "normal leans away from the +Z climb");
            }
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void Build_StairCell_LowEdgeFollowsPreviousModuleTop()
        {
            // Другий модуль прольоту: низ quad'а = TopY попереднього модуля.
            var tile = new Vector2Int(1, 0);
            var prev = new Vector2Int(1, -1);
            _grid.SetTileData(tile, "grass");
            _terrain.ExplicitSurfaceMap = true;
            _terrain.SurfaceY[tile] = 2.5f;
            _passages.Modules[tile] = new TerrainPassageModule
            {
                Cell = tile, DirectionIndex = 0, TopY = 2.5f,
                LowSurfaceY = 2.0f, HighSurfaceY = 3.0f,
            };
            _passages.Modules[prev] = new TerrainPassageModule
            {
                Cell = prev, DirectionIndex = 0, TopY = 2.25f,
                LowSurfaceY = 2.0f, HighSurfaceY = 3.0f,
            };

            var builder = CreateBuilder();
            Assert.IsTrue(builder.TryBuild(CreateDescriptor(0, 0, 2, 1), out Mesh mesh));

            float centerZ = tile.y * 2f;
            foreach (Vector3 v in mesh.vertices)
            {
                float expected = (v.z > centerZ ? 2.5f : 2.25f) + DefaultSurfaceOffsetY;
                Assert.AreEqual(expected, v.y, 0.001f);
            }
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void Collect_StairCell_TiltsQuadAlongClimb()
        {
            var tile = new Vector2Int(1, 0);
            _grid.SetTileData(tile, "grass");
            _terrain.ExplicitSurfaceMap = true;
            _terrain.SurfaceY[tile] = 2.25f;
            _passages.Modules[tile] = new TerrainPassageModule
            {
                Cell = tile, DirectionIndex = 0, TopY = 2.25f,
                LowSurfaceY = 2.0f, HighSurfaceY = 3.0f,
            };

            var alignment = new ConstructionTerrainAlignmentService(
                _grid,
                gridProjection: _projection,
                gridGeometry: _geometry,
                generatedTerrainLevelQuery: _terrain,
                terrainPassages: _passages);
            var collector = new ConstructionBuildGridTileCollector(
                _grid, alignment, _geometry);

            var entries = new List<ConstructionBuildGridOverlayEntry>();
            collector.Collect(entries, _ => ConstructionBuildGridTileVisualState.General);

            Assert.AreEqual(1, entries.Count);
            Matrix4x4 m = entries[0].Matrix;

            // Верхній край (локальний +Y) сидить на TopY, нижній — на LowSurfaceY.
            Vector3 topEdge = m.MultiplyPoint(new Vector3(0f, 0.5f, 0f));
            Vector3 lowEdge = m.MultiplyPoint(new Vector3(0f, -0.5f, 0f));
            Assert.AreEqual(2.25f + DefaultSurfaceOffsetY, topEdge.y, 0.01f);
            Assert.AreEqual(2.0f + DefaultSurfaceOffsetY, lowEdge.y, 0.01f);
            Assert.Greater(topEdge.z, lowEdge.z, "quad climbs toward +Z");
            // Перпендикулярна ширина квада не змінюється.
            Vector3 right = m.MultiplyVector(new Vector3(1f, 0f, 0f));
            Assert.AreEqual(0f, right.y, 0.001f);
        }

        private ConstructionBuildGridChunkSurfaceBuilder CreateBuilder(
            bool withGeometry = true,
            bool withProjection = true)
        {
            var alignment = new ConstructionTerrainAlignmentService(
                _grid,
                gridProjection: withProjection ? _projection : null,
                gridGeometry: withGeometry ? _geometry : null,
                generatedTerrainLevelQuery: _terrain,
                terrainPassages: _passages);
            return new ConstructionBuildGridChunkSurfaceBuilder(
                _grid,
                alignment,
                gridGeometry: withGeometry ? _geometry : null);
        }

        private static MapChunkDescriptor CreateDescriptor(
            int x, int y, int width, int height)
            => new MapChunkDescriptor(
                new MapChunkCoord(x, y),
                new RectInt(x, y, width, height),
                new Bounds(Vector3.zero, Vector3.one));

        private sealed class FakeGridService : IGridService
        {
            private readonly Dictionary<Vector2Int, string> _tiles = new();

            public string GetTileData(Vector2Int position)
                => _tiles.TryGetValue(position, out var id) ? id : null;

            public bool TryGetTileData(Vector2Int position, out string tileTypeId)
                => _tiles.TryGetValue(position, out tileTypeId);

            public void SetTileData(Vector2Int position, string tileTypeId)
                => _tiles[position] = tileTypeId;

            public int GridWidth => 100;
            public int GridHeight => 100;
        }

        private sealed class FakeGridGeometry : IConstructionGridGeometryService
        {
            public Vector2 CellSize = Vector2.one;
            public readonly Dictionary<Vector2Int, Vector3> CellCenters = new();

            public bool TryGetCellCenter(Vector2Int tile, out Vector3 center)
            {
                if (CellCenters.TryGetValue(tile, out center))
                    return true;
                center = new Vector3(tile.x * CellSize.x, 0f, tile.y * CellSize.y);
                return true;
            }

            public bool TryGetCellSize(out Vector2 size)
            {
                size = CellSize;
                return true;
            }

            public bool TryGetCellAtWorld(Vector3 worldPosition, out Vector2Int tile)
            {
                tile = default;
                return false;
            }

            public bool TryGetGridPlaneY(out float y)
            {
                y = 0f;
                return false;
            }
        }

        private sealed class FakeTerrainQuery : IGeneratedTerrainLevelQuery
        {
            public bool ExplicitSurfaceMap;
            public readonly Dictionary<Vector2Int, float> SurfaceY = new();
            public readonly Dictionary<Vector2Int, int> Levels = new();

            public bool HasExplicitTerrainSurfaceMap => ExplicitSurfaceMap;

            public bool TryGetTerrainLevel(Vector2Int position, out int level)
                => Levels.TryGetValue(position, out level);

            public bool TryGetTerrainSurfaceY(Vector2Int position, out float surfaceY)
                => SurfaceY.TryGetValue(position, out surfaceY);
        }

        private sealed class FakePassageMap : ITerrainPassageMap
        {
            public readonly Dictionary<Vector2Int, TerrainPassageModule> Modules = new();

            public int Version => 1;
            public bool HasPassages => Modules.Count > 0;

            public bool TryGetModule(Vector2Int cell, out TerrainPassageModule module)
                => Modules.TryGetValue(cell, out module);

            public bool IsStairCell(Vector2Int cell) => Modules.ContainsKey(cell);

            public bool IsStairStep(Vector2Int from, Vector2Int to) => false;
        }

        private sealed class FakeGridProjection : IGridProjection
        {
            private readonly Vector2 _cellSize;

            public FakeGridProjection(Vector2 cellSize) => _cellSize = cellSize;

            public GridProjectionMode ProjectionMode => GridProjectionMode.Orthographic3D;
            public GridTopology Topology => GridTopology.Orthogonal;
            public GridWorldPlane WorldPlane => GridWorldPlane.XZ;

            public Vector3 GridToWorld(Vector2Int gridPosition)
                => new Vector3(gridPosition.x * _cellSize.x, 0f, gridPosition.y * _cellSize.y);

            public Vector3 GridToWorld(Vector2Int gridPosition, float elevation, float layerOffset = 0f)
                => new Vector3(
                    gridPosition.x * _cellSize.x,
                    elevation + layerOffset,
                    gridPosition.y * _cellSize.y);

            public Vector2Int WorldToGrid(Vector3 worldPosition)
                => new Vector2Int(
                    Mathf.RoundToInt(worldPosition.x / _cellSize.x),
                    Mathf.RoundToInt(worldPosition.z / _cellSize.y));

            public IEnumerable<Vector2Int> GetNeighborCandidates(Vector2Int gridPosition)
            {
                yield break;
            }

            public float GetStepDistance(Vector2Int from, Vector2Int to) => 1f;
            public float EstimateDistance(Vector2Int from, Vector2Int to) => 1f;
            public Bounds GetWorldBounds(int width, int height) => default;
        }
    }
}
