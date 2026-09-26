using Kruty1918.Moyva.Generator.API;
using Kruty1918.Moyva.Generator.Runtime;
using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using NUnit.Framework;
using UnityEngine;

namespace Kruty1918.Moyva.Generator.Tests.Runtime
{
    public sealed class SeabedFieldPlannerTests
    {
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

        private static void Fill(bool[,] mask, bool value)
        {
            for (int x = 0; x < mask.GetLength(0); x++)
            for (int y = 0; y < mask.GetLength(1); y++)
                mask[x, y] = value;
        }

        private static float[,] Surfaces(int w, int h, float value)
        {
            var s = new float[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                s[x, y] = value;
            return s;
        }

        private static RecipeWaterKind[,] Kinds(
            int w, int h, RecipeWaterKind value, bool[,] water)
        {
            var k = new RecipeWaterKind[w, h];
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                k[x, y] = water[x, y] ? value : RecipeWaterKind.None;
            return k;
        }

        [Test]
        public void OpenWater_DepthGrowsFromShoreToCentre()
        {
            const int w = 21, h = 21;
            var water = new bool[w, h];
            Fill(water, true);
            // Island of land in the middle removes it from the body.
            for (int x = 9; x <= 11; x++)
            for (int y = 9; y <= 11; y++)
                water[x, y] = false;

            var waterY = Surfaces(w, h, 0f);
            var landY = Surfaces(w, h, 0f);
            var kinds = Kinds(w, h, RecipeWaterKind.Sink, water);
            var field = SeabedFieldPlanner.Build(
                w, h, 1f, water, waterY, landY, kinds, Config());

            // Cell hugging the island shore vs mid-sea cell far from land.
            Assert.IsTrue(field.TryGetBedY(new Vector2Int(8, 10), out float shoreBed));
            Assert.IsTrue(field.TryGetBedY(new Vector2Int(0, 0), out _));
            Assert.Greater(shoreBed, -1f, "shore-adjacent bed stays near the surface");

            // Distances must be finite everywhere in the body.
            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                if (water[x, y])
                    Assert.IsFalse(float.IsInfinity(field.ShoreDistance[x, y]));
        }

        [Test]
        public void DepthProfile_ReachesMaxDepthOnFarCells()
        {
            const int w = 20, h = 1;
            // One long open-water strip ending at the map border on both
            // sides; a land cell in the middle is the only shore.
            var water = new bool[w, h];
            Fill(water, true);
            water[10, 0] = false;
            var waterY = Surfaces(w, h, 0f);
            var landY = Surfaces(w, h, 0f);
            var kinds = Kinds(w, h, RecipeWaterKind.Sink, water);
            var field = SeabedFieldPlanner.Build(
                w, h, 1f, water, waterY, landY, kinds, Config());

            // Distance to the single shore from the far end = 9 cells >
            // shelf (1) + falloff (3) => bed must sit at max depth.
            Assert.IsTrue(field.TryGetBedY(new Vector2Int(0, 0), out float far));
            Assert.AreEqual(-2f, far, 0.001f);

            // Cell next to the land is inside the shallow shelf.
            Assert.IsTrue(field.TryGetBedY(new Vector2Int(9, 0), out float near));
            Assert.AreEqual(-0.05f, near, 0.001f);
        }

        [Test]
        public void ShoreDistance_DoesNotJumpAcrossLandBetweenBodies()
        {
            const int w = 9, h = 9;
            var water = new bool[w, h];
            // Two lakes separated by a land wall at x==4 (except none touching).
            for (int y = 2; y <= 6; y++)
            {
                for (int x = 1; x <= 3; x++) water[x, y] = true;
                for (int x = 5; x <= 7; x++) water[x, y] = true;
            }
            var waterY = Surfaces(w, h, 0f);
            var landY = Surfaces(w, h, 0f);
            var kinds = Kinds(w, h, RecipeWaterKind.Lake, water);
            var field = SeabedFieldPlanner.Build(
                w, h, 1f, water, waterY, landY, kinds, Config());

            // The right lake's left edge touches land (x==4 wall) => dist 0;
            // its right edge borders the map edge => dist is interior-only.
            Assert.AreEqual(0f, field.ShoreDistance[5, 4]);
            // If distance leaked through the land wall the left lake's cells
            // would measure the right lake's shore; symmetric bodies must
            // have symmetric distance fields.
            Assert.AreEqual(field.ShoreDistance[2, 4], field.ShoreDistance[6, 4]);
        }

        [Test]
        public void NarrowRiver_GetsRiverDepthNotOceanDepth()
        {
            const int w = 9, h = 1;
            var water = new bool[w, h];
            Fill(water, true);
            var waterY = Surfaces(w, h, 0f);
            var landY = Surfaces(w, h, 0f);
            var kinds = Kinds(w, h, RecipeWaterKind.River, water);
            var field = SeabedFieldPlanner.Build(
                w, h, 1f, water, waterY, landY, kinds, Config());

            // No land anywhere: all cells get max distance => river max depth.
            for (int x = 0; x < w; x++)
            {
                Assert.IsTrue(field.TryGetBedY(new Vector2Int(x, 0), out float bed));
                Assert.AreEqual(-0.5f, bed, 0.001f);
            }
        }

        [Test]
        public void CornerLattice_ShoreVertexStitchesToWaterline()
        {
            const int w = 3, h = 3;
            var water = new bool[w, h];
            water[0, 0] = true; // single pond at the corner
            var waterY = Surfaces(w, h, 0f);
            var landY = Surfaces(w, h, 0.5f);
            var kinds = Kinds(w, h, RecipeWaterKind.Lake, water);
            var config = Config();
            var field = SeabedFieldPlanner.Build(
                w, h, 1f, water, waterY, landY, kinds, config);

            // Vertex between the pond and land: bed recesses just below the
            // waterline (surface - shore recess), not deeper.
            Assert.IsTrue(field.TryGetCornerY(1, 1, out float inner));
            Assert.AreEqual(-config.ShoreRecessMeters, inner, 0.001f);
        }

        [Test]
        public void RaisedWaterBody_BedTracksLocalWaterLevel()
        {
            const int w = 7, h = 7;
            var water = new bool[w, h];
            Fill(water, true);
            water[3, 3] = false; // island
            var waterY = Surfaces(w, h, 3f); // raised lake surface
            var landY = Surfaces(w, h, 3.2f);
            var kinds = Kinds(w, h, RecipeWaterKind.Lake, water);
            var field = SeabedFieldPlanner.Build(
                w, h, 1f, water, waterY, landY, kinds, Config());

            Assert.IsTrue(field.TryGetBedY(new Vector2Int(0, 0), out float bed));
            // Bed is relative to the raised surface, not world zero.
            Assert.Greater(bed, 0f);
            Assert.LessOrEqual(bed, 3f - 0.05f + 0.001f);
        }

        [Test]
        public void DepthAt_RespectsPerKindMaximums()
        {
            var config = Config();
            Assert.AreEqual(2f,
                SeabedFieldPlanner.DepthAt(RecipeWaterKind.Sink, 100f, config), 0.001f);
            Assert.AreEqual(1.2f,
                SeabedFieldPlanner.DepthAt(RecipeWaterKind.Lake, 100f, config), 0.001f);
            Assert.AreEqual(0.5f,
                SeabedFieldPlanner.DepthAt(RecipeWaterKind.River, 100f, config), 0.001f);
        }

        [Test]
        public void LandCells_ProduceNoBedData()
        {
            const int w = 4, h = 4;
            var water = new bool[w, h];
            var waterY = Surfaces(w, h, 0f);
            var landY = Surfaces(w, h, 1f);
            var kinds = Kinds(w, h, RecipeWaterKind.None, water);
            var field = SeabedFieldPlanner.Build(
                w, h, 1f, water, waterY, landY, kinds, Config());

            Assert.IsFalse(field.TryGetBedY(new Vector2Int(1, 1), out _));
        }
    }
}
