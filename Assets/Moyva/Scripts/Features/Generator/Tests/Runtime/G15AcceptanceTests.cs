using System;
using System.Collections.Generic;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// G15: the seabed must be a watertight sheet stitched to the real
    /// shoreline — every bed vertex strictly below the water surface, no
    /// holes in water cells, chunk borders sampled from one shared field,
    /// and map-edge borders closed by outward-facing skirts.
    /// </summary>
    public sealed class G15AcceptanceTests
    {
        private const float Surface = 0f;
        private const float CellSize = 1f;

        private static RecipeSeabedConfig Config() => new RecipeSeabedConfig
        {
            Enabled = true,
            ShallowShelfMeters = 1f,
            FalloffMeters = 3f,
            DepthCurveExponent = 1f,
            MaxDepthSeaMeters = 2f,
            MaxDepthLakeMeters = 1.2f,
            MaxDepthRiverMeters = 0.5f,
            RiverFalloffScale = 0.4f,
            ShoreRecessMeters = 0.02f,
            BorderSkirtMeters = 0.4f,
        };

        private static float[,] Surfaces(int w, int h, float v)
        {
            var s = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                s[x, y] = v;
            return s;
        }

        private static RecipeWaterKind[,] Kinds(
            int w, int h, RecipeWaterKind v, bool[,] water)
        {
            var k = new RecipeWaterKind[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                k[x, y] = water[x, y] ? v : RecipeWaterKind.None;
            return k;
        }

        // ---------- field level ----------

        [Test]
        public void ZeroShoreRecess_ShoreVertexStillBelowWaterSurface()
        {
            // ShoreRecessMeters is authorable down to zero; the shore vertex
            // must still recess below the sheet or it z-fights the waterline.
            const int w = 3, h = 3;
            var water = new bool[w, h];
            water[0, 0] = true;
            var config = Config();
            config.ShoreRecessMeters = 0f;
            var field = SeabedFieldPlanner.Build(
                w, h, CellSize, water,
                Surfaces(w, h, Surface), Surfaces(w, h, 0.5f),
                Kinds(w, h, RecipeWaterKind.Lake, water), config);

            Assert.IsTrue(field.TryGetCornerY(1, 1, out float shoreY));
            Assert.Less(shoreY, Surface - 0.004f,
                "shore vertex must recess under the water sheet even at zero configured recess");
        }

        [Test]
        public void NegativeShoreRecess_ShoreVertexNeverAboveWaterSurface()
        {
            // A negative recess would push the bed corner through the sheet.
            const int w = 3, h = 3;
            var water = new bool[w, h];
            water[0, 0] = true;
            var config = Config();
            config.ShoreRecessMeters = -0.05f;
            var field = SeabedFieldPlanner.Build(
                w, h, CellSize, water,
                Surfaces(w, h, Surface), Surfaces(w, h, 0.5f),
                Kinds(w, h, RecipeWaterKind.Lake, water), config);

            Assert.IsTrue(field.TryGetCornerY(1, 1, out float shoreY));
            Assert.Less(shoreY, Surface - 0.004f);
        }

        [Test]
        public void EveryWaterCell_AllFourCornersAreFinite()
        {
            // A NaN corner collapses to the cell centre fallback at mesh time;
            // on a bed that is a hole waiting to happen — every water cell
            // must have four real lattice heights, including map-edge cells.
            const int w = 5, h = 5;
            var water = new bool[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                water[x, y] = true;
            water[2, 2] = false; // enclosed land cell
            var field = SeabedFieldPlanner.Build(
                w, h, CellSize, water,
                Surfaces(w, h, Surface), Surfaces(w, h, 0.6f),
                Kinds(w, h, RecipeWaterKind.Sink, water), Config());

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (!water[x, y])
                    continue;
                for (int dx = 0; dx <= 1; dx++)
                for (int dy = 0; dy <= 1; dy++)
                    Assert.IsTrue(field.TryGetCornerY(x + dx, y + dy, out float cy)
                                  && float.IsFinite(cy),
                        $"corner ({x + dx},{y + dy}) of water cell ({x},{y})");
            }
        }

        [Test]
        public void EveryWaterCorner_NeverAboveLowestTouchingSurface()
        {
            // Confluence fixture: a high lake and a low sea sharing a vertex.
            // The corner must sit under the *lower* surface — if it rode the
            // higher target it would pierce the low sheet.
            const int w = 4, h = 4;
            var water = new bool[w, h];
            for (int x = 0; x < 2; x++)
            for (int y = 0; y < 2; y++)
                water[x, y] = true;           // low sea, SW quadrant
            for (int x = 2; x < 4; x++)
            for (int y = 2; y < 4; y++)
                water[x, y] = true;           // high lake, NE quadrant
            var waterY = Surfaces(w, h, Surface);
            var landY = Surfaces(w, h, 0.5f);
            for (int x = 2; x < 4; x++)
            for (int y = 2; y < 4; y++)
                waterY[x, y] = 3f;
            var kinds = Kinds(w, h, RecipeWaterKind.Lake, water);
            for (int x = 0; x < 2; x++)
            for (int y = 0; y < 2; y++)
                kinds[x, y] = RecipeWaterKind.Sink;
            var config = Config();
            var field = SeabedFieldPlanner.Build(
                w, h, CellSize, water, waterY, landY, kinds, config);

            Assert.IsTrue(field.TryGetCornerY(2, 2, out float corner));
            Assert.Less(corner, Surface - 0.004f,
                "shared corner must sit under the lowest touching water surface");
        }

        [Test]
        public void DepthAt_IsNonDecreasingWithShoreDistance()
        {
            var config = Config();
            foreach (RecipeWaterKind kind in new[]
                     {
                         RecipeWaterKind.Sink, RecipeWaterKind.Lake,
                         RecipeWaterKind.River,
                     })
            {
                float prev = float.NegativeInfinity;
                for (int i = 0; i <= 16; i++)
                {
                    float d = SeabedFieldPlanner.DepthAt(kind, i * 0.5f, config);
                    Assert.GreaterOrEqual(d, prev - 0.0001f,
                        $"{kind} depth regressed at dist {i * 0.5f}m");
                    prev = d;
                }
            }
        }

        [Test]
        public void EnclosedLandCell_HasNoBedButShoreCornersStitchUnderWater()
        {
            const int w = 3, h = 3;
            var water = new bool[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                water[x, y] = true;
            water[1, 1] = false;
            var config = Config();
            var field = SeabedFieldPlanner.Build(
                w, h, CellSize, water,
                Surfaces(w, h, Surface), Surfaces(w, h, 0.8f),
                Kinds(w, h, RecipeWaterKind.Lake, water), config);

            Assert.IsFalse(field.TryGetBedY(new Vector2Int(1, 1), out _));
            for (int vx = 1; vx <= 2; vx++)
            for (int vy = 1; vy <= 2; vy++)
            {
                Assert.IsTrue(field.TryGetCornerY(vx, vy, out float cy));
                Assert.Less(cy, Surface - 0.004f,
                    $"shore corner ({vx},{vy}) must recess under the sheet");
            }
        }

        // ---------- chunk-mesh level ----------

        private sealed class HydrologyStub : IRecipeHydrologyMap
        {
            public RecipeSeabedConfig SeabedConfig;
            public Func<Vector2Int, RecipeWaterKind> KindOf =
                _ => RecipeWaterKind.Lake;

            public int Version => 7;
            public bool HasHydrology => SeabedConfig != null;
            public float WaterfallMinDropMeters => 0.5f;
            public RecipeSeabedConfig Seabed => SeabedConfig;
            public RecipeWaterfallConfig Waterfalls => null;
            public bool IsRiverCell(Vector2Int c)
                => KindOf(c) == RecipeWaterKind.River;
            public bool IsLakeCell(Vector2Int c)
                => KindOf(c) == RecipeWaterKind.Lake;
            public bool IsWaterCell(Vector2Int c)
                => KindOf(c) != RecipeWaterKind.None;
            public RecipeWaterKind GetWaterKind(Vector2Int c) => KindOf(c);
            public bool TryGetWaterSurface(Vector2Int c, out float s)
            {
                s = 0f;
                return false;
            }
            public bool TryGetBedHeight(Vector2Int c, out float b)
            {
                b = 0f;
                return false;
            }
            public bool TryGetWaterfall(Vector2Int c, out Vector2Int d,
                out float u, out float l)
            {
                d = default;
                u = l = 0f;
                return false;
            }
            public bool TryGetFlowDirection(Vector2Int c, out Vector2Int dir)
            {
                dir = default;
                return false;
            }
        }

        // The real catalog loads the JSON atlas config; IsActive stays false
        // only if the sand theme cannot resolve a material, which the mesh
        // tests surface as a failed Prepare.
        private static TileLayerSample WaterSheet(float surfaceY)
            => new TileLayerSample(
                "water", "Water", null, null, "water", "water",
                LayerKind.BaseTerrain, 0, 0, 0, surfaceY - 1f, surfaceY, null,
                TileGeometryMode.SurfaceOnly);

        private static TileLayerSample Land(float surface)
            => new TileLayerSample(
                "terrain", "Terrain", null, null, "grass", "grass",
                LayerKind.BaseTerrain, 0, 0, 0, surface, surface, null,
                TileGeometryMode.SolidTerrain);

        private static SeabedChunkMeshService BuildService(
            int w, int h,
            Func<int, int, TileLayerSample> sample,
            HydrologyStub hydrology)
        {
            var cells = new Dictionary<Vector2Int, ResolvedTileComposition>();
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                var cell = new Vector2Int(x, y);
                cells[cell] = new ResolvedTileComposition(
                    cell, sample(x, y), default, hasMainTerrain: true,
                    hasOverlay: false, "test");
            }
            var service = new SeabedChunkMeshService(
                hydrology, new AtlasTileSetCatalog());
            service.Prepare(cells, w, h, CellSize);
            return service;
        }

        private static Vector3 FaceNormal(Vector3 a, Vector3 b, Vector3 c)
            => Vector3.Cross(b - a, c - a).normalized;

        [Test]
        public void ChunkMesh_TopFanFacesUp_AndSkirtsFaceOutward()
        {
            const int w = 2, h = 1;
            var hydro = new HydrologyStub { SeabedConfig = Config() };
            var service = BuildService(
                w, h,
                (x, y) => x == 0 ? WaterSheet(Surface) : Land(0.5f),
                hydro);
            {
                Assert.IsTrue(service.IsActive,
                    "seabed service inactive — atlas/material unresolved");
                Assert.IsTrue(service.TryBuildChunkMesh(
                    new RectInt(0, 0, w, h), out Mesh mesh, out Material mat));
                Assert.IsNotNull(mat);

                var v = mesh.vertices;
                var t = mesh.triangles;
                var center = Vector3.zero; // cell (0,0) centre in world XZ
                for (int i = 0; i < t.Length; i += 3)
                {
                    var a = v[t[i]]; var b = v[t[i + 1]]; var c = v[t[i + 2]];
                    var n = FaceNormal(a, b, c);
                    var centroid = (a + b + c) / 3f;
                    bool skirt = a.y < -0.3f || b.y < -0.3f || c.y < -0.3f;
                    if (!skirt)
                    {
                        // Top fan triangle: must face up, never inverted.
                        Assert.Greater(n.y, 0.9f, $"inverted top tri {i / 3}");
                    }
                    else
                    {
                        // Skirt quad half: horizontal normal pointing away
                        // from the cell centre, i.e. out of the map.
                        Assert.Less(Mathf.Abs(n.y), 0.01f, $"skirt {i / 3} tilted");
                        var outDir = centroid - center;
                        outDir.y = 0f;
                        Assert.Greater(Vector3.Dot(n, outDir.normalized), 0.5f,
                            $"skirt {i / 3} faces inward");
                    }
                }
            }
        }

        [Test]
        public void AdjacentChunks_ShareIdenticalBorderVertexHeights()
        {
            const int w = 4, h = 2;
            var hydro = new HydrologyStub { SeabedConfig = Config() };
            var service = BuildService(
                w, h, (x, y) => WaterSheet(Surface), hydro);
            {
                Assert.IsTrue(service.TryBuildChunkMesh(
                    new RectInt(0, 0, 2, h), out Mesh left, out _));
                Assert.IsTrue(service.TryBuildChunkMesh(
                    new RectInt(2, 0, 2, h), out Mesh right, out _));

                float seamX = 1.5f * CellSize; // border between cells x=1,2
                // A column can hold a top corner and its skirt bottom; the
                // seam contract is about the top surface, so compare maxima.
                static Dictionary<float, float> TopPerColumn(Mesh m, float sx)
                {
                    var map = new Dictionary<float, float>();
                    foreach (var p in m.vertices)
                    {
                        if (Mathf.Abs(p.x - sx) > 0.001f)
                            continue;
                        if (!map.TryGetValue(p.z, out float y) || p.y > y)
                            map[p.z] = p.y;
                    }
                    return map;
                }
                var leftEdge = TopPerColumn(left, seamX);
                var rightEdge = TopPerColumn(right, seamX);
                Assert.IsNotEmpty(leftEdge);
                foreach (var pair in rightEdge)
                {
                    Assert.IsTrue(leftEdge.TryGetValue(pair.Key, out float ly),
                        $"right-chunk border vert z={pair.Key} has no left twin");
                    Assert.AreEqual(ly, pair.Value, 0.0001f,
                        $"seam mismatch at z={pair.Key}");
                }
            }
        }

        [Test]
        public void MapBorderCell_EmitsSkirtOnlyOnMapEdges()
        {
            // Water cell at the SW map corner, land elsewhere: only the south
            // and west edges are map borders and may grow skirts; the east
            // and north edges face in-map land cells and must stay open.
            var hydro = new HydrologyStub { SeabedConfig = Config() };
            var service = BuildService(
                2, 2,
                (x, y) => x == 0 && y == 0 ? WaterSheet(Surface) : Land(0.5f),
                hydro);
            {
                Assert.IsTrue(service.TryBuildChunkMesh(
                    new RectInt(0, 0, 2, 2), out Mesh mesh, out _));
                Assert.AreEqual(4 + 4, mesh.triangles.Length / 3,
                    "4 fan tris + 2 skirt quads expected");

                // 2 skirts × 2 bottom verts; all must sit on the two
                // map-border planes (west x=-cs/2, south z=-cs/2).
                int deep = 0;
                foreach (var p in mesh.vertices)
                {
                    if (p.y > -0.3f)
                        continue;
                    deep++;
                    bool onBorder =
                        Mathf.Abs(p.x + 0.5f * CellSize) < 0.001f
                        || Mathf.Abs(p.z + 0.5f * CellSize) < 0.001f;
                    Assert.IsTrue(onBorder,
                        $"skirt bottom off the border planes at {p}");
                }
                Assert.AreEqual(4, deep, "skirt bottom vert count");
            }
        }

        [Test]
        public void InteriorCell_EmitsNoSkirtIntoMap()
        {
            // Water on all cells of a 2x2 map. Interior corners are legitimately
            // deep (far from shore), so depth alone cannot flag a skirt — a
            // skirt shows as two verts sharing (x,z) separated by the drop.
            var config = Config();
            var hydro = new HydrologyStub { SeabedConfig = config };
            var service = BuildService(
                2, 2, (x, y) => WaterSheet(Surface), hydro);
            {
                Assert.IsTrue(service.TryBuildChunkMesh(
                    new RectInt(0, 0, 2, 2), out Mesh mesh, out _));
                var columns = new Dictionary<Vector2, List<float>>();
                foreach (var p in mesh.vertices)
                {
                    var key = new Vector2(p.x, p.z);
                    if (!columns.TryGetValue(key, out var ys))
                        columns[key] = ys = new List<float>();
                    ys.Add(p.y);
                }
                foreach (var pair in columns)
                {
                    float min = float.MaxValue, max = float.MinValue;
                    foreach (float y in pair.Value)
                    {
                        if (y < min) min = y;
                        if (y > max) max = y;
                    }
                    if (max - min < 0.1f)
                        continue; // coincident duplicate top verts, not a skirt
                    bool onBorder =
                        Mathf.Abs(pair.Key.x + 0.5f) < 0.001f
                        || Mathf.Abs(pair.Key.x - 1.5f) < 0.001f
                        || Mathf.Abs(pair.Key.y + 0.5f) < 0.001f
                        || Mathf.Abs(pair.Key.y - 1.5f) < 0.001f;
                    Assert.IsTrue(onBorder,
                        $"dropped vertex column inside the map at {pair.Key}");
                }
            }
        }

        [Test]
        public void MeshVertices_NeverCoplanarOrAboveWaterSurface()
        {
            // Mesh-level guard for the zero-recess defect: with the authored
            // floor clamped, every emitted vertex must stay below the sheet.
            var config = Config();
            config.ShoreRecessMeters = 0f;
            var hydro = new HydrologyStub { SeabedConfig = config };
            var service = BuildService(
                2, 2,
                (x, y) => x == 0 ? WaterSheet(Surface) : Land(0.5f),
                hydro);
            {
                Assert.IsTrue(service.TryBuildChunkMesh(
                    new RectInt(0, 0, 2, 2), out Mesh mesh, out _));
                foreach (var p in mesh.vertices)
                    Assert.Less(p.y, Surface - 0.004f,
                        $"bed vertex at/beyond water plane: {p}");
            }
        }

        [Test]
        public void NoWaterOrDisabledConfig_ProducesNoField()
        {
            var hydro = new HydrologyStub { SeabedConfig = Config() };
            var service = BuildService(
                2, 2, (x, y) => Land(0.5f), hydro);
            {
                Assert.IsFalse(service.HasField);
                Assert.IsFalse(service.TryBuildChunkMesh(
                    new RectInt(0, 0, 2, 2), out _, out _));
            }

            var disabled = Config();
            disabled.Enabled = false;
            var hydro2 = new HydrologyStub { SeabedConfig = disabled };
            var service2 = BuildService(
                2, 2, (x, y) => WaterSheet(Surface), hydro2);
            {
                Assert.IsFalse(service2.HasField);
            }
        }
    }
}
