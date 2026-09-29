using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using UnityEditor;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    /// <summary>
    /// Acceptance cases for automatic waterfall fronts: flat water emits
    /// nothing, deep beds under flat water emit nothing, dry cliffs emit
    /// nothing, real drops merge into fronts per ledge, corner diagonals
    /// don't double-cover one pour event.
    /// </summary>
    public class WaterfallFieldPlannerTests
    {
        private const float MinDrop = 2f;

        [Test]
        public void ParallelLedges_MergeIndependently()
        {
            var g = new Grid(8, 8);
            for (int x = 1; x <= 3; x++)
            {
                g.Water(x, 1, 3f); g.Water(x, 2, 0f);
                g.Water(x, 5, 3f); g.Water(x, 6, 0f);
            }
            var field = WaterfallFieldPlanner.Build(8, 8, 1f,
                g.Sheet, g.Surf, g.Target, 0.02f, true);
            Assert.AreEqual(2, field.Fronts.Count);
            Assert.That(field.Fronts.All(f => f.WidthCells == 3));
        }

        [TestCase(0.025f, false, 0f)]
        [TestCase(0.025f, true, 0f)]
        [TestCase(2f, true, 0f)]
        [TestCase(2f, true, 1f)]
        [TestCase(0.35f, true, 0f)]
        public void CornerMeshes_ReachBothSurfacesAndShareSeam(float drop, bool usePrefab, float secondBottom)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Moyva/Art/Materials/Waterfall_SimpleStable.mat");
            var prefab = usePrefab ? AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/ThirdParty/Stylized Water 3/Prefabs/Waterfalls/Waterfall_5x20.prefab") : null;
            Assert.IsNotNull(material);
            Assert.IsFalse(ShaderUtil.ShaderHasError(material.shader), "Waterfall shader must compile.");
            if (usePrefab)
            {
                Assert.IsNotNull(prefab);
                Assert.IsTrue(prefab.GetComponent<MeshFilter>().sharedMesh.isReadable);
            }
            var config = new RecipeWaterfallConfig { CurtainMaterial = material,
                CurtainPrefab = prefab, MinDropMeters = 0.02f };
            var store = new RecipeHydrologyStore();
            store.Replace(new RecipeHydrologyPlan { Waterfalls = config, RiverMask = new bool[3,3] });
            var service = new WaterfallChunkMeshService(store, profile:
                new ResolvedGenerationProfile("test", true, true, material, 0f, false, ""));
            var cells = new Dictionary<Vector2Int, ResolvedTileComposition>();
            AddWater(new Vector2Int(1, 1), drop);
            AddWater(new Vector2Int(2, 1), 0f);
            AddWater(new Vector2Int(1, 2), secondBottom);
            service.Prepare(cells, 3, 3, 1f);
            var meshes = new List<WaterfallChunkMeshService.Curtain>();
            try
            {
                Assert.AreEqual(2, service.CollectChunkMeshes(new RectInt(0, 0, 3, 3), meshes));
                foreach (var curtain in meshes)
                {
                    Assert.Greater(curtain.Mesh.bounds.max.y, drop * 0.9f);
                    Assert.Less(curtain.Mesh.bounds.min.y, secondBottom + 0.0001f);
                    Assert.IsTrue(curtain.Mesh.vertices.All(v => !float.IsNaN(v.x + v.y + v.z)));
                    if (usePrefab)
                    {
                        StringAssert.StartsWith("waterfall_prefab_", curtain.Mesh.name);
                        Assert.AreEqual(curtain.Mesh.vertexCount, curtain.Mesh.uv2.Length);
                        Assert.That(curtain.Mesh.uv2.Any(v => v.y < 0f), "Foam must approach the lip over the upper sheet.");
                        Assert.That(curtain.Mesh.uv2.Any(v => v.y > 1f), "Impact foam must spread over the lower sheet.");
                        Assert.That(curtain.Mesh.uv2.All(v => v.y >= -0.301f && v.y <= 1.301f));
                    }
                }
                var a = meshes[0].Mesh.vertices;
                var b = meshes[1].Mesh.vertices;
                var shared = a.Where(v => b.Any(w => (v - w).sqrMagnitude < 0.000001f)).ToArray();
                Assert.GreaterOrEqual(shared.Length, usePrefab ? 3 : 4,
                    "Corner needs a shared seam from lip to impact, including unequal lower pools.");
                Assert.Greater(shared.Max(v => v.y), drop * 0.9f);
                Assert.Less(shared.Min(v => v.y), secondBottom + 0.0001f);
                if (usePrefab)
                    foreach (var curtain in meshes)
                    {
                        var vertices = curtain.Mesh.vertices;
                        var indices = curtain.Mesh.triangles;
                        for (int i = 0; i < indices.Length; i += 3)
                        {
                            var heights = new[] { vertices[indices[i]].y, vertices[indices[i+1]].y, vertices[indices[i+2]].y };
                            Assert.LessOrEqual(heights.Max() - heights.Min(), 0.084f,
                                "No stretched triangles at unequal pool corners.");
                        }
                    }

            }
            finally
            {
                foreach (var curtain in meshes) Object.DestroyImmediate(curtain.Mesh);
                foreach (var mat in meshes.Select(m => m.Material).Distinct()) Object.DestroyImmediate(mat);
            }

            void AddWater(Vector2Int cell, float height)
            {
                var sample = new TileLayerSample("water", "water", "", "", "water", "water",
                    default, 0, 0, 0, height, height, "water", TileGeometryMode.SurfaceOnly);
                cells.Add(cell, new ResolvedTileComposition(cell, sample, default, true, false, "test"));
            }
        }

        [Test]
        public void NonReadableDecorationMesh_IsNotReadAsCard()
        {
            var mesh = new Mesh { vertices = new[] { Vector3.zero, Vector3.right, Vector3.up },
                triangles = new[] { 0, 1, 2 } };
            try
            {
                mesh.UploadMeshData(true);
                Assert.AreEqual(0, Kruty1918.SpriteGrounding.QuadCardProjector.ExtractQuads(mesh,
                    new List<Kruty1918.SpriteGrounding.CardQuad>()));
            }
            finally { Object.DestroyImmediate(mesh); }
        }

        /// <summary>All-land grid: add water cells explicitly per case.</summary>
        private sealed class Grid
        {
            public readonly bool[,] Sheet;
            public readonly float[,] Surf;
            public readonly bool[,] Target;
            public readonly int W, H;

            public Grid(int w, int h)
            {
                W = w; H = h;
                Sheet = new bool[w, h];
                Surf = new float[w, h];
                Target = new bool[w, h];
                for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    Surf[x, y] = float.NaN;
            }

            public void Water(int x, int y, float surfaceY)
            {
                Sheet[x, y] = true;
                Surf[x, y] = surfaceY;
                Target[x, y] = true;
            }

            public void Land(int x, int y, float surfaceY)
            {
                Sheet[x, y] = false;
                Surf[x, y] = surfaceY;
                Target[x, y] = false;
            }

            public WaterfallFieldPlanner.Field Build(float minDrop = MinDrop)
                => WaterfallFieldPlanner.Build(W, H, 1f, Sheet, Surf, Target, minDrop);
        }

        [Test]
        public void FlatWater_ProducesNoFronts()
        {
            var g = new Grid(8, 8);
            for (int x = 2; x <= 5; x++)
            for (int y = 2; y <= 5; y++)
                g.Water(x, y, 1.5f);
            Assert.AreEqual(0, g.Build().Fronts.Count);
        }

        [Test]
        public void OneLevelWaterDrop_ProducesFrontAtConfiguredThreshold()
        {
            var g = new Grid(3, 3);
            g.Water(1, 1, 2f);
            g.Water(1, 2, 1f);

            var field = g.Build(minDrop: 1f);

            Assert.AreEqual(1, field.Fronts.Count);
            Assert.AreEqual(1f, field.Fronts[0].Drop);
        }

        [Test]
        public void SmallRenderedDrop_ProducesFront()
        {
            var g = new Grid(3, 3);
            g.Water(1, 1, 0.42f);
            g.Water(1, 2, 0.20f);
            var field = g.Build(0.02f);
            Assert.AreEqual(1, field.Fronts.Count);
            Assert.AreEqual(0.22f, field.Fronts[0].Drop, 0.0001f);
        }

        [Test]
        public void SquareSheets_TouchingOnlyAtCorner_DoNotCreateFloatingCurtain()
        {
            var g = new Grid(3, 3);
            g.Water(1, 1, 0.92f);
            g.Water(2, 2, 0.20f);
            var field = WaterfallFieldPlanner.Build(3, 3, 1f,
                g.Sheet, g.Surf, g.Target, 0.02f, cardinalEdgesOnly: true);
            Assert.IsEmpty(field.Fronts);
        }

        [Test]
        public void AdjacentSmallDropsAtDifferentLevels_KeepSeparateLips()
        {
            var g = new Grid(4, 3);
            g.Water(1, 1, 0.42f);
            g.Water(2, 1, 0.47f);
            g.Water(1, 2, 0.20f);
            g.Water(2, 2, 0.20f);
            var field = g.Build(0.02f);
            int northFronts = 0;
            foreach (var front in field.Fronts)
                if (front.Dir == Vector2Int.up) { northFronts++; Assert.AreEqual(1, front.WidthCells); }
            Assert.AreEqual(2, northFronts);
        }

        [Test]
        public void DeepBedUnderFlatWater_ProducesNoFronts()
        {
            // Bed depth is not a surface drop: a flat water surface over a
            // deep bed must not emit a fall — the planner only sees sheets.
            var g = new Grid(8, 8);
            for (int x = 2; x <= 5; x++)
            for (int y = 2; y <= 5; y++)
                g.Water(x, y, 1.5f);
            Assert.AreEqual(0, g.Build().Fronts.Count);
        }

        [Test]
        public void DropOntoDryCliff_ProducesNoFronts()
        {
            var g = new Grid(8, 8);
            g.Water(3, 3, 4f);
            g.Land(4, 3, 1f);           // lower neighbour, but land
            Assert.AreEqual(0, g.Build().Fronts.Count);
        }

        [Test]
        public void NarrowStream_TwoLevelDrop_ProducesOneFront()
        {
            var g = new Grid(8, 8);
            g.Water(3, 4, 4f);
            g.Water(4, 4, 1.5f);
            var field = g.Build();

            Assert.AreEqual(1, field.Fronts.Count);
            var front = field.Fronts[0];
            Assert.AreEqual(1, front.WidthCells);
            Assert.AreEqual(new Vector2Int(1, 0), front.Dir);
            Assert.AreEqual(new Vector2Int(3, 4), front.Anchor);
            Assert.AreEqual(4f, front.TopY, 0.001f);
            Assert.AreEqual(1.5f, front.BottomY, 0.001f);
            Assert.AreEqual(2.5f, front.Drop, 0.001f);
            Assert.IsTrue(field.IsCovered(new Vector2Int(3, 4), new Vector2Int(1, 0)));
            Assert.IsFalse(field.IsCovered(new Vector2Int(4, 4), new Vector2Int(-1, 0)));
        }

        [Test]
        public void WidePour_MergesIntoOneFront()
        {
            var g = new Grid(10, 8);
            for (int x = 3; x <= 5; x++)
            {
                g.Water(x, 4, 4f);
                g.Water(x, 3, 1.5f);
            }
            var field = g.Build();

            Assert.AreEqual(1, field.Fronts.Count);
            var front = field.Fronts[0];
            Assert.AreEqual(new Vector2Int(0, -1), front.Dir);
            Assert.AreEqual(3, front.WidthCells);
            // Anchor is the middle edge cell — deterministic owner.
            Assert.AreEqual(new Vector2Int(4, 4), front.Anchor);
        }

        [Test]
        public void SplitBottomHeights_SplitFronts()
        {
            var g = new Grid(10, 8);
            for (int x = 3; x <= 5; x++)
                g.Water(x, 4, 4f);
            g.Water(3, 3, 1.5f);
            g.Water(4, 3, 1.5f);
            g.Water(5, 3, 0.2f);          // deeper pocket — its own ledge
            var field = g.Build();

            Assert.AreEqual(2, field.Fronts.Count);
            foreach (var front in field.Fronts)
            {
                Assert.AreEqual(new Vector2Int(0, -1), front.Dir);
                Assert.GreaterOrEqual(front.Drop, MinDrop);
            }
        }

        [Test]
        public void Cascade_ProducesFrontPerLedge()
        {
            var g = new Grid(10, 8);
            g.Water(3, 5, 5f);            // upper pool
            g.Water(4, 5, 3f);            // middle pool (2 m ledge)
            g.Water(4, 4, 0.8f);          // bottom pool (2.2 m ledge)
            var field = g.Build();

            Assert.AreEqual(2, field.Fronts.Count);
            foreach (var front in field.Fronts)
            {
                Assert.AreEqual(1, front.WidthCells);
                Assert.GreaterOrEqual(front.Drop, MinDrop);
            }
        }

        [Test]
        public void CornerPour_SuppressesDiagonalDuplicate()
        {
            var g = new Grid(8, 8);
            g.Water(3, 3, 4f);
            g.Water(4, 3, 1.5f);
            g.Water(3, 4, 1.5f);
            g.Water(4, 4, 1.5f);
            var field = g.Build();

            // The two ortho faces already tile the corner; the diagonal
            // edge of the same cell is dropped — no double fall on one
            // D8 pour event.
            Assert.AreEqual(2, field.Fronts.Count);
            Assert.IsFalse(field.IsCovered(new Vector2Int(3, 3), new Vector2Int(1, 1)));
            Assert.IsTrue(field.IsCovered(new Vector2Int(3, 3), new Vector2Int(1, 0)));
            Assert.IsTrue(field.IsCovered(new Vector2Int(3, 3), new Vector2Int(0, 1)));
        }

        [Test]
        public void LoneDiagonal_KeptWhenFlankMissing()
        {
            var g = new Grid(8, 8);
            g.Water(3, 3, 4f);
            g.Water(4, 4, 1.5f);
            var field = g.Build();

            Assert.AreEqual(1, field.Fronts.Count);
            Assert.AreEqual(new Vector2Int(1, 1), field.Fronts[0].Dir);
            Assert.IsTrue(field.IsCovered(new Vector2Int(3, 3), new Vector2Int(1, 1)));
        }

        [Test]
        public void NonSheetUpperCell_ProducesNoFronts()
        {
            var g = new Grid(8, 8);
            g.Land(3, 3, 4f);             // solid terrain, not a water sheet
            g.Water(4, 3, 1.5f);
            Assert.AreEqual(0, g.Build().Fronts.Count);
        }
    }
}
