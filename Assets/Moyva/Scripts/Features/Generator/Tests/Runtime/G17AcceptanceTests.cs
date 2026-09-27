using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// G17 acceptance invariants for waterfall curtain meshes: the mesh is
    /// emitted only from the G16 field (no independent detection), builds
    /// exactly once in the anchor chunk, spans the real top→bottom drop with
    /// the lip tucked under the upper sheet and the base under the lower
    /// (never coplanar), and carries a two-sided fall profile.
    /// </summary>
    public sealed class G17AcceptanceTests
    {
        private static readonly List<Object> ToDestroy = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var o in ToDestroy)
                if (o != null) Object.DestroyImmediate(o);
            ToDestroy.Clear();
        }

        private static T Track<T>(T o) where T : Object
        {
            ToDestroy.Add(o);
            return o;
        }

        private sealed class HydroStub : IRecipeHydrologyMap
        {
            public RecipeWaterfallConfig WaterfallConfig;
            public readonly HashSet<Vector2Int> Water = new HashSet<Vector2Int>();
            public int Version => 1;
            public bool HasHydrology => WaterfallConfig != null;
            public float WaterfallMinDropMeters => 0.5f;
            public RecipeSeabedConfig Seabed => null;
            public RecipeWaterfallConfig Waterfalls => WaterfallConfig;
            public bool IsRiverCell(Vector2Int c) => Water.Contains(c);
            public bool IsLakeCell(Vector2Int c) => Water.Contains(c);
            public bool IsWaterCell(Vector2Int c) => Water.Contains(c);
            public RecipeWaterKind GetWaterKind(Vector2Int c)
                => Water.Contains(c) ? RecipeWaterKind.River : RecipeWaterKind.None;
            public bool TryGetWaterSurface(Vector2Int c, out float s)
            {
                s = Water.Contains(c) ? 0f : float.NaN;
                return Water.Contains(c);
            }
            public bool TryGetBedHeight(Vector2Int c, out float b)
            {
                b = Water.Contains(c) ? -1f : float.NaN;
                return Water.Contains(c);
            }
            public bool TryGetFlowDirection(Vector2Int c, out Vector2Int d)
            {
                d = c; return Water.Contains(c);
            }
            public bool TryGetWaterfall(Vector2Int c, out Vector2Int d,
                out float upperY, out float lowerY)
            {
                d = c; upperY = lowerY = 0f; return false;
            }
        }

        private static TileLayerSample WaterSheet(float surface) =>
            new TileLayerSample("water", "water", null, null, "w", null,
                LayerKind.BaseTerrain, 0, 0, 0, surface, surface, null,
                TileGeometryMode.SurfaceOnly);

        private WaterfallChunkMeshService _service;
        private HydroStub _hydro;

        [SetUp]
        public void SetUp()
        {
            _hydro = new HydroStub
            {
                WaterfallConfig = new RecipeWaterfallConfig
                {
                    Enabled = true,
                    MinDropLevels = 1,
                    CurtainMaterial = Track(
                        new Material(Shader.Find("Hidden/InternalErrorShader"))),
                },
            };
            _service = new WaterfallChunkMeshService(_hydro, null);
        }

        private void PrepareWithFall()
        {
            // Upper water (5m) at (3,3) pours east onto lower water (4m) at (4,3).
            var cells = new Dictionary<Vector2Int, ResolvedTileComposition>();
            foreach (var (c, s) in new[]
                     { (new Vector2Int(3, 3), 5f), (new Vector2Int(4, 3), 4f) })
            {
                cells[c] = new ResolvedTileComposition(
                    c, WaterSheet(s), default, true, false, "g17");
                _hydro.Water.Add(c);
            }
            _service.Prepare(cells, 8, 8, 1f);
        }

        [Test]
        public void FrontAnchors_InsideChunk_BuildsThere_Only()
        {
            PrepareWithFall();
            Assert.IsTrue(_service.HasField);
            Assert.AreEqual(1, _service.Fronts.Count);

            Assert.IsTrue(_service.TryBuildChunkMesh(
                new RectInt(0, 0, 16, 16), out Mesh mesh, out Material mat));
            Assert.IsNotNull(mesh);
            Assert.AreSame(_hydro.WaterfallConfig.CurtainMaterial, mat);

            // A chunk that does not contain the anchor emits nothing —
            // curtains can't duplicate across borders.
            Assert.IsFalse(_service.TryBuildChunkMesh(
                new RectInt(0, 16, 16, 16), out _, out _));
            Track(mesh);
        }

        [Test]
        public void Curtain_SpansRealDrop_LipTucked_BaseSubmerged()
        {
            PrepareWithFall();
            Assert.IsTrue(_service.TryBuildChunkMesh(
                new RectInt(0, 0, 16, 16), out Mesh mesh, out _));
            Track(mesh);

            float minY = float.MaxValue, maxY = float.MinValue;
            foreach (var v in mesh.vertices)
            {
                minY = Mathf.Min(minY, v.y);
                maxY = Mathf.Max(maxY, v.y);
            }
            // Top lip tucks just under the 5m upper sheet, base dips below
            // the 4m lower sheet — the curtain joins both without z-fighting.
            Assert.Greater(maxY, 4.9f, "crest must reach the upper sheet");
            Assert.Less(maxY, 5.0f, "lip tucks under the upper rim, never above");
            Assert.Less(minY, 4.0f, "base must submerge under the lower sheet");
            foreach (var v in mesh.vertices)
            {
                Assert.That(Mathf.Abs(v.y - 5f), Is.GreaterThan(0.0001f),
                    "coplanar with upper sheet → z-fighting");
                Assert.That(Mathf.Abs(v.y - 4f), Is.GreaterThan(0.0001f),
                    "coplanar with lower sheet → z-fighting");
            }
        }

        [Test]
        public void Curtain_IsDoubleSided_AndUvComplete()
        {
            PrepareWithFall();
            Assert.IsTrue(_service.TryBuildChunkMesh(
                new RectInt(0, 0, 16, 16), out Mesh mesh, out _));
            Track(mesh);

            bool plus = false, minus = false;
            foreach (var n in mesh.normals)
            {
                if (n.x > 0.9f) plus = true;
                if (n.x < -0.9f) minus = true;
            }
            Assert.IsTrue(plus && minus, "curtain must emit both faces");
            Assert.AreEqual(mesh.vertexCount, mesh.uv.Length, "every vertex needs uv");
            Assert.AreEqual(mesh.vertexCount, mesh.normals.Length);
        }

        [Test]
        public void Inactive_WithoutMaterial_OrWithoutHydrology()
        {
            _hydro.WaterfallConfig.CurtainMaterial = null;
            var noMat = new WaterfallChunkMeshService(_hydro, null);
            noMat.Prepare(new Dictionary<Vector2Int, ResolvedTileComposition>(), 8, 8, 1f);
            Assert.IsFalse(noMat.IsActive);
            Assert.IsFalse(noMat.TryBuildChunkMesh(new RectInt(0, 0, 16, 16), out _, out _));

            var noHydro = new WaterfallChunkMeshService(null, null);
            Assert.IsFalse(noHydro.IsActive);
            noHydro.Prepare(new Dictionary<Vector2Int, ResolvedTileComposition>(), 8, 8, 1f);
            Assert.IsFalse(noHydro.TryBuildChunkMesh(new RectInt(0, 0, 16, 16), out _, out _));
        }
    }
}
