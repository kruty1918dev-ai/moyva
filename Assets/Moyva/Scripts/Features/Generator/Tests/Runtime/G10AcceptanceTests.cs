using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using Kruty1918.Moyva.MapChunks;
using Kruty1918.Moyva.MapChunks.API;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// G10 acceptance invariants for chunk-boundary seams: two adjacent
    /// 16×16 chunks built through the real builder must share coincident
    /// boundary vertices in world space (no accidental hole), every logical
    /// cell contributes exactly one fragment to exactly one chunk, and the
    /// logical-tile origin survives combining.
    /// </summary>
    public sealed class G10AcceptanceTests
    {
        private const int ChunkSize = 16;
        private const int MapW = 32;
        private const int MapH = 16;
        private const float CellSize = 1f;

        private static readonly List<Object> ToDestroy = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in ToDestroy)
                if (o != null)
                    Object.DestroyImmediate(o);
            ToDestroy.Clear();
        }

        private static T Track<T>(T o) where T : Object
        {
            ToDestroy.Add(o);
            return o;
        }

        /// <summary>Flat quad covering exactly one logical cell in map space.</summary>
        private sealed class QuadMeshSource : IResolvedTileMeshSource
        {
            private readonly Material _material;

            public QuadMeshSource(Material material) => _material = material;

            public int CollectMeshSources(
                ResolvedTileComposition composition, List<TileMeshSource> results)
            {
                Vector2Int c = composition.Cell;
                var mesh = Track(new Mesh { name = "G10Quad" });
                float x0 = c.x * CellSize, x1 = x0 + CellSize;
                float z0 = c.y * CellSize, z1 = z0 + CellSize;
                mesh.vertices = new[]
                {
                    new Vector3(x0, 0, z0), new Vector3(x1, 0, z0),
                    new Vector3(x1, 0, z1), new Vector3(x0, 0, z1),
                };
                mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
                mesh.RecalculateBounds();
                results.Add(new TileMeshSource(
                    mesh, new[] { _material }, Matrix4x4.identity,
                    tileGeometryMode: TileGeometryMode.SolidTerrain));
                return 1;
            }
        }

        private static Dictionary<Vector2Int, ResolvedTileComposition> Cells()
        {
            var map = new Dictionary<Vector2Int, ResolvedTileComposition>();
            for (int x = 0; x < MapW; x++)
            for (int y = 0; y < MapH; y++)
            {
                var c = new Vector2Int(x, y);
                map[c] = new ResolvedTileComposition(
                    c, default, default, hasMainTerrain: true,
                    hasOverlay: false, "g10");
            }
            return map;
        }

        private static ChunkBuildArea Area(int cx, int cy)
        {
            var rect = new RectInt(
                cx * ChunkSize, cy * ChunkSize, ChunkSize, ChunkSize);
            return new ChunkBuildArea(new MapChunkCoord(cx, cy), rect, rect, CellSize);
        }

        private static Mesh BuildChunk(
            ChunkTerrainMeshBuilder builder,
            IResolvedTileMeshSource source,
            Dictionary<Vector2Int, ResolvedTileComposition> cells,
            int cx)
        {
            var root = Track(new GameObject($"chunk-{cx}")).transform;
            int built = builder.Build(root, Area(cx, 0), cells, source);
            Assert.AreEqual(1, built, $"chunk {cx} produced no terrain object");
            var filter = root.GetComponentInChildren<MeshFilter>();
            Assert.IsNotNull(filter, $"chunk {cx} has no MeshFilter");
            Assert.IsNotNull(filter.sharedMesh);
            return filter.sharedMesh;
        }

        [Test]
        public void AdjacentChunks_ShareCoincidentBoundaryVertices()
        {
            var material = Track(new Material(Shader.Find("Hidden/InternalErrorShader")));
            var builder = new ChunkTerrainMeshBuilder(new ChunkFirstRuntimeMeshRegistry());
            var cells = Cells();
            var source = new QuadMeshSource(material);

            Mesh a = BuildChunk(builder, source, cells, 0);
            Mesh b = BuildChunk(builder, source, cells, 1);

            var west = new List<Vector3>();
            foreach (var v in b.vertices)
                if (Mathf.Abs(v.x - ChunkSize * CellSize) < 0.0001f)
                    west.Add(v);

            Assert.Greater(west.Count, 0, "east chunk carries no boundary verts");
            var aVerts = a.vertices;
            foreach (var wb in west)
            {
                bool found = false;
                foreach (var va in aVerts)
                    if (Mathf.Abs(va.x - wb.x) < 0.0001f
                        && Mathf.Abs(va.z - wb.z) < 0.0001f
                        && Mathf.Abs(va.y - wb.y) < 0.0001f)
                    {
                        found = true;
                        break;
                    }
                Assert.IsTrue(found,
                    $"west-edge vertex {wb} has no coincident east-edge vertex — seam hole");
            }
        }

        [Test]
        public void EachCell_ContributesToExactlyOneChunk()
        {
            var material = Track(new Material(Shader.Find("Hidden/InternalErrorShader")));
            var builder = new ChunkTerrainMeshBuilder(new ChunkFirstRuntimeMeshRegistry());
            var cells = Cells();
            var source = new QuadMeshSource(material);

            Mesh a = BuildChunk(builder, source, cells, 0);
            Mesh b = BuildChunk(builder, source, cells, 1);

            // 16×16 cells per chunk → 2 triangles per cell quad. Vertices weld
            // across shared cell edges, so coverage is measured in triangles.
            Assert.AreEqual(ChunkSize * ChunkSize * 2, a.triangles.Length / 3,
                "chunk 0 must own exactly its 16×16 cells");
            Assert.AreEqual(ChunkSize * ChunkSize * 2, b.triangles.Length / 3,
                "chunk 1 must own exactly its 16×16 cells");
            foreach (var v in a.vertices)
                Assert.LessOrEqual(v.x, ChunkSize * CellSize + 0.0001f,
                    "chunk 0 vertex leaked past its east border");
            foreach (var v in b.vertices)
                Assert.GreaterOrEqual(v.x, ChunkSize * CellSize - 0.0001f,
                    "chunk 1 vertex leaked past its west border");
        }

        [Test]
        public void LogicalTileOrigin_IsPreserved()
        {
            var material = Track(new Material(Shader.Find("Hidden/InternalErrorShader")));
            var builder = new ChunkTerrainMeshBuilder(new ChunkFirstRuntimeMeshRegistry());
            var cells = Cells();
            var source = new QuadMeshSource(material);

            Mesh a = BuildChunk(builder, source, cells, 0);
            // Cell (0,0)'s quad must begin at world origin — no offset drift.
            bool hasOrigin = false;
            foreach (var v in a.vertices)
                if (v.x < 0.0001f && v.z < 0.0001f)
                    hasOrigin = true;
            Assert.IsTrue(hasOrigin, "logical-tile origin (0,0) was not preserved");
        }
    }
}
