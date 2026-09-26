using Kruty1918.Moyva.Generator.Runtime.ChunkFirst;
using NUnit.Framework;
using UnityEngine;

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
