using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using NUnit.Framework;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    public sealed class TerrainWaterlineLiftTests
    {
        private const float Lift = 0.05f;

        private static TileLayerSample Land(string tileId, float surface)
            => new TileLayerSample(
                "terrain", "Terrain", null, null, tileId, tileId,
                LayerKind.BaseTerrain, 0, 0, 0, surface, surface, null,
                TileGeometryMode.SolidTerrain);

        private static TileLayerSample Land(string tileId, float surface, LayerKind kind)
            => new TileLayerSample(
                "terrain", "Terrain", null, null, tileId, tileId,
                kind, 0, 0, 0, surface, surface, null,
                TileGeometryMode.SolidTerrain);

        private static TileLayerSample Water(float surface)
            => new TileLayerSample(
                "water", "Water", null, null, "water", "water",
                LayerKind.BaseTerrain, 0, 0, 0, surface, surface, null,
                TileGeometryMode.SurfaceOnly);

        private static LogicalTileMap BuildMap(int width, int height)
            => new LogicalTileMap(width, height);

        private static TerrainWaterlineLiftConfig Config(
            int transitionCells = 0,
            float terraceJoin = 0.25f)
            => new TerrainWaterlineLiftConfig
            {
                Enabled = true,
                LiftMeters = Lift,
                TerraceJoinMeters = terraceJoin,
                TransitionCells = transitionCells,
                TransitionRiseMeters = 0.5f
            };

        [Test]
        public void SubmergedLand_LiftedAboveAdjacentWater()
        {
            var map = BuildMap(2, 1);
            map.AddSample(0, 0, Water(-0.25f));
            map.AddSample(1, 0, Land("grass", -0.5f));

            var plan = new TerrainWaterlineLiftPlanner().Apply(map, Config(), null);
            map.ReprojectAll();

            Assert.IsTrue(plan.Corrected[1, 0]);
            Assert.AreEqual(-0.25f + Lift, map.SurfaceHeights[1, 0], 0.0001f);
            Assert.AreEqual("grass", map.TileIds[1, 0]);
        }

        [Test]
        public void LandAboveWater_StaysUntouched()
        {
            var map = BuildMap(2, 1);
            map.AddSample(0, 0, Water(-0.25f));
            map.AddSample(1, 0, Land("grass", 0.5f));

            var plan = new TerrainWaterlineLiftPlanner().Apply(map, Config(), null);
            map.ReprojectAll();

            Assert.IsFalse(plan.Corrected[1, 0]);
            Assert.AreEqual(0.5f, map.SurfaceHeights[1, 0], 0.0001f);
        }

        [Test]
        public void MultipleWaterHeights_UsesHighestNeighbour()
        {
            var map = BuildMap(3, 1);
            map.AddSample(0, 0, Water(-0.25f));
            map.AddSample(1, 0, Land("grass", -0.5f));
            map.AddSample(2, 0, Water(0.3f));

            new TerrainWaterlineLiftPlanner().Apply(map, Config(), null);
            map.ReprojectAll();

            Assert.AreEqual(0.3f + Lift, map.SurfaceHeights[1, 0], 0.0001f);
        }

        [Test]
        public void DiagonalWater_CountsAsAdjacent()
        {
            var map = BuildMap(2, 2);
            map.AddSample(0, 0, Water(-0.25f));
            map.AddSample(1, 1, Land("grass", -0.5f));

            var plan = new TerrainWaterlineLiftPlanner().Apply(map, Config(), null);
            map.ReprojectAll();

            Assert.IsTrue(plan.Corrected[1, 1]);
            Assert.AreEqual(-0.25f + Lift, map.SurfaceHeights[1, 1], 0.0001f);
        }

        [Test]
        public void NonBaseTerrainWinner_StillLifted()
        {
            var map = BuildMap(2, 1);
            map.AddSample(0, 0, Water(-0.25f));
            map.AddSample(1, 0, Land("cliff", -0.5f, LayerKind.Cliff));

            var plan = new TerrainWaterlineLiftPlanner().Apply(map, Config(), null);
            map.ReprojectAll();

            Assert.IsTrue(plan.Corrected[1, 0]);
            Assert.AreEqual(-0.25f + Lift, map.SurfaceHeights[1, 0], 0.0001f);
            Assert.AreEqual("cliff", map.TileIds[1, 0]);
        }

        [Test]
        public void TerraceJoin_SnapsToStableNeighbourWithinReach()
        {
            var map = BuildMap(3, 1);
            map.AddSample(0, 0, Water(0f));           // floor = 0.05
            map.AddSample(1, 0, Land("grass", -0.5f)); // problem
            map.AddSample(2, 0, Land("grass", 0.2f));  // stable, within join 0.25

            var plan = new TerrainWaterlineLiftPlanner().Apply(map, Config(), null);
            map.ReprojectAll();

            Assert.AreEqual(0.2f, map.SurfaceHeights[1, 0], 0.0001f);
            Assert.AreEqual(0.2f, plan.TargetSurface[1, 0], 0.0001f);
        }

        [Test]
        public void TerraceJoin_HighNeighbourIgnored()
        {
            var map = BuildMap(3, 1);
            map.AddSample(0, 0, Water(0f));
            map.AddSample(1, 0, Land("grass", -0.5f));
            map.AddSample(2, 0, Land("grass", 5f)); // too far above floor to join

            new TerrainWaterlineLiftPlanner().Apply(map, Config(), null);
            map.ReprojectAll();

            Assert.AreEqual(0.05f, map.SurfaceHeights[1, 0], 0.0001f);
        }

        [Test]
        public void TransitionRing_EasesDeepLift()
        {
            var map = BuildMap(3, 1);
            map.AddSample(0, 0, Water(1f));           // floor = 1.05
            map.AddSample(1, 0, Land("grass", 0f));   // problem -> 1.05
            map.AddSample(2, 0, Land("grass", 0f));   // ring 1 -> 1.05 - 0.5

            var plan = new TerrainWaterlineLiftPlanner()
                .Apply(map, Config(transitionCells: 1), null);
            map.ReprojectAll();

            Assert.AreEqual(1.05f, map.SurfaceHeights[1, 0], 0.0001f);
            Assert.IsTrue(plan.Smoothed[2, 0]);
            Assert.AreEqual(0.55f, map.SurfaceHeights[2, 0], 0.0001f);
        }

        [Test]
        public void TransitionRing_Disabled_LeavesOuterCells()
        {
            var map = BuildMap(3, 1);
            map.AddSample(0, 0, Water(1f));
            map.AddSample(1, 0, Land("grass", 0f));
            map.AddSample(2, 0, Land("grass", 0f));

            var plan = new TerrainWaterlineLiftPlanner()
                .Apply(map, Config(transitionCells: 0), null);
            map.ReprojectAll();

            Assert.AreEqual(1.05f, map.SurfaceHeights[1, 0], 0.0001f);
            Assert.AreEqual(0f, map.SurfaceHeights[2, 0], 0.0001f);
            Assert.IsFalse(plan.Smoothed[2, 0]);
        }

        [Test]
        public void TransitionRing_NeverTouchesWater()
        {
            var map = BuildMap(3, 1);
            map.AddSample(0, 0, Water(1f));
            map.AddSample(1, 0, Land("grass", 0f));
            map.AddSample(2, 0, Water(0.5f));

            new TerrainWaterlineLiftPlanner()
                .Apply(map, Config(transitionCells: 1), null);
            map.ReprojectAll();

            Assert.AreEqual(0.5f, map.SurfaceHeights[2, 0], 0.0001f);
        }

        [Test]
        public void MapBorder_WaterInCorner_LiftsAllBelowNeighbours()
        {
            var map = BuildMap(2, 2);
            map.AddSample(0, 0, Water(-0.25f));
            map.AddSample(1, 0, Land("grass", -0.5f));
            map.AddSample(0, 1, Land("grass", -0.5f));
            map.AddSample(1, 1, Land("grass", -0.5f));

            var plan = new TerrainWaterlineLiftPlanner().Apply(map, Config(), null);
            map.ReprojectAll();

            for (int x = 0; x < 2; x++)
            for (int y = 0; y < 2; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                Assert.GreaterOrEqual(
                    map.SurfaceHeights[x, y], -0.25f + Lift - 0.0001f,
                    $"cell {x},{y}");
            }
            Assert.AreEqual(1, plan.Groups.Count);
            Assert.AreEqual(3, plan.Groups[0].Count);
        }

        [Test]
        public void SubmergedSamplesAndOverlays_ShiftWithColumn()
        {
            var map = BuildMap(2, 1);
            map.AddSample(0, 0, Water(-0.25f));
            // Land winner at -0.5 plus a buried water sheet below it and a
            // decoration riding the surface — the whole non-water column
            // must shift, the hidden sheet must stay.
            map.AddSample(1, 0, Land("grass", -0.5f));
            map.AddSample(1, 0, Water(-0.6f));
            map.AddSample(1, 0, new TileLayerSample(
                "deco", "Deco", null, null, "flowers", "flowers",
                LayerKind.Decoration, 0, 0, 0, -0.45f, -0.45f, null,
                TileGeometryMode.SolidTerrain));

            new TerrainWaterlineLiftPlanner().Apply(map, Config(), null);
            map.ReprojectAll();

            Assert.AreEqual(-0.25f + Lift, map.SurfaceHeights[1, 0], 0.0001f);

            var stack = map.GetCellStack(1, 0);
            float deco = float.NaN;
            float buriedWater = float.NaN;
            for (int i = 0; i < stack.Samples.Count; i++)
            {
                var s = stack.Samples[i];
                if (s.TileId == "flowers")
                    deco = s.SurfaceHeight;
                if (s.TileId == "water")
                    buriedWater = s.SurfaceHeight;
            }
            Assert.AreEqual(-0.45f + 0.3f, deco, 0.0001f);
            Assert.AreEqual(-0.6f, buriedWater, 0.0001f); // water never shifts
        }

        [Test]
        public void Disabled_IsNoOp()
        {
            var map = BuildMap(2, 1);
            map.AddSample(0, 0, Water(-0.25f));
            map.AddSample(1, 0, Land("grass", -0.5f));

            var config = Config();
            config.Enabled = false;
            var plan = new TerrainWaterlineLiftPlanner().Apply(map, config, null);
            map.ReprojectAll();

            Assert.IsNull(plan);
            Assert.AreEqual(-0.5f, map.SurfaceHeights[1, 0], 0.0001f);
        }

        [Test]
        public void IdenticalInput_IdenticalResult()
        {
            var a = BuildMap(3, 2);
            var b = BuildMap(3, 2);
            foreach (var map in new[] { a, b })
            {
                map.AddSample(0, 0, Water(-0.25f));
                map.AddSample(0, 1, Water(-0.25f));
                map.AddSample(1, 0, Land("grass", -0.6f));
                map.AddSample(1, 1, Land("grass", -0.6f));
                map.AddSample(2, 0, Land("grass", 0.4f));
                map.AddSample(2, 1, Land("grass", 0.4f));
            }

            new TerrainWaterlineLiftPlanner().Apply(a, Config(transitionCells: 2), null);
            new TerrainWaterlineLiftPlanner().Apply(b, Config(transitionCells: 2), null);
            a.ReprojectAll();
            b.ReprojectAll();

            for (int x = 0; x < 3; x++)
            for (int y = 0; y < 2; y++)
                Assert.AreEqual(a.SurfaceHeights[x, y], b.SurfaceHeights[x, y], 0.0001f);
        }

        [Test]
        public void WaterLikeTileId_TreatedAsWater()
        {
            var map = BuildMap(2, 1);
            map.AddSample(0, 0, Land("swamp", -0.25f)); // solid terrain, water-like id
            map.AddSample(1, 0, Land("grass", -0.5f));

            var plan = new TerrainWaterlineLiftPlanner()
                .Apply(map, Config(), new[] { "swamp" });
            map.ReprojectAll();

            Assert.IsTrue(plan.Corrected[1, 0]);
            Assert.AreEqual(-0.25f + Lift, map.SurfaceHeights[1, 0], 0.0001f);
        }

        [Test]
        public void EmptyAndNoWaterCells_NoCorrections()
        {
            var map = BuildMap(2, 2);
            map.AddSample(0, 0, Land("grass", -0.5f));
            // (1,0) empty, rest land above nothing

            var plan = new TerrainWaterlineLiftPlanner().Apply(map, Config(), null);

            Assert.AreEqual(0, plan.Groups.Count);
            Assert.AreEqual(-0.5f, plan.OriginalSurface[0, 0], 0.0001f);
        }
    }
}
